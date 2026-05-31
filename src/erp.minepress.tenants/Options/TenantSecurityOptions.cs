namespace erp.minepress.tenants.Options;

public class TenantSecurityOptions
{
    public const string SectionName = "TenantSecurity";

    public string MasterKey { get; set; } = string.Empty;
    public int ApiKeyExpiryDays { get; set; } = 90;
    public int SessionExpiryMinutes { get; set; } = 30;
}
