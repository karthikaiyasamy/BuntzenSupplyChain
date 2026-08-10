namespace BuntzenSupplyChain.Domain.Entities;

public enum RequisitionStatus
{
    Draft,
    Submitted,
    Approved,
    Processing,
    Fulfilled,
    Cancelled
}

public enum RequisitionPriority
{
    Routine,
    Urgent,
    STAT
}

public class RequisitionOrder
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string RequisitionNumber { get; set; } = string.Empty; // e.g. REQ-2026-0810-001
    public Guid SourceSiteId { get; set; }
    public HealthAuthoritySite SourceSite { get; set; } = null!;
    
    public string RequestedByStaffId { get; set; } = string.Empty;
    public RequisitionStatus Status { get; set; } = RequisitionStatus.Draft;
    public RequisitionPriority Priority { get; set; } = RequisitionPriority.Routine;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ApprovedAt { get; set; }
    public DateTime? FulfilledAt { get; set; }
    
    public List<RequisitionLineItem> LineItems { get; set; } = new();
    public decimal TotalEstimatedCost => LineItems.Sum(x => x.TotalCost);
}

public class RequisitionLineItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RequisitionOrderId { get; set; }
    
    public Guid ItemId { get; set; }
    public SupplyItem Item { get; set; } = null!;
    
    public int QuantityRequested { get; set; }
    public int QuantityFulfilled { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalCost => QuantityRequested * UnitPrice;
}
