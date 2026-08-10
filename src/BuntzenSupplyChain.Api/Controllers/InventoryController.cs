using BuntzenSupplyChain.Domain.Entities;
using BuntzenSupplyChain.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BuntzenSupplyChain.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly BuntzenDbContext _db;

    public InventoryController(BuntzenDbContext db)
    {
        _db = db;
    }

    [HttpGet("sites")]
    public async Task<IActionResult> GetSites()
    {
        var sites = await _db.Sites.AsNoTracking().ToListAsync();
        return Ok(sites);
    }

    [HttpGet("items")]
    public async Task<IActionResult> GetItems()
    {
        var items = await _db.Items.AsNoTracking().ToListAsync();
        return Ok(items);
    }

    [HttpGet("stock")]
    public async Task<IActionResult> GetStock([FromQuery] string? siteCode, [FromQuery] bool? onlyDeficits)
    {
        var query = _db.Inventories
            .Include(x => x.Site)
            .Include(x => x.Item)
            .AsNoTracking();

        if (!string.IsNullOrEmpty(siteCode))
        {
            query = query.Where(x => x.Site.SiteCode == siteCode);
        }

        if (onlyDeficits == true)
        {
            query = query.Where(x => x.QuantityOnHand < x.ReorderThreshold);
        }

        var results = await query.ToListAsync();

        var dtoList = results.Select(x => new
        {
            x.Id,
            SiteName = x.Site.Name,
            SiteCode = x.Site.SiteCode,
            Authority = x.Site.Authority.ToString(),
            ItemNumber = x.Item.ItemNumber,
            ItemName = x.Item.Name,
            Category = x.Item.Category.ToString(),
            x.QuantityOnHand,
            x.QuantityAllocated,
            x.QuantityAvailable,
            x.ParLevel,
            x.ReorderThreshold,
            x.MaxStockLevel,
            IsDeficit = x.IsParDeficit,
            x.LastRestockedAt
        });

        return Ok(dtoList);
    }

    [HttpGet("requisitions")]
    public async Task<IActionResult> GetRequisitions()
    {
        var requisitions = await _db.Requisitions
            .Include(x => x.SourceSite)
            .Include(x => x.LineItems)
            .ThenInclude(li => li.Item)
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(requisitions);
    }

    [HttpPost("requisitions")]
    public async Task<IActionResult> CreateRequisition([FromBody] CreateRequisitionRequest request)
    {
        var site = await _db.Sites.FindAsync(request.SiteId);
        if (site == null) return BadRequest("Invalid SiteId");

        var req = new RequisitionOrder
        {
            RequisitionNumber = $"REQ-PHSA-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(100, 999)}",
            SourceSiteId = request.SiteId,
            RequestedByStaffId = request.RequestedByStaffId,
            Priority = request.Priority,
            Status = RequisitionStatus.Submitted,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var item in request.Items)
        {
            var supplyItem = await _db.Items.FindAsync(item.ItemId);
            if (supplyItem != null)
            {
                req.LineItems.Add(new RequisitionLineItem
                {
                    ItemId = supplyItem.Id,
                    QuantityRequested = item.Quantity,
                    UnitPrice = supplyItem.UnitPrice
                });
            }
        }

        _db.Requisitions.Add(req);
        
        // Log Audit Trail
        _db.AuditLogs.Add(new SupplyChainAuditLog
        {
            AuditId = await _db.AuditLogs.CountAsync() + 1,
            PartitionDateKey = DateTime.UtcNow.ToString("yyyy-MM"),
            EntityName = "RequisitionOrder",
            EntityId = req.Id.ToString(),
            Action = "SUBMIT_REQUISITION",
            PerformedBy = request.RequestedByStaffId,
            Timestamp = DateTime.UtcNow,
            AuditDetailsJson = $"{{\"RequisitionNumber\":\"{req.RequisitionNumber}\", \"SiteCode\":\"{site.SiteCode}\"}}"
        });

        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetRequisitions), new { id = req.Id }, req);
    }
}

public class CreateRequisitionRequest
{
    public Guid SiteId { get; set; }
    public string RequestedByStaffId { get; set; } = string.Empty;
    public RequisitionPriority Priority { get; set; } = RequisitionPriority.Routine;
    public List<RequisitionItemDto> Items { get; set; } = new();
}

public class RequisitionItemDto
{
    public Guid ItemId { get; set; }
    public int Quantity { get; set; }
}
