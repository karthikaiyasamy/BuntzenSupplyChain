using BuntzenSupplyChain.Application.Interfaces;
using BuntzenSupplyChain.Domain.Entities;
using BuntzenSupplyChain.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace BuntzenSupplyChain.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EdiIntegrationController : ControllerBase
{
    private readonly IXmlXsltTransformationService _xmlService;
    private readonly BuntzenDbContext _db;

    public EdiIntegrationController(IXmlXsltTransformationService xmlService, BuntzenDbContext db)
    {
        _xmlService = xmlService;
        _db = db;
    }

    [HttpGet("templates")]
    public IActionResult GetTemplates()
    {
        return Ok(_xmlService.GetAvailableXsltTemplates());
    }

    [HttpGet("sample/{docType}")]
    public IActionResult GetSamplePayload(EdiDocumentType docType)
    {
        return Ok(new
        {
            DocumentType = docType.ToString(),
            RawXml = _xmlService.GetSampleVendorXmlPayload(docType)
        });
    }

    [HttpPost("transform")]
    public async Task<IActionResult> TransformVendorPayload([FromBody] TransformRequest request)
    {
        var result = await _xmlService.TransformVendorXmlAsync(
            request.RawXml,
            request.XsltTemplateName,
            request.DocumentType,
            request.VendorName
        );

        _db.EdiTransactions.Add(result);
        await _db.SaveChangesAsync();

        return Ok(result);
    }
}

public class TransformRequest
{
    public string RawXml { get; set; } = string.Empty;
    public string XsltTemplateName { get; set; } = string.Empty;
    public EdiDocumentType DocumentType { get; set; } = EdiDocumentType.EDI_850_PurchaseOrder;
    public string VendorName { get; set; } = "Medtronic Surgical BC";
}
