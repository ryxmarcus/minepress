using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using erp.minepress.tenants.Interfaces;

namespace erp.minepress.tenants.Services;

// Simple HMAC-based short-lived token for tenant selection
public class TenantTokenService : ITenantTokenService
{
    private readonly byte[] _key;
    private readonly TimeSpan _ttl = TimeSpan.FromMinutes(15);

    public TenantTokenService(IConfiguration configuration)
    {
        var secret = configuration["TenantSelection:Secret"] ?? "default-secret-change-me";
        _key = Encoding.UTF8.GetBytes(secret);
    }

    public string CreateToken(Guid tenantId, string tenantKey)
    {
        var payload = $"{tenantId}|{tenantKey}|{DateTimeOffset.UtcNow.Add(_ttl).ToUnixTimeSeconds()}";
        var bytes = Encoding.UTF8.GetBytes(payload);
        using var hmac = new HMACSHA256(_key);
        var sig = hmac.ComputeHash(bytes);
        var token = System.Convert.ToBase64String(Encoding.UTF8.GetBytes(payload)) + "." + System.Convert.ToBase64String(sig);
        return token;
    }

    public bool ValidateToken(string token, out Guid tenantId, out string tenantKey)
    {
        tenantId = Guid.Empty;
        tenantKey = string.Empty;

        if (string.IsNullOrWhiteSpace(token))
            return false;

        var parts = token.Split('.', 2);
        if (parts.Length != 2)
            return false;

        try
        {
            var payload = Encoding.UTF8.GetString(System.Convert.FromBase64String(parts[0]));
            var sig = System.Convert.FromBase64String(parts[1]);

            using var hmac = new HMACSHA256(_key);
            var expected = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            if (!CryptographicOperations.FixedTimeEquals(expected, sig))
                return false;

            var seg = payload.Split('|');
            if (seg.Length != 3)
                return false;

            tenantId = Guid.Parse(seg[0]);
            tenantKey = seg[1];
            var exp = DateTimeOffset.FromUnixTimeSeconds(long.Parse(seg[2]));
            if (DateTimeOffset.UtcNow > exp)
                return false;

            return true;
        }
        catch
        {
            return false;
        }
    }
}
