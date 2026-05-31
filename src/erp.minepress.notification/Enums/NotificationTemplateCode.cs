using System.ComponentModel;

namespace erp.minepress.notification.Enums;

/// <summary>
/// Template codes matching mst_notification_template.template_code.
/// </summary>
public enum NotificationTemplateCode
{
    [Description("Rate Approval Request")]
    RateApprovalReq,

    [Description("Rate Approved")]
    RateApproved,

    [Description("Quotation Sent to Customer")]
    QuotSent,

    [Description("Quotation Sent WhatsApp")]
    QuotSentWa,

    [Description("Job Created")]
    JobCreated,

    [Description("Job Card Assigned")]
    JobAssigned,

    [Description("Job Completed")]
    JobCompleted,

    [Description("Task Assigned")]
    TaskAssigned,

    [Description("Approval Pending")]
    ApprovalPending,

    [Description("System Error Alert")]
    ErrorAlert,

    [Description("Overdue Task Alert")]
    OverdueTask,

    [Description("Sales Invoice Created")]
    SalesInvoiceCreated,

    [Description("Sales Invoice Sent to Customer")]
    SalesInvoiceSent,

    [Description("Purchase Invoice Received")]
    PurchaseInvoiceReceived,

    [Description("Payment Receipt Confirmation")]
    ReceiptConfirmation,

    [Description("Payment Made to Supplier")]
    PaymentMade,

    [Description("Credit Note Issued")]
    CreditNoteIssued,

    [Description("Debit Note Issued")]
    DebitNoteIssued,

    [Description("Invoice Overdue Reminder")]
    InvoiceOverdue,

    [Description("Proforma Invoice Sent")]
    ProformaInvoiceSent,

    [Description("Purchase Order Created")]
    PurchaseOrderCreated,

    [Description("Expense Voucher Approval")]
    ExpenseApproval
}
