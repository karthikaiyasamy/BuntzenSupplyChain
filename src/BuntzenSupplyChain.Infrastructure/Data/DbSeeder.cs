using BuntzenSupplyChain.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BuntzenSupplyChain.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(BuntzenDbContext db)
    {
        await db.Database.EnsureCreatedAsync();

        if (await db.Sites.AnyAsync()) return; // Already seeded

        // 1. Seed Sites across BC Health Authorities
        var bcch = new HealthAuthoritySite
        {
            SiteCode = "BCCH",
            Name = "BC Children's Hospital",
            Authority = HealthAuthority.PHSA,
            Department = "Pediatric Emergency & OR",
            Address = "4480 Oak St, Vancouver, BC V6H 3N1"
        };
        var bcwh = new HealthAuthoritySite
        {
            SiteCode = "BCWH",
            Name = "BC Women's Hospital & Health Centre",
            Authority = HealthAuthority.PHSA,
            Department = "Maternal & Neonatal Intensive Care",
            Address = "4500 Oak St, Vancouver, BC V6H 3N1"
        };
        var vgh = new HealthAuthoritySite
        {
            SiteCode = "VGH",
            Name = "Vancouver General Hospital",
            Authority = HealthAuthority.VCH,
            Department = "Central Logistics & Supply Chain",
            Address = "855 W 12th Ave, Vancouver, BC V5Z 1M9"
        };
        var smmh = new HealthAuthoritySite
        {
            SiteCode = "SMMH",
            Name = "Surrey Memorial Hospital",
            Authority = HealthAuthority.FHA,
            Department = "Trauma & General Stores",
            Address = "13750 96 Ave, Surrey, BC V3V 1Z2"
        };

        db.Sites.AddRange(bcch, bcwh, vgh, smmh);
        await db.SaveChangesAsync();

        // 2. Seed Items
        var items = new List<SupplyItem>
        {
            new SupplyItem { ItemNumber = "PHSA-PPE-204", Name = "N95 Respirator Mask (Regular Fit)", Description = "Fluid-resistant surgical N95 respirator", Category = ItemCategory.PersonalProtectiveEquipment, UnitOfMeasure = "BOX", UnitPrice = 28.50m, DefaultReorderPoint = 150, DefaultSafetyStock = 50, VendorSku = "3M-1860", PreferredVendorName = "3M Health Care Canada" },
            new SupplyItem { ItemNumber = "PHSA-MED-9942", Name = "Surgical Scalpel Handle #3", Description = "Stainless steel precision scalpel handle", Category = ItemCategory.SurgicalSupplies, UnitOfMeasure = "EA", UnitPrice = 45.00m, DefaultReorderPoint = 30, DefaultSafetyStock = 10, VendorSku = "BD-3001", PreferredVendorName = "Becton Dickinson Canada" },
            new SupplyItem { ItemNumber = "PHSA-MED-1088", Name = "Sterile Nitrile Exam Gloves (Medium)", Description = "Powder-free latex-free examination gloves", Category = ItemCategory.PersonalProtectiveEquipment, UnitOfMeasure = "CASE", UnitPrice = 84.00m, DefaultReorderPoint = 80, DefaultSafetyStock = 25, VendorSku = "HAL-NTR-M", PreferredVendorName = "Halyard Health" },
            new SupplyItem { ItemNumber = "PHSA-FAR-3301", Name = "Sodium Chloride 0.9% IV Solution 1000ml", Description = "Sterile isotonic saline for intravenous infusion", Category = ItemCategory.Pharmaceuticals, UnitOfMeasure = "CASE", UnitPrice = 62.20m, DefaultReorderPoint = 100, DefaultSafetyStock = 40, VendorSku = "BAX-SAL-1000", PreferredVendorName = "Baxter Healthcare" },
            new SupplyItem { ItemNumber = "PHSA-DIA-5520", Name = "Rapid Antigen Diagnostic Kit (25 Tests)", Description = "Point-of-care rapid diagnostic testing kit", Category = ItemCategory.DiagnosticKits, UnitOfMeasure = "BOX", UnitPrice = 115.00m, DefaultReorderPoint = 40, DefaultSafetyStock = 15, VendorSku = "ABB-RAG-25", PreferredVendorName = "Abbott Diagnostics" }
        };

        db.Items.AddRange(items);
        await db.SaveChangesAsync();

        // 3. Seed Site Inventories with PAR Levels
        var inventories = new List<SiteInventory>();
        var random = new Random(42);

        foreach (var site in new[] { bcch, bcwh, vgh, smmh })
        {
            foreach (var item in items)
            {
                int par = item.DefaultReorderPoint * 2;
                int onHand = random.Next(item.DefaultSafetyStock / 2, par + 20); // Some will be in deficit!

                inventories.Add(new SiteInventory
                {
                    SiteId = site.Id,
                    ItemId = item.Id,
                    QuantityOnHand = onHand,
                    QuantityAllocated = random.Next(0, 10),
                    ParLevel = par,
                    ReorderThreshold = item.DefaultReorderPoint,
                    MaxStockLevel = par + 50,
                    LastRestockedAt = DateTime.UtcNow.AddDays(-random.Next(1, 10)),
                    LastAuditedAt = DateTime.UtcNow.AddDays(-random.Next(0, 3))
                });
            }
        }

        db.Inventories.AddRange(inventories);
        await db.SaveChangesAsync();

        // 4. Seed Requisition Orders
        var req = new RequisitionOrder
        {
            RequisitionNumber = "REQ-PHSA-2026-0810-01",
            SourceSiteId = bcch.Id,
            RequestedByStaffId = "STF-99201",
            Status = RequisitionStatus.Processing,
            Priority = RequisitionPriority.Urgent,
            CreatedAt = DateTime.UtcNow.AddHours(-4)
        };
        req.LineItems.Add(new RequisitionLineItem
        {
            ItemId = items[0].Id,
            QuantityRequested = 200,
            QuantityFulfilled = 150,
            UnitPrice = items[0].UnitPrice
        });
        req.LineItems.Add(new RequisitionLineItem
        {
            ItemId = items[3].Id,
            QuantityRequested = 50,
            QuantityFulfilled = 50,
            UnitPrice = items[3].UnitPrice
        });

        db.Requisitions.Add(req);
        await db.SaveChangesAsync();

        // 5. Seed 3,000 Audit Log Entries for T-SQL Performance Lab
        var auditLogs = new List<SupplyChainAuditLog>();
        var actions = new[] { "UPDATE_PAR", "SUBMIT_REQUISITION", "PROCESS_EDI_850", "INVENTORY_TRANSFER", "VENDOR_ASN_RECEIVED" };
        var entities = new[] { "SiteInventory", "RequisitionOrder", "EdiTransaction", "SupplyItem" };

        for (int i = 1; i <= 3000; i++)
        {
            var action = actions[i % actions.Length];
            var entity = entities[i % entities.Length];
            var date = DateTime.UtcNow.AddMinutes(-i * 5);

            auditLogs.Add(new SupplyChainAuditLog
            {
                AuditId = i,
                PartitionDateKey = date.ToString("yyyy-MM"),
                EntityName = entity,
                EntityId = Guid.NewGuid().ToString(),
                Action = action,
                PerformedBy = $"usr_sc_staff_{(i % 15) + 1}@phsa.ca",
                Timestamp = date,
                AuditDetailsJson = $"{{\"AuditId\":{i}, \"Action\":\"{action}\", \"SiteCode\":\"{(i % 2 == 0 ? "BCCH" : "VGH")}\", \"Volume\":{i * 3}}}"
            });
        }

        db.AuditLogs.AddRange(auditLogs);
        await db.SaveChangesAsync();
    }
}
