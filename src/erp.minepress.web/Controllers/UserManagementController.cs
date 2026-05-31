using erp.minepress.infrastructure.ErrorLogging;
using erp.minepress.persistence.Context;
using erp.minepress.persistence.Models;
using erp.minepress.web.Helpers;
using erp.minepress.web.Services;
using erp.minepress.notification.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace erp.minepress.web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserManagementController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<UserManagementController> _logger;
    private readonly INotificationService _notification;
    private readonly IUserActivityService _activity;
    private readonly ISystemErrorLogger _systemErrorLogger;

    public UserManagementController(
        ApplicationDbContext db,
        ILogger<UserManagementController> logger,
        INotificationService notification,
        IUserActivityService activity,
        ISystemErrorLogger systemErrorLogger)
    {
        _db = db;
        _logger = logger;
        _notification = notification;
        _activity = activity;
        _systemErrorLogger = systemErrorLogger;
    }

    private async Task AuditExceptionAsync(Exception ex, string additionalData, string severity = "Error")
        => await _systemErrorLogger.LogAsync(ex, HttpContext, severity, additionalData);

    private UserSessionData? CurrentUser =>
        HttpContext.Session.GetObject<UserSessionData>("CurrentUser");

    // ═══════════════════════════════════════════════════════════════
    // KPIs
    // ═══════════════════════════════════════════════════════════════
    [HttpGet("kpis")]
    public async Task<IActionResult> GetKpis()
    {
        var totalUsers = await _db.MstUsers.CountAsync(u => u.Isdeleted != true);
        var activeUsers = await _db.MstUsers.CountAsync(u => u.Isactive == true && u.Isdeleted != true);
        var lockedUsers = await _db.MstUsers.CountAsync(u => u.Islocked == true && u.Isdeleted != true);
        var totalRoles = await _db.MstRoles.CountAsync(r => r.Isactive == true);
        var totalPerms = await _db.MstPermissions.CountAsync(p => p.Isactive == true);

        // AI insights
        var noLoginUsers = await _db.MstUsers.CountAsync(u => u.Isactive == true && u.Isdeleted != true && u.Lastloginat == null);
        var staleUsers = await _db.MstUsers.CountAsync(u => u.Isactive == true && u.Isdeleted != true && u.Lastloginat != null && u.Lastloginat < DateTime.UtcNow.AddDays(-90));

        return Ok(new
        {
            totalUsers,
            activeUsers,
            lockedUsers,
            totalRoles,
            totalPerms,
            aiInsights = new
            {
                noLoginUsers,
                staleUsers,
                adminCount = await _db.MstUsers.CountAsync(u => u.Issystemadmin == true && u.Isactive == true && u.Isdeleted != true)
            }
        });
    }

    // ═══════════════════════════════════════════════════════════════
    // USERS — List
    // ═══════════════════════════════════════════════════════════════
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string? q,
        [FromQuery] string? status,
        [FromQuery] string? userType,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20)
    {
        var query = _db.MstUsers
            .Include(u => u.Department)
            .Include(u => u.Designation)
            .Include(u => u.Location)
            .Where(u => u.Isdeleted != true);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.ToLower();
            query = query.Where(u =>
                u.Name.ToLower().Contains(term) ||
                u.Usercode.ToLower().Contains(term) ||
                u.Username.ToLower().Contains(term) ||
                (u.Emailid != null && u.Emailid.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = status.ToLower() switch
            {
                "active" => query.Where(u => u.Isactive == true && u.Islocked != true),
                "inactive" => query.Where(u => u.Isactive != true),
                "locked" => query.Where(u => u.Islocked == true),
                _ => query
            };
        }

        if (!string.IsNullOrWhiteSpace(userType))
            query = query.Where(u => u.UserType == userType);

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(u => u.Name)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(u => new
            {
                userId = u.Userid,
                userCode = u.Usercode,
                username = u.Username,
                name = u.Name,
                email = u.Emailid,
                mobile = u.Mobileno,
                department = u.Department.DeptName,
                designation = u.Designation.DesignationName,
                location = u.Location.LocationName,
                userType = u.UserType,
                isActive = u.Isactive ?? false,
                isLocked = u.Islocked ?? false,
                isSystemAdmin = u.Issystemadmin ?? false,
                aiHealthScore = u.AiHealthScore,
                lastLogin = u.Lastloginat
            })
            .ToListAsync();

        return Ok(new { items, total, page, size, totalPages = (int)Math.Ceiling(total / (double)size) });
    }

    // ═══════════════════════════════════════════════════════════════
    // USERS — Get single
    // ═══════════════════════════════════════════════════════════════
    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUser(long id)
    {
        var u = await _db.MstUsers
            .Include(x => x.Department)
            .Include(x => x.Designation)
            .Include(x => x.Location)
            .Include(x => x.Employee)
            .Include(x => x.Reportinguser)
            .Include(x => x.UserLoginLogs.OrderByDescending(l => l.Loginat).Take(5))
            .FirstOrDefaultAsync(x => x.Userid == id && x.Isdeleted != true);

        if (u == null) return NotFound(new { message = "User not found" });

        return Ok(new
        {
            userId = u.Userid,
            userCode = u.Usercode,
            username = u.Username,
            name = u.Name,
            email = u.Emailid,
            mobile = u.Mobileno,
            departmentId = u.Departmentid,
            department = u.Department.DeptName,
            designationId = u.Designationid,
            designation = u.Designation.DesignationName,
            locationId = u.Locationid,
            location = u.Location.LocationName,
            employeeCode = u.Employeecode,
            userType = u.UserType,
            userCategory = u.UserCategory,
            isActive = u.Isactive ?? false,
            isLocked = u.Islocked ?? false,
            isDeleted = u.Isdeleted ?? false,
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
            reportingUser = u.Reportinguser != null ? new { id = u.Reportinguser.Userid, name = u.Reportinguser.Name } : null,
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
    // USERS — Create
    // ═══════════════════════════════════════════════════════════════
    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] UserDto dto)
    {
        try
        {
            // ── Field-level validation ──
            var errors = ValidateUser(dto);
            if (errors.Count > 0)
                return BadRequest(new { message = errors[0], errors });

            // ── Duplicate checks ──
            if (await _db.MstUsers.AnyAsync(u => u.Usercode == dto.UserCode && u.Isdeleted != true))
                return BadRequest(new { message = "User code already exists" });
            if (await _db.MstUsers.AnyAsync(u => u.Username == dto.Username && u.Isdeleted != true))
                return BadRequest(new { message = "Username already exists" });
            if (!string.IsNullOrWhiteSpace(dto.Email) && await _db.MstUsers.AnyAsync(u => u.Emailid == dto.Email && u.Isdeleted != true))
                return BadRequest(new { message = "Email address already in use" });

            // ── FK existence checks ──
            if (!await _db.MstDepartments.AnyAsync(d => d.DeptId == dto.DepartmentId && d.IsActive == true))
                return BadRequest(new { message = "Selected department does not exist or is inactive" });
            if (!await _db.MstDesignations.AnyAsync(d => d.DesignationId == dto.DesignationId && d.IsActive == true))
                return BadRequest(new { message = "Selected designation does not exist or is inactive" });
            if (!await _db.MstLocations.AnyAsync(l => l.LocationId == dto.LocationId && l.IsActive == true))
                return BadRequest(new { message = "Selected location does not exist or is inactive" });

            var user = new MstUser
            {
                Usercode = dto.UserCode,
                Username = dto.Username,
                Passwordhash = HashPassword(dto.Password ?? "Welcome@123"),
                Name = dto.Name,
                Emailid = dto.Email,
                Mobileno = dto.Mobile,
                UserType = dto.UserType ?? "EMPLOYEE",
                Departmentid = dto.DepartmentId,
                Designationid = dto.DesignationId,
                Locationid = dto.LocationId,
                Employeecode = dto.EmployeeCode,
                Issystemadmin = dto.IsSystemAdmin,
                Isapprovaluser = dto.IsApprovalUser,
                Isproductionuser = dto.IsProductionUser,
                Iswebaccessallowed = dto.IsWebAccess ?? true,
                Ismobileaccessallowed = dto.IsMobileAccess,
                Isactive = true,
                Islocked = false,
                Isdeleted = false,
                Createdby = CurrentUser?.UserCode ?? "SYSTEM",
                Createdat = DateTime.UtcNow
            };

            _db.MstUsers.Add(user);
            await _db.SaveChangesAsync();

            // ── Activity Log ──
            try
            {
                var session = CurrentUser;
                if (session != null)
                {
                    var logEntry = ActivityLogEntry.FromUser(session, "USER_MGMT", "CREATE", $"Created user {user.Usercode} — {user.Name}");
                    logEntry.EntityType = "USER";
                    logEntry.EntityId = user.Userid;
                    logEntry.EntityCode = user.Usercode;
                    logEntry.Description = $"New user created: {user.Name} ({user.Username}), Type: {user.UserType}, Dept: {user.Departmentid}";
                    await _activity.LogActivityAsync(logEntry);
                }
            }
            catch (Exception actEx)
            {
                _logger.LogWarning(actEx, "Failed to log user creation activity");
            }

            // ── Email Notifications (fire-and-forget style) ──
            var plainPassword = dto.Password ?? "Welcome@123";
            _ = Task.Run(async () =>
            {
                try
                {
                    // 1. Welcome email to user
                    if (!string.IsNullOrWhiteSpace(user.Emailid))
                    {
                        var body = BuildWelcomeEmailBody(user.Name ?? user.Username!, user.Username!, plainPassword, user.Usercode!);
                        await _notification.SendEmailAsync(user.Emailid, "Welcome to MinePress ERP — Your Login Credentials", body);
                    }

                    // 2. HR notification email
                    var hrEmail = await GetHrEmailAsync();
                    if (!string.IsNullOrWhiteSpace(hrEmail))
                    {
                        var hrBody = BuildHrNotificationBody(user.Name ?? user.Username!, user.Usercode!, user.UserType ?? "EMPLOYEE");
                        await _notification.SendEmailAsync(hrEmail, $"New User Created — {user.Name} ({user.Usercode})", hrBody);
                    }
                }
                catch (Exception emailEx)
                {
                    _logger.LogWarning(emailEx, "Failed to send user creation email notifications");
                }
            });

            return Ok(new { message = "User created successfully", userId = user.Userid });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(500, new { message = ex.InnerException?.Message ?? ex.Message });
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // USERS — Update
    // ═══════════════════════════════════════════════════════════════
    [HttpPut("users/{id}")]
    public async Task<IActionResult> UpdateUser(long id, [FromBody] UserDto dto)
    {
        try
        {
            var user = await _db.MstUsers.FirstOrDefaultAsync(u => u.Userid == id && u.Isdeleted != true);
            if (user == null) return NotFound(new { message = "User not found" });

            // ── Field-level validation ──
            var errors = ValidateUser(dto, isUpdate: true);
            if (errors.Count > 0)
                return BadRequest(new { message = errors[0], errors });

            // ── Duplicate checks (exclude current user) ──
            if (await _db.MstUsers.AnyAsync(u => u.Usercode == dto.UserCode && u.Userid != id && u.Isdeleted != true))
                return BadRequest(new { message = "User code already exists" });
            if (await _db.MstUsers.AnyAsync(u => u.Username == dto.Username && u.Userid != id && u.Isdeleted != true))
                return BadRequest(new { message = "Username already exists" });
            if (!string.IsNullOrWhiteSpace(dto.Email) && await _db.MstUsers.AnyAsync(u => u.Emailid == dto.Email && u.Userid != id && u.Isdeleted != true))
                return BadRequest(new { message = "Email address already in use" });

            // ── FK existence checks ──
            if (!await _db.MstDepartments.AnyAsync(d => d.DeptId == dto.DepartmentId && d.IsActive == true))
                return BadRequest(new { message = "Selected department does not exist or is inactive" });
            if (!await _db.MstDesignations.AnyAsync(d => d.DesignationId == dto.DesignationId && d.IsActive == true))
                return BadRequest(new { message = "Selected designation does not exist or is inactive" });
            if (!await _db.MstLocations.AnyAsync(l => l.LocationId == dto.LocationId && l.IsActive == true))
                return BadRequest(new { message = "Selected location does not exist or is inactive" });

            user.Usercode = dto.UserCode;
            user.Username = dto.Username;
            user.Name = dto.Name;
            user.Emailid = dto.Email;
            user.Mobileno = dto.Mobile;
            user.UserType = dto.UserType ?? user.UserType;
            user.Departmentid = dto.DepartmentId;
            user.Designationid = dto.DesignationId;
            user.Locationid = dto.LocationId;
            user.Employeecode = dto.EmployeeCode;
            user.Issystemadmin = dto.IsSystemAdmin;
            user.Isapprovaluser = dto.IsApprovalUser;
            user.Isproductionuser = dto.IsProductionUser;
            user.Iswebaccessallowed = dto.IsWebAccess;
            user.Ismobileaccessallowed = dto.IsMobileAccess;
            user.Updatedby = CurrentUser?.UserCode ?? "SYSTEM";
            user.Updatedat = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            // ── Activity Log ──
            try
            {
                var session = CurrentUser;
                if (session != null)
                {
                    var logEntry = ActivityLogEntry.FromUser(session, "USER_MGMT", "UPDATE", $"Updated user {user.Usercode} — {user.Name}");
                    logEntry.EntityType = "USER";
                    logEntry.EntityId = user.Userid;
                    logEntry.EntityCode = user.Usercode;
                    logEntry.Description = $"User updated: {user.Name} ({user.Username}), Type: {user.UserType}";
                    await _activity.LogActivityAsync(logEntry);
                }
            }
            catch (Exception actEx)
            {
                _logger.LogWarning(actEx, "Failed to log user update activity");
            }

            return Ok(new { message = "User updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            return StatusCode(500, new { message = ex.InnerException?.Message ?? ex.Message });
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // USERS — Toggle Active/Locked
    // ═══════════════════════════════════════════════════════════════
    [HttpPost("users/{id}/toggle-status")]
    public async Task<IActionResult> ToggleUserStatus(long id, [FromBody] ToggleDto dto)
    {
        var user = await _db.MstUsers.FirstOrDefaultAsync(u => u.Userid == id && u.Isdeleted != true);
        if (user == null) return NotFound(new { message = "User not found" });

        if (dto.Field == "active")
        {
            user.Isactive = !user.Isactive;
            await _db.SaveChangesAsync();
            return Ok(new { message = user.Isactive == true ? "User activated" : "User deactivated" });
        }
        if (dto.Field == "locked")
        {
            user.Islocked = false;
            user.Failedlogincount = 0;
            await _db.SaveChangesAsync();
            return Ok(new { message = "User unlocked" });
        }

        return BadRequest(new { message = "Invalid field" });
    }

    // ═══════════════════════════════════════════════════════════════
    // USERS — Reset Password
    // ═══════════════════════════════════════════════════════════════
    [HttpPost("users/{id}/reset-password")]
    public async Task<IActionResult> ResetPassword(long id)
    {
        var user = await _db.MstUsers.FirstOrDefaultAsync(u => u.Userid == id && u.Isdeleted != true);
        if (user == null) return NotFound(new { message = "User not found" });

        user.Passwordhash = HashPassword("Welcome@123");
        user.MustChangePassword = true;
        user.Lastpasswordchange = DateTime.UtcNow;
        user.Updatedby = CurrentUser?.UserCode ?? "SYSTEM";
        user.Updatedat = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(new { message = "Password reset to default (Welcome@123)" });
    }

    // ═══════════════════════════════════════════════════════════════
    // USER-ROLE MAPPING — Get roles assigned to a user
    // ═══════════════════════════════════════════════════════════════
    [HttpGet("users/{id}/roles")]
    public async Task<IActionResult> GetUserRoles(long id)
    {
        var user = await _db.MstUsers.FirstOrDefaultAsync(u => u.Userid == id && u.Isdeleted != true);
        if (user == null) return NotFound(new { message = "User not found" });

        var assignedRoleIds = await _db.MapUserRoles
            .Where(ur => ur.Userid == id && ur.Isactive == true)
            .Select(ur => ur.Roleid)
            .ToListAsync();

        var allRoles = await _db.MstRoles
            .Where(r => r.Isactive == true)
            .OrderBy(r => r.Rolename)
            .Select(r => new
            {
                roleId = r.Roleid,
                roleCode = r.Rolecode,
                roleName = r.Rolename,
                description = r.Description,
                isAssigned = assignedRoleIds.Contains(r.Roleid)
            })
            .ToListAsync();

        return Ok(new { userId = id, userName = user.Name, roles = allRoles });
    }

    // ═══════════════════════════════════════════════════════════════
    // USER-ROLE MAPPING — Sync roles for a user
    // ═══════════════════════════════════════════════════════════════
    [HttpPost("users/{id}/roles")]
    public async Task<IActionResult> SaveUserRoles(long id, [FromBody] UserRoleSaveDto dto)
    {
        try
        {
            var user = await _db.MstUsers.FirstOrDefaultAsync(u => u.Userid == id && u.Isdeleted != true);
            if (user == null) return NotFound(new { message = "User not found" });

            // Deactivate existing mappings
            var existing = await _db.MapUserRoles.Where(ur => ur.Userid == id).ToListAsync();
            foreach (var e in existing)
                e.Isactive = false;

            // Activate or create new mappings
            foreach (var roleId in dto.RoleIds ?? [])
            {
                var map = existing.FirstOrDefault(e => e.Roleid == roleId);
                if (map != null)
                {
                    map.Isactive = true;
                }
                else
                {
                    _db.MapUserRoles.Add(new MapUserRole
                    {
                        Userid = id,
                        Roleid = roleId,
                        Isactive = true
                    });
                }
            }

            await _db.SaveChangesAsync();

            // Activity log
            try
            {
                var session = CurrentUser;
                if (session != null)
                {
                    var logEntry = ActivityLogEntry.FromUser(session, "USER_MGMT", "ROLE_ASSIGN",
                        $"Updated role assignments for {user.Usercode} — {user.Name}");
                    logEntry.EntityType = "USER";
                    logEntry.EntityId = user.Userid;
                    logEntry.EntityCode = user.Usercode;
                    logEntry.Description = $"Roles updated: [{string.Join(", ", dto.RoleIds ?? [])}] for user {user.Name}";
                    await _activity.LogActivityAsync(logEntry);
                }
            }
            catch (Exception actEx)
            {
                _logger.LogWarning(actEx, "Failed to log role assignment activity");
            }

            return Ok(new { message = "User roles updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving user roles for {UserId}", id);
            return StatusCode(500, new { message = ex.InnerException?.Message ?? ex.Message });
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // ROLES — List
    // ═══════════════════════════════════════════════════════════════
    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _db.MstRoles
            .OrderBy(r => r.Rolename)
            .Select(r => new
            {
                roleId = r.Roleid,
                roleCode = r.Rolecode,
                roleName = r.Rolename,
                description = r.Description,
                isSystem = r.Issystem ?? false,
                isActive = r.Isactive ?? false
            })
            .ToListAsync();

        return Ok(roles);
    }

    // ═══════════════════════════════════════════════════════════════
    // ROLES — Create / Update
    // ═══════════════════════════════════════════════════════════════
    [HttpPost("roles")]
    public async Task<IActionResult> SaveRole([FromBody] RoleDto dto)
    {
        try
        {
            // ── Field-level validation ──
            var errors = ValidateRole(dto);
            if (errors.Count > 0)
                return BadRequest(new { message = errors[0], errors });

            MstRole role;
            if (dto.RoleId > 0)
            {
                role = await _db.MstRoles.FirstOrDefaultAsync(r => r.Roleid == dto.RoleId);
                if (role == null) return NotFound(new { message = "Role not found" });

                if (await _db.MstRoles.AnyAsync(r => r.Rolecode == dto.RoleCode && r.Roleid != dto.RoleId))
                    return BadRequest(new { message = "Role code already exists" });

                role.Rolecode = dto.RoleCode;
                role.Rolename = dto.RoleName;
                role.Description = dto.Description;
            }
            else
            {
                if (await _db.MstRoles.AnyAsync(r => r.Rolecode == dto.RoleCode))
                    return BadRequest(new { message = "Role code already exists" });

                role = new MstRole
                {
                    Rolecode = dto.RoleCode,
                    Rolename = dto.RoleName,
                    Description = dto.Description,
                    Isactive = true,
                    Issystem = false,
                    Createdat = DateTime.UtcNow
                };
                _db.MstRoles.Add(role);
            }

            await _db.SaveChangesAsync();
            return Ok(new { message = dto.RoleId > 0 ? "Role updated" : "Role created", roleId = role.Roleid });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving role");
            return StatusCode(500, new { message = ex.InnerException?.Message ?? ex.Message });
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // ROLES — Toggle Active
    // ═══════════════════════════════════════════════════════════════
    [HttpPost("roles/{id}/toggle")]
    public async Task<IActionResult> ToggleRole(int id)
    {
        var role = await _db.MstRoles.FindAsync(id);
        if (role == null) return NotFound(new { message = "Role not found" });

        role.Isactive = !(role.Isactive ?? false);
        await _db.SaveChangesAsync();
        return Ok(new { message = role.Isactive == true ? "Role activated" : "Role deactivated" });
    }

    // ═══════════════════════════════════════════════════════════════
    // PERMISSIONS — List
    // ═══════════════════════════════════════════════════════════════
    [HttpGet("permissions")]
    public async Task<IActionResult> GetPermissions()
    {
        var perms = await _db.MstPermissions
            .OrderBy(p => p.Modulename).ThenBy(p => p.Permissionname)
            .Select(p => new
            {
                permissionId = p.Permissionid,
                permissionCode = p.Permissioncode,
                permissionName = p.Permissionname,
                moduleName = p.Modulename,
                isActive = p.Isactive ?? false
            })
            .ToListAsync();

        return Ok(perms);
    }

    // ═══════════════════════════════════════════════════════════════
    // PERMISSIONS — Create / Update
    // ═══════════════════════════════════════════════════════════════
    [HttpPost("permissions")]
    public async Task<IActionResult> SavePermission([FromBody] PermDto dto)
    {
        try
        {
            // ── Field-level validation ──
            var errors = ValidatePermission(dto);
            if (errors.Count > 0)
                return BadRequest(new { message = errors[0], errors });

            MstPermission perm;
            if (dto.PermissionId > 0)
            {
                perm = await _db.MstPermissions.FirstOrDefaultAsync(p => p.Permissionid == dto.PermissionId);
                if (perm == null) return NotFound(new { message = "Permission not found" });

                if (await _db.MstPermissions.AnyAsync(p => p.Permissioncode == dto.PermissionCode && p.Permissionid != dto.PermissionId))
                    return BadRequest(new { message = "Permission code already exists" });

                perm.Permissioncode = dto.PermissionCode;
                perm.Permissionname = dto.PermissionName;
                perm.Modulename = dto.ModuleName;
            }
            else
            {
                if (await _db.MstPermissions.AnyAsync(p => p.Permissioncode == dto.PermissionCode))
                    return BadRequest(new { message = "Permission code already exists" });

                perm = new MstPermission
                {
                    Permissioncode = dto.PermissionCode,
                    Permissionname = dto.PermissionName,
                    Modulename = dto.ModuleName,
                    Isactive = true
                };
                _db.MstPermissions.Add(perm);
            }

            await _db.SaveChangesAsync();
            return Ok(new { message = dto.PermissionId > 0 ? "Permission updated" : "Permission created", permissionId = perm.Permissionid });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving permission");
            return StatusCode(500, new { message = ex.InnerException?.Message ?? ex.Message });
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // USER-PERMISSION MAPPING — Get permissions assigned to a user
    // ═══════════════════════════════════════════════════════════════
    [HttpGet("users/{id}/permissions")]
    public async Task<IActionResult> GetUserPermissions(long id)
    {
        var user = await _db.MstUsers.FirstOrDefaultAsync(u => u.Userid == id && u.Isdeleted != true);
        if (user == null) return NotFound(new { message = "User not found" });

        var assignedPermIds = await _db.MapUserPermissions
            .Where(up => up.Userid == id && up.Isallowed == true)
            .Select(up => up.Permissionid)
            .ToListAsync();

        var allPerms = await _db.MstPermissions
            .Where(p => p.Isactive == true)
            .OrderBy(p => p.Modulename).ThenBy(p => p.Permissionname)
            .Select(p => new
            {
                permissionId = p.Permissionid,
                permissionCode = p.Permissioncode,
                permissionName = p.Permissionname,
                moduleName = p.Modulename,
                isAssigned = assignedPermIds.Contains(p.Permissionid)
            })
            .ToListAsync();

        return Ok(new { userId = id, userName = user.Name, permissions = allPerms });
    }

    // ═══════════════════════════════════════════════════════════════
    // USER-PERMISSION MAPPING — Sync permissions for a user
    // ═══════════════════════════════════════════════════════════════
    [HttpPost("users/{id}/permissions")]
    public async Task<IActionResult> SaveUserPermissions(long id, [FromBody] UserPermissionSaveDto dto)
    {
        try
        {
            var user = await _db.MstUsers.FirstOrDefaultAsync(u => u.Userid == id && u.Isdeleted != true);
            if (user == null) return NotFound(new { message = "User not found" });

            // Deactivate existing mappings
            var existing = await _db.MapUserPermissions.Where(up => up.Userid == id).ToListAsync();
            foreach (var e in existing)
                e.Isallowed = false;

            // Activate or create new mappings
            foreach (var permId in dto.PermissionIds ?? [])
            {
                var map = existing.FirstOrDefault(e => e.Permissionid == permId);
                if (map != null)
                {
                    map.Isallowed = true;
                }
                else
                {
                    _db.MapUserPermissions.Add(new MapUserPermission
                    {
                        Userid = id,
                        Permissionid = permId,
                        Isallowed = true
                    });
                }
            }

            await _db.SaveChangesAsync();

            // Activity log
            try
            {
                var session = CurrentUser;
                if (session != null)
                {
                    var logEntry = ActivityLogEntry.FromUser(session, "USER_MGMT", "PERMISSION_ASSIGN",
                        $"Updated permission assignments for {user.Usercode} — {user.Name}");
                    logEntry.EntityType = "USER";
                    logEntry.EntityId = user.Userid;
                    logEntry.EntityCode = user.Usercode;
                    logEntry.Description = $"Permissions updated: [{string.Join(", ", dto.PermissionIds ?? [])}] for user {user.Name}";
                    await _activity.LogActivityAsync(logEntry);
                }
            }
            catch (Exception actEx)
            {
                _logger.LogWarning(actEx, "Failed to log permission assignment activity");
            }

            return Ok(new { message = "User permissions updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving user permissions for {UserId}", id);
            return StatusCode(500, new { message = ex.InnerException?.Message ?? ex.Message });
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // MENUS — List (hierarchical)
    // ═══════════════════════════════════════════════════════════════
    [HttpGet("menus")]
    public async Task<IActionResult> GetMenus()
    {
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

    // ═══════════════════════════════════════════════════════════════
    // LOOKUPS — Departments, Designations, Locations
    // ═══════════════════════════════════════════════════════════════
    [HttpGet("lookups")]
    public async Task<IActionResult> GetLookups()
    {
        var departments = await _db.MstDepartments
            .Where(d => d.IsActive == true)
            .OrderBy(d => d.DeptName)
            .Select(d => new { id = d.DeptId, name = d.DeptName })
            .ToListAsync();

        var designations = await _db.MstDesignations
            .Where(d => d.IsActive == true)
            .OrderBy(d => d.DesignationName)
            .Select(d => new { id = d.DesignationId, name = d.DesignationName })
            .ToListAsync();

        var locations = await _db.MstLocations
            .Where(l => l.IsActive == true)
            .OrderBy(l => l.LocationName)
            .Select(l => new { id = l.LocationId, name = l.LocationName })
            .ToListAsync();

        return Ok(new { departments, designations, locations });
    }

    // ═══════════════════════════════════════════════════════════════
    // Validation
    // ═══════════════════════════════════════════════════════════════
    private static readonly string[] ValidUserTypes = ["EMPLOYEE", "CONTRACTOR", "VENDOR", "ADMIN", "GUEST"];

    private static List<string> ValidateUser(UserDto dto, bool isUpdate = false)
    {
        var errors = new List<string>();

        // Required fields
        if (string.IsNullOrWhiteSpace(dto.UserCode))
            errors.Add("User code is required");
        if (string.IsNullOrWhiteSpace(dto.Username))
            errors.Add("Username is required");
        if (string.IsNullOrWhiteSpace(dto.Name))
            errors.Add("Full name is required");
        if (dto.DepartmentId <= 0)
            errors.Add("Department is required");
        if (dto.DesignationId <= 0)
            errors.Add("Designation is required");
        if (dto.LocationId <= 0)
            errors.Add("Location is required");

        // String length limits
        if (!string.IsNullOrWhiteSpace(dto.UserCode) && dto.UserCode.Length > 20)
            errors.Add("User code must be 20 characters or less");
        if (!string.IsNullOrWhiteSpace(dto.Username) && dto.Username.Length > 50)
            errors.Add("Username must be 50 characters or less");
        if (!string.IsNullOrWhiteSpace(dto.Name) && dto.Name.Length > 100)
            errors.Add("Name must be 100 characters or less");
        if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email.Length > 150)
            errors.Add("Email must be 150 characters or less");
        if (!string.IsNullOrWhiteSpace(dto.Mobile) && dto.Mobile.Length > 15)
            errors.Add("Mobile number must be 15 characters or less");
        if (!string.IsNullOrWhiteSpace(dto.EmployeeCode) && dto.EmployeeCode.Length > 20)
            errors.Add("Employee code must be 20 characters or less");

        // Format: UserCode — alphanumeric, underscores, hyphens only
        if (!string.IsNullOrWhiteSpace(dto.UserCode) && !Regex.IsMatch(dto.UserCode, @"^[A-Za-z0-9_\-]+$"))
            errors.Add("User code must contain only letters, digits, underscores or hyphens");

        // Format: Username — no spaces, printable ASCII
        if (!string.IsNullOrWhiteSpace(dto.Username) && dto.Username.Contains(' '))
            errors.Add("Username must not contain spaces");

        // Format: Email
        if (!string.IsNullOrWhiteSpace(dto.Email) && !Regex.IsMatch(dto.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            errors.Add("Invalid email address format");

        // Format: Mobile — digits, optional leading +
        if (!string.IsNullOrWhiteSpace(dto.Mobile) && !Regex.IsMatch(dto.Mobile, @"^\+?[0-9]{7,15}$"))
            errors.Add("Mobile number must be 7-15 digits (optional leading +)");

        // UserType validation
        if (!string.IsNullOrWhiteSpace(dto.UserType) && !ValidUserTypes.Contains(dto.UserType.ToUpper()))
            errors.Add($"User type must be one of: {string.Join(", ", ValidUserTypes)}");

        // Password strength (only on create or when explicitly provided)
        if (!isUpdate && !string.IsNullOrWhiteSpace(dto.Password))
        {
            if (dto.Password.Length < 6)
                errors.Add("Password must be at least 6 characters");
            if (dto.Password.Length > 100)
                errors.Add("Password must be 100 characters or less");
        }

        return errors;
    }

    private static List<string> ValidateRole(RoleDto dto)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(dto.RoleCode))
            errors.Add("Role code is required");
        if (string.IsNullOrWhiteSpace(dto.RoleName))
            errors.Add("Role name is required");
        if (!string.IsNullOrWhiteSpace(dto.RoleCode) && dto.RoleCode.Length > 20)
            errors.Add("Role code must be 20 characters or less");
        if (!string.IsNullOrWhiteSpace(dto.RoleName) && dto.RoleName.Length > 100)
            errors.Add("Role name must be 100 characters or less");
        if (!string.IsNullOrWhiteSpace(dto.Description) && dto.Description.Length > 500)
            errors.Add("Description must be 500 characters or less");
        if (!string.IsNullOrWhiteSpace(dto.RoleCode) && !Regex.IsMatch(dto.RoleCode, @"^[A-Za-z0-9_\-]+$"))
            errors.Add("Role code must contain only letters, digits, underscores or hyphens");

        return errors;
    }

    private static List<string> ValidatePermission(PermDto dto)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(dto.PermissionCode))
            errors.Add("Permission code is required");
        if (string.IsNullOrWhiteSpace(dto.PermissionName))
            errors.Add("Permission name is required");
        if (!string.IsNullOrWhiteSpace(dto.PermissionCode) && dto.PermissionCode.Length > 50)
            errors.Add("Permission code must be 50 characters or less");
        if (!string.IsNullOrWhiteSpace(dto.PermissionName) && dto.PermissionName.Length > 100)
            errors.Add("Permission name must be 100 characters or less");
        if (!string.IsNullOrWhiteSpace(dto.ModuleName) && dto.ModuleName.Length > 100)
            errors.Add("Module name must be 100 characters or less");
        if (!string.IsNullOrWhiteSpace(dto.PermissionCode) && !Regex.IsMatch(dto.PermissionCode, @"^[A-Za-z0-9_\-\.]+$"))
            errors.Add("Permission code must contain only letters, digits, underscores, hyphens or dots");

        return errors;
    }

    // ═══════════════════════════════════════════════════════════════
    // Helpers
    // ═══════════════════════════════════════════════════════════════
    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    private async Task<string?> GetHrEmailAsync()
    {
        // Try to find an HR department user with email to send notification
        var hrUser = await _db.MstUsers
            .Where(u => u.Isdeleted != true && u.Isactive == true && u.Emailid != null
                && u.Department != null && u.Department.DeptName != null
                && u.Department.DeptName.ToUpper().Contains("HR"))
            .Select(u => u.Emailid)
            .FirstOrDefaultAsync();
        return hrUser;
    }

    private static string BuildWelcomeEmailBody(string name, string username, string password, string userCode)
    {
        return $@"
<!DOCTYPE html>
<html><head><meta charset='utf-8'/></head>
<body style='font-family:Segoe UI,Helvetica,Arial,sans-serif;margin:0;padding:0;background:#f4f6fa;'>
<div style='max-width:600px;margin:40px auto;background:#fff;border-radius:12px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,.08);'>
  <div style='background:linear-gradient(135deg,#6366f1,#8b5cf6);padding:32px 40px;text-align:center;'>
    <h1 style='color:#fff;margin:0;font-size:24px;'>Welcome to MinePress ERP</h1>
    <p style='color:rgba(255,255,255,.8);margin:8px 0 0;font-size:14px;'>Your account has been created successfully</p>
  </div>
  <div style='padding:32px 40px;'>
    <p style='font-size:16px;color:#334155;'>Hello <strong>{name}</strong>,</p>
    <p style='color:#64748b;line-height:1.6;'>Your MinePress ERP account is ready. Please use the following credentials to log in:</p>
    <div style='background:#f8fafc;border:1px solid #e2e8f0;border-radius:8px;padding:20px;margin:20px 0;'>
      <table style='width:100%;border-collapse:collapse;'>
        <tr><td style='padding:8px 0;color:#64748b;width:120px;'>User Code</td><td style='padding:8px 0;font-weight:600;color:#1e293b;'>{userCode}</td></tr>
        <tr><td style='padding:8px 0;color:#64748b;'>Username</td><td style='padding:8px 0;font-weight:600;color:#1e293b;'>{username}</td></tr>
        <tr><td style='padding:8px 0;color:#64748b;'>Password</td><td style='padding:8px 0;font-weight:600;color:#1e293b;'>{password}</td></tr>
      </table>
    </div>
    <div style='background:#fef3c7;border-left:4px solid #f59e0b;padding:12px 16px;border-radius:4px;margin:16px 0;'>
      <p style='margin:0;color:#92400e;font-size:13px;'><strong>⚠ Security Notice:</strong> Please change your password after your first login.</p>
    </div>
    <p style='color:#64748b;font-size:13px;margin-top:24px;'>If you have any questions, please contact your system administrator.</p>
  </div>
  <div style='background:#f8fafc;padding:16px 40px;text-align:center;border-top:1px solid #e2e8f0;'>
    <p style='margin:0;color:#94a3b8;font-size:12px;'>MinePress ERP &mdash; Powered by AI</p>
  </div>
</div>
</body></html>";
    }

    private static string BuildHrNotificationBody(string name, string userCode, string userType)
    {
        return $@"
<!DOCTYPE html>
<html><head><meta charset='utf-8'/></head>
<body style='font-family:Segoe UI,Helvetica,Arial,sans-serif;margin:0;padding:0;background:#f4f6fa;'>
<div style='max-width:600px;margin:40px auto;background:#fff;border-radius:12px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,.08);'>
  <div style='background:linear-gradient(135deg,#0ea5e9,#6366f1);padding:24px 40px;'>
    <h2 style='color:#fff;margin:0;font-size:20px;'>📋 New User Created — HR Notification</h2>
  </div>
  <div style='padding:28px 40px;'>
    <p style='color:#334155;'>A new user account has been created in MinePress ERP:</p>
    <div style='background:#f0fdf4;border:1px solid #bbf7d0;border-radius:8px;padding:16px;margin:16px 0;'>
      <table style='width:100%;border-collapse:collapse;'>
        <tr><td style='padding:6px 0;color:#64748b;width:120px;'>Name</td><td style='padding:6px 0;font-weight:600;color:#166534;'>{name}</td></tr>
        <tr><td style='padding:6px 0;color:#64748b;'>User Code</td><td style='padding:6px 0;font-weight:600;color:#166534;'>{userCode}</td></tr>
        <tr><td style='padding:6px 0;color:#64748b;'>User Type</td><td style='padding:6px 0;font-weight:600;color:#166534;'>{userType}</td></tr>
      </table>
    </div>
    <p style='color:#64748b;font-size:13px;'>Please update your records accordingly. This is an automated notification from MinePress ERP.</p>
  </div>
</div>
</body></html>";
    }

    // ═══════════════════════════════════════════════════════════════
    // DTOs
    // ═══════════════════════════════════════════════════════════════
    public record UserDto(
        string UserCode, string Username, string Name,
        string? Email, string? Mobile, string? UserType,
        long DepartmentId, long DesignationId, int LocationId,
        string? EmployeeCode, string? Password,
        bool? IsSystemAdmin, bool? IsApprovalUser, bool? IsProductionUser,
        bool? IsWebAccess, bool? IsMobileAccess,
        int[]? RoleIds);

    public record RoleDto(int RoleId, string RoleCode, string RoleName, string? Description, int[]? PermissionIds);
    public record PermDto(int PermissionId, string PermissionCode, string PermissionName, string? ModuleName);
    public record ToggleDto(string Field);
    public record UserRoleSaveDto(int[]? RoleIds);
    public record UserPermissionSaveDto(int[]? PermissionIds);
}
