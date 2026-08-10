namespace BuntzenSupplyChain.Domain.Entities;

public enum EdiDocumentType
{
    EDI_850_PurchaseOrder,
    EDI_856_AdvanceShipNotice,
    XML_VendorCatalogUpdate,
    XML_ParReplenishmentFeed
}

public enum EdiProcessingStatus
{
    Received,
    Transforming,
    Validated,
    DispatchedToEsb,
    Failed
}

public class EdiTransaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TransactionReference { get; set; } = string.Empty; // e.g. TRN-850-99812
    public string VendorName { get; set; } = string.Empty;
    public EdiDocumentType DocumentType { get; set; } = EdiDocumentType.EDI_850_PurchaseOrder;
    
    public string RawXmlPayload { get; set; } = string.Empty;
    public string TransformedJsonPayload { get; set; } = string.Empty;
    public string XsltTemplateName { get; set; } = string.Empty;
    
    public EdiProcessingStatus Status { get; set; } = EdiProcessingStatus.Received;
    public string? ValidationErrorMessage { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public double ProcessingTimeMs { get; set; }
}
