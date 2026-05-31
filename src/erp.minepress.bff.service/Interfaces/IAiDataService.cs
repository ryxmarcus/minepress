namespace erp.minepress.bff.service.Interfaces;

/// <summary>
/// BFF data service that provides real database access for Agentic AI agents.
/// Covers ALL modules: Jobs, Machines, Billing, Delivery, Vendor, Reporting,
/// Customer/Party, Employee, HR, Enquiry, Quotation, Purchase, Accounting,
/// Store/Inventory, Challan, ProformaInvoice, Machine Breakdown, Printing Masters.
/// </summary>
public interface IAiDataService
{
    // ══════════════════════════ JOBS ══════════════════════════
    Task<AiJobDto?> GetJobByNoAsync(string jobNo, CancellationToken ct = default);
    Task<IReadOnlyList<AiJobDto>> GetJobsByStatusAsync(string? statusCode, int limit = 20, CancellationToken ct = default);
    Task<IReadOnlyList<AiJobDto>> SearchJobsAsync(string keyword, int limit = 20, CancellationToken ct = default);
    Task<AiJobDto?> CreateJobAsync(AiCreateJobRequest request, CancellationToken ct = default);
    Task<AiJobDto?> UpdateJobAsync(string jobNo, string? statusCode, int? quantity, string? priority, CancellationToken ct = default);

    // ══════════════════════════ MACHINES ══════════════════════════
    Task<IReadOnlyList<AiMachineDto>> GetAvailableMachinesAsync(string? machineType = null, CancellationToken ct = default);
    Task<AiMachineAllocationDto?> AllocateMachineAsync(string jobNo, long? machineId, string? processCode, CancellationToken ct = default);
    Task<IReadOnlyList<AiMachineAllocationDto>> GetMachineAllocationsForJobAsync(string jobNo, CancellationToken ct = default);

    // ══════════════════════════ BILLING ══════════════════════════
    Task<AiInvoiceDto?> GetInvoiceByJobNoAsync(string jobNo, CancellationToken ct = default);
    Task<IReadOnlyList<AiInvoiceDto>> GetRecentInvoicesAsync(int limit = 20, CancellationToken ct = default);
    Task<IReadOnlyList<AiCreditNoteDto>> GetCreditNotesAsync(string? status = null, int limit = 20, CancellationToken ct = default);
    Task<IReadOnlyList<AiDebitNoteDto>> GetDebitNotesAsync(string? status = null, int limit = 20, CancellationToken ct = default);

    // ══════════════════════════ DELIVERY / GATE PASS ══════════════════════════
    Task<AiGatePassDto?> CreateGatePassAsync(string jobNo, string? vehicleNo, string? driverName, string? driverContact, CancellationToken ct = default);
    Task<IReadOnlyList<AiGatePassDto>> GetGatePassesByJobNoAsync(string jobNo, CancellationToken ct = default);
    Task<IReadOnlyList<AiGatePassDto>> GetAllGatePassesAsync(string? status = null, int limit = 20, CancellationToken ct = default);

    // ══════════════════════════ VENDOR / OUTSOURCE ══════════════════════════
    Task<IReadOnlyList<AiVendorOutsourceDto>> GetOutsourcesByJobNoAsync(string jobNo, CancellationToken ct = default);
    Task<AiVendorOutsourceDto?> CreateVendorJobAsync(string jobNo, long vendorId, string? processType, decimal? quantity, CancellationToken ct = default);
    Task<IReadOnlyList<AiVendorDto>> GetAllVendorsAsync(int limit = 50, CancellationToken ct = default);
    Task<IReadOnlyList<AiVendorDto>> SearchVendorsAsync(string keyword, CancellationToken ct = default);

    // ══════════════════════════ REPORTING ══════════════════════════
    Task<AiReportSummaryDto> GetReportSummaryAsync(DateOnly? from, DateOnly? to, CancellationToken ct = default);

    // ══════════════════════════ CUSTOMER / PARTY ══════════════════════════
    Task<IReadOnlyList<AiCustomerDto>> GetAllCustomersAsync(int limit = 50, CancellationToken ct = default);
    Task<AiCustomerDto?> GetCustomerByIdAsync(int partyId, CancellationToken ct = default);
    Task<IReadOnlyList<AiCustomerDto>> SearchCustomersAsync(string keyword, CancellationToken ct = default);
    Task<IReadOnlyList<AiJobDto>> GetCustomerJobsAsync(int partyId, int limit = 20, CancellationToken ct = default);
    Task<IReadOnlyList<AiInvoiceDto>> GetCustomerInvoicesAsync(int partyId, int limit = 20, CancellationToken ct = default);
    Task<AiCustomerSummaryDto> GetCustomerSummaryAsync(int partyId, CancellationToken ct = default);

    // ══════════════════════════ EMPLOYEE ══════════════════════════
    Task<IReadOnlyList<AiEmployeeDto>> GetAllEmployeesAsync(int limit = 50, CancellationToken ct = default);
    Task<AiEmployeeDto?> GetEmployeeByIdAsync(long employeeId, CancellationToken ct = default);
    Task<IReadOnlyList<AiEmployeeDto>> SearchEmployeesAsync(string keyword, CancellationToken ct = default);
    Task<IReadOnlyList<AiEmployeeDto>> GetEmployeesByDepartmentAsync(string departmentName, CancellationToken ct = default);

    // ══════════════════════════ HR ══════════════════════════
    Task<IReadOnlyList<AiLeaveRequestDto>> GetLeaveRequestsAsync(string? status = null, int limit = 20, CancellationToken ct = default);
    Task<IReadOnlyList<AiLeaveRequestDto>> GetEmployeeLeaveRequestsAsync(long employeeId, CancellationToken ct = default);
    Task<IReadOnlyList<AiAttendanceDto>> GetAttendanceAsync(long? employeeId, DateOnly? date, int limit = 50, CancellationToken ct = default);
    Task<IReadOnlyList<AiLoanDto>> GetLoansAsync(string? status = null, int limit = 20, CancellationToken ct = default);
    Task<IReadOnlyList<AiOvertimeDto>> GetOvertimesAsync(long? employeeId, int limit = 20, CancellationToken ct = default);
    Task<IReadOnlyList<AiReimbursementDto>> GetReimbursementsAsync(string? status = null, int limit = 20, CancellationToken ct = default);
    Task<AiHrSummaryDto> GetHrSummaryAsync(CancellationToken ct = default);
    Task<IReadOnlyList<AiBonusDto>> GetBonusesAsync(string? status = null, int limit = 20, CancellationToken ct = default);
    Task<IReadOnlyList<AiIncentiveDto>> GetIncentivesAsync(string? status = null, int limit = 20, CancellationToken ct = default);
    Task<IReadOnlyList<AiHolidayDto>> GetHolidaysAsync(int? year = null, CancellationToken ct = default);
    Task<IReadOnlyList<AiMedicalClaimDto>> GetMedicalClaimsAsync(string? status = null, int limit = 20, CancellationToken ct = default);
    Task<IReadOnlyList<AiResignationDto>> GetResignationsAsync(string? status = null, int limit = 20, CancellationToken ct = default);
    Task<IReadOnlyList<AiSalaryAdvanceDto>> GetSalaryAdvancesAsync(string? status = null, int limit = 20, CancellationToken ct = default);
    Task<IReadOnlyList<AiTransferDto>> GetTransfersAsync(string? status = null, int limit = 20, CancellationToken ct = default);
    Task<IReadOnlyList<AiTravelExpenseDto>> GetTravelExpensesAsync(string? status = null, int limit = 20, CancellationToken ct = default);
    Task<IReadOnlyList<AiVacancyDto>> GetVacanciesAsync(string? status = null, int limit = 20, CancellationToken ct = default);

    // ══════════════════════════ ENQUIRY ══════════════════════════
    Task<IReadOnlyList<AiEnquiryDto>> GetAllEnquiriesAsync(string? status = null, int limit = 20, CancellationToken ct = default);
    Task<AiEnquiryDto?> GetEnquiryByIdAsync(long enquiryId, CancellationToken ct = default);
    Task<IReadOnlyList<AiEnquiryDto>> SearchEnquiriesAsync(string keyword, CancellationToken ct = default);

    // ══════════════════════════ QUOTATION ══════════════════════════
    Task<IReadOnlyList<AiQuotationDto>> GetAllQuotationsAsync(string? status = null, int limit = 20, CancellationToken ct = default);
    Task<AiQuotationDto?> GetQuotationByIdAsync(long quotationId, CancellationToken ct = default);
    Task<IReadOnlyList<AiQuotationDto>> SearchQuotationsAsync(string keyword, CancellationToken ct = default);

    // ══════════════════════════ PURCHASE ══════════════════════════
    Task<IReadOnlyList<AiPurchaseOrderDto>> GetAllPurchaseOrdersAsync(string? status = null, int limit = 20, CancellationToken ct = default);
    Task<AiPurchaseOrderDto?> GetPurchaseOrderByIdAsync(long poId, CancellationToken ct = default);
    Task<IReadOnlyList<AiPurchaseOrderDto>> SearchPurchaseOrdersAsync(string keyword, CancellationToken ct = default);
    Task<IReadOnlyList<AiGoodsReceiptDto>> GetGoodsReceiptsAsync(string? status = null, int limit = 20, CancellationToken ct = default);
    Task<IReadOnlyList<AiPurchaseInvoiceDto>> GetPurchaseInvoicesAsync(string? status = null, int limit = 20, CancellationToken ct = default);

    // ══════════════════════════ ACCOUNTING ══════════════════════════
    Task<IReadOnlyList<AiReceiptDto>> GetReceiptsAsync(DateOnly? from = null, DateOnly? to = null, int limit = 20, CancellationToken ct = default);
    Task<IReadOnlyList<AiPaymentDto>> GetPaymentsAsync(DateOnly? from = null, DateOnly? to = null, int limit = 20, CancellationToken ct = default);
    Task<IReadOnlyList<AiExpenseVoucherDto>> GetExpenseVouchersAsync(string? category = null, int limit = 20, CancellationToken ct = default);
    Task<AiOutstandingSummaryDto> GetOutstandingSummaryAsync(CancellationToken ct = default);

    // ══════════════════════════ STORE / INVENTORY ══════════════════════════
    Task<IReadOnlyList<AiStoreIssueDto>> GetStoreIssuesAsync(string? status = null, int limit = 20, CancellationToken ct = default);
    Task<IReadOnlyList<AiStoreReceiveDto>> GetStoreReceivesAsync(string? status = null, int limit = 20, CancellationToken ct = default);
    Task<IReadOnlyList<AiMaterialDto>> GetAllMaterialsAsync(string? category = null, CancellationToken ct = default);
    Task<IReadOnlyList<AiMaterialDto>> SearchMaterialsAsync(string keyword, CancellationToken ct = default);
    Task<IReadOnlyList<AiStockLedgerDto>> GetStockLedgerAsync(string? materialCode = null, int limit = 50, CancellationToken ct = default);
    Task<IReadOnlyList<AiChemicalDto>> GetAllChemicalsAsync(CancellationToken ct = default);

    // ══════════════════════════ CHALLAN ══════════════════════════
    Task<IReadOnlyList<AiChallanDto>> GetAllChallansAsync(string? status = null, int limit = 20, CancellationToken ct = default);
    Task<IReadOnlyList<AiChallanDto>> GetChallansByJobNoAsync(string jobNo, CancellationToken ct = default);

    // ══════════════════════════ PROFORMA INVOICE ══════════════════════════
    Task<IReadOnlyList<AiProformaInvoiceDto>> GetAllProformaInvoicesAsync(string? status = null, int limit = 20, CancellationToken ct = default);

    // ══════════════════════════ MACHINE BREAKDOWN ══════════════════════════
    Task<IReadOnlyList<AiMachineBreakdownDto>> GetMachineBreakdownsAsync(string? status = null, int limit = 20, CancellationToken ct = default);
    Task<IReadOnlyList<AiMachineBreakdownDto>> GetBreakdownsByMachineAsync(long machineId, CancellationToken ct = default);

    // ══════════════════════════ ACCOUNTING EXTENDED ══════════════════════════
    Task<IReadOnlyList<AiCreditNoteDto>> GetAccountingCreditNotesAsync(string? status = null, int limit = 20, CancellationToken ct = default);
    Task<IReadOnlyList<AiDebitNoteDto>> GetAccountingDebitNotesAsync(string? status = null, int limit = 20, CancellationToken ct = default);
    Task<IReadOnlyList<AiBankReceiptDto>> GetBankReceiptsAsync(DateOnly? from = null, DateOnly? to = null, int limit = 20, CancellationToken ct = default);
    Task<IReadOnlyList<AiBankPaymentDto>> GetBankPaymentsAsync(DateOnly? from = null, DateOnly? to = null, int limit = 20, CancellationToken ct = default);
    Task<IReadOnlyList<AiJournalVoucherDto>> GetJournalVouchersAsync(DateOnly? from = null, DateOnly? to = null, int limit = 20, CancellationToken ct = default);
    Task<IReadOnlyList<AiLedgerEntryDto>> GetLedgerEntriesAsync(string? accountHead = null, DateOnly? from = null, DateOnly? to = null, int limit = 50, CancellationToken ct = default);

    // ══════════════════════════ PRINTING MASTERS ══════════════════════════
    Task<IReadOnlyList<AiPaperDto>> GetAllPapersAsync(CancellationToken ct = default);
    Task<IReadOnlyList<AiInkDto>> GetAllInksAsync(CancellationToken ct = default);
    Task<IReadOnlyList<AiPlateDto>> GetAllPlatesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<AiBindingDto>> GetAllBindingsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<AiFinishingDto>> GetAllFinishingsAsync(CancellationToken ct = default);
}

// ══════════════════════════════════════════════════════════════════════
// DTOs — Job Module
// ══════════════════════════════════════════════════════════════════════

public record AiJobDto
{
    public long JobId { get; init; }
    public string JobNo { get; init; } = "";
    public DateOnly JobDate { get; init; }
    public string? CustomerName { get; init; }
    public string? ProductName { get; init; }
    public string? ProductDescription { get; init; }
    public int Quantity { get; init; }
    public string? JobType { get; init; }
    public string? StatusCode { get; init; }
    public string? CurrentStage { get; init; }
    public int? ProgressPercent { get; init; }
    public string? Priority { get; init; }
    public decimal? EstimatedCost { get; init; }
    public decimal? NetAmount { get; init; }
    public DateOnly? DeliveryDate { get; init; }
    public DateTime? CreatedOn { get; init; }
}

public record AiCreateJobRequest
{
    public string? CustomerName { get; init; }
    public string? ProductName { get; init; }
    public string? ProductDescription { get; init; }
    public int Quantity { get; init; }
    public string? JobType { get; init; }
    public string? Priority { get; init; }
    public string? PaperSize { get; init; }
    public string? ColorMode { get; init; }
    public long CreatedByUserId { get; init; }
    public int CompanyId { get; init; } = 1;
}

// ══════════════════════════════════════════════════════════════════════
// DTOs — Machine Module
// ══════════════════════════════════════════════════════════════════════

public record AiMachineDto
{
    public long MachineId { get; init; }
    public string MachineCode { get; init; } = "";
    public string MachineName { get; init; } = "";
    public string? MachineType { get; init; }
    public string? MachineCategory { get; init; }
    public int? MaxColors { get; init; }
    public int? MaxSpeed { get; init; }
    public decimal? HourlyRunningCost { get; init; }
    public bool? IsActive { get; init; }
    public int ActiveAllocations { get; init; }
}

public record AiMachineAllocationDto
{
    public long AllocationId { get; init; }
    public long JobId { get; init; }
    public string JobNo { get; init; } = "";
    public long MachineId { get; init; }
    public string? MachineCode { get; init; }
    public string? MachineName { get; init; }
    public string? ProcessCode { get; init; }
    public string? ProcessName { get; init; }
    public decimal? PlannedQuantity { get; init; }
    public decimal? CompletedQuantity { get; init; }
    public string? AllocationStatus { get; init; }
    public DateTime? PlannedStartTime { get; init; }
    public DateTime? PlannedEndTime { get; init; }
}

// ══════════════════════════════════════════════════════════════════════
// DTOs — Billing Module
// ══════════════════════════════════════════════════════════════════════

public record AiInvoiceDto
{
    public long SalesInvoiceId { get; init; }
    public string InvoiceNo { get; init; } = "";
    public DateOnly InvoiceDate { get; init; }
    public string? CustomerName { get; init; }
    public string? JobNo { get; init; }
    public decimal? SubtotalAmount { get; init; }
    public decimal? TotalTaxAmount { get; init; }
    public decimal? GrandTotal { get; init; }
    public decimal? PaidAmount { get; init; }
    public decimal? BalanceAmount { get; init; }
    public string Status { get; init; } = "";
}

// ══════════════════════════════════════════════════════════════════════
// DTOs — Delivery Module
// ══════════════════════════════════════════════════════════════════════

public record AiGatePassDto
{
    public long GatePassId { get; init; }
    public string GatePassNo { get; init; } = "";
    public DateOnly GatePassDate { get; init; }
    public string? GatepassType { get; init; }
    public string? ReferenceNo { get; init; }
    public string? VehicleNo { get; init; }
    public string? DriverName { get; init; }
    public string? DriverContact { get; init; }
    public string? Purpose { get; init; }
    public string? Status { get; init; }
    public decimal? TotalQuantity { get; init; }
}

// ══════════════════════════════════════════════════════════════════════
// DTOs — Vendor Module
// ══════════════════════════════════════════════════════════════════════

public record AiVendorOutsourceDto
{
    public long OutsourceId { get; init; }
    public string OutsourceNo { get; init; } = "";
    public DateOnly OutsourceDate { get; init; }
    public string? JobNo { get; init; }
    public long VendorId { get; init; }
    public string? VendorName { get; init; }
    public string? ProcessType { get; init; }
    public decimal? TotalQuantity { get; init; }
    public decimal? TotalAmount { get; init; }
    public string? Status { get; init; }
    public DateOnly? ExpectedDeliveryDate { get; init; }
}

// ══════════════════════════════════════════════════════════════════════
// DTOs — Reporting Module
// ══════════════════════════════════════════════════════════════════════

public record AiReportSummaryDto
{
    public int TotalJobs { get; init; }
    public int ActiveJobs { get; init; }
    public int CompletedJobs { get; init; }
    public int CancelledJobs { get; init; }
    public decimal TotalRevenue { get; init; }
    public decimal TotalOutstanding { get; init; }
    public int TotalInvoices { get; init; }
    public int TotalGatePasses { get; init; }
    public int TotalMachineAllocations { get; init; }
    public int TotalVendorJobs { get; init; }
    public DateOnly FromDate { get; init; }
    public DateOnly ToDate { get; init; }
    public IReadOnlyList<AiJobStatusCount> JobsByStatus { get; init; } = [];
}

public record AiJobStatusCount
{
    public string StatusCode { get; init; } = "";
    public int Count { get; init; }
}

// ══════════════════════════════════════════════════════════════════════
// DTOs — Customer / Party Module
// ══════════════════════════════════════════════════════════════════════

public record AiCustomerDto
{
    public int PartyId { get; init; }
    public string? Code { get; init; }
    public string? Name { get; init; }
    public string? GstNo { get; init; }
    public string? PanNo { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
    public bool IsActive { get; init; }
    public string? CustomerType { get; init; }
    public string? CustomerGroup { get; init; }
}

public record AiCustomerSummaryDto
{
    public int PartyId { get; init; }
    public string? CustomerName { get; init; }
    public int TotalJobs { get; init; }
    public int ActiveJobs { get; init; }
    public int CompletedJobs { get; init; }
    public int TotalInvoices { get; init; }
    public decimal TotalRevenue { get; init; }
    public decimal TotalOutstanding { get; init; }
    public int TotalEnquiries { get; init; }
    public int TotalQuotations { get; init; }
}

// ══════════════════════════════════════════════════════════════════════
// DTOs — Employee Module
// ══════════════════════════════════════════════════════════════════════

public record AiEmployeeDto
{
    public long EmployeeId { get; init; }
    public string EmpCode { get; init; } = "";
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? FullName { get; init; }
    public string? Department { get; init; }
    public string? Designation { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public DateOnly? DateOfJoining { get; init; }
    public bool IsActive { get; init; }
}

// ══════════════════════════════════════════════════════════════════════
// DTOs — HR Module
// ══════════════════════════════════════════════════════════════════════

public record AiLeaveRequestDto
{
    public long LeaveId { get; init; }
    public string LeaveNo { get; init; } = "";
    public long EmployeeId { get; init; }
    public string? EmployeeName { get; init; }
    public string? LeaveType { get; init; }
    public DateOnly FromDate { get; init; }
    public DateOnly ToDate { get; init; }
    public decimal TotalDays { get; init; }
    public string? Reason { get; init; }
    public string Status { get; init; } = "";
}

public record AiAttendanceDto
{
    public long AttendanceId { get; init; }
    public long EmployeeId { get; init; }
    public string? EmployeeName { get; init; }
    public DateOnly AttendanceDate { get; init; }
    public string? Status { get; init; }
    public TimeOnly? CheckIn { get; init; }
    public TimeOnly? CheckOut { get; init; }
    public decimal? TotalHours { get; init; }
}

public record AiLoanDto
{
    public long LoanId { get; init; }
    public string LoanNo { get; init; } = "";
    public long EmployeeId { get; init; }
    public string? EmployeeName { get; init; }
    public string? LoanType { get; init; }
    public decimal LoanAmount { get; init; }
    public decimal? PaidAmount { get; init; }
    public decimal? BalanceAmount { get; init; }
    public string? Status { get; init; }
}

public record AiOvertimeDto
{
    public long OvertimeId { get; init; }
    public long EmployeeId { get; init; }
    public string? EmployeeName { get; init; }
    public DateOnly OvertimeDate { get; init; }
    public decimal? Hours { get; init; }
    public string? Reason { get; init; }
    public string? Status { get; init; }
}

public record AiReimbursementDto
{
    public long ReimbursementId { get; init; }
    public long EmployeeId { get; init; }
    public string? EmployeeName { get; init; }
    public string? Category { get; init; }
    public decimal Amount { get; init; }
    public string? Description { get; init; }
    public string? Status { get; init; }
}

public record AiHrSummaryDto
{
    public int TotalEmployees { get; init; }
    public int ActiveEmployees { get; init; }
    public int PendingLeaves { get; init; }
    public int ApprovedLeavesToday { get; init; }
    public int ActiveLoans { get; init; }
    public int PendingReimbursements { get; init; }
    public decimal TotalPendingReimbursementAmount { get; init; }
}

// ══════════════════════════════════════════════════════════════════════
// DTOs — Enquiry Module
// ══════════════════════════════════════════════════════════════════════

public record AiEnquiryDto
{
    public long EnquiryId { get; init; }
    public string EnquiryNo { get; init; } = "";
    public DateOnly EnquiryDate { get; init; }
    public string? CustomerName { get; init; }
    public string? ContactPerson { get; init; }
    public string? ContactMobile { get; init; }
    public string? EnquirySource { get; init; }
    public string? Priority { get; init; }
    public string? Status { get; init; }
    public DateOnly? ExpectedDeliveryDate { get; init; }
    public string? Remarks { get; init; }
    public int ItemCount { get; init; }
}

// ══════════════════════════════════════════════════════════════════════
// DTOs — Quotation Module
// ══════════════════════════════════════════════════════════════════════

public record AiQuotationDto
{
    public long QuotationId { get; init; }
    public string QuotationNo { get; init; } = "";
    public DateOnly QuotationDate { get; init; }
    public string? CustomerName { get; init; }
    public long? EnquiryId { get; init; }
    public string? EnquiryNo { get; init; }
    public decimal? TotalAmount { get; init; }
    public decimal? NetAmount { get; init; }
    public DateOnly? ValidTill { get; init; }
    public string? Status { get; init; }
    public int ItemCount { get; init; }
}

// ══════════════════════════════════════════════════════════════════════
// DTOs — Purchase Module
// ══════════════════════════════════════════════════════════════════════

public record AiPurchaseOrderDto
{
    public long PurchaseOrderId { get; init; }
    public string PoNo { get; init; } = "";
    public DateOnly PoDate { get; init; }
    public string? SupplierName { get; init; }
    public decimal? GrandTotal { get; init; }
    public string Status { get; init; } = "";
    public bool? IsApproved { get; init; }
    public DateOnly? ExpectedDeliveryDate { get; init; }
}

public record AiGoodsReceiptDto
{
    public long GrnId { get; init; }
    public string GrnNo { get; init; } = "";
    public DateOnly GrnDate { get; init; }
    public string? SupplierName { get; init; }
    public string? PoNo { get; init; }
    public decimal? TotalQuantity { get; init; }
    public decimal? TotalAcceptedQty { get; init; }
    public decimal? TotalRejectedQty { get; init; }
    public string Status { get; init; } = "";
    public bool? IsQualityChecked { get; init; }
}

public record AiPurchaseInvoiceDto
{
    public long PurchaseInvoiceId { get; init; }
    public string InvoiceNo { get; init; } = "";
    public DateOnly InvoiceDate { get; init; }
    public string? SupplierName { get; init; }
    public decimal? GrandTotal { get; init; }
    public decimal? PaidAmount { get; init; }
    public decimal? BalanceAmount { get; init; }
    public string? Status { get; init; }
}

// ══════════════════════════════════════════════════════════════════════
// DTOs — Accounting Module
// ══════════════════════════════════════════════════════════════════════

public record AiReceiptDto
{
    public long ReceiptId { get; init; }
    public string ReceiptNo { get; init; } = "";
    public DateOnly ReceiptDate { get; init; }
    public string? PartyName { get; init; }
    public string PaymentMode { get; init; } = "";
    public decimal Amount { get; init; }
    public string? Status { get; init; }
}

public record AiPaymentDto
{
    public long PaymentId { get; init; }
    public string PaymentNo { get; init; } = "";
    public DateOnly PaymentDate { get; init; }
    public string? PartyName { get; init; }
    public string PaymentMode { get; init; } = "";
    public decimal Amount { get; init; }
    public string? Status { get; init; }
}

public record AiExpenseVoucherDto
{
    public long ExpenseVoucherId { get; init; }
    public string VoucherNo { get; init; } = "";
    public DateOnly VoucherDate { get; init; }
    public string? ExpenseCategory { get; init; }
    public string? PartyName { get; init; }
    public decimal? GrandTotal { get; init; }
    public string? Status { get; init; }
}

public record AiOutstandingSummaryDto
{
    public decimal TotalReceivable { get; init; }
    public decimal TotalPayable { get; init; }
    public int ReceivableCount { get; init; }
    public int PayableCount { get; init; }
    public IReadOnlyList<AiOutstandingPartyDto> TopReceivables { get; init; } = [];
    public IReadOnlyList<AiOutstandingPartyDto> TopPayables { get; init; } = [];
}

public record AiOutstandingPartyDto
{
    public int PartyId { get; init; }
    public string? PartyName { get; init; }
    public decimal Amount { get; init; }
}

// ══════════════════════════════════════════════════════════════════════
// DTOs — Store / Inventory Module
// ══════════════════════════════════════════════════════════════════════

public record AiStoreIssueDto
{
    public long IssueId { get; init; }
    public string IssueNo { get; init; } = "";
    public DateOnly IssueDate { get; init; }
    public string IssueType { get; init; } = "";
    public string? JobNo { get; init; }
    public int? TotalItems { get; init; }
    public decimal? TotalAmount { get; init; }
    public string Status { get; init; } = "";
}

public record AiStoreReceiveDto
{
    public long ReceiveId { get; init; }
    public string ReceiveNo { get; init; } = "";
    public DateOnly ReceiveDate { get; init; }
    public string ReceiveType { get; init; } = "";
    public string? GrnNo { get; init; }
    public string? SupplierName { get; init; }
    public int? TotalItems { get; init; }
    public decimal? TotalAmount { get; init; }
    public string Status { get; init; } = "";
}

public record AiMaterialDto
{
    public string MaterialCode { get; init; } = "";
    public string MaterialName { get; init; } = "";
    public string? MaterialCategory { get; init; }
    public string? UnitOfMeasure { get; init; }
    public decimal? RatePerUnit { get; init; }
    public decimal? ReorderLevel { get; init; }
    public bool IsActive { get; init; }
}

// ══════════════════════════════════════════════════════════════════════
// DTOs — Challan Module
// ══════════════════════════════════════════════════════════════════════

public record AiChallanDto
{
    public long ChallanId { get; init; }
    public string ChallanNo { get; init; } = "";
    public DateOnly ChallanDate { get; init; }
    public string? CustomerName { get; init; }
    public string? VehicleNo { get; init; }
    public decimal? TotalQty { get; init; }
    public decimal? TotalAmount { get; init; }
    public string? Status { get; init; }
}

// ══════════════════════════════════════════════════════════════════════
// DTOs — Proforma Invoice Module
// ══════════════════════════════════════════════════════════════════════

public record AiProformaInvoiceDto
{
    public long ProformaInvoiceId { get; init; }
    public string ProformaNo { get; init; } = "";
    public DateOnly ProformaDate { get; init; }
    public string? CustomerName { get; init; }
    public decimal? SubtotalAmount { get; init; }
    public decimal? GrandTotal { get; init; }
    public DateOnly? ValidTill { get; init; }
    public string? Status { get; init; }
}

// ══════════════════════════════════════════════════════════════════════
// DTOs — Machine Breakdown Module
// ══════════════════════════════════════════════════════════════════════

public record AiMachineBreakdownDto
{
    public long BreakdownId { get; init; }
    public long MachineId { get; init; }
    public string? MachineName { get; init; }
    public string? FaultCode { get; init; }
    public string? FaultDescription { get; init; }
    public string? FaultCategory { get; init; }
    public string? SeverityLevel { get; init; }
    public DateTime BreakdownStartTime { get; init; }
    public DateTime? BreakdownEndTime { get; init; }
    public decimal? DowntimeMinutes { get; init; }
    public string? BreakdownStatus { get; init; }
}

// ══════════════════════════════════════════════════════════════════════
// DTOs — Printing Masters Module
// ══════════════════════════════════════════════════════════════════════

public record AiPaperDto
{
    public int PaperId { get; init; }
    public string? PaperName { get; init; }
    public string? PaperType { get; init; }
    public decimal? Gsm { get; init; }
    public string? Size { get; init; }
    public decimal? RatePerKg { get; init; }
    public bool IsActive { get; init; }
}

public record AiInkDto
{
    public int InkId { get; init; }
    public string? InkName { get; init; }
    public string? InkType { get; init; }
    public string? Color { get; init; }
    public decimal? RatePerKg { get; init; }
    public bool IsActive { get; init; }
}

public record AiPlateDto
{
    public int PlateId { get; init; }
    public string? PlateName { get; init; }
    public string? PlateType { get; init; }
    public string? Size { get; init; }
    public decimal? Rate { get; init; }
    public bool IsActive { get; init; }
}

public record AiBindingDto
{
    public int BindingId { get; init; }
    public string? BindingName { get; init; }
    public string? BindingType { get; init; }
    public decimal? RatePerUnit { get; init; }
    public bool IsActive { get; init; }
}

public record AiFinishingDto
{
    public int FinishingId { get; init; }
    public string? FinishingName { get; init; }
    public string? FinishingType { get; init; }
    public decimal? RatePerUnit { get; init; }
    public bool IsActive { get; init; }
}

// ══════════════════════════════════════════════════════════════════════
// DTOs — Vendor Module
// ══════════════════════════════════════════════════════════════════════

public record AiVendorDto
{
    public long VendorId { get; init; }
    public string? VendorCode { get; init; }
    public string? VendorName { get; init; }
    public string? VendorType { get; init; }
    public string? ContactPerson { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public string? City { get; init; }
    public string? GstNo { get; init; }
    public bool IsActive { get; init; }
}

// ══════════════════════════════════════════════════════════════════════
// DTOs — Billing Extended (Credit/Debit Notes)
// ══════════════════════════════════════════════════════════════════════

public record AiCreditNoteDto
{
    public long CreditNoteId { get; init; }
    public string CreditNoteNo { get; init; } = "";
    public DateOnly CreditNoteDate { get; init; }
    public string? CustomerName { get; init; }
    public string? InvoiceNo { get; init; }
    public string? Reason { get; init; }
    public decimal? GrandTotal { get; init; }
    public string? Status { get; init; }
}

public record AiDebitNoteDto
{
    public long DebitNoteId { get; init; }
    public string DebitNoteNo { get; init; } = "";
    public DateOnly DebitNoteDate { get; init; }
    public string? SupplierName { get; init; }
    public string? InvoiceNo { get; init; }
    public string? Reason { get; init; }
    public decimal? GrandTotal { get; init; }
    public string? Status { get; init; }
}

// ══════════════════════════════════════════════════════════════════════
// DTOs — HR Extended
// ══════════════════════════════════════════════════════════════════════

public record AiBonusDto
{
    public long BonusId { get; init; }
    public string BonusNo { get; init; } = "";
    public DateOnly BonusDate { get; init; }
    public long EmployeeId { get; init; }
    public string? EmployeeName { get; init; }
    public string? BonusType { get; init; }
    public decimal BonusAmount { get; init; }
    public string? Status { get; init; }
}

public record AiIncentiveDto
{
    public long IncentiveId { get; init; }
    public string IncentiveNo { get; init; } = "";
    public DateOnly IncentiveDate { get; init; }
    public long EmployeeId { get; init; }
    public string? EmployeeName { get; init; }
    public string? IncentiveType { get; init; }
    public decimal Amount { get; init; }
    public string? Status { get; init; }
}

public record AiHolidayDto
{
    public long HolidayId { get; init; }
    public string? HolidayName { get; init; }
    public DateOnly HolidayDate { get; init; }
    public string? HolidayType { get; init; }
    public bool IsOptional { get; init; }
}

public record AiMedicalClaimDto
{
    public long ClaimId { get; init; }
    public string ClaimNo { get; init; } = "";
    public long EmployeeId { get; init; }
    public string? EmployeeName { get; init; }
    public decimal ClaimAmount { get; init; }
    public decimal? ApprovedAmount { get; init; }
    public string? Description { get; init; }
    public string? Status { get; init; }
}

public record AiResignationDto
{
    public long ResignationId { get; init; }
    public long EmployeeId { get; init; }
    public string? EmployeeName { get; init; }
    public DateOnly ResignationDate { get; init; }
    public DateOnly? LastWorkingDate { get; init; }
    public string? Reason { get; init; }
    public string? Status { get; init; }
}

public record AiSalaryAdvanceDto
{
    public long AdvanceId { get; init; }
    public string AdvanceNo { get; init; } = "";
    public long EmployeeId { get; init; }
    public string? EmployeeName { get; init; }
    public decimal Amount { get; init; }
    public string? Reason { get; init; }
    public string? Status { get; init; }
}

public record AiTransferDto
{
    public long TransferId { get; init; }
    public long EmployeeId { get; init; }
    public string? EmployeeName { get; init; }
    public string? FromDepartment { get; init; }
    public string? ToDepartment { get; init; }
    public string? FromDesignation { get; init; }
    public string? ToDesignation { get; init; }
    public DateOnly TransferDate { get; init; }
    public string? Status { get; init; }
}

public record AiTravelExpenseDto
{
    public long TravelExpenseId { get; init; }
    public long EmployeeId { get; init; }
    public string? EmployeeName { get; init; }
    public string? TravelPurpose { get; init; }
    public DateOnly? FromDate { get; init; }
    public DateOnly? ToDate { get; init; }
    public decimal? ClaimAmount { get; init; }
    public decimal? ApprovedAmount { get; init; }
    public string? Status { get; init; }
}

public record AiVacancyDto
{
    public long VacancyId { get; init; }
    public string? VacancyTitle { get; init; }
    public string? Department { get; init; }
    public string? Designation { get; init; }
    public int Positions { get; init; }
    public DateOnly? PostedDate { get; init; }
    public DateOnly? ClosingDate { get; init; }
    public string? Status { get; init; }
}

// ══════════════════════════════════════════════════════════════════════
// DTOs — Store Extended
// ══════════════════════════════════════════════════════════════════════

public record AiStockLedgerDto
{
    public long StockLedgerId { get; init; }
    public string? MaterialCode { get; init; }
    public string? MaterialName { get; init; }
    public string? TransactionType { get; init; }
    public decimal? Quantity { get; init; }
    public decimal? Rate { get; init; }
    public decimal? Amount { get; init; }
    public DateOnly TransactionDate { get; init; }
    public string? ReferenceNo { get; init; }
}

public record AiChemicalDto
{
    public int ChemicalId { get; init; }
    public string? ChemicalName { get; init; }
    public string? ChemicalType { get; init; }
    public decimal? RatePerUnit { get; init; }
    public string? UnitOfMeasure { get; init; }
    public bool IsActive { get; init; }
}

// ══════════════════════════════════════════════════════════════════════
// DTOs — Accounting Extended
// ══════════════════════════════════════════════════════════════════════

public record AiBankReceiptDto
{
    public long BankReceiptId { get; init; }
    public string ReceiptNo { get; init; } = "";
    public DateOnly ReceiptDate { get; init; }
    public string? PartyName { get; init; }
    public string? BankName { get; init; }
    public string? PaymentMode { get; init; }
    public decimal Amount { get; init; }
    public string? Status { get; init; }
}

public record AiBankPaymentDto
{
    public long BankPaymentId { get; init; }
    public string PaymentNo { get; init; } = "";
    public DateOnly PaymentDate { get; init; }
    public string? PartyName { get; init; }
    public string? BankName { get; init; }
    public string? PaymentMode { get; init; }
    public decimal Amount { get; init; }
    public string? Status { get; init; }
}

public record AiJournalVoucherDto
{
    public long JournalVoucherId { get; init; }
    public string VoucherNo { get; init; } = "";
    public DateOnly VoucherDate { get; init; }
    public string? Narration { get; init; }
    public decimal TotalDebit { get; init; }
    public decimal TotalCredit { get; init; }
    public string? Status { get; init; }
}

public record AiLedgerEntryDto
{
    public long LedgerId { get; init; }
    public DateOnly TransactionDate { get; init; }
    public string? AccountHead { get; init; }
    public string? Narration { get; init; }
    public decimal DebitAmount { get; init; }
    public decimal CreditAmount { get; init; }
    public decimal RunningBalance { get; init; }
    public string? ReferenceNo { get; init; }
    public string? TransactionType { get; init; }
}
