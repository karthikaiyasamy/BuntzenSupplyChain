using BuntzenSupplyChain.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BuntzenSupplyChain.Infrastructure.Data;

public class BuntzenDbContext : DbContext
{
    public BuntzenDbContext(DbContextOptions<BuntzenDbContext> options) : base(options) { }

    public DbSet<HealthAuthoritySite> Sites => Set<HealthAuthoritySite>();
    public DbSet<SupplyItem> Items => Set<SupplyItem>();
    public DbSet<SiteInventory> Inventories => Set<SiteInventory>();
    public DbSet<RequisitionOrder> Requisitions => Set<RequisitionOrder>();
    public DbSet<RequisitionLineItem> RequisitionLineItems => Set<RequisitionLineItem>();
    public DbSet<EdiTransaction> EdiTransactions => Set<EdiTransaction>();
    public DbSet<SqlPerformanceMetric> SqlMetrics => Set<SqlPerformanceMetric>();
    public DbSet<SupplyChainAuditLog> AuditLogs => Set<SupplyChainAuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // SiteInventory relationship & keys
        modelBuilder.Entity<SiteInventory>()
            .HasOne(x => x.Site)
            .WithMany()
            .HasForeignKey(x => x.SiteId);

        modelBuilder.Entity<SiteInventory>()
            .HasOne(x => x.Item)
            .WithMany()
            .HasForeignKey(x => x.ItemId);

        // RequisitionOrder
        modelBuilder.Entity<RequisitionOrder>()
            .HasOne(x => x.SourceSite)
            .WithMany()
            .HasForeignKey(x => x.SourceSiteId);

        modelBuilder.Entity<RequisitionOrder>()
            .HasMany(x => x.LineItems)
            .WithOne()
            .HasForeignKey(x => x.RequisitionOrderId);

        modelBuilder.Entity<RequisitionLineItem>()
            .HasOne(x => x.Item)
            .WithMany()
            .HasForeignKey(x => x.ItemId);

        // Indexes for performance tuning practice
        modelBuilder.Entity<SupplyItem>()
            .HasIndex(x => x.ItemNumber)
            .IsUnique();

        modelBuilder.Entity<SupplyChainAuditLog>()
            .HasKey(x => x.AuditId);
    }
}
