using BuntzenSupplyChain.Domain.Entities;

namespace BuntzenSupplyChain.Application.Interfaces;

public interface IXmlXsltTransformationService
{
    Task<EdiTransaction> TransformVendorXmlAsync(string rawXml, string xsltTemplateName, EdiDocumentType docType, string vendorName);
    List<string> GetAvailableXsltTemplates();
    string GetSampleVendorXmlPayload(EdiDocumentType docType);
}
