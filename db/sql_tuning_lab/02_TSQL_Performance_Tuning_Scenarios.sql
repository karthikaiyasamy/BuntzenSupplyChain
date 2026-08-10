-- ============================================================================
-- T-SQL Query Optimization & Performance Tuning Lab
-- Target Role: PHSA Senior Programmer (SC Performance)
-- ============================================================================

USE BuntzenSupplyChainDB;
GO

-- Set statistics I/O and Time to analyze execution cost
SET STATISTICS IO ON;
SET STATISTICS TIME ON;
GO

-- ----------------------------------------------------------------------------
-- SCENARIO 1: Table Scan vs Clustered / Non-Clustered Index Seek
-- ----------------------------------------------------------------------------
-- UNOPTIMIZED: Full Table Scan looking inside JSON string
SELECT * 
FROM SupplyChainAuditLogs 
WHERE AuditDetailsJson LIKE '%UPDATE_PAR%';

-- OPTIMIZED: Index Seek using IX_SupplyChainAuditLogs_Action_Entity
SELECT AuditId, EntityName, Action, Timestamp, PerformedBy
FROM SupplyChainAuditLogs WITH (INDEX(IX_SupplyChainAuditLogs_Action_Entity))
WHERE Action = 'UPDATE_PAR' AND EntityName = 'SiteInventory';
GO

-- ----------------------------------------------------------------------------
-- SCENARIO 2: SARGability Loss Fix
-- ----------------------------------------------------------------------------
-- UNOPTIMIZED: Non-SARGable scalar function on indexed column prevents Index Seek
SELECT * 
FROM SupplyItems 
WHERE UPPER(ItemNumber) = 'PHSA-PPE-204' 
   OR CAST(CreatedAt AS DATE) = '2026-08-10';

-- OPTIMIZED: SARGable predicate enables Clustered Index Seek
DECLARE @TargetItem NVARCHAR(50) = 'PHSA-PPE-204';
DECLARE @StartDate DATETIME2 = '2026-08-10T00:00:00';
DECLARE @EndDate DATETIME2 = '2026-08-11T00:00:00';

SELECT * 
FROM SupplyItems 
WHERE ItemNumber = @TargetItem 
   OR (CreatedAt >= @StartDate AND CreatedAt < @EndDate);
GO

-- ----------------------------------------------------------------------------
-- SCENARIO 3: Parameter Sniffing Mitigation
-- ----------------------------------------------------------------------------
-- OPTION A: OPTION (RECOMPILE) for highly skewed data distributions
EXEC sp_GetHospitalParDeficits @SiteCode = 'BCCH' OPTION (RECOMPILE);

-- OPTION B: Local Variable Assignment to disable parameter sniffing
DECLARE @LocalSiteCode NVARCHAR(10) = 'BCCH';
EXEC sp_GetHospitalParDeficits @SiteCode = @LocalSiteCode;
GO

SET STATISTICS IO OFF;
SET STATISTICS TIME OFF;
GO
