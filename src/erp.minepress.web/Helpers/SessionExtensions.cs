using System.Text.Json;

namespace erp.minepress.web.Helpers;

/// <summary>
/// User session data stored after successful login
/// </summary>
public class UserSessionData
{
    public long UserId { get; set; }
    public string UserCode { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? EmailId { get; set; }
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

public class TenantAdminSessionData
{
    public long UserId { get; set; }
    public string UserCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsSystemAdmin { get; set; }
    public DateTime LoginAt { get; set; }
}

/// <summary>
/// Extension methods for storing/retrieving objects in session
/// </summary>
public static class SessionExtensions
{
    private const string UserSessionKey = "CurrentUser";
    private const string TenantAdminSessionKey = "TenantAdminUser";

    public static void SetObject<T>(this ISession session, string key, T value)
    {
        session.SetString(key, JsonSerializer.Serialize(value));
    }

    public static T? GetObject<T>(this ISession session, string key)
    {
        var value = session.GetString(key);
        return value == null ? default : JsonSerializer.Deserialize<T>(value);
    }

    public static void SetCurrentUser(this ISession session, UserSessionData user)
    {
        session.SetObject(UserSessionKey, user);
    }

    public static UserSessionData? GetCurrentUser(this ISession session)
    {
        return session.GetObject<UserSessionData>(UserSessionKey);
    }

    public static bool IsAuthenticated(this ISession session)
    {
        return session.GetString(UserSessionKey) != null;
    }

    public static void ClearUser(this ISession session)
    {
        session.Remove(UserSessionKey);
    }

    public static void SetTenantAdmin(this ISession session, TenantAdminSessionData admin)
    {
        session.SetObject(TenantAdminSessionKey, admin);
    }

    public static TenantAdminSessionData? GetTenantAdmin(this ISession session)
    {
        return session.GetObject<TenantAdminSessionData>(TenantAdminSessionKey);
    }

    public static bool IsTenantAdminAuthenticated(this ISession session)
    {
        return session.GetString(TenantAdminSessionKey) != null;
    }

    public static void ClearTenantAdmin(this ISession session)
    {
        session.Remove(TenantAdminSessionKey);
    }
}
