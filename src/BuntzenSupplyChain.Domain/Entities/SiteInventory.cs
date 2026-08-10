namespace BuntzenSupplyChain.Domain.Entities;

public class SiteInventory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SiteId { get; set; }
    public HealthAuthoritySite Site { get; set; } = null!;
    
    public Guid ItemId { get; set; }
    public SupplyItem Item { get; set; } = null!;
    
    public int QuantityOnHand { get; set; }
    public int QuantityAllocated { get; set; }
    public int QuantityAvailable => QuantityOnHand - QuantityAllocated;
    
    public int ParLevel { get; set; }
    public int ReorderThreshold { get; set; }
    public int MaxStockLevel { get; set; }
    
    public DateTime LastRestockedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastAuditedAt { get; set; } = DateTime.UtcNow;

    public bool IsParDeficit => QuantityOnHand < ReorderThreshold;
}
