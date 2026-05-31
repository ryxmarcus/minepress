namespace erp.minepress.tenants.Interfaces;

public interface ITenantSecurityService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
    string ComputeHash(string plainText);
    string GenerateApiKey();
}
