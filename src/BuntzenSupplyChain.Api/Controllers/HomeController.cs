using BuntzenSupplyChain.Application.Interfaces;
using BuntzenSupplyChain.Domain.Entities;
using BuntzenSupplyChain.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BuntzenSupplyChain.Api.Controllers;

public class HomeController : Controller
{
    private readonly BuntzenDbContext _db;
    private readonly ISqlPerformanceTuningService _sqlService;
    private readonly IXmlXsltTransformationService _xmlService;

    public HomeController(BuntzenDbContext db, ISqlPerformanceTuningService sqlService, IXmlXsltTransformationService xmlService)
    {
        _db = db;
        _sqlService = sqlService;
        _xmlService = xmlService;
    }

    public async Task<IActionResult> Index()
    {
        var sites = await _db.Sites.AsNoTracking().ToListAsync();
        var stock = await _db.Inventories
            .Include(x => x.Site)
            .Include(x => x.Item)
            .AsNoTracking()
            .ToListAsync();
        
        var requisitions = await _db.Requisitions
            .Include(x => x.SourceSite)
            .Include(x => x.LineItems)
            .ThenInclude(li => li.Item)
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Take(5)
            .ToListAsync();

        ViewBag.TotalItems = await _db.Items.CountAsync();
        ViewBag.DeficitCount = stock.Count(x => x.IsParDeficit);
        ViewBag.TotalRequisitions = await _db.Requisitions.CountAsync();
        ViewBag.Sites = sites;

        return View(stock);
    }

    public async Task<IActionResult> PerformanceLab()
    {
        var scenarios = await _sqlService.RunAllPerformanceScenariosAsync();
        return View(scenarios);
    }

    public IActionResult XmlIntegration()
    {
        ViewBag.Templates = _xmlService.GetAvailableXsltTemplates();
        ViewBag.SampleXml850 = _xmlService.GetSampleVendorXmlPayload(EdiDocumentType.EDI_850_PurchaseOrder);
        ViewBag.SampleXml856 = _xmlService.GetSampleVendorXmlPayload(EdiDocumentType.EDI_856_AdvanceShipNotice);
        return View();
    }

    public IActionResult AzureDevOps()
    {
        return View();
    }
}
