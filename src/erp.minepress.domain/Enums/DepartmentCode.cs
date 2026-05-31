namespace erp.minepress.domain.Enums;

/// <summary>
/// Department codes matching mst_department.dept_code.
/// Used to resolve department-specific dashboards and workflows.
/// </summary>
public enum DepartmentCode
{
    /// <summary>Top Management — Owners, Directors</summary>
    MGT = 1001,

    /// <summary>Administration — Admin &amp; legal</summary>
    ADM = 1002,

    /// <summary>Human Resource — HR &amp; payroll</summary>
    HR = 1003,

    /// <summary>Accounts &amp; Finance — Accounts &amp; taxation</summary>
    FIN = 1004,

    /// <summary>IT &amp; ERP Support — ERP &amp; systems</summary>
    IT = 1005,

    /// <summary>Sales &amp; Marketing</summary>
    SAL = 1006,

    /// <summary>Customer Service &amp; CRM — Client handling</summary>
    CST = 1007,

    /// <summary>Estimation &amp; Costing — Quotation &amp; costing</summary>
    EST = 1008,

    /// <summary>Pre-Press &amp; Design — Design to plate making (Production)</summary>
    PRE = 1009,

    /// <summary>Printing — All printing types (Production)</summary>
    PRT = 1010,

    /// <summary>Post-Press &amp; Finishing — Cutting to final finish (Production)</summary>
    FINP = 1011,

    /// <summary>Packaging — Packing (Production)</summary>
    PKG = 1012,

    /// <summary>Dispatch &amp; Logistics — Delivery &amp; transport (Production)</summary>
    DSP = 1013,

    /// <summary>Inventory &amp; Stores — All stores</summary>
    INV = 1014,

    /// <summary>Purchase — Procurement</summary>
    PUR = 1015,

    /// <summary>Quality Management — Quality &amp; inspection (Production)</summary>
    QMS = 1016,

    /// <summary>Maintenance &amp; Utilities — All maintenance (Production)</summary>
    MNT = 1017,

    /// <summary>Security &amp; Gatepass — Security operations</summary>
    SEC = 1018,
    /// <summary>Proprietary — Parties users Department</summary>
    PTY = 9999
}
