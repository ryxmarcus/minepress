using erp.minepress.persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace erp.minepress.web.Services;

/// <summary>
/// Loads menus from mst_menu filtered by the user's department via map_module_department.
/// Flow: login user → get department → check map_module_department → filter mst_menu by module_id → build tree.
/// </summary>
public class MenuService : IMenuService
{
    private readonly ApplicationDbContext _db;

    public MenuService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<MenuNode>> GetMenuForDepartmentAsync(long departmentId, bool isSystemAdmin)
    {
        // Step 1: Get allowed module_ids for this department
        HashSet<int> allowedModuleIds;

        if (isSystemAdmin)
        {
            // System admin sees everything
            allowedModuleIds = (await _db.MstMenus
                .Where(m => m.Isactive == true && m.ModuleId.HasValue)
                .Select(m => m.ModuleId!.Value)
                .Distinct()
                .ToListAsync())
                .ToHashSet();
        }
        else
        {
            // Normal user: check map_module_department
            allowedModuleIds = (await _db.MapModuleDepartments
                .Where(md => md.DepartmentId == departmentId && md.IsActive)
                .Select(md => md.ModuleId)
                .Distinct()
                .ToListAsync())
                .ToHashSet();
        }

        if (allowedModuleIds.Count == 0)
            return new List<MenuNode>();

        // Step 2: Load all active web menus whose module_id is in the allowed set
        var allMenus = await _db.MstMenus
            .Where(m => m.Isactive == true && m.Isweb == true && m.ModuleId.HasValue
                        && allowedModuleIds.Contains(m.ModuleId!.Value))
            .OrderBy(m => m.Displayorder)
            .Select(m => new MenuNode
            {
                MenuId = m.Menuid,
                MenuCode = m.Menucode,
                MenuName = m.Menuname,
                ParentMenuId = m.Parentmenuid,
                RouteUrl = m.Routeurl,
                Icon = m.Icon,
                DisplayOrder = m.Displayorder ?? 0,
                MenuLevel = m.Menulevel ?? 1,
                IsSectionHeader = m.Issectionheader ?? false,
                SectionName = m.Sectionname,
                BadgeText = m.Badgetext,
                BadgeClass = m.Badgeclass,
                HasDividerBefore = m.Hasdividerbefore ?? false,
                IconSvg = m.Iconsvg,
                ModuleId = m.ModuleId
            })
            .ToListAsync();

        // Step 3: Build parent-child tree
        var parents = allMenus.Where(m => m.MenuLevel == 1).OrderBy(m => m.DisplayOrder).ToList();
        var childLookup = allMenus.Where(m => m.MenuLevel == 2)
            .GroupBy(m => m.ParentMenuId ?? 0)
            .ToDictionary(g => g.Key, g => g.OrderBy(c => c.DisplayOrder).ToList());

        foreach (var parent in parents)
        {
            if (childLookup.TryGetValue(parent.MenuId, out var children))
            {
                parent.Children = children;
            }
        }

        // Only include parents that either have a direct route or have children
        return parents.Where(p => !string.IsNullOrEmpty(p.RouteUrl) || p.Children.Count > 0).ToList();
    }
}
