namespace BuntzenSupplyChain.Domain.Entities;

public enum ItemCategory
{
    PersonalProtectiveEquipment,
    SurgicalSupplies,
    Pharmaceuticals,
    DiagnosticKits,
    GeneralMedical
}

public class SupplyItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ItemNumber { get; set; } = string.Empty; // e.g. PHSA-PPE-204
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ItemCategory Category { get; set; } = ItemCategory.GeneralMedical;
    public string UnitOfMeasure { get; set; } = "BOX"; // BOX, CASE, EA
    public decimal UnitPrice { get; set; }
    public int DefaultReorderPoint { get; set; }
    public int DefaultSafetyStock { get; set; }
    public string VendorSku { get; set; } = string.Empty;
    public string PreferredVendorName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
