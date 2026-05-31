using erp.minepress.infrastructure.ErrorLogging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using erp.minepress.persistence.Context;
using erp.minepress.web.Helpers;

namespace erp.minepress.web.Pages.Dashboard;

public class DeptDashboardModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly ISystemErrorLogger _systemErrorLogger;

    public DeptDashboardModel(ApplicationDbContext db, ISystemErrorLogger systemErrorLogger)
    {
        _db = db;
        _systemErrorLogger = systemErrorLogger;
    }

    public string DeptCode { get; set; } = string.Empty;
    public string DeptName { get; set; } = string.Empty;
    public string Remarks { get; set; } = string.Empty;
    public bool IsProduction { get; set; }
    public string? ParentDeptCode { get; set; }

    // Current user info
    public string UserName { get; set; } = string.Empty;
    public string UserFullName { get; set; } = string.Empty;
    public string UserDeptCode { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
    public bool CanViewAllDepts { get; set; }
    public bool IsApproval { get; set; }

    // All departments for quick navigation
    public List<DeptNavItem> AllDepartments { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(string deptCode)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return RedirectToPage("/Account/Login");

        if (string.IsNullOrWhiteSpace(deptCode))
            return RedirectToPage("/Dashboard/Index");

        // Look up the department
        var dept = await _db.MstDepartments
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.DeptCode == deptCode.ToUpper() && d.IsActive == true);

        if (dept == null)
            return RedirectToPage("/Dashboard/Index");

        DeptCode = dept.DeptCode!;
        DeptName = dept.DeptName ?? "Department";
        Remarks = dept.Remarks ?? string.Empty;
        IsProduction = dept.IsProduction ?? false;
        ParentDeptCode = dept.ParentDeptCode;

        UserName = user.UserCode;
        UserFullName = user.Name;
        UserDeptCode = user.DeptCode;
        IsAdmin = user.IsSystemAdmin;
        CanViewAllDepts = user.IsSystemAdmin || user.DeptCode == "MGT" || user.DeptCode == "ADM";
        IsApproval = user.IsApprovalUser;

        // Load all active departments for quick nav
        AllDepartments = await _db.MstDepartments
            .AsNoTracking()
            .Where(d => d.IsActive == true)
            .OrderBy(d => d.DeptId)
            .Select(d => new DeptNavItem
            {
                Code = d.DeptCode!,
                Name = d.DeptName ?? "",
                IsProduction = d.IsProduction ?? false
            })
            .ToListAsync();

        return Page();
    }

    public class DeptNavItem
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsProduction { get; set; }
    }
}
