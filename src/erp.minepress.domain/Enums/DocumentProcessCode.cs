using System.ComponentModel;

namespace erp.minepress.domain.Enums;

/// <summary>
/// Process codes for document serial number generation via mst_document_sequence / fn_get_next_document_number.
/// Each value maps to the process_code column in press_db.mst_document_sequence.
/// </summary>
public enum DocumentProcessCode
{
    // ── Sales ──
    [Description("Customer Enquiry")]
    ENQUIRY,

    [Description("Quotation")]
    QUOTATION,

    [Description("Proforma Invoice")]
    PROFORMA_INVOICE,

    [Description("Sales Order")]
    SALES_ORDER,

    [Description("Delivery Challan")]
    DELIVERY_CHALLAN,

    [Description("Sales Invoice")]
    SALES_INVOICE,

    [Description("Sales Return")]
    SALES_RETURN,

    [Description("Customer Receipt")]
    RECEIPT,

    // ── Purchase ──
    [Description("Purchase Enquiry")]
    PURCHASE_ENQUIRY,

    [Description("Purchase Quotation")]
    PURCHASE_QUOTATION,

    [Description("Purchase Order")]
    PURCHASE_ORDER,

    [Description("Goods Receipt Note")]
    GOODS_RECEIPT,

    [Description("Purchase Invoice")]
    PURCHASE_INVOICE,

    [Description("Purchase Return")]
    PURCHASE_RETURN,

    [Description("Vendor Payment")]
    PAYMENT,

    // ── Job / Production ──
    [Description("Job Card")]
    JOB_CARD,

    [Description("Production Plan")]
    PRODUCTION_PLAN,

    [Description("Plate Making")]
    PLATE_MAKING,

    [Description("Printing Process")]
    PRINTING,

    [Description("Lamination")]
    LAMINATION,

    [Description("Cutting")]
    CUTTING,

    [Description("Binding")]
    BINDING,

    [Description("Packing")]
    PACKING,

    [Description("Quality Check")]
    QUALITY_CHECK,

    // ── Inventory / Store ──
    [Description("Stock Inward")]
    STOCK_IN,

    [Description("Stock Outward")]
    STOCK_OUT,

    [Description("Stock Transfer")]
    STOCK_TRANSFER,

    [Description("Stock Adjustment")]
    STOCK_ADJUSTMENT,

    [Description("Material Issue")]
    MATERIAL_ISSUE,

    [Description("Store Issue")]
    STORE_ISSUE,

    [Description("Store Receive")]
    STORE_RECEIVE,

    [Description("Purchase GRN")]
    PURCHASE_GRN,

    // ── Finance / Accounting ──
    [Description("Journal Voucher")]
    JOURNAL_VOUCHER,

    [Description("Credit Note")]
    CREDIT_NOTE,

    [Description("Debit Note")]
    DEBIT_NOTE,

    [Description("Employee Advance")]
    EMPLOYEE_ADVANCE,

    [Description("Expense Voucher")]
    EXPENSE_VOUCHER,

    [Description("Bank Receipt")]
    BANK_RECEIPT,

    [Description("Bank Payment")]
    BANK_PAYMENT,

    // ── Service / Maintenance ──
    [Description("Service Job")]
    SERVICE_JOB,

    [Description("Customer Complaint")]
    COMPLAINT,

    [Description("Machine Maintenance")]
    MAINTENANCE,

    // ── Gate Pass ──
    [Description("Gate Pass In")]
    GATE_PASS_IN,

    [Description("Gate Pass Out")]
    GATE_PASS_OUT,

    // ── Outsource ──
    [Description("Job Outsource Order")]
    JOB_OUTSOURCE,

    [Description("Outsource Dispatch")]
    OUTSOURCE_DISPATCH,

    [Description("Outsource Receive")]
    OUTSOURCE_RECEIVE,

    [Description("Outsource Bill")]
    OUTSOURCE_BILL
}
