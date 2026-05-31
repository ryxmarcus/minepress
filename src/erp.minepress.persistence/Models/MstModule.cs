using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Master table for ERP navigation modules and menu items. Supports hierarchical parent-child structure (up to 3 levels), section headers, badges, inline SVG icons, and separate mobile/web visibility flags. Seeded from mst_menu.csv — original menuid values are preserved as module_id.
/// </summary>
public partial class MstModule
{
    /// <summary>
    /// Primary key. Preserved from original mst_menu.menuid values (1-1217). New rows after seeding use sequence press_db.mst_module_module_id_seq starting at 2000.
    /// </summary>
    public int ModuleId { get; set; }

    /// <summary>
    /// Unique business key for the module. Used by application layer for permission checks and role-menu mapping. e.g. DASHBOARD, SALES_CRM, RATE_CALCULATOR.
    /// </summary>
    public string ModuleCode { get; set; } = null!;

    /// <summary>
    /// Display name shown in the navigation sidebar. e.g. Dashboard, Sales &amp; CRM.
    /// </summary>
    public string ModuleName { get; set; } = null!;

    /// <summary>
    /// Self-referencing FK. NULL for root/top-level modules (module_level=1). Points to parent module_id for child items (module_level=2).
    /// </summary>
    public int? ParentModuleId { get; set; }

    /// <summary>
    /// Blazor client-side route for navigation. NULL for group header items that have no direct page (e.g. SALES_CRM, PRODUCTION).
    /// </summary>
    public string? RouteUrl { get; set; }

    /// <summary>
    /// Tabler Icons icon name (without the ti- prefix). e.g. home, building-bank, package, chart-bar.
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Render order within the parent group. Lower values appear first in sidebar.
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// true = visible in the .NET MAUI mobile app navigation. false = web-only (Blazor).
    /// </summary>
    public bool IsMobile { get; set; }

    /// <summary>
    /// true = visible in the Blazor web application navigation.
    /// </summary>
    public bool IsWeb { get; set; }

    /// <summary>
    /// Soft enable/disable. Inactive modules are hidden from all navigation.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// 1 = root top-level item (section group). 2 = child leaf item under a root. 3 = reserved for future deep nesting.
    /// </summary>
    public short ModuleLevel { get; set; }

    /// <summary>
    /// If true, this row renders as a non-clickable section group label in the sidebar (e.g. Sales, CRM, Reports headings within a parent group).
    /// </summary>
    public bool IsSectionHeader { get; set; }

    /// <summary>
    /// Label shown as a section divider above child items. e.g. Sales, CRM, Reports, Plate Making, Post-Press.
    /// </summary>
    public string? SectionName { get; set; }

    /// <summary>
    /// Short badge label displayed on the menu item. e.g. New, Beta, Soon.
    /// </summary>
    public string? BadgeText { get; set; }

    /// <summary>
    /// Tabler/Bootstrap CSS classes for the badge. e.g. badge badge-sm bg-red-lt, badge badge-sm bg-blue-lt.
    /// </summary>
    public string? BadgeClass { get; set; }

    /// <summary>
    /// If true, a horizontal &lt;hr&gt; divider is rendered above this menu item in the sidebar.
    /// </summary>
    public bool HasDividerBefore { get; set; }

    /// <summary>
    /// Full inline SVG markup for the icon. Used primarily for level-1 root items that require custom branded SVG icons rather than standard Tabler icon names.
    /// </summary>
    public string? IconSvg { get; set; }

    /// <summary>
    /// Username or user ID of the person who created this record.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Timestamp when this record was created. Defaults to CURRENT_TIMESTAMP.
    /// </summary>
    public DateTime CreatedOn { get; set; }

    /// <summary>
    /// Username or user ID of the person who last modified this record.
    /// </summary>
    public string? ModifiedBy { get; set; }

    /// <summary>
    /// Timestamp of the last modification. NULL if never updated after creation.
    /// </summary>
    public DateTime? ModifiedOn { get; set; }

    public virtual ICollection<MstModule> InverseParentModule { get; set; } = new List<MstModule>();

    public virtual ICollection<MstMenu> MstMenus { get; set; } = new List<MstMenu>();

    public virtual MstModule? ParentModule { get; set; }
}
