namespace BuntzenSupplyChain.Domain.Entities;

public class SqlPerformanceMetric
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ScenarioKey { get; set; } = string.Empty; // e.g. "AuditLog_Search_Unindexed" vs "AuditLog_Search_Indexed"
    public string Description { get; set; } = string.Empty;
    public string SqlQueryExecuted { get; set; } = string.Empty;
    public bool UsesIndex { get; set; }
    public string IndexStrategy { get; set; } = string.Empty;
    
    public double ExecutionDurationMs { get; set; }
    public int RowsScanned { get; set; }
    public int RowsReturned { get; set; }
    public long MemoryKb { get; set; }
    
    public string ExecutionPlanSummary { get; set; } = string.Empty;
    public DateTime TestedAt { get; set; } = DateTime.UtcNow;
}

public class SupplyChainAuditLog
{
    public long AuditId { get; set; }
    public string PartitionDateKey { get; set; } = string.Empty; // e.g. "2026-08" for date partitioning practice
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // "UPDATE_PAR", "SUBMIT_REQUISITION", "PROCESS_EDI"
    public string PerformedBy { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string AuditDetailsJson { get; set; } = string.Empty;
}
