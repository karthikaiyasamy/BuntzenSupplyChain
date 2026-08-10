using BuntzenSupplyChain.Domain.Entities;

namespace BuntzenSupplyChain.Application.Interfaces;

public class SqlTuningScenarioResult
{
    public string ScenarioName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string UnoptimizedQuery { get; set; } = string.Empty;
    public double UnoptimizedTimeMs { get; set; }
    public int UnoptimizedRowsScanned { get; set; }
    public string UnoptimizedExecutionPlanSummary { get; set; } = string.Empty;

    public string OptimizedQuery { get; set; } = string.Empty;
    public double OptimizedTimeMs { get; set; }
    public int OptimizedRowsScanned { get; set; }
    public string IndexRecommendation { get; set; } = string.Empty;
    public string OptimizedExecutionPlanSummary { get; set; } = string.Empty;

    public double SpeedupFactor => UnoptimizedTimeMs > 0 ? Math.Round(UnoptimizedTimeMs / Math.Max(0.01, OptimizedTimeMs), 2) : 1.0;
    public string KeyTakeaway { get; set; } = string.Empty;
}

public interface ISqlPerformanceTuningService
{
    Task<List<SqlTuningScenarioResult>> RunAllPerformanceScenariosAsync();
    Task<SqlTuningScenarioResult> RunScenarioByNameAsync(string scenarioName);
    Task<List<SupplyChainAuditLog>> GetAuditTrailPagedAsync(string healthAuthority, string entityName, int page, int pageSize);
}
