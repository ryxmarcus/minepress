using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using erp.minepress.infrastructure.ErrorLogging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using erp.minepress.persistence.Context;
using erp.minepress.web.Helpers;
using erp.minepress.web.Services;

namespace erp.minepress.web.Pages.Account;

public class LoginModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly IUserActivityService _activityService;
    private readonly erp.minepress.tenants.Interfaces.ITenantResolver _tenantResolver;
    private readonly erp.minepress.tenants.Interfaces.ITenantContextAccessor _tenantContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly ISystemErrorLogger _systemErrorLogger;

    public LoginModel(ApplicationDbContext db, IUserActivityService activityService,
        erp.minepress.tenants.Interfaces.ITenantResolver tenantResolver,
        erp.minepress.tenants.Interfaces.ITenantContextAccessor tenantContextAccessor,
        IConfiguration configuration,
        ISystemErrorLogger systemErrorLogger)
    {
        _db = db;
        _activityService = activityService;
        _tenantResolver = tenantResolver;
        _tenantContextAccessor = tenantContextAccessor;
        _configuration = configuration;
        _systemErrorLogger = systemErrorLogger;
    }

    [BindProperty]
    public LoginInput Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    [BindProperty]
    public string? TenantToken { get; set; }

    public void OnGet(string? returnUrl = null)
    {
        // If already authenticated, redirect to home
        if (HttpContext.Session.IsAuthenticated())
        {
            Response.Redirect(returnUrl ?? Url.Content("~/Dashboard"));
            return;
        }

        ReturnUrl = returnUrl ?? Url.Content("~/Dashboard");

        // If landing page passed a tenant token via TempData, use it to set tenant context for this request
        if (TempData.TryGetValue("TenantToken", out var obj) && obj is string token)
        {
            TenantToken = token;
            try
            {
                var bytes = System.Convert.FromBase64String(token);
                var json = System.Text.Encoding.UTF8.GetString(bytes);
                var doc = System.Text.Json.JsonDocument.Parse(json);
                var id = doc.RootElement.GetProperty("id").GetGuid();
                var key = doc.RootElement.GetProperty("key").GetString();
                if (!string.IsNullOrWhiteSpace(key))
                {
                    var tenant = _tenantResolver.ResolveTenantAsync(key).GetAwaiter().GetResult();
                    if (tenant != null)
                    {
                        _tenantContextAccessor.SetCurrent(new erp.minepress.tenants.Interfaces.TenantContext
                        {
                            TenantId = tenant.TenantId,
                            TenantKey = tenant.TenantKey,
                            TenantName = tenant.TenantName,
                            Source = "landing"
                        });

                        HttpContext.Items[erp.minepress.tenants.TenantConnectionConstants.TenantIdItemKey] = tenant.TenantId;
                        HttpContext.Items[erp.minepress.tenants.TenantConnectionConstants.TenantKeyItemKey] = tenant.TenantKey;
                        HttpContext.Items[erp.minepress.tenants.TenantConnectionConstants.TenantConnectionStringItemKey] = tenant.ConnectionString;
                    }
                }
            }
            catch
            {
                // ignore invalid token
            }
        }
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/Dashboard");

        if (!ModelState.IsValid)
        {
            return Page();
        }

        // If TenantToken was posted in hidden field, restore tenant context for this POST request
        if (!string.IsNullOrWhiteSpace(TenantToken))
        {
            try
            {
                var bytes = System.Convert.FromBase64String(TenantToken);
                var json = System.Text.Encoding.UTF8.GetString(bytes);
                var doc = System.Text.Json.JsonDocument.Parse(json);
                var id = doc.RootElement.GetProperty("id").GetGuid();
                var key = doc.RootElement.GetProperty("key").GetString();
                if (!string.IsNullOrWhiteSpace(key))
                {
                    var tenant = await _tenantResolver.ResolveTenantAsync(key);
                    if (tenant != null)
                    {
                        _tenantContextAccessor.SetCurrent(new erp.minepress.tenants.Interfaces.TenantContext
                        {
                            TenantId = tenant.TenantId,
                            TenantKey = tenant.TenantKey,
                            TenantName = tenant.TenantName,
                            Source = "landing"
                        });

                        HttpContext.Items[erp.minepress.tenants.TenantConnectionConstants.TenantIdItemKey] = tenant.TenantId;
                        HttpContext.Items[erp.minepress.tenants.TenantConnectionConstants.TenantKeyItemKey] = tenant.TenantKey;
                        HttpContext.Items[erp.minepress.tenants.TenantConnectionConstants.TenantConnectionStringItemKey] = tenant.ConnectionString;
                    }
                }
            }
            catch
            {
                // ignore
            }
        }

        // Find user by usercode
        var user = await _db.MstUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(u =>
                u.Usercode == Input.Username);

        if (user == null)
        {
            ErrorMessage = "Invalid username or password.";
            return Page();
        }

        // Check if account is active
        if (user.Isactive != true)
        {
            ErrorMessage = "Your account is inactive. Please contact the administrator.";
            return Page();
        }

        // Check if account is locked
        if (user.Islocked == true)
        {
            ErrorMessage = "Your account is locked. Please contact the administrator.";
            return Page();
        }

        // Verify password
        if (!VerifyPassword(Input.Password, user.Passwordhash))
        {
            // Update failed login count
            var trackUser = await _db.MstUsers.FindAsync(user.Userid);
            if (trackUser != null)
            {
                trackUser.Failedlogincount = (trackUser.Failedlogincount ?? 0) + 1;
                trackUser.Lastfailedloginat = DateTime.UtcNow;

                // Lock account after 5 failed attempts
                if (trackUser.Failedlogincount >= 5)
                {
                    trackUser.Islocked = true;
                }

                await _db.SaveChangesAsync();
            }

            ErrorMessage = "Invalid username or password.";
            return Page();
        }

        // Successful login — reset failed count and update last login
        var loginUser = await _db.MstUsers.FindAsync(user.Userid);
        if (loginUser != null)
        {
            loginUser.Failedlogincount = 0;
            loginUser.Lastloginat = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        // Store user data in session
        var dept = await _db.MstDepartments
            .AsNoTracking()
            .Where(d => d.DeptId == user.Departmentid)
            .Select(d => new { d.DeptCode, d.DeptName })
            .FirstOrDefaultAsync();

        var sessionData = new UserSessionData
        {
            UserId = user.Userid,
            UserCode = user.Usercode,
            UserName = user.Username,
            Name = user.Name,
            EmailId = user.Emailid,
            MobileNo = user.Mobileno,
            LocationId = user.Locationid,
            DepartmentId = user.Departmentid,
            DeptCode = dept?.DeptCode ?? "GEN",
            DeptName = dept?.DeptName ?? "General",
            EmployeeCode = user.Employeecode,
            DesignationId = user.Designationid,
            IsSystemAdmin = user.Issystemadmin ?? false,
            IsProductionUser = user.Isproductionuser ?? false,
            IsApprovalUser = user.Isapprovaluser ?? false,
            UserType = user.UserType,
            CompanyId = user.CompanyId,
            RefId = user.RefId,
            LoginAt = DateTime.UtcNow
        };

        // Load party roles if this is a party user
        if (user.UserType == "PARTY" && user.RefId.HasValue)
        {
            sessionData.PartyRoles = await _db.MstPartyRoles
                .Where(r => r.PartyId == (int)user.RefId.Value && r.IsActive)
                .Select(r => r.RoleType)
                .ToListAsync();
        }

        // Load user roles from map_user_role
        sessionData.Roles = await _db.MapUserRoles
            .Where(ur => ur.Userid == user.Userid && ur.Isactive == true)
            .Join(_db.MstRoles.Where(r => r.Isactive == true),
                ur => ur.Roleid, r => r.Roleid, (ur, r) => r.Rolecode)
            .Where(code => code != null)
            .Select(code => code!)
            .ToListAsync();

        // Load user permissions from map_user_permission + mst_permission
        sessionData.Permissions = await _db.MapUserPermissions
            .Where(up => up.Userid == user.Userid && up.Isallowed == true)
            .Join(_db.MstPermissions.Where(p => p.Isactive == true),
                up => up.Permissionid, p => p.Permissionid, (up, p) => p.Permissioncode)
            .Where(code => code != null)
            .Select(code => code!)
            .ToListAsync();

        HttpContext.Session.SetCurrentUser(sessionData);

        // ── Login Log ──
        var loginLogId = await _activityService.LogLoginAsync(user.Userid);
        if (loginLogId > 0)
            HttpContext.Session.SetObject("LoginLogId", loginLogId);

        // ── Activity Log: Login ──
        await _activityService.LogActivityAsync(new ActivityLogEntry
        {
            UserId = user.Userid,
            UserCode = user.Usercode,
            UserName = user.Name,
            Module = "AUTH",
            ActivityType = "LOGIN",
            ActivityCategory = "AUTH",
            Title = $"{user.Name} logged in",
            Description = $"User {user.Usercode} ({user.Name}) logged in successfully.",
            CompanyId = user.CompanyId,
            LocationId = user.Locationid,
            Severity = "INFO"
        });

        // ── In-App Notification: Login ──
        await _activityService.LogNotificationAsync(new UserNotificationEntry
        {
            UserId = user.Userid,
            Title = "Login Successful",
            Message = $"Welcome back, {user.Name}! You logged in at {DateTime.Now:dd-MMM-yyyy HH:mm}.",
            Icon = "bi bi-box-arrow-in-right",
            Color = "success",
            Module = "AUTH",
            EventType = "LOGIN"
        });

        // Redirect party users to Party Portal
        if (user.UserType == "PARTY")
        {
            return LocalRedirect(Url.Content("~/PartyPortal"));
        }

        return LocalRedirect(ReturnUrl);
    }

    /// <summary>
    /// Verifies the password against the stored hash.
    /// Supports plain text comparison and SHA-256 hash.
    /// </summary>
    private static bool VerifyPassword(string password, string storedHash)
    {
        if (string.IsNullOrEmpty(storedHash))
            return false;

        // Direct comparison (plain text or pre-matched)
        if (password == storedHash)
            return true;

        // SHA-256 hash comparison
        var sha256Hash = ComputeSha256Hash(password);
        if (string.Equals(sha256Hash, storedHash, StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }

    private static string ComputeSha256Hash(string rawData)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawData));
        var sb = new StringBuilder();
        foreach (var b in bytes)
            sb.Append(b.ToString("x2"));
        return sb.ToString();
    }

    public class LoginInput
    {
        [Required(ErrorMessage = "Username is required")]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }
}
