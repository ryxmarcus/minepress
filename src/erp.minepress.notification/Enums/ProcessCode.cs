using System.ComponentModel;

namespace erp.minepress.notification.Enums;

/// <summary>
/// Process codes matching mst_process.processcode.
/// </summary>
public enum ProcessCode
{
    [Description("Enquiry & Job Order Creation")]
    EnqJob,

    [Description("Pre-Design / Content Preparation")]
    PreDes,

    [Description("Designing / DTP / Layout")]
    DesDtp,

    [Description("Client Approval (Proofing)")]
    Proof,

    [Description("Pre-Press / Plate Making")]
    PrePress,

    [Description("Material Planning & Procurement")]
    Proc,

    [Description("Paper Cutting / Sheet Preparation")]
    Cut,

    [Description("Printing")]
    Print,

    [Description("Drying / Curing")]
    Dry,

    [Description("Post-Press / Finishing")]
    PostPress,

    [Description("Folding / Gathering / Collation")]
    Fold,

    [Description("Binding")]
    Bind,

    [Description("Final Trimming / Cutting")]
    Trim,

    [Description("Final Quality Check")]
    Qc,

    [Description("Packing")]
    Pack,

    [Description("Challan Generation")]
    Challan,

    [Description("Gate Pass")]
    GatePass,

    [Description("Dispatch / Transportation")]
    Dispatch,

    [Description("Billing & Payment")]
    Bill,

    [Description("Job Closure")]
    JobClose,

    [Description("Accounts Receivable")]
    AccRecv,

    [Description("Accounts Payable")]
    AccPay,

    [Description("Banking & Reconciliation")]
    Banking,

    [Description("General Ledger & Journal")]
    GLJournal,

    [Description("Purchase Order & GRN")]
    Purchase
}
