using System.Security.Cryptography;
using System.Text;
using erp.minepress.tenants.Interfaces;
using erp.minepress.tenants.Options;
using Microsoft.Extensions.Options;

namespace erp.minepress.tenants.Services;

public class TenantSecurityService : ITenantSecurityService
{
    private readonly byte[] _key;

    public TenantSecurityService(IOptions<TenantSecurityOptions> options)
    {
        var configuredKey = options.Value.MasterKey?.Trim() ?? string.Empty;
        _key = ResolveKey(configuredKey);
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrWhiteSpace(plainText))
            return string.Empty;

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        var payload = new byte[aes.IV.Length + cipherBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, payload, 0, aes.IV.Length);
        Buffer.BlockCopy(cipherBytes, 0, payload, aes.IV.Length, cipherBytes.Length);

        return Convert.ToBase64String(payload);
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrWhiteSpace(cipherText))
            return string.Empty;

        var payload = Convert.FromBase64String(cipherText);
        if (payload.Length < 17)
            return string.Empty;

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        var iv = new byte[16];
        var cipher = new byte[payload.Length - iv.Length];

        Buffer.BlockCopy(payload, 0, iv, 0, iv.Length);
        Buffer.BlockCopy(payload, iv.Length, cipher, 0, cipher.Length);

        using var decryptor = aes.CreateDecryptor(aes.Key, iv);
        var plainBytes = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
        return Encoding.UTF8.GetString(plainBytes);
    }

    public string ComputeHash(string plainText)
    {
        if (string.IsNullOrWhiteSpace(plainText))
            return string.Empty;

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(plainText));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public string GenerateApiKey()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return $"mp_{Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=')}";
    }

    private static byte[] ResolveKey(string configuredKey)
    {
        if (!string.IsNullOrWhiteSpace(configuredKey))
        {
            try
            {
                var decoded = Convert.FromBase64String(configuredKey);
                if (decoded.Length == 32)
                    return decoded;
            }
            catch
            {
            }

            var rawBytes = Encoding.UTF8.GetBytes(configuredKey);
            if (rawBytes.Length == 32)
                return rawBytes;
        }

        return SHA256.HashData(Encoding.UTF8.GetBytes("minepress-tenant-default-master-key-change-me"));
    }
}
