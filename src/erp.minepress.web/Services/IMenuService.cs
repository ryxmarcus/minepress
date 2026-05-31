using erp.minepress.persistence.Models;

namespace erp.minepress.web.Services;

/// <summary>
/// Provides dynamic menu data filtered by the logged-in user's department.
/// Flow: user → department → map_module_department → mst_menu
/// </summary>
public interface IMenuService
{
    /// <summary>
    /// Returns the menu tree (level-1 parents with their level-2 children)
    /// that the given department is allowed to see.
    /// System admins receive the full menu regardless of department.
    /// </summary>
    Task<List<MenuNode>> GetMenuForDepartmentAsync(long departmentId, bool isSystemAdmin);
}

/// <summary>
/// Represents a navigation menu node with optional children.
/// </summary>
public class MenuNode
{
    public int MenuId { get; set; }
    public string MenuCode { get; set; } = string.Empty;
    public string MenuName { get; set; } = string.Empty;
    public int? ParentMenuId { get; set; }
    public string? RouteUrl { get; set; }
    public string? Icon { get; set; }
    public int DisplayOrder { get; set; }
    public int MenuLevel { get; set; }
    public bool IsSectionHeader { get; set; }
    public string? SectionName { get; set; }
    public string? BadgeText { get; set; }
    public string? BadgeClass { get; set; }
    public bool HasDividerBefore { get; set; }
    public string? IconSvg { get; set; }
    public int? ModuleId { get; set; }

    /// <summary>Level-2 children of a level-1 parent.</summary>
    public List<MenuNode> Children { get; set; } = new();
}
