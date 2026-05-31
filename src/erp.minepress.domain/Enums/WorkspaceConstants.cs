namespace erp.minepress.domain.Enums;

// ═══════════════════════════════════════════════════════════════════════════
//  WORKSPACE CONSTANTS — Replaces all hardcoded magic strings in workspace engine
//  Ref: User prompt — "dont use the hard code names, make enums for all"
//  Source: mst_process.csv, mst_department.csv, mst_process_notification_config.csv
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>
/// Process codes from mst_process.process_code.
/// Ref: docs/erp-data/mst_process.csv — 42 workflow processes (seq 2–42).
/// </summary>
public static class WkProcessCode
{
    // ── Sales & Enquiry ──
    public const string EnqJob = "ENQ_JOB";             // seq 2 — Enquiry Received / Job Enquiry
    public const string EnqEst = "ENQ_EST";             // seq 3 — Estimation / Costing Started
    public const string Quot = "QUOT";                   // seq 4 — Quotation Generated
    public const string QuotAppr = "QUOT_APPR";         // seq 5 — Quotation Approval (disabled)
    public const string QuotApproval = "QUOT_APPROVAL";  // alias (disabled)
    public const string QuotationApproval = "QUOTATION_APPROVAL"; // alias (disabled)

    // ── Job Creation ──
    public const string JobCreate = "JOB_CREATE";       // seq 6 — Job Created / Confirmed
    public const string JobApproval = "JOB_APPROVAL";   // seq 7 — Job Approval

    // ── Pre-Press ──
    public const string DesDtp = "DES_DTP";             // seq 8 — DTP / Artwork
    public const string Proof = "PROOF";                 // seq 9 — Proofing
    public const string PrePress = "PRE_PRESS";         // seq 10 — Pre-Press / CTP

    // ── Procurement ──
    public const string Proc = "PROC";                   // seq 11 — Procurement (disabled)
    public const string Grn = "GRN";                     // seq 12 — GRN (disabled)
    public const string QcIn = "QC_IN";                  // seq 13 — QC Inward (disabled)
    public const string StoreIssue = "STORE_ISSUE";     // seq 14 — Store Issue

    // ── Planning & Scheduling ──
    public const string JobPlan = "JOB_PLAN";           // seq 15 — Job Planning
    public const string JobSched = "JOB_SCHED";         // seq 16 — Job Scheduling
    public const string JobCard = "JOB_CARD";           // seq 17 — Job Card Issue

    // ── Production ──
    public const string Cut = "CUT";                     // seq 18 — Cutting
    public const string Print = "PRINT";                 // seq 19 — Printing
    public const string QcProc = "QC_PROC";             // seq 20 — QC (In-Process)
    public const string Dry = "DRY";                     // seq 21 — Drying / Curing
    public const string PostPress = "POST_PRESS";       // seq 22 — Post-Press
    public const string Fold = "FOLD";                   // seq 23 — Folding
    public const string Bind = "BIND";                   // seq 24 — Binding
    public const string Trim = "TRIM";                   // seq 25 — Trimming / Final Cut
    public const string QcPost = "QC_POST";             // seq 26 — QC (Post-Press)

    // ── Dispatch ──
    public const string Pack = "PACK";                   // seq 27 — Packing
    public const string Load = "LOAD";                   // seq 28 — Loading
    public const string Challan = "CHALLAN";             // seq 29 — Challan / DC
    public const string GatePass = "GATE_PASS";         // seq 30 — Gate Pass
    public const string Dispatch = "DISPATCH";           // seq 31 — Dispatch

    // ── Post-Delivery ──
    public const string DeliveryConf = "DELIVERY_CONF"; // seq 32 — Delivery Confirmation
    public const string Bill = "BILL";                   // seq 33 — Billing / Invoice
    public const string PayRec = "PAY_REC";             // seq 34 — Payment Receipt
    public const string CreditNote = "CREDIT_NOTE";     // seq 35 — Credit Note
    public const string DebitNote = "DEBIT_NOTE";       // seq 36 — Debit Note

    // ── Inventory & Costing ──
    public const string StoreReturn = "STORE_RETURN";   // seq 37 — Store Return
    public const string WasteEntry = "WASTE_ENTRY";     // seq 38 — Waste Entry
    public const string CostFinal = "COST_FINAL";       // seq 39 — Final Costing
    public const string ProfitAnalysis = "PROFIT_ANALYSIS"; // seq 40 — Profit Analysis

    // ── Closure ──
    public const string JobClose = "JOB_CLOSE";         // seq 41 — Job Close
    public const string JobArchive = "JOB_ARCHIVE";     // seq 42 — Archive

    // ── Pre-Sales (seq 1) ──
    public const string AdvPay = "ADV_PAY";             // seq 1 — Advance Payment (optional, pre-job)

    /// <summary>
    /// Process codes that are disabled/skipped in workspace task generation.
    /// Ref: DisabledProcessCodes array in WorkspaceProcessEngine.
    /// </summary>
    public static readonly string[] Disabled =
        [QuotAppr, QuotApproval, QuotationApproval, Proc, Grn, QcIn];

    /// <summary>
    /// Pre-job processes that must be skipped when the source is already a Job (trn_job).
    /// Once a Job exists, the workflow must never loop back to enquiry/estimation/quotation steps.
    /// Includes ADV_PAY (seq 1) which is optional advance payment before job creation.
    /// </summary>
    public static readonly string[] PreJobProcesses =
        [AdvPay, EnqJob, EnqEst, Quot, QuotAppr, QuotApproval, QuotationApproval, JobCreate];

    /// <summary>
    /// Job approval process codes that must only route to MGT/ADM/EST departments.
    /// Customer/party approvals are suppressed for these processes.
    /// </summary>
    public static readonly string[] ApprovalProcessCodes = [JobApproval];
}

/// <summary>
/// Event type codes from mst_process_notification_config.event_type_code.
/// Ref: docs/erp-data/mst_process_notification_config.csv.
/// </summary>
public static class WkEventTypeCode
{
    public const string ProcStart = "PROC_START";
    public const string ProcComplete = "PROC_COMPLETE";
    public const string ProcApproval = "PROC_APPROVAL";
    public const string TaskAssign = "TASK_ASSIGN";
    public const string TaskAssigned = "TASK_ASSIGNED";
    public const string TaskStart = "TASK_START";
    public const string TaskComplete = "TASK_COMPLETE";
    public const string TaskCompleted = "TASK_COMPLETED";
    public const string TaskOverdue = "TASK_OVERDUE";
    public const string TaskStarted = "TASK_STARTED";
    public const string ApprovalRequest = "APPROVAL_REQUEST";
    public const string ApprovalApproved = "APPROVAL_APPROVED";
    public const string ApprovalRejected = "APPROVAL_REJECTED";
    public const string OverdueAlert = "OVERDUE_ALERT";
    public const string TopupAlert = "TOPUP_ALERT";
    public const string ClientNotify = "CLIENT_NOTIFY";
    public const string AiInsight = "AI_INSIGHT";
    public const string StatusChanged = "STATUS_CHANGED";
}

/// <summary>
/// Task status values stored in trn_workspace_task.task_status.
/// </summary>
public static class WkTaskStatus
{
    public const string Queued = "QUEUED";          // Pre-generated, waiting for previous task to complete
    public const string Pending = "PENDING";        // Active, ready for user action
    public const string InProgress = "IN_PROGRESS"; // User has started working
    public const string Completed = "COMPLETED";    // Task finished successfully
    public const string Cancelled = "CANCELLED";   // Task cancelled/skipped
    public const string Rejected = "REJECTED";      // Approval rejected
    public const string Approved = "APPROVED";      // Approval granted
    public const string Overdue = "OVERDUE";        // Past due date
}

/// <summary>
/// Task type values stored in trn_workspace_task.task_type.
/// </summary>
public static class WkTaskType
{
    public const string Task = "TASK";
    public const string Approval = "APPROVAL";
    public const string FollowUp = "FOLLOW_UP";
    public const string Review = "REVIEW";
}

/// <summary>
/// Priority values stored in trn_workspace_task.priority.
/// </summary>
public static class WkPriority
{
    public const string Urgent = "URGENT";
    public const string High = "HIGH";
    public const string Normal = "NORMAL";
    public const string Low = "LOW";
    public const string Critical = "CRITICAL";
}

/// <summary>
/// Source table names used to identify the originating transaction.
/// </summary>
public static class WkSourceTable
{
    public const string Enquiry = "trn_enquiry";
    public const string Quotation = "trn_quotation";
    public const string Job = "trn_job";
    public const string Challan = "trn_challan";
    public const string PurchaseOrder = "trn_purchase_order";
    public const string SalesInvoice = "trn_sales_invoice";
}

/// <summary>
/// Module name identifiers used in activity logs and notifications.
/// </summary>
public static class WkModuleName
{
    public const string Enquiry = "ENQUIRY";
    public const string Quotation = "QUOTATION";
    public const string Job = "JOB";
    public const string Challan = "CHALLAN";
    public const string Purchase = "PURCHASE";
    public const string Invoice = "INVOICE";
    public const string Workspace = "WORKSPACE";
}

/// <summary>
/// Enquiry status values.
/// </summary>
public static class WkEnquiryStatus
{
    public const string Draft = "DRAFT";
    public const string Submitted = "SUBMITTED";
    public const string Approved = "APPROVED";
    public const string Cancelled = "CANCELLED";
    public const string Closed = "CLOSED";
    public const string Converted = "CONVERTED";
}

/// <summary>
/// Item-level task status values for trn_workspace_task_item.task_status.
/// Per-item independent tracking for parallel execution.
/// </summary>
public static class WkItemTaskStatus
{
    public const string NotStarted = "NOT_STARTED";
    public const string Running = "RUNNING";
    public const string Completed = "COMPLETED";
    public const string Closed = "CLOSED";
}

/// <summary>
/// Process codes that support item-level parallel execution.
/// When a workspace task has one of these process codes, the system creates
/// per-item sub-tasks that can run independently and trigger the next process per item.
/// </summary>
public static class WkParallelProcessCodes
{
    /// <summary>
    /// Processes that support item-level parallel task execution.
    /// </summary>
    public static readonly string[] Eligible =
        [WkProcessCode.DesDtp, WkProcessCode.PrePress, WkProcessCode.PostPress];

    /// <summary>
    /// Dependency chain: when an item completes in the key process,
    /// create an item task in the value process.
    /// Design → CTP (PrePress), CTP → PostPress.
    /// </summary>
    public static readonly Dictionary<string, string> NextProcessPerItem = new()
    {
        [WkProcessCode.DesDtp] = WkProcessCode.PrePress,
        [WkProcessCode.PrePress] = WkProcessCode.PostPress
    };
}

/// <summary>
/// Party department ID constant.
/// Ref: mst_department — dept_id 9999 = "Party Related Activity (Approvals)".
/// </summary>
public static class WkDepartment
{
    public const int PartyDeptId = 9999;

    /// <summary>
    /// Departments authorised to action ALL job approval tasks.
    /// Restricted to: MGT (1001), ADM (1002), EST (1008).
    /// Customer/party approvals (dept 9999) are excluded.
    /// </summary>
    public static readonly int[] ApprovalDeptIds = [(int)DepartmentCode.MGT, (int)DepartmentCode.ADM, (int)DepartmentCode.EST];
}
