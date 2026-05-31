namespace erp.minepress.tenants.Interfaces;

public interface ITenantTokenService
{
    string CreateToken(Guid tenantId, string tenantKey);
    bool ValidateToken(string token, out Guid tenantId, out string tenantKey);
}
