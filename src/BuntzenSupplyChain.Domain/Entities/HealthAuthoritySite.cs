namespace BuntzenSupplyChain.Domain.Entities;

public enum HealthAuthority
{
    PHSA,
    VCH,
    FHA,
    NH,
    IH,
    IslandHealth
}

public class HealthAuthoritySite
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string SiteCode { get; set; } = string.Empty; // e.g. BCCH, BCWH, VGH
    public string Name { get; set; } = string.Empty;
    public HealthAuthority Authority { get; set; } = HealthAuthority.PHSA;
    public string Department { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
