-- ============================================================================
-- PHSA Supply Chain Performance (SC Performance) - SQL Server Database DDL
-- Database: BuntzenSupplyChainDB (Microsoft SQL Server 2022 / Azure SQL Edge)
-- Author: PHSA Senior Programmer Team
-- ============================================================================

CREATE DATABASE BuntzenSupplyChainDB;
GO

USE BuntzenSupplyChainDB;
GO

-- 1. Health Authority Sites Table
CREATE TABLE HealthAuthoritySites (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    SiteCode NVARCHAR(10) NOT NULL UNIQUE,
    Name NVARCHAR(150) NOT NULL,
    Authority NVARCHAR(50) NOT NULL, -- PHSA, VCH, FHA
    Department NVARCHAR(100) NOT NULL,
    Address NVARCHAR(250) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1
);

-- 2. Supply Items Table (Medical Supplies, PPE, Surgical)
CREATE TABLE SupplyItems (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ItemNumber NVARCHAR(50) NOT NULL UNIQUE, -- e.g. PHSA-PPE-204
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(500) NULL,
    Category NVARCHAR(50) NOT NULL,
    UnitOfMeasure NVARCHAR(20) NOT NULL DEFAULT 'BOX',
    UnitPrice DECIMAL(18, 2) NOT NULL,
    DefaultReorderPoint INT NOT NULL,
    DefaultSafetyStock INT NOT NULL,
    VendorSku NVARCHAR(100) NULL,
    PreferredVendorName NVARCHAR(150) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- 3. Site Inventory Stock & PAR Levels Table
CREATE TABLE SiteInventories (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    SiteId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES HealthAuthoritySites(Id),
    ItemId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES SupplyItems(Id),
    QuantityOnHand INT NOT NULL,
    QuantityAllocated INT NOT NULL DEFAULT 0,
    ParLevel INT NOT NULL,
    ReorderThreshold INT NOT NULL,
    MaxStockLevel INT NOT NULL,
    LastRestockedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    LastAuditedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- Index for fast PAR level deficit queries across hospital sites
CREATE NONCLUSTERED INDEX IX_SiteInventories_ParDeficit
ON SiteInventories(SiteId, ReorderThreshold)
INCLUDE (QuantityOnHand, ParLevel, ItemId);

-- 4. High-Volume Supply Chain Audit Trail (T-SQL Performance Practice)
CREATE TABLE SupplyChainAuditLogs (
    AuditId BIGINT IDENTITY(1,1) PRIMARY KEY CLUSTERED,
    PartitionDateKey NVARCHAR(7) NOT NULL, -- e.g. '2026-08'
    EntityName NVARCHAR(100) NOT NULL,
    EntityId NVARCHAR(100) NOT NULL,
    Action NVARCHAR(50) NOT NULL, -- UPDATE_PAR, SUBMIT_REQUISITION
    PerformedBy NVARCHAR(150) NOT NULL,
    Timestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    AuditDetailsJson NVARCHAR(MAX) NULL
);

-- Covering Index for High-Throughput Audit Searches
CREATE NONCLUSTERED INDEX IX_SupplyChainAuditLogs_Action_Entity
ON SupplyChainAuditLogs(Action, EntityName)
INCLUDE (Timestamp, PerformedBy);

GO

-- ============================================================================
-- Stored Procedure: Optimized PAR Deficit Report using CTE & Window Function
-- ============================================================================
CREATE PROCEDURE sp_GetHospitalParDeficits
    @SiteCode NVARCHAR(10) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    WITH RankedDeficits AS (
        SELECT 
            s.SiteCode,
            s.Name AS SiteName,
            i.ItemNumber,
            i.Name AS ItemName,
            inv.QuantityOnHand,
            inv.ParLevel,
            inv.ReorderThreshold,
            (inv.ParLevel - inv.QuantityOnHand) AS DeficitAmount,
            ROW_NUMBER() OVER (PARTITION BY inv.SiteId ORDER BY (inv.ParLevel - inv.QuantityOnHand) DESC) AS DeficitRank
        FROM SiteInventories inv
        INNER JOIN HealthAuthoritySites s ON inv.SiteId = s.Id
        INNER JOIN SupplyItems i ON inv.ItemId = i.Id
        WHERE inv.QuantityOnHand < inv.ReorderThreshold
          AND (@SiteCode IS NULL OR s.SiteCode = @SiteCode)
    )
    SELECT * 
    FROM RankedDeficits
    ORDER BY DeficitAmount DESC;
END;
GO
