using System.Diagnostics;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using System.Text.Json;
using BuntzenSupplyChain.Application.Interfaces;
using BuntzenSupplyChain.Domain.Entities;

namespace BuntzenSupplyChain.Infrastructure.Services;

public class XmlXsltTransformationService : IXmlXsltTransformationService
{
    private readonly Dictionary<string, string> _xsltTemplates = new()
    {
        { "EDI_850_VendorToCanonical", @"<?xml version=""1.0"" encoding=""UTF-8""?>
<xsl:stylesheet version=""1.0"" xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"">
  <xsl:output method=""xml"" indent=""yes""/>
  <xsl:template match=""/VendorPurchaseOrder"">
    <CanonicalPO>
      <PoNumber><xsl:value-of select=""Header/OrderNumber""/></PoNumber>
      <Vendor><xsl:value-of select=""Header/VendorName""/></Vendor>
      <DeliverySite><xsl:value-of select=""Header/DestinationCode""/></DeliverySite>
      <OrderDate><xsl:value-of select=""Header/Date""/></OrderDate>
      <Items>
        <xsl:for-each select=""LineItems/Item"">
          <Line>
            <Sku><xsl:value-of select=""SKU""/></Sku>
            <Qty><xsl:value-of select=""Quantity""/></Qty>
            <Price><xsl:value-of select=""UnitPrice""/></Price>
          </Line>
        </xsl:for-each>
      </Items>
    </CanonicalPO>
  </xsl:template>
</xsl:stylesheet>" },
        { "EDI_856_ASNToCanonical", @"<?xml version=""1.0"" encoding=""UTF-8""?>
<xsl:stylesheet version=""1.0"" xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"">
  <xsl:output method=""xml"" indent=""yes""/>
  <xsl:template match=""/AdvanceShipNotice"">
    <CanonicalASN>
      <AsnId><xsl:value-of select=""ShipmentHeader/AsnNumber""/></AsnId>
      <Carrier><xsl:value-of select=""ShipmentHeader/CarrierName""/></Carrier>
      <TrackingNumber><xsl:value-of select=""ShipmentHeader/Tracking""/></TrackingNumber>
      <ShippedItems>
        <xsl:for-each select=""CartonDetails/Package"">
          <PackageItem>
            <ItemSku><xsl:value-of select=""ProductCode""/></ItemSku>
            <QuantityShipped><xsl:value-of select=""Qty""/></QuantityShipped>
          </PackageItem>
        </xsl:for-each>
      </ShippedItems>
    </CanonicalASN>
  </xsl:template>
</xsl:stylesheet>" }
    };

    public List<string> GetAvailableXsltTemplates() => _xsltTemplates.Keys.ToList();

    public string GetSampleVendorXmlPayload(EdiDocumentType docType)
    {
        return docType switch
        {
            EdiDocumentType.EDI_850_PurchaseOrder => @"<?xml version=""1.0"" encoding=""UTF-8""?>
<VendorPurchaseOrder>
  <Header>
    <OrderNumber>PO-PHSA-2026-8831</OrderNumber>
    <VendorName>Medtronic Surgical BC</VendorName>
    <DestinationCode>BCCH-OR-2</DestinationCode>
    <Date>2026-08-10</Date>
  </Header>
  <LineItems>
    <Item>
      <SKU>PHSA-MED-9942</SKU>
      <Quantity>150</Quantity>
      <UnitPrice>45.50</UnitPrice>
    </Item>
    <Item>
      <SKU>PHSA-PPE-204</SKU>
      <Quantity>500</Quantity>
      <UnitPrice>12.75</UnitPrice>
    </Item>
  </LineItems>
</VendorPurchaseOrder>",
            _ => @"<?xml version=""1.0"" encoding=""UTF-8""?>
<AdvanceShipNotice>
  <ShipmentHeader>
    <AsnNumber>ASN-90021-BC</AsnNumber>
    <CarrierName>FedEx Healthcare Logistics</CarrierName>
    <Tracking>784920194812</Tracking>
  </ShipmentHeader>
  <CartonDetails>
    <Package>
      <ProductCode>PHSA-PPE-204</ProductCode>
      <Qty>500</Qty>
    </Package>
  </CartonDetails>
</AdvanceShipNotice>"
        };
    }

    public async Task<EdiTransaction> TransformVendorXmlAsync(string rawXml, string xsltTemplateName, EdiDocumentType docType, string vendorName)
    {
        var timer = Stopwatch.StartNew();
        var transaction = new EdiTransaction
        {
            TransactionReference = $"TRN-{docType.ToString().Substring(0, 7)}-{Random.Shared.Next(10000, 99999)}",
            VendorName = vendorName,
            DocumentType = docType,
            RawXmlPayload = rawXml,
            XsltTemplateName = xsltTemplateName,
            Status = EdiProcessingStatus.Transforming,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            if (!_xsltTemplates.TryGetValue(xsltTemplateName, out var xsltString))
            {
                xsltString = _xsltTemplates["EDI_850_VendorToCanonical"];
            }

            // Perform XSLT Transformation
            using var xmlReader = XmlReader.Create(new StringReader(rawXml));
            using var xsltReader = XmlReader.Create(new StringReader(xsltString));
            
            var transform = new XslCompiledTransform();
            transform.Load(xsltReader);

            using var sw = new StringWriter();
            using var xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings { Indent = true });
            
            transform.Transform(xmlReader, xmlWriter);
            string transformedXml = sw.ToString();

            // Convert transformed XML to Canonical JSON for API consumption
            var doc = new XmlDocument();
            doc.LoadXml(transformedXml);
            string jsonResult = JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });

            timer.Stop();
            transaction.TransformedJsonPayload = jsonResult;
            transaction.Status = EdiProcessingStatus.DispatchedToEsb;
            transaction.ProcessedAt = DateTime.UtcNow;
            transaction.ProcessingTimeMs = timer.Elapsed.TotalMilliseconds;
        }
        catch (Exception ex)
        {
            timer.Stop();
            transaction.Status = EdiProcessingStatus.Failed;
            transaction.ValidationErrorMessage = ex.Message;
            transaction.ProcessingTimeMs = timer.Elapsed.TotalMilliseconds;
        }

        return await Task.FromResult(transaction);
    }
}
