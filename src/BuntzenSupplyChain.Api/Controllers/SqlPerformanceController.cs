using BuntzenSupplyChain.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BuntzenSupplyChain.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SqlPerformanceController : ControllerBase
{
    private readonly ISqlPerformanceTuningService _sqlService;

    public SqlPerformanceController(ISqlPerformanceTuningService sqlService)
    {
        _sqlService = sqlService;
    }

    [HttpGet("scenarios")]
    public async Task<IActionResult> GetAllScenarios()
    {
        var results = await _sqlService.RunAllPerformanceScenariosAsync();
        return Ok(results);
    }

    [HttpGet("scenario/{scenarioName}")]
    public async Task<IActionResult> GetScenario(string scenarioName)
    {
        var result = await _sqlService.RunScenarioByNameAsync(scenarioName);
        return Ok(result);
    }

    [HttpGet("audit-trail")]
    public async Task<IActionResult> GetAuditTrail([FromQuery] string? entityName, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var logs = await _sqlService.GetAuditTrailPagedAsync("PHSA", entityName ?? "", page, pageSize);
        return Ok(logs);
    }
}
