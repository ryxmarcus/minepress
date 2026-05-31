namespace erp.minepress.frameworks.Authentication;

/// <summary>
/// Represents the authenticated user identity from JWT claims
/// </summary>
public class JwtUserIdentity
{
    public long UserId { get; set; }
    public string UserCode { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? MobileNo { get; set; }
    public int LocationId { get; set; }
    public long DepartmentId { get; set; }
    public string DeptCode { get; set; } = string.Empty;
    public string DeptName { get; set; } = string.Empty;
    public string? EmployeeCode { get; set; }
    public long DesignationId { get; set; }
    public bool IsSystemAdmin { get; set; }
    public bool IsProductionUser { get; set; }
    public bool IsApprovalUser { get; set; }
    public string? UserType { get; set; }
    public int? CompanyId { get; set; }
    public long? RefId { get; set; }
    public Guid? TenantId { get; set; }
    public string? TenantKey { get; set; }
    public List<string> PartyRoles { get; set; } = [];
    public List<string> Roles { get; set; } = [];
    public List<string> Permissions { get; set; } = [];
    public DateTime LoginAt { get; set; }

    public bool IsPartyUser => UserType == "PARTY";

    /// <summary>
    /// Checks if the user has a specific permission code.
    /// System admins always have all permissions.
    /// </summary>
    public bool HasPermission(string permissionCode)
    {
        return IsSystemAdmin || Permissions.Contains(permissionCode, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if the user has a specific role code.
    /// </summary>
    public bool HasRole(string roleCode)
    {
        return Roles.Contains(roleCode, StringComparer.OrdinalIgnoreCase);
    }
}

/// <summary>
/// Token pair returned after successful authentication
/// </summary>
public class TokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiration { get; set; }
    public DateTime RefreshTokenExpiration { get; set; }
    public string TokenType { get; set; } = "Bearer";
}

/// <summary>
/// Request model for token refresh
/// </summary>
public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}
