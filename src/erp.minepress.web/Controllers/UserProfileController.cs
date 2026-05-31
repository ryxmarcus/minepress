using erp.minepress.infrastructure.ErrorLogging;
using erp.minepress.persistence.Context;
using erp.minepress.web.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace erp.minepress.web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserProfileController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<UserProfileController> _logger;
    private readonly ISystemErrorLogger _systemErrorLogger;

    public UserProfileController(ApplicationDbContext db, ILogger<UserProfileController> logger, ISystemErrorLogger systemErrorLogger)
    {
        _db = db;
        _logger = logger;
        _systemErrorLogger = systemErrorLogger;
    }

    private async Task AuditExceptionAsync(Exception ex, string additionalData, string severity = "Error")
        => await _systemErrorLogger.LogAsync(ex, HttpContext, severity, additionalData);

    private UserSessionData? CurrentUser =>
        HttpContext.Session.GetObject<UserSessionData>("CurrentUser");

    // ═══════════════════════════════════════════════════════════════
    // GET /api/userprofile/me — Current user's profile
    // ═══════════════════════════════════════════════════════════════
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var session = CurrentUser;
        if (session == null)
            return Unauthorized(new { message = "Not logged in" });

        var u = await _db.MstUsers
            .Include(x => x.Department)
            .Include(x => x.Designation)
            .Include(x => x.Location)
            .Include(x => x.Employee)
            .Include(x => x.Reportinguser)
            .Include(x => x.UserLoginLogs.OrderByDescending(l => l.Loginat).Take(5))
            .FirstOrDefaultAsync(x => x.Userid == session.UserId && x.Isdeleted != true);

        if (u == null)
            return NotFound(new { message = "User not found" });

        return Ok(new
        {
            userId = u.Userid,
            userCode = u.Usercode,
            username = u.Username,
            name = u.Name,
            email = u.Emailid,
            mobile = u.Mobileno,
            departmentId = u.Departmentid,
            department = u.Department?.DeptName,
            designationId = u.Designationid,
            designation = u.Designation?.DesignationName,
            locationId = u.Locationid,
            location = u.Location?.LocationName,
            employeeCode = u.Employeecode,
            userType = u.UserType,
            userCategory = u.UserCategory,
            isActive = u.Isactive ?? false,
            isLocked = u.Islocked ?? false,
            isSystemAdmin = u.Issystemadmin ?? false,
            isApprovalUser = u.Isapprovaluser ?? false,
            isProductionUser = u.Isproductionuser ?? false,
            isWebAccess = u.Iswebaccessallowed ?? false,
            isMobileAccess = u.Ismobileaccessallowed ?? false,
            joiningDate = u.Joiningdate,
            exitDate = u.Exitdate,
            lastLogin = u.Lastloginat,
            lastPasswordChange = u.Lastpasswordchange,
            failedLoginCount = u.Failedlogincount ?? 0,
            aiHealthScore = u.AiHealthScore,
            aiLastReviewed = u.AiLastReviewedAt,
            aiAlertCount = u.AiAlertCount ?? 0,
            aiAutoConfigured = u.AiAutoConfigured ?? false,
            reportingUser = u.Reportinguser != null
                ? new { id = u.Reportinguser.Userid, name = u.Reportinguser.Name }
                : null,
            recentLogins = u.UserLoginLogs.Select(l => new
            {
                loginAt = l.Loginat,
                logoutAt = l.Logoutat,
                ip = l.Ipaddress,
                channel = l.Channel
            })
        });
    }

    // ═══════════════════════════════════════════════════════════════
    // POST /api/userprofile/change-password
    // ═══════════════════════════════════════════════════════════════
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var session = CurrentUser;
        if (session == null)
            return Unauthorized(new { message = "Not logged in" });

        var user = await _db.MstUsers.FirstOrDefaultAsync(u => u.Userid == session.UserId && u.Isdeleted != true);
        if (user == null)
            return NotFound(new { message = "User not found" });

        var currentHash = HashPassword(dto.CurrentPassword);
        if (user.Passwordhash != currentHash)
            return BadRequest(new { message = "Current password is incorrect" });

        if (dto.NewPassword.Length < 8)
            return BadRequest(new { message = "New password must be at least 8 characters" });

        user.Passwordhash = HashPassword(dto.NewPassword);
        user.Lastpasswordchange = DateTime.UtcNow;
        user.MustChangePassword = false;
        user.Updatedby = session.UserCode;
        user.Updatedat = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(new { message = "Password changed successfully" });
    }

    // ═══════════════════════════════════════════════════════════════
    // GET /api/userprofile/my-roles — Roles assigned to current user
    // ═══════════════════════════════════════════════════════════════
    [HttpGet("my-roles")]
    public async Task<IActionResult> GetMyRoles()
    {
        var session = CurrentUser;
        if (session == null)
            return Unauthorized(new { message = "Not logged in" });

        var roles = await _db.MapUserRoles
            .Where(ur => ur.Userid == session.UserId && ur.Isactive == true)
            .Join(_db.MstRoles.Where(r => r.Isactive == true),
                ur => ur.Roleid, r => r.Roleid,
                (ur, r) => new
                {
                    roleId = r.Roleid,
                    roleCode = r.Rolecode,
                    roleName = r.Rolename,
                    description = r.Description
                })
            .OrderBy(r => r.roleName)
            .ToListAsync();

        return Ok(roles);
    }

    // ═══════════════════════════════════════════════════════════════
    // GET /api/userprofile/my-permissions — Permissions assigned to current user
    // ═══════════════════════════════════════════════════════════════
    [HttpGet("my-permissions")]
    public async Task<IActionResult> GetMyPermissions()
    {
        var session = CurrentUser;
        if (session == null)
            return Unauthorized(new { message = "Not logged in" });

        var permissions = await _db.MapUserPermissions
            .Where(up => up.Userid == session.UserId && up.Isallowed == true)
            .Join(_db.MstPermissions.Where(p => p.Isactive == true),
                up => up.Permissionid, p => p.Permissionid,
                (up, p) => new
                {
                    permissionId = p.Permissionid,
                    permissionCode = p.Permissioncode,
                    permissionName = p.Permissionname,
                    moduleName = p.Modulename
                })
            .OrderBy(p => p.moduleName).ThenBy(p => p.permissionName)
            .ToListAsync();

        return Ok(permissions);
    }

    // ═══════════════════════════════════════════════════════════════
    // GET /api/userprofile/my-menus — Menus accessible to current user
    // ═══════════════════════════════════════════════════════════════
    [HttpGet("my-menus")]
    public async Task<IActionResult> GetMyMenus()
    {
        var session = CurrentUser;
        if (session == null)
            return Unauthorized(new { message = "Not logged in" });

        var menus = await _db.MstMenus
            .Where(m => m.Isactive == true && m.Isweb == true)
            .OrderBy(m => m.Displayorder)
            .Select(m => new
            {
                menuId = m.Menuid,
                menuCode = m.Menucode,
                menuName = m.Menuname,
                parentMenuId = m.Parentmenuid,
                icon = m.Icon,
                routeUrl = m.Routeurl,
                menuLevel = m.Menulevel
            })
            .ToListAsync();

        return Ok(menus);
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    public record ChangePasswordDto(string CurrentPassword, string NewPassword);
}
