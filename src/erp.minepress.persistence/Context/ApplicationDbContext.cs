using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using erp.minepress.persistence.Models;

namespace erp.minepress.persistence.Context;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ErrorLog> ErrorLogs { get; set; }

    public virtual DbSet<HrBonu> HrBonus { get; set; }

    public virtual DbSet<HrHoliday> HrHolidays { get; set; }

    public virtual DbSet<HrIncentive> HrIncentives { get; set; }

    public virtual DbSet<HrLeaveBalance> HrLeaveBalances { get; set; }

    public virtual DbSet<HrLeaveRequest> HrLeaveRequests { get; set; }

    public virtual DbSet<HrLeaveType> HrLeaveTypes { get; set; }

    public virtual DbSet<HrLoan> HrLoans { get; set; }

    public virtual DbSet<HrLoanRepayment> HrLoanRepayments { get; set; }

    public virtual DbSet<HrMedicalClaim> HrMedicalClaims { get; set; }

    public virtual DbSet<HrOvertime> HrOvertimes { get; set; }

    public virtual DbSet<HrReimbursement> HrReimbursements { get; set; }

    public virtual DbSet<HrResignation> HrResignations { get; set; }

    public virtual DbSet<HrSalaryAdvance> HrSalaryAdvances { get; set; }

    public virtual DbSet<HrShiftRoster> HrShiftRosters { get; set; }

    public virtual DbSet<HrTransfer> HrTransfers { get; set; }

    public virtual DbSet<HrTravelExpense> HrTravelExpenses { get; set; }

    public virtual DbSet<HrUniformAllotment> HrUniformAllotments { get; set; }

    public virtual DbSet<HrVacancy> HrVacancies { get; set; }

    public virtual DbSet<HybEmployeeAttendance> HybEmployeeAttendances { get; set; }

    public virtual DbSet<HybJobRateCalculator> HybJobRateCalculators { get; set; }

    public virtual DbSet<MapModuleDepartment> MapModuleDepartments { get; set; }

    public virtual DbSet<MapUserPermission> MapUserPermissions { get; set; }

    public virtual DbSet<MapUserRole> MapUserRoles { get; set; }

    public virtual DbSet<MstAccountHead> MstAccountHeads { get; set; }

    public virtual DbSet<MstApprovalLevel> MstApprovalLevels { get; set; }

    public virtual DbSet<MstApprovalType> MstApprovalTypes { get; set; }

    public virtual DbSet<MstBankAccount> MstBankAccounts { get; set; }

    public virtual DbSet<MstBinding> MstBindings { get; set; }

    public virtual DbSet<MstBrand> MstBrands { get; set; }

    public virtual DbSet<MstChemical> MstChemicals { get; set; }

    public virtual DbSet<MstCity> MstCities { get; set; }

    public virtual DbSet<MstCompany> MstCompanies { get; set; }

    public virtual DbSet<MstCostCenter> MstCostCenters { get; set; }

    public virtual DbSet<MstCostComponent> MstCostComponents { get; set; }

    public virtual DbSet<MstCountry> MstCountries { get; set; }

    public virtual DbSet<MstCurrency> MstCurrencies { get; set; }

    public virtual DbSet<MstCustomer> MstCustomers { get; set; }

    public virtual DbSet<MstCustomerGroup> MstCustomerGroups { get; set; }

    public virtual DbSet<MstCustomerPaymentTerm> MstCustomerPaymentTerms { get; set; }

    public virtual DbSet<MstCustomerType> MstCustomerTypes { get; set; }

    public virtual DbSet<MstDepartment> MstDepartments { get; set; }

    public virtual DbSet<MstDepartmentRoleMap> MstDepartmentRoleMaps { get; set; }

    public virtual DbSet<MstDesignation> MstDesignations { get; set; }

    public virtual DbSet<MstDesigning> MstDesignings { get; set; }

    public virtual DbSet<MstDirection> MstDirections { get; set; }

    public virtual DbSet<MstDocumentSequence> MstDocumentSequences { get; set; }

    public virtual DbSet<MstEmployee> MstEmployees { get; set; }

    public virtual DbSet<MstEmployeeMachineMapping> MstEmployeeMachineMappings { get; set; }

    public virtual DbSet<MstEmployeeType> MstEmployeeTypes { get; set; }

    public virtual DbSet<MstExecutionType> MstExecutionTypes { get; set; }

    public virtual DbSet<MstExpenseCategory> MstExpenseCategories { get; set; }

    public virtual DbSet<MstFinancialYear> MstFinancialYears { get; set; }

    public virtual DbSet<MstFinishing> MstFinishings { get; set; }

    public virtual DbSet<MstHsnSacCode> MstHsnSacCodes { get; set; }

    public virtual DbSet<MstInk> MstInks { get; set; }

    public virtual DbSet<MstJobCategory> MstJobCategories { get; set; }

    public virtual DbSet<MstJobType> MstJobTypes { get; set; }

    public virtual DbSet<MstLocation> MstLocations { get; set; }

    public virtual DbSet<MstLocationType> MstLocationTypes { get; set; }

    public virtual DbSet<MstMachine> MstMachines { get; set; }

    public virtual DbSet<MstMachineMaintenance> MstMachineMaintenances { get; set; }

    public virtual DbSet<MstMachineSelectionRule> MstMachineSelectionRules { get; set; }

    public virtual DbSet<MstMaterial> MstMaterials { get; set; }

    public virtual DbSet<MstMenu> MstMenus { get; set; }

    public virtual DbSet<MstModule> MstModules { get; set; }

    public virtual DbSet<MstNotificationPreference> MstNotificationPreferences { get; set; }

    public virtual DbSet<MstNotificationProvider> MstNotificationProviders { get; set; }

    public virtual DbSet<MstNotificationTemplate> MstNotificationTemplates { get; set; }

    public virtual DbSet<MstOtherItem> MstOtherItems { get; set; }

    public virtual DbSet<MstPaper> MstPapers { get; set; }

    public virtual DbSet<MstPaperSize> MstPaperSizes { get; set; }

    public virtual DbSet<MstParty> MstParties { get; set; }

    public virtual DbSet<MstPartyAddress> MstPartyAddresses { get; set; }

    public virtual DbSet<MstPartyBank> MstPartyBanks { get; set; }

    public virtual DbSet<MstPartyContact> MstPartyContacts { get; set; }

    public virtual DbSet<MstPartyRole> MstPartyRoles { get; set; }

    public virtual DbSet<MstPartyTax> MstPartyTaxes { get; set; }

    public virtual DbSet<MstPaymentTerm> MstPaymentTerms { get; set; }

    public virtual DbSet<MstPermission> MstPermissions { get; set; }

    public virtual DbSet<MstPlate> MstPlates { get; set; }

    public virtual DbSet<MstPrintProcess> MstPrintProcesses { get; set; }

    public virtual DbSet<MstPrintProductSize> MstPrintProductSizes { get; set; }

    public virtual DbSet<MstPrintProductType> MstPrintProductTypes { get; set; }

    public virtual DbSet<MstProcess> MstProcesses { get; set; }

    public virtual DbSet<MstProcessDepartmentMap> MstProcessDepartmentMaps { get; set; }

    public virtual DbSet<MstProcessNotificationConfig> MstProcessNotificationConfigs { get; set; }

    public virtual DbSet<MstProcessRoleMap> MstProcessRoleMaps { get; set; }

    public virtual DbSet<MstProcessStage> MstProcessStages { get; set; }

    public virtual DbSet<MstProductPart> MstProductParts { get; set; }

    public virtual DbSet<MstRole> MstRoles { get; set; }

    public virtual DbSet<MstRoleType> MstRoleTypes { get; set; }

    public virtual DbSet<MstShiftType> MstShiftTypes { get; set; }

    public virtual DbSet<MstState> MstStates { get; set; }

    public virtual DbSet<MstStatus> MstStatuses { get; set; }

    public virtual DbSet<MstSupplier> MstSuppliers { get; set; }

    public virtual DbSet<MstSupplierType> MstSupplierTypes { get; set; }

    public virtual DbSet<MstTaxCategory> MstTaxCategories { get; set; }

    public virtual DbSet<MstTaxCategoryComponent> MstTaxCategoryComponents { get; set; }

    public virtual DbSet<MstTaxComponent> MstTaxComponents { get; set; }

    public virtual DbSet<MstTaxRate> MstTaxRates { get; set; }

    public virtual DbSet<MstTaxRegion> MstTaxRegions { get; set; }

    public virtual DbSet<MstTaxType> MstTaxTypes { get; set; }

    public virtual DbSet<MstTransactionType> MstTransactionTypes { get; set; }

    public virtual DbSet<MstUom> MstUoms { get; set; }

    public virtual DbSet<MstUomType> MstUomTypes { get; set; }

    public virtual DbSet<MstUser> MstUsers { get; set; }

    public virtual DbSet<MstUserRole> MstUserRoles { get; set; }

    public virtual DbSet<MstVendor> MstVendors { get; set; }

    public virtual DbSet<MstVendorType> MstVendorTypes { get; set; }

    public virtual DbSet<MstVoucherType> MstVoucherTypes { get; set; }

    public virtual DbSet<MstWorkflowConnection> MstWorkflowConnections { get; set; }

    public virtual DbSet<MstWorkflowStep> MstWorkflowSteps { get; set; }

    public virtual DbSet<MstWorkflowTemplate> MstWorkflowTemplates { get; set; }

    public virtual DbSet<MstWorkspaceConfig> MstWorkspaceConfigs { get; set; }

    public virtual DbSet<PartyActivityLog> PartyActivityLogs { get; set; }

    public virtual DbSet<RptQueryPlan> RptQueryPlans { get; set; }

    public virtual DbSet<RptSavedReport> RptSavedReports { get; set; }

    public virtual DbSet<RptSavedReportColumn> RptSavedReportColumns { get; set; }

    public virtual DbSet<RptSavedReportFilter> RptSavedReportFilters { get; set; }

    public virtual DbSet<SysErrorLog> SysErrorLogs { get; set; }

    public virtual DbSet<TrnAccountLedger> TrnAccountLedgers { get; set; }

    public virtual DbSet<TrnAdvanceLedger> TrnAdvanceLedgers { get; set; }

    public virtual DbSet<TrnAiAgentActivity> TrnAiAgentActivities { get; set; }

    public virtual DbSet<TrnAiNotificationLog> TrnAiNotificationLogs { get; set; }

    public virtual DbSet<TrnApOutstanding> TrnApOutstandings { get; set; }

    public virtual DbSet<TrnArOutstanding> TrnArOutstandings { get; set; }

    public virtual DbSet<TrnBankPayment> TrnBankPayments { get; set; }

    public virtual DbSet<TrnBankPaymentAllocation> TrnBankPaymentAllocations { get; set; }

    public virtual DbSet<TrnBankReceipt> TrnBankReceipts { get; set; }

    public virtual DbSet<TrnBankReceiptAllocation> TrnBankReceiptAllocations { get; set; }

    public virtual DbSet<TrnBankReconciliation> TrnBankReconciliations { get; set; }

    public virtual DbSet<TrnBankReconciliationItem> TrnBankReconciliationItems { get; set; }

    public virtual DbSet<TrnChallan> TrnChallans { get; set; }

    public virtual DbSet<TrnChallanItem> TrnChallanItems { get; set; }

    public virtual DbSet<TrnChallanTimeline> TrnChallanTimelines { get; set; }

    public virtual DbSet<TrnContraVoucher> TrnContraVouchers { get; set; }

    public virtual DbSet<TrnCreditNote> TrnCreditNotes { get; set; }

    public virtual DbSet<TrnCreditNoteItem> TrnCreditNoteItems { get; set; }

    public virtual DbSet<TrnDebitNote> TrnDebitNotes { get; set; }

    public virtual DbSet<TrnDebitNoteItem> TrnDebitNoteItems { get; set; }

    public virtual DbSet<TrnDesignWorkEntry> TrnDesignWorkEntries { get; set; }

    public virtual DbSet<TrnPlateMakingEntry> TrnPlateMakingEntries { get; set; }

    public virtual DbSet<TrnEnquiry> TrnEnquiries { get; set; }

    public virtual DbSet<TrnEnquiryItem> TrnEnquiryItems { get; set; }

    public virtual DbSet<TrnEnquiryTimeline> TrnEnquiryTimelines { get; set; }

    public virtual DbSet<TrnExpenseVoucher> TrnExpenseVouchers { get; set; }

    public virtual DbSet<TrnExpenseVoucherItem> TrnExpenseVoucherItems { get; set; }

    public virtual DbSet<TrnGatePass> TrnGatePasses { get; set; }

    public virtual DbSet<TrnGatePassItem> TrnGatePassItems { get; set; }

    public virtual DbSet<TrnGoodsReceipt> TrnGoodsReceipts { get; set; }

    public virtual DbSet<TrnGoodsReceiptItem> TrnGoodsReceiptItems { get; set; }

    public virtual DbSet<TrnJob> TrnJobs { get; set; }

    public virtual DbSet<TrnJobItem> TrnJobItems { get; set; }

    public virtual DbSet<TrnJobMachineAllocation> TrnJobMachineAllocations { get; set; }

    public virtual DbSet<TrnJobMachineManpowerAllocation> TrnJobMachineManpowerAllocations { get; set; }

    public virtual DbSet<TrnJobOutsource> TrnJobOutsources { get; set; }

    public virtual DbSet<TrnJobOutsourceItem> TrnJobOutsourceItems { get; set; }

    public virtual DbSet<TrnJobTimeline> TrnJobTimelines { get; set; }

    public virtual DbSet<TrnJournalVoucher> TrnJournalVouchers { get; set; }

    public virtual DbSet<TrnJournalVoucherLine> TrnJournalVoucherLines { get; set; }

    public virtual DbSet<TrnLedger> TrnLedgers { get; set; }

    public virtual DbSet<TrnMachineBreakdown> TrnMachineBreakdowns { get; set; }

    public virtual DbSet<TrnNotification> TrnNotifications { get; set; }

    public virtual DbSet<TrnOutsourceDispatch> TrnOutsourceDispatches { get; set; }

    public virtual DbSet<TrnOutsourceReceive> TrnOutsourceReceives { get; set; }

    public virtual DbSet<TrnOutsourceTimeline> TrnOutsourceTimelines { get; set; }

    public virtual DbSet<TrnPayment> TrnPayments { get; set; }

    public virtual DbSet<TrnPaymentAllocation> TrnPaymentAllocations { get; set; }

    public virtual DbSet<TrnPrintWorkEntry> TrnPrintWorkEntries { get; set; }

    public virtual DbSet<TrnProformaInvoice> TrnProformaInvoices { get; set; }

    public virtual DbSet<TrnProformaInvoiceItem> TrnProformaInvoiceItems { get; set; }

    public virtual DbSet<TrnPurchaseGrn> TrnPurchaseGrns { get; set; }

    public virtual DbSet<TrnPurchaseGrnItem> TrnPurchaseGrnItems { get; set; }

    public virtual DbSet<TrnPurchaseInvoice> TrnPurchaseInvoices { get; set; }

    public virtual DbSet<TrnPurchaseInvoiceItem> TrnPurchaseInvoiceItems { get; set; }

    public virtual DbSet<TrnPurchaseOrder> TrnPurchaseOrders { get; set; }

    public virtual DbSet<TrnPurchaseOrderItem> TrnPurchaseOrderItems { get; set; }

    public virtual DbSet<TrnQuotation> TrnQuotations { get; set; }

    public virtual DbSet<TrnQuotationItem> TrnQuotationItems { get; set; }

    public virtual DbSet<TrnQuotationTimeline> TrnQuotationTimelines { get; set; }

    public virtual DbSet<TrnReceipt> TrnReceipts { get; set; }

    public virtual DbSet<TrnReceiptAllocation> TrnReceiptAllocations { get; set; }

    public virtual DbSet<TrnSalesInvoice> TrnSalesInvoices { get; set; }

    public virtual DbSet<TrnSalesInvoiceItem> TrnSalesInvoiceItems { get; set; }

    public virtual DbSet<TrnStockLedger> TrnStockLedgers { get; set; }

    public virtual DbSet<TrnStoreIssue> TrnStoreIssues { get; set; }

    public virtual DbSet<TrnStoreIssueItem> TrnStoreIssueItems { get; set; }

    public virtual DbSet<TrnStoreReceive> TrnStoreReceives { get; set; }

    public virtual DbSet<TrnStoreReceiveItem> TrnStoreReceiveItems { get; set; }

    public virtual DbSet<TrnStoreTimeline> TrnStoreTimelines { get; set; }

    public virtual DbSet<TrnTaxLedger> TrnTaxLedgers { get; set; }

    public virtual DbSet<TrnTdsLedger> TrnTdsLedgers { get; set; }

    public virtual DbSet<TrnUserAccessLog> TrnUserAccessLogs { get; set; }

    public virtual DbSet<TrnUserActivityLog> TrnUserActivityLogs { get; set; }

    public virtual DbSet<TrnUserNotification> TrnUserNotifications { get; set; }

    public virtual DbSet<TrnWorkspaceTask> TrnWorkspaceTasks { get; set; }

    public virtual DbSet<TrnWorkspaceTaskItem> TrnWorkspaceTaskItems { get; set; }

    public virtual DbSet<TxnNotification> TxnNotifications { get; set; }

    public virtual DbSet<TxnUserActivity> TxnUserActivities { get; set; }

    public virtual DbSet<UserLoginLog> UserLoginLogs { get; set; }

    public virtual DbSet<VwJobCostingMasterJson> VwJobCostingMasterJsons { get; set; }

    public virtual DbSet<VwMstItem> VwMstItems { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Database=minepress_db;Username=postgres;Password=minepress@123456");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ErrorLog>(entity =>
        {
            entity.HasKey(e => e.ErrorId).HasName("error_log_pkey");

            entity.ToTable("error_log", "press_db");

            entity.HasIndex(e => e.FunctionName, "idx_error_log_function");

            entity.HasIndex(e => e.ProcessCode, "idx_error_log_process");

            entity.HasIndex(e => e.ErrorTime, "idx_error_log_time");

            entity.Property(e => e.ErrorId).HasColumnName("error_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.DatabaseName).HasColumnName("database_name");
            entity.Property(e => e.ErrorContext).HasColumnName("error_context");
            entity.Property(e => e.ErrorDetail).HasColumnName("error_detail");
            entity.Property(e => e.ErrorHint).HasColumnName("error_hint");
            entity.Property(e => e.ErrorMessage).HasColumnName("error_message");
            entity.Property(e => e.ErrorState).HasColumnName("error_state");
            entity.Property(e => e.ErrorTime)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("error_time");
            entity.Property(e => e.FunctionName).HasColumnName("function_name");
            entity.Property(e => e.InputParameters)
                .HasColumnType("jsonb")
                .HasColumnName("input_parameters");
            entity.Property(e => e.ProcessCode).HasColumnName("process_code");
            entity.Property(e => e.UserName).HasColumnName("user_name");
        });

        modelBuilder.Entity<HrBonu>(entity =>
        {
            entity.HasKey(e => e.BonusId).HasName("hr_bonus_pkey");

            entity.ToTable("hr_bonus", "press_db");

            entity.HasIndex(e => e.BonusNo, "hr_bonus_bonus_no_key").IsUnique();

            entity.Property(e => e.BonusId)
                .HasDefaultValueSql("nextval('sop_db.hr_bonus_bonus_id_seq'::regclass)")
                .HasColumnName("bonus_id");
            entity.Property(e => e.ApprovedBy).HasColumnName("approved_by");
            entity.Property(e => e.ApprovedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("approved_on");
            entity.Property(e => e.BonusAmount)
                .HasPrecision(14, 2)
                .HasColumnName("bonus_amount");
            entity.Property(e => e.BonusDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("bonus_date");
            entity.Property(e => e.BonusNo)
                .HasMaxLength(30)
                .HasColumnName("bonus_no");
            entity.Property(e => e.BonusType)
                .HasMaxLength(50)
                .HasColumnName("bonus_type");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.FinYear)
                .HasMaxLength(9)
                .HasColumnName("fin_year");
            entity.Property(e => e.PayrollRunId).HasColumnName("payroll_run_id");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'PENDING'::character varying")
                .HasColumnName("status");
        });

        modelBuilder.Entity<HrHoliday>(entity =>
        {
            entity.HasKey(e => e.HolidayId).HasName("hr_holiday_pkey");

            entity.ToTable("hr_holiday", "press_db");

            entity.HasIndex(e => new { e.HolidayDate, e.CompanyId }, "hr_holiday_holiday_date_company_id_key").IsUnique();

            entity.Property(e => e.HolidayId)
                .HasDefaultValueSql("nextval('sop_db.hr_holiday_holiday_id_seq'::regclass)")
                .HasColumnName("holiday_id");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.FinYear)
                .HasMaxLength(9)
                .HasColumnName("fin_year");
            entity.Property(e => e.HolidayDate).HasColumnName("holiday_date");
            entity.Property(e => e.HolidayName)
                .HasMaxLength(150)
                .HasColumnName("holiday_name");
            entity.Property(e => e.HolidayType)
                .HasMaxLength(20)
                .HasDefaultValueSql("'NATIONAL'::character varying")
                .HasColumnName("holiday_type");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsOptional)
                .HasDefaultValue(false)
                .HasColumnName("is_optional");
            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
        });

        modelBuilder.Entity<HrIncentive>(entity =>
        {
            entity.HasKey(e => e.IncentiveId).HasName("hr_incentive_pkey");

            entity.ToTable("hr_incentive", "press_db");

            entity.HasIndex(e => e.IncentiveNo, "hr_incentive_incentive_no_key").IsUnique();

            entity.Property(e => e.IncentiveId)
                .HasDefaultValueSql("nextval('sop_db.hr_incentive_incentive_id_seq'::regclass)")
                .HasColumnName("incentive_id");
            entity.Property(e => e.ApprovedBy).HasColumnName("approved_by");
            entity.Property(e => e.ApprovedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("approved_on");
            entity.Property(e => e.CalculationBasis).HasColumnName("calculation_basis");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.IncentiveAmount)
                .HasPrecision(14, 2)
                .HasColumnName("incentive_amount");
            entity.Property(e => e.IncentiveDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("incentive_date");
            entity.Property(e => e.IncentiveNo)
                .HasMaxLength(30)
                .HasColumnName("incentive_no");
            entity.Property(e => e.IncentiveType)
                .HasMaxLength(50)
                .HasColumnName("incentive_type");
            entity.Property(e => e.PayrollRunId).HasColumnName("payroll_run_id");
            entity.Property(e => e.ReferencePeriod)
                .HasMaxLength(30)
                .HasColumnName("reference_period");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'PENDING'::character varying")
                .HasColumnName("status");
        });

        modelBuilder.Entity<HrLeaveBalance>(entity =>
        {
            entity.HasKey(e => e.BalanceId).HasName("hr_leave_balance_pkey");

            entity.ToTable("hr_leave_balance", "press_db");

            entity.HasIndex(e => new { e.EmployeeId, e.LeaveTypeId, e.FinYear }, "hr_leave_balance_employee_id_leave_type_id_fin_year_key").IsUnique();

            entity.Property(e => e.BalanceId)
                .HasDefaultValueSql("nextval('sop_db.hr_leave_balance_balance_id_seq'::regclass)")
                .HasColumnName("balance_id");
            entity.Property(e => e.Accrued)
                .HasPrecision(6, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("accrued");
            entity.Property(e => e.Availed)
                .HasPrecision(6, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("availed");
            entity.Property(e => e.CarryForward)
                .HasPrecision(6, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("carry_forward");
            entity.Property(e => e.ClosingBalance)
                .HasPrecision(6, 2)
                .HasComputedColumnSql("((((opening_balance + accrued) - availed) - encashed) - lapsed)", true)
                .HasColumnName("closing_balance");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.Encashed)
                .HasPrecision(6, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("encashed");
            entity.Property(e => e.FinYear)
                .HasMaxLength(9)
                .HasColumnName("fin_year");
            entity.Property(e => e.Lapsed)
                .HasPrecision(6, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("lapsed");
            entity.Property(e => e.LeaveTypeId).HasColumnName("leave_type_id");
            entity.Property(e => e.OpeningBalance)
                .HasPrecision(6, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("opening_balance");
        });

        modelBuilder.Entity<HrLeaveRequest>(entity =>
        {
            entity.HasKey(e => e.LeaveId).HasName("hr_leave_request_pkey");

            entity.ToTable("hr_leave_request", "press_db");

            entity.HasIndex(e => e.EmployeeId, "hr_leave_request_employee_id_idx");

            entity.HasIndex(e => e.LeaveNo, "hr_leave_request_leave_no_key").IsUnique();

            entity.HasIndex(e => e.Status, "hr_leave_request_status_idx");

            entity.Property(e => e.LeaveId)
                .HasDefaultValueSql("nextval('sop_db.hr_leave_request_leave_id_seq'::regclass)")
                .HasColumnName("leave_id");
            entity.Property(e => e.AppliedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("applied_on");
            entity.Property(e => e.ApprovedBy).HasColumnName("approved_by");
            entity.Property(e => e.ApprovedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("approved_on");
            entity.Property(e => e.CancelledBy).HasColumnName("cancelled_by");
            entity.Property(e => e.CancelledOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("cancelled_on");
            entity.Property(e => e.ContactDuringLeave)
                .HasMaxLength(100)
                .HasColumnName("contact_during_leave");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.DocumentPath)
                .HasMaxLength(500)
                .HasColumnName("document_path");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.FromDate).HasColumnName("from_date");
            entity.Property(e => e.HalfDay)
                .HasDefaultValue(false)
                .HasColumnName("half_day");
            entity.Property(e => e.HalfDaySession)
                .HasMaxLength(10)
                .HasColumnName("half_day_session");
            entity.Property(e => e.LeaveNo)
                .HasMaxLength(30)
                .HasColumnName("leave_no");
            entity.Property(e => e.LeaveTypeId).HasColumnName("leave_type_id");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.RejectionReason).HasColumnName("rejection_reason");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'PENDING'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.ToDate).HasColumnName("to_date");
            entity.Property(e => e.TotalDays)
                .HasPrecision(5, 1)
                .HasColumnName("total_days");
        });

        modelBuilder.Entity<HrLeaveType>(entity =>
        {
            entity.HasKey(e => e.LeaveTypeId).HasName("hr_leave_type_pkey");

            entity.ToTable("hr_leave_type", "press_db");

            entity.HasIndex(e => e.LeaveCode, "hr_leave_type_leave_code_key").IsUnique();

            entity.Property(e => e.LeaveTypeId)
                .HasDefaultValueSql("nextval('sop_db.hr_leave_type_leave_type_id_seq'::regclass)")
                .HasColumnName("leave_type_id");
            entity.Property(e => e.ApplicableGender)
                .HasMaxLength(10)
                .HasDefaultValueSql("'ALL'::character varying")
                .HasColumnName("applicable_gender");
            entity.Property(e => e.CarryForward)
                .HasDefaultValue(false)
                .HasColumnName("carry_forward");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.Encashable)
                .HasDefaultValue(false)
                .HasColumnName("encashable");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.LeaveCategory)
                .HasMaxLength(30)
                .HasDefaultValueSql("'PAID'::character varying")
                .HasColumnName("leave_category");
            entity.Property(e => e.LeaveCode)
                .HasMaxLength(20)
                .HasColumnName("leave_code");
            entity.Property(e => e.LeaveName)
                .HasMaxLength(100)
                .HasColumnName("leave_name");
            entity.Property(e => e.MaxCarryForward)
                .HasDefaultValue(0)
                .HasColumnName("max_carry_forward");
            entity.Property(e => e.MaxDaysPerMonth)
                .HasDefaultValue(0)
                .HasColumnName("max_days_per_month");
            entity.Property(e => e.MaxDaysPerYear)
                .HasDefaultValue(0)
                .HasColumnName("max_days_per_year");
            entity.Property(e => e.MinServiceMonths)
                .HasDefaultValue(0)
                .HasColumnName("min_service_months");
            entity.Property(e => e.ProRataOnJoin)
                .HasDefaultValue(true)
                .HasColumnName("pro_rata_on_join");
            entity.Property(e => e.RequiresDocs)
                .HasDefaultValue(false)
                .HasColumnName("requires_docs");
        });

        modelBuilder.Entity<HrLoan>(entity =>
        {
            entity.HasKey(e => e.LoanId).HasName("hr_loan_pkey");

            entity.ToTable("hr_loan", "press_db");

            entity.HasIndex(e => e.EmployeeId, "hr_loan_employee_id_idx");

            entity.HasIndex(e => e.LoanNo, "hr_loan_loan_no_key").IsUnique();

            entity.Property(e => e.LoanId)
                .HasDefaultValueSql("nextval('sop_db.hr_loan_loan_id_seq'::regclass)")
                .HasColumnName("loan_id");
            entity.Property(e => e.ApprovedBy).HasColumnName("approved_by");
            entity.Property(e => e.ApprovedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("approved_on");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.DisbursedAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("disbursed_amount");
            entity.Property(e => e.DisbursedOn).HasColumnName("disbursed_on");
            entity.Property(e => e.EmiAmount)
                .HasPrecision(12, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("emi_amount");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.InterestRate)
                .HasPrecision(5, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("interest_rate");
            entity.Property(e => e.LoanAmount)
                .HasPrecision(14, 2)
                .HasColumnName("loan_amount");
            entity.Property(e => e.LoanDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("loan_date");
            entity.Property(e => e.LoanNo)
                .HasMaxLength(30)
                .HasColumnName("loan_no");
            entity.Property(e => e.LoanType)
                .HasMaxLength(50)
                .HasColumnName("loan_type");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.OutstandingAmount)
                .HasPrecision(14, 2)
                .HasComputedColumnSql("(disbursed_amount - recovered_amount)", true)
                .HasColumnName("outstanding_amount");
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.RecoveredAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("recovered_amount");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'PENDING'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.TenureMonths).HasColumnName("tenure_months");
        });

        modelBuilder.Entity<HrLoanRepayment>(entity =>
        {
            entity.HasKey(e => e.RepaymentId).HasName("hr_loan_repayment_pkey");

            entity.ToTable("hr_loan_repayment", "press_db");

            entity.HasIndex(e => new { e.LoanId, e.InstallmentNo }, "hr_loan_repayment_loan_id_installment_no_key").IsUnique();

            entity.Property(e => e.RepaymentId)
                .HasDefaultValueSql("nextval('sop_db.hr_loan_repayment_repayment_id_seq'::regclass)")
                .HasColumnName("repayment_id");
            entity.Property(e => e.DueAmount)
                .HasPrecision(12, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("due_amount");
            entity.Property(e => e.DueDate).HasColumnName("due_date");
            entity.Property(e => e.InstallmentNo).HasColumnName("installment_no");
            entity.Property(e => e.InterestAmount)
                .HasPrecision(12, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("interest_amount");
            entity.Property(e => e.IsPaid)
                .HasDefaultValue(false)
                .HasColumnName("is_paid");
            entity.Property(e => e.LoanId).HasColumnName("loan_id");
            entity.Property(e => e.PaidAmount)
                .HasPrecision(12, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("paid_amount");
            entity.Property(e => e.PaidDate).HasColumnName("paid_date");
            entity.Property(e => e.PayrollRunId).HasColumnName("payroll_run_id");
            entity.Property(e => e.PrincipalAmount)
                .HasPrecision(12, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("principal_amount");
        });

        modelBuilder.Entity<HrMedicalClaim>(entity =>
        {
            entity.HasKey(e => e.MedicalClaimId).HasName("hr_medical_claim_pkey");

            entity.ToTable("hr_medical_claim", "press_db");

            entity.HasIndex(e => e.ClaimNo, "hr_medical_claim_claim_no_key").IsUnique();

            entity.HasIndex(e => e.EmployeeId, "hr_medical_claim_employee_id_idx");

            entity.Property(e => e.MedicalClaimId)
                .HasDefaultValueSql("nextval('sop_db.hr_medical_claim_medical_claim_id_seq'::regclass)")
                .HasColumnName("medical_claim_id");
            entity.Property(e => e.ApprovedAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("approved_amount");
            entity.Property(e => e.ApprovedBy).HasColumnName("approved_by");
            entity.Property(e => e.ApprovedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("approved_on");
            entity.Property(e => e.ClaimAmount)
                .HasPrecision(14, 2)
                .HasColumnName("claim_amount");
            entity.Property(e => e.ClaimDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("claim_date");
            entity.Property(e => e.ClaimNo)
                .HasMaxLength(30)
                .HasColumnName("claim_no");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DocumentsJson)
                .HasDefaultValueSql("'[]'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("documents_json");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.HospitalName)
                .HasMaxLength(200)
                .HasColumnName("hospital_name");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.PaidAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("paid_amount");
            entity.Property(e => e.PaidOn).HasColumnName("paid_on");
            entity.Property(e => e.PatientName)
                .HasMaxLength(150)
                .HasColumnName("patient_name");
            entity.Property(e => e.RejectionReason).HasColumnName("rejection_reason");
            entity.Property(e => e.Relation)
                .HasMaxLength(30)
                .HasDefaultValueSql("'SELF'::character varying")
                .HasColumnName("relation");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'PENDING'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.TreatmentFrom).HasColumnName("treatment_from");
            entity.Property(e => e.TreatmentTo).HasColumnName("treatment_to");
            entity.Property(e => e.TreatmentType)
                .HasMaxLength(100)
                .HasColumnName("treatment_type");
        });

        modelBuilder.Entity<HrOvertime>(entity =>
        {
            entity.HasKey(e => e.OtId).HasName("hr_overtime_pkey");

            entity.ToTable("hr_overtime", "press_db");

            entity.HasIndex(e => new { e.EmployeeId, e.OtDate }, "hr_overtime_employee_id_ot_date_idx");

            entity.HasIndex(e => e.OtNo, "hr_overtime_ot_no_key").IsUnique();

            entity.Property(e => e.OtId)
                .HasDefaultValueSql("nextval('sop_db.hr_overtime_ot_id_seq'::regclass)")
                .HasColumnName("ot_id");
            entity.Property(e => e.ApprovedBy).HasColumnName("approved_by");
            entity.Property(e => e.ApprovedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("approved_on");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.FromTime).HasColumnName("from_time");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.OtAmount)
                .HasPrecision(12, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("ot_amount");
            entity.Property(e => e.OtDate).HasColumnName("ot_date");
            entity.Property(e => e.OtHours)
                .HasPrecision(5, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("ot_hours");
            entity.Property(e => e.OtNo)
                .HasMaxLength(30)
                .HasColumnName("ot_no");
            entity.Property(e => e.OtRatePerHour)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("ot_rate_per_hour");
            entity.Property(e => e.OtReason).HasColumnName("ot_reason");
            entity.Property(e => e.RejectionReason).HasColumnName("rejection_reason");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'PENDING'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.ToTime).HasColumnName("to_time");
        });

        modelBuilder.Entity<HrReimbursement>(entity =>
        {
            entity.HasKey(e => e.ReimbursementId).HasName("hr_reimbursement_pkey");

            entity.ToTable("hr_reimbursement", "press_db", tb => tb.HasComment("Employee expense reimbursement claims (medical, travel, fuel, etc.)."));

            entity.HasIndex(e => e.ReimbursementNo, "uq_hr_reimbursement_no").IsUnique();

            entity.Property(e => e.ReimbursementId).HasColumnName("reimbursement_id");
            entity.Property(e => e.ApprovedAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("approved_amount");
            entity.Property(e => e.ApprovedBy).HasColumnName("approved_by");
            entity.Property(e => e.ApprovedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("approved_on");
            entity.Property(e => e.ClaimAmount)
                .HasPrecision(14, 2)
                .HasColumnName("claim_amount");
            entity.Property(e => e.ClaimDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("claim_date");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DocumentPath)
                .HasMaxLength(500)
                .HasColumnName("document_path");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.PaidAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("paid_amount");
            entity.Property(e => e.PaidOn).HasColumnName("paid_on");
            entity.Property(e => e.PayrollRunId).HasColumnName("payroll_run_id");
            entity.Property(e => e.ReimbursementNo)
                .HasMaxLength(30)
                .HasColumnName("reimbursement_no");
            entity.Property(e => e.ReimbursementType)
                .HasMaxLength(50)
                .HasColumnName("reimbursement_type");
            entity.Property(e => e.RejectionReason).HasColumnName("rejection_reason");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'PENDING'::character varying")
                .HasColumnName("status");

            entity.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.HrReimbursementApprovedByNavigations)
                .HasForeignKey(d => d.ApprovedBy)
                .HasConstraintName("fk_hrreim_approved_by");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.HrReimbursementCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_hrreim_created_by");

            entity.HasOne(d => d.Employee).WithMany(p => p.HrReimbursements)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_hrreim_employee");
        });

        modelBuilder.Entity<HrResignation>(entity =>
        {
            entity.HasKey(e => e.ResignationId).HasName("hr_resignation_pkey");

            entity.ToTable("hr_resignation", "press_db");

            entity.HasIndex(e => e.EmployeeId, "hr_resignation_employee_id_idx");

            entity.HasIndex(e => e.ResignationNo, "hr_resignation_resignation_no_key").IsUnique();

            entity.Property(e => e.ResignationId)
                .HasDefaultValueSql("nextval('sop_db.hr_resignation_resignation_id_seq'::regclass)")
                .HasColumnName("resignation_id");
            entity.Property(e => e.AcceptedBy).HasColumnName("accepted_by");
            entity.Property(e => e.AcceptedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("accepted_on");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.LastWorkingDay).HasColumnName("last_working_day");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.NoticePeriodDays)
                .HasDefaultValue(0)
                .HasColumnName("notice_period_days");
            entity.Property(e => e.NoticeWaiverDays)
                .HasDefaultValue(0)
                .HasColumnName("notice_waiver_days");
            entity.Property(e => e.RejectionReason).HasColumnName("rejection_reason");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.ResignationDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("resignation_date");
            entity.Property(e => e.ResignationNo)
                .HasMaxLength(30)
                .HasColumnName("resignation_no");
            entity.Property(e => e.ResignationReason).HasColumnName("resignation_reason");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'SUBMITTED'::character varying")
                .HasColumnName("status");
        });

        modelBuilder.Entity<HrSalaryAdvance>(entity =>
        {
            entity.HasKey(e => e.AdvanceId).HasName("hr_salary_advance_pkey");

            entity.ToTable("hr_salary_advance", "press_db");

            entity.HasIndex(e => e.AdvanceNo, "hr_salary_advance_advance_no_key").IsUnique();

            entity.HasIndex(e => e.EmployeeId, "hr_salary_advance_employee_id_idx");

            entity.Property(e => e.AdvanceId)
                .HasDefaultValueSql("nextval('sop_db.hr_salary_advance_advance_id_seq'::regclass)")
                .HasColumnName("advance_id");
            entity.Property(e => e.AdvanceAmount)
                .HasPrecision(14, 2)
                .HasColumnName("advance_amount");
            entity.Property(e => e.AdvanceDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("advance_date");
            entity.Property(e => e.AdvanceNo)
                .HasMaxLength(30)
                .HasColumnName("advance_no");
            entity.Property(e => e.ApprovedBy).HasColumnName("approved_by");
            entity.Property(e => e.ApprovedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("approved_on");
            entity.Property(e => e.BalanceAmount)
                .HasPrecision(14, 2)
                .HasComputedColumnSql("(advance_amount - recovered_amount)", true)
                .HasColumnName("balance_amount");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.MonthlyDeduction)
                .HasPrecision(12, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("monthly_deduction");
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.RecoveredAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("recovered_amount");
            entity.Property(e => e.RejectionReason).HasColumnName("rejection_reason");
            entity.Property(e => e.RepaymentMonths)
                .HasDefaultValue(1)
                .HasColumnName("repayment_months");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'PENDING'::character varying")
                .HasColumnName("status");
        });

        modelBuilder.Entity<HrShiftRoster>(entity =>
        {
            entity.HasKey(e => e.RosterId).HasName("hr_shift_roster_pkey");

            entity.ToTable("hr_shift_roster", "press_db");

            entity.HasIndex(e => e.EmployeeId, "hr_shift_roster_employee_id_idx");

            entity.Property(e => e.RosterId)
                .HasDefaultValueSql("nextval('sop_db.hr_shift_roster_roster_id_seq'::regclass)")
                .HasColumnName("roster_id");
            entity.Property(e => e.AssignedBy).HasColumnName("assigned_by");
            entity.Property(e => e.AssignedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("assigned_on");
            entity.Property(e => e.EffectiveFrom).HasColumnName("effective_from");
            entity.Property(e => e.EffectiveTo).HasColumnName("effective_to");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.ShiftTypeId).HasColumnName("shift_type_id");
            entity.Property(e => e.WeekOffDays)
                .HasMaxLength(50)
                .HasColumnName("week_off_days");
        });

        modelBuilder.Entity<HrTransfer>(entity =>
        {
            entity.HasKey(e => e.TransferId).HasName("hr_transfer_pkey");

            entity.ToTable("hr_transfer", "press_db");

            entity.HasIndex(e => e.TransferNo, "hr_transfer_transfer_no_key").IsUnique();

            entity.Property(e => e.TransferId)
                .HasDefaultValueSql("nextval('sop_db.hr_transfer_transfer_id_seq'::regclass)")
                .HasColumnName("transfer_id");
            entity.Property(e => e.ApprovedBy).HasColumnName("approved_by");
            entity.Property(e => e.ApprovedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("approved_on");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.EffectiveDate).HasColumnName("effective_date");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.FromDeptId).HasColumnName("from_dept_id");
            entity.Property(e => e.FromLocationId).HasColumnName("from_location_id");
            entity.Property(e => e.OrderLetterPath)
                .HasMaxLength(500)
                .HasColumnName("order_letter_path");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'PENDING'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.ToDeptId).HasColumnName("to_dept_id");
            entity.Property(e => e.ToLocationId).HasColumnName("to_location_id");
            entity.Property(e => e.TransferDate).HasColumnName("transfer_date");
            entity.Property(e => e.TransferNo)
                .HasMaxLength(30)
                .HasColumnName("transfer_no");
            entity.Property(e => e.TransferReason).HasColumnName("transfer_reason");
        });

        modelBuilder.Entity<HrTravelExpense>(entity =>
        {
            entity.HasKey(e => e.TravelId).HasName("hr_travel_expense_pkey");

            entity.ToTable("hr_travel_expense", "press_db");

            entity.HasIndex(e => e.EmployeeId, "hr_travel_expense_employee_id_idx");

            entity.HasIndex(e => e.TravelNo, "hr_travel_expense_travel_no_key").IsUnique();

            entity.Property(e => e.TravelId)
                .HasDefaultValueSql("nextval('sop_db.hr_travel_expense_travel_id_seq'::regclass)")
                .HasColumnName("travel_id");
            entity.Property(e => e.AdvanceAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("advance_amount");
            entity.Property(e => e.ApprovedAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("approved_amount");
            entity.Property(e => e.ApprovedBy).HasColumnName("approved_by");
            entity.Property(e => e.ApprovedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("approved_on");
            entity.Property(e => e.ClaimAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("claim_amount");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.ExpenseLinesJson)
                .HasDefaultValueSql("'[]'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("expense_lines_json");
            entity.Property(e => e.FromLocation)
                .HasMaxLength(200)
                .HasColumnName("from_location");
            entity.Property(e => e.ModeOfTravel)
                .HasMaxLength(30)
                .HasColumnName("mode_of_travel");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.Purpose).HasColumnName("purpose");
            entity.Property(e => e.RejectionReason).HasColumnName("rejection_reason");
            entity.Property(e => e.ReturnDate).HasColumnName("return_date");
            entity.Property(e => e.SettledAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("settled_amount");
            entity.Property(e => e.SettledOn).HasColumnName("settled_on");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'PENDING'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.ToLocation)
                .HasMaxLength(200)
                .HasColumnName("to_location");
            entity.Property(e => e.TravelDate).HasColumnName("travel_date");
            entity.Property(e => e.TravelNo)
                .HasMaxLength(30)
                .HasColumnName("travel_no");
        });

        modelBuilder.Entity<HrUniformAllotment>(entity =>
        {
            entity.HasKey(e => e.UniformId).HasName("hr_uniform_allotment_pkey");

            entity.ToTable("hr_uniform_allotment", "press_db");

            entity.Property(e => e.UniformId)
                .HasDefaultValueSql("nextval('sop_db.hr_uniform_allotment_uniform_id_seq'::regclass)")
                .HasColumnName("uniform_id");
            entity.Property(e => e.AllotmentDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("allotment_date");
            entity.Property(e => e.CostPerUnit)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("cost_per_unit");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.ItemDescription)
                .HasMaxLength(200)
                .HasColumnName("item_description");
            entity.Property(e => e.ItemType)
                .HasMaxLength(30)
                .HasColumnName("item_type");
            entity.Property(e => e.Quantity)
                .HasDefaultValue(1)
                .HasColumnName("quantity");
            entity.Property(e => e.RecoveryFromSalary)
                .HasDefaultValue(false)
                .HasColumnName("recovery_from_salary");
            entity.Property(e => e.RecoveryMonths)
                .HasDefaultValue(0)
                .HasColumnName("recovery_months");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.Size)
                .HasMaxLength(20)
                .HasColumnName("size");
            entity.Property(e => e.TotalCost)
                .HasPrecision(12, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("total_cost");
        });

        modelBuilder.Entity<HrVacancy>(entity =>
        {
            entity.HasKey(e => e.VacancyId).HasName("hr_vacancy_pkey");

            entity.ToTable("hr_vacancy", "press_db");

            entity.HasIndex(e => e.VacancyNo, "hr_vacancy_vacancy_no_key").IsUnique();

            entity.Property(e => e.VacancyId)
                .HasDefaultValueSql("nextval('sop_db.hr_vacancy_vacancy_id_seq'::regclass)")
                .HasColumnName("vacancy_id");
            entity.Property(e => e.ApprovedBy).HasColumnName("approved_by");
            entity.Property(e => e.ApprovedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("approved_on");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.DeptId).HasColumnName("dept_id");
            entity.Property(e => e.DesignationId).HasColumnName("designation_id");
            entity.Property(e => e.ExperienceMax).HasColumnName("experience_max");
            entity.Property(e => e.ExperienceMin)
                .HasDefaultValue(0)
                .HasColumnName("experience_min");
            entity.Property(e => e.FilledPositions)
                .HasDefaultValue(0)
                .HasColumnName("filled_positions");
            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.Positions)
                .HasDefaultValue(1)
                .HasColumnName("positions");
            entity.Property(e => e.Qualification)
                .HasMaxLength(200)
                .HasColumnName("qualification");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.SalaryMax)
                .HasPrecision(12, 2)
                .HasColumnName("salary_max");
            entity.Property(e => e.SalaryMin)
                .HasPrecision(12, 2)
                .HasColumnName("salary_min");
            entity.Property(e => e.SkillsRequired).HasColumnName("skills_required");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'OPEN'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.TargetDate).HasColumnName("target_date");
            entity.Property(e => e.VacancyNo)
                .HasMaxLength(30)
                .HasColumnName("vacancy_no");
            entity.Property(e => e.VacancyType)
                .HasMaxLength(20)
                .HasDefaultValueSql("'PERMANENT'::character varying")
                .HasColumnName("vacancy_type");
        });

        modelBuilder.Entity<HybEmployeeAttendance>(entity =>
        {
            entity.HasKey(e => e.AttendanceId).HasName("hyb_employee_attendance_pkey");

            entity.ToTable("hyb_employee_attendance", "press_db", tb => tb.HasComment("Employee attendance and shift tracking for production floor. Links to job/process for labour cost allocation."));

            entity.HasIndex(e => e.AttendanceData, "idx_att_data").HasMethod("gin");

            entity.HasIndex(e => e.AttendanceDate, "idx_att_date");

            entity.HasIndex(e => e.DepartmentId, "idx_att_department");

            entity.HasIndex(e => new { e.DepartmentId, e.AttendanceDate }, "idx_att_dept");

            entity.HasIndex(e => new { e.EmployeeId, e.AttendanceDate }, "idx_att_employee");

            entity.HasIndex(e => e.JobId, "idx_att_job").HasFilter("(job_id IS NOT NULL)");

            entity.HasIndex(e => e.Status, "idx_att_status");

            entity.HasIndex(e => new { e.EmployeeId, e.AttendanceDate }, "uq_att_employee_date").IsUnique();

            entity.Property(e => e.AttendanceId).HasColumnName("attendance_id");
            entity.Property(e => e.AttendanceData)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("attendance_data");
            entity.Property(e => e.AttendanceDate).HasColumnName("attendance_date");
            entity.Property(e => e.BreakMinutes)
                .HasDefaultValue(0)
                .HasColumnName("break_minutes");
            entity.Property(e => e.CheckIn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("check_in");
            entity.Property(e => e.CheckOut)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("check_out");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.MachineId).HasColumnName("machine_id");
            entity.Property(e => e.ModifiedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.OvertimeApproved)
                .HasDefaultValue(false)
                .HasColumnName("overtime_approved");
            entity.Property(e => e.OvertimeHours)
                .HasPrecision(5, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("overtime_hours");
            entity.Property(e => e.ProcessId).HasColumnName("process_id");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.ShiftTypeId).HasColumnName("shift_type_id");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'PRESENT'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.TotalHours)
                .HasPrecision(5, 2)
                .HasComputedColumnSql("\nCASE\n    WHEN ((check_in IS NOT NULL) AND (check_out IS NOT NULL)) THEN ((EXTRACT(epoch FROM (check_out - check_in)) / 3600.0) - ((break_minutes)::numeric / 60.0))\n    ELSE (0)::numeric\nEND", true)
                .HasColumnName("total_hours");

            entity.HasOne(d => d.Department).WithMany(p => p.HybEmployeeAttendances)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("fk_att_department");

            entity.HasOne(d => d.Employee).WithMany(p => p.HybEmployeeAttendances)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_att_employee");

            entity.HasOne(d => d.Job).WithMany(p => p.HybEmployeeAttendances)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("fk_att_job");

            entity.HasOne(d => d.Machine).WithMany(p => p.HybEmployeeAttendances)
                .HasForeignKey(d => d.MachineId)
                .HasConstraintName("fk_att_machine");

            entity.HasOne(d => d.Process).WithMany(p => p.HybEmployeeAttendances)
                .HasForeignKey(d => d.ProcessId)
                .HasConstraintName("fk_att_process");

            entity.HasOne(d => d.ShiftType).WithMany(p => p.HybEmployeeAttendances)
                .HasForeignKey(d => d.ShiftTypeId)
                .HasConstraintName("fk_att_shift");
        });

        modelBuilder.Entity<HybJobRateCalculator>(entity =>
        {
            entity.HasKey(e => e.RateCalcId).HasName("hyb_job_rate_calculator_pkey");

            entity.ToTable("hyb_job_rate_calculator", "press_db", tb => tb.HasComment("Hybrid SQL+JSONB table storing AI Rate Calculator results. Relational columns for product config, IDs, totals (joins/filters/reporting). JSONB columns for parts, cost breakdown, BOM, AI insights, machine recommendations, and full input snapshot. Links to enquiry received process for quotation and negotiation workflow."));

            entity.HasIndex(e => e.BomData, "idx_rc_bom").HasMethod("gin");

            entity.HasIndex(e => e.CalcRefNo, "idx_rc_calc_ref_no");

            entity.HasIndex(e => e.CostBreakdown, "idx_rc_cost").HasMethod("gin");

            entity.HasIndex(e => e.CreatedOn, "idx_rc_created").IsDescending();

            entity.HasIndex(e => e.CreatedBy, "idx_rc_created_by");

            entity.HasIndex(e => e.EnquiryId, "idx_rc_enquiry");

            entity.HasIndex(e => e.EnquiryId, "idx_rc_enquiry_id");

            entity.HasIndex(e => new { e.EnquiryId, e.Version }, "idx_rc_enquiry_version").IsDescending(false, true);

            entity.HasIndex(e => e.AiInsights, "idx_rc_insights").HasMethod("gin");

            entity.HasIndex(e => e.JobId, "idx_rc_job");

            entity.HasIndex(e => e.JobId, "idx_rc_job_id");

            entity.HasIndex(e => e.JobTypeId, "idx_rc_job_type_id");

            entity.HasIndex(e => e.ParentCalcId, "idx_rc_parent_calc_id");

            entity.HasIndex(e => e.PartsData, "idx_rc_parts").HasMethod("gin");

            entity.HasIndex(e => new { e.PartyId, e.Status }, "idx_rc_party");

            entity.HasIndex(e => e.PartyId, "idx_rc_party_id");

            entity.HasIndex(e => new { e.ProductTypeId, e.JobTypeId }, "idx_rc_product");

            entity.HasIndex(e => e.ProductSizeId, "idx_rc_product_size_id");

            entity.HasIndex(e => e.ProductTypeId, "idx_rc_product_type_id");

            entity.HasIndex(e => e.QuotationId, "idx_rc_quotation_id");

            entity.HasIndex(e => e.CalcRefNo, "idx_rc_ref_no").IsUnique();

            entity.HasIndex(e => e.CalcInputSnapshot, "idx_rc_snapshot").HasMethod("gin");

            entity.HasIndex(e => e.Status, "idx_rc_status").HasFilter("((status)::text = ANY (ARRAY[('DRAFT'::character varying)::text, ('SAVED'::character varying)::text, ('ENQUIRY_CREATED'::character varying)::text, ('QUOTATION_SENT'::character varying)::text]))");

            entity.HasIndex(e => e.GrandTotal, "idx_rc_total");

            entity.Property(e => e.RateCalcId).HasColumnName("rate_calc_id");
            entity.Property(e => e.AiInsights)
                .HasDefaultValueSql("'[]'::jsonb")
                .HasComment("JSONB array of AI-generated insights/recommendations. Each has icon, title, description, and severity (info/warn/error).")
                .HasColumnType("jsonb")
                .HasColumnName("ai_insights");
            entity.Property(e => e.BomData)
                .HasDefaultValueSql("'[]'::jsonb")
                .HasComment("JSONB array of Bill of Materials line items. Each item has category, material_name, specification, for_part, quantity, unit, rate, and amount.")
                .HasColumnType("jsonb")
                .HasColumnName("bom_data");
            entity.Property(e => e.CalcInputSnapshot)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasComment("JSONB snapshot of all selected master data at calculation time for reproducibility and audit trail.")
                .HasColumnType("jsonb")
                .HasColumnName("calc_input_snapshot");
            entity.Property(e => e.CalcRefNo)
                .HasMaxLength(50)
                .HasComment("Unique reference number generated as RC-YYYYMMDD-HHMMSS.")
                .HasColumnName("calc_ref_no");
            entity.Property(e => e.ClientRemarks).HasColumnName("client_remarks");
            entity.Property(e => e.ConfigData)
                .HasColumnType("jsonb")
                .HasColumnName("config_data");
            entity.Property(e => e.CostBreakdown)
                .HasDefaultValueSql("'[]'::jsonb")
                .HasComment("JSONB array of cost line items displayed in the Cost Breakdown table. Each item has icon, name, category, detail, and amount.")
                .HasColumnType("jsonb")
                .HasColumnName("cost_breakdown");
            entity.Property(e => e.CostPerUnit)
                .HasPrecision(14, 4)
                .HasColumnName("cost_per_unit");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.EnquiryId).HasColumnName("enquiry_id");
            entity.Property(e => e.GrandTotal)
                .HasPrecision(14, 2)
                .HasColumnName("grand_total");
            entity.Property(e => e.InternalRemarks).HasColumnName("internal_remarks");
            entity.Property(e => e.IsCustomerMaterial)
                .HasDefaultValue(false)
                .HasColumnName("is_customer_material");
            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.JobTypeId).HasColumnName("job_type_id");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(50)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.NetTotal)
                .HasPrecision(14, 2)
                .HasColumnName("net_total");
            entity.Property(e => e.ParentCalcId)
                .HasComment("Self-referencing FK to previous version when a rate calculation is revised.")
                .HasColumnName("parent_calc_id");
            entity.Property(e => e.PartsData)
                .HasDefaultValueSql("'[]'::jsonb")
                .HasComment("JSONB array of product parts with per-part configuration (pages, copies, colors, paper, finishing) and calculated results (sheets, paper cost, plate cost, ink cost, finishing cost, sub-total).")
                .HasColumnType("jsonb")
                .HasColumnName("parts_data");
            entity.Property(e => e.PartyId).HasColumnName("party_id");
            entity.Property(e => e.PrintingMode)
                .HasMaxLength(30)
                .HasColumnName("printing_mode");
            entity.Property(e => e.ProductSizeId).HasColumnName("product_size_id");
            entity.Property(e => e.ProductTypeId).HasColumnName("product_type_id");
            entity.Property(e => e.Quantity)
                .HasDefaultValue(0)
                .HasColumnName("quantity");
            entity.Property(e => e.QuotationId).HasColumnName("quotation_id");
            entity.Property(e => e.RecommendedMachines)
                .HasDefaultValueSql("'[]'::jsonb")
                .HasComment("JSONB array of machine options with estimated costs for comparison.")
                .HasColumnType("jsonb")
                .HasColumnName("recommended_machines");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'DRAFT'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.TaxAmount)
                .HasPrecision(14, 2)
                .HasColumnName("tax_amount");
            entity.Property(e => e.TotalPages)
                .HasDefaultValue(0)
                .HasColumnName("total_pages");
            entity.Property(e => e.TrimHeightMm)
                .HasPrecision(8, 2)
                .HasColumnName("trim_height_mm");
            entity.Property(e => e.TrimWidthMm)
                .HasPrecision(8, 2)
                .HasColumnName("trim_width_mm");
            entity.Property(e => e.ValidityDate).HasColumnName("validity_date");
            entity.Property(e => e.Version)
                .HasDefaultValue(1)
                .HasColumnName("version");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.HybJobRateCalculators)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_rc_created_by");

            entity.HasOne(d => d.Enquiry).WithMany(p => p.HybJobRateCalculators)
                .HasForeignKey(d => d.EnquiryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_rc_enquiry");

            entity.HasOne(d => d.Job).WithMany(p => p.HybJobRateCalculators)
                .HasForeignKey(d => d.JobId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_rc_job");

            entity.HasOne(d => d.JobType).WithMany(p => p.HybJobRateCalculators)
                .HasForeignKey(d => d.JobTypeId)
                .HasConstraintName("fk_rc_job_type");

            entity.HasOne(d => d.ParentCalc).WithMany(p => p.InverseParentCalc)
                .HasForeignKey(d => d.ParentCalcId)
                .HasConstraintName("fk_rc_parent");

            entity.HasOne(d => d.Party).WithMany(p => p.HybJobRateCalculators)
                .HasForeignKey(d => d.PartyId)
                .HasConstraintName("fk_rc_party");

            entity.HasOne(d => d.ProductSize).WithMany(p => p.HybJobRateCalculators)
                .HasForeignKey(d => d.ProductSizeId)
                .HasConstraintName("fk_rc_product_size");

            entity.HasOne(d => d.ProductType).WithMany(p => p.HybJobRateCalculators)
                .HasForeignKey(d => d.ProductTypeId)
                .HasConstraintName("fk_rc_product_type");

            entity.HasOne(d => d.Quotation).WithMany(p => p.HybJobRateCalculators)
                .HasForeignKey(d => d.QuotationId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_rc_quotation");
        });

        modelBuilder.Entity<MapModuleDepartment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("map_module_department_pkey");

            entity.ToTable("map_module_department", "press_db", tb => tb.HasComment("Maps departments to allowed navigation modules. Controls which top-level menu groups are visible per department."));

            entity.HasIndex(e => new { e.DepartmentId, e.ModuleId }, "uq_dept_module").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.DepartmentId)
                .HasComment("FK → mst_department.dept_id")
                .HasColumnName("department_id");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.ModuleId)
                .HasComment("Module identifier matching mst_menu.module_id (top-level grouping)")
                .HasColumnName("module_id");
        });

        modelBuilder.Entity<MapUserPermission>(entity =>
        {
            entity.HasKey(e => new { e.Userid, e.Permissionid }).HasName("map_user_permission_pkey");

            entity.ToTable("map_user_permission", "press_db");

            entity.Property(e => e.Userid).HasColumnName("userid");
            entity.Property(e => e.Permissionid).HasColumnName("permissionid");
            entity.Property(e => e.Isallowed)
                .HasDefaultValue(true)
                .HasColumnName("isallowed");

            entity.HasOne(d => d.Permission).WithMany(p => p.MapUserPermissions)
                .HasForeignKey(d => d.Permissionid)
                .HasConstraintName("fk_map_user_permission_permissionid");

            entity.HasOne(d => d.User).WithMany(p => p.MapUserPermissions)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("fk_map_user_permission_userid");
        });

        modelBuilder.Entity<MapUserRole>(entity =>
        {
            entity.HasKey(e => new { e.Userid, e.Roleid }).HasName("map_user_role_pkey");

            entity.ToTable("map_user_role", "press_db");

            entity.HasIndex(e => e.Roleid, "idx_map_user_role_roleid");

            entity.HasIndex(e => e.Userid, "idx_map_user_role_userid");

            entity.Property(e => e.Userid).HasColumnName("userid");
            entity.Property(e => e.Roleid).HasColumnName("roleid");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");

            entity.HasOne(d => d.Role).WithMany(p => p.MapUserRoles)
                .HasForeignKey(d => d.Roleid)
                .HasConstraintName("fk_map_user_role_role");

            entity.HasOne(d => d.User).WithMany(p => p.MapUserRoles)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("fk_map_user_role_user");
        });

        modelBuilder.Entity<MstAccountHead>(entity =>
        {
            entity.HasKey(e => e.AccountHeadId).HasName("mst_account_head_pkey");

            entity.ToTable("mst_account_head", "press_db");

            entity.HasIndex(e => e.IsActive, "idx_account_head_active");

            entity.HasIndex(e => e.ParentAccountId, "idx_account_head_parent");

            entity.HasIndex(e => e.AccountType, "idx_account_head_type");

            entity.HasIndex(e => e.AccountCode, "mst_account_head_account_code_key").IsUnique();

            entity.Property(e => e.AccountHeadId).HasColumnName("account_head_id");
            entity.Property(e => e.AccountCode)
                .HasMaxLength(20)
                .HasColumnName("account_code");
            entity.Property(e => e.AccountName)
                .HasMaxLength(200)
                .HasColumnName("account_name");
            entity.Property(e => e.AccountType)
                .HasMaxLength(20)
                .HasColumnName("account_type");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsGroup)
                .HasDefaultValue(false)
                .HasColumnName("is_group");
            entity.Property(e => e.IsPartyAccount)
                .HasDefaultValue(false)
                .HasColumnName("is_party_account");
            entity.Property(e => e.LevelNo)
                .HasDefaultValue(0)
                .HasColumnName("level_no");
            entity.Property(e => e.OpeningBalance)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("opening_balance");
            entity.Property(e => e.OpeningType)
                .HasMaxLength(10)
                .HasDefaultValueSql("'DR'::character varying")
                .HasColumnName("opening_type");
            entity.Property(e => e.ParentAccountId).HasColumnName("parent_account_id");
            entity.Property(e => e.SortOrder)
                .HasDefaultValue(0)
                .HasColumnName("sort_order");

            entity.HasOne(d => d.ParentAccount).WithMany(p => p.InverseParentAccount)
                .HasForeignKey(d => d.ParentAccountId)
                .HasConstraintName("fk_account_parent");
        });

        modelBuilder.Entity<MstApprovalLevel>(entity =>
        {
            entity.HasKey(e => e.Approvallevelid).HasName("mst_approval_level_pkey");

            entity.ToTable("mst_approval_level", "press_db");

            entity.Property(e => e.Approvallevelid).HasColumnName("approvallevelid");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.Levelname)
                .HasMaxLength(50)
                .HasColumnName("levelname");
            entity.Property(e => e.Sequenceno).HasColumnName("sequenceno");
        });

        modelBuilder.Entity<MstApprovalType>(entity =>
        {
            entity.HasKey(e => e.Approvaltypeid).HasName("mst_approval_type_pkey");

            entity.ToTable("mst_approval_type", "press_db");

            entity.Property(e => e.Approvaltypeid)
                .ValueGeneratedNever()
                .HasColumnName("approvaltypeid");
            entity.Property(e => e.Approvalcode)
                .HasMaxLength(50)
                .HasColumnName("approvalcode");
            entity.Property(e => e.Approvalname)
                .HasMaxLength(150)
                .HasColumnName("approvalname");
            entity.Property(e => e.Createdby)
                .HasMaxLength(50)
                .HasColumnName("createdby");
            entity.Property(e => e.Createdon)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdon");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.Isactive).HasColumnName("isactive");
            entity.Property(e => e.Isclientapproval).HasColumnName("isclientapproval");
            entity.Property(e => e.Isfinancial).HasColumnName("isfinancial");
            entity.Property(e => e.Ismandatory).HasColumnName("ismandatory");
            entity.Property(e => e.Issystemapproval).HasColumnName("issystemapproval");
        });

        modelBuilder.Entity<MstBankAccount>(entity =>
        {
            entity.HasKey(e => e.BankAccountId).HasName("mst_bank_account_pkey");

            entity.ToTable("mst_bank_account", "press_db", tb => tb.HasComment("Company bank accounts used for bank receipt, bank payment and bank reconciliation."));

            entity.HasIndex(e => e.IsActive, "idx_bank_account_active");

            entity.HasIndex(e => e.CompanyId, "idx_bank_account_company");

            entity.HasIndex(e => e.CurrencyId, "idx_bank_account_currency");

            entity.HasIndex(e => e.AccountHeadId, "idx_bank_account_head");

            entity.HasIndex(e => e.LocationId, "idx_bank_account_location");

            entity.HasIndex(e => e.AccountCode, "uq_bank_account_code").IsUnique();

            entity.Property(e => e.BankAccountId).HasColumnName("bank_account_id");
            entity.Property(e => e.AccountCode)
                .HasMaxLength(30)
                .HasColumnName("account_code");
            entity.Property(e => e.AccountHeadId).HasColumnName("account_head_id");
            entity.Property(e => e.AccountName)
                .HasMaxLength(200)
                .HasColumnName("account_name");
            entity.Property(e => e.AccountNo)
                .HasMaxLength(50)
                .HasColumnName("account_no");
            entity.Property(e => e.AccountType)
                .HasMaxLength(30)
                .HasDefaultValueSql("'CURRENT'::character varying")
                .HasColumnName("account_type");
            entity.Property(e => e.BankName)
                .HasMaxLength(150)
                .HasColumnName("bank_name");
            entity.Property(e => e.BranchName)
                .HasMaxLength(150)
                .HasColumnName("branch_name");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.CurrentBalance)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("current_balance");
            entity.Property(e => e.IfscCode)
                .HasMaxLength(20)
                .HasColumnName("ifsc_code");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDefault)
                .HasDefaultValue(false)
                .HasColumnName("is_default");
            entity.Property(e => e.LastReconciledBalance)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("last_reconciled_balance");
            entity.Property(e => e.LastReconciledOn).HasColumnName("last_reconciled_on");
            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.MicrCode)
                .HasMaxLength(20)
                .HasColumnName("micr_code");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.OpeningBalance)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("opening_balance");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.SwiftCode)
                .HasMaxLength(20)
                .HasColumnName("swift_code");

            entity.HasOne(d => d.AccountHead).WithMany(p => p.MstBankAccounts)
                .HasForeignKey(d => d.AccountHeadId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_bank_account_head");

            entity.HasOne(d => d.Company).WithMany(p => p.MstBankAccounts)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_bank_account_company");

            entity.HasOne(d => d.Currency).WithMany(p => p.MstBankAccounts)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_bank_account_currency");

            entity.HasOne(d => d.Location).WithMany(p => p.MstBankAccounts)
                .HasForeignKey(d => d.LocationId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_bank_account_location");
        });

        modelBuilder.Entity<MstBinding>(entity =>
        {
            entity.HasKey(e => e.BindingId).HasName("mst_binding_pkey");

            entity.ToTable("mst_binding", "press_db");

            entity.HasIndex(e => e.BindingCode, "mst_binding_binding_code_key").IsUnique();

            entity.Property(e => e.BindingId).HasColumnName("binding_id");
            entity.Property(e => e.BindingCategory)
                .HasMaxLength(50)
                .HasColumnName("binding_category");
            entity.Property(e => e.BindingCode)
                .HasMaxLength(50)
                .HasColumnName("binding_code");
            entity.Property(e => e.BindingName)
                .HasMaxLength(150)
                .HasColumnName("binding_name");
            entity.Property(e => e.BindingType)
                .HasMaxLength(50)
                .HasColumnName("binding_type");
            entity.Property(e => e.ChangeoverTimeMin).HasColumnName("changeover_time_min");
            entity.Property(e => e.CostPerBook)
                .HasPrecision(10, 2)
                .HasColumnName("cost_per_book");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.LabourCostPerHour)
                .HasPrecision(10, 2)
                .HasColumnName("labour_cost_per_hour");
            entity.Property(e => e.MachineRequired)
                .HasDefaultValue(true)
                .HasColumnName("machine_required");
            entity.Property(e => e.ManpowerRequired).HasColumnName("manpower_required");
            entity.Property(e => e.ManualAllowed)
                .HasDefaultValue(false)
                .HasColumnName("manual_allowed");
            entity.Property(e => e.MaxBookThicknessMm)
                .HasPrecision(6, 2)
                .HasColumnName("max_book_thickness_mm");
            entity.Property(e => e.MaxGsm).HasColumnName("max_gsm");
            entity.Property(e => e.MaxPages).HasColumnName("max_pages");
            entity.Property(e => e.MaxSpeedPerHour).HasColumnName("max_speed_per_hour");
            entity.Property(e => e.MinGsm).HasColumnName("min_gsm");
            entity.Property(e => e.MinPages).HasColumnName("min_pages");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.SetupCost)
                .HasPrecision(10, 2)
                .HasColumnName("setup_cost");
            entity.Property(e => e.SetupTimeMin).HasColumnName("setup_time_min");
            entity.Property(e => e.SpeedUnit)
                .HasMaxLength(30)
                .HasColumnName("speed_unit");
            entity.Property(e => e.SupportedJobTypes)
                .HasMaxLength(200)
                .HasColumnName("supported_job_types");
        });

        modelBuilder.Entity<MstBrand>(entity =>
        {
            entity.HasKey(e => e.BrandId).HasName("mst_brand_pkey");

            entity.ToTable("mst_brand", "press_db");

            entity.HasIndex(e => e.BrandCode, "mst_brand_brand_code_key").IsUnique();

            entity.Property(e => e.BrandId).HasColumnName("brand_id");
            entity.Property(e => e.BrandCode)
                .HasMaxLength(50)
                .HasColumnName("brand_code");
            entity.Property(e => e.BrandName)
                .HasMaxLength(150)
                .HasColumnName("brand_name");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.ManufacturerName)
                .HasMaxLength(150)
                .HasColumnName("manufacturer_name");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.Website)
                .HasMaxLength(255)
                .HasColumnName("website");
        });

        modelBuilder.Entity<MstChemical>(entity =>
        {
            entity.HasKey(e => e.ChemicalCode).HasName("mst_chemical_pkey");

            entity.ToTable("mst_chemical", "press_db");

            entity.HasIndex(e => e.IsActive, "idx_mst_chemical_active");

            entity.HasIndex(e => e.ChemicalCategory, "idx_mst_chemical_category");

            entity.HasIndex(e => e.ChemicalCode, "idx_mst_chemical_code");

            entity.Property(e => e.ChemicalCode)
                .HasMaxLength(30)
                .HasColumnName("chemical_code");
            entity.Property(e => e.ApplicationArea)
                .HasMaxLength(100)
                .HasColumnName("application_area");
            entity.Property(e => e.AvgConsumptionPerHr)
                .HasPrecision(10, 3)
                .HasColumnName("avg_consumption_per_hr");
            entity.Property(e => e.Brand)
                .HasMaxLength(100)
                .HasColumnName("brand");
            entity.Property(e => e.ChemicalCategory)
                .HasMaxLength(50)
                .HasColumnName("chemical_category");
            entity.Property(e => e.ChemicalName)
                .HasMaxLength(150)
                .HasColumnName("chemical_name");
            entity.Property(e => e.ChemicalType)
                .HasMaxLength(50)
                .HasColumnName("chemical_type");
            entity.Property(e => e.CompatibleMachineType)
                .HasMaxLength(100)
                .HasColumnName("compatible_machine_type");
            entity.Property(e => e.CompatibleProcess)
                .HasMaxLength(50)
                .HasColumnName("compatible_process");
            entity.Property(e => e.ConductivityRange)
                .HasMaxLength(50)
                .HasColumnName("conductivity_range");
            entity.Property(e => e.ConsumptionUnit)
                .HasMaxLength(30)
                .HasColumnName("consumption_unit");
            entity.Property(e => e.CurrentStock)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("current_stock");
            entity.Property(e => e.DilutionRatio)
                .HasMaxLength(50)
                .HasColumnName("dilution_ratio");
            entity.Property(e => e.GstRate)
                .HasPrecision(5, 2)
                .HasDefaultValueSql("18.00")
                .HasColumnName("gst_rate");
            entity.Property(e => e.Hazardous)
                .HasDefaultValue(false)
                .HasColumnName("hazardous");
            entity.Property(e => e.HourlyCost)
                .HasPrecision(10, 2)
                .HasColumnName("hourly_cost");
            entity.Property(e => e.HsnCode)
                .HasMaxLength(10)
                .HasColumnName("hsn_code");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.LastPurchaseDate).HasColumnName("last_purchase_date");
            entity.Property(e => e.LastPurchaseRate)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("last_purchase_rate");
            entity.Property(e => e.LeadTimeDays)
                .HasDefaultValue(0)
                .HasColumnName("lead_time_days");
            entity.Property(e => e.Manufacturer)
                .HasMaxLength(100)
                .HasColumnName("manufacturer");
            entity.Property(e => e.MinOrderQty)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("min_order_qty");
            entity.Property(e => e.PhValueRange)
                .HasMaxLength(30)
                .HasColumnName("ph_value_range");
            entity.Property(e => e.ProcessStage)
                .HasMaxLength(50)
                .HasColumnName("process_stage");
            entity.Property(e => e.RatePerUnit)
                .HasPrecision(10, 2)
                .HasColumnName("rate_per_unit");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.ReorderLevel)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("reorder_level");
            entity.Property(e => e.ShelfLifeMonths).HasColumnName("shelf_life_months");
            entity.Property(e => e.StorageCondition)
                .HasMaxLength(100)
                .HasColumnName("storage_condition");
            entity.Property(e => e.Uom)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Ltr'::character varying")
                .HasColumnName("uom");
        });

        modelBuilder.Entity<MstCity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("mst_city_pkey");

            entity.ToTable("mst_city", "press_db");

            entity.HasIndex(e => e.IsActive, "idx_city_active");

            entity.HasIndex(e => e.StateId, "idx_city_state_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.DeliveryZone)
                .HasMaxLength(50)
                .HasColumnName("delivery_zone");
            entity.Property(e => e.DistrictName)
                .HasMaxLength(100)
                .HasColumnName("district_name");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(false)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDefault)
                .HasDefaultValue(false)
                .HasColumnName("is_default");
            entity.Property(e => e.Latitude)
                .HasPrecision(10, 6)
                .HasColumnName("latitude");
            entity.Property(e => e.Longitude)
                .HasPrecision(10, 6)
                .HasColumnName("longitude");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(50)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Pincode)
                .HasMaxLength(10)
                .HasColumnName("pincode");
            entity.Property(e => e.StateId).HasColumnName("state_id");
            entity.Property(e => e.TalukaName)
                .HasMaxLength(100)
                .HasColumnName("taluka_name");
            entity.Property(e => e.TransportHub)
                .HasDefaultValue(false)
                .HasColumnName("transport_hub");

            entity.HasOne(d => d.State).WithMany(p => p.MstCities)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_city_state");
        });

        modelBuilder.Entity<MstCompany>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("mst_company_pkey");

            entity.ToTable("mst_company", "press_db");

            entity.HasIndex(e => e.IsActive, "idx_company_active");

            entity.HasIndex(e => e.BaseCurrencyId, "idx_company_base_currency");

            entity.HasIndex(e => e.CityId, "idx_company_city");

            entity.HasIndex(e => e.CountryId, "idx_company_country");

            entity.HasIndex(e => e.CurrencyId, "idx_company_currency");

            entity.HasIndex(e => e.ParentCompanyId, "idx_company_parent");

            entity.HasIndex(e => e.StateId, "idx_company_state");

            entity.HasIndex(e => e.DefaultTaxCategoryId, "idx_company_tax_category");

            entity.HasIndex(e => e.Code, "mst_company_code_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccountNo)
                .HasMaxLength(50)
                .HasColumnName("account_no");
            entity.Property(e => e.AddressLine1)
                .HasMaxLength(200)
                .HasColumnName("address_line1");
            entity.Property(e => e.AddressLine2)
                .HasMaxLength(200)
                .HasColumnName("address_line2");
            entity.Property(e => e.AltContactNo)
                .HasMaxLength(20)
                .HasColumnName("alt_contact_no");
            entity.Property(e => e.BankName)
                .HasMaxLength(150)
                .HasColumnName("bank_name");
            entity.Property(e => e.BaseCurrencyId).HasColumnName("base_currency_id");
            entity.Property(e => e.BooksStartDate).HasColumnName("books_start_date");
            entity.Property(e => e.BranchName)
                .HasMaxLength(150)
                .HasColumnName("branch_name");
            entity.Property(e => e.CinNo)
                .HasMaxLength(30)
                .HasColumnName("cin_no");
            entity.Property(e => e.CityId).HasColumnName("city_id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.ContactNo)
                .HasMaxLength(20)
                .HasColumnName("contact_no");
            entity.Property(e => e.ContactPerson)
                .HasMaxLength(150)
                .HasColumnName("contact_person");
            entity.Property(e => e.CountryId).HasColumnName("country_id");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.DefaultTaxCategoryId).HasColumnName("default_tax_category_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.EmailId)
                .HasMaxLength(150)
                .HasColumnName("email_id");
            entity.Property(e => e.FinYearEnd).HasColumnName("fin_year_end");
            entity.Property(e => e.FinYearStart).HasColumnName("fin_year_start");
            entity.Property(e => e.Gstin)
                .HasMaxLength(20)
                .HasColumnName("gstin");
            entity.Property(e => e.IecCode)
                .HasMaxLength(20)
                .HasColumnName("iec_code");
            entity.Property(e => e.IfscCode)
                .HasMaxLength(20)
                .HasColumnName("ifsc_code");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsGroupCompany)
                .HasDefaultValue(false)
                .HasColumnName("is_group_company");
            entity.Property(e => e.LegalName)
                .HasMaxLength(250)
                .HasColumnName("legal_name");
            entity.Property(e => e.LogoUrl)
                .HasMaxLength(255)
                .HasColumnName("logo_url");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.MsmeNo)
                .HasMaxLength(30)
                .HasColumnName("msme_no");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.PanNo)
                .HasMaxLength(20)
                .HasColumnName("pan_no");
            entity.Property(e => e.ParentCompanyId).HasColumnName("parent_company_id");
            entity.Property(e => e.Pincode)
                .HasMaxLength(15)
                .HasColumnName("pincode");
            entity.Property(e => e.PrintFooterText)
                .HasMaxLength(255)
                .HasColumnName("print_footer_text");
            entity.Property(e => e.PrintHeaderText)
                .HasMaxLength(255)
                .HasColumnName("print_header_text");
            entity.Property(e => e.RegistrationNo)
                .HasMaxLength(100)
                .HasColumnName("registration_no");
            entity.Property(e => e.ShortName)
                .HasMaxLength(50)
                .HasColumnName("short_name");
            entity.Property(e => e.StateId).HasColumnName("state_id");
            entity.Property(e => e.SwiftCode)
                .HasMaxLength(20)
                .HasColumnName("swift_code");
            entity.Property(e => e.TanNo)
                .HasMaxLength(20)
                .HasColumnName("tan_no");
            entity.Property(e => e.TaxRegime)
                .HasMaxLength(50)
                .HasColumnName("tax_regime");
            entity.Property(e => e.Website)
                .HasMaxLength(150)
                .HasColumnName("website");

            entity.HasOne(d => d.BaseCurrency).WithMany(p => p.MstCompanyBaseCurrencies)
                .HasForeignKey(d => d.BaseCurrencyId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_company_base_currency");

            entity.HasOne(d => d.City).WithMany(p => p.MstCompanies)
                .HasForeignKey(d => d.CityId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_company_city");

            entity.HasOne(d => d.Country).WithMany(p => p.MstCompanies)
                .HasForeignKey(d => d.CountryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_company_country");

            entity.HasOne(d => d.Currency).WithMany(p => p.MstCompanyCurrencies)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_company_currency");

            entity.HasOne(d => d.DefaultTaxCategory).WithMany(p => p.MstCompanies)
                .HasForeignKey(d => d.DefaultTaxCategoryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_company_tax_category");

            entity.HasOne(d => d.ParentCompany).WithMany(p => p.InverseParentCompany)
                .HasForeignKey(d => d.ParentCompanyId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_company_parent");

            entity.HasOne(d => d.State).WithMany(p => p.MstCompanies)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_company_state");
        });

        modelBuilder.Entity<MstCostCenter>(entity =>
        {
            entity.HasKey(e => e.CostCenterId).HasName("mst_cost_center_pkey");

            entity.ToTable("mst_cost_center", "press_db", tb => tb.HasComment("Cost center master for departmental/project-wise expense tracking. Referenced by journal lines, expense items, invoice items."));

            entity.HasIndex(e => e.CenterCode, "uq_cost_center_code").IsUnique();

            entity.Property(e => e.CostCenterId).HasColumnName("cost_center_id");
            entity.Property(e => e.CenterCode)
                .HasMaxLength(30)
                .HasColumnName("center_code");
            entity.Property(e => e.CenterName)
                .HasMaxLength(150)
                .HasColumnName("center_name");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.ParentCenterId).HasColumnName("parent_center_id");

            entity.HasOne(d => d.Department).WithMany(p => p.MstCostCenters)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("fk_cc_dept");

            entity.HasOne(d => d.ParentCenter).WithMany(p => p.InverseParentCenter)
                .HasForeignKey(d => d.ParentCenterId)
                .HasConstraintName("fk_cc_parent");
        });

        modelBuilder.Entity<MstCostComponent>(entity =>
        {
            entity.HasKey(e => e.CostComponentId).HasName("mst_cost_component_pkey");

            entity.ToTable("mst_cost_component", "press_db");

            entity.HasIndex(e => e.ComponentCategory, "idx_cost_comp_category");

            entity.HasIndex(e => e.ApplicableLevel, "idx_cost_comp_level");

            entity.HasIndex(e => e.TaxCategoryId, "idx_cost_comp_tax_cat");

            entity.HasIndex(e => e.ComponentCode, "mst_cost_component_component_code_key").IsUnique();

            entity.Property(e => e.CostComponentId).HasColumnName("cost_component_id");
            entity.Property(e => e.ApplicableLevel)
                .HasMaxLength(30)
                .HasColumnName("applicable_level");
            entity.Property(e => e.ApplicableToPart)
                .HasDefaultValue(false)
                .HasColumnName("applicable_to_part");
            entity.Property(e => e.ApplicableToProduct)
                .HasDefaultValue(false)
                .HasColumnName("applicable_to_product");
            entity.Property(e => e.BaseUom)
                .HasMaxLength(20)
                .HasColumnName("base_uom");
            entity.Property(e => e.CalculationType)
                .HasMaxLength(30)
                .HasColumnName("calculation_type");
            entity.Property(e => e.ComponentCategory)
                .HasMaxLength(50)
                .HasColumnName("component_category");
            entity.Property(e => e.ComponentCode)
                .HasMaxLength(50)
                .HasColumnName("component_code");
            entity.Property(e => e.ComponentName)
                .HasMaxLength(150)
                .HasColumnName("component_name");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.DefaultRate)
                .HasPrecision(12, 2)
                .HasColumnName("default_rate");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsMandatory)
                .HasDefaultValue(false)
                .HasColumnName("is_mandatory");
            entity.Property(e => e.IsOutsourceAllowed)
                .HasDefaultValue(true)
                .HasColumnName("is_outsource_allowed");
            entity.Property(e => e.IsTaxable)
                .HasDefaultValue(true)
                .HasColumnName("is_taxable");
            entity.Property(e => e.MaxRate)
                .HasPrecision(12, 2)
                .HasColumnName("max_rate");
            entity.Property(e => e.MinRate)
                .HasPrecision(12, 2)
                .HasColumnName("min_rate");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(50)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.SequenceNo).HasColumnName("sequence_no");
            entity.Property(e => e.TaxCategoryId).HasColumnName("tax_category_id");

            entity.HasOne(d => d.TaxCategory).WithMany(p => p.MstCostComponents)
                .HasForeignKey(d => d.TaxCategoryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_cost_component_tax_category");
        });

        modelBuilder.Entity<MstCountry>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("mst_country_pkey");

            entity.ToTable("mst_country", "press_db");

            entity.HasIndex(e => e.Code, "uq_country_code").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.CurrencyCode)
                .HasMaxLength(10)
                .HasColumnName("currency_code");
            entity.Property(e => e.CurrencyName)
                .HasMaxLength(50)
                .HasColumnName("currency_name");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDefault)
                .HasDefaultValue(false)
                .HasColumnName("is_default");
            entity.Property(e => e.IsoAlpha2)
                .HasMaxLength(2)
                .HasColumnName("iso_alpha2");
            entity.Property(e => e.IsoAlpha3)
                .HasMaxLength(3)
                .HasColumnName("iso_alpha3");
            entity.Property(e => e.IsoNumeric)
                .HasMaxLength(3)
                .HasColumnName("iso_numeric");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(50)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Nationality)
                .HasMaxLength(100)
                .HasColumnName("nationality");
            entity.Property(e => e.PhoneCode)
                .HasMaxLength(10)
                .HasColumnName("phone_code");
            entity.Property(e => e.Timezone)
                .HasMaxLength(50)
                .HasColumnName("timezone");
        });

        modelBuilder.Entity<MstCurrency>(entity =>
        {
            entity.HasKey(e => e.CurrencyId).HasName("mst_currency_pkey");

            entity.ToTable("mst_currency", "press_db");

            entity.HasIndex(e => e.BaseCurrency, "idx_currency_base").HasFilter("(base_currency = true)");

            entity.HasIndex(e => e.CountryId, "idx_currency_country");

            entity.HasIndex(e => e.CurrencyCode, "mst_currency_currency_code_key").IsUnique();

            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.BaseCurrency)
                .HasDefaultValue(false)
                .HasColumnName("base_currency");
            entity.Property(e => e.CountryId).HasColumnName("country_id");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.CurrencyCode)
                .HasMaxLength(10)
                .HasColumnName("currency_code");
            entity.Property(e => e.CurrencyName)
                .HasMaxLength(100)
                .HasColumnName("currency_name");
            entity.Property(e => e.DecimalPlaces)
                .HasDefaultValue(2)
                .HasColumnName("decimal_places");
            entity.Property(e => e.DecimalSeparator)
                .HasMaxLength(5)
                .HasDefaultValueSql("'.'::character varying")
                .HasColumnName("decimal_separator");
            entity.Property(e => e.EffectiveFrom)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("effective_from");
            entity.Property(e => e.EffectiveTo).HasColumnName("effective_to");
            entity.Property(e => e.ExchangeRate)
                .HasPrecision(18, 6)
                .HasDefaultValueSql("1.000000")
                .HasColumnName("exchange_rate");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.RateSource)
                .HasMaxLength(100)
                .HasColumnName("rate_source");
            entity.Property(e => e.Symbol)
                .HasMaxLength(10)
                .HasColumnName("symbol");
            entity.Property(e => e.SymbolPosition)
                .HasMaxLength(10)
                .HasDefaultValueSql("'Left'::character varying")
                .HasColumnName("symbol_position");
            entity.Property(e => e.ThousandSeparator)
                .HasMaxLength(5)
                .HasDefaultValueSql("','::character varying")
                .HasColumnName("thousand_separator");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(100)
                .HasColumnName("updated_by");
            entity.Property(e => e.UpdatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_on");
        });

        modelBuilder.Entity<MstCustomer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("mst_customer_pkey");

            entity.ToTable("mst_customer", "press_db");

            entity.HasIndex(e => e.IsActive, "idx_customer_active");

            entity.HasIndex(e => e.CustomerGroup, "idx_customer_group");

            entity.HasIndex(e => e.PartyId, "idx_customer_party_id");

            entity.HasIndex(e => e.PaymentTerms, "idx_customer_payment_terms");

            entity.HasIndex(e => e.CustomerType, "idx_customer_type");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AvailableCreditLimitAmt)
                .HasPrecision(18, 2)
                .HasColumnName("available_credit_limit_amt");
            entity.Property(e => e.CustomerGroup).HasColumnName("customer_group");
            entity.Property(e => e.CustomerType).HasColumnName("customer_type");
            entity.Property(e => e.DueDateBase).HasColumnName("due_date_base");
            entity.Property(e => e.HoldCreditLimitAmt)
                .HasPrecision(18, 2)
                .HasColumnName("hold_credit_limit_amt");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Language).HasColumnName("language");
            entity.Property(e => e.MaxCreditLimit)
                .HasPrecision(18, 2)
                .HasColumnName("max_credit_limit");
            entity.Property(e => e.PartyId).HasColumnName("party_id");
            entity.Property(e => e.PaymentTerms).HasColumnName("payment_terms");
            entity.Property(e => e.Salesperson)
                .HasMaxLength(50)
                .HasColumnName("salesperson");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.SuspendedCreditLimitAmt)
                .HasPrecision(18, 2)
                .HasColumnName("suspended_credit_limit_amt");
            entity.Property(e => e.TotalUtilizedCreditLimitAmt)
                .HasPrecision(18, 2)
                .HasColumnName("total_utilized_credit_limit_amt");

            entity.HasOne(d => d.CustomerGroupNavigation).WithMany(p => p.MstCustomers)
                .HasForeignKey(d => d.CustomerGroup)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_customer_group");

            entity.HasOne(d => d.CustomerTypeNavigation).WithMany(p => p.MstCustomers)
                .HasForeignKey(d => d.CustomerType)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_customer_type");

            entity.HasOne(d => d.Party).WithMany(p => p.MstCustomers)
                .HasForeignKey(d => d.PartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_customer_party");

            entity.HasOne(d => d.PaymentTermsNavigation).WithMany(p => p.MstCustomers)
                .HasForeignKey(d => d.PaymentTerms)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_customer_payment_terms");
        });

        modelBuilder.Entity<MstCustomerGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("mst_customer_group_pkey");

            entity.ToTable("mst_customer_group", "press_db");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<MstCustomerPaymentTerm>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("mst_customer_payment_term_pkey");

            entity.ToTable("mst_customer_payment_term", "press_db");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.Txt)
                .HasMaxLength(10)
                .HasColumnName("txt");
            entity.Property(e => e.Val).HasColumnName("val");
        });

        modelBuilder.Entity<MstCustomerType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("mst_customer_type_pkey");

            entity.ToTable("mst_customer_type", "press_db");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<MstDepartment>(entity =>
        {
            entity.HasKey(e => e.DeptId).HasName("mst_department_pkey");

            entity.ToTable("mst_department", "press_db");

            entity.HasIndex(e => e.DeptCode, "mst_department_dept_code_key").IsUnique();

            entity.Property(e => e.DeptId)
                .HasIdentityOptions(1001L, null, null, null, null, null)
                .HasColumnName("dept_id");
            entity.Property(e => e.DeptCode)
                .HasMaxLength(20)
                .HasColumnName("dept_code");
            entity.Property(e => e.DeptName)
                .HasMaxLength(100)
                .HasColumnName("dept_name");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsProduction)
                .HasDefaultValue(false)
                .HasColumnName("is_production");
            entity.Property(e => e.ParentDeptCode)
                .HasMaxLength(20)
                .HasColumnName("parent_dept_code");
            entity.Property(e => e.Remarks)
                .HasMaxLength(200)
                .HasColumnName("remarks");
        });

        modelBuilder.Entity<MstDepartmentRoleMap>(entity =>
        {
            entity.HasKey(e => e.MapId).HasName("mst_department_role_map_pkey");

            entity.ToTable("mst_department_role_map", "press_db");

            entity.Property(e => e.MapId).HasColumnName("map_id");
            entity.Property(e => e.DeptId).HasColumnName("dept_id");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsPrimary)
                .HasDefaultValue(true)
                .HasColumnName("is_primary");
            entity.Property(e => e.Roleid).HasColumnName("roleid");
        });

        modelBuilder.Entity<MstDesignation>(entity =>
        {
            entity.HasKey(e => e.DesignationId).HasName("mst_designation_pkey");

            entity.ToTable("mst_designation", "press_db");

            entity.HasIndex(e => e.DesignationName, "uq_designation_name").IsUnique();

            entity.Property(e => e.DesignationId)
                .HasIdentityOptions(1001L, null, null, null, null, null)
                .HasColumnName("designation_id");
            entity.Property(e => e.DesignationName)
                .HasMaxLength(100)
                .HasColumnName("designation_name");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.LevelNo).HasColumnName("level_no");
        });

        modelBuilder.Entity<MstDesigning>(entity =>
        {
            entity.HasKey(e => e.DesigningId).HasName("mst_designing_pkey");

            entity.ToTable("mst_designing", "press_db");

            entity.HasIndex(e => e.DesignCode, "mst_designing_design_code_key").IsUnique();

            entity.Property(e => e.DesigningId).HasColumnName("designing_id");
            entity.Property(e => e.AvgTimeHours)
                .HasPrecision(6, 2)
                .HasColumnName("avg_time_hours");
            entity.Property(e => e.BaseCost)
                .HasPrecision(12, 2)
                .HasColumnName("base_cost");
            entity.Property(e => e.ColorMode)
                .HasMaxLength(50)
                .HasColumnName("color_mode");
            entity.Property(e => e.CostUnit)
                .HasMaxLength(50)
                .HasColumnName("cost_unit");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.DesignCategory)
                .HasMaxLength(50)
                .HasColumnName("design_category");
            entity.Property(e => e.DesignCode)
                .HasMaxLength(50)
                .HasColumnName("design_code");
            entity.Property(e => e.DesignName)
                .HasMaxLength(150)
                .HasColumnName("design_name");
            entity.Property(e => e.DesignType)
                .HasMaxLength(50)
                .HasColumnName("design_type");
            entity.Property(e => e.FileFormat)
                .HasMaxLength(100)
                .HasColumnName("file_format");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsCostApplicable)
                .HasDefaultValue(true)
                .HasColumnName("is_cost_applicable");
            entity.Property(e => e.IsDesignByParty)
                .HasDefaultValue(false)
                .HasColumnName("is_design_by_party");
            entity.Property(e => e.IsPlateByParty)
                .HasDefaultValue(false)
                .HasColumnName("is_plate_by_party");
            entity.Property(e => e.JobTypesSupported)
                .HasMaxLength(200)
                .HasColumnName("job_types_supported");
            entity.Property(e => e.ManpowerRequired).HasColumnName("manpower_required");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.RevisionAllowed)
                .HasDefaultValue(1)
                .HasColumnName("revision_allowed");
            entity.Property(e => e.ReworkChargePerRevision)
                .HasPrecision(10, 2)
                .HasColumnName("rework_charge_per_revision");
            entity.Property(e => e.SoftwareUsed)
                .HasMaxLength(150)
                .HasColumnName("software_used");
        });

        modelBuilder.Entity<MstDirection>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("mst_direction_pkey");

            entity.ToTable("mst_direction", "press_db", tb => tb.HasComment("Lookup for tax/accounting flow direction. id=1 Output (Payable), id=2 Input (ITC). Referenced by trn_tax_ledger.direction."));

            entity.HasIndex(e => e.Name, "uq_direction_name").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("Primary key. 1=Output Tax, 2=Input Tax — used directly as FK value in trn_tax_ledger.")
                .HasColumnName("id");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasComment("TRUE = active.")
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasComment("Human-readable direction label.")
                .HasColumnName("name");
        });

        modelBuilder.Entity<MstDocumentSequence>(entity =>
        {
            entity.HasKey(e => e.SequenceId).HasName("mst_document_sequence_pkey");

            entity.ToTable("mst_document_sequence", "press_db");

            entity.HasIndex(e => e.ProcessCode, "mst_document_sequence_process_code_key").IsUnique();

            entity.Property(e => e.SequenceId).HasColumnName("sequence_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.CurrentNumber)
                .HasDefaultValue(0L)
                .HasColumnName("current_number");
            entity.Property(e => e.FinancialYear)
                .HasMaxLength(20)
                .HasColumnName("financial_year");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.PaddingLength)
                .HasDefaultValue(5)
                .HasColumnName("padding_length");
            entity.Property(e => e.Prefix)
                .HasMaxLength(20)
                .HasColumnName("prefix");
            entity.Property(e => e.ProcessCode)
                .HasMaxLength(50)
                .HasColumnName("process_code");
            entity.Property(e => e.ProcessName)
                .HasMaxLength(100)
                .HasColumnName("process_name");
            entity.Property(e => e.Suffix)
                .HasMaxLength(20)
                .HasColumnName("suffix");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<MstEmployee>(entity =>
        {
            entity.HasKey(e => e.EmployeeId).HasName("mst_employee_pkey");

            entity.ToTable("mst_employee", "press_db");

            entity.HasIndex(e => e.IsActive, "idx_employee_active");

            entity.HasIndex(e => e.CompanyId, "idx_employee_company");

            entity.HasIndex(e => e.DeptId, "idx_employee_dept");

            entity.HasIndex(e => e.DesignationId, "idx_employee_designation");

            entity.HasIndex(e => e.LocationId, "idx_employee_location");

            entity.HasIndex(e => e.ReportingEmployeeId, "idx_employee_reporting");

            entity.HasIndex(e => e.ShiftTypeId, "idx_employee_shift");

            entity.HasIndex(e => e.EmployeeTypeId, "idx_employee_type");

            entity.HasIndex(e => e.EmpCode, "mst_employee_emp_code_key").IsUnique();

            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.AadharNo)
                .HasMaxLength(20)
                .HasColumnName("aadhar_no");
            entity.Property(e => e.BankAccountNo)
                .HasMaxLength(30)
                .HasColumnName("bank_account_no");
            entity.Property(e => e.BankName)
                .HasMaxLength(150)
                .HasColumnName("bank_name");
            entity.Property(e => e.BranchName)
                .HasMaxLength(150)
                .HasColumnName("branch_name");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
            entity.Property(e => e.DateOfJoining).HasColumnName("date_of_joining");
            entity.Property(e => e.DateOfRelieving).HasColumnName("date_of_relieving");
            entity.Property(e => e.DeptId).HasColumnName("dept_id");
            entity.Property(e => e.DesignationId).HasColumnName("designation_id");
            entity.Property(e => e.Email1)
                .HasMaxLength(150)
                .HasColumnName("email_1");
            entity.Property(e => e.Email2)
                .HasMaxLength(150)
                .HasColumnName("email_2");
            entity.Property(e => e.EmpCode)
                .HasMaxLength(30)
                .HasColumnName("emp_code");
            entity.Property(e => e.EmployeeTypeId).HasColumnName("employee_type_id");
            entity.Property(e => e.EsiNo)
                .HasMaxLength(30)
                .HasColumnName("esi_no");
            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .HasColumnName("first_name");
            entity.Property(e => e.IfscCode)
                .HasMaxLength(20)
                .HasColumnName("ifsc_code");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .HasColumnName("last_name");
            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.MiddleName)
                .HasMaxLength(100)
                .HasColumnName("middle_name");
            entity.Property(e => e.MobileNo1)
                .HasMaxLength(20)
                .HasColumnName("mobile_no_1");
            entity.Property(e => e.MobileNo2)
                .HasMaxLength(20)
                .HasColumnName("mobile_no_2");
            entity.Property(e => e.PanNo)
                .HasMaxLength(20)
                .HasColumnName("pan_no");
            entity.Property(e => e.PfNo)
                .HasMaxLength(30)
                .HasColumnName("pf_no");
            entity.Property(e => e.PhoneNo)
                .HasMaxLength(20)
                .HasColumnName("phone_no");
            entity.Property(e => e.ReportingEmployeeId).HasColumnName("reporting_employee_id");
            entity.Property(e => e.ShiftTypeId).HasColumnName("shift_type_id");

            entity.HasOne(d => d.Company).WithMany(p => p.MstEmployees)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_employee_company");

            entity.HasOne(d => d.Dept).WithMany(p => p.MstEmployees)
                .HasForeignKey(d => d.DeptId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_employee_department");

            entity.HasOne(d => d.Designation).WithMany(p => p.MstEmployees)
                .HasForeignKey(d => d.DesignationId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_employee_designation");

            entity.HasOne(d => d.EmployeeType).WithMany(p => p.MstEmployees)
                .HasForeignKey(d => d.EmployeeTypeId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_employee_type");

            entity.HasOne(d => d.Location).WithMany(p => p.MstEmployees)
                .HasForeignKey(d => d.LocationId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_employee_location");

            entity.HasOne(d => d.ReportingEmployee).WithMany(p => p.InverseReportingEmployee)
                .HasForeignKey(d => d.ReportingEmployeeId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_employee_reporting");

            entity.HasOne(d => d.ShiftType).WithMany(p => p.MstEmployees)
                .HasForeignKey(d => d.ShiftTypeId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_employee_shift_type");
        });

        modelBuilder.Entity<MstEmployeeMachineMapping>(entity =>
        {
            entity.HasKey(e => e.MappingId).HasName("mst_employee_machine_mapping_pkey");

            entity.ToTable("mst_employee_machine_mapping", "press_db");

            entity.HasIndex(e => e.IsAuthorized, "idx_emp_machine_authorized");

            entity.HasIndex(e => e.EmployeeId, "idx_emp_machine_employee");

            entity.HasIndex(e => e.MachineId, "idx_emp_machine_machine");

            entity.HasIndex(e => e.RoleCode, "idx_emp_machine_role");

            entity.Property(e => e.MappingId).HasColumnName("mapping_id");
            entity.Property(e => e.CertificationDate).HasColumnName("certification_date");
            entity.Property(e => e.CertificationNo)
                .HasMaxLength(100)
                .HasColumnName("certification_no");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.EmployeeCode)
                .HasMaxLength(50)
                .HasColumnName("employee_code");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.EmployeeName)
                .HasMaxLength(150)
                .HasColumnName("employee_name");
            entity.Property(e => e.ExperienceYears)
                .HasPrecision(5, 2)
                .HasColumnName("experience_years");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsAuthorized)
                .HasDefaultValue(true)
                .HasColumnName("is_authorized");
            entity.Property(e => e.IsPrimaryMachine)
                .HasDefaultValue(false)
                .HasColumnName("is_primary_machine");
            entity.Property(e => e.MachineCode)
                .HasMaxLength(50)
                .HasColumnName("machine_code");
            entity.Property(e => e.MachineId).HasColumnName("machine_id");
            entity.Property(e => e.MachineName)
                .HasMaxLength(150)
                .HasColumnName("machine_name");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(50)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.RoleCode)
                .HasMaxLength(50)
                .HasColumnName("role_code");
            entity.Property(e => e.RoleName)
                .HasMaxLength(100)
                .HasColumnName("role_name");
            entity.Property(e => e.SkillLevel)
                .HasMaxLength(30)
                .HasColumnName("skill_level");

            entity.HasOne(d => d.Employee).WithMany(p => p.MstEmployeeMachineMappings)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_emp_machine_employee");

            entity.HasOne(d => d.Machine).WithMany(p => p.MstEmployeeMachineMappings)
                .HasForeignKey(d => d.MachineId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_emp_machine_machine");
        });

        modelBuilder.Entity<MstEmployeeType>(entity =>
        {
            entity.HasKey(e => e.EmployeeTypeId).HasName("mst_employee_type_pkey");

            entity.ToTable("mst_employee_type", "press_db");

            entity.HasIndex(e => e.TypeCode, "mst_employee_type_type_code_key").IsUnique();

            entity.Property(e => e.EmployeeTypeId).HasColumnName("employee_type_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.TypeCode)
                .HasMaxLength(30)
                .HasColumnName("type_code");
            entity.Property(e => e.TypeName)
                .HasMaxLength(100)
                .HasColumnName("type_name");
        });

        modelBuilder.Entity<MstExecutionType>(entity =>
        {
            entity.HasKey(e => e.Executiontypeid).HasName("mst_execution_type_pkey");

            entity.ToTable("mst_execution_type", "press_db");

            entity.Property(e => e.Executiontypeid)
                .ValueGeneratedNever()
                .HasColumnName("executiontypeid");
            entity.Property(e => e.Executioncode)
                .HasMaxLength(20)
                .HasColumnName("executioncode");
            entity.Property(e => e.Executionname)
                .HasMaxLength(50)
                .HasColumnName("executionname");
        });

        modelBuilder.Entity<MstExpenseCategory>(entity =>
        {
            entity.HasKey(e => e.ExpenseCategoryId).HasName("mst_expense_category_pkey");

            entity.ToTable("mst_expense_category", "press_db", tb => tb.HasComment("Master table for expense categories: Office, Travel, Utilities, Repairs, Rent, Salary, Transport, Printing, Misc. Maps to account head for GL posting."));

            entity.HasIndex(e => e.CategoryCode, "uq_expense_category_code").IsUnique();

            entity.Property(e => e.ExpenseCategoryId).HasColumnName("expense_category_id");
            entity.Property(e => e.AccountHeadId).HasColumnName("account_head_id");
            entity.Property(e => e.ApprovalLimit)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("approval_limit");
            entity.Property(e => e.CategoryCode)
                .HasMaxLength(30)
                .HasColumnName("category_code");
            entity.Property(e => e.CategoryName)
                .HasMaxLength(150)
                .HasColumnName("category_name");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsReimbursable)
                .HasDefaultValue(false)
                .HasColumnName("is_reimbursable");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.ParentCategoryId).HasColumnName("parent_category_id");
            entity.Property(e => e.RequiresApproval)
                .HasDefaultValue(true)
                .HasColumnName("requires_approval");
            entity.Property(e => e.TaxCategoryId).HasColumnName("tax_category_id");

            entity.HasOne(d => d.AccountHead).WithMany(p => p.MstExpenseCategories)
                .HasForeignKey(d => d.AccountHeadId)
                .HasConstraintName("fk_expcat_account");

            entity.HasOne(d => d.ParentCategory).WithMany(p => p.InverseParentCategory)
                .HasForeignKey(d => d.ParentCategoryId)
                .HasConstraintName("fk_expcat_parent");
        });

        modelBuilder.Entity<MstFinancialYear>(entity =>
        {
            entity.HasKey(e => e.FinYearId).HasName("mst_financial_year_pkey");

            entity.ToTable("mst_financial_year", "press_db", tb => tb.HasComment("Financial year periods per company. Used for period-wise reporting, GST returns, and ledger closing."));

            entity.HasIndex(e => e.CompanyId, "idx_fin_year_company");

            entity.HasIndex(e => e.IsCurrent, "idx_fin_year_current").HasFilter("(is_current = true)");

            entity.HasIndex(e => new { e.CompanyId, e.FinYearCode }, "uq_fin_year").IsUnique();

            entity.Property(e => e.FinYearId).HasColumnName("fin_year_id");
            entity.Property(e => e.ClosedBy)
                .HasMaxLength(100)
                .HasColumnName("closed_by");
            entity.Property(e => e.ClosedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("closed_on");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.FinYearCode)
                .HasMaxLength(20)
                .HasColumnName("fin_year_code");
            entity.Property(e => e.IsClosed)
                .HasDefaultValue(false)
                .HasColumnName("is_closed");
            entity.Property(e => e.IsCurrent)
                .HasDefaultValue(false)
                .HasColumnName("is_current");
            entity.Property(e => e.OpeningDone)
                .HasDefaultValue(false)
                .HasColumnName("opening_done");
            entity.Property(e => e.StartDate).HasColumnName("start_date");

            entity.HasOne(d => d.Company).WithMany(p => p.MstFinancialYears)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_fin_year_company");
        });

        modelBuilder.Entity<MstFinishing>(entity =>
        {
            entity.HasKey(e => e.FinishingId).HasName("mst_finishing_pkey");

            entity.ToTable("mst_finishing", "press_db");

            entity.HasIndex(e => e.FinishingCode, "mst_finishing_finishing_code_key").IsUnique();

            entity.Property(e => e.FinishingId).HasColumnName("finishing_id");
            entity.Property(e => e.ChangeoverTimeMin).HasColumnName("changeover_time_min");
            entity.Property(e => e.CostPerSheet)
                .HasPrecision(10, 2)
                .HasColumnName("cost_per_sheet");
            entity.Property(e => e.FinishingCategory)
                .HasMaxLength(50)
                .HasColumnName("finishing_category");
            entity.Property(e => e.FinishingCode)
                .HasMaxLength(50)
                .HasColumnName("finishing_code");
            entity.Property(e => e.FinishingName)
                .HasMaxLength(150)
                .HasColumnName("finishing_name");
            entity.Property(e => e.FinishingType)
                .HasMaxLength(50)
                .HasColumnName("finishing_type");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.LabourCostPerHour)
                .HasPrecision(10, 2)
                .HasColumnName("labour_cost_per_hour");
            entity.Property(e => e.MachineRequired)
                .HasDefaultValue(true)
                .HasColumnName("machine_required");
            entity.Property(e => e.ManpowerRequired).HasColumnName("manpower_required");
            entity.Property(e => e.ManualAllowed)
                .HasDefaultValue(false)
                .HasColumnName("manual_allowed");
            entity.Property(e => e.MaxGsm).HasColumnName("max_gsm");
            entity.Property(e => e.MaxSheetLengthMm).HasColumnName("max_sheet_length_mm");
            entity.Property(e => e.MaxSheetWidthMm).HasColumnName("max_sheet_width_mm");
            entity.Property(e => e.MaxSpeedPerHour).HasColumnName("max_speed_per_hour");
            entity.Property(e => e.MinGsm).HasColumnName("min_gsm");
            entity.Property(e => e.MinSheetLengthMm).HasColumnName("min_sheet_length_mm");
            entity.Property(e => e.MinSheetWidthMm).HasColumnName("min_sheet_width_mm");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.SetupCost)
                .HasPrecision(10, 2)
                .HasColumnName("setup_cost");
            entity.Property(e => e.SetupTimeMin).HasColumnName("setup_time_min");
            entity.Property(e => e.SpeedUnit)
                .HasMaxLength(30)
                .HasColumnName("speed_unit");
            entity.Property(e => e.SupportedJobTypes)
                .HasMaxLength(200)
                .HasColumnName("supported_job_types");
            entity.Property(e => e.SupportedProducts)
                .HasMaxLength(200)
                .HasColumnName("supported_products");
        });

        modelBuilder.Entity<MstHsnSacCode>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("mst_hsn_sac_code_pkey");

            entity.ToTable("mst_hsn_sac_code", "press_db", tb => tb.HasComment("Master table for HSN (goods) and SAC (services) codes for GST compliance. Stores tax rates and classification details."));

            entity.HasIndex(e => e.IsActive, "idx_hsn_sac_active");

            entity.HasIndex(e => new { e.Category, e.IsActive }, "idx_hsn_sac_category");

            entity.HasIndex(e => new { e.CodeType, e.IsActive }, "idx_hsn_sac_code_type");

            entity.HasIndex(e => new { e.IsCommonlyUsed, e.IsActive }, "idx_hsn_sac_commonly_used").HasFilter("(is_commonly_used = true)");

            entity.HasIndex(e => new { e.EffectiveFrom, e.EffectiveTo }, "idx_hsn_sac_effective");

            entity.HasIndex(e => e.DefaultGstRate, "idx_hsn_sac_gst_rate");

            entity.HasIndex(e => e.ParentCode, "idx_hsn_sac_parent");

            entity.HasIndex(e => e.TaxCategoryId, "idx_hsn_sac_tax_category");

            entity.HasIndex(e => e.Code, "uq_hsn_sac_code").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("Primary key, auto-generated.")
                .HasColumnName("id");
            entity.Property(e => e.Category)
                .HasMaxLength(100)
                .HasComment("Business category (Paper Products, Printing Services, Inks, etc.).")
                .HasColumnName("category");
            entity.Property(e => e.CessRate)
                .HasPrecision(6, 3)
                .HasDefaultValueSql("0")
                .HasComment("Additional cess rate if applicable.")
                .HasColumnName("cess_rate");
            entity.Property(e => e.CgstRate)
                .HasPrecision(6, 3)
                .HasComment("Central GST rate (for intra-state transactions).")
                .HasColumnName("cgst_rate");
            entity.Property(e => e.Code)
                .HasMaxLength(20)
                .HasComment("HSN or SAC code (unique identifier).")
                .HasColumnName("code");
            entity.Property(e => e.CodeType)
                .HasMaxLength(10)
                .HasComment("Type of code: HSN for goods, SAC for services.")
                .HasColumnName("code_type");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.DefaultGstRate)
                .HasPrecision(6, 3)
                .HasComment("Default total GST rate percentage (e.g., 18.000 for 18% GST).")
                .HasColumnName("default_gst_rate");
            entity.Property(e => e.Description)
                .HasComment("Detailed description of the goods/service.")
                .HasColumnName("description");
            entity.Property(e => e.EffectiveFrom)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("effective_from");
            entity.Property(e => e.EffectiveTo).HasColumnName("effective_to");
            entity.Property(e => e.IgstRate)
                .HasPrecision(6, 3)
                .HasComment("Integrated GST rate (for inter-state transactions).")
                .HasColumnName("igst_rate");
            entity.Property(e => e.IndustryType)
                .HasMaxLength(50)
                .HasColumnName("industry_type");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsCommonlyUsed)
                .HasDefaultValue(false)
                .HasComment("Flag for frequently used codes in printing press business.")
                .HasColumnName("is_commonly_used");
            entity.Property(e => e.IsExempt)
                .HasDefaultValue(false)
                .HasColumnName("is_exempt");
            entity.Property(e => e.IsNilRated)
                .HasDefaultValue(false)
                .HasColumnName("is_nil_rated");
            entity.Property(e => e.LevelNo)
                .HasDefaultValue((short)1)
                .HasComment("Classification level: 1=Chapter, 2=Heading, 3=Sub-heading, 4=Tariff item.")
                .HasColumnName("level_no");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.ParentCode)
                .HasMaxLength(20)
                .HasComment("Parent HSN/SAC code for hierarchical classification.")
                .HasColumnName("parent_code");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.SgstRate)
                .HasPrecision(6, 3)
                .HasComment("State GST rate (for intra-state transactions).")
                .HasColumnName("sgst_rate");
            entity.Property(e => e.TaxCategoryId)
                .HasComment("FK to mst_tax_category for default tax slab.")
                .HasColumnName("tax_category_id");
            entity.Property(e => e.UnitOfMeasure)
                .HasMaxLength(50)
                .HasColumnName("unit_of_measure");

            entity.HasOne(d => d.TaxCategory).WithMany(p => p.MstHsnSacCodes)
                .HasForeignKey(d => d.TaxCategoryId)
                .HasConstraintName("fk_hsn_sac_tax_category");
        });

        modelBuilder.Entity<MstInk>(entity =>
        {
            entity.HasKey(e => e.InkCode).HasName("mst_ink_pkey");

            entity.ToTable("mst_ink", "press_db");

            entity.HasIndex(e => e.IsActive, "idx_mst_ink_active");

            entity.HasIndex(e => e.InkCode, "idx_mst_ink_code");

            entity.Property(e => e.InkCode)
                .HasMaxLength(50)
                .HasColumnName("ink_code");
            entity.Property(e => e.AutoSelectPriority).HasColumnName("auto_select_priority");
            entity.Property(e => e.ColorName)
                .HasMaxLength(50)
                .HasColumnName("color_name");
            entity.Property(e => e.ColorType)
                .HasMaxLength(50)
                .HasColumnName("color_type");
            entity.Property(e => e.CompatibleMachineType)
                .HasMaxLength(100)
                .HasColumnName("compatible_machine_type");
            entity.Property(e => e.CompatibleProcess)
                .HasMaxLength(100)
                .HasColumnName("compatible_process");
            entity.Property(e => e.ConsumptionGsm)
                .HasPrecision(8, 2)
                .HasColumnName("consumption_gsm");
            entity.Property(e => e.CostPerKg)
                .HasPrecision(10, 2)
                .HasColumnName("cost_per_kg");
            entity.Property(e => e.CoverageSqMPerKg)
                .HasPrecision(10, 2)
                .HasColumnName("coverage_sq_m_per_kg");
            entity.Property(e => e.CurrentStock)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("current_stock");
            entity.Property(e => e.DryingType)
                .HasMaxLength(50)
                .HasColumnName("drying_type");
            entity.Property(e => e.GlossLevel)
                .HasMaxLength(50)
                .HasColumnName("gloss_level");
            entity.Property(e => e.GstRate)
                .HasPrecision(5, 2)
                .HasDefaultValueSql("18.00")
                .HasColumnName("gst_rate");
            entity.Property(e => e.HsnCode)
                .HasMaxLength(10)
                .HasColumnName("hsn_code");
            entity.Property(e => e.InkCategory)
                .HasMaxLength(50)
                .HasColumnName("ink_category");
            entity.Property(e => e.InkName)
                .HasMaxLength(150)
                .HasColumnName("ink_name");
            entity.Property(e => e.InkSeries)
                .HasMaxLength(100)
                .HasColumnName("ink_series");
            entity.Property(e => e.InkType)
                .HasMaxLength(50)
                .HasColumnName("ink_type");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.LastPurchaseDate).HasColumnName("last_purchase_date");
            entity.Property(e => e.LastPurchaseRate)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("last_purchase_rate");
            entity.Property(e => e.LeadTimeDays)
                .HasDefaultValue(0)
                .HasColumnName("lead_time_days");
            entity.Property(e => e.Manufacturer)
                .HasMaxLength(100)
                .HasColumnName("manufacturer");
            entity.Property(e => e.MinOrderQty)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("min_order_qty");
            entity.Property(e => e.PantoneCode)
                .HasMaxLength(50)
                .HasColumnName("pantone_code");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.ReorderLevel)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("reorder_level");
            entity.Property(e => e.RubResistance)
                .HasMaxLength(50)
                .HasColumnName("rub_resistance");
            entity.Property(e => e.ShelfLifeMonths).HasColumnName("shelf_life_months");
            entity.Property(e => e.StorageCondition)
                .HasMaxLength(100)
                .HasColumnName("storage_condition");
            entity.Property(e => e.SupportedJobTypes)
                .HasMaxLength(200)
                .HasColumnName("supported_job_types");
            entity.Property(e => e.Uom)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Kg'::character varying")
                .HasColumnName("uom");
            entity.Property(e => e.WastagePercent)
                .HasPrecision(5, 2)
                .HasColumnName("wastage_percent");
        });

        modelBuilder.Entity<MstJobCategory>(entity =>
        {
            entity.HasKey(e => e.JobCategoryId).HasName("mst_job_category_pkey");

            entity.ToTable("mst_job_category", "press_db");

            entity.Property(e => e.JobCategoryId).HasColumnName("job_category_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.JobCategoryName)
                .HasMaxLength(100)
                .HasColumnName("job_category_name");
        });

        modelBuilder.Entity<MstJobType>(entity =>
        {
            entity.HasKey(e => e.Jobtypeid).HasName("mst_job_type_pkey");

            entity.ToTable("mst_job_type", "press_db");

            entity.HasIndex(e => e.Jobtypecode, "mst_job_type_jobtypecode_key").IsUnique();

            entity.Property(e => e.Jobtypeid)
                .ValueGeneratedNever()
                .HasColumnName("jobtypeid");
            entity.Property(e => e.Allowadvancepayment)
                .HasDefaultValue(true)
                .HasColumnName("allowadvancepayment");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Defaultendprocesscode)
                .HasMaxLength(30)
                .HasColumnName("defaultendprocesscode");
            entity.Property(e => e.Defaultstartprocesscode)
                .HasMaxLength(30)
                .HasColumnName("defaultstartprocesscode");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.Isbindingrequired)
                .HasDefaultValue(false)
                .HasColumnName("isbindingrequired");
            entity.Property(e => e.Isctprequired)
                .HasDefaultValue(false)
                .HasColumnName("isctprequired");
            entity.Property(e => e.Iscustomermaterial)
                .HasDefaultValue(false)
                .HasColumnName("iscustomermaterial");
            entity.Property(e => e.Isdesignrequired)
                .HasDefaultValue(false)
                .HasColumnName("isdesignrequired");
            entity.Property(e => e.Isdtprequired)
                .HasDefaultValue(false)
                .HasColumnName("isdtprequired");
            entity.Property(e => e.Isfinishingrequired)
                .HasDefaultValue(false)
                .HasColumnName("isfinishingrequired");
            entity.Property(e => e.Isfullprocess)
                .HasDefaultValue(false)
                .HasColumnName("isfullprocess");
            entity.Property(e => e.Isinhousematerial)
                .HasDefaultValue(true)
                .HasColumnName("isinhousematerial");
            entity.Property(e => e.Isoutsourcejob)
                .HasDefaultValue(false)
                .HasColumnName("isoutsourcejob");
            entity.Property(e => e.Isprintingrequired)
                .HasDefaultValue(false)
                .HasColumnName("isprintingrequired");
            entity.Property(e => e.Issingleprocess)
                .HasDefaultValue(false)
                .HasColumnName("issingleprocess");
            entity.Property(e => e.Jobtypecode)
                .HasMaxLength(30)
                .HasColumnName("jobtypecode");
            entity.Property(e => e.Jobtypename)
                .HasMaxLength(100)
                .HasColumnName("jobtypename");
            entity.Property(e => e.Printingmode)
                .HasMaxLength(20)
                .HasColumnName("printingmode");
            entity.Property(e => e.Requirecostingapproval)
                .HasDefaultValue(false)
                .HasColumnName("requirecostingapproval");
        });

        modelBuilder.Entity<MstLocation>(entity =>
        {
            entity.HasKey(e => e.LocationId).HasName("mst_location_pkey");

            entity.ToTable("mst_location", "press_db");

            entity.HasIndex(e => e.IsActive, "idx_location_active");

            entity.HasIndex(e => e.CityId, "idx_location_city");

            entity.HasIndex(e => e.CompanyId, "idx_location_company");

            entity.HasIndex(e => e.CountryId, "idx_location_country");

            entity.HasIndex(e => e.ParentLocationId, "idx_location_parent");

            entity.HasIndex(e => e.StateId, "idx_location_state");

            entity.HasIndex(e => e.LocationCode, "mst_location_location_code_key").IsUnique();

            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.AddressLine1)
                .HasMaxLength(255)
                .HasColumnName("address_line1");
            entity.Property(e => e.AddressLine2)
                .HasMaxLength(255)
                .HasColumnName("address_line2");
            entity.Property(e => e.CityId).HasColumnName("city_id");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.ContactEmail)
                .HasMaxLength(150)
                .HasColumnName("contact_email");
            entity.Property(e => e.ContactPerson)
                .HasMaxLength(100)
                .HasColumnName("contact_person");
            entity.Property(e => e.ContactPhone)
                .HasMaxLength(20)
                .HasColumnName("contact_phone");
            entity.Property(e => e.CountryId).HasColumnName("country_id");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsPurchasePoint)
                .HasDefaultValue(false)
                .HasColumnName("is_purchase_point");
            entity.Property(e => e.IsSalesPoint)
                .HasDefaultValue(false)
                .HasColumnName("is_sales_point");
            entity.Property(e => e.IsStorageAllowed)
                .HasDefaultValue(true)
                .HasColumnName("is_storage_allowed");
            entity.Property(e => e.Latitude)
                .HasPrecision(10, 6)
                .HasColumnName("latitude");
            entity.Property(e => e.LocationCode)
                .HasMaxLength(50)
                .HasColumnName("location_code");
            entity.Property(e => e.LocationName)
                .HasMaxLength(150)
                .HasColumnName("location_name");
            entity.Property(e => e.LocationType)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Warehouse'::character varying")
                .HasColumnName("location_type");
            entity.Property(e => e.Longitude)
                .HasPrecision(10, 6)
                .HasColumnName("longitude");
            entity.Property(e => e.ParentLocationId).HasColumnName("parent_location_id");
            entity.Property(e => e.PostalCode)
                .HasMaxLength(20)
                .HasColumnName("postal_code");
            entity.Property(e => e.StateId).HasColumnName("state_id");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(100)
                .HasColumnName("updated_by");
            entity.Property(e => e.UpdatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_on");

            entity.HasOne(d => d.City).WithMany(p => p.MstLocations)
                .HasForeignKey(d => d.CityId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_location_city");

            entity.HasOne(d => d.Company).WithMany(p => p.MstLocations)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_location_company");

            entity.HasOne(d => d.Country).WithMany(p => p.MstLocations)
                .HasForeignKey(d => d.CountryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_location_country");

            entity.HasOne(d => d.ParentLocation).WithMany(p => p.InverseParentLocation)
                .HasForeignKey(d => d.ParentLocationId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_location_parent");

            entity.HasOne(d => d.State).WithMany(p => p.MstLocations)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_location_state");
        });

        modelBuilder.Entity<MstLocationType>(entity =>
        {
            entity.HasKey(e => e.LocationTypeId).HasName("mst_location_type_pkey");

            entity.ToTable("mst_location_type", "press_db");

            entity.HasIndex(e => e.Name, "mst_location_type_name_key").IsUnique();

            entity.Property(e => e.LocationTypeId).HasColumnName("location_type_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<MstMachine>(entity =>
        {
            entity.HasKey(e => e.MachineId).HasName("mst_machine_pkey");

            entity.ToTable("mst_machine", "press_db");

            entity.HasIndex(e => e.IsActive, "idx_machine_active");

            entity.HasIndex(e => e.MachineCategory, "idx_machine_category");

            entity.HasIndex(e => e.DepartmentCode, "idx_machine_dept");

            entity.HasIndex(e => e.MachineCode, "mst_machine_machine_code_key").IsUnique();

            entity.Property(e => e.MachineId).HasColumnName("machine_id");
            entity.Property(e => e.AirRequired).HasColumnName("air_required");
            entity.Property(e => e.AutoSelectPriority).HasColumnName("auto_select_priority");
            entity.Property(e => e.AvgDowntimeHours).HasColumnName("avg_downtime_hours");
            entity.Property(e => e.ChangeoverTimeMin).HasColumnName("changeover_time_min");
            entity.Property(e => e.ChangeoverTimeMinutes).HasColumnName("changeover_time_minutes");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.DepartmentCode)
                .HasMaxLength(10)
                .HasColumnName("department_code");
            entity.Property(e => e.HourlyRunningCost)
                .HasPrecision(10, 2)
                .HasColumnName("hourly_running_cost");
            entity.Property(e => e.InstallationYear).HasColumnName("installation_year");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsProduction)
                .HasDefaultValue(true)
                .HasColumnName("is_production");
            entity.Property(e => e.IsProductionMachine)
                .HasDefaultValue(true)
                .HasColumnName("is_production_machine");
            entity.Property(e => e.LabourCostPerHour)
                .HasPrecision(10, 2)
                .HasColumnName("labour_cost_per_hour");
            entity.Property(e => e.MachineCategory)
                .HasMaxLength(30)
                .HasColumnName("machine_category");
            entity.Property(e => e.MachineCode)
                .HasMaxLength(30)
                .HasColumnName("machine_code");
            entity.Property(e => e.MachineName)
                .HasMaxLength(150)
                .HasColumnName("machine_name");
            entity.Property(e => e.MachineType)
                .HasMaxLength(50)
                .HasColumnName("machine_type");
            entity.Property(e => e.MaintenanceCycleDays).HasColumnName("maintenance_cycle_days");
            entity.Property(e => e.ManpowerRequired).HasColumnName("manpower_required");
            entity.Property(e => e.Manufacturer)
                .HasMaxLength(100)
                .HasColumnName("manufacturer");
            entity.Property(e => e.MaxColors).HasColumnName("max_colors");
            entity.Property(e => e.MaxGsm).HasColumnName("max_gsm");
            entity.Property(e => e.MaxSheetLengthMm).HasColumnName("max_sheet_length_mm");
            entity.Property(e => e.MaxSheetWidthMm).HasColumnName("max_sheet_width_mm");
            entity.Property(e => e.MaxSpeed).HasColumnName("max_speed");
            entity.Property(e => e.MaxSpeedPerHour).HasColumnName("max_speed_per_hour");
            entity.Property(e => e.MinGsm).HasColumnName("min_gsm");
            entity.Property(e => e.MinSheetLengthMm).HasColumnName("min_sheet_length_mm");
            entity.Property(e => e.MinSheetWidthMm).HasColumnName("min_sheet_width_mm");
            entity.Property(e => e.ModelNo)
                .HasMaxLength(100)
                .HasColumnName("model_no");
            entity.Property(e => e.PowerConsumptionKw)
                .HasPrecision(6, 2)
                .HasColumnName("power_consumption_kw");
            entity.Property(e => e.PowerCostPerHour)
                .HasPrecision(10, 2)
                .HasColumnName("power_cost_per_hour");
            entity.Property(e => e.PrintingSide)
                .HasMaxLength(20)
                .HasColumnName("printing_side");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.SetupCost)
                .HasPrecision(10, 2)
                .HasColumnName("setup_cost");
            entity.Property(e => e.SetupTimeMin).HasColumnName("setup_time_min");
            entity.Property(e => e.SetupTimeMinutes).HasColumnName("setup_time_minutes");
            entity.Property(e => e.SpeedUnit)
                .HasMaxLength(30)
                .HasColumnName("speed_unit");
            entity.Property(e => e.SupportedJobTypes).HasColumnName("supported_job_types");
        });

        modelBuilder.Entity<MstMachineMaintenance>(entity =>
        {
            entity.HasKey(e => e.MaintenanceId).HasName("mst_machine_maintenance_pkey");

            entity.ToTable("mst_machine_maintenance", "press_db", tb => tb.HasComment("Stores preventive maintenance schedule details for machines"));

            entity.HasIndex(e => e.MachineId, "idx_maint_machine");

            entity.HasIndex(e => e.NextDueDate, "idx_maint_next_due");

            entity.Property(e => e.MaintenanceId)
                .HasComment("Primary key for maintenance record")
                .HasColumnName("maintenance_id");
            entity.Property(e => e.BreakdownEndTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("breakdown_end_time");
            entity.Property(e => e.BreakdownStartTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("breakdown_start_time");
            entity.Property(e => e.CompletionDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("completion_date");
            entity.Property(e => e.DowntimeMinutes)
                .HasPrecision(10, 2)
                .HasColumnName("downtime_minutes");
            entity.Property(e => e.EstimatedCost)
                .HasPrecision(10, 2)
                .HasComment("Estimated cost for scheduled maintenance")
                .HasColumnName("estimated_cost");
            entity.Property(e => e.FrequencyDays)
                .HasComment("Maintenance frequency in days")
                .HasColumnName("frequency_days");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasComment("Indicates whether the maintenance schedule is active")
                .HasColumnName("is_active");
            entity.Property(e => e.LastMaintenanceDate)
                .HasComment("Date when last maintenance was performed")
                .HasColumnName("last_maintenance_date");
            entity.Property(e => e.MachineId)
                .HasComment("Reference to machine requiring maintenance")
                .HasColumnName("machine_id");
            entity.Property(e => e.MaintenanceType)
                .HasMaxLength(30)
                .HasComment("Type of maintenance (Preventive, Calibration, AMC, Routine)")
                .HasColumnName("maintenance_type");
            entity.Property(e => e.NextDueDate)
                .HasComment("Next scheduled maintenance date")
                .HasColumnName("next_due_date");
            entity.Property(e => e.Remarks)
                .HasComment("Additional notes related to maintenance")
                .HasColumnName("remarks");
            entity.Property(e => e.RepairStatus)
                .HasMaxLength(30)
                .HasDefaultValueSql("'Pending'::character varying")
                .HasColumnName("repair_status");
            entity.Property(e => e.VendorName)
                .HasMaxLength(100)
                .HasComment("Vendor or service provider performing maintenance")
                .HasColumnName("vendor_name");

            entity.HasOne(d => d.Machine).WithMany(p => p.MstMachineMaintenances)
                .HasForeignKey(d => d.MachineId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_maintenance_machine");
        });

        modelBuilder.Entity<MstMachineSelectionRule>(entity =>
        {
            entity.HasKey(e => e.RuleId).HasName("mst_machine_selection_rule_pkey");

            entity.ToTable("mst_machine_selection_rule", "press_db");

            entity.Property(e => e.RuleId).HasColumnName("rule_id");
            entity.Property(e => e.ColorRequired).HasColumnName("color_required");
            entity.Property(e => e.DepartmentCode)
                .HasMaxLength(10)
                .HasColumnName("department_code");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.JobType)
                .HasMaxLength(50)
                .HasColumnName("job_type");
            entity.Property(e => e.MaxGsm).HasColumnName("max_gsm");
            entity.Property(e => e.MaxLengthMm).HasColumnName("max_length_mm");
            entity.Property(e => e.MaxWidthMm).HasColumnName("max_width_mm");
            entity.Property(e => e.MinGsm).HasColumnName("min_gsm");
            entity.Property(e => e.MinLengthMm).HasColumnName("min_length_mm");
            entity.Property(e => e.MinWidthMm).HasColumnName("min_width_mm");
            entity.Property(e => e.PrintingSide)
                .HasMaxLength(20)
                .HasColumnName("printing_side");
            entity.Property(e => e.Priority).HasColumnName("priority");
        });

        modelBuilder.Entity<MstMaterial>(entity =>
        {
            entity.HasKey(e => e.MaterialCode).HasName("mst_material_pkey");

            entity.ToTable("mst_material", "press_db");

            entity.Property(e => e.MaterialCode)
                .HasMaxLength(30)
                .HasColumnName("material_code");
            entity.Property(e => e.AvgConsumptionPerJob)
                .HasPrecision(10, 3)
                .HasColumnName("avg_consumption_per_job");
            entity.Property(e => e.CompatibleJobTypes)
                .HasMaxLength(150)
                .HasColumnName("compatible_job_types");
            entity.Property(e => e.CompatibleProcess)
                .HasMaxLength(50)
                .HasColumnName("compatible_process");
            entity.Property(e => e.CostPerJob)
                .HasPrecision(10, 2)
                .HasColumnName("cost_per_job");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsConsumable)
                .HasDefaultValue(true)
                .HasColumnName("is_consumable");
            entity.Property(e => e.MaterialCategory)
                .HasMaxLength(50)
                .HasColumnName("material_category");
            entity.Property(e => e.MaterialName)
                .HasMaxLength(150)
                .HasColumnName("material_name");
            entity.Property(e => e.MaterialSubCategory)
                .HasMaxLength(50)
                .HasColumnName("material_sub_category");
            entity.Property(e => e.MaxStockLevel)
                .HasPrecision(10, 2)
                .HasColumnName("max_stock_level");
            entity.Property(e => e.ProcessStage)
                .HasMaxLength(50)
                .HasColumnName("process_stage");
            entity.Property(e => e.RatePerUnit)
                .HasPrecision(10, 2)
                .HasColumnName("rate_per_unit");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.ReorderLevel)
                .HasPrecision(10, 2)
                .HasColumnName("reorder_level");
            entity.Property(e => e.ShelfLifeMonths).HasColumnName("shelf_life_months");
            entity.Property(e => e.StorageLocation)
                .HasMaxLength(50)
                .HasColumnName("storage_location");
            entity.Property(e => e.SupplierName)
                .HasMaxLength(100)
                .HasColumnName("supplier_name");
            entity.Property(e => e.UnitOfMeasure)
                .HasMaxLength(30)
                .HasColumnName("unit_of_measure");
            entity.Property(e => e.UsageArea)
                .HasMaxLength(100)
                .HasColumnName("usage_area");
        });

        modelBuilder.Entity<MstMenu>(entity =>
        {
            entity.HasKey(e => e.Menuid).HasName("mst_menu_pkey");

            entity.ToTable("mst_menu", "press_db");

            entity.HasIndex(e => e.Isactive, "idx_menu_active");

            entity.HasIndex(e => e.ModuleId, "idx_menu_module");

            entity.HasIndex(e => e.Parentmenuid, "idx_menu_parent");

            entity.HasIndex(e => e.Menucode, "mst_menu_menucode_key").IsUnique();

            entity.Property(e => e.Menuid).HasColumnName("menuid");
            entity.Property(e => e.Badgeclass)
                .HasMaxLength(50)
                .HasColumnName("badgeclass");
            entity.Property(e => e.Badgetext)
                .HasMaxLength(50)
                .HasColumnName("badgetext");
            entity.Property(e => e.Displayorder).HasColumnName("displayorder");
            entity.Property(e => e.Hasdividerbefore)
                .HasDefaultValue(false)
                .HasColumnName("hasdividerbefore");
            entity.Property(e => e.Icon)
                .HasMaxLength(50)
                .HasColumnName("icon");
            entity.Property(e => e.Iconsvg).HasColumnName("iconsvg");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.Ismobile)
                .HasDefaultValue(false)
                .HasColumnName("ismobile");
            entity.Property(e => e.Issectionheader)
                .HasDefaultValue(false)
                .HasColumnName("issectionheader");
            entity.Property(e => e.Isweb)
                .HasDefaultValue(true)
                .HasColumnName("isweb");
            entity.Property(e => e.Menucode)
                .HasMaxLength(50)
                .HasColumnName("menucode");
            entity.Property(e => e.Menulevel)
                .HasDefaultValue(1)
                .HasColumnName("menulevel");
            entity.Property(e => e.Menuname)
                .HasMaxLength(100)
                .HasColumnName("menuname");
            entity.Property(e => e.ModuleId).HasColumnName("module_id");
            entity.Property(e => e.Parentmenuid).HasColumnName("parentmenuid");
            entity.Property(e => e.Routeurl)
                .HasMaxLength(200)
                .HasColumnName("routeurl");
            entity.Property(e => e.Sectionname)
                .HasMaxLength(100)
                .HasColumnName("sectionname");

            entity.HasOne(d => d.Module).WithMany(p => p.MstMenus)
                .HasForeignKey(d => d.ModuleId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_menu_module");

            entity.HasOne(d => d.Parentmenu).WithMany(p => p.InverseParentmenu)
                .HasForeignKey(d => d.Parentmenuid)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_menu_parent");
        });

        modelBuilder.Entity<MstModule>(entity =>
        {
            entity.HasKey(e => e.ModuleId).HasName("pk_mst_module");

            entity.ToTable("mst_module", "press_db", tb => tb.HasComment("Master table for ERP navigation modules and menu items. Supports hierarchical parent-child structure (up to 3 levels), section headers, badges, inline SVG icons, and separate mobile/web visibility flags. Seeded from mst_menu.csv — original menuid values are preserved as module_id."));

            entity.HasIndex(e => e.IsActive, "idx_module_active");

            entity.HasIndex(e => e.DisplayOrder, "idx_module_display");

            entity.HasIndex(e => e.ParentModuleId, "idx_module_parent");

            entity.HasIndex(e => e.ModuleCode, "uq_mst_module_code").IsUnique();

            entity.Property(e => e.ModuleId)
                .HasComment("Primary key. Preserved from original mst_menu.menuid values (1-1217). New rows after seeding use sequence press_db.mst_module_module_id_seq starting at 2000.")
                .HasColumnName("module_id");
            entity.Property(e => e.BadgeClass)
                .HasMaxLength(100)
                .HasComment("Tabler/Bootstrap CSS classes for the badge. e.g. badge badge-sm bg-red-lt, badge badge-sm bg-blue-lt.")
                .HasColumnName("badge_class");
            entity.Property(e => e.BadgeText)
                .HasMaxLength(30)
                .HasComment("Short badge label displayed on the menu item. e.g. New, Beta, Soon.")
                .HasColumnName("badge_text");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .HasComment("Username or user ID of the person who created this record.")
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("Timestamp when this record was created. Defaults to CURRENT_TIMESTAMP.")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.DisplayOrder)
                .HasDefaultValue(0)
                .HasComment("Render order within the parent group. Lower values appear first in sidebar.")
                .HasColumnName("display_order");
            entity.Property(e => e.HasDividerBefore)
                .HasDefaultValue(false)
                .HasComment("If true, a horizontal <hr> divider is rendered above this menu item in the sidebar.")
                .HasColumnName("has_divider_before");
            entity.Property(e => e.Icon)
                .HasMaxLength(100)
                .HasComment("Tabler Icons icon name (without the ti- prefix). e.g. home, building-bank, package, chart-bar.")
                .HasColumnName("icon");
            entity.Property(e => e.IconSvg)
                .HasComment("Full inline SVG markup for the icon. Used primarily for level-1 root items that require custom branded SVG icons rather than standard Tabler icon names.")
                .HasColumnName("icon_svg");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasComment("Soft enable/disable. Inactive modules are hidden from all navigation.")
                .HasColumnName("is_active");
            entity.Property(e => e.IsMobile)
                .HasDefaultValue(false)
                .HasComment("true = visible in the .NET MAUI mobile app navigation. false = web-only (Blazor).")
                .HasColumnName("is_mobile");
            entity.Property(e => e.IsSectionHeader)
                .HasDefaultValue(false)
                .HasComment("If true, this row renders as a non-clickable section group label in the sidebar (e.g. Sales, CRM, Reports headings within a parent group).")
                .HasColumnName("is_section_header");
            entity.Property(e => e.IsWeb)
                .HasDefaultValue(true)
                .HasComment("true = visible in the Blazor web application navigation.")
                .HasColumnName("is_web");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .HasComment("Username or user ID of the person who last modified this record.")
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasComment("Timestamp of the last modification. NULL if never updated after creation.")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.ModuleCode)
                .HasMaxLength(60)
                .HasComment("Unique business key for the module. Used by application layer for permission checks and role-menu mapping. e.g. DASHBOARD, SALES_CRM, RATE_CALCULATOR.")
                .HasColumnName("module_code");
            entity.Property(e => e.ModuleLevel)
                .HasDefaultValue((short)1)
                .HasComment("1 = root top-level item (section group). 2 = child leaf item under a root. 3 = reserved for future deep nesting.")
                .HasColumnName("module_level");
            entity.Property(e => e.ModuleName)
                .HasMaxLength(150)
                .HasComment("Display name shown in the navigation sidebar. e.g. Dashboard, Sales & CRM.")
                .HasColumnName("module_name");
            entity.Property(e => e.ParentModuleId)
                .HasComment("Self-referencing FK. NULL for root/top-level modules (module_level=1). Points to parent module_id for child items (module_level=2).")
                .HasColumnName("parent_module_id");
            entity.Property(e => e.RouteUrl)
                .HasMaxLength(300)
                .HasComment("Blazor client-side route for navigation. NULL for group header items that have no direct page (e.g. SALES_CRM, PRODUCTION).")
                .HasColumnName("route_url");
            entity.Property(e => e.SectionName)
                .HasMaxLength(100)
                .HasComment("Label shown as a section divider above child items. e.g. Sales, CRM, Reports, Plate Making, Post-Press.")
                .HasColumnName("section_name");

            entity.HasOne(d => d.ParentModule).WithMany(p => p.InverseParentModule)
                .HasForeignKey(d => d.ParentModuleId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_module_parent");
        });

        modelBuilder.Entity<MstNotificationPreference>(entity =>
        {
            entity.HasKey(e => e.PreferenceId).HasName("mst_notification_preference_pkey");

            entity.ToTable("mst_notification_preference", "press_db");

            entity.HasIndex(e => e.UserId, "idx_notif_pref_user");

            entity.HasIndex(e => new { e.UserId, e.Module, e.EventType }, "mst_notification_preference_user_id_module_event_type_key").IsUnique();

            entity.Property(e => e.PreferenceId).HasColumnName("preference_id");
            entity.Property(e => e.ChannelEmail)
                .HasDefaultValue(true)
                .HasColumnName("channel_email");
            entity.Property(e => e.ChannelInApp)
                .HasDefaultValue(true)
                .HasColumnName("channel_in_app");
            entity.Property(e => e.ChannelPush)
                .HasDefaultValue(false)
                .HasColumnName("channel_push");
            entity.Property(e => e.ChannelSms)
                .HasDefaultValue(false)
                .HasColumnName("channel_sms");
            entity.Property(e => e.ChannelWhatsapp)
                .HasDefaultValue(false)
                .HasColumnName("channel_whatsapp");
            entity.Property(e => e.EventType)
                .HasMaxLength(50)
                .HasColumnName("event_type");
            entity.Property(e => e.IsMuted)
                .HasDefaultValue(false)
                .HasColumnName("is_muted");
            entity.Property(e => e.Module)
                .HasMaxLength(50)
                .HasColumnName("module");
            entity.Property(e => e.MutedUntil)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("muted_until");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.MstNotificationPreferences)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("fk_notif_pref_user");
        });

        modelBuilder.Entity<MstNotificationProvider>(entity =>
        {
            entity.HasKey(e => e.ProviderId).HasName("mst_notification_provider_pkey");

            entity.ToTable("mst_notification_provider", "press_db");

            entity.HasIndex(e => e.ProviderName, "mst_notification_provider_provider_name_key").IsUnique();

            entity.Property(e => e.ProviderId).HasColumnName("provider_id");
            entity.Property(e => e.Channel)
                .HasMaxLength(20)
                .HasColumnName("channel");
            entity.Property(e => e.ConfigJson)
                .HasColumnType("jsonb")
                .HasColumnName("config_json");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDefault)
                .HasDefaultValue(false)
                .HasColumnName("is_default");
            entity.Property(e => e.Priority)
                .HasDefaultValue(1)
                .HasColumnName("priority");
            entity.Property(e => e.ProviderName)
                .HasMaxLength(50)
                .HasColumnName("provider_name");
            entity.Property(e => e.ProviderType)
                .HasMaxLength(50)
                .HasColumnName("provider_type");
            entity.Property(e => e.RateLimitPerHour)
                .HasDefaultValue(1000)
                .HasColumnName("rate_limit_per_hour");
            entity.Property(e => e.RateLimitPerMin)
                .HasDefaultValue(60)
                .HasColumnName("rate_limit_per_min");
        });

        modelBuilder.Entity<MstNotificationTemplate>(entity =>
        {
            entity.HasKey(e => e.TemplateId).HasName("mst_notification_template_pkey");

            entity.ToTable("mst_notification_template", "press_db");

            entity.HasIndex(e => e.TemplateCode, "mst_notification_template_template_code_key").IsUnique();

            entity.Property(e => e.TemplateId).HasColumnName("template_id");
            entity.Property(e => e.AiPromptTemplate).HasColumnName("ai_prompt_template");
            entity.Property(e => e.BodyFormat)
                .HasMaxLength(20)
                .HasDefaultValueSql("'HTML'::character varying")
                .HasColumnName("body_format");
            entity.Property(e => e.BodyTemplate).HasColumnName("body_template");
            entity.Property(e => e.Channel)
                .HasMaxLength(20)
                .HasColumnName("channel");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.EventType)
                .HasMaxLength(50)
                .HasColumnName("event_type");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsAiEnabled)
                .HasDefaultValue(false)
                .HasColumnName("is_ai_enabled");
            entity.Property(e => e.ModifiedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.Module)
                .HasMaxLength(50)
                .HasColumnName("module");
            entity.Property(e => e.SubjectTemplate)
                .HasMaxLength(500)
                .HasColumnName("subject_template");
            entity.Property(e => e.TemplateCode)
                .HasMaxLength(50)
                .HasColumnName("template_code");
            entity.Property(e => e.TemplateName)
                .HasMaxLength(150)
                .HasColumnName("template_name");
            entity.Property(e => e.VariablesJson)
                .HasColumnType("jsonb")
                .HasColumnName("variables_json");
        });

        modelBuilder.Entity<MstOtherItem>(entity =>
        {
            entity.HasKey(e => e.ItemId).HasName("mst_other_items_pkey");

            entity.ToTable("mst_other_items", "press_db");

            entity.HasIndex(e => e.IsActive, "idx_mst_other_active");

            entity.HasIndex(e => e.ItemCode, "idx_mst_other_code");

            entity.HasIndex(e => e.IsActive, "idx_mst_other_items_active");

            entity.HasIndex(e => e.ItemCategory, "idx_mst_other_items_category");

            entity.HasIndex(e => e.ItemCode, "mst_other_items_item_code_key").IsUnique();

            entity.Property(e => e.ItemId).HasColumnName("item_id");
            entity.Property(e => e.Brand)
                .HasMaxLength(100)
                .HasColumnName("brand");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.CurrentStock)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("current_stock");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.GstRate)
                .HasPrecision(5, 2)
                .HasDefaultValueSql("18.00")
                .HasColumnName("gst_rate");
            entity.Property(e => e.HsnCode)
                .HasMaxLength(20)
                .HasColumnName("hsn_code");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.ItemCategory)
                .HasMaxLength(50)
                .HasColumnName("item_category");
            entity.Property(e => e.ItemCode)
                .HasMaxLength(30)
                .HasColumnName("item_code");
            entity.Property(e => e.ItemName)
                .HasMaxLength(200)
                .HasColumnName("item_name");
            entity.Property(e => e.ItemType)
                .HasMaxLength(50)
                .HasColumnName("item_type");
            entity.Property(e => e.LastPurchaseDate).HasColumnName("last_purchase_date");
            entity.Property(e => e.LastPurchaseRate)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("last_purchase_rate");
            entity.Property(e => e.LeadTimeDays)
                .HasDefaultValue(0)
                .HasColumnName("lead_time_days");
            entity.Property(e => e.MinOrderQty)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("min_order_qty");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(50)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.RatePerUnit)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("rate_per_unit");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.ReorderLevel)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("reorder_level");
            entity.Property(e => e.SupplierName)
                .HasMaxLength(200)
                .HasColumnName("supplier_name");
            entity.Property(e => e.Uom)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Pcs'::character varying")
                .HasColumnName("uom");
        });

        modelBuilder.Entity<MstPaper>(entity =>
        {
            entity.HasKey(e => e.PaperId).HasName("mst_paper_pkey");

            entity.ToTable("mst_paper", "press_db");

            entity.HasIndex(e => e.IsActive, "idx_mst_paper_active");

            entity.HasIndex(e => e.PaperCode, "idx_mst_paper_code");

            entity.HasIndex(e => e.PaperCode, "mst_paper_paper_code_key").IsUnique();

            entity.Property(e => e.PaperId).HasColumnName("paper_id");
            entity.Property(e => e.BrandName)
                .HasMaxLength(100)
                .HasColumnName("brand_name");
            entity.Property(e => e.CostPerKg)
                .HasPrecision(10, 2)
                .HasColumnName("cost_per_kg");
            entity.Property(e => e.CostPerSheet)
                .HasPrecision(10, 2)
                .HasColumnName("cost_per_sheet");
            entity.Property(e => e.CountryOfOrigin)
                .HasMaxLength(50)
                .HasColumnName("country_of_origin");
            entity.Property(e => e.CurrentStock)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("current_stock");
            entity.Property(e => e.GrainDirection)
                .HasMaxLength(20)
                .HasColumnName("grain_direction");
            entity.Property(e => e.Gsm).HasColumnName("gsm");
            entity.Property(e => e.GstRate)
                .HasPrecision(5, 2)
                .HasDefaultValueSql("12.00")
                .HasColumnName("gst_rate");
            entity.Property(e => e.HsnCode)
                .HasMaxLength(10)
                .HasColumnName("hsn_code");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsFscCertified)
                .HasDefaultValue(false)
                .HasColumnName("is_fsc_certified");
            entity.Property(e => e.IsRecycled)
                .HasDefaultValue(false)
                .HasColumnName("is_recycled");
            entity.Property(e => e.LastPurchaseDate).HasColumnName("last_purchase_date");
            entity.Property(e => e.LastPurchaseRate)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("last_purchase_rate");
            entity.Property(e => e.LeadTimeDays).HasColumnName("lead_time_days");
            entity.Property(e => e.MinOrderQty)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("min_order_qty");
            entity.Property(e => e.MinOrderQtyKg)
                .HasPrecision(10, 2)
                .HasColumnName("min_order_qty_kg");
            entity.Property(e => e.PaperCategory)
                .HasMaxLength(50)
                .HasColumnName("paper_category");
            entity.Property(e => e.PaperCode)
                .HasMaxLength(50)
                .HasColumnName("paper_code");
            entity.Property(e => e.PaperFinish)
                .HasMaxLength(50)
                .HasColumnName("paper_finish");
            entity.Property(e => e.PaperName)
                .HasMaxLength(150)
                .HasColumnName("paper_name");
            entity.Property(e => e.PaperType)
                .HasMaxLength(50)
                .HasColumnName("paper_type");
            entity.Property(e => e.ReelDiameterMm).HasColumnName("reel_diameter_mm");
            entity.Property(e => e.ReelWidthMm).HasColumnName("reel_width_mm");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.ReorderLevel)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("reorder_level");
            entity.Property(e => e.SheetLengthMm).HasColumnName("sheet_length_mm");
            entity.Property(e => e.SheetSizeName)
                .HasMaxLength(50)
                .HasColumnName("sheet_size_name");
            entity.Property(e => e.SheetWidthMm).HasColumnName("sheet_width_mm");
            entity.Property(e => e.SupplierName)
                .HasMaxLength(100)
                .HasColumnName("supplier_name");
            entity.Property(e => e.SupportedJobTypes)
                .HasMaxLength(200)
                .HasColumnName("supported_job_types");
            entity.Property(e => e.SupportedUsage)
                .HasMaxLength(100)
                .HasColumnName("supported_usage");
            entity.Property(e => e.Uom)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Sheets'::character varying")
                .HasColumnName("uom");
        });

        modelBuilder.Entity<MstPaperSize>(entity =>
        {
            entity.HasKey(e => e.PaperId).HasName("mst_paper_sizes_pkey");

            entity.ToTable("mst_paper_sizes", "press_db");

            entity.Property(e => e.PaperId).HasColumnName("paper_id");
            entity.Property(e => e.Category)
                .HasMaxLength(100)
                .HasColumnName("category");
            entity.Property(e => e.CommonUses).HasColumnName("common_uses");
            entity.Property(e => e.HeightIn)
                .HasPrecision(10, 2)
                .HasColumnName("height_in");
            entity.Property(e => e.HeightMm).HasColumnName("height_mm");
            entity.Property(e => e.Series)
                .HasMaxLength(50)
                .HasColumnName("series");
            entity.Property(e => e.SizeName)
                .HasMaxLength(100)
                .HasColumnName("size_name");
            entity.Property(e => e.WidthIn)
                .HasPrecision(10, 2)
                .HasColumnName("width_in");
            entity.Property(e => e.WidthMm).HasColumnName("width_mm");
        });

        modelBuilder.Entity<MstParty>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("mst_party_pkey");

            entity.ToTable("mst_party", "press_db");

            entity.HasIndex(e => e.IsActive, "idx_party_active");

            entity.HasIndex(e => e.CityId, "idx_party_city");

            entity.HasIndex(e => e.Gstno, "idx_party_gstno");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address1)
                .HasMaxLength(200)
                .HasColumnName("address1");
            entity.Property(e => e.Address2)
                .HasMaxLength(200)
                .HasColumnName("address2");
            entity.Property(e => e.CityId).HasColumnName("city_id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Gstno)
                .HasMaxLength(20)
                .HasColumnName("gstno");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Mobile).HasColumnName("mobile");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.PanNo)
                .HasMaxLength(20)
                .HasColumnName("pan_no");
            entity.Property(e => e.Pin)
                .HasMaxLength(10)
                .HasColumnName("pin");
        });

        modelBuilder.Entity<MstPartyAddress>(entity =>
        {
            entity.HasKey(e => e.AddressId).HasName("mst_party_address_pkey");

            entity.ToTable("mst_party_address", "press_db");

            entity.HasIndex(e => e.CityId, "idx_party_addr_city");

            entity.HasIndex(e => e.CountryId, "idx_party_addr_country");

            entity.HasIndex(e => e.PartyId, "idx_party_addr_party");

            entity.HasIndex(e => e.StateId, "idx_party_addr_state");

            entity.HasIndex(e => e.AddressType, "idx_party_addr_type");

            entity.Property(e => e.AddressId).HasColumnName("address_id");
            entity.Property(e => e.AddressLabel)
                .HasMaxLength(100)
                .HasColumnName("address_label");
            entity.Property(e => e.AddressLine1)
                .HasMaxLength(255)
                .HasColumnName("address_line1");
            entity.Property(e => e.AddressLine2)
                .HasMaxLength(255)
                .HasColumnName("address_line2");
            entity.Property(e => e.AddressType)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Billing'::character varying")
                .HasColumnName("address_type");
            entity.Property(e => e.CityId).HasColumnName("city_id");
            entity.Property(e => e.ContactDesignation)
                .HasMaxLength(20)
                .HasColumnName("contact_designation");
            entity.Property(e => e.ContactEmail)
                .HasMaxLength(50)
                .HasColumnName("contact_email");
            entity.Property(e => e.ContactPersonName)
                .HasMaxLength(30)
                .HasColumnName("contact_person_name");
            entity.Property(e => e.ContactPhone)
                .HasMaxLength(10)
                .HasColumnName("contact_phone");
            entity.Property(e => e.CountryId).HasColumnName("country_id");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.DeliveryInstructions).HasColumnName("delivery_instructions");
            entity.Property(e => e.DeliveryTimeSlot)
                .HasMaxLength(50)
                .HasColumnName("delivery_time_slot");
            entity.Property(e => e.DistrictName)
                .HasMaxLength(50)
                .HasColumnName("district_name");
            entity.Property(e => e.GeoTagVerified)
                .HasDefaultValue(false)
                .HasColumnName("geo_tag_verified");
            entity.Property(e => e.Gstin)
                .HasMaxLength(20)
                .HasColumnName("gstin");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDefault)
                .HasDefaultValue(false)
                .HasColumnName("is_default");
            entity.Property(e => e.Landmark)
                .HasMaxLength(150)
                .HasColumnName("landmark");
            entity.Property(e => e.Latitude)
                .HasPrecision(10, 6)
                .HasColumnName("latitude");
            entity.Property(e => e.Longitude)
                .HasPrecision(10, 6)
                .HasColumnName("longitude");
            entity.Property(e => e.PanNo)
                .HasMaxLength(15)
                .HasColumnName("pan_no");
            entity.Property(e => e.PartyId).HasColumnName("party_id");
            entity.Property(e => e.PostalCode)
                .HasMaxLength(20)
                .HasColumnName("postal_code");
            entity.Property(e => e.PreferredCarrier)
                .HasMaxLength(100)
                .HasColumnName("preferred_carrier");
            entity.Property(e => e.StateId).HasColumnName("state_id");
            entity.Property(e => e.TaxRegionCode)
                .HasMaxLength(10)
                .HasColumnName("tax_region_code");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(100)
                .HasColumnName("updated_by");
            entity.Property(e => e.UpdatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_on");

            entity.HasOne(d => d.City).WithMany(p => p.MstPartyAddresses)
                .HasForeignKey(d => d.CityId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_party_address_city");

            entity.HasOne(d => d.Country).WithMany(p => p.MstPartyAddresses)
                .HasForeignKey(d => d.CountryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_party_address_country");

            entity.HasOne(d => d.Party).WithMany(p => p.MstPartyAddresses)
                .HasForeignKey(d => d.PartyId)
                .HasConstraintName("fk_party_address_party");

            entity.HasOne(d => d.State).WithMany(p => p.MstPartyAddresses)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_party_address_state");
        });

        modelBuilder.Entity<MstPartyBank>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("mst_party_bank_pkey");

            entity.ToTable("mst_party_bank", "press_db");

            entity.HasIndex(e => e.PartyId, "idx_party_bank_party");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccountNo)
                .HasMaxLength(50)
                .HasColumnName("account_no");
            entity.Property(e => e.BankName)
                .HasMaxLength(100)
                .HasColumnName("bank_name");
            entity.Property(e => e.BranchName)
                .HasMaxLength(30)
                .HasColumnName("branch_name");
            entity.Property(e => e.IfscCode)
                .HasMaxLength(20)
                .HasColumnName("ifsc_code");
            entity.Property(e => e.MicrNo)
                .HasMaxLength(20)
                .HasColumnName("micr_no");
            entity.Property(e => e.PartyId).HasColumnName("party_id");

            entity.HasOne(d => d.Party).WithMany(p => p.MstPartyBanks)
                .HasForeignKey(d => d.PartyId)
                .HasConstraintName("fk_party_bank_party");
        });

        modelBuilder.Entity<MstPartyContact>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("mst_party_contact_pkey");

            entity.ToTable("mst_party_contact", "press_db");

            entity.HasIndex(e => e.PartyId, "idx_party_contact_party");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ContactName)
                .HasMaxLength(100)
                .HasColumnName("contact_name");
            entity.Property(e => e.Designation)
                .HasMaxLength(50)
                .HasColumnName("designation");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Mobile).HasColumnName("mobile");
            entity.Property(e => e.PartyId).HasColumnName("party_id");

            entity.HasOne(d => d.Party).WithMany(p => p.MstPartyContacts)
                .HasForeignKey(d => d.PartyId)
                .HasConstraintName("fk_party_contact_party");
        });

        modelBuilder.Entity<MstPartyRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("mst_party_role_pkey");

            entity.ToTable("mst_party_role", "press_db");

            entity.HasIndex(e => e.PartyId, "idx_party_role_party");

            entity.HasIndex(e => e.RoleType, "idx_party_role_type");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.PartyId).HasColumnName("party_id");
            entity.Property(e => e.RoleType)
                .HasMaxLength(20)
                .HasColumnName("role_type");

            entity.HasOne(d => d.Party).WithMany(p => p.MstPartyRoles)
                .HasForeignKey(d => d.PartyId)
                .HasConstraintName("fk_party_role_party");
        });

        modelBuilder.Entity<MstPartyTax>(entity =>
        {
            entity.HasKey(e => e.PartyTaxId).HasName("mst_party_tax_pkey");

            entity.ToTable("mst_party_tax", "press_db", tb => tb.HasComment("Tax registrations per party: GSTIN, PAN, TAN, VAT numbers."));

            entity.HasIndex(e => e.PartyId, "idx_party_tax_party");

            entity.HasIndex(e => e.TaxRegionId, "idx_party_tax_region");

            entity.HasIndex(e => e.TaxTypeId, "idx_party_tax_type");

            entity.Property(e => e.PartyTaxId).HasColumnName("party_tax_id");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.ExpiryDate).HasColumnName("expiry_date");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDefault)
                .HasDefaultValue(false)
                .HasColumnName("is_default");
            entity.Property(e => e.PartyId).HasColumnName("party_id");
            entity.Property(e => e.RegistrationDate).HasColumnName("registration_date");
            entity.Property(e => e.TaxNumber)
                .HasMaxLength(50)
                .HasColumnName("tax_number");
            entity.Property(e => e.TaxRegionId).HasColumnName("tax_region_id");
            entity.Property(e => e.TaxTypeId).HasColumnName("tax_type_id");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(100)
                .HasColumnName("updated_by");
            entity.Property(e => e.UpdatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_on");

            entity.HasOne(d => d.Party).WithMany(p => p.MstPartyTaxes)
                .HasForeignKey(d => d.PartyId)
                .HasConstraintName("fk_party_tax_party");

            entity.HasOne(d => d.TaxRegion).WithMany(p => p.MstPartyTaxes)
                .HasForeignKey(d => d.TaxRegionId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_party_tax_region");

            entity.HasOne(d => d.TaxType).WithMany(p => p.MstPartyTaxes)
                .HasForeignKey(d => d.TaxTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_party_tax_type");
        });

        modelBuilder.Entity<MstPaymentTerm>(entity =>
        {
            entity.HasKey(e => e.PaymentTermId).HasName("mst_payment_term_pkey");

            entity.ToTable("mst_payment_term", "press_db");

            entity.HasIndex(e => e.ApplicableToPartyType, "idx_payment_term_type");

            entity.HasIndex(e => e.Code, "mst_payment_term_code_key").IsUnique();

            entity.Property(e => e.PaymentTermId).HasColumnName("payment_term_id");
            entity.Property(e => e.ApplicableToPartyType)
                .HasMaxLength(20)
                .HasColumnName("applicable_to_party_type");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DiscountDays)
                .HasDefaultValue(0)
                .HasColumnName("discount_days");
            entity.Property(e => e.DiscountPercent)
                .HasPrecision(5, 2)
                .HasDefaultValueSql("0.00")
                .HasColumnName("discount_percent");
            entity.Property(e => e.DueDays)
                .HasDefaultValue(0)
                .HasColumnName("due_days");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDefault)
                .HasDefaultValue(false)
                .HasColumnName("is_default");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .HasColumnName("name");
            entity.Property(e => e.TermType)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Net'::character varying")
                .HasColumnName("term_type");
        });

        modelBuilder.Entity<MstPermission>(entity =>
        {
            entity.HasKey(e => e.Permissionid).HasName("mst_permission_pkey");

            entity.ToTable("mst_permission", "press_db");

            entity.HasIndex(e => e.Permissioncode, "mst_permission_permissioncode_key").IsUnique();

            entity.Property(e => e.Permissionid).HasColumnName("permissionid");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.Modulename)
                .HasMaxLength(100)
                .HasColumnName("modulename");
            entity.Property(e => e.Permissioncode)
                .HasMaxLength(100)
                .HasColumnName("permissioncode");
            entity.Property(e => e.Permissionname)
                .HasMaxLength(150)
                .HasColumnName("permissionname");
        });

        modelBuilder.Entity<MstPlate>(entity =>
        {
            entity.HasKey(e => e.PlateId).HasName("mst_plate_pkey");

            entity.ToTable("mst_plate", "press_db");

            entity.HasIndex(e => e.IsActive, "idx_mst_plate_active");

            entity.HasIndex(e => e.PlateCode, "idx_mst_plate_code");

            entity.HasIndex(e => e.PlateCode, "mst_plate_plate_code_key").IsUnique();

            entity.Property(e => e.PlateId).HasColumnName("plate_id");
            entity.Property(e => e.AutoSelectPriority).HasColumnName("auto_select_priority");
            entity.Property(e => e.CoatingType)
                .HasMaxLength(50)
                .HasColumnName("coating_type");
            entity.Property(e => e.CompatibleCtp)
                .HasMaxLength(100)
                .HasColumnName("compatible_ctp");
            entity.Property(e => e.CompatibleMachineType)
                .HasMaxLength(100)
                .HasColumnName("compatible_machine_type");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.CurrentStock)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("current_stock");
            entity.Property(e => e.ExposureType)
                .HasMaxLength(50)
                .HasColumnName("exposure_type");
            entity.Property(e => e.GstRate)
                .HasPrecision(5, 2)
                .HasDefaultValueSql("18.00")
                .HasColumnName("gst_rate");
            entity.Property(e => e.HsnCode)
                .HasMaxLength(10)
                .HasColumnName("hsn_code");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.LastPurchaseDate).HasColumnName("last_purchase_date");
            entity.Property(e => e.LastPurchaseRate)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("last_purchase_rate");
            entity.Property(e => e.MaxGsmSupported).HasColumnName("max_gsm_supported");
            entity.Property(e => e.MaxImpressions).HasColumnName("max_impressions");
            entity.Property(e => e.MinGsmSupported).HasColumnName("min_gsm_supported");
            entity.Property(e => e.MinOrderQty)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("min_order_qty");
            entity.Property(e => e.PlateCode)
                .HasMaxLength(50)
                .HasColumnName("plate_code");
            entity.Property(e => e.PlateCost)
                .HasPrecision(12, 2)
                .HasColumnName("plate_cost");
            entity.Property(e => e.PlateLengthMm).HasColumnName("plate_length_mm");
            entity.Property(e => e.PlateName)
                .HasMaxLength(150)
                .HasColumnName("plate_name");
            entity.Property(e => e.PlateType)
                .HasMaxLength(50)
                .HasColumnName("plate_type");
            entity.Property(e => e.PlateWidthMm).HasColumnName("plate_width_mm");
            entity.Property(e => e.ProcessingCost)
                .HasPrecision(12, 2)
                .HasColumnName("processing_cost");
            entity.Property(e => e.Remarks)
                .HasMaxLength(255)
                .HasColumnName("remarks");
            entity.Property(e => e.ReorderLevel)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("reorder_level");
            entity.Property(e => e.Reusability)
                .HasDefaultValue(false)
                .HasColumnName("reusability");
            entity.Property(e => e.ShelfCondition)
                .HasMaxLength(100)
                .HasColumnName("shelf_condition");
            entity.Property(e => e.StorageLifeMonths).HasColumnName("storage_life_months");
            entity.Property(e => e.SupportedJobTypes)
                .HasMaxLength(200)
                .HasColumnName("supported_job_types");
            entity.Property(e => e.ThicknessMm)
                .HasPrecision(5, 2)
                .HasColumnName("thickness_mm");
            entity.Property(e => e.Uom)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Pcs'::character varying")
                .HasColumnName("uom");
            entity.Property(e => e.WastagePercent)
                .HasPrecision(5, 2)
                .HasColumnName("wastage_percent");
        });

        modelBuilder.Entity<MstPrintProcess>(entity =>
        {
            entity.HasKey(e => e.Processid).HasName("mst_print_process_pkey");

            entity.ToTable("mst_print_process", "press_db");

            entity.Property(e => e.Processid)
                .ValueGeneratedNever()
                .HasColumnName("processid");
            entity.Property(e => e.Createdon)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdon");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Displayorder).HasColumnName("displayorder");
            entity.Property(e => e.Isactive).HasColumnName("isactive");
            entity.Property(e => e.Processcategory)
                .HasMaxLength(50)
                .HasColumnName("processcategory");
            entity.Property(e => e.Processcode)
                .HasMaxLength(50)
                .HasColumnName("processcode");
            entity.Property(e => e.Processname)
                .HasMaxLength(150)
                .HasColumnName("processname");
        });

        modelBuilder.Entity<MstPrintProductSize>(entity =>
        {
            entity.HasKey(e => e.Productsizeid).HasName("mst_print_product_size_pkey");

            entity.ToTable("mst_print_product_size", "press_db");

            entity.Property(e => e.Productsizeid).HasColumnName("productsizeid");
            entity.Property(e => e.Category)
                .HasMaxLength(50)
                .HasColumnName("category");
            entity.Property(e => e.Heightinch)
                .HasPrecision(6, 2)
                .HasColumnName("heightinch");
            entity.Property(e => e.Heightmm)
                .HasPrecision(8, 2)
                .HasColumnName("heightmm");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.Isstandard)
                .HasDefaultValue(true)
                .HasColumnName("isstandard");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.Sizecode)
                .HasMaxLength(50)
                .HasColumnName("sizecode");
            entity.Property(e => e.Sizename)
                .HasMaxLength(100)
                .HasColumnName("sizename");
            entity.Property(e => e.Widthinch)
                .HasPrecision(6, 2)
                .HasColumnName("widthinch");
            entity.Property(e => e.Widthmm)
                .HasPrecision(8, 2)
                .HasColumnName("widthmm");
        });

        modelBuilder.Entity<MstPrintProductType>(entity =>
        {
            entity.HasKey(e => e.Printproducttypeid).HasName("mst_print_product_type_pkey");

            entity.ToTable("mst_print_product_type", "press_db");

            entity.HasIndex(e => e.Productcode, "mst_print_product_type_productcode_key").IsUnique();

            entity.Property(e => e.Printproducttypeid)
                .ValueGeneratedNever()
                .HasColumnName("printproducttypeid");
            entity.Property(e => e.Category)
                .HasMaxLength(50)
                .HasColumnName("category");
            entity.Property(e => e.Createdby)
                .HasMaxLength(50)
                .HasDefaultValueSql("'SYSTEM'::character varying")
                .HasColumnName("createdby");
            entity.Property(e => e.Createdon)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdon");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.Isbindingrequired)
                .HasDefaultValue(false)
                .HasColumnName("isbindingrequired");
            entity.Property(e => e.Iscustomsize)
                .HasDefaultValue(true)
                .HasColumnName("iscustomsize");
            entity.Property(e => e.Isfinishingrequired)
                .HasDefaultValue(true)
                .HasColumnName("isfinishingrequired");
            entity.Property(e => e.Isprintingrequired)
                .HasDefaultValue(true)
                .HasColumnName("isprintingrequired");
            entity.Property(e => e.Productcode)
                .HasMaxLength(30)
                .HasColumnName("productcode");
            entity.Property(e => e.Productname)
                .HasMaxLength(100)
                .HasColumnName("productname");
        });

        modelBuilder.Entity<MstProcess>(entity =>
        {
            entity.HasKey(e => e.Processid).HasName("mst_process_pkey");

            entity.ToTable("mst_process", "press_db");

            entity.HasIndex(e => e.Departmentid, "idx_process_dept");

            entity.HasIndex(e => e.Sequenceno, "idx_process_sequence");

            entity.HasIndex(e => e.Processcode, "mst_process_processcode_key").IsUnique();

            entity.Property(e => e.Processid).HasColumnName("processid");
            entity.Property(e => e.Createdby)
                .HasMaxLength(50)
                .HasColumnName("createdby");
            entity.Property(e => e.Createdon)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdon");
            entity.Property(e => e.Departmentid).HasColumnName("departmentid");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.Isapprovalrequired)
                .HasDefaultValue(false)
                .HasColumnName("isapprovalrequired");
            entity.Property(e => e.Isclientapproval)
                .HasDefaultValue(false)
                .HasColumnName("isclientapproval");
            entity.Property(e => e.Ismandatory)
                .HasDefaultValue(true)
                .HasColumnName("ismandatory");
            entity.Property(e => e.Modifiedby)
                .HasMaxLength(50)
                .HasColumnName("modifiedby");
            entity.Property(e => e.Modifiedon)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modifiedon");
            entity.Property(e => e.Processcode)
                .HasMaxLength(50)
                .HasColumnName("processcode");
            entity.Property(e => e.Processname)
                .HasMaxLength(150)
                .HasColumnName("processname");
            entity.Property(e => e.Sequenceno).HasColumnName("sequenceno");
            entity.Property(e => e.Templatecode)
                .HasMaxLength(50)
                .HasColumnName("templatecode");
            entity.Property(e => e.Templatename)
                .HasMaxLength(100)
                .HasColumnName("templatename");

            entity.HasOne(d => d.Department).WithMany(p => p.MstProcesses)
                .HasForeignKey(d => d.Departmentid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_process_department");
        });

        modelBuilder.Entity<MstProcessDepartmentMap>(entity =>
        {
            entity.HasKey(e => e.MapId).HasName("mst_process_department_map_pkey");

            entity.ToTable("mst_process_department_map", "press_db");

            entity.Property(e => e.MapId).HasColumnName("map_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.DeptId).HasColumnName("dept_id");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsPrimary)
                .HasDefaultValue(true)
                .HasColumnName("is_primary");
            entity.Property(e => e.ProcessCode)
                .HasMaxLength(30)
                .HasColumnName("process_code");
        });

        modelBuilder.Entity<MstProcessNotificationConfig>(entity =>
        {
            entity.HasKey(e => e.ConfigId).HasName("mst_process_notification_config_pkey");

            entity.ToTable("mst_process_notification_config", "press_db", tb => tb.HasComment("Hybrid master: drives ALL notifications (task, approval, client, AI, overdue, escalation)\n for every subprocess across all 20 job processes.\n Relational columns → fast filter/join. JSONB → flexible payload & AI configuration."));

            entity.HasIndex(e => e.IsActive, "idx_pnc_active");

            entity.HasIndex(e => e.ApprovalTypeId, "idx_pnc_approval_type");

            entity.HasIndex(e => e.DepartmentId, "idx_pnc_department");

            entity.HasIndex(e => e.EventTypeCode, "idx_pnc_event_type");

            entity.HasIndex(e => e.ProcessId, "idx_pnc_process");

            entity.HasIndex(e => e.ProcessCode, "idx_pnc_process_code");

            entity.HasIndex(e => e.SubprocessId, "idx_pnc_subprocess");

            entity.Property(e => e.ConfigId).HasColumnName("config_id");
            entity.Property(e => e.AiConfig)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasComment("JSONB: AI model, prompt template, event category, auto-assign flag, threshold, fallback.")
                .HasColumnType("jsonb")
                .HasColumnName("ai_config");
            entity.Property(e => e.ApprovalLevel)
                .HasDefaultValue(0)
                .HasColumnName("approval_level");
            entity.Property(e => e.ApprovalTypeId).HasColumnName("approval_type_id");
            entity.Property(e => e.AutoTrigger)
                .HasDefaultValue(true)
                .HasColumnName("auto_trigger");
            entity.Property(e => e.BodyTemplate).HasColumnName("body_template");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .HasDefaultValueSql("'SYSTEM'::character varying")
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.EscalateAfterHours)
                .HasPrecision(6, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("escalate_after_hours");
            entity.Property(e => e.EscalateTo)
                .HasMaxLength(50)
                .HasColumnName("escalate_to");
            entity.Property(e => e.EventLabel)
                .HasMaxLength(100)
                .HasColumnName("event_label");
            entity.Property(e => e.EventTypeCode)
                .HasMaxLength(30)
                .HasColumnName("event_type_code");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsMandatory)
                .HasDefaultValue(false)
                .HasColumnName("is_mandatory");
            entity.Property(e => e.JobTypeCode)
                .HasMaxLength(30)
                .HasColumnName("job_type_code");
            entity.Property(e => e.JobTypeId).HasColumnName("job_type_id");
            entity.Property(e => e.Meta)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("meta");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(50)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.NotifyApprover)
                .HasDefaultValue(false)
                .HasColumnName("notify_approver");
            entity.Property(e => e.NotifyAssignee)
                .HasDefaultValue(true)
                .HasColumnName("notify_assignee");
            entity.Property(e => e.NotifyClientEmail)
                .HasDefaultValue(false)
                .HasColumnName("notify_client_email");
            entity.Property(e => e.NotifyClientSms)
                .HasDefaultValue(false)
                .HasColumnName("notify_client_sms");
            entity.Property(e => e.NotifyClientWhatsapp)
                .HasDefaultValue(false)
                .HasColumnName("notify_client_whatsapp");
            entity.Property(e => e.NotifyDeptHead)
                .HasDefaultValue(false)
                .HasColumnName("notify_dept_head");
            entity.Property(e => e.NotifyInternalEmail)
                .HasDefaultValue(false)
                .HasColumnName("notify_internal_email");
            entity.Property(e => e.NotifyInternalSms)
                .HasDefaultValue(false)
                .HasColumnName("notify_internal_sms");
            entity.Property(e => e.NotifyInternalWhatsapp)
                .HasDefaultValue(false)
                .HasColumnName("notify_internal_whatsapp");
            entity.Property(e => e.NotifyPush)
                .HasDefaultValue(false)
                .HasColumnName("notify_push");
            entity.Property(e => e.NotifySupervisor)
                .HasDefaultValue(false)
                .HasColumnName("notify_supervisor");
            entity.Property(e => e.NotifyTopupAlert)
                .HasDefaultValue(false)
                .HasColumnName("notify_topup_alert");
            entity.Property(e => e.OverdueReminderIntervalHours)
                .HasPrecision(6, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("overdue_reminder_interval_hours");
            entity.Property(e => e.PayloadConfig)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasComment("JSONB: template variables list, per-channel enable/retry rules, recipient routing hints.")
                .HasColumnType("jsonb")
                .HasColumnName("payload_config");
            entity.Property(e => e.Priority)
                .HasMaxLength(20)
                .HasDefaultValueSql("'NORMAL'::character varying")
                .HasColumnName("priority");
            entity.Property(e => e.ProcessCode)
                .HasMaxLength(30)
                .HasColumnName("process_code");
            entity.Property(e => e.ProcessId).HasColumnName("process_id");
            entity.Property(e => e.RecipientType)
                .HasMaxLength(20)
                .HasDefaultValueSql("'INTERNAL'::character varying")
                .HasColumnName("recipient_type");
            entity.Property(e => e.SequenceNo)
                .HasDefaultValue(1)
                .HasColumnName("sequence_no");
            entity.Property(e => e.SlaHours)
                .HasPrecision(6, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("sla_hours");
            entity.Property(e => e.SubjectTemplate)
                .HasMaxLength(300)
                .HasColumnName("subject_template");
            entity.Property(e => e.SubprocessCode)
                .HasMaxLength(30)
                .HasColumnName("subprocess_code");
            entity.Property(e => e.SubprocessId).HasColumnName("subprocess_id");
            entity.Property(e => e.TemplateCode)
                .HasMaxLength(50)
                .HasColumnName("template_code");
            entity.Property(e => e.TriggerCondition).HasColumnName("trigger_condition");
            entity.Property(e => e.TriggerOnStatus)
                .HasMaxLength(30)
                .HasColumnName("trigger_on_status");

            entity.HasOne(d => d.ApprovalType).WithMany(p => p.MstProcessNotificationConfigs)
                .HasForeignKey(d => d.ApprovalTypeId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_pnc_approval_type");

            entity.HasOne(d => d.Department).WithMany(p => p.MstProcessNotificationConfigs)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_pnc_department");

            entity.HasOne(d => d.JobType).WithMany(p => p.MstProcessNotificationConfigs)
                .HasForeignKey(d => d.JobTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_pnc_job_type_id");

            entity.HasOne(d => d.Process).WithMany(p => p.MstProcessNotificationConfigs)
                .HasForeignKey(d => d.ProcessId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_pnc_process");
        });

        modelBuilder.Entity<MstProcessRoleMap>(entity =>
        {
            entity.HasKey(e => e.MapId).HasName("mst_process_role_map_pkey");

            entity.ToTable("mst_process_role_map", "press_db");

            entity.Property(e => e.MapId).HasColumnName("map_id");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsMandatory)
                .HasDefaultValue(true)
                .HasColumnName("is_mandatory");
            entity.Property(e => e.ProcessCode)
                .HasMaxLength(30)
                .HasColumnName("process_code");
            entity.Property(e => e.RoleType)
                .HasMaxLength(30)
                .HasColumnName("role_type");
            entity.Property(e => e.Roleid).HasColumnName("roleid");
            entity.Property(e => e.SequenceNo).HasColumnName("sequence_no");
        });

        modelBuilder.Entity<MstProcessStage>(entity =>
        {
            entity.HasKey(e => e.StageId).HasName("mst_process_stage_pkey");

            entity.ToTable("mst_process_stage", "press_db");

            entity.Property(e => e.StageId).HasColumnName("stage_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.StageName)
                .HasMaxLength(100)
                .HasColumnName("stage_name");
        });

        modelBuilder.Entity<MstProductPart>(entity =>
        {
            entity.HasKey(e => e.Productpartid).HasName("mst_product_part_pkey");

            entity.ToTable("mst_product_part", "press_db");

            entity.HasIndex(e => e.Printproducttypeid, "idx_product_part_type");

            entity.HasIndex(e => e.Partcode, "mst_product_part_partcode_key").IsUnique();

            entity.Property(e => e.Productpartid)
                .ValueGeneratedNever()
                .HasColumnName("productpartid");
            entity.Property(e => e.Createdon)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdon");
            entity.Property(e => e.Defaultpages)
                .HasDefaultValue(0)
                .HasColumnName("defaultpages");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Displayorder).HasColumnName("displayorder");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.Ismultiple)
                .HasDefaultValue(false)
                .HasColumnName("ismultiple");
            entity.Property(e => e.Ispagebased)
                .HasDefaultValue(true)
                .HasColumnName("ispagebased");
            entity.Property(e => e.Partcode)
                .HasMaxLength(50)
                .HasColumnName("partcode");
            entity.Property(e => e.Partname)
                .HasMaxLength(100)
                .HasColumnName("partname");
            entity.Property(e => e.Printproducttypeid).HasColumnName("printproducttypeid");
            entity.Property(e => e.Requiresbinding)
                .HasDefaultValue(false)
                .HasColumnName("requiresbinding");
            entity.Property(e => e.Requiresdesign)
                .HasDefaultValue(true)
                .HasColumnName("requiresdesign");
            entity.Property(e => e.Requiresfinishing)
                .HasDefaultValue(false)
                .HasColumnName("requiresfinishing");
            entity.Property(e => e.Requirespaper)
                .HasDefaultValue(true)
                .HasColumnName("requirespaper");
            entity.Property(e => e.Requiresplate)
                .HasDefaultValue(true)
                .HasColumnName("requiresplate");
            entity.Property(e => e.Requiresprinting)
                .HasDefaultValue(true)
                .HasColumnName("requiresprinting");

            entity.HasOne(d => d.Printproducttype).WithMany(p => p.MstProductParts)
                .HasForeignKey(d => d.Printproducttypeid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_product_part_product_type");
        });

        modelBuilder.Entity<MstRole>(entity =>
        {
            entity.HasKey(e => e.Roleid).HasName("mst_role_pkey");

            entity.ToTable("mst_role", "press_db");

            entity.HasIndex(e => e.Isactive, "idx_role_active");

            entity.HasIndex(e => e.ApprovalLevel, "idx_role_approval");

            entity.HasIndex(e => e.DeptId, "idx_role_dept");

            entity.HasIndex(e => e.RoleType, "idx_role_type");

            entity.HasIndex(e => e.Rolecode, "mst_role_rolecode_key").IsUnique();

            entity.Property(e => e.Roleid).HasColumnName("roleid");
            entity.Property(e => e.ApprovalLevel)
                .HasDefaultValue(0)
                .HasColumnName("approval_level");
            entity.Property(e => e.CanApprove)
                .HasDefaultValue(false)
                .HasColumnName("can_approve");
            entity.Property(e => e.CanExecute)
                .HasDefaultValue(true)
                .HasColumnName("can_execute");
            entity.Property(e => e.CanReview)
                .HasDefaultValue(false)
                .HasColumnName("can_review");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Createdby)
                .HasMaxLength(50)
                .HasColumnName("createdby");
            entity.Property(e => e.DashboardCode)
                .HasMaxLength(50)
                .HasColumnName("dashboard_code");
            entity.Property(e => e.DeptId).HasColumnName("dept_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsDefault)
                .HasDefaultValue(false)
                .HasColumnName("is_default");
            entity.Property(e => e.IsEditable)
                .HasDefaultValue(true)
                .HasColumnName("is_editable");
            entity.Property(e => e.IsWorkflowRole)
                .HasDefaultValue(true)
                .HasColumnName("is_workflow_role");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.Issystem)
                .HasDefaultValue(false)
                .HasColumnName("issystem");
            entity.Property(e => e.Modifiedat)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modifiedat");
            entity.Property(e => e.Modifiedby)
                .HasMaxLength(50)
                .HasColumnName("modifiedby");
            entity.Property(e => e.ParentRoleid).HasColumnName("parent_roleid");
            entity.Property(e => e.RoleCategory)
                .HasMaxLength(30)
                .HasColumnName("role_category");
            entity.Property(e => e.RoleType)
                .HasMaxLength(30)
                .HasColumnName("role_type");
            entity.Property(e => e.Rolecode)
                .HasMaxLength(50)
                .HasColumnName("rolecode");
            entity.Property(e => e.Rolename)
                .HasMaxLength(100)
                .HasColumnName("rolename");
            entity.Property(e => e.SecurityLevel)
                .HasDefaultValue(1)
                .HasColumnName("security_level");

            entity.HasOne(d => d.Dept).WithMany(p => p.MstRoles)
                .HasForeignKey(d => d.DeptId)
                .HasConstraintName("fk_role_department");

            entity.HasOne(d => d.ParentRole).WithMany(p => p.InverseParentRole)
                .HasForeignKey(d => d.ParentRoleid)
                .HasConstraintName("fk_parent_role");
        });

        modelBuilder.Entity<MstRoleType>(entity =>
        {
            entity.HasKey(e => e.Roletypeid).HasName("mst_role_type_pkey");

            entity.ToTable("mst_role_type", "press_db");

            entity.HasIndex(e => e.Roletypecode, "mst_role_type_roletypecode_key").IsUnique();

            entity.Property(e => e.Roletypeid).HasColumnName("roletypeid");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.Roletypecode)
                .HasMaxLength(30)
                .HasColumnName("roletypecode");
            entity.Property(e => e.Roletypename)
                .HasMaxLength(50)
                .HasColumnName("roletypename");
        });

        modelBuilder.Entity<MstShiftType>(entity =>
        {
            entity.HasKey(e => e.ShiftTypeId).HasName("mst_shift_type_pkey");

            entity.ToTable("mst_shift_type", "press_db");

            entity.HasIndex(e => e.ShiftCode, "mst_shift_type_shift_code_key").IsUnique();

            entity.Property(e => e.ShiftTypeId).HasColumnName("shift_type_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.ShiftCode)
                .HasMaxLength(30)
                .HasColumnName("shift_code");
            entity.Property(e => e.ShiftEndTime).HasColumnName("shift_end_time");
            entity.Property(e => e.ShiftName)
                .HasMaxLength(100)
                .HasColumnName("shift_name");
            entity.Property(e => e.ShiftStartTime).HasColumnName("shift_start_time");
        });

        modelBuilder.Entity<MstState>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("mst_state_pkey");

            entity.ToTable("mst_state", "press_db");

            entity.HasIndex(e => e.CountryId, "idx_state_country");

            entity.HasIndex(e => e.GstStateCode, "idx_state_gst_code");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CapitalCity)
                .HasMaxLength(100)
                .HasColumnName("capital_city");
            entity.Property(e => e.Code)
                .HasMaxLength(100)
                .HasColumnName("code");
            entity.Property(e => e.CountryId).HasColumnName("country_id");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.GstStateCode)
                .HasMaxLength(5)
                .HasColumnName("gst_state_code");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(false)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDefault)
                .HasDefaultValue(false)
                .HasColumnName("is_default");
            entity.Property(e => e.IsUnionTerritory)
                .HasDefaultValue(false)
                .HasColumnName("is_union_territory");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(50)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.RegionName)
                .HasMaxLength(50)
                .HasColumnName("region_name");
            entity.Property(e => e.ZoneName)
                .HasMaxLength(50)
                .HasColumnName("zone_name");

            entity.HasOne(d => d.Country).WithMany(p => p.MstStates)
                .HasForeignKey(d => d.CountryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_state_country");
        });

        modelBuilder.Entity<MstStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("mst_status_pkey");

            entity.ToTable("mst_status", "press_db");

            entity.HasIndex(e => e.Module, "idx_status_module");

            entity.HasIndex(e => e.Stage, "idx_status_stage");

            entity.HasIndex(e => e.Statuscode, "mst_status_statuscode_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Colorcode)
                .HasMaxLength(20)
                .HasColumnName("colorcode");
            entity.Property(e => e.Createdby)
                .HasMaxLength(50)
                .HasColumnName("createdby");
            entity.Property(e => e.Createdon)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdon");
            entity.Property(e => e.Icon)
                .HasMaxLength(50)
                .HasColumnName("icon");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.Iseditable)
                .HasDefaultValue(true)
                .HasColumnName("iseditable");
            entity.Property(e => e.Isfailure)
                .HasDefaultValue(false)
                .HasColumnName("isfailure");
            entity.Property(e => e.Isfinal)
                .HasDefaultValue(false)
                .HasColumnName("isfinal");
            entity.Property(e => e.Module)
                .HasMaxLength(50)
                .HasColumnName("module");
            entity.Property(e => e.Sequenceno).HasColumnName("sequenceno");
            entity.Property(e => e.Stage)
                .HasMaxLength(50)
                .HasColumnName("stage");
            entity.Property(e => e.Statuscode)
                .HasMaxLength(50)
                .HasColumnName("statuscode");
            entity.Property(e => e.Statusname)
                .HasMaxLength(100)
                .HasColumnName("statusname");
        });

        modelBuilder.Entity<MstSupplier>(entity =>
        {
            entity.HasKey(e => e.SupplierId).HasName("mst_supplier_pkey");

            entity.ToTable("mst_supplier", "press_db");

            entity.HasIndex(e => e.PartyId, "idx_supplier_party");

            entity.HasIndex(e => e.SupplierTypeId, "idx_supplier_type");

            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.ModifiedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.PartyId).HasColumnName("party_id");
            entity.Property(e => e.PaymentCycleDays)
                .HasDefaultValue(30)
                .HasColumnName("payment_cycle_days");
            entity.Property(e => e.PreferredCurrency).HasColumnName("preferred_currency");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.SupplierTypeId).HasColumnName("supplier_type_id");
            entity.Property(e => e.TdsApplicable)
                .HasDefaultValue(false)
                .HasColumnName("tds_applicable");
            entity.Property(e => e.TdsRate)
                .HasPrecision(5, 2)
                .HasColumnName("tds_rate");

            entity.HasOne(d => d.Party).WithMany(p => p.MstSuppliers)
                .HasForeignKey(d => d.PartyId)
                .HasConstraintName("fk_supplier_party");

            entity.HasOne(d => d.PreferredCurrencyNavigation).WithMany(p => p.MstSuppliers)
                .HasForeignKey(d => d.PreferredCurrency)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_supplier_currency");

            entity.HasOne(d => d.SupplierType).WithMany(p => p.MstSuppliers)
                .HasForeignKey(d => d.SupplierTypeId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_supplier_type");
        });

        modelBuilder.Entity<MstSupplierType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("mst_supplier_type_pkey");

            entity.ToTable("mst_supplier_type", "press_db");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<MstTaxCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("mst_tax_category_pkey");

            entity.ToTable("mst_tax_category", "press_db", tb => tb.HasComment("Tax slabs: GST 5%, 12%, 18%, 28%, EXEMPT, ZERO-RATED, etc."));

            entity.HasIndex(e => e.Code, "mst_tax_category_code_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ApplicableFrom).HasColumnName("applicable_from");
            entity.Property(e => e.ApplicableTo).HasColumnName("applicable_to");
            entity.Property(e => e.Code)
                .HasMaxLength(30)
                .HasColumnName("code");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.HsnSacCode)
                .HasMaxLength(20)
                .HasColumnName("hsn_sac_code");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsExempt)
                .HasDefaultValue(false)
                .HasColumnName("is_exempt");
            entity.Property(e => e.IsReverseChargeApplicable)
                .HasDefaultValue(false)
                .HasColumnName("is_reverse_charge_applicable");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .HasColumnName("name");
            entity.Property(e => e.ParentTaxCategoryId).HasColumnName("parent_tax_category_id");
            entity.Property(e => e.TaxRegime)
                .HasMaxLength(30)
                .HasColumnName("tax_regime");
            entity.Property(e => e.TaxType)
                .HasMaxLength(20)
                .HasColumnName("tax_type");

            entity.HasOne(d => d.ParentTaxCategory).WithMany(p => p.InverseParentTaxCategory)
                .HasForeignKey(d => d.ParentTaxCategoryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_tax_category_parent");
        });

        modelBuilder.Entity<MstTaxCategoryComponent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("mst_tax_category_component_pkey");

            entity.ToTable("mst_tax_category_component", "press_db", tb => tb.HasComment("Rate split per component for a tax category (CGST/SGST/IGST rates)."));

            entity.HasIndex(e => e.TaxCategoryId, "idx_tcc_tax_category");

            entity.HasIndex(e => e.TaxComponentId, "idx_tcc_tax_component");

            entity.HasIndex(e => new { e.TaxCategoryId, e.TaxComponentId, e.EffectiveFrom }, "mst_tax_category_component_tax_category_id_tax_component_id_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.EffectiveFrom).HasColumnName("effective_from");
            entity.Property(e => e.EffectiveTo).HasColumnName("effective_to");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.RatePercent)
                .HasPrecision(8, 4)
                .HasColumnName("rate_percent");
            entity.Property(e => e.TaxCategoryId).HasColumnName("tax_category_id");
            entity.Property(e => e.TaxComponentId).HasColumnName("tax_component_id");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(100)
                .HasColumnName("updated_by");
            entity.Property(e => e.UpdatedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_on");

            entity.HasOne(d => d.TaxCategory).WithMany(p => p.MstTaxCategoryComponents)
                .HasForeignKey(d => d.TaxCategoryId)
                .HasConstraintName("fk_tcc_tax_category");

            entity.HasOne(d => d.TaxComponent).WithMany(p => p.MstTaxCategoryComponents)
                .HasForeignKey(d => d.TaxComponentId)
                .HasConstraintName("fk_tcc_tax_component");
        });

        modelBuilder.Entity<MstTaxComponent>(entity =>
        {
            entity.HasKey(e => e.TaxComponentId).HasName("mst_tax_component_pkey");

            entity.ToTable("mst_tax_component", "press_db", tb => tb.HasComment("Tax sub-components: CGST, SGST, IGST, CESS, TDS, TCS, etc."));

            entity.HasIndex(e => e.Code, "mst_tax_component_code_key").IsUnique();

            entity.Property(e => e.TaxComponentId).HasColumnName("tax_component_id");
            entity.Property(e => e.ApplicableOn)
                .HasMaxLength(50)
                .HasDefaultValueSql("'TAXABLE_VALUE'::character varying")
                .HasColumnName("applicable_on");
            entity.Property(e => e.Code)
                .HasMaxLength(20)
                .HasColumnName("code");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsPercentage)
                .HasDefaultValue(true)
                .HasColumnName("is_percentage");
            entity.Property(e => e.IsRecoverable)
                .HasDefaultValue(true)
                .HasColumnName("is_recoverable");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(100)
                .HasColumnName("updated_by");
            entity.Property(e => e.UpdatedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_on");
        });

        modelBuilder.Entity<MstTaxRate>(entity =>
        {
            entity.HasKey(e => e.TaxRateId).HasName("mst_tax_rate_pkey");

            entity.ToTable("mst_tax_rate", "press_db", tb => tb.HasComment("Effective-dated tax rates per type/category/region/HSN."));

            entity.HasIndex(e => e.TaxCategoryId, "idx_tax_rate_category");

            entity.HasIndex(e => new { e.EffectiveFrom, e.EffectiveTo }, "idx_tax_rate_effective");

            entity.HasIndex(e => e.HsnSacCode, "idx_tax_rate_hsn");

            entity.HasIndex(e => e.RegionId, "idx_tax_rate_region");

            entity.HasIndex(e => e.TaxTypeId, "idx_tax_rate_type");

            entity.Property(e => e.TaxRateId).HasColumnName("tax_rate_id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.EffectiveFrom).HasColumnName("effective_from");
            entity.Property(e => e.EffectiveTo).HasColumnName("effective_to");
            entity.Property(e => e.HsnSacCode)
                .HasMaxLength(20)
                .HasColumnName("hsn_sac_code");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.RatePercent)
                .HasPrecision(8, 4)
                .HasColumnName("rate_percent");
            entity.Property(e => e.RegionId).HasColumnName("region_id");
            entity.Property(e => e.TaxCategoryId).HasColumnName("tax_category_id");
            entity.Property(e => e.TaxTypeId).HasColumnName("tax_type_id");

            entity.HasOne(d => d.Region).WithMany(p => p.MstTaxRates)
                .HasForeignKey(d => d.RegionId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_tax_rate_region");

            entity.HasOne(d => d.TaxCategory).WithMany(p => p.MstTaxRates)
                .HasForeignKey(d => d.TaxCategoryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_tax_rate_category");

            entity.HasOne(d => d.TaxType).WithMany(p => p.MstTaxRates)
                .HasForeignKey(d => d.TaxTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_tax_rate_type");
        });

        modelBuilder.Entity<MstTaxRegion>(entity =>
        {
            entity.HasKey(e => e.RegionId).HasName("mst_tax_region_pkey");

            entity.ToTable("mst_tax_region", "press_db", tb => tb.HasComment("Geographic tax jurisdiction for GST inter-state / intra-state logic."));

            entity.HasIndex(e => e.CountryId, "idx_tax_region_country");

            entity.HasIndex(e => e.StateId, "idx_tax_region_state");

            entity.HasIndex(e => e.RegionCode, "mst_tax_region_region_code_key").IsUnique();

            entity.Property(e => e.RegionId).HasColumnName("region_id");
            entity.Property(e => e.CountryId).HasColumnName("country_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.RegionCode)
                .HasMaxLength(20)
                .HasColumnName("region_code");
            entity.Property(e => e.StateId).HasColumnName("state_id");

            entity.HasOne(d => d.Country).WithMany(p => p.MstTaxRegions)
                .HasForeignKey(d => d.CountryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_tax_region_country");

            entity.HasOne(d => d.State).WithMany(p => p.MstTaxRegions)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_tax_region_state");
        });

        modelBuilder.Entity<MstTaxType>(entity =>
        {
            entity.HasKey(e => e.TaxTypeId).HasName("mst_tax_type_pkey");

            entity.ToTable("mst_tax_type", "press_db", tb => tb.HasComment("Top-level tax classification: GST, VAT, TDS, TCS, Customs, etc."));

            entity.HasIndex(e => e.Code, "mst_tax_type_code_key").IsUnique();

            entity.Property(e => e.TaxTypeId).HasColumnName("tax_type_id");
            entity.Property(e => e.Code)
                .HasMaxLength(20)
                .HasColumnName("code");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsPercentage)
                .HasDefaultValue(true)
                .HasColumnName("is_percentage");
            entity.Property(e => e.IsRecoverable)
                .HasDefaultValue(true)
                .HasColumnName("is_recoverable");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<MstTransactionType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("mst_transaction_type_pkey");

            entity.ToTable("mst_transaction_type", "press_db", tb => tb.HasComment("Lookup for ERP transaction types. Referenced by trn_tax_ledger.transaction_type_id."));

            entity.HasIndex(e => e.Name, "uq_transaction_type_name").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("Primary key, auto-generated.")
                .HasColumnName("id");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasComment("TRUE = active and selectable in transactions.")
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasComment("Unique name of the transaction type.")
                .HasColumnName("name");
        });

        modelBuilder.Entity<MstUom>(entity =>
        {
            entity.HasKey(e => e.UomId).HasName("mst_uom_pkey");

            entity.ToTable("mst_uom", "press_db");

            entity.HasIndex(e => e.UomTypeId, "idx_uom_type");

            entity.HasIndex(e => e.UomCode, "mst_uom_uom_code_key").IsUnique();

            entity.Property(e => e.UomId).HasColumnName("uom_id");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.DecimalPlaces)
                .HasDefaultValue(2)
                .HasColumnName("decimal_places");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.UomCode)
                .HasMaxLength(20)
                .HasColumnName("uom_code");
            entity.Property(e => e.UomName)
                .HasMaxLength(100)
                .HasColumnName("uom_name");
            entity.Property(e => e.UomTypeId).HasColumnName("uom_type_id");

            entity.HasOne(d => d.UomType).WithMany(p => p.MstUoms)
                .HasForeignKey(d => d.UomTypeId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_uom_type");
        });

        modelBuilder.Entity<MstUomType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("mst_uom_type_pkey");

            entity.ToTable("mst_uom_type", "press_db");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<MstUser>(entity =>
        {
            entity.HasKey(e => e.Userid).HasName("mst_user_pkey");

            entity.ToTable("mst_user", "press_db");

            entity.HasIndex(e => e.Isactive, "idx_user_active");

            entity.HasIndex(e => e.UserCategory, "idx_user_category");

            entity.HasIndex(e => e.CompanyId, "idx_user_company");

            entity.HasIndex(e => e.Departmentid, "idx_user_department");

            entity.HasIndex(e => e.Designationid, "idx_user_designation");

            entity.HasIndex(e => e.EmployeeId, "idx_user_employee");

            entity.HasIndex(e => e.Locationid, "idx_user_location");

            entity.HasIndex(e => e.RefId, "idx_user_ref_id");

            entity.HasIndex(e => e.Reportinguserid, "idx_user_reporting");

            entity.HasIndex(e => e.ShiftTypeId, "idx_user_shift");

            entity.HasIndex(e => e.UserType, "idx_user_type");

            entity.HasIndex(e => e.Usercode, "mst_user_usercode_key").IsUnique();

            entity.HasIndex(e => e.Username, "mst_user_username_key").IsUnique();

            entity.Property(e => e.Userid).HasColumnName("userid");
            entity.Property(e => e.Accessfromtime).HasColumnName("accessfromtime");
            entity.Property(e => e.Accesstotime).HasColumnName("accesstotime");
            entity.Property(e => e.AiAlertCount)
                .HasDefaultValue(0)
                .HasComment("Number of active AI alerts for this user")
                .HasColumnName("ai_alert_count");
            entity.Property(e => e.AiAutoConfigured)
                .HasDefaultValue(false)
                .HasComment("Whether AI agent auto-configured roles/permissions based on dept+designation")
                .HasColumnName("ai_auto_configured");
            entity.Property(e => e.AiHealthScore)
                .HasDefaultValue(100)
                .HasComment("User configuration health score (0-100) calculated by AI Smart Agent")
                .HasColumnName("ai_health_score");
            entity.Property(e => e.AiLastReviewedAt)
                .HasComment("Last time AI agent reviewed this user configuration")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("ai_last_reviewed_at");
            entity.Property(e => e.Allowedgeolocation)
                .HasMaxLength(200)
                .HasColumnName("allowedgeolocation");
            entity.Property(e => e.Allowediprange)
                .HasMaxLength(200)
                .HasColumnName("allowediprange");
            entity.Property(e => e.Allowedlocationrangemeter)
                .HasDefaultValue(100)
                .HasColumnName("allowedlocationrangemeter");
            entity.Property(e => e.Approvallevel)
                .HasDefaultValue(0)
                .HasColumnName("approvallevel");
            entity.Property(e => e.Approvallimit)
                .HasPrecision(12, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("approvallimit");
            entity.Property(e => e.Appversion)
                .HasMaxLength(20)
                .HasColumnName("appversion");
            entity.Property(e => e.Canoverride)
                .HasDefaultValue(false)
                .HasColumnName("canoverride");
            entity.Property(e => e.CompanyId)
                .HasComment("Primary company assignment for multi-company ERP setups")
                .HasColumnName("company_id");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Createdby)
                .HasMaxLength(50)
                .HasColumnName("createdby");
            entity.Property(e => e.Departmentid).HasColumnName("departmentid");
            entity.Property(e => e.Designationid).HasColumnName("designationid");
            entity.Property(e => e.Deviceid)
                .HasMaxLength(200)
                .HasColumnName("deviceid");
            entity.Property(e => e.Deviceosversion)
                .HasMaxLength(50)
                .HasColumnName("deviceosversion");
            entity.Property(e => e.Devicetype)
                .HasMaxLength(50)
                .HasColumnName("devicetype");
            entity.Property(e => e.Emailid)
                .HasMaxLength(150)
                .HasColumnName("emailid");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.Employeecode)
                .HasMaxLength(50)
                .HasColumnName("employeecode");
            entity.Property(e => e.Exitdate).HasColumnName("exitdate");
            entity.Property(e => e.Failedlogincount)
                .HasDefaultValue(0)
                .HasColumnName("failedlogincount");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.Isapprovaluser)
                .HasDefaultValue(false)
                .HasColumnName("isapprovaluser");
            entity.Property(e => e.Isclientuser)
                .HasDefaultValue(false)
                .HasColumnName("isclientuser");
            entity.Property(e => e.Isdeleted)
                .HasDefaultValue(false)
                .HasColumnName("isdeleted");
            entity.Property(e => e.Isdevicebindingenabled)
                .HasDefaultValue(false)
                .HasColumnName("isdevicebindingenabled");
            entity.Property(e => e.Isgeorestrictionenabled)
                .HasDefaultValue(false)
                .HasColumnName("isgeorestrictionenabled");
            entity.Property(e => e.Islocked)
                .HasDefaultValue(false)
                .HasColumnName("islocked");
            entity.Property(e => e.Ismobileaccessallowed)
                .HasDefaultValue(false)
                .HasColumnName("ismobileaccessallowed");
            entity.Property(e => e.Ismultideviceallowed)
                .HasDefaultValue(false)
                .HasColumnName("ismultideviceallowed");
            entity.Property(e => e.Isotprequired)
                .HasDefaultValue(true)
                .HasColumnName("isotprequired");
            entity.Property(e => e.Ispermissiononleave)
                .HasDefaultValue(false)
                .HasColumnName("ispermissiononleave");
            entity.Property(e => e.Isproductionuser)
                .HasDefaultValue(false)
                .HasColumnName("isproductionuser");
            entity.Property(e => e.Issystemadmin)
                .HasDefaultValue(false)
                .HasColumnName("issystemadmin");
            entity.Property(e => e.Iswebaccessallowed)
                .HasDefaultValue(true)
                .HasColumnName("iswebaccessallowed");
            entity.Property(e => e.Joiningdate).HasColumnName("joiningdate");
            entity.Property(e => e.Lastdeviceip)
                .HasMaxLength(45)
                .HasColumnName("lastdeviceip");
            entity.Property(e => e.Lastdeviceloginat)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("lastdeviceloginat");
            entity.Property(e => e.Lastfailedloginat)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("lastfailedloginat");
            entity.Property(e => e.Lastloginat)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("lastloginat");
            entity.Property(e => e.Lastotpverifiedat)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("lastotpverifiedat");
            entity.Property(e => e.Lastpasswordchange)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("lastpasswordchange");
            entity.Property(e => e.Locationid).HasColumnName("locationid");
            entity.Property(e => e.MaxConcurrentSessions)
                .HasDefaultValue(1)
                .HasComment("Maximum simultaneous login sessions allowed")
                .HasColumnName("max_concurrent_sessions");
            entity.Property(e => e.Mobileno)
                .HasMaxLength(15)
                .HasColumnName("mobileno");
            entity.Property(e => e.Mobilesessiontimeoutmin)
                .HasDefaultValue(30)
                .HasColumnName("mobilesessiontimeoutmin");
            entity.Property(e => e.MustChangePassword)
                .HasDefaultValue(false)
                .HasColumnName("must_change_password");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Otp).HasColumnName("otp");
            entity.Property(e => e.Otpexpiryminutes)
                .HasDefaultValue(5)
                .HasColumnName("otpexpiryminutes");
            entity.Property(e => e.PasswordExpiresAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("password_expires_at");
            entity.Property(e => e.PasswordHistory)
                .HasComment("JSON array of previous password hashes to prevent reuse")
                .HasColumnType("jsonb")
                .HasColumnName("password_history");
            entity.Property(e => e.Passwordhash).HasColumnName("passwordhash");
            entity.Property(e => e.RefId)
                .HasComment("Reference ID to source entity: employee_id (mst_employee), party_id (mst_party), or other entity based on user_type")
                .HasColumnName("ref_id");
            entity.Property(e => e.Refreshtoken).HasColumnName("refreshtoken");
            entity.Property(e => e.Refreshtokenexpiry)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("refreshtokenexpiry");
            entity.Property(e => e.Registredmobile)
                .HasMaxLength(15)
                .HasColumnName("registredmobile");
            entity.Property(e => e.Reportinguserid).HasColumnName("reportinguserid");
            entity.Property(e => e.ShiftTypeId)
                .HasComment("FK to shift type - used by AI agent to set accessfromtime/accesstotime automatically")
                .HasColumnName("shift_type_id");
            entity.Property(e => e.Updatedat)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updatedat");
            entity.Property(e => e.Updatedby)
                .HasMaxLength(50)
                .HasColumnName("updatedby");
            entity.Property(e => e.UserCategory)
                .HasMaxLength(30)
                .HasDefaultValueSql("'INTERNAL'::character varying")
                .HasComment("AI classification: INTERNAL, CLIENT, VENDOR, CONTRACTOR, TEMPORARY")
                .HasColumnName("user_category");
            entity.Property(e => e.UserType)
                .HasMaxLength(20)
                .HasComment("User classification: EMPLOYEE, CUSTOMER, VENDOR, ADMIN, OTHER. Determines entity linkage and access scope")
                .HasColumnName("user_type");
            entity.Property(e => e.Usercode)
                .HasMaxLength(50)
                .HasColumnName("usercode");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .HasColumnName("username");

            entity.HasOne(d => d.Company).WithMany(p => p.MstUsers)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_user_company");

            entity.HasOne(d => d.Department).WithMany(p => p.MstUsers)
                .HasForeignKey(d => d.Departmentid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_user_department");

            entity.HasOne(d => d.Designation).WithMany(p => p.MstUsers)
                .HasForeignKey(d => d.Designationid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_user_designation");

            entity.HasOne(d => d.Employee).WithMany(p => p.MstUsers)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_user_employee");

            entity.HasOne(d => d.Location).WithMany(p => p.MstUsers)
                .HasForeignKey(d => d.Locationid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_user_location");

            entity.HasOne(d => d.Reportinguser).WithMany(p => p.InverseReportinguser)
                .HasForeignKey(d => d.Reportinguserid)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_user_reporting");

            entity.HasOne(d => d.ShiftType).WithMany(p => p.MstUsers)
                .HasForeignKey(d => d.ShiftTypeId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_user_shift_type");
        });

        modelBuilder.Entity<MstUserRole>(entity =>
        {
            entity.HasKey(e => e.UserRoleId).HasName("mst_user_role_pkey");

            entity.ToTable("mst_user_role", "press_db");

            entity.Property(e => e.UserRoleId).HasColumnName("user_role_id");
            entity.Property(e => e.AssignedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("assigned_at");
            entity.Property(e => e.DeptId).HasColumnName("dept_id");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsPrimary)
                .HasDefaultValue(true)
                .HasColumnName("is_primary");
            entity.Property(e => e.Roleid).HasColumnName("roleid");
            entity.Property(e => e.Userid).HasColumnName("userid");
        });

        modelBuilder.Entity<MstVendor>(entity =>
        {
            entity.HasKey(e => e.VendorId).HasName("mst_vendor_pkey");

            entity.ToTable("mst_vendor", "press_db");

            entity.HasIndex(e => e.PartyId, "idx_vendor_party");

            entity.HasIndex(e => e.VendorTypeId, "idx_vendor_type");

            entity.Property(e => e.VendorId).HasColumnName("vendor_id");
            entity.Property(e => e.ContractEndDate).HasColumnName("contract_end_date");
            entity.Property(e => e.ContractStartDate).HasColumnName("contract_start_date");
            entity.Property(e => e.ContractValue)
                .HasPrecision(18, 2)
                .HasColumnName("contract_value");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.ModifiedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.PartyId).HasColumnName("party_id");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.ServiceArea)
                .HasMaxLength(100)
                .HasColumnName("service_area");
            entity.Property(e => e.VendorTypeId).HasColumnName("vendor_type_id");

            entity.HasOne(d => d.Party).WithMany(p => p.MstVendors)
                .HasForeignKey(d => d.PartyId)
                .HasConstraintName("fk_vendor_party");

            entity.HasOne(d => d.VendorType).WithMany(p => p.MstVendors)
                .HasForeignKey(d => d.VendorTypeId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_vendor_type");
        });

        modelBuilder.Entity<MstVendorType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("mst_vendor_type_pkey");

            entity.ToTable("mst_vendor_type", "press_db");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<MstVoucherType>(entity =>
        {
            entity.HasKey(e => e.VoucherTypeId).HasName("mst_voucher_type_pkey");

            entity.ToTable("mst_voucher_type", "press_db");

            entity.HasIndex(e => e.VoucherCode, "mst_voucher_type_voucher_code_key").IsUnique();

            entity.Property(e => e.VoucherTypeId).HasColumnName("voucher_type_id");
            entity.Property(e => e.AffectsInventory)
                .HasDefaultValue(false)
                .HasColumnName("affects_inventory");
            entity.Property(e => e.AffectsParty)
                .HasDefaultValue(false)
                .HasColumnName("affects_party");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsAutoNumbering)
                .HasDefaultValue(true)
                .HasColumnName("is_auto_numbering");
            entity.Property(e => e.LastNumber)
                .HasDefaultValue(0)
                .HasColumnName("last_number");
            entity.Property(e => e.Prefix)
                .HasMaxLength(10)
                .HasColumnName("prefix");
            entity.Property(e => e.SortOrder)
                .HasDefaultValue(0)
                .HasColumnName("sort_order");
            entity.Property(e => e.Suffix)
                .HasMaxLength(10)
                .HasColumnName("suffix");
            entity.Property(e => e.TransactionNature)
                .HasMaxLength(20)
                .HasColumnName("transaction_nature");
            entity.Property(e => e.VoucherCode)
                .HasMaxLength(20)
                .HasColumnName("voucher_code");
            entity.Property(e => e.VoucherName)
                .HasMaxLength(100)
                .HasColumnName("voucher_name");
        });

        modelBuilder.Entity<MstWorkflowConnection>(entity =>
        {
            entity.HasKey(e => e.ConnectionId).HasName("mst_workflow_connection_pkey");

            entity.ToTable("mst_workflow_connection", "press_db", tb => tb.HasComment("Connections between workflow steps. Supports conditional branching via condition_expression."));

            entity.HasIndex(e => e.FromStepId, "idx_wf_conn_from");

            entity.HasIndex(e => e.WorkflowTemplateId, "idx_wf_conn_template");

            entity.HasIndex(e => e.ToStepId, "idx_wf_conn_to");

            entity.Property(e => e.ConnectionId).HasColumnName("connection_id");
            entity.Property(e => e.ConditionExpression).HasColumnName("condition_expression");
            entity.Property(e => e.FromStepId).HasColumnName("from_step_id");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Label)
                .HasMaxLength(200)
                .HasColumnName("label");
            entity.Property(e => e.SequenceNo)
                .HasDefaultValue(0)
                .HasColumnName("sequence_no");
            entity.Property(e => e.ToStepId).HasColumnName("to_step_id");
            entity.Property(e => e.WorkflowTemplateId).HasColumnName("workflow_template_id");

            entity.HasOne(d => d.FromStep).WithMany(p => p.MstWorkflowConnectionFromSteps)
                .HasForeignKey(d => d.FromStepId)
                .HasConstraintName("mst_workflow_connection_from_step_id_fkey");

            entity.HasOne(d => d.ToStep).WithMany(p => p.MstWorkflowConnectionToSteps)
                .HasForeignKey(d => d.ToStepId)
                .HasConstraintName("mst_workflow_connection_to_step_id_fkey");

            entity.HasOne(d => d.WorkflowTemplate).WithMany(p => p.MstWorkflowConnections)
                .HasForeignKey(d => d.WorkflowTemplateId)
                .HasConstraintName("mst_workflow_connection_workflow_template_id_fkey");
        });

        modelBuilder.Entity<MstWorkflowStep>(entity =>
        {
            entity.HasKey(e => e.WorkflowStepId).HasName("mst_workflow_step_pkey");

            entity.ToTable("mst_workflow_step", "press_db", tb => tb.HasComment("Individual steps within a workflow template. Each step has routing, assignment, notification, and visual position metadata."));

            entity.HasIndex(e => e.DepartmentId, "idx_wf_step_dept");

            entity.HasIndex(e => e.WorkflowTemplateId, "idx_wf_step_template");

            entity.HasIndex(e => e.StepType, "idx_wf_step_type");

            entity.HasIndex(e => new { e.WorkflowTemplateId, e.StepCode }, "uq_wf_step_template_code").IsUnique();

            entity.Property(e => e.WorkflowStepId).HasColumnName("workflow_step_id");
            entity.Property(e => e.AppliesToEnquiry)
                .HasDefaultValue(true)
                .HasComment("If TRUE, this step is included when workflow starts from an enquiry.")
                .HasColumnName("applies_to_enquiry");
            entity.Property(e => e.AppliesToJob)
                .HasDefaultValue(true)
                .HasComment("If TRUE, this step is included when workflow starts directly from a job.")
                .HasColumnName("applies_to_job");
            entity.Property(e => e.AppliesToQuotation)
                .HasDefaultValue(true)
                .HasComment("If TRUE, this step is included when workflow starts from a quotation.")
                .HasColumnName("applies_to_quotation");
            entity.Property(e => e.ApprovalLevelId).HasColumnName("approval_level_id");
            entity.Property(e => e.ApprovalTypeId).HasColumnName("approval_type_id");
            entity.Property(e => e.AssignedUserId).HasColumnName("assigned_user_id");
            entity.Property(e => e.AssignmentRule)
                .HasMaxLength(30)
                .HasColumnName("assignment_rule");
            entity.Property(e => e.CanvasX).HasColumnName("canvas_x");
            entity.Property(e => e.CanvasY).HasColumnName("canvas_y");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.EscalateAfterHours)
                .HasPrecision(8, 2)
                .HasColumnName("escalate_after_hours");
            entity.Property(e => e.EscalateTo)
                .HasMaxLength(200)
                .HasColumnName("escalate_to");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsBlocking)
                .HasDefaultValue(true)
                .HasComment("If TRUE, workflow cannot progress until this step is completed. If FALSE (non-blocking), workflow can proceed to next step even if this task is pending. Party-related tasks are typically non-blocking.")
                .HasColumnName("is_blocking");
            entity.Property(e => e.IsMandatory)
                .HasDefaultValue(false)
                .HasColumnName("is_mandatory");
            entity.Property(e => e.NodeColor)
                .HasMaxLength(20)
                .HasColumnName("node_color");
            entity.Property(e => e.NotifyAssignedUser)
                .HasDefaultValue(false)
                .HasColumnName("notify_assigned_user");
            entity.Property(e => e.NotifyCustomer)
                .HasDefaultValue(false)
                .HasColumnName("notify_customer");
            entity.Property(e => e.NotifyDeptHead)
                .HasDefaultValue(false)
                .HasColumnName("notify_dept_head");
            entity.Property(e => e.NotifySupplier)
                .HasDefaultValue(false)
                .HasColumnName("notify_supplier");
            entity.Property(e => e.NotifyVendor)
                .HasDefaultValue(false)
                .HasColumnName("notify_vendor");
            entity.Property(e => e.ProcessId).HasColumnName("process_id");
            entity.Property(e => e.SendEmail)
                .HasDefaultValue(false)
                .HasColumnName("send_email");
            entity.Property(e => e.SendPushNotification)
                .HasDefaultValue(false)
                .HasColumnName("send_push_notification");
            entity.Property(e => e.SendSms)
                .HasDefaultValue(false)
                .HasColumnName("send_sms");
            entity.Property(e => e.SendWhatsapp)
                .HasDefaultValue(false)
                .HasColumnName("send_whatsapp");
            entity.Property(e => e.SequenceNo)
                .HasDefaultValue(0)
                .HasColumnName("sequence_no");
            entity.Property(e => e.SlaHours)
                .HasPrecision(8, 2)
                .HasColumnName("sla_hours");
            entity.Property(e => e.StepCode)
                .HasMaxLength(50)
                .HasColumnName("step_code");
            entity.Property(e => e.StepName)
                .HasMaxLength(200)
                .HasColumnName("step_name");
            entity.Property(e => e.StepType)
                .HasMaxLength(30)
                .HasColumnName("step_type");
            entity.Property(e => e.SubProcessId).HasColumnName("sub_process_id");
            entity.Property(e => e.WorkflowTemplateId).HasColumnName("workflow_template_id");

            entity.HasOne(d => d.ApprovalLevel).WithMany(p => p.MstWorkflowSteps)
                .HasForeignKey(d => d.ApprovalLevelId)
                .HasConstraintName("mst_workflow_step_approval_level_id_fkey");

            entity.HasOne(d => d.ApprovalType).WithMany(p => p.MstWorkflowSteps)
                .HasForeignKey(d => d.ApprovalTypeId)
                .HasConstraintName("mst_workflow_step_approval_type_id_fkey");

            entity.HasOne(d => d.AssignedUser).WithMany(p => p.MstWorkflowSteps)
                .HasForeignKey(d => d.AssignedUserId)
                .HasConstraintName("mst_workflow_step_assigned_user_id_fkey");

            entity.HasOne(d => d.Department).WithMany(p => p.MstWorkflowSteps)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("mst_workflow_step_department_id_fkey");

            entity.HasOne(d => d.Process).WithMany(p => p.MstWorkflowSteps)
                .HasForeignKey(d => d.ProcessId)
                .HasConstraintName("mst_workflow_step_process_id_fkey");

            entity.HasOne(d => d.WorkflowTemplate).WithMany(p => p.MstWorkflowSteps)
                .HasForeignKey(d => d.WorkflowTemplateId)
                .HasConstraintName("mst_workflow_step_workflow_template_id_fkey");
        });

        modelBuilder.Entity<MstWorkflowTemplate>(entity =>
        {
            entity.HasKey(e => e.WorkflowTemplateId).HasName("mst_workflow_template_pkey");

            entity.ToTable("mst_workflow_template", "press_db", tb => tb.HasComment("Workflow definitions for job routing. Each template links a Job Type + Product Type to a sequence of steps."));

            entity.HasIndex(e => e.IsActive, "idx_wf_template_active").HasFilter("(is_active = true)");

            entity.HasIndex(e => e.JobTypeId, "idx_wf_template_jobtype");

            entity.HasIndex(e => e.PrintProductTypeId, "idx_wf_template_product");

            entity.HasIndex(e => e.WorkflowCode, "uq_wf_template_code").IsUnique();

            entity.Property(e => e.WorkflowTemplateId).HasColumnName("workflow_template_id");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDefault)
                .HasDefaultValue(false)
                .HasColumnName("is_default");
            entity.Property(e => e.JobTypeId).HasColumnName("job_type_id");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.PrintProductTypeId).HasColumnName("print_product_type_id");
            entity.Property(e => e.Version)
                .HasDefaultValue(1)
                .HasColumnName("version");
            entity.Property(e => e.WorkflowCode)
                .HasMaxLength(50)
                .HasColumnName("workflow_code");
            entity.Property(e => e.WorkflowName)
                .HasMaxLength(200)
                .HasColumnName("workflow_name");

            entity.HasOne(d => d.JobType).WithMany(p => p.MstWorkflowTemplates)
                .HasForeignKey(d => d.JobTypeId)
                .HasConstraintName("mst_workflow_template_job_type_id_fkey");

            entity.HasOne(d => d.PrintProductType).WithMany(p => p.MstWorkflowTemplates)
                .HasForeignKey(d => d.PrintProductTypeId)
                .HasConstraintName("mst_workflow_template_print_product_type_id_fkey");
        });

        modelBuilder.Entity<MstWorkspaceConfig>(entity =>
        {
            entity.HasKey(e => e.ConfigId).HasName("mst_workspace_config_pkey");

            entity.ToTable("mst_workspace_config", "press_db", tb => tb.HasComment("Per-user workspace configuration. Controls widget visibility, default filters, calendar view, notification preferences, pinned items and layout options for the My Workspace dashboard."));

            entity.HasIndex(e => e.UserId, "idx_workspace_config_user_id");

            entity.HasIndex(e => e.UserId, "mst_workspace_config_user_id_key").IsUnique();

            entity.Property(e => e.ConfigId).HasColumnName("config_id");
            entity.Property(e => e.AutoRefreshSeconds)
                .HasDefaultValue(60)
                .HasComment("Auto-refresh interval in seconds for workspace widgets. 0 disables auto-refresh.")
                .HasColumnName("auto_refresh_seconds");
            entity.Property(e => e.CompactMode)
                .HasDefaultValue(false)
                .HasColumnName("compact_mode");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.DefaultApprovalFilter)
                .HasMaxLength(20)
                .HasDefaultValueSql("'PENDING'::character varying")
                .HasColumnName("default_approval_filter");
            entity.Property(e => e.DefaultCalendarView)
                .HasMaxLength(20)
                .HasDefaultValueSql("'WEEKLY'::character varying")
                .HasComment("Default calendar view when workspace loads: DAILY, WEEKLY, MONTHLY")
                .HasColumnName("default_calendar_view");
            entity.Property(e => e.DefaultTaskFilter)
                .HasMaxLength(20)
                .HasDefaultValueSql("'PENDING'::character varying")
                .HasColumnName("default_task_filter");
            entity.Property(e => e.HistoryDays)
                .HasDefaultValue(30)
                .HasColumnName("history_days");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.ItemsPerPage)
                .HasDefaultValue(20)
                .HasColumnName("items_per_page");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.NotifyOnApprovalComplete)
                .HasDefaultValue(true)
                .HasColumnName("notify_on_approval_complete");
            entity.Property(e => e.NotifyOnApprovalRequest)
                .HasDefaultValue(true)
                .HasColumnName("notify_on_approval_request");
            entity.Property(e => e.NotifyOnTaskAssign)
                .HasDefaultValue(true)
                .HasColumnName("notify_on_task_assign");
            entity.Property(e => e.NotifyOnTaskOverdue)
                .HasDefaultValue(true)
                .HasColumnName("notify_on_task_overdue");
            entity.Property(e => e.PinnedJobs)
                .HasDefaultValueSql("'[]'::jsonb")
                .HasComment("JSON array of job_ids that user has pinned for quick access on workspace")
                .HasColumnType("jsonb")
                .HasColumnName("pinned_jobs");
            entity.Property(e => e.PinnedProcesses)
                .HasDefaultValueSql("'[]'::jsonb")
                .HasComment("JSON array of process_ids pinned by user for quick navigation")
                .HasColumnType("jsonb")
                .HasColumnName("pinned_processes");
            entity.Property(e => e.ShowApprovals)
                .HasDefaultValue(true)
                .HasColumnName("show_approvals");
            entity.Property(e => e.ShowAssignedTasks)
                .HasDefaultValue(true)
                .HasColumnName("show_assigned_tasks");
            entity.Property(e => e.ShowCalendar)
                .HasDefaultValue(true)
                .HasColumnName("show_calendar");
            entity.Property(e => e.ShowCompletedTasks)
                .HasDefaultValue(true)
                .HasColumnName("show_completed_tasks");
            entity.Property(e => e.ShowHistory)
                .HasDefaultValue(true)
                .HasColumnName("show_history");
            entity.Property(e => e.ShowNotifications)
                .HasDefaultValue(true)
                .HasColumnName("show_notifications");
            entity.Property(e => e.ShowPendingTasks)
                .HasDefaultValue(true)
                .HasColumnName("show_pending_tasks");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.WidgetOrder)
                .HasDefaultValueSql("'[\"PENDING_TASKS\", \"APPROVALS\", \"CALENDAR\", \"NOTIFICATIONS\", \"HISTORY\"]'::jsonb")
                .HasComment("JSON array defining display order of workspace widgets: PENDING_TASKS, APPROVALS, CALENDAR, NOTIFICATIONS, HISTORY")
                .HasColumnType("jsonb")
                .HasColumnName("widget_order");

            entity.HasOne(d => d.User).WithOne(p => p.MstWorkspaceConfig)
                .HasForeignKey<MstWorkspaceConfig>(d => d.UserId)
                .HasConstraintName("fk_workspace_config_user");
        });

        modelBuilder.Entity<PartyActivityLog>(entity =>
        {
            entity.HasKey(e => e.ActivityId).HasName("party_activity_log_pkey");

            entity.ToTable("party_activity_log", "press_db");

            entity.HasIndex(e => e.CreatedOn, "idx_party_activity_date");

            entity.HasIndex(e => e.PartyId, "idx_party_activity_party");

            entity.HasIndex(e => new { e.ReferenceTable, e.ReferenceId }, "idx_party_activity_reference");

            entity.HasIndex(e => e.ActivityType, "idx_party_activity_type");

            entity.Property(e => e.ActivityId).HasColumnName("activity_id");
            entity.Property(e => e.ActivityCode)
                .HasMaxLength(50)
                .HasColumnName("activity_code");
            entity.Property(e => e.ActivityDescription).HasColumnName("activity_description");
            entity.Property(e => e.ActivityTitle)
                .HasMaxLength(250)
                .HasColumnName("activity_title");
            entity.Property(e => e.ActivityType)
                .HasMaxLength(50)
                .HasColumnName("activity_type");
            entity.Property(e => e.Amount)
                .HasPrecision(18, 2)
                .HasColumnName("amount");
            entity.Property(e => e.ApprovalStatus)
                .HasMaxLength(50)
                .HasColumnName("approval_status");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.DocumentDate).HasColumnName("document_date");
            entity.Property(e => e.DocumentNo)
                .HasMaxLength(100)
                .HasColumnName("document_no");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.PartyId).HasColumnName("party_id");
            entity.Property(e => e.ReferenceId).HasColumnName("reference_id");
            entity.Property(e => e.ReferenceTable)
                .HasMaxLength(100)
                .HasColumnName("reference_table");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_on");

            entity.HasOne(d => d.Party).WithMany(p => p.PartyActivityLogs)
                .HasForeignKey(d => d.PartyId)
                .HasConstraintName("fk_party_activity_party");
        });

        modelBuilder.Entity<RptQueryPlan>(entity =>
        {
            entity.HasKey(e => e.QueryPlanId).HasName("rpt_query_plan_pkey");

            entity.ToTable("rpt_query_plan", "press_db");

            entity.HasIndex(e => e.ExecutedOn, "idx_rpt_query_plan_executed_on");

            entity.HasIndex(e => e.FilterJson, "idx_rpt_query_plan_filter_json").HasMethod("gin");

            entity.HasIndex(e => e.ParametersJson, "idx_rpt_query_plan_parameters_json").HasMethod("gin");

            entity.HasIndex(e => e.ReportId, "idx_rpt_query_plan_report_id");

            entity.HasIndex(e => e.ReportName, "idx_rpt_query_plan_report_name");

            entity.Property(e => e.QueryPlanId).HasColumnName("query_plan_id");
            entity.Property(e => e.ExecutedBy)
                .HasMaxLength(150)
                .HasColumnName("executed_by");
            entity.Property(e => e.ExecutedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("executed_on");
            entity.Property(e => e.ExecutionTimeMs)
                .HasDefaultValue(0)
                .HasColumnName("execution_time_ms");
            entity.Property(e => e.FilterJson)
                .HasColumnType("jsonb")
                .HasColumnName("filter_json");
            entity.Property(e => e.GeneratedSql).HasColumnName("generated_sql");
            entity.Property(e => e.GroupByClause).HasColumnName("group_by_clause");
            entity.Property(e => e.HavingClause).HasColumnName("having_clause");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.JoinClause).HasColumnName("join_clause");
            entity.Property(e => e.OrderByClause).HasColumnName("order_by_clause");
            entity.Property(e => e.ParametersJson)
                .HasColumnType("jsonb")
                .HasColumnName("parameters_json");
            entity.Property(e => e.ReportId).HasColumnName("report_id");
            entity.Property(e => e.ReportName)
                .HasMaxLength(200)
                .HasColumnName("report_name");
            entity.Property(e => e.RowCount)
                .HasDefaultValue(0L)
                .HasColumnName("row_count");
            entity.Property(e => e.SelectedColumns).HasColumnName("selected_columns");
            entity.Property(e => e.SourceTable)
                .HasMaxLength(150)
                .HasColumnName("source_table");
            entity.Property(e => e.WhereClause).HasColumnName("where_clause");

            entity.HasOne(d => d.Report).WithMany(p => p.RptQueryPlans)
                .HasForeignKey(d => d.ReportId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_rpt_query_plan_report");
        });

        modelBuilder.Entity<RptSavedReport>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("rpt_saved_reports_pkey");

            entity.ToTable("rpt_saved_reports", "press_db", tb => tb.HasComment("User-saved report definitions for the self-service report builder"));

            entity.HasIndex(e => e.ReportCode, "idx_rpt_saved_reports_code")
                .IsUnique()
                .HasFilter("is_active");

            entity.HasIndex(e => new { e.CreatedBy, e.IsActive }, "idx_rpt_saved_reports_user");

            entity.Property(e => e.ReportId).HasColumnName("report_id");
            entity.Property(e => e.AiSummaryPrompt).HasColumnName("ai_summary_prompt");
            entity.Property(e => e.ChartConfig).HasColumnName("chart_config");
            entity.Property(e => e.ChartType)
                .HasMaxLength(50)
                .HasColumnName("chart_type");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.GroupByColumns).HasColumnName("group_by_columns");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDefault)
                .HasDefaultValue(false)
                .HasColumnName("is_default");
            entity.Property(e => e.IsShared)
                .HasDefaultValue(false)
                .HasColumnName("is_shared");
            entity.Property(e => e.JoinedTables)
                .HasComment("JSON array of joined tables with FK/PK mapping")
                .HasColumnName("joined_tables");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.OrderByColumns).HasColumnName("order_by_columns");
            entity.Property(e => e.PageSize)
                .HasDefaultValue(25)
                .HasColumnName("page_size");
            entity.Property(e => e.ReportCode)
                .HasMaxLength(50)
                .HasColumnName("report_code");
            entity.Property(e => e.ReportName)
                .HasMaxLength(200)
                .HasColumnName("report_name");
            entity.Property(e => e.ReportType)
                .HasMaxLength(20)
                .HasDefaultValueSql("'detail'::character varying")
                .HasComment("detail = row-level, summary = grouped aggregates")
                .HasColumnName("report_type");
            entity.Property(e => e.ShowGrandTotal)
                .HasDefaultValue(false)
                .HasComment("Show grand total row at the bottom")
                .HasColumnName("show_grand_total");
            entity.Property(e => e.ShowTotals)
                .HasDefaultValue(false)
                .HasComment("Show column totals for numeric columns")
                .HasColumnName("show_totals");
            entity.Property(e => e.SourceTable)
                .HasMaxLength(200)
                .HasColumnName("source_table");
        });

        modelBuilder.Entity<RptSavedReportColumn>(entity =>
        {
            entity.HasKey(e => e.ReportColumnId).HasName("rpt_saved_report_columns_pkey");

            entity.ToTable("rpt_saved_report_columns", "press_db", tb => tb.HasComment("Column selection and display config for saved reports"));

            entity.HasIndex(e => e.ReportId, "idx_rpt_saved_report_columns_report");

            entity.Property(e => e.ReportColumnId).HasColumnName("report_column_id");
            entity.Property(e => e.AggregateFunction)
                .HasMaxLength(20)
                .HasColumnName("aggregate_function");
            entity.Property(e => e.ColumnName)
                .HasMaxLength(200)
                .HasColumnName("column_name");
            entity.Property(e => e.ColumnOrder)
                .HasDefaultValue(0)
                .HasColumnName("column_order");
            entity.Property(e => e.ColumnWidth).HasColumnName("column_width");
            entity.Property(e => e.DisplayName)
                .HasMaxLength(200)
                .HasColumnName("display_name");
            entity.Property(e => e.FormatString)
                .HasMaxLength(100)
                .HasColumnName("format_string");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsVisible)
                .HasDefaultValue(true)
                .HasColumnName("is_visible");
            entity.Property(e => e.ReportId).HasColumnName("report_id");

            entity.HasOne(d => d.Report).WithMany(p => p.RptSavedReportColumns)
                .HasForeignKey(d => d.ReportId)
                .HasConstraintName("rpt_saved_report_columns_report_id_fkey");
        });

        modelBuilder.Entity<RptSavedReportFilter>(entity =>
        {
            entity.HasKey(e => e.ReportFilterId).HasName("rpt_saved_report_filters_pkey");

            entity.ToTable("rpt_saved_report_filters", "press_db", tb => tb.HasComment("Filter conditions for saved reports"));

            entity.HasIndex(e => e.ReportId, "idx_rpt_saved_report_filters_report");

            entity.Property(e => e.ReportFilterId).HasColumnName("report_filter_id");
            entity.Property(e => e.ColumnName)
                .HasMaxLength(200)
                .HasColumnName("column_name");
            entity.Property(e => e.FilterOrder)
                .HasDefaultValue(0)
                .HasColumnName("filter_order");
            entity.Property(e => e.FilterValue).HasColumnName("filter_value");
            entity.Property(e => e.FilterValue2).HasColumnName("filter_value2");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.LogicOperator)
                .HasMaxLength(5)
                .HasDefaultValueSql("'AND'::character varying")
                .HasColumnName("logic_operator");
            entity.Property(e => e.Operator)
                .HasMaxLength(30)
                .HasDefaultValueSql("'eq'::character varying")
                .HasColumnName("operator");
            entity.Property(e => e.ReportId).HasColumnName("report_id");

            entity.HasOne(d => d.Report).WithMany(p => p.RptSavedReportFilters)
                .HasForeignKey(d => d.ReportId)
                .HasConstraintName("rpt_saved_report_filters_report_id_fkey");
        });

        modelBuilder.Entity<SysErrorLog>(entity =>
        {
            entity.HasKey(e => e.ErrorLogId).HasName("sys_error_log_pkey");

            entity.ToTable("sys_error_log", "press_db", tb => tb.HasComment("Centralized error log for capturing exceptions across all application layers"));

            entity.HasIndex(e => e.CorrelationId, "idx_sys_err_correlation");

            entity.HasIndex(e => e.CreatedOn, "idx_sys_err_created");

            entity.HasIndex(e => e.Layer, "idx_sys_err_layer");

            entity.HasIndex(e => e.Severity, "idx_sys_err_severity");

            entity.HasIndex(e => e.IsReviewed, "idx_sys_err_unreviewed").HasFilter("(is_reviewed = false)");

            entity.HasIndex(e => e.UserId, "idx_sys_err_user");

            entity.Property(e => e.ErrorLogId).HasColumnName("error_log_id");
            entity.Property(e => e.AdditionalData).HasColumnName("additional_data");
            entity.Property(e => e.AppVersion)
                .HasMaxLength(50)
                .HasColumnName("app_version");
            entity.Property(e => e.CorrelationId)
                .HasMaxLength(50)
                .HasComment("Unique ID for tracing related log entries across services")
                .HasColumnName("correlation_id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.ExceptionType)
                .HasMaxLength(500)
                .HasColumnName("exception_type");
            entity.Property(e => e.HttpMethod)
                .HasMaxLength(10)
                .HasColumnName("http_method");
            entity.Property(e => e.InnerException).HasColumnName("inner_exception");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(50)
                .HasColumnName("ip_address");
            entity.Property(e => e.IsReviewed)
                .HasDefaultValue(false)
                .HasColumnName("is_reviewed");
            entity.Property(e => e.Layer)
                .HasMaxLength(50)
                .HasComment("Application layer: UI, API, Infrastructure, Application, Persistence")
                .HasColumnName("layer");
            entity.Property(e => e.MachineName)
                .HasMaxLength(100)
                .HasColumnName("machine_name");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.MethodName)
                .HasMaxLength(500)
                .HasColumnName("method_name");
            entity.Property(e => e.RequestData).HasColumnName("request_data");
            entity.Property(e => e.RequestPath)
                .HasMaxLength(500)
                .HasColumnName("request_path");
            entity.Property(e => e.ReviewNotes)
                .HasDefaultValueSql("''::text")
                .HasColumnName("review_notes");
            entity.Property(e => e.ReviewedBy)
                .HasMaxLength(100)
                .HasDefaultValueSql("''::character varying")
                .HasColumnName("reviewed_by");
            entity.Property(e => e.ReviewedOn)
                .HasDefaultValueSql("'1900-01-01 00:00:00'::timestamp without time zone")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("reviewed_on");
            entity.Property(e => e.Severity)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Error'::character varying")
                .HasComment("Severity level: Critical, Error, Warning, Info")
                .HasColumnName("severity");
            entity.Property(e => e.Source)
                .HasMaxLength(500)
                .HasComment("Source component: Controller name, Service class, Repository, Blazor component, etc.")
                .HasColumnName("source");
            entity.Property(e => e.StackTrace).HasColumnName("stack_trace");
            entity.Property(e => e.TenantKey)
                .HasMaxLength(50)
                .HasColumnName("tenant_key");
            entity.Property(e => e.UserAgent)
                .HasMaxLength(500)
                .HasColumnName("user_agent");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.UserName)
                .HasMaxLength(100)
                .HasColumnName("user_name");
        });

        modelBuilder.Entity<TrnAccountLedger>(entity =>
        {
            entity.HasKey(e => e.LedgerEntryId).HasName("trn_account_ledger_pkey");

            entity.ToTable("trn_account_ledger", "press_db", tb => tb.HasComment("General ledger — one row per journal line per account. Powers Ledger report, Trial Balance, P&L, Balance Sheet. Updated on journal posting."));

            entity.HasIndex(e => e.AccountId, "idx_acct_ledger_account");

            entity.HasIndex(e => new { e.AccountId, e.PartyId }, "idx_acct_ledger_acct_party");

            entity.HasIndex(e => e.CompanyId, "idx_acct_ledger_company");

            entity.HasIndex(e => e.FinYearId, "idx_acct_ledger_fin_year");

            entity.HasIndex(e => e.JournalId, "idx_acct_ledger_journal");

            entity.HasIndex(e => e.PartyId, "idx_acct_ledger_party");

            entity.HasIndex(e => e.PostingDate, "idx_acct_ledger_posting_dt");

            entity.HasIndex(e => new { e.VoucherType, e.VoucherNo }, "idx_acct_ledger_voucher");

            entity.Property(e => e.LedgerEntryId).HasColumnName("ledger_entry_id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.BalanceType)
                .HasMaxLength(10)
                .HasColumnName("balance_type");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.CostCenterId).HasColumnName("cost_center_id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.CreditAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("credit_amount");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.DebitAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("debit_amount");
            entity.Property(e => e.FinYearId).HasColumnName("fin_year_id");
            entity.Property(e => e.IsOpeningEntry)
                .HasDefaultValue(false)
                .HasColumnName("is_opening_entry");
            entity.Property(e => e.JournalId).HasColumnName("journal_id");
            entity.Property(e => e.JournalLineId).HasColumnName("journal_line_id");
            entity.Property(e => e.Narration).HasColumnName("narration");
            entity.Property(e => e.PartyId).HasColumnName("party_id");
            entity.Property(e => e.PostingDate).HasColumnName("posting_date");
            entity.Property(e => e.ReferenceId).HasColumnName("reference_id");
            entity.Property(e => e.ReferenceNo)
                .HasMaxLength(100)
                .HasColumnName("reference_no");
            entity.Property(e => e.ReferenceType)
                .HasMaxLength(50)
                .HasColumnName("reference_type");
            entity.Property(e => e.RunningBalance)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("running_balance");
            entity.Property(e => e.VoucherNo)
                .HasMaxLength(50)
                .HasColumnName("voucher_no");
            entity.Property(e => e.VoucherType)
                .HasMaxLength(50)
                .HasColumnName("voucher_type");

            entity.HasOne(d => d.Account).WithMany(p => p.TrnAccountLedgers)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_acct_ledger_account");

            entity.HasOne(d => d.Company).WithMany(p => p.TrnAccountLedgers)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_acct_ledger_company");

            entity.HasOne(d => d.Currency).WithMany(p => p.TrnAccountLedgers)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_acct_ledger_currency");

            entity.HasOne(d => d.FinYear).WithMany(p => p.TrnAccountLedgers)
                .HasForeignKey(d => d.FinYearId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_acct_ledger_fin_year");

            entity.HasOne(d => d.Party).WithMany(p => p.TrnAccountLedgers)
                .HasForeignKey(d => d.PartyId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_acct_ledger_party");
        });

        modelBuilder.Entity<TrnAdvanceLedger>(entity =>
        {
            entity.HasKey(e => e.AdvanceId).HasName("trn_advance_ledger_pkey");

            entity.ToTable("trn_advance_ledger", "press_db", tb => tb.HasComment("Advance received from customers or paid to suppliers. Tracks adjustment status against invoices."));

            entity.HasIndex(e => e.CompanyId, "idx_adv_ledger_company");

            entity.HasIndex(e => e.FinYearId, "idx_adv_ledger_fin_year");

            entity.HasIndex(e => e.PartyId, "idx_adv_ledger_party");

            entity.HasIndex(e => new { e.PartyId, e.PartyType }, "idx_adv_ledger_party_type");

            entity.HasIndex(e => e.IsFullyAdjusted, "idx_adv_ledger_unadjusted").HasFilter("(is_fully_adjusted = false)");

            entity.Property(e => e.AdvanceId).HasColumnName("advance_id");
            entity.Property(e => e.AdjustedAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("adjusted_amount");
            entity.Property(e => e.AdvanceAmount)
                .HasPrecision(18, 2)
                .HasColumnName("advance_amount");
            entity.Property(e => e.AdvanceDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("advance_date");
            entity.Property(e => e.BankPaymentId).HasColumnName("bank_payment_id");
            entity.Property(e => e.BankReceiptId).HasColumnName("bank_receipt_id");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.FinYearId).HasColumnName("fin_year_id");
            entity.Property(e => e.IsFullyAdjusted)
                .HasDefaultValue(false)
                .HasColumnName("is_fully_adjusted");
            entity.Property(e => e.Narration).HasColumnName("narration");
            entity.Property(e => e.PartyId).HasColumnName("party_id");
            entity.Property(e => e.PartyType)
                .HasMaxLength(20)
                .HasColumnName("party_type");
            entity.Property(e => e.PaymentVoucherId).HasColumnName("payment_voucher_id");
            entity.Property(e => e.ReceiptVoucherId).HasColumnName("receipt_voucher_id");
            entity.Property(e => e.UnadjustedAmount)
                .HasPrecision(18, 2)
                .HasComputedColumnSql("(advance_amount - adjusted_amount)", true)
                .HasColumnName("unadjusted_amount");
            entity.Property(e => e.VoucherType)
                .HasMaxLength(30)
                .HasColumnName("voucher_type");

            entity.HasOne(d => d.Company).WithMany(p => p.TrnAdvanceLedgers)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_advance_ledger_company");

            entity.HasOne(d => d.Party).WithMany(p => p.TrnAdvanceLedgers)
                .HasForeignKey(d => d.PartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_advance_ledger_party");
        });

        modelBuilder.Entity<TrnAiAgentActivity>(entity =>
        {
            entity.HasKey(e => e.ActivityId).HasName("trn_ai_agent_activity_pkey");

            entity.ToTable("trn_ai_agent_activity", "press_db");

            entity.HasIndex(e => e.CreatedOn, "idx_ai_agent_created");

            entity.HasIndex(e => e.Module, "idx_ai_agent_module");

            entity.HasIndex(e => new { e.AgentName, e.AgentAction }, "idx_ai_agent_name_action");

            entity.HasIndex(e => e.UserId, "idx_ai_agent_user");

            entity.Property(e => e.ActivityId).HasColumnName("activity_id");
            entity.Property(e => e.AgentAction)
                .HasMaxLength(50)
                .HasColumnName("agent_action");
            entity.Property(e => e.AgentName)
                .HasMaxLength(50)
                .HasColumnName("agent_name");
            entity.Property(e => e.ConfidenceScore)
                .HasPrecision(5, 2)
                .HasColumnName("confidence_score");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.ExecutionTimeMs).HasColumnName("execution_time_ms");
            entity.Property(e => e.Feedback).HasColumnName("feedback");
            entity.Property(e => e.InputJson)
                .HasColumnType("jsonb")
                .HasColumnName("input_json");
            entity.Property(e => e.Module)
                .HasMaxLength(50)
                .HasColumnName("module");
            entity.Property(e => e.OutputJson)
                .HasColumnType("jsonb")
                .HasColumnName("output_json");
            entity.Property(e => e.ReferenceId).HasColumnName("reference_id");
            entity.Property(e => e.ReferenceNo)
                .HasMaxLength(50)
                .HasColumnName("reference_no");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.WasAccepted).HasColumnName("was_accepted");

            entity.HasOne(d => d.User).WithMany(p => p.TrnAiAgentActivities)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_ai_activity_user");
        });

        modelBuilder.Entity<TrnAiNotificationLog>(entity =>
        {
            entity.HasKey(e => e.AiLogId).HasName("trn_ai_notification_log_pkey");

            entity.ToTable("trn_ai_notification_log", "press_db");

            entity.HasIndex(e => e.AiAction, "idx_ai_notif_log_action");

            entity.HasIndex(e => e.NotificationId, "idx_ai_notif_log_notif");

            entity.Property(e => e.AiLogId).HasColumnName("ai_log_id");
            entity.Property(e => e.AiAction)
                .HasMaxLength(50)
                .HasColumnName("ai_action");
            entity.Property(e => e.AiConfidence)
                .HasPrecision(5, 2)
                .HasColumnName("ai_confidence");
            entity.Property(e => e.AiModel)
                .HasMaxLength(50)
                .HasColumnName("ai_model");
            entity.Property(e => e.AiPrompt).HasColumnName("ai_prompt");
            entity.Property(e => e.AiResponse).HasColumnName("ai_response");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.LatencyMs).HasColumnName("latency_ms");
            entity.Property(e => e.NotificationId).HasColumnName("notification_id");
            entity.Property(e => e.ReviewedBy).HasColumnName("reviewed_by");
            entity.Property(e => e.TokensUsed).HasColumnName("tokens_used");
            entity.Property(e => e.WasApproved)
                .HasDefaultValue(true)
                .HasColumnName("was_approved");

            entity.HasOne(d => d.Notification).WithMany(p => p.TrnAiNotificationLogs)
                .HasForeignKey(d => d.NotificationId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_ai_notif_log_notification");
        });

        modelBuilder.Entity<TrnApOutstanding>(entity =>
        {
            entity.HasKey(e => e.ApId).HasName("trn_ap_outstanding_pkey");

            entity.ToTable("trn_ap_outstanding", "press_db", tb => tb.HasComment("Accounts Payable outstanding tracker. One row per purchase invoice/debit note. Updated on payment/allocation. Powers AP aging report, vendor statement, payment scheduling."));

            entity.HasIndex(e => e.CompanyId, "idx_ap_company");

            entity.HasIndex(e => e.DueDate, "idx_ap_due_date");

            entity.HasIndex(e => e.PartyId, "idx_ap_party");

            entity.HasIndex(e => e.Status, "idx_ap_status");

            entity.Property(e => e.ApId).HasColumnName("ap_id");
            entity.Property(e => e.AdjustedAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("adjusted_amount");
            entity.Property(e => e.AgingBucket)
                .HasMaxLength(20)
                .HasComment("CURRENT, 1-30, 31-60, 61-90, 91-120, 120+")
                .HasColumnName("aging_bucket");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.DocumentDate).HasColumnName("document_date");
            entity.Property(e => e.DocumentId).HasColumnName("document_id");
            entity.Property(e => e.DocumentNo)
                .HasMaxLength(50)
                .HasColumnName("document_no");
            entity.Property(e => e.DocumentType)
                .HasMaxLength(30)
                .HasComment("PURCHASE_INVOICE, DEBIT_NOTE, CREDIT_NOTE")
                .HasColumnName("document_type");
            entity.Property(e => e.DueDate).HasColumnName("due_date");
            entity.Property(e => e.FinYearId).HasColumnName("fin_year_id");
            entity.Property(e => e.IsFullySettled)
                .HasDefaultValue(false)
                .HasColumnName("is_fully_settled");
            entity.Property(e => e.LastPaymentDate).HasColumnName("last_payment_date");
            entity.Property(e => e.ModifiedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.OriginalAmount)
                .HasPrecision(18, 2)
                .HasColumnName("original_amount");
            entity.Property(e => e.OutstandingAmount)
                .HasPrecision(18, 2)
                .HasComputedColumnSql("((((original_amount - paid_amount) - adjusted_amount) - tds_amount) - write_off_amount)", true)
                .HasColumnName("outstanding_amount");
            entity.Property(e => e.OverdueDays)
                .HasDefaultValue(0)
                .HasColumnName("overdue_days");
            entity.Property(e => e.PaidAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("paid_amount");
            entity.Property(e => e.PartyId).HasColumnName("party_id");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'OPEN'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            entity.Property(e => e.TdsAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("tds_amount");
            entity.Property(e => e.WriteOffAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("write_off_amount");

            entity.HasOne(d => d.Company).WithMany(p => p.TrnApOutstandings)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ap_company");

            entity.HasOne(d => d.Party).WithMany(p => p.TrnApOutstandings)
                .HasForeignKey(d => d.PartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ap_party");
        });

        modelBuilder.Entity<TrnArOutstanding>(entity =>
        {
            entity.HasKey(e => e.ArId).HasName("trn_ar_outstanding_pkey");

            entity.ToTable("trn_ar_outstanding", "press_db", tb => tb.HasComment("Accounts Receivable outstanding tracker. One row per sales invoice/credit note. Updated on receipt/allocation. Powers AR aging report, customer statement, collection follow-up."));

            entity.HasIndex(e => e.CompanyId, "idx_ar_company");

            entity.HasIndex(e => e.DueDate, "idx_ar_due_date");

            entity.HasIndex(e => e.PartyId, "idx_ar_party");

            entity.HasIndex(e => e.Status, "idx_ar_status");

            entity.Property(e => e.ArId).HasColumnName("ar_id");
            entity.Property(e => e.AdjustedAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("adjusted_amount");
            entity.Property(e => e.AgingBucket)
                .HasMaxLength(20)
                .HasComment("CURRENT, 1-30, 31-60, 61-90, 91-120, 120+")
                .HasColumnName("aging_bucket");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.DocumentDate).HasColumnName("document_date");
            entity.Property(e => e.DocumentId).HasColumnName("document_id");
            entity.Property(e => e.DocumentNo)
                .HasMaxLength(50)
                .HasColumnName("document_no");
            entity.Property(e => e.DocumentType)
                .HasMaxLength(30)
                .HasComment("SALES_INVOICE, CREDIT_NOTE, DEBIT_NOTE")
                .HasColumnName("document_type");
            entity.Property(e => e.DueDate).HasColumnName("due_date");
            entity.Property(e => e.FinYearId).HasColumnName("fin_year_id");
            entity.Property(e => e.IsFullySettled)
                .HasDefaultValue(false)
                .HasColumnName("is_fully_settled");
            entity.Property(e => e.LastPaymentDate).HasColumnName("last_payment_date");
            entity.Property(e => e.LastReminderDate).HasColumnName("last_reminder_date");
            entity.Property(e => e.ModifiedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.OriginalAmount)
                .HasPrecision(18, 2)
                .HasColumnName("original_amount");
            entity.Property(e => e.OutstandingAmount)
                .HasPrecision(18, 2)
                .HasComputedColumnSql("(((original_amount - paid_amount) - adjusted_amount) - write_off_amount)", true)
                .HasColumnName("outstanding_amount");
            entity.Property(e => e.OverdueDays)
                .HasDefaultValue(0)
                .HasColumnName("overdue_days");
            entity.Property(e => e.PaidAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("paid_amount");
            entity.Property(e => e.PartyId).HasColumnName("party_id");
            entity.Property(e => e.ReminderCount)
                .HasDefaultValue(0)
                .HasColumnName("reminder_count");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'OPEN'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.WriteOffAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("write_off_amount");

            entity.HasOne(d => d.Company).WithMany(p => p.TrnArOutstandings)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ar_company");

            entity.HasOne(d => d.Party).WithMany(p => p.TrnArOutstandings)
                .HasForeignKey(d => d.PartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ar_party");
        });

        modelBuilder.Entity<TrnBankPayment>(entity =>
        {
            entity.HasKey(e => e.BankPaymentId).HasName("trn_bank_payment_pkey");

            entity.ToTable("trn_bank_payment", "press_db", tb => tb.HasComment("Bank payment voucher for money paid from company bank account to supplier/vendor. Supports cheque, NEFT, RTGS, UPI."));

            entity.HasIndex(e => e.BankAccountId, "idx_bp_bank");

            entity.HasIndex(e => e.PaymentDate, "idx_bp_date");

            entity.HasIndex(e => e.PartyId, "idx_bp_party");

            entity.HasIndex(e => e.PaymentNo, "uq_bank_payment_no").IsUnique();

            entity.Property(e => e.BankPaymentId).HasColumnName("bank_payment_id");
            entity.Property(e => e.AccountHeadId).HasColumnName("account_head_id");
            entity.Property(e => e.Amount)
                .HasPrecision(18, 2)
                .HasColumnName("amount");
            entity.Property(e => e.BankAccountId).HasColumnName("bank_account_id");
            entity.Property(e => e.CancelReason).HasColumnName("cancel_reason");
            entity.Property(e => e.CancelledBy).HasColumnName("cancelled_by");
            entity.Property(e => e.CancelledOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("cancelled_on");
            entity.Property(e => e.ChequeDate).HasColumnName("cheque_date");
            entity.Property(e => e.ChequeNo)
                .HasMaxLength(30)
                .HasColumnName("cheque_no");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.FinYearId).HasColumnName("fin_year_id");
            entity.Property(e => e.GlPostedBy).HasColumnName("gl_posted_by");
            entity.Property(e => e.GlPostedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("gl_posted_on");
            entity.Property(e => e.IsAdvance)
                .HasDefaultValue(false)
                .HasColumnName("is_advance");
            entity.Property(e => e.IsCancelled)
                .HasDefaultValue(false)
                .HasColumnName("is_cancelled");
            entity.Property(e => e.IsPostedToGl)
                .HasDefaultValue(false)
                .HasColumnName("is_posted_to_gl");
            entity.Property(e => e.IsReconciled)
                .HasDefaultValue(false)
                .HasColumnName("is_reconciled");
            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.Narration).HasColumnName("narration");
            entity.Property(e => e.NetAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("net_amount");
            entity.Property(e => e.PaidTo)
                .HasMaxLength(200)
                .HasColumnName("paid_to");
            entity.Property(e => e.PartyId).HasColumnName("party_id");
            entity.Property(e => e.PaymentDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("payment_date");
            entity.Property(e => e.PaymentMode)
                .HasMaxLength(30)
                .HasColumnName("payment_mode");
            entity.Property(e => e.PaymentNo)
                .HasMaxLength(50)
                .HasColumnName("payment_no");
            entity.Property(e => e.ReconciledOn).HasColumnName("reconciled_on");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'POSTED'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.TdsAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("tds_amount");
            entity.Property(e => e.TransactionRefNo)
                .HasMaxLength(100)
                .HasColumnName("transaction_ref_no");

            entity.HasOne(d => d.BankAccount).WithMany(p => p.TrnBankPayments)
                .HasForeignKey(d => d.BankAccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_bp_bank");

            entity.HasOne(d => d.Company).WithMany(p => p.TrnBankPayments)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_bp_company");
        });

        modelBuilder.Entity<TrnBankPaymentAllocation>(entity =>
        {
            entity.HasKey(e => e.AllocationId).HasName("trn_bank_payment_allocation_pkey");

            entity.ToTable("trn_bank_payment_allocation", "press_db", tb => tb.HasComment("Allocation of bank payment against purchase invoices or advance adjustments."));

            entity.HasIndex(e => e.BankPaymentId, "idx_bpa_payment");

            entity.Property(e => e.AllocationId).HasColumnName("allocation_id");
            entity.Property(e => e.AllocatedAmount)
                .HasPrecision(18, 2)
                .HasColumnName("allocated_amount");
            entity.Property(e => e.AllocationAgainst)
                .HasMaxLength(30)
                .HasComment("PURCHASE_INVOICE, DEBIT_NOTE, ADVANCE, EXPENSE, OTHER")
                .HasColumnName("allocation_against");
            entity.Property(e => e.BankPaymentId).HasColumnName("bank_payment_id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.RefDate).HasColumnName("ref_date");
            entity.Property(e => e.RefId).HasColumnName("ref_id");
            entity.Property(e => e.RefNo)
                .HasMaxLength(50)
                .HasColumnName("ref_no");

            entity.HasOne(d => d.BankPayment).WithMany(p => p.TrnBankPaymentAllocations)
                .HasForeignKey(d => d.BankPaymentId)
                .HasConstraintName("fk_bpa_payment");
        });

        modelBuilder.Entity<TrnBankReceipt>(entity =>
        {
            entity.HasKey(e => e.BankReceiptId).HasName("trn_bank_receipt_pkey");

            entity.ToTable("trn_bank_receipt", "press_db", tb => tb.HasComment("Bank receipt voucher for money received into company bank account. Supports cheque, NEFT, RTGS, UPI. Links to party and optional AR allocation."));

            entity.HasIndex(e => e.BankAccountId, "idx_br_bank");

            entity.HasIndex(e => e.ReceiptDate, "idx_br_date");

            entity.HasIndex(e => e.PartyId, "idx_br_party");

            entity.HasIndex(e => e.ReceiptNo, "uq_bank_receipt_no").IsUnique();

            entity.Property(e => e.BankReceiptId).HasColumnName("bank_receipt_id");
            entity.Property(e => e.AccountHeadId).HasColumnName("account_head_id");
            entity.Property(e => e.Amount)
                .HasPrecision(18, 2)
                .HasColumnName("amount");
            entity.Property(e => e.BankAccountId).HasColumnName("bank_account_id");
            entity.Property(e => e.CancelReason).HasColumnName("cancel_reason");
            entity.Property(e => e.CancelledBy).HasColumnName("cancelled_by");
            entity.Property(e => e.CancelledOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("cancelled_on");
            entity.Property(e => e.ChequeDate).HasColumnName("cheque_date");
            entity.Property(e => e.ChequeNo)
                .HasMaxLength(30)
                .HasColumnName("cheque_no");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.FinYearId).HasColumnName("fin_year_id");
            entity.Property(e => e.GlPostedBy).HasColumnName("gl_posted_by");
            entity.Property(e => e.GlPostedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("gl_posted_on");
            entity.Property(e => e.IsAdvance)
                .HasDefaultValue(false)
                .HasColumnName("is_advance");
            entity.Property(e => e.IsCancelled)
                .HasDefaultValue(false)
                .HasColumnName("is_cancelled");
            entity.Property(e => e.IsPostedToGl)
                .HasDefaultValue(false)
                .HasColumnName("is_posted_to_gl");
            entity.Property(e => e.IsReconciled)
                .HasDefaultValue(false)
                .HasColumnName("is_reconciled");
            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.Narration).HasColumnName("narration");
            entity.Property(e => e.NetAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("net_amount");
            entity.Property(e => e.PartyId).HasColumnName("party_id");
            entity.Property(e => e.PaymentMode)
                .HasMaxLength(30)
                .HasColumnName("payment_mode");
            entity.Property(e => e.ReceiptDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("receipt_date");
            entity.Property(e => e.ReceiptNo)
                .HasMaxLength(50)
                .HasColumnName("receipt_no");
            entity.Property(e => e.ReceivedFrom)
                .HasMaxLength(200)
                .HasColumnName("received_from");
            entity.Property(e => e.ReconciledOn).HasColumnName("reconciled_on");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'POSTED'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.TdsAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("tds_amount");
            entity.Property(e => e.TransactionRefNo)
                .HasMaxLength(100)
                .HasColumnName("transaction_ref_no");

            entity.HasOne(d => d.BankAccount).WithMany(p => p.TrnBankReceipts)
                .HasForeignKey(d => d.BankAccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_br_bank");

            entity.HasOne(d => d.Company).WithMany(p => p.TrnBankReceipts)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_br_company");
        });

        modelBuilder.Entity<TrnBankReceiptAllocation>(entity =>
        {
            entity.HasKey(e => e.AllocationId).HasName("trn_bank_receipt_allocation_pkey");

            entity.ToTable("trn_bank_receipt_allocation", "press_db", tb => tb.HasComment("Allocation of bank receipt against sales invoices or advance adjustments."));

            entity.HasIndex(e => e.BankReceiptId, "idx_bra_receipt");

            entity.Property(e => e.AllocationId).HasColumnName("allocation_id");
            entity.Property(e => e.AllocatedAmount)
                .HasPrecision(18, 2)
                .HasColumnName("allocated_amount");
            entity.Property(e => e.AllocationAgainst)
                .HasMaxLength(30)
                .HasComment("SALES_INVOICE, CREDIT_NOTE, ADVANCE, OTHER")
                .HasColumnName("allocation_against");
            entity.Property(e => e.BankReceiptId).HasColumnName("bank_receipt_id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.RefDate).HasColumnName("ref_date");
            entity.Property(e => e.RefId).HasColumnName("ref_id");
            entity.Property(e => e.RefNo)
                .HasMaxLength(50)
                .HasColumnName("ref_no");

            entity.HasOne(d => d.BankReceipt).WithMany(p => p.TrnBankReceiptAllocations)
                .HasForeignKey(d => d.BankReceiptId)
                .HasConstraintName("fk_bra_receipt");
        });

        modelBuilder.Entity<TrnBankReconciliation>(entity =>
        {
            entity.HasKey(e => e.ReconciliationId).HasName("trn_bank_reconciliation_pkey");

            entity.ToTable("trn_bank_reconciliation", "press_db", tb => tb.HasComment("Bank reconciliation header. Matches book entries with bank statement for a given bank account and statement date."));

            entity.HasIndex(e => e.BankAccountId, "idx_recon_bank");

            entity.HasIndex(e => e.StatementDate, "idx_recon_date");

            entity.HasIndex(e => e.ReconciliationNo, "uq_bank_reconciliation_no").IsUnique();

            entity.Property(e => e.ReconciliationId).HasColumnName("reconciliation_id");
            entity.Property(e => e.BankAccountId).HasColumnName("bank_account_id");
            entity.Property(e => e.BookBalance)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("book_balance");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.CompletedBy).HasColumnName("completed_by");
            entity.Property(e => e.CompletedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("completed_on");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.DifferenceAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("difference_amount");
            entity.Property(e => e.FinYearId).HasColumnName("fin_year_id");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.PendingItems)
                .HasDefaultValue(0)
                .HasColumnName("pending_items");
            entity.Property(e => e.ReconciledBalance)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("reconciled_balance");
            entity.Property(e => e.ReconciledItems)
                .HasDefaultValue(0)
                .HasColumnName("reconciled_items");
            entity.Property(e => e.ReconciliationNo)
                .HasMaxLength(50)
                .HasColumnName("reconciliation_no");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.StatementBalance)
                .HasPrecision(18, 2)
                .HasColumnName("statement_balance");
            entity.Property(e => e.StatementDate).HasColumnName("statement_date");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'IN_PROGRESS'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.TotalItems)
                .HasDefaultValue(0)
                .HasColumnName("total_items");

            entity.HasOne(d => d.BankAccount).WithMany(p => p.TrnBankReconciliations)
                .HasForeignKey(d => d.BankAccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_recon_bank");

            entity.HasOne(d => d.Company).WithMany(p => p.TrnBankReconciliations)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_recon_company");
        });

        modelBuilder.Entity<TrnBankReconciliationItem>(entity =>
        {
            entity.HasKey(e => e.ReconItemId).HasName("trn_bank_reconciliation_item_pkey");

            entity.ToTable("trn_bank_reconciliation_item", "press_db", tb => tb.HasComment("Bank reconciliation line items. Each row maps a book voucher entry to its bank statement clearance date."));

            entity.HasIndex(e => e.ReconciliationId, "idx_recon_item_header");

            entity.Property(e => e.ReconItemId).HasColumnName("recon_item_id");
            entity.Property(e => e.BankDate).HasColumnName("bank_date");
            entity.Property(e => e.ChequeNo)
                .HasMaxLength(30)
                .HasColumnName("cheque_no");
            entity.Property(e => e.CreditAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("credit_amount");
            entity.Property(e => e.DebitAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("debit_amount");
            entity.Property(e => e.IsReconciled)
                .HasDefaultValue(false)
                .HasColumnName("is_reconciled");
            entity.Property(e => e.ReconciledOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("reconciled_on");
            entity.Property(e => e.ReconciliationId).HasColumnName("reconciliation_id");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.VoucherDate).HasColumnName("voucher_date");
            entity.Property(e => e.VoucherId).HasColumnName("voucher_id");
            entity.Property(e => e.VoucherNo)
                .HasMaxLength(50)
                .HasColumnName("voucher_no");
            entity.Property(e => e.VoucherType)
                .HasMaxLength(50)
                .HasColumnName("voucher_type");

            entity.HasOne(d => d.Reconciliation).WithMany(p => p.TrnBankReconciliationItems)
                .HasForeignKey(d => d.ReconciliationId)
                .HasConstraintName("fk_recon_item_header");
        });

        modelBuilder.Entity<TrnChallan>(entity =>
        {
            entity.HasKey(e => e.ChallanId).HasName("trn_challan_pkey");

            entity.ToTable("trn_challan", "press_db");

            entity.HasIndex(e => e.CompanyId, "idx_challan_company");

            entity.HasIndex(e => e.ChallanDate, "idx_challan_date");

            entity.HasIndex(e => e.JobId, "idx_challan_job_id");

            entity.HasIndex(e => e.PartyId, "idx_challan_party");

            entity.HasIndex(e => e.Status, "idx_challan_status");

            entity.HasIndex(e => e.ChallanNo, "trn_challan_challan_no_key").IsUnique();

            entity.Property(e => e.ChallanId).HasColumnName("challan_id");
            entity.Property(e => e.ChallanDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("challan_date");
            entity.Property(e => e.ChallanNo)
                .HasMaxLength(30)
                .HasColumnName("challan_no");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.DeliveryAddress).HasColumnName("delivery_address");
            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(50)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.PartyId).HasColumnName("party_id");
            entity.Property(e => e.ReferenceNo)
                .HasMaxLength(50)
                .HasColumnName("reference_no");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'CREATED'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("total_amount");
            entity.Property(e => e.TotalQty)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("total_qty");
            entity.Property(e => e.TransportDetails)
                .HasMaxLength(200)
                .HasColumnName("transport_details");
            entity.Property(e => e.VehicleNo)
                .HasMaxLength(50)
                .HasColumnName("vehicle_no");

            entity.HasOne(d => d.Company).WithMany(p => p.TrnChallans)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_challan_company");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.TrnChallans)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_challan_created_by");

            entity.HasOne(d => d.Job).WithMany(p => p.TrnChallans)
                .HasForeignKey(d => d.JobId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_challan_job");

            entity.HasOne(d => d.Party).WithMany(p => p.TrnChallans)
                .HasForeignKey(d => d.PartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_challan_party");
        });

        modelBuilder.Entity<TrnChallanItem>(entity =>
        {
            entity.HasKey(e => e.ChallanItemId).HasName("trn_challan_item_pkey");

            entity.ToTable("trn_challan_item", "press_db");

            entity.HasIndex(e => e.ChallanId, "idx_challan_item_challan");

            entity.HasIndex(e => e.JobItemId, "idx_challan_item_job_item");

            entity.HasIndex(e => e.JobItemId, "idx_challan_item_job_item_id");

            entity.Property(e => e.ChallanItemId).HasColumnName("challan_item_id");
            entity.Property(e => e.Amount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("amount");
            entity.Property(e => e.ChallanId).HasColumnName("challan_id");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.DeliveredQuantity).HasColumnName("delivered_quantity");
            entity.Property(e => e.ItemSequence)
                .HasDefaultValue(1)
                .HasColumnName("item_sequence");
            entity.Property(e => e.JobItemId).HasColumnName("job_item_id");
            entity.Property(e => e.JobQuantity).HasColumnName("job_quantity");
            entity.Property(e => e.PendingQuantity).HasColumnName("pending_quantity");
            entity.Property(e => e.ProductDescription)
                .HasMaxLength(300)
                .HasColumnName("product_description");
            entity.Property(e => e.ProductName)
                .HasMaxLength(150)
                .HasColumnName("product_name");
            entity.Property(e => e.Rate)
                .HasPrecision(14, 4)
                .HasDefaultValueSql("0")
                .HasColumnName("rate");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.UomId).HasColumnName("uom_id");

            entity.HasOne(d => d.Challan).WithMany(p => p.TrnChallanItems)
                .HasForeignKey(d => d.ChallanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_challan_item_challan");

            entity.HasOne(d => d.JobItem).WithMany(p => p.TrnChallanItems)
                .HasForeignKey(d => d.JobItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_challan_item_job_item");
        });

        modelBuilder.Entity<TrnChallanTimeline>(entity =>
        {
            entity.HasKey(e => e.TimelineId).HasName("trn_challan_timeline_pkey");

            entity.ToTable("trn_challan_timeline", "press_db");

            entity.HasIndex(e => e.ChallanId, "idx_challan_timeline_challan_id");

            entity.HasIndex(e => e.CreatedOn, "idx_challan_timeline_created_on").IsDescending();

            entity.HasIndex(e => e.MovementType, "idx_challan_timeline_movement");

            entity.HasIndex(e => e.NewStatus, "idx_challan_timeline_status");

            entity.Property(e => e.TimelineId).HasColumnName("timeline_id");
            entity.Property(e => e.AssignedToUserId).HasColumnName("assigned_to_user_id");
            entity.Property(e => e.AttachmentUrl).HasColumnName("attachment_url");
            entity.Property(e => e.ChallanId).HasColumnName("challan_id");
            entity.Property(e => e.CommunicationMode)
                .HasMaxLength(50)
                .HasColumnName("communication_mode");
            entity.Property(e => e.CommunicationReference).HasColumnName("communication_reference");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.EnquiryId).HasColumnName("enquiry_id");
            entity.Property(e => e.EventCode)
                .HasMaxLength(50)
                .HasColumnName("event_code");
            entity.Property(e => e.EventDescription).HasColumnName("event_description");
            entity.Property(e => e.EventTitle)
                .HasMaxLength(200)
                .HasColumnName("event_title");
            entity.Property(e => e.EventType)
                .HasMaxLength(50)
                .HasColumnName("event_type");
            entity.Property(e => e.FromLocationId).HasColumnName("from_location_id");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.MachineId).HasColumnName("machine_id");
            entity.Property(e => e.MovementType)
                .HasMaxLength(30)
                .HasColumnName("movement_type");
            entity.Property(e => e.NewAmount)
                .HasPrecision(18, 2)
                .HasColumnName("new_amount");
            entity.Property(e => e.NewQuantity)
                .HasPrecision(18, 2)
                .HasColumnName("new_quantity");
            entity.Property(e => e.NewStatus)
                .HasMaxLength(50)
                .HasColumnName("new_status");
            entity.Property(e => e.OldAmount)
                .HasPrecision(18, 2)
                .HasColumnName("old_amount");
            entity.Property(e => e.OldQuantity)
                .HasPrecision(18, 2)
                .HasColumnName("old_quantity");
            entity.Property(e => e.OldStatus)
                .HasMaxLength(50)
                .HasColumnName("old_status");
            entity.Property(e => e.OperatorId).HasColumnName("operator_id");
            entity.Property(e => e.ProcessCode)
                .HasMaxLength(50)
                .HasColumnName("process_code");
            entity.Property(e => e.ProcessName)
                .HasMaxLength(100)
                .HasColumnName("process_name");
            entity.Property(e => e.QuotationId).HasColumnName("quotation_id");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.ToLocationId).HasColumnName("to_location_id");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
            entity.Property(e => e.UpdatedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_on");
            entity.Property(e => e.WorkCenterId).HasColumnName("work_center_id");

            entity.HasOne(d => d.Challan).WithMany(p => p.TrnChallanTimelines)
                .HasForeignKey(d => d.ChallanId)
                .HasConstraintName("fk_challan_timeline_challan");

            entity.HasOne(d => d.Enquiry).WithMany(p => p.TrnChallanTimelines)
                .HasForeignKey(d => d.EnquiryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_challan_timeline_enquiry");

            entity.HasOne(d => d.Job).WithMany(p => p.TrnChallanTimelines)
                .HasForeignKey(d => d.JobId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_challan_timeline_job");

            entity.HasOne(d => d.Quotation).WithMany(p => p.TrnChallanTimelines)
                .HasForeignKey(d => d.QuotationId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_challan_timeline_quotation");
        });

        modelBuilder.Entity<TrnContraVoucher>(entity =>
        {
            entity.HasKey(e => e.ContraId).HasName("trn_contra_voucher_pkey");

            entity.ToTable("trn_contra_voucher", "press_db", tb => tb.HasComment("Contra voucher for fund transfers: Cash→Bank, Bank→Cash, Bank→Bank. No party involved."));

            entity.HasIndex(e => e.CompanyId, "idx_contra_company");

            entity.HasIndex(e => e.ContraDate, "idx_contra_date");

            entity.HasIndex(e => e.ContraNo, "uq_contra_no").IsUnique();

            entity.Property(e => e.ContraId).HasColumnName("contra_id");
            entity.Property(e => e.Amount)
                .HasPrecision(18, 2)
                .HasColumnName("amount");
            entity.Property(e => e.CancelReason).HasColumnName("cancel_reason");
            entity.Property(e => e.CancelledBy).HasColumnName("cancelled_by");
            entity.Property(e => e.CancelledOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("cancelled_on");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.ContraDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("contra_date");
            entity.Property(e => e.ContraNo)
                .HasMaxLength(50)
                .HasColumnName("contra_no");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.FinYearId).HasColumnName("fin_year_id");
            entity.Property(e => e.GlPostedBy).HasColumnName("gl_posted_by");
            entity.Property(e => e.GlPostedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("gl_posted_on");
            entity.Property(e => e.IsCancelled)
                .HasDefaultValue(false)
                .HasColumnName("is_cancelled");
            entity.Property(e => e.IsPostedToGl)
                .HasDefaultValue(false)
                .HasColumnName("is_posted_to_gl");
            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.Narration).HasColumnName("narration");
            entity.Property(e => e.ReferenceNo)
                .HasMaxLength(100)
                .HasColumnName("reference_no");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'POSTED'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.TransferFromId)
                .HasComment("FK to mst_bank_account.bank_account_id (for BANK) or 0 (for CASH)")
                .HasColumnName("transfer_from_id");
            entity.Property(e => e.TransferFromType)
                .HasMaxLength(10)
                .HasComment("CASH or BANK")
                .HasColumnName("transfer_from_type");
            entity.Property(e => e.TransferToId)
                .HasComment("FK to mst_bank_account.bank_account_id (for BANK) or 0 (for CASH)")
                .HasColumnName("transfer_to_id");
            entity.Property(e => e.TransferToType)
                .HasMaxLength(10)
                .HasComment("CASH or BANK")
                .HasColumnName("transfer_to_type");

            entity.HasOne(d => d.Company).WithMany(p => p.TrnContraVouchers)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_contra_company");
        });

        modelBuilder.Entity<TrnCreditNote>(entity =>
        {
            entity.HasKey(e => e.CreditNoteId).HasName("trn_credit_note_pkey");

            entity.ToTable("trn_credit_note", "press_db", tb => tb.HasComment("Credit note issued to customer for sales returns, rate difference, or post-sale adjustments. Reduces Accounts Receivable. Supports GST and e-Invoice."));

            entity.HasIndex(e => e.CreditNoteDate, "idx_cn_date");

            entity.HasIndex(e => e.OriginalInvoiceId, "idx_cn_original_inv");

            entity.HasIndex(e => e.PartyId, "idx_cn_party");

            entity.HasIndex(e => e.Status, "idx_cn_status");

            entity.HasIndex(e => e.CreditNoteNo, "uq_credit_note_no").IsUnique();

            entity.Property(e => e.CreditNoteId).HasColumnName("credit_note_id");
            entity.Property(e => e.AdjustedAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("adjusted_amount");
            entity.Property(e => e.AttachmentsJson)
                .HasDefaultValueSql("'[]'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("attachments_json");
            entity.Property(e => e.BillingAddressId).HasColumnName("billing_address_id");
            entity.Property(e => e.CancelReason).HasColumnName("cancel_reason");
            entity.Property(e => e.CancelledBy).HasColumnName("cancelled_by");
            entity.Property(e => e.CancelledOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("cancelled_on");
            entity.Property(e => e.CessAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("cess_amount");
            entity.Property(e => e.CgstAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("cgst_amount");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.CreditNoteDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("credit_note_date");
            entity.Property(e => e.CreditNoteNo)
                .HasMaxLength(50)
                .HasColumnName("credit_note_no");
            entity.Property(e => e.CreditNoteType)
                .HasMaxLength(30)
                .HasDefaultValueSql("'SALES_RETURN'::character varying")
                .HasComment("SALES_RETURN, RATE_DIFFERENCE, QUALITY_ISSUE, DISCOUNT_AFTER_SALE, OTHER")
                .HasColumnName("credit_note_type");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.DiscountAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("discount_amount");
            entity.Property(e => e.EInvoiceAckDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("e_invoice_ack_date");
            entity.Property(e => e.EInvoiceAckNo)
                .HasMaxLength(50)
                .HasColumnName("e_invoice_ack_no");
            entity.Property(e => e.EInvoiceIrn)
                .HasMaxLength(100)
                .HasColumnName("e_invoice_irn");
            entity.Property(e => e.ExchangeRate)
                .HasPrecision(10, 4)
                .HasDefaultValueSql("1")
                .HasColumnName("exchange_rate");
            entity.Property(e => e.FinYearId).HasColumnName("fin_year_id");
            entity.Property(e => e.GlPostedBy).HasColumnName("gl_posted_by");
            entity.Property(e => e.GlPostedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("gl_posted_on");
            entity.Property(e => e.GrandTotal)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("grand_total");
            entity.Property(e => e.IgstAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("igst_amount");
            entity.Property(e => e.InternalNotes).HasColumnName("internal_notes");
            entity.Property(e => e.IsCancelled)
                .HasDefaultValue(false)
                .HasColumnName("is_cancelled");
            entity.Property(e => e.IsPostedToGl)
                .HasDefaultValue(false)
                .HasColumnName("is_posted_to_gl");
            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.OriginalInvoiceDate).HasColumnName("original_invoice_date");
            entity.Property(e => e.OriginalInvoiceId).HasColumnName("original_invoice_id");
            entity.Property(e => e.OriginalInvoiceNo)
                .HasMaxLength(50)
                .HasColumnName("original_invoice_no");
            entity.Property(e => e.PartyId).HasColumnName("party_id");
            entity.Property(e => e.PlaceOfSupply)
                .HasMaxLength(150)
                .HasColumnName("place_of_supply");
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.RoundOff)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("round_off");
            entity.Property(e => e.SgstAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("sgst_amount");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'DRAFT'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.SubtotalAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("subtotal_amount");
            entity.Property(e => e.TaxableAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("taxable_amount");
            entity.Property(e => e.TotalTaxAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("total_tax_amount");
            entity.Property(e => e.UnadjustedAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("unadjusted_amount");

            entity.HasOne(d => d.Company).WithMany(p => p.TrnCreditNotes)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_cn_company");

            entity.HasOne(d => d.OriginalInvoice).WithMany(p => p.TrnCreditNotes)
                .HasForeignKey(d => d.OriginalInvoiceId)
                .HasConstraintName("fk_cn_original_invoice");

            entity.HasOne(d => d.Party).WithMany(p => p.TrnCreditNotes)
                .HasForeignKey(d => d.PartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_cn_party");
        });

        modelBuilder.Entity<TrnCreditNoteItem>(entity =>
        {
            entity.HasKey(e => e.CreditNoteItemId).HasName("trn_credit_note_item_pkey");

            entity.ToTable("trn_credit_note_item", "press_db", tb => tb.HasComment("Credit note line items with full GST breakup per item."));

            entity.HasIndex(e => e.CreditNoteId, "idx_cn_item_header");

            entity.Property(e => e.CreditNoteItemId).HasColumnName("credit_note_item_id");
            entity.Property(e => e.AccountHeadId).HasColumnName("account_head_id");
            entity.Property(e => e.CessAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("cess_amount");
            entity.Property(e => e.CessPercent)
                .HasPrecision(6, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("cess_percent");
            entity.Property(e => e.CgstAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("cgst_amount");
            entity.Property(e => e.CgstPercent)
                .HasPrecision(6, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("cgst_percent");
            entity.Property(e => e.CreditNoteId).HasColumnName("credit_note_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DiscountAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("discount_amount");
            entity.Property(e => e.DiscountPercent)
                .HasPrecision(6, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("discount_percent");
            entity.Property(e => e.HsnSacCode)
                .HasMaxLength(20)
                .HasColumnName("hsn_sac_code");
            entity.Property(e => e.IgstAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("igst_amount");
            entity.Property(e => e.IgstPercent)
                .HasPrecision(6, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("igst_percent");
            entity.Property(e => e.ItemId).HasColumnName("item_id");
            entity.Property(e => e.ItemSequence)
                .HasDefaultValue(1)
                .HasColumnName("item_sequence");
            entity.Property(e => e.LineTotal)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("line_total");
            entity.Property(e => e.OriginalInvoiceItemId).HasColumnName("original_invoice_item_id");
            entity.Property(e => e.Quantity)
                .HasPrecision(14, 4)
                .HasDefaultValueSql("0")
                .HasColumnName("quantity");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.SgstAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("sgst_amount");
            entity.Property(e => e.SgstPercent)
                .HasPrecision(6, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("sgst_percent");
            entity.Property(e => e.TaxCategoryId).HasColumnName("tax_category_id");
            entity.Property(e => e.TaxableValue)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("taxable_value");
            entity.Property(e => e.TotalTaxAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("total_tax_amount");
            entity.Property(e => e.UnitRate)
                .HasPrecision(14, 4)
                .HasDefaultValueSql("0")
                .HasColumnName("unit_rate");
            entity.Property(e => e.UomId).HasColumnName("uom_id");

            entity.HasOne(d => d.CreditNote).WithMany(p => p.TrnCreditNoteItems)
                .HasForeignKey(d => d.CreditNoteId)
                .HasConstraintName("fk_cn_item_header");
        });

        modelBuilder.Entity<TrnDebitNote>(entity =>
        {
            entity.HasKey(e => e.DebitNoteId).HasName("trn_debit_note_pkey");

            entity.ToTable("trn_debit_note", "press_db", tb => tb.HasComment("Debit note issued to supplier for purchase returns, rate difference, or quality issues. Reduces Accounts Payable. Supports GST."));

            entity.HasIndex(e => e.DebitNoteDate, "idx_dn_date");

            entity.HasIndex(e => e.OriginalInvoiceId, "idx_dn_original_inv");

            entity.HasIndex(e => e.PartyId, "idx_dn_party");

            entity.HasIndex(e => e.Status, "idx_dn_status");

            entity.HasIndex(e => e.DebitNoteNo, "uq_debit_note_no").IsUnique();

            entity.Property(e => e.DebitNoteId).HasColumnName("debit_note_id");
            entity.Property(e => e.AdjustedAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("adjusted_amount");
            entity.Property(e => e.AttachmentsJson)
                .HasDefaultValueSql("'[]'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("attachments_json");
            entity.Property(e => e.BillingAddressId).HasColumnName("billing_address_id");
            entity.Property(e => e.CancelReason).HasColumnName("cancel_reason");
            entity.Property(e => e.CancelledBy).HasColumnName("cancelled_by");
            entity.Property(e => e.CancelledOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("cancelled_on");
            entity.Property(e => e.CessAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("cess_amount");
            entity.Property(e => e.CgstAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("cgst_amount");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.DebitNoteDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("debit_note_date");
            entity.Property(e => e.DebitNoteNo)
                .HasMaxLength(50)
                .HasColumnName("debit_note_no");
            entity.Property(e => e.DebitNoteType)
                .HasMaxLength(30)
                .HasDefaultValueSql("'PURCHASE_RETURN'::character varying")
                .HasComment("PURCHASE_RETURN, RATE_DIFFERENCE, QUALITY_ISSUE, SHORT_SUPPLY, OTHER")
                .HasColumnName("debit_note_type");
            entity.Property(e => e.DiscountAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("discount_amount");
            entity.Property(e => e.ExchangeRate)
                .HasPrecision(10, 4)
                .HasDefaultValueSql("1")
                .HasColumnName("exchange_rate");
            entity.Property(e => e.FinYearId).HasColumnName("fin_year_id");
            entity.Property(e => e.GlPostedBy).HasColumnName("gl_posted_by");
            entity.Property(e => e.GlPostedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("gl_posted_on");
            entity.Property(e => e.GrandTotal)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("grand_total");
            entity.Property(e => e.IgstAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("igst_amount");
            entity.Property(e => e.InternalNotes).HasColumnName("internal_notes");
            entity.Property(e => e.IsCancelled)
                .HasDefaultValue(false)
                .HasColumnName("is_cancelled");
            entity.Property(e => e.IsPostedToGl)
                .HasDefaultValue(false)
                .HasColumnName("is_posted_to_gl");
            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.OriginalInvoiceDate).HasColumnName("original_invoice_date");
            entity.Property(e => e.OriginalInvoiceId).HasColumnName("original_invoice_id");
            entity.Property(e => e.OriginalInvoiceNo)
                .HasMaxLength(50)
                .HasColumnName("original_invoice_no");
            entity.Property(e => e.PartyId).HasColumnName("party_id");
            entity.Property(e => e.PlaceOfSupply)
                .HasMaxLength(150)
                .HasColumnName("place_of_supply");
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.RoundOff)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("round_off");
            entity.Property(e => e.SgstAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("sgst_amount");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'DRAFT'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.SubtotalAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("subtotal_amount");
            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            entity.Property(e => e.TaxableAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("taxable_amount");
            entity.Property(e => e.TotalTaxAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("total_tax_amount");
            entity.Property(e => e.UnadjustedAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("unadjusted_amount");

            entity.HasOne(d => d.Company).WithMany(p => p.TrnDebitNotes)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_dn_company");

            entity.HasOne(d => d.OriginalInvoice).WithMany(p => p.TrnDebitNotes)
                .HasForeignKey(d => d.OriginalInvoiceId)
                .HasConstraintName("fk_dn_original_invoice");

            entity.HasOne(d => d.Party).WithMany(p => p.TrnDebitNotes)
                .HasForeignKey(d => d.PartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_dn_party");
        });

        modelBuilder.Entity<TrnDebitNoteItem>(entity =>
        {
            entity.HasKey(e => e.DebitNoteItemId).HasName("trn_debit_note_item_pkey");

            entity.ToTable("trn_debit_note_item", "press_db", tb => tb.HasComment("Debit note line items with full GST breakup per item."));

            entity.HasIndex(e => e.DebitNoteId, "idx_dn_item_header");

            entity.Property(e => e.DebitNoteItemId).HasColumnName("debit_note_item_id");
            entity.Property(e => e.AccountHeadId).HasColumnName("account_head_id");
            entity.Property(e => e.CessAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("cess_amount");
            entity.Property(e => e.CessPercent)
                .HasPrecision(6, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("cess_percent");
            entity.Property(e => e.CgstAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("cgst_amount");
            entity.Property(e => e.CgstPercent)
                .HasPrecision(6, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("cgst_percent");
            entity.Property(e => e.DebitNoteId).HasColumnName("debit_note_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DiscountAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("discount_amount");
            entity.Property(e => e.DiscountPercent)
                .HasPrecision(6, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("discount_percent");
            entity.Property(e => e.HsnSacCode)
                .HasMaxLength(20)
                .HasColumnName("hsn_sac_code");
            entity.Property(e => e.IgstAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("igst_amount");
            entity.Property(e => e.IgstPercent)
                .HasPrecision(6, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("igst_percent");
            entity.Property(e => e.ItemId).HasColumnName("item_id");
            entity.Property(e => e.ItemSequence)
                .HasDefaultValue(1)
                .HasColumnName("item_sequence");
            entity.Property(e => e.LineTotal)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("line_total");
            entity.Property(e => e.OriginalInvoiceItemId).HasColumnName("original_invoice_item_id");
            entity.Property(e => e.Quantity)
                .HasPrecision(14, 4)
                .HasDefaultValueSql("0")
                .HasColumnName("quantity");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.SgstAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("sgst_amount");
            entity.Property(e => e.SgstPercent)
                .HasPrecision(6, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("sgst_percent");
            entity.Property(e => e.TaxCategoryId).HasColumnName("tax_category_id");
            entity.Property(e => e.TaxableValue)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("taxable_value");
            entity.Property(e => e.TotalTaxAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("total_tax_amount");
            entity.Property(e => e.UnitRate)
                .HasPrecision(14, 4)
                .HasDefaultValueSql("0")
                .HasColumnName("unit_rate");
            entity.Property(e => e.UomId).HasColumnName("uom_id");

            entity.HasOne(d => d.DebitNote).WithMany(p => p.TrnDebitNoteItems)
                .HasForeignKey(d => d.DebitNoteId)
                .HasConstraintName("fk_dn_item_header");
        });

        modelBuilder.Entity<TrnDesignWorkEntry>(entity =>
        {
            entity.HasKey(e => e.DesignWorkId).HasName("pk_trn_design_work_entry");

            entity.ToTable("trn_design_work_entry", "press_db", tb => tb.HasComment("Per-activity Design/DTP progress captured in Workspace > DesignWork page. One row per activity per workspace task."));

            entity.HasIndex(e => e.JobId, "idx_design_work_job_id");

            entity.HasIndex(e => e.WorkspaceTaskId, "idx_design_work_task_id");

            entity.Property(e => e.DesignWorkId)
                .HasComment("Surrogate primary key — auto-incremented.")
                .HasColumnName("design_work_id");
            entity.Property(e => e.ActivityName)
                .HasMaxLength(300)
                .HasComment("Design/DTP activity label e.g. Cover Design, Text DTP.")
                .HasColumnName("activity_name");
            entity.Property(e => e.ActivitySequence)
                .HasDefaultValue(1)
                .HasComment("Display/processing order of this activity within the task.")
                .HasColumnName("activity_sequence");
            entity.Property(e => e.CompletedOn)
                .HasComment("Timestamp when this activity was marked completed.")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("completed_on");
            entity.Property(e => e.CreatedBy)
                .HasComment("User ID of the person who created this record.")
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("now()")
                .HasComment("Record creation timestamp.")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.IsCompleted)
                .HasDefaultValue(false)
                .HasComment("True when the row-level Complete button is clicked.")
                .HasColumnName("is_completed");
            entity.Property(e => e.JobId)
                .HasComment("Denormalised FK to trn_job for fast reporting.")
                .HasColumnName("job_id");
            entity.Property(e => e.ModifiedBy)
                .HasComment("User ID of the last person who modified this record.")
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasComment("Last modification timestamp.")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.Notes)
                .HasComment("Free-text work notes entered in the right sidebar.")
                .HasColumnName("notes");
            entity.Property(e => e.PagesCompleted)
                .HasDefaultValue(0)
                .HasComment("Pages finished so far — updated on Save Progress.")
                .HasColumnName("pages_completed");
            entity.Property(e => e.PagesPending)
                .HasComputedColumnSql("GREATEST(0, (pages_required - pages_completed))", true)
                .HasComment("Computed column: MAX(0, pages_required - pages_completed).")
                .HasColumnName("pages_pending");
            entity.Property(e => e.PagesRequired)
                .HasDefaultValue(0)
                .HasComment("Total pages to be designed/DTPed for this activity.")
                .HasColumnName("pages_required");
            entity.Property(e => e.WorkspaceTaskId)
                .HasComment("FK to trn_workspace_task. Identifies the parent task.")
                .HasColumnName("workspace_task_id");

            entity.HasOne(d => d.WorkspaceTask).WithMany(p => p.TrnDesignWorkEntries)
                .HasForeignKey(d => d.WorkspaceTaskId)
                .HasConstraintName("fk_design_work_task");
        });

        modelBuilder.Entity<TrnPlateMakingEntry>(entity =>
        {
            entity.HasKey(e => e.PlateMakingId).HasName("pk_trn_plate_making_entry");

            entity.ToTable("trn_plate_making_entry", "press_db", tb => tb.HasComment("Per-activity Plate Making progress captured in Workspace > PlateMaking page."));

            entity.HasIndex(e => e.JobId, "idx_plate_making_job_id");
            entity.HasIndex(e => e.WorkspaceTaskId, "idx_plate_making_task_id");

            entity.Property(e => e.PlateMakingId)
                .HasComment("Surrogate primary key — auto-incremented.")
                .HasColumnName("plate_making_id");
            entity.Property(e => e.WorkspaceTaskId)
                .HasComment("FK to trn_workspace_task.")
                .HasColumnName("workspace_task_id");
            entity.Property(e => e.JobId)
                .HasComment("Denormalised FK to trn_job for fast reporting.")
                .HasColumnName("job_id");
            entity.Property(e => e.ActivityName)
                .HasMaxLength(300)
                .HasComment("Plate making activity label e.g. Cover Plates, Text Plates.")
                .HasColumnName("activity_name");
            entity.Property(e => e.ActivitySequence)
                .HasDefaultValue(1)
                .HasComment("Display/processing order within the task.")
                .HasColumnName("activity_sequence");
            entity.Property(e => e.PartName)
                .HasMaxLength(200)
                .HasComment("Product part name from job config.")
                .HasColumnName("part_name");
            entity.Property(e => e.PlateType)
                .HasMaxLength(100)
                .HasComment("Plate technology: CTP, Conventional, Violet, Thermal, etc.")
                .HasColumnName("plate_type");
            entity.Property(e => e.NumberOfColors)
                .HasDefaultValue(0)
                .HasComment("Number of ink colors for this activity.")
                .HasColumnName("number_of_colors");
            entity.Property(e => e.NumberOfPlates)
                .HasDefaultValue(0)
                .HasComment("Total plates to be made.")
                .HasColumnName("number_of_plates");
            entity.Property(e => e.PlatesMade)
                .HasDefaultValue(0)
                .HasComment("Plates finished so far.")
                .HasColumnName("plates_made");
            entity.Property(e => e.PlatesPending)
                .HasComputedColumnSql("GREATEST(0, (number_of_plates - plates_made))", true)
                .HasComment("Computed: GREATEST(0, number_of_plates - plates_made).")
                .HasColumnName("plates_pending");
            entity.Property(e => e.IsCompleted)
                .HasDefaultValue(false)
                .HasComment("True when row-level Complete button clicked.")
                .HasColumnName("is_completed");
            entity.Property(e => e.CompletedOn)
                .HasComment("Timestamp when activity was marked completed.")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("completed_on");
            entity.Property(e => e.Notes)
                .HasComment("Free-text work notes.")
                .HasColumnName("notes");
            entity.Property(e => e.CreatedBy)
                .HasComment("User ID who created the record.")
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("now()")
                .HasComment("Record creation timestamp.")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.ModifiedBy)
                .HasComment("User ID of last modifier.")
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasComment("Last modification timestamp.")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");

            entity.HasOne(d => d.WorkspaceTask).WithMany(p => p.TrnPlateMakingEntries)
                .HasForeignKey(d => d.WorkspaceTaskId)
                .HasConstraintName("fk_plate_making_task");
        });

        modelBuilder.Entity<TrnEnquiry>(entity =>
        {
            entity.HasKey(e => e.EnquiryId).HasName("trn_enquiry_pkey");

            entity.ToTable("trn_enquiry", "press_db");

            entity.HasIndex(e => e.CompanyId, "idx_enquiry_company");

            entity.HasIndex(e => e.EnquiryDate, "idx_enquiry_date");

            entity.HasIndex(e => e.PartyId, "idx_enquiry_party");

            entity.HasIndex(e => e.Priority, "idx_enquiry_priority");

            entity.HasIndex(e => e.Status, "idx_enquiry_status");

            entity.HasIndex(e => e.EnquiryNo, "trn_enquiry_enquiry_no_key").IsUnique();

            entity.Property(e => e.EnquiryId).HasColumnName("enquiry_id");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.ContactEmail)
                .HasMaxLength(100)
                .HasColumnName("contact_email");
            entity.Property(e => e.ContactMobile)
                .HasMaxLength(20)
                .HasColumnName("contact_mobile");
            entity.Property(e => e.ContactPerson)
                .HasMaxLength(100)
                .HasColumnName("contact_person");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.EnquiryDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("enquiry_date");
            entity.Property(e => e.EnquiryNo)
                .HasMaxLength(30)
                .HasColumnName("enquiry_no");
            entity.Property(e => e.EnquirySource)
                .HasMaxLength(50)
                .HasColumnName("enquiry_source");
            entity.Property(e => e.ExpectedDeliveryDate).HasColumnName("expected_delivery_date");
            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(50)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.PartyId).HasColumnName("party_id");
            entity.Property(e => e.Priority)
                .HasMaxLength(20)
                .HasDefaultValueSql("'NORMAL'::character varying")
                .HasColumnName("priority");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'OPEN'::character varying")
                .HasColumnName("status");

            entity.HasOne(d => d.Company).WithMany(p => p.TrnEnquiries)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_enquiry_company");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.TrnEnquiries)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_enquiry_created_by");

            entity.HasOne(d => d.Party).WithMany(p => p.TrnEnquiries)
                .HasForeignKey(d => d.PartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_enquiry_party");
        });

        modelBuilder.Entity<TrnEnquiryItem>(entity =>
        {
            entity.HasKey(e => e.EnquiryItemId).HasName("trn_enquiry_item_pkey");

            entity.ToTable("trn_enquiry_item", "press_db");

            entity.HasIndex(e => e.EnquiryId, "idx_enq_item_enquiry");

            entity.HasIndex(e => e.RateCalculatorId, "idx_enq_item_rate_calc");

            entity.HasIndex(e => e.Status, "idx_enq_item_status");

            entity.Property(e => e.EnquiryItemId).HasColumnName("enquiry_item_id");
            entity.Property(e => e.CalcRefNo)
                .HasMaxLength(50)
                .HasColumnName("calc_ref_no");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.EnquiryId).HasColumnName("enquiry_id");
            entity.Property(e => e.ItemSequence)
                .HasDefaultValue(1)
                .HasColumnName("item_sequence");
            entity.Property(e => e.JobTypeName)
                .HasMaxLength(100)
                .HasColumnName("job_type_name");
            entity.Property(e => e.NoOfPages).HasColumnName("no_of_pages");
            entity.Property(e => e.PrintingMethod)
                .HasMaxLength(30)
                .HasColumnName("printing_method");
            entity.Property(e => e.ProductDescription).HasColumnName("product_description");
            entity.Property(e => e.ProductName)
                .HasMaxLength(150)
                .HasColumnName("product_name");
            entity.Property(e => e.ProductSizeName)
                .HasMaxLength(100)
                .HasColumnName("product_size_name");
            entity.Property(e => e.ProductTypeName)
                .HasMaxLength(100)
                .HasColumnName("product_type_name");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.RateCalculatorId).HasColumnName("rate_calculator_id");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.SpecificationsJson)
                .HasColumnType("jsonb")
                .HasColumnName("specifications_json");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'ACTIVE'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.TrimHeightMm)
                .HasPrecision(8, 2)
                .HasColumnName("trim_height_mm");
            entity.Property(e => e.TrimWidthMm)
                .HasPrecision(8, 2)
                .HasColumnName("trim_width_mm");
            entity.Property(e => e.UomId).HasColumnName("uom_id");

            entity.HasOne(d => d.Enquiry).WithMany(p => p.TrnEnquiryItems)
                .HasForeignKey(d => d.EnquiryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_enquiry_item_hdr");

            entity.HasOne(d => d.RateCalculator).WithMany(p => p.TrnEnquiryItems)
                .HasForeignKey(d => d.RateCalculatorId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_enquiry_item_rate_calc");
        });

        modelBuilder.Entity<TrnEnquiryTimeline>(entity =>
        {
            entity.HasKey(e => e.TimelineId).HasName("trn_enquiry_timeline_pkey");

            entity.ToTable("trn_enquiry_timeline", "press_db");

            entity.HasIndex(e => e.CreatedOn, "idx_enquiry_timeline_created_on").IsDescending();

            entity.HasIndex(e => e.EnquiryId, "idx_enquiry_timeline_enquiry_id");

            entity.HasIndex(e => e.FollowupDate, "idx_enquiry_timeline_followup");

            entity.Property(e => e.TimelineId).HasColumnName("timeline_id");
            entity.Property(e => e.AssignedToUserId).HasColumnName("assigned_to_user_id");
            entity.Property(e => e.AttachmentUrl).HasColumnName("attachment_url");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.EnquiryId).HasColumnName("enquiry_id");
            entity.Property(e => e.EventCode)
                .HasMaxLength(50)
                .HasColumnName("event_code");
            entity.Property(e => e.EventDescription).HasColumnName("event_description");
            entity.Property(e => e.EventTitle)
                .HasMaxLength(200)
                .HasColumnName("event_title");
            entity.Property(e => e.EventType)
                .HasMaxLength(50)
                .HasColumnName("event_type");
            entity.Property(e => e.FollowupDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("followup_date");
            entity.Property(e => e.FollowupMode)
                .HasMaxLength(50)
                .HasColumnName("followup_mode");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.NewStatus)
                .HasMaxLength(50)
                .HasColumnName("new_status");
            entity.Property(e => e.OldStatus)
                .HasMaxLength(50)
                .HasColumnName("old_status");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
            entity.Property(e => e.UpdatedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_on");

            entity.HasOne(d => d.Enquiry).WithMany(p => p.TrnEnquiryTimelines)
                .HasForeignKey(d => d.EnquiryId)
                .HasConstraintName("fk_enquiry_timeline_enquiry");
        });

        modelBuilder.Entity<TrnExpenseVoucher>(entity =>
        {
            entity.HasKey(e => e.ExpenseVoucherId).HasName("trn_expense_voucher_pkey");

            entity.ToTable("trn_expense_voucher", "press_db", tb => tb.HasComment("Expense voucher for direct business expenses (rent, utilities, travel, repairs, etc.). Supports multi-line with GST and TDS. Approval workflow."));

            entity.HasIndex(e => e.CompanyId, "idx_ev_company");

            entity.HasIndex(e => e.VoucherDate, "idx_ev_date");

            entity.HasIndex(e => e.EmployeeId, "idx_ev_employee");

            entity.HasIndex(e => e.Status, "idx_ev_status");

            entity.HasIndex(e => e.VoucherNo, "uq_expense_voucher_no").IsUnique();

            entity.Property(e => e.ExpenseVoucherId).HasColumnName("expense_voucher_id");
            entity.Property(e => e.ApprovedBy).HasColumnName("approved_by");
            entity.Property(e => e.ApprovedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("approved_on");
            entity.Property(e => e.AttachmentsJson)
                .HasDefaultValueSql("'[]'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("attachments_json");
            entity.Property(e => e.BankAccountId).HasColumnName("bank_account_id");
            entity.Property(e => e.CancelReason).HasColumnName("cancel_reason");
            entity.Property(e => e.CancelledBy).HasColumnName("cancelled_by");
            entity.Property(e => e.CancelledOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("cancelled_on");
            entity.Property(e => e.CessAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("cess_amount");
            entity.Property(e => e.CgstAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("cgst_amount");
            entity.Property(e => e.ChequeDate).HasColumnName("cheque_date");
            entity.Property(e => e.ChequeNo)
                .HasMaxLength(30)
                .HasColumnName("cheque_no");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.ExpenseCategory)
                .HasMaxLength(50)
                .HasComment("OFFICE, TRAVEL, UTILITIES, REPAIRS, SALARY, RENT, PRINTING, TRANSPORT, MISC")
                .HasColumnName("expense_category");
            entity.Property(e => e.FinYearId).HasColumnName("fin_year_id");
            entity.Property(e => e.GlPostedBy).HasColumnName("gl_posted_by");
            entity.Property(e => e.GlPostedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("gl_posted_on");
            entity.Property(e => e.GrandTotal)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("grand_total");
            entity.Property(e => e.IgstAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("igst_amount");
            entity.Property(e => e.IsApproved)
                .HasDefaultValue(false)
                .HasColumnName("is_approved");
            entity.Property(e => e.IsCancelled)
                .HasDefaultValue(false)
                .HasColumnName("is_cancelled");
            entity.Property(e => e.IsPostedToGl)
                .HasDefaultValue(false)
                .HasColumnName("is_posted_to_gl");
            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.Narration).HasColumnName("narration");
            entity.Property(e => e.PartyId).HasColumnName("party_id");
            entity.Property(e => e.PaymentMode)
                .HasMaxLength(30)
                .HasDefaultValueSql("'CASH'::character varying")
                .HasColumnName("payment_mode");
            entity.Property(e => e.ReferenceDate).HasColumnName("reference_date");
            entity.Property(e => e.ReferenceNo)
                .HasMaxLength(100)
                .HasColumnName("reference_no");
            entity.Property(e => e.SgstAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("sgst_amount");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'DRAFT'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.SubtotalAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("subtotal_amount");
            entity.Property(e => e.TaxableAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("taxable_amount");
            entity.Property(e => e.TdsAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("tds_amount");
            entity.Property(e => e.TotalTaxAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("total_tax_amount");
            entity.Property(e => e.VoucherDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("voucher_date");
            entity.Property(e => e.VoucherNo)
                .HasMaxLength(50)
                .HasColumnName("voucher_no");

            entity.HasOne(d => d.BankAccount).WithMany(p => p.TrnExpenseVouchers)
                .HasForeignKey(d => d.BankAccountId)
                .HasConstraintName("fk_ev_bank");

            entity.HasOne(d => d.Company).WithMany(p => p.TrnExpenseVouchers)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ev_company");

            entity.HasOne(d => d.Employee).WithMany(p => p.TrnExpenseVouchers)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("fk_ev_employee");
        });

        modelBuilder.Entity<TrnExpenseVoucherItem>(entity =>
        {
            entity.HasKey(e => e.ExpenseItemId).HasName("trn_expense_voucher_item_pkey");

            entity.ToTable("trn_expense_voucher_item", "press_db", tb => tb.HasComment("Expense voucher line items. Each line debits a different expense account head with optional GST breakup."));

            entity.HasIndex(e => e.ExpenseVoucherId, "idx_ev_item_header");

            entity.Property(e => e.ExpenseItemId).HasColumnName("expense_item_id");
            entity.Property(e => e.AccountHeadId).HasColumnName("account_head_id");
            entity.Property(e => e.Amount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("amount");
            entity.Property(e => e.CessAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("cess_amount");
            entity.Property(e => e.CessPercent)
                .HasPrecision(6, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("cess_percent");
            entity.Property(e => e.CgstAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("cgst_amount");
            entity.Property(e => e.CgstPercent)
                .HasPrecision(6, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("cgst_percent");
            entity.Property(e => e.CostCenterId).HasColumnName("cost_center_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ExpenseVoucherId).HasColumnName("expense_voucher_id");
            entity.Property(e => e.HsnSacCode)
                .HasMaxLength(20)
                .HasColumnName("hsn_sac_code");
            entity.Property(e => e.IgstAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("igst_amount");
            entity.Property(e => e.IgstPercent)
                .HasPrecision(6, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("igst_percent");
            entity.Property(e => e.ItemSequence)
                .HasDefaultValue(1)
                .HasColumnName("item_sequence");
            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.LineTotal)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("line_total");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.SgstAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("sgst_amount");
            entity.Property(e => e.SgstPercent)
                .HasPrecision(6, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("sgst_percent");
            entity.Property(e => e.TaxCategoryId).HasColumnName("tax_category_id");
            entity.Property(e => e.TotalTaxAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("total_tax_amount");

            entity.HasOne(d => d.AccountHead).WithMany(p => p.TrnExpenseVoucherItems)
                .HasForeignKey(d => d.AccountHeadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ev_item_account");

            entity.HasOne(d => d.ExpenseVoucher).WithMany(p => p.TrnExpenseVoucherItems)
                .HasForeignKey(d => d.ExpenseVoucherId)
                .HasConstraintName("fk_ev_item_header");
        });

        modelBuilder.Entity<TrnGatePass>(entity =>
        {
            entity.HasKey(e => e.GatePassId).HasName("trn_gate_pass_pkey");

            entity.ToTable("trn_gate_pass", "press_db");

            entity.HasIndex(e => e.CompanyId, "idx_gatepass_company");

            entity.HasIndex(e => e.GatePassDate, "idx_gatepass_date");

            entity.HasIndex(e => e.LocationId, "idx_gatepass_location");

            entity.HasIndex(e => e.Status, "idx_gatepass_status");

            entity.HasIndex(e => e.GatepassType, "idx_gatepass_type");

            entity.HasIndex(e => e.GatePassNo, "trn_gate_pass_gate_pass_no_key").IsUnique();

            entity.Property(e => e.GatePassId).HasColumnName("gate_pass_id");
            entity.Property(e => e.ApprovedBy).HasColumnName("approved_by");
            entity.Property(e => e.ApprovedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("approved_on");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.DriverContact)
                .HasMaxLength(20)
                .HasColumnName("driver_contact");
            entity.Property(e => e.DriverName)
                .HasMaxLength(100)
                .HasColumnName("driver_name");
            entity.Property(e => e.GatePassDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("gate_pass_date");
            entity.Property(e => e.GatePassNo)
                .HasMaxLength(30)
                .HasColumnName("gate_pass_no");
            entity.Property(e => e.GatepassType)
                .HasMaxLength(10)
                .HasColumnName("gatepass_type");
            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(50)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.Purpose)
                .HasMaxLength(200)
                .HasColumnName("purpose");
            entity.Property(e => e.ReferenceDate).HasColumnName("reference_date");
            entity.Property(e => e.ReferenceNo)
                .HasMaxLength(50)
                .HasColumnName("reference_no");
            entity.Property(e => e.ReferenceType)
                .HasMaxLength(50)
                .HasColumnName("reference_type");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'CREATED'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.TotalQuantity)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("total_quantity");
            entity.Property(e => e.VehicleNo)
                .HasMaxLength(50)
                .HasColumnName("vehicle_no");

            entity.HasOne(d => d.Company).WithMany(p => p.TrnGatePasses)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_gate_pass_company");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.TrnGatePasses)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_gate_pass_created_by");
        });

        modelBuilder.Entity<TrnGatePassItem>(entity =>
        {
            entity.HasKey(e => e.GatePassItemId).HasName("trn_gate_pass_item_pkey");

            entity.ToTable("trn_gate_pass_item", "press_db");

            entity.HasIndex(e => e.GatePassId, "idx_gatepass_item_gp");

            entity.HasIndex(e => e.Status, "idx_gatepass_item_status");

            entity.Property(e => e.GatePassItemId).HasColumnName("gate_pass_item_id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .HasColumnName("description");
            entity.Property(e => e.GatePassId).HasColumnName("gate_pass_id");
            entity.Property(e => e.ItemSequence)
                .HasDefaultValue(1)
                .HasColumnName("item_sequence");
            entity.Property(e => e.PendingQuantity)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("pending_quantity");
            entity.Property(e => e.Quantity)
                .HasPrecision(14, 2)
                .HasColumnName("quantity");
            entity.Property(e => e.ReceivedQuantity)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("received_quantity");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'PENDING'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.UomId).HasColumnName("uom_id");

            entity.HasOne(d => d.GatePass).WithMany(p => p.TrnGatePassItems)
                .HasForeignKey(d => d.GatePassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_gatepass_hdr");
        });

        modelBuilder.Entity<TrnGoodsReceipt>(entity =>
        {
            entity.HasKey(e => e.GrnId).HasName("trn_goods_receipt_pkey");

            entity.ToTable("trn_goods_receipt", "press_db", tb => tb.HasComment("Goods Receipt Note (GRN) for material received from suppliers. Links PO to purchase invoice. Supports quality check."));

            entity.HasIndex(e => e.GrnDate, "idx_grn_date");

            entity.HasIndex(e => e.PartyId, "idx_grn_party");

            entity.HasIndex(e => e.PurchaseOrderId, "idx_grn_po");

            entity.HasIndex(e => e.GrnNo, "uq_grn_no").IsUnique();

            entity.Property(e => e.GrnId).HasColumnName("grn_id");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.GrnDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("grn_date");
            entity.Property(e => e.GrnNo)
                .HasMaxLength(50)
                .HasColumnName("grn_no");
            entity.Property(e => e.IsQualityChecked)
                .HasDefaultValue(false)
                .HasColumnName("is_quality_checked");
            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.PartyId).HasColumnName("party_id");
            entity.Property(e => e.PoNo)
                .HasMaxLength(50)
                .HasColumnName("po_no");
            entity.Property(e => e.PurchaseOrderId).HasColumnName("purchase_order_id");
            entity.Property(e => e.QualityCheckedBy).HasColumnName("quality_checked_by");
            entity.Property(e => e.QualityCheckedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("quality_checked_on");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'DRAFT'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.SupplierChallanDate).HasColumnName("supplier_challan_date");
            entity.Property(e => e.SupplierChallanNo)
                .HasMaxLength(100)
                .HasColumnName("supplier_challan_no");
            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            entity.Property(e => e.TotalAcceptedQty)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("total_accepted_qty");
            entity.Property(e => e.TotalQuantity)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("total_quantity");
            entity.Property(e => e.TotalRejectedQty)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("total_rejected_qty");
            entity.Property(e => e.VehicleNo)
                .HasMaxLength(50)
                .HasColumnName("vehicle_no");

            entity.HasOne(d => d.Company).WithMany(p => p.TrnGoodsReceipts)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_grn_company");

            entity.HasOne(d => d.Party).WithMany(p => p.TrnGoodsReceipts)
                .HasForeignKey(d => d.PartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_grn_party");

            entity.HasOne(d => d.PurchaseOrder).WithMany(p => p.TrnGoodsReceipts)
                .HasForeignKey(d => d.PurchaseOrderId)
                .HasConstraintName("fk_grn_po");
        });

        modelBuilder.Entity<TrnGoodsReceiptItem>(entity =>
        {
            entity.HasKey(e => e.GrnItemId).HasName("trn_goods_receipt_item_pkey");

            entity.ToTable("trn_goods_receipt_item", "press_db", tb => tb.HasComment("GRN line items with accepted/rejected quantities and quality check status."));

            entity.HasIndex(e => e.GrnId, "idx_grni_header");

            entity.Property(e => e.GrnItemId).HasColumnName("grn_item_id");
            entity.Property(e => e.AcceptedQuantity)
                .HasPrecision(14, 4)
                .HasDefaultValueSql("0")
                .HasColumnName("accepted_quantity");
            entity.Property(e => e.Amount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("amount");
            entity.Property(e => e.BatchNo)
                .HasMaxLength(50)
                .HasColumnName("batch_no");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ExpiryDate).HasColumnName("expiry_date");
            entity.Property(e => e.GrnId).HasColumnName("grn_id");
            entity.Property(e => e.ItemId).HasColumnName("item_id");
            entity.Property(e => e.ItemSequence)
                .HasDefaultValue(1)
                .HasColumnName("item_sequence");
            entity.Property(e => e.OrderedQuantity)
                .HasPrecision(14, 4)
                .HasDefaultValueSql("0")
                .HasColumnName("ordered_quantity");
            entity.Property(e => e.PoItemId).HasColumnName("po_item_id");
            entity.Property(e => e.QualityStatus)
                .HasMaxLength(30)
                .HasDefaultValueSql("'PENDING'::character varying")
                .HasColumnName("quality_status");
            entity.Property(e => e.ReceivedQuantity)
                .HasPrecision(14, 4)
                .HasDefaultValueSql("0")
                .HasColumnName("received_quantity");
            entity.Property(e => e.RejectedQuantity)
                .HasPrecision(14, 4)
                .HasDefaultValueSql("0")
                .HasColumnName("rejected_quantity");
            entity.Property(e => e.RejectionReason).HasColumnName("rejection_reason");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.UnitRate)
                .HasPrecision(14, 4)
                .HasDefaultValueSql("0")
                .HasColumnName("unit_rate");
            entity.Property(e => e.UomId).HasColumnName("uom_id");

            entity.HasOne(d => d.Grn).WithMany(p => p.TrnGoodsReceiptItems)
                .HasForeignKey(d => d.GrnId)
                .HasConstraintName("fk_grni_header");
        });

        modelBuilder.Entity<TrnJob>(entity =>
        {
            entity.HasKey(e => e.JobId).HasName("trn_job_pkey");

            entity.ToTable("trn_job", "press_db");

            entity.HasIndex(e => e.AssignedTo, "idx_job_assigned_to");

            entity.HasIndex(e => e.JobCategoryId, "idx_job_category");

            entity.HasIndex(e => e.CompanyId, "idx_job_company");

            entity.HasIndex(e => new { e.CompanyId, e.StatusCode }, "idx_job_company_status");

            entity.HasIndex(e => e.CurrentProcessId, "idx_job_current_process");

            entity.HasIndex(e => e.JobDate, "idx_job_date");

            entity.HasIndex(e => e.DeliveryDate, "idx_job_delivery_date");

            entity.HasIndex(e => e.EnquiryId, "idx_job_enquiry");

            entity.HasIndex(e => e.PartyId, "idx_job_party");

            entity.HasIndex(e => new { e.PartyId, e.StatusCode }, "idx_job_party_status");

            entity.HasIndex(e => e.Priority, "idx_job_priority");

            entity.HasIndex(e => e.QuotationId, "idx_job_quotation");

            entity.HasIndex(e => e.RateCalcId, "idx_job_rate_calc");

            entity.HasIndex(e => e.StatusCode, "idx_job_status");

            entity.HasIndex(e => e.JobTypeId, "idx_job_type");

            entity.HasIndex(e => e.JobNo, "trn_job_job_no_key").IsUnique();

            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.ActualCost)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("actual_cost");
            entity.Property(e => e.AiBottleneckJson)
                .HasColumnType("jsonb")
                .HasColumnName("ai_bottleneck_json");
            entity.Property(e => e.AiEstimatedCompletion).HasColumnName("ai_estimated_completion");
            entity.Property(e => e.AiPriorityScore).HasColumnName("ai_priority_score");
            entity.Property(e => e.AssignedTo).HasColumnName("assigned_to");
            entity.Property(e => e.ClosedBy).HasColumnName("closed_by");
            entity.Property(e => e.ClosedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("closed_on");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.CompletedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("completed_on");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.CurrentProcessId).HasColumnName("current_process_id");
            entity.Property(e => e.CurrentStage)
                .HasMaxLength(50)
                .HasColumnName("current_stage");
            entity.Property(e => e.DeliveryDate).HasColumnName("delivery_date");
            entity.Property(e => e.DiscountAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("discount_amount");
            entity.Property(e => e.EnquiryId).HasColumnName("enquiry_id");
            entity.Property(e => e.EstimatedCost)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("estimated_cost");
            entity.Property(e => e.GrossAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("gross_amount");
            entity.Property(e => e.JobCategoryId).HasColumnName("job_category_id");
            entity.Property(e => e.JobDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("job_date");
            entity.Property(e => e.JobNo)
                .HasMaxLength(30)
                .HasColumnName("job_no");
            entity.Property(e => e.JobTypeId).HasColumnName("job_type_id");
            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(50)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.NetAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("net_amount");
            entity.Property(e => e.PartyId).HasColumnName("party_id");
            entity.Property(e => e.PartyRefNo)
                .HasMaxLength(20)
                .HasColumnName("party_ref_no");
            entity.Property(e => e.PartyRefNoDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("party_ref_no_date");
            entity.Property(e => e.Priority)
                .HasMaxLength(20)
                .HasDefaultValueSql("'NORMAL'::character varying")
                .HasColumnName("priority");
            entity.Property(e => e.ProductDescription).HasColumnName("product_description");
            entity.Property(e => e.ProductName)
                .HasMaxLength(200)
                .HasColumnName("product_name");
            entity.Property(e => e.ProgressPercent)
                .HasDefaultValue(0)
                .HasColumnName("progress_percent");
            entity.Property(e => e.Quantity)
                .HasDefaultValue(1)
                .HasColumnName("quantity");
            entity.Property(e => e.QuotationId).HasColumnName("quotation_id");
            entity.Property(e => e.QuotedAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("quoted_amount");
            entity.Property(e => e.RateCalcId).HasColumnName("rate_calc_id");
            entity.Property(e => e.SpecificationsJson)
                .HasColumnType("jsonb")
                .HasColumnName("specifications_json");
            entity.Property(e => e.StatusCode)
                .HasMaxLength(30)
                .HasDefaultValueSql("'CREATED'::character varying")
                .HasColumnName("status_code");
            entity.Property(e => e.TaxAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("tax_amount");
            entity.Property(e => e.TaxableAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("taxable_amount");
            entity.Property(e => e.TotalPages)
                .HasDefaultValue(0)
                .HasColumnName("total_pages");

            entity.HasOne(d => d.AssignedToNavigation).WithMany(p => p.TrnJobAssignedToNavigations)
                .HasForeignKey(d => d.AssignedTo)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_job_assigned_to");

            entity.HasOne(d => d.Company).WithMany(p => p.TrnJobs)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_job_company");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.TrnJobCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_job_created_by");

            entity.HasOne(d => d.Enquiry).WithMany(p => p.TrnJobs)
                .HasForeignKey(d => d.EnquiryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_job_enquiry");

            entity.HasOne(d => d.JobCategory).WithMany(p => p.TrnJobs)
                .HasForeignKey(d => d.JobCategoryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_job_category");

            entity.HasOne(d => d.JobType).WithMany(p => p.TrnJobs)
                .HasForeignKey(d => d.JobTypeId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_job_type");

            entity.HasOne(d => d.Party).WithMany(p => p.TrnJobs)
                .HasForeignKey(d => d.PartyId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_job_party");

            entity.HasOne(d => d.Quotation).WithMany(p => p.TrnJobs)
                .HasForeignKey(d => d.QuotationId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_job_quotation");

            entity.HasOne(d => d.RateCalc).WithMany(p => p.TrnJobs)
                .HasForeignKey(d => d.RateCalcId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_job_rate_calc");
        });

        modelBuilder.Entity<TrnJobItem>(entity =>
        {
            entity.HasKey(e => e.JobItemId).HasName("trn_job_item_pkey");

            entity.ToTable("trn_job_item", "press_db", tb => tb.HasComment("Line items for trn_job. Holds product identity, pricing, specs and a frozen cost_breakdown JSONB snapshot from the rate calculator at job time. Detailed calculation data lives in hyb_job_rate_calculator via rate_calculator_id."));

            entity.HasIndex(e => e.EnquiryItemId, "idx_job_item_enq_item");

            entity.HasIndex(e => e.JobId, "idx_job_item_job");

            entity.HasIndex(e => e.JobTypeId, "idx_job_item_job_type");

            entity.HasIndex(e => e.PrintProductTypeId, "idx_job_item_product_type");

            entity.HasIndex(e => e.QuotationItemId, "idx_job_item_quot_item");

            entity.HasIndex(e => e.RateCalculatorId, "idx_job_item_rate_calc");

            entity.HasIndex(e => e.Status, "idx_job_item_status");

            entity.HasIndex(e => e.TaxCategoryId, "idx_job_item_tax_category");

            entity.Property(e => e.JobItemId).HasColumnName("job_item_id");
            entity.Property(e => e.CalcRefNo)
                .HasMaxLength(50)
                .HasComment("Denormalized reference number from hyb_job_rate_calculator for quick display on PDF/UI.")
                .HasColumnName("calc_ref_no");
            entity.Property(e => e.CessAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("cess_amount");
            entity.Property(e => e.CessPercent)
                .HasPrecision(6, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("cess_percent");
            entity.Property(e => e.CgstAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("cgst_amount");
            entity.Property(e => e.CgstPercent)
                .HasPrecision(6, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("cgst_percent");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.DeliveredQuantity)
                .HasDefaultValue(0)
                .HasColumnName("delivered_quantity");
            entity.Property(e => e.DiscountAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("discount_amount");
            entity.Property(e => e.DiscountPercent)
                .HasPrecision(6, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("discount_percent");
            entity.Property(e => e.EnquiryItemId).HasColumnName("enquiry_item_id");
            entity.Property(e => e.GrossAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("gross_amount");
            entity.Property(e => e.HsnSacCode)
                .HasMaxLength(20)
                .HasColumnName("hsn_sac_code");
            entity.Property(e => e.IgstAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("igst_amount");
            entity.Property(e => e.IgstPercent)
                .HasPrecision(6, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("igst_percent");
            entity.Property(e => e.InternalRemarks).HasColumnName("internal_remarks");
            entity.Property(e => e.ItemSequence)
                .HasDefaultValue(1)
                .HasColumnName("item_sequence");
            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.JobTypeId).HasColumnName("job_type_id");
            entity.Property(e => e.JobTypeName)
                .HasMaxLength(100)
                .HasColumnName("job_type_name");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(50)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.NetAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("net_amount");
            entity.Property(e => e.NoOfPages).HasColumnName("no_of_pages");
            entity.Property(e => e.PendingQuantity)
                .HasDefaultValue(0)
                .HasColumnName("pending_quantity");
            entity.Property(e => e.PrintProductTypeId).HasColumnName("print_product_type_id");
            entity.Property(e => e.PrintingMethod)
                .HasMaxLength(30)
                .HasColumnName("printing_method");
            entity.Property(e => e.ProductDescription)
                .HasMaxLength(300)
                .HasColumnName("product_description");
            entity.Property(e => e.ProductName)
                .HasMaxLength(150)
                .HasColumnName("product_name");
            entity.Property(e => e.ProductSizeName)
                .HasMaxLength(100)
                .HasColumnName("product_size_name");
            entity.Property(e => e.ProductTypeName)
                .HasMaxLength(100)
                .HasColumnName("product_type_name");
            entity.Property(e => e.Quantity)
                .HasDefaultValue(0)
                .HasColumnName("quantity");
            entity.Property(e => e.QuotationItemId).HasColumnName("quotation_item_id");
            entity.Property(e => e.RateCalculatorId).HasColumnName("rate_calculator_id");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.SgstAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("sgst_amount");
            entity.Property(e => e.SgstPercent)
                .HasPrecision(6, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("sgst_percent");
            entity.Property(e => e.SortOrder)
                .HasDefaultValue(0)
                .HasColumnName("sort_order");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'ACTIVE'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.TaxCategoryId).HasColumnName("tax_category_id");
            entity.Property(e => e.TaxableValue)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("taxable_value");
            entity.Property(e => e.TotalTaxAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("total_tax_amount");
            entity.Property(e => e.TrimHeightMm)
                .HasPrecision(8, 2)
                .HasColumnName("trim_height_mm");
            entity.Property(e => e.TrimWidthMm)
                .HasPrecision(8, 2)
                .HasColumnName("trim_width_mm");
            entity.Property(e => e.UnitRate)
                .HasPrecision(14, 4)
                .HasDefaultValueSql("0")
                .HasColumnName("unit_rate");
            entity.Property(e => e.UomId).HasColumnName("uom_id");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.TrnJobItems)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_job_item_created_by");

            entity.HasOne(d => d.Job).WithMany(p => p.TrnJobItems)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("fk_job_item_job");

            entity.HasOne(d => d.JobType).WithMany(p => p.TrnJobItems)
                .HasForeignKey(d => d.JobTypeId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_job_item_job_type");

            entity.HasOne(d => d.PrintProductType).WithMany(p => p.TrnJobItems)
                .HasForeignKey(d => d.PrintProductTypeId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_job_item_product_type");

            entity.HasOne(d => d.RateCalculator).WithMany(p => p.TrnJobItems)
                .HasForeignKey(d => d.RateCalculatorId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_job_item_rate_calc");
        });

        modelBuilder.Entity<TrnJobMachineAllocation>(entity =>
        {
            entity.HasKey(e => e.AllocationId).HasName("trn_job_machine_allocation_pkey");

            entity.ToTable("trn_job_machine_allocation", "press_db");

            entity.HasIndex(e => e.JobId, "idx_job_machine_allocation_job");

            entity.HasIndex(e => e.MachineId, "idx_job_machine_allocation_machine");

            entity.HasIndex(e => e.ProcessCode, "idx_job_machine_allocation_process");

            entity.HasIndex(e => e.AllocationStatus, "idx_job_machine_allocation_status");

            entity.Property(e => e.AllocationId).HasColumnName("allocation_id");
            entity.Property(e => e.ActualEndTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("actual_end_time");
            entity.Property(e => e.ActualHours)
                .HasPrecision(10, 2)
                .HasColumnName("actual_hours");
            entity.Property(e => e.ActualStartTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("actual_start_time");
            entity.Property(e => e.AllocationStatus)
                .HasMaxLength(30)
                .HasDefaultValueSql("'Planned'::character varying")
                .HasColumnName("allocation_status");
            entity.Property(e => e.CompletedQuantity)
                .HasPrecision(18, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("completed_quantity");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.EstimatedHours)
                .HasPrecision(10, 2)
                .HasColumnName("estimated_hours");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.JobNo)
                .HasMaxLength(50)
                .HasColumnName("job_no");
            entity.Property(e => e.MachineCode)
                .HasMaxLength(50)
                .HasColumnName("machine_code");
            entity.Property(e => e.MachineId).HasColumnName("machine_id");
            entity.Property(e => e.MachineName)
                .HasMaxLength(150)
                .HasColumnName("machine_name");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(50)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.PlannedEndTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("planned_end_time");
            entity.Property(e => e.PlannedQuantity)
                .HasPrecision(18, 3)
                .HasColumnName("planned_quantity");
            entity.Property(e => e.PlannedStartTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("planned_start_time");
            entity.Property(e => e.ProcessCode)
                .HasMaxLength(50)
                .HasColumnName("process_code");
            entity.Property(e => e.ProcessName)
                .HasMaxLength(100)
                .HasColumnName("process_name");
            entity.Property(e => e.Remarks).HasColumnName("remarks");

            entity.HasOne(d => d.Job).WithMany(p => p.TrnJobMachineAllocations)
                .HasForeignKey(d => d.JobId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_job_machine_allocation_job");

            entity.HasOne(d => d.Machine).WithMany(p => p.TrnJobMachineAllocations)
                .HasForeignKey(d => d.MachineId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_job_machine_allocation_machine");
        });

        modelBuilder.Entity<TrnJobMachineManpowerAllocation>(entity =>
        {
            entity.HasKey(e => e.ManpowerAllocationId).HasName("trn_job_machine_manpower_allocation_pkey");

            entity.ToTable("trn_job_machine_manpower_allocation", "press_db");

            entity.HasIndex(e => e.AllocationId, "idx_manpower_allocation");

            entity.HasIndex(e => e.EmployeeId, "idx_manpower_employee");

            entity.HasIndex(e => e.JobId, "idx_manpower_job");

            entity.HasIndex(e => e.MachineId, "idx_manpower_machine");

            entity.HasIndex(e => e.AllocationStatus, "idx_manpower_status");

            entity.Property(e => e.ManpowerAllocationId).HasColumnName("manpower_allocation_id");
            entity.Property(e => e.ActualEndTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("actual_end_time");
            entity.Property(e => e.ActualHours)
                .HasPrecision(10, 2)
                .HasColumnName("actual_hours");
            entity.Property(e => e.ActualStartTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("actual_start_time");
            entity.Property(e => e.AllocationId).HasColumnName("allocation_id");
            entity.Property(e => e.AllocationStatus)
                .HasMaxLength(30)
                .HasDefaultValueSql("'Planned'::character varying")
                .HasColumnName("allocation_status");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.EmployeeCode)
                .HasMaxLength(50)
                .HasColumnName("employee_code");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.EmployeeName)
                .HasMaxLength(150)
                .HasColumnName("employee_name");
            entity.Property(e => e.EstimatedHours)
                .HasPrecision(10, 2)
                .HasColumnName("estimated_hours");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.JobNo)
                .HasMaxLength(50)
                .HasColumnName("job_no");
            entity.Property(e => e.MachineId).HasColumnName("machine_id");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(50)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.PlannedEndTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("planned_end_time");
            entity.Property(e => e.PlannedStartTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("planned_start_time");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.RoleCode)
                .HasMaxLength(50)
                .HasColumnName("role_code");
            entity.Property(e => e.RoleName)
                .HasMaxLength(100)
                .HasColumnName("role_name");
            entity.Property(e => e.ShiftCode)
                .HasMaxLength(20)
                .HasColumnName("shift_code");

            entity.HasOne(d => d.Allocation).WithMany(p => p.TrnJobMachineManpowerAllocations)
                .HasForeignKey(d => d.AllocationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_manpower_allocation_machine");

            entity.HasOne(d => d.Employee).WithMany(p => p.TrnJobMachineManpowerAllocations)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_manpower_employee");

            entity.HasOne(d => d.Machine).WithMany(p => p.TrnJobMachineManpowerAllocations)
                .HasForeignKey(d => d.MachineId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_manpower_machine");
        });

        modelBuilder.Entity<TrnJobOutsource>(entity =>
        {
            entity.HasKey(e => e.OutsourceId).HasName("trn_job_outsource_pkey");

            entity.ToTable("trn_job_outsource", "press_db");

            entity.HasIndex(e => e.OutsourceDate, "idx_outsource_date");

            entity.HasIndex(e => e.Status, "idx_outsource_status");

            entity.HasIndex(e => e.VendorId, "idx_outsource_vendor");

            entity.HasIndex(e => e.OutsourceNo, "trn_job_outsource_outsource_no_key").IsUnique();

            entity.Property(e => e.OutsourceId).HasColumnName("outsource_id");
            entity.Property(e => e.ActualDeliveryDate).HasColumnName("actual_delivery_date");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.ExpectedDeliveryDate).HasColumnName("expected_delivery_date");
            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(50)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.OutsourceDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("outsource_date");
            entity.Property(e => e.OutsourceNo)
                .HasMaxLength(30)
                .HasColumnName("outsource_no");
            entity.Property(e => e.ProcessType)
                .HasMaxLength(50)
                .HasColumnName("process_type");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'CREATED'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("total_amount");
            entity.Property(e => e.TotalQuantity)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("total_quantity");
            entity.Property(e => e.VendorId).HasColumnName("vendor_id");

            entity.HasOne(d => d.Job).WithMany(p => p.TrnJobOutsources)
                .HasForeignKey(d => d.JobId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_outsource_job");
        });

        modelBuilder.Entity<TrnJobOutsourceItem>(entity =>
        {
            entity.HasKey(e => e.OutsourceItemId).HasName("trn_job_outsource_item_pkey");

            entity.ToTable("trn_job_outsource_item", "press_db");

            entity.HasIndex(e => e.JobItemId, "idx_outsource_item_jobitem");

            entity.HasIndex(e => e.OutsourceId, "idx_outsource_item_outsrc");

            entity.HasIndex(e => e.Status, "idx_outsource_item_status");

            entity.Property(e => e.OutsourceItemId).HasColumnName("outsource_item_id");
            entity.Property(e => e.Amount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("amount");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.ItemSequence)
                .HasDefaultValue(1)
                .HasColumnName("item_sequence");
            entity.Property(e => e.JobItemId).HasColumnName("job_item_id");
            entity.Property(e => e.OutsourceId).HasColumnName("outsource_id");
            entity.Property(e => e.ProcessName)
                .HasMaxLength(100)
                .HasColumnName("process_name");
            entity.Property(e => e.ProductName)
                .HasMaxLength(150)
                .HasColumnName("product_name");
            entity.Property(e => e.Quantity)
                .HasPrecision(14, 2)
                .HasColumnName("quantity");
            entity.Property(e => e.Rate)
                .HasPrecision(14, 4)
                .HasDefaultValueSql("0")
                .HasColumnName("rate");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'PENDING'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.UomId).HasColumnName("uom_id");

            entity.HasOne(d => d.JobItem).WithMany(p => p.TrnJobOutsourceItems)
                .HasForeignKey(d => d.JobItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_outsource_item_job_item");

            entity.HasOne(d => d.Outsource).WithMany(p => p.TrnJobOutsourceItems)
                .HasForeignKey(d => d.OutsourceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_outsource_item_hdr");
        });

        modelBuilder.Entity<TrnJobTimeline>(entity =>
        {
            entity.HasKey(e => e.TimelineId).HasName("trn_job_timeline_pkey");

            entity.ToTable("trn_job_timeline", "press_db");

            entity.HasIndex(e => e.CreatedOn, "idx_job_timeline_created_on").IsDescending();

            entity.HasIndex(e => e.JobId, "idx_job_timeline_job_id");

            entity.HasIndex(e => e.NewStatus, "idx_job_timeline_status");

            entity.Property(e => e.TimelineId).HasColumnName("timeline_id");
            entity.Property(e => e.AssignedToUserId).HasColumnName("assigned_to_user_id");
            entity.Property(e => e.AttachmentUrl).HasColumnName("attachment_url");
            entity.Property(e => e.CommunicationMode)
                .HasMaxLength(50)
                .HasColumnName("communication_mode");
            entity.Property(e => e.CommunicationReference).HasColumnName("communication_reference");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.EnquiryId).HasColumnName("enquiry_id");
            entity.Property(e => e.EventCode)
                .HasMaxLength(50)
                .HasColumnName("event_code");
            entity.Property(e => e.EventDescription).HasColumnName("event_description");
            entity.Property(e => e.EventTitle)
                .HasMaxLength(200)
                .HasColumnName("event_title");
            entity.Property(e => e.EventType)
                .HasMaxLength(50)
                .HasColumnName("event_type");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.MachineId).HasColumnName("machine_id");
            entity.Property(e => e.NewAmount)
                .HasPrecision(18, 2)
                .HasColumnName("new_amount");
            entity.Property(e => e.NewStatus)
                .HasMaxLength(50)
                .HasColumnName("new_status");
            entity.Property(e => e.OldAmount)
                .HasPrecision(18, 2)
                .HasColumnName("old_amount");
            entity.Property(e => e.OldStatus)
                .HasMaxLength(50)
                .HasColumnName("old_status");
            entity.Property(e => e.OperatorId).HasColumnName("operator_id");
            entity.Property(e => e.ProcessCode)
                .HasMaxLength(50)
                .HasColumnName("process_code");
            entity.Property(e => e.ProcessName)
                .HasMaxLength(100)
                .HasColumnName("process_name");
            entity.Property(e => e.QuotationId).HasColumnName("quotation_id");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
            entity.Property(e => e.UpdatedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_on");
            entity.Property(e => e.WorkCenterId).HasColumnName("work_center_id");

            entity.HasOne(d => d.Enquiry).WithMany(p => p.TrnJobTimelines)
                .HasForeignKey(d => d.EnquiryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_job_timeline_enquiry");

            entity.HasOne(d => d.Job).WithMany(p => p.TrnJobTimelines)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("fk_job_timeline_job");

            entity.HasOne(d => d.Quotation).WithMany(p => p.TrnJobTimelines)
                .HasForeignKey(d => d.QuotationId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_job_timeline_quotation");
        });

        modelBuilder.Entity<TrnJournalVoucher>(entity =>
        {
            entity.HasKey(e => e.JournalId).HasName("trn_journal_voucher_pkey");

            entity.ToTable("trn_journal_voucher", "press_db", tb => tb.HasComment("General journal voucher header. Supports manual entries, auto-generated GL postings from invoices/payments, and reversing entries. Debit must equal Credit."));

            entity.HasIndex(e => e.CompanyId, "idx_jv_company");

            entity.HasIndex(e => e.JournalDate, "idx_jv_date");

            entity.HasIndex(e => e.Status, "idx_jv_status");

            entity.HasIndex(e => e.JournalNo, "uq_journal_no").IsUnique();

            entity.Property(e => e.JournalId).HasColumnName("journal_id");
            entity.Property(e => e.CancelReason).HasColumnName("cancel_reason");
            entity.Property(e => e.CancelledBy).HasColumnName("cancelled_by");
            entity.Property(e => e.CancelledOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("cancelled_on");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.FinYearId).HasColumnName("fin_year_id");
            entity.Property(e => e.IsAutoGenerated)
                .HasDefaultValue(false)
                .HasColumnName("is_auto_generated");
            entity.Property(e => e.IsCancelled)
                .HasDefaultValue(false)
                .HasColumnName("is_cancelled");
            entity.Property(e => e.IsPosted)
                .HasDefaultValue(false)
                .HasColumnName("is_posted");
            entity.Property(e => e.IsReversingEntry)
                .HasDefaultValue(false)
                .HasColumnName("is_reversing_entry");
            entity.Property(e => e.JournalDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("journal_date");
            entity.Property(e => e.JournalNo)
                .HasMaxLength(50)
                .HasColumnName("journal_no");
            entity.Property(e => e.JournalType)
                .HasMaxLength(30)
                .HasDefaultValueSql("'GENERAL'::character varying")
                .HasComment("GENERAL, OPENING, CLOSING, ADJUSTMENT, DEPRECIATION, PROVISION, REVERSAL, AUTO")
                .HasColumnName("journal_type");
            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.Narration).HasColumnName("narration");
            entity.Property(e => e.OriginalJournalId).HasColumnName("original_journal_id");
            entity.Property(e => e.PostedBy).HasColumnName("posted_by");
            entity.Property(e => e.PostedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("posted_on");
            entity.Property(e => e.ReferenceDate).HasColumnName("reference_date");
            entity.Property(e => e.ReferenceNo)
                .HasMaxLength(100)
                .HasColumnName("reference_no");
            entity.Property(e => e.ReversalDate).HasColumnName("reversal_date");
            entity.Property(e => e.SourceVoucherId).HasColumnName("source_voucher_id");
            entity.Property(e => e.SourceVoucherNo)
                .HasMaxLength(50)
                .HasColumnName("source_voucher_no");
            entity.Property(e => e.SourceVoucherType)
                .HasMaxLength(50)
                .HasColumnName("source_voucher_type");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'DRAFT'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.TotalCredit)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("total_credit");
            entity.Property(e => e.TotalDebit)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("total_debit");

            entity.HasOne(d => d.Company).WithMany(p => p.TrnJournalVouchers)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_jv_company");

            entity.HasOne(d => d.OriginalJournal).WithMany(p => p.InverseOriginalJournal)
                .HasForeignKey(d => d.OriginalJournalId)
                .HasConstraintName("fk_jv_original");
        });

        modelBuilder.Entity<TrnJournalVoucherLine>(entity =>
        {
            entity.HasKey(e => e.JournalLineId).HasName("trn_journal_voucher_line_pkey");

            entity.ToTable("trn_journal_voucher_line", "press_db", tb => tb.HasComment("Journal voucher debit/credit lines. Each line posts to one account head. Sum of debits must equal sum of credits within a journal."));

            entity.HasIndex(e => e.AccountHeadId, "idx_jvl_account");

            entity.HasIndex(e => e.JournalId, "idx_jvl_header");

            entity.Property(e => e.JournalLineId).HasColumnName("journal_line_id");
            entity.Property(e => e.AccountHeadId).HasColumnName("account_head_id");
            entity.Property(e => e.CostCenterId).HasColumnName("cost_center_id");
            entity.Property(e => e.CreditAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("credit_amount");
            entity.Property(e => e.DebitAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("debit_amount");
            entity.Property(e => e.JournalId).HasColumnName("journal_id");
            entity.Property(e => e.LineNo)
                .HasDefaultValue(1)
                .HasColumnName("line_no");
            entity.Property(e => e.Narration).HasColumnName("narration");
            entity.Property(e => e.PartyId).HasColumnName("party_id");
            entity.Property(e => e.ReferenceId).HasColumnName("reference_id");
            entity.Property(e => e.ReferenceNo)
                .HasMaxLength(100)
                .HasColumnName("reference_no");
            entity.Property(e => e.ReferenceType)
                .HasMaxLength(50)
                .HasColumnName("reference_type");

            entity.HasOne(d => d.AccountHead).WithMany(p => p.TrnJournalVoucherLines)
                .HasForeignKey(d => d.AccountHeadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_jvl_account");

            entity.HasOne(d => d.Journal).WithMany(p => p.TrnJournalVoucherLines)
                .HasForeignKey(d => d.JournalId)
                .HasConstraintName("fk_jvl_header");
        });

        modelBuilder.Entity<TrnLedger>(entity =>
        {
            entity.HasKey(e => e.LedgerId).HasName("trn_ledger_pkey");

            entity.ToTable("trn_ledger", "press_db");

            entity.HasIndex(e => e.AccountHeadId, "idx_ledger_account");

            entity.HasIndex(e => new { e.AccountHeadId, e.TransactionDate }, "idx_ledger_acct_date");

            entity.HasIndex(e => e.PartyId, "idx_ledger_party");

            entity.HasIndex(e => e.TransactionDate, "idx_ledger_txn_date");

            entity.HasIndex(e => e.VoucherNo, "idx_ledger_voucher_no");

            entity.HasIndex(e => e.VoucherTypeId, "idx_ledger_voucher_type");

            entity.Property(e => e.LedgerId).HasColumnName("ledger_id");
            entity.Property(e => e.AccountHeadId).HasColumnName("account_head_id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.CreditAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("credit_amount");
            entity.Property(e => e.DebitAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("debit_amount");
            entity.Property(e => e.PartyId).HasColumnName("party_id");
            entity.Property(e => e.ReferenceDate).HasColumnName("reference_date");
            entity.Property(e => e.ReferenceNo)
                .HasMaxLength(100)
                .HasColumnName("reference_no");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.TransactionDate).HasColumnName("transaction_date");
            entity.Property(e => e.VoucherId).HasColumnName("voucher_id");
            entity.Property(e => e.VoucherNo)
                .HasMaxLength(50)
                .HasColumnName("voucher_no");
            entity.Property(e => e.VoucherTypeId).HasColumnName("voucher_type_id");

            entity.HasOne(d => d.AccountHead).WithMany(p => p.TrnLedgers)
                .HasForeignKey(d => d.AccountHeadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ledger_account");

            entity.HasOne(d => d.Party).WithMany(p => p.TrnLedgers)
                .HasForeignKey(d => d.PartyId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_acct_ledger_party");

            entity.HasOne(d => d.VoucherType).WithMany(p => p.TrnLedgers)
                .HasForeignKey(d => d.VoucherTypeId)
                .HasConstraintName("fk_voucher_type_id");
        });

        modelBuilder.Entity<TrnMachineBreakdown>(entity =>
        {
            entity.HasKey(e => e.BreakdownId).HasName("trn_machine_breakdown_pkey");

            entity.ToTable("trn_machine_breakdown", "press_db", tb => tb.HasComment("Stores machine fault and breakdown records"));

            entity.HasIndex(e => e.MachineId, "idx_breakdown_machine");

            entity.HasIndex(e => e.BreakdownStartTime, "idx_breakdown_start");

            entity.HasIndex(e => e.BreakdownStatus, "idx_breakdown_status");

            entity.Property(e => e.BreakdownId)
                .HasComment("Primary key for machine breakdown record")
                .HasColumnName("breakdown_id");
            entity.Property(e => e.BreakdownEndTime)
                .HasComment("Timestamp when breakdown ended")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("breakdown_end_time");
            entity.Property(e => e.BreakdownStartTime)
                .HasComment("Timestamp when machine breakdown started")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("breakdown_start_time");
            entity.Property(e => e.BreakdownStatus)
                .HasMaxLength(30)
                .HasDefaultValueSql("'Open'::character varying")
                .HasComment("Current breakdown status (Open, Assigned, In Progress, Resolved, Closed)")
                .HasColumnName("breakdown_status");
            entity.Property(e => e.CorrectiveAction)
                .HasComment("Action taken to fix the breakdown")
                .HasColumnName("corrective_action");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .HasComment("User who created the breakdown record")
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("Record creation timestamp")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.DowntimeMinutes)
                .HasPrecision(10, 2)
                .HasComment("Total downtime in minutes caused by breakdown")
                .HasColumnName("downtime_minutes");
            entity.Property(e => e.FaultCategory)
                .HasMaxLength(50)
                .HasComment("Fault category (Mechanical, Electrical, Software, Operator Error)")
                .HasColumnName("fault_category");
            entity.Property(e => e.FaultCode)
                .HasMaxLength(50)
                .HasComment("Unique code identifying the fault type")
                .HasColumnName("fault_code");
            entity.Property(e => e.FaultDescription)
                .HasComment("Detailed description of the fault")
                .HasColumnName("fault_description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasComment("Indicates whether breakdown record is active")
                .HasColumnName("is_active");
            entity.Property(e => e.MachineId)
                .HasComment("Reference to machine where breakdown occurred")
                .HasColumnName("machine_id");
            entity.Property(e => e.PreventiveAction)
                .HasComment("Preventive steps taken to avoid recurrence")
                .HasColumnName("preventive_action");
            entity.Property(e => e.Remarks)
                .HasComment("Additional notes related to breakdown")
                .HasColumnName("remarks");
            entity.Property(e => e.RepairCost)
                .HasPrecision(10, 2)
                .HasComment("Total repair cost incurred")
                .HasColumnName("repair_cost");
            entity.Property(e => e.ReportedBy)
                .HasMaxLength(100)
                .HasComment("Name or ID of person who reported the breakdown")
                .HasColumnName("reported_by");
            entity.Property(e => e.ResolvedDate)
                .HasComment("Date when issue was resolved")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("resolved_date");
            entity.Property(e => e.RootCause)
                .HasComment("Root cause identified after analysis")
                .HasColumnName("root_cause");
            entity.Property(e => e.SeverityLevel)
                .HasMaxLength(30)
                .HasComment("Severity level of breakdown (Low, Medium, High, Critical)")
                .HasColumnName("severity_level");
            entity.Property(e => e.SparePartsUsed)
                .HasComment("List of spare parts used during repair")
                .HasColumnName("spare_parts_used");
            entity.Property(e => e.TechnicianId)
                .HasComment("Reference to technician assigned to fix the issue")
                .HasColumnName("technician_id");
            entity.Property(e => e.TechnicianName)
                .HasMaxLength(150)
                .HasComment("Name of technician handling the repair")
                .HasColumnName("technician_name");

            entity.HasOne(d => d.Machine).WithMany(p => p.TrnMachineBreakdowns)
                .HasForeignKey(d => d.MachineId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_breakdown_machine");
        });

        modelBuilder.Entity<TrnNotification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("trn_notification_pkey");

            entity.ToTable("trn_notification", "press_db");

            entity.HasIndex(e => e.Channel, "idx_notif_channel");

            entity.HasIndex(e => new { e.Module, e.EventType }, "idx_notif_module_event");

            entity.HasIndex(e => e.RecipientPartyId, "idx_notif_recipient_party");

            entity.HasIndex(e => e.RecipientUserId, "idx_notif_recipient_user");

            entity.HasIndex(e => e.ScheduledAt, "idx_notif_scheduled").HasFilter("((status)::text = 'QUEUED'::text)");

            entity.HasIndex(e => e.Status, "idx_notif_status");

            entity.HasIndex(e => e.TemplateId, "idx_notif_template");

            entity.Property(e => e.NotificationId).HasColumnName("notification_id");
            entity.Property(e => e.AiGenerated)
                .HasDefaultValue(false)
                .HasColumnName("ai_generated");
            entity.Property(e => e.AiPersonalized)
                .HasDefaultValue(false)
                .HasColumnName("ai_personalized");
            entity.Property(e => e.AiSummary).HasColumnName("ai_summary");
            entity.Property(e => e.AttachmentsJson)
                .HasColumnType("jsonb")
                .HasColumnName("attachments_json");
            entity.Property(e => e.Body).HasColumnName("body");
            entity.Property(e => e.BodyFormat)
                .HasMaxLength(20)
                .HasDefaultValueSql("'HTML'::character varying")
                .HasColumnName("body_format");
            entity.Property(e => e.Channel)
                .HasMaxLength(20)
                .HasColumnName("channel");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.DeliveredAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("delivered_at");
            entity.Property(e => e.EventType)
                .HasMaxLength(50)
                .HasColumnName("event_type");
            entity.Property(e => e.ExternalRefId)
                .HasMaxLength(100)
                .HasColumnName("external_ref_id");
            entity.Property(e => e.FailureReason).HasColumnName("failure_reason");
            entity.Property(e => e.MaxRetries)
                .HasDefaultValue(3)
                .HasColumnName("max_retries");
            entity.Property(e => e.MetadataJson)
                .HasColumnType("jsonb")
                .HasColumnName("metadata_json");
            entity.Property(e => e.Module)
                .HasMaxLength(50)
                .HasColumnName("module");
            entity.Property(e => e.NotificationNo)
                .HasMaxLength(30)
                .HasColumnName("notification_no");
            entity.Property(e => e.Priority)
                .HasMaxLength(20)
                .HasDefaultValueSql("'NORMAL'::character varying")
                .HasColumnName("priority");
            entity.Property(e => e.ProviderName)
                .HasMaxLength(50)
                .HasColumnName("provider_name");
            entity.Property(e => e.ProviderResponse).HasColumnName("provider_response");
            entity.Property(e => e.ReadAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("read_at");
            entity.Property(e => e.RecipientEmail)
                .HasMaxLength(150)
                .HasColumnName("recipient_email");
            entity.Property(e => e.RecipientMobile)
                .HasMaxLength(20)
                .HasColumnName("recipient_mobile");
            entity.Property(e => e.RecipientName)
                .HasMaxLength(150)
                .HasColumnName("recipient_name");
            entity.Property(e => e.RecipientPartyId).HasColumnName("recipient_party_id");
            entity.Property(e => e.RecipientType)
                .HasMaxLength(30)
                .HasColumnName("recipient_type");
            entity.Property(e => e.RecipientUserId).HasColumnName("recipient_user_id");
            entity.Property(e => e.RecipientWhatsapp)
                .HasMaxLength(20)
                .HasColumnName("recipient_whatsapp");
            entity.Property(e => e.ReferenceId).HasColumnName("reference_id");
            entity.Property(e => e.ReferenceNo)
                .HasMaxLength(50)
                .HasColumnName("reference_no");
            entity.Property(e => e.RetryCount)
                .HasDefaultValue(0)
                .HasColumnName("retry_count");
            entity.Property(e => e.ScheduledAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("scheduled_at");
            entity.Property(e => e.SentAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("sent_at");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'QUEUED'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.Subject)
                .HasMaxLength(500)
                .HasColumnName("subject");
            entity.Property(e => e.TemplateId).HasColumnName("template_id");

            entity.HasOne(d => d.RecipientUser).WithMany(p => p.TrnNotifications)
                .HasForeignKey(d => d.RecipientUserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_notification_recipient_user");

            entity.HasOne(d => d.Template).WithMany(p => p.TrnNotifications)
                .HasForeignKey(d => d.TemplateId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_notification_template");
        });

        modelBuilder.Entity<TrnOutsourceDispatch>(entity =>
        {
            entity.HasKey(e => e.DispatchId).HasName("trn_outsource_dispatch_pkey");

            entity.ToTable("trn_outsource_dispatch", "press_db");

            entity.HasIndex(e => e.DispatchDate, "idx_outsrc_disp_date");

            entity.HasIndex(e => e.OutsourceId, "idx_outsrc_disp_outsource");

            entity.Property(e => e.DispatchId).HasColumnName("dispatch_id");
            entity.Property(e => e.ChallanNo)
                .HasMaxLength(50)
                .HasColumnName("challan_no");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.DispatchDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("dispatch_date");
            entity.Property(e => e.OutsourceId).HasColumnName("outsource_id");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.TotalQuantity)
                .HasPrecision(14, 2)
                .HasColumnName("total_quantity");

            entity.HasOne(d => d.Outsource).WithMany(p => p.TrnOutsourceDispatches)
                .HasForeignKey(d => d.OutsourceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_dispatch_outsource");
        });

        modelBuilder.Entity<TrnOutsourceReceive>(entity =>
        {
            entity.HasKey(e => e.ReceiveId).HasName("trn_outsource_receive_pkey");

            entity.ToTable("trn_outsource_receive", "press_db");

            entity.HasIndex(e => e.ReceiveDate, "idx_outsrc_recv_date");

            entity.HasIndex(e => e.OutsourceId, "idx_outsrc_recv_outsource");

            entity.Property(e => e.ReceiveId).HasColumnName("receive_id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.OutsourceId).HasColumnName("outsource_id");
            entity.Property(e => e.ReceiveDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("receive_date");
            entity.Property(e => e.ReceivedQuantity)
                .HasPrecision(14, 2)
                .HasColumnName("received_quantity");
            entity.Property(e => e.RejectedQuantity)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("rejected_quantity");
            entity.Property(e => e.Remarks).HasColumnName("remarks");

            entity.HasOne(d => d.Outsource).WithMany(p => p.TrnOutsourceReceives)
                .HasForeignKey(d => d.OutsourceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_receive_outsource");
        });

        modelBuilder.Entity<TrnOutsourceTimeline>(entity =>
        {
            entity.HasKey(e => e.TimelineId).HasName("trn_outsource_timeline_pkey");

            entity.ToTable("trn_outsource_timeline", "press_db");

            entity.HasIndex(e => e.CreatedOn, "idx_outsource_timeline_created_on").IsDescending();

            entity.HasIndex(e => e.MovementType, "idx_outsource_timeline_movement");

            entity.HasIndex(e => e.OutsourceId, "idx_outsource_timeline_outsource_id");

            entity.HasIndex(e => e.NewStatus, "idx_outsource_timeline_status");

            entity.HasIndex(e => e.VendorId, "idx_outsource_timeline_vendor");

            entity.Property(e => e.TimelineId).HasColumnName("timeline_id");
            entity.Property(e => e.ActualReturnDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("actual_return_date");
            entity.Property(e => e.AssignedToUserId).HasColumnName("assigned_to_user_id");
            entity.Property(e => e.AttachmentUrl).HasColumnName("attachment_url");
            entity.Property(e => e.ChallanId).HasColumnName("challan_id");
            entity.Property(e => e.CommunicationMode)
                .HasMaxLength(50)
                .HasColumnName("communication_mode");
            entity.Property(e => e.CommunicationReference).HasColumnName("communication_reference");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.DelayReason).HasColumnName("delay_reason");
            entity.Property(e => e.EnquiryId).HasColumnName("enquiry_id");
            entity.Property(e => e.EventCode)
                .HasMaxLength(50)
                .HasColumnName("event_code");
            entity.Property(e => e.EventDescription).HasColumnName("event_description");
            entity.Property(e => e.EventTitle)
                .HasMaxLength(200)
                .HasColumnName("event_title");
            entity.Property(e => e.EventType)
                .HasMaxLength(50)
                .HasColumnName("event_type");
            entity.Property(e => e.ExpectedReturnDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("expected_return_date");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.MovementType)
                .HasMaxLength(30)
                .HasColumnName("movement_type");
            entity.Property(e => e.NewAmount)
                .HasPrecision(18, 2)
                .HasColumnName("new_amount");
            entity.Property(e => e.NewQuantity)
                .HasPrecision(18, 2)
                .HasColumnName("new_quantity");
            entity.Property(e => e.NewStatus)
                .HasMaxLength(50)
                .HasColumnName("new_status");
            entity.Property(e => e.OldAmount)
                .HasPrecision(18, 2)
                .HasColumnName("old_amount");
            entity.Property(e => e.OldQuantity)
                .HasPrecision(18, 2)
                .HasColumnName("old_quantity");
            entity.Property(e => e.OldStatus)
                .HasMaxLength(50)
                .HasColumnName("old_status");
            entity.Property(e => e.OutsourceId).HasColumnName("outsource_id");
            entity.Property(e => e.ProcessCode)
                .HasMaxLength(50)
                .HasColumnName("process_code");
            entity.Property(e => e.ProcessName)
                .HasMaxLength(100)
                .HasColumnName("process_name");
            entity.Property(e => e.QuotationId).HasColumnName("quotation_id");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
            entity.Property(e => e.UpdatedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_on");
            entity.Property(e => e.VendorId).HasColumnName("vendor_id");
            entity.Property(e => e.VendorName)
                .HasMaxLength(150)
                .HasColumnName("vendor_name");

            entity.HasOne(d => d.Challan).WithMany(p => p.TrnOutsourceTimelines)
                .HasForeignKey(d => d.ChallanId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_outsource_timeline_challan");

            entity.HasOne(d => d.Enquiry).WithMany(p => p.TrnOutsourceTimelines)
                .HasForeignKey(d => d.EnquiryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_outsource_timeline_enquiry");

            entity.HasOne(d => d.Job).WithMany(p => p.TrnOutsourceTimelines)
                .HasForeignKey(d => d.JobId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_outsource_timeline_job");

            entity.HasOne(d => d.Outsource).WithMany(p => p.TrnOutsourceTimelines)
                .HasForeignKey(d => d.OutsourceId)
                .HasConstraintName("fk_outsource_timeline_outsource");

            entity.HasOne(d => d.Quotation).WithMany(p => p.TrnOutsourceTimelines)
                .HasForeignKey(d => d.QuotationId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_outsource_timeline_quotation");
        });

        modelBuilder.Entity<TrnPayment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("trn_payment_pkey");

            entity.ToTable("trn_payment", "press_db");

            entity.HasIndex(e => e.CompanyId, "idx_payment_company");

            entity.HasIndex(e => e.PaymentDate, "idx_payment_date");

            entity.HasIndex(e => e.PaymentMode, "idx_payment_mode");

            entity.HasIndex(e => e.PartyId, "idx_payment_party");

            entity.HasIndex(e => e.Status, "idx_payment_status");

            entity.HasIndex(e => e.PaymentNo, "trn_payment_payment_no_key").IsUnique();

            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.Amount)
                .HasPrecision(18, 2)
                .HasColumnName("amount");
            entity.Property(e => e.BankId).HasColumnName("bank_id");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(50)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.PartyId).HasColumnName("party_id");
            entity.Property(e => e.PaymentDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("payment_date");
            entity.Property(e => e.PaymentMode)
                .HasMaxLength(30)
                .HasColumnName("payment_mode");
            entity.Property(e => e.PaymentNo)
                .HasMaxLength(30)
                .HasColumnName("payment_no");
            entity.Property(e => e.ReferenceDate).HasColumnName("reference_date");
            entity.Property(e => e.ReferenceNo)
                .HasMaxLength(100)
                .HasColumnName("reference_no");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'POSTED'::character varying")
                .HasColumnName("status");

            entity.HasOne(d => d.Bank).WithMany(p => p.TrnPayments)
                .HasForeignKey(d => d.BankId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_payment_bank");

            entity.HasOne(d => d.Company).WithMany(p => p.TrnPayments)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_payment_company");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.TrnPayments)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_payment_created_by");

            entity.HasOne(d => d.Party).WithMany(p => p.TrnPayments)
                .HasForeignKey(d => d.PartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_payment_party");
        });

        modelBuilder.Entity<TrnPaymentAllocation>(entity =>
        {
            entity.HasKey(e => e.PaymentAllocationId).HasName("trn_payment_allocation_pkey");

            entity.ToTable("trn_payment_allocation", "press_db");

            entity.HasIndex(e => e.RefId, "idx_payment_alloc_invoice");

            entity.HasIndex(e => e.PaymentId, "idx_payment_alloc_payment");

            entity.HasIndex(e => e.RefId, "idx_payment_alloc_ref");

            entity.Property(e => e.PaymentAllocationId).HasColumnName("payment_allocation_id");
            entity.Property(e => e.AllocatedAmount)
                .HasPrecision(18, 2)
                .HasColumnName("allocated_amount");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.PaymentAgainst)
                .HasMaxLength(30)
                .HasColumnName("payment_against");
            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.RefDate)
                .HasMaxLength(30)
                .HasColumnName("ref_date");
            entity.Property(e => e.RefId).HasColumnName("ref_id");
            entity.Property(e => e.RefNo)
                .HasMaxLength(30)
                .HasColumnName("ref_no");

            entity.HasOne(d => d.Payment).WithMany(p => p.TrnPaymentAllocations)
                .HasForeignKey(d => d.PaymentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_payment_alloc_payment");
        });

        modelBuilder.Entity<TrnPrintWorkEntry>(entity =>
        {
            entity.HasKey(e => e.PrintWorkId).HasName("pk_trn_print_work_entry");

            entity.ToTable("trn_print_work_entry", "press_db", tb => tb.HasComment("Per-part printing process inputs captured in Workspace PrintWork page. One row per product part per task."));

            entity.HasIndex(e => e.JobId, "idx_print_work_job_id");

            entity.HasIndex(e => e.WorkspaceTaskId, "idx_print_work_task_id");

            entity.Property(e => e.PrintWorkId).HasColumnName("print_work_id");
            entity.Property(e => e.CompletedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("completed_on");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.IsSelected)
                .HasDefaultValue(false)
                .HasColumnName("is_selected");
            entity.Property(e => e.IsStarted)
                .HasDefaultValue(false)
                .HasColumnName("is_started");
            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.MachineId).HasColumnName("machine_id");
            entity.Property(e => e.MachineName)
                .HasMaxLength(150)
                .HasColumnName("machine_name");
            entity.Property(e => e.ModifiedBy).HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.NumberOfColors)
                .HasDefaultValue(0)
                .HasColumnName("number_of_colors");
            entity.Property(e => e.NumberOfPlates)
                .HasDefaultValue(0)
                .HasColumnName("number_of_plates");
            entity.Property(e => e.PartName)
                .HasMaxLength(150)
                .HasColumnName("part_name");
            entity.Property(e => e.PartSequence)
                .HasDefaultValue(0)
                .HasColumnName("part_sequence");
            entity.Property(e => e.PrintingMethod)
                .HasMaxLength(30)
                .HasComment("Printing method selected for this part: OFFSET, DIGITAL, or SCREEN")
                .HasColumnName("printing_method");
            entity.Property(e => e.StartedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("started_on");
            entity.Property(e => e.TotalSheetsPrinted)
                .HasDefaultValue(0)
                .HasComment("Actual sheets printed so far — updated during execution")
                .HasColumnName("total_sheets_printed");
            entity.Property(e => e.TotalSheetsRequired)
                .HasDefaultValue(0)
                .HasComment("Total sheets required for this part (pre-filled from job specs)")
                .HasColumnName("total_sheets_required");
            entity.Property(e => e.WorkspaceTaskId).HasColumnName("workspace_task_id");

            entity.HasOne(d => d.WorkspaceTask).WithMany(p => p.TrnPrintWorkEntries)
                .HasForeignKey(d => d.WorkspaceTaskId)
                .HasConstraintName("fk_print_work_task");
        });

        modelBuilder.Entity<TrnProformaInvoice>(entity =>
        {
            entity.HasKey(e => e.ProformaInvoiceId).HasName("trn_proforma_invoice_pkey");

            entity.ToTable("trn_proforma_invoice", "press_db", tb => tb.HasComment("Proforma invoice issued to customer before delivery/final sales invoice. Can be converted to sales invoice. Does NOT post to GL."));

            entity.HasIndex(e => e.ProformaDate, "idx_prof_date");

            entity.HasIndex(e => e.PartyId, "idx_prof_party");

            entity.HasIndex(e => e.Status, "idx_prof_status");

            entity.HasIndex(e => e.ProformaNo, "uq_proforma_no").IsUnique();

            entity.Property(e => e.ProformaInvoiceId).HasColumnName("proforma_invoice_id");
            entity.Property(e => e.AttachmentsJson)
                .HasDefaultValueSql("'[]'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("attachments_json");
            entity.Property(e => e.BillingAddressId).HasColumnName("billing_address_id");
            entity.Property(e => e.CessAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("cess_amount");
            entity.Property(e => e.CgstAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("cgst_amount");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.ConvertedToInvoice)
                .HasDefaultValue(false)
                .HasColumnName("converted_to_invoice");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.DiscountAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("discount_amount");
            entity.Property(e => e.ExchangeRate)
                .HasPrecision(10, 4)
                .HasDefaultValueSql("1")
                .HasColumnName("exchange_rate");
            entity.Property(e => e.FinYearId).HasColumnName("fin_year_id");
            entity.Property(e => e.GrandTotal)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("grand_total");
            entity.Property(e => e.IgstAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("igst_amount");
            entity.Property(e => e.InternalNotes).HasColumnName("internal_notes");
            entity.Property(e => e.IsExport)
                .HasDefaultValue(false)
                .HasColumnName("is_export");
            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.PartyId).HasColumnName("party_id");
            entity.Property(e => e.PaymentTermId).HasColumnName("payment_term_id");
            entity.Property(e => e.PlaceOfSupply)
                .HasMaxLength(150)
                .HasColumnName("place_of_supply");
            entity.Property(e => e.PoDate).HasColumnName("po_date");
            entity.Property(e => e.PoNo)
                .HasMaxLength(100)
                .HasColumnName("po_no");
            entity.Property(e => e.ProformaDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("proforma_date");
            entity.Property(e => e.ProformaNo)
                .HasMaxLength(50)
                .HasColumnName("proforma_no");
            entity.Property(e => e.QuotationId).HasColumnName("quotation_id");
            entity.Property(e => e.RoundOff)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("round_off");
            entity.Property(e => e.SalesInvoiceId).HasColumnName("sales_invoice_id");
            entity.Property(e => e.SalesPerson)
                .HasMaxLength(200)
                .HasColumnName("sales_person");
            entity.Property(e => e.SgstAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("sgst_amount");
            entity.Property(e => e.ShippingAddressId).HasColumnName("shipping_address_id");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'DRAFT'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.SubtotalAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("subtotal_amount");
            entity.Property(e => e.TaxableAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("taxable_amount");
            entity.Property(e => e.TermsConditions).HasColumnName("terms_conditions");
            entity.Property(e => e.TotalTaxAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("total_tax_amount");
            entity.Property(e => e.ValidTill).HasColumnName("valid_till");

            entity.HasOne(d => d.Company).WithMany(p => p.TrnProformaInvoices)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_prof_company");

            entity.HasOne(d => d.Party).WithMany(p => p.TrnProformaInvoices)
                .HasForeignKey(d => d.PartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_prof_party");

            entity.HasOne(d => d.SalesInvoice).WithMany(p => p.TrnProformaInvoices)
                .HasForeignKey(d => d.SalesInvoiceId)
                .HasConstraintName("fk_prof_sales_invoice");
        });

        modelBuilder.Entity<TrnProformaInvoiceItem>(entity =>
        {
            entity.HasKey(e => e.ProformaItemId).HasName("trn_proforma_invoice_item_pkey");

            entity.ToTable("trn_proforma_invoice_item", "press_db", tb => tb.HasComment("Proforma invoice line items with full GST breakup."));

            entity.HasIndex(e => e.ProformaInvoiceId, "idx_prof_item_header");

            entity.Property(e => e.ProformaItemId).HasColumnName("proforma_item_id");
            entity.Property(e => e.AccountHeadId).HasColumnName("account_head_id");
            entity.Property(e => e.CessAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("cess_amount");
            entity.Property(e => e.CessPercent)
                .HasPrecision(6, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("cess_percent");
            entity.Property(e => e.CgstAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("cgst_amount");
            entity.Property(e => e.CgstPercent)
                .HasPrecision(6, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("cgst_percent");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DiscountAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("discount_amount");
            entity.Property(e => e.DiscountPercent)
                .HasPrecision(6, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("discount_percent");
            entity.Property(e => e.HsnSacCode)
                .HasMaxLength(20)
                .HasColumnName("hsn_sac_code");
            entity.Property(e => e.IgstAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("igst_amount");
            entity.Property(e => e.IgstPercent)
                .HasPrecision(6, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("igst_percent");
            entity.Property(e => e.ItemId).HasColumnName("item_id");
            entity.Property(e => e.ItemSequence)
                .HasDefaultValue(1)
                .HasColumnName("item_sequence");
            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.LineTotal)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("line_total");
            entity.Property(e => e.ProformaInvoiceId).HasColumnName("proforma_invoice_id");
            entity.Property(e => e.Quantity)
                .HasPrecision(14, 4)
                .HasDefaultValueSql("0")
                .HasColumnName("quantity");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.SgstAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("sgst_amount");
            entity.Property(e => e.SgstPercent)
                .HasPrecision(6, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("sgst_percent");
            entity.Property(e => e.TaxCategoryId).HasColumnName("tax_category_id");
            entity.Property(e => e.TaxableValue)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("taxable_value");
            entity.Property(e => e.TotalTaxAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("total_tax_amount");
            entity.Property(e => e.UnitRate)
                .HasPrecision(14, 4)
                .HasDefaultValueSql("0")
                .HasColumnName("unit_rate");
            entity.Property(e => e.UomId).HasColumnName("uom_id");

            entity.HasOne(d => d.ProformaInvoice).WithMany(p => p.TrnProformaInvoiceItems)
                .HasForeignKey(d => d.ProformaInvoiceId)
                .HasConstraintName("fk_prof_item_header");
        });

        modelBuilder.Entity<TrnPurchaseGrn>(entity =>
        {
            entity.HasKey(e => e.GrnId).HasName("trn_purchase_grn_pkey");

            entity.ToTable("trn_purchase_grn", "press_db");

            entity.HasIndex(e => e.GrnDate, "idx_trn_purchase_grn_date");

            entity.HasIndex(e => e.JobId, "idx_trn_purchase_grn_job");

            entity.HasIndex(e => e.Status, "idx_trn_purchase_grn_status");

            entity.HasIndex(e => e.SupplierId, "idx_trn_purchase_grn_supplier");

            entity.HasIndex(e => e.GrnType, "idx_trn_purchase_grn_type");

            entity.HasIndex(e => e.GrnNo, "trn_purchase_grn_grn_no_key").IsUnique();

            entity.Property(e => e.GrnId).HasColumnName("grn_id");
            entity.Property(e => e.ApprovedBy).HasColumnName("approved_by");
            entity.Property(e => e.ApprovedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("approved_on");
            entity.Property(e => e.CompanyId)
                .HasDefaultValue(1)
                .HasColumnName("company_id");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.GrnDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("grn_date");
            entity.Property(e => e.GrnNo)
                .HasMaxLength(30)
                .HasColumnName("grn_no");
            entity.Property(e => e.GrnType)
                .HasMaxLength(30)
                .HasDefaultValueSql("'JOB'::character varying")
                .HasColumnName("grn_type");
            entity.Property(e => e.InvoiceDate).HasColumnName("invoice_date");
            entity.Property(e => e.InvoiceNo)
                .HasMaxLength(50)
                .HasColumnName("invoice_no");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.JobNo)
                .HasMaxLength(30)
                .HasColumnName("job_no");
            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(50)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.NetAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("net_amount");
            entity.Property(e => e.PurchaseOrderId).HasColumnName("purchase_order_id");
            entity.Property(e => e.PurchaseOrderNo)
                .HasMaxLength(30)
                .HasColumnName("purchase_order_no");
            entity.Property(e => e.QualityStatus)
                .HasMaxLength(20)
                .HasDefaultValueSql("'PENDING'::character varying")
                .HasColumnName("quality_status");
            entity.Property(e => e.RateCalcId).HasColumnName("rate_calc_id");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'DRAFT'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            entity.Property(e => e.SupplierName)
                .HasMaxLength(200)
                .HasColumnName("supplier_name");
            entity.Property(e => e.TaxAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("tax_amount");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("total_amount");
            entity.Property(e => e.TotalItems)
                .HasDefaultValue(0)
                .HasColumnName("total_items");
        });

        modelBuilder.Entity<TrnPurchaseGrnItem>(entity =>
        {
            entity.HasKey(e => e.GrnItemId).HasName("trn_purchase_grn_item_pkey");

            entity.ToTable("trn_purchase_grn_item", "press_db");

            entity.HasIndex(e => e.MaterialCategory, "idx_trn_purchase_grn_item_cat");

            entity.HasIndex(e => e.GrnId, "idx_trn_purchase_grn_item_grn");

            entity.Property(e => e.GrnItemId).HasColumnName("grn_item_id");
            entity.Property(e => e.AcceptedQuantity)
                .HasPrecision(14, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("accepted_quantity");
            entity.Property(e => e.Amount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("amount");
            entity.Property(e => e.AvailableStock)
                .HasPrecision(14, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("available_stock");
            entity.Property(e => e.BatchNo)
                .HasMaxLength(50)
                .HasColumnName("batch_no");
            entity.Property(e => e.BomQuantity)
                .HasPrecision(14, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("bom_quantity");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.ExpiryDate).HasColumnName("expiry_date");
            entity.Property(e => e.ForPart)
                .HasMaxLength(50)
                .HasColumnName("for_part");
            entity.Property(e => e.GrnId).HasColumnName("grn_id");
            entity.Property(e => e.IsSelected)
                .HasDefaultValue(true)
                .HasColumnName("is_selected");
            entity.Property(e => e.ItemSequence)
                .HasDefaultValue(1)
                .HasColumnName("item_sequence");
            entity.Property(e => e.MaterialCategory)
                .HasMaxLength(30)
                .HasColumnName("material_category");
            entity.Property(e => e.MaterialCode)
                .HasMaxLength(50)
                .HasColumnName("material_code");
            entity.Property(e => e.MaterialId).HasColumnName("material_id");
            entity.Property(e => e.MaterialName)
                .HasMaxLength(200)
                .HasColumnName("material_name");
            entity.Property(e => e.NetAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("net_amount");
            entity.Property(e => e.OrderedQuantity)
                .HasPrecision(14, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("ordered_quantity");
            entity.Property(e => e.QualityStatus)
                .HasMaxLength(20)
                .HasDefaultValueSql("'PENDING'::character varying")
                .HasColumnName("quality_status");
            entity.Property(e => e.Rate)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("rate");
            entity.Property(e => e.ReceivedQuantity)
                .HasPrecision(14, 3)
                .HasColumnName("received_quantity");
            entity.Property(e => e.RejectedQuantity)
                .HasPrecision(14, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("rejected_quantity");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.Specification).HasColumnName("specification");
            entity.Property(e => e.TaxAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("tax_amount");
            entity.Property(e => e.TaxRate)
                .HasPrecision(5, 2)
                .HasDefaultValueSql("18.00")
                .HasColumnName("tax_rate");
            entity.Property(e => e.Uom)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Pcs'::character varying")
                .HasColumnName("uom");

            entity.HasOne(d => d.Grn).WithMany(p => p.TrnPurchaseGrnItems)
                .HasForeignKey(d => d.GrnId)
                .HasConstraintName("trn_purchase_grn_item_grn_id_fkey");
        });

        modelBuilder.Entity<TrnPurchaseInvoice>(entity =>
        {
            entity.HasKey(e => e.PurchaseInvoiceId).HasName("trn_purchase_invoice_pkey");

            entity.ToTable("trn_purchase_invoice", "press_db", tb => tb.HasComment("Purchase invoice header for goods/services received from suppliers. Supports GST (CGST/SGST/IGST), reverse charge, TDS, import purchases."));

            entity.HasIndex(e => e.CompanyId, "idx_pi_company");

            entity.HasIndex(e => e.InvoiceDate, "idx_pi_date");

            entity.HasIndex(e => e.PartyId, "idx_pi_party");

            entity.HasIndex(e => e.Status, "idx_pi_status");

            entity.HasIndex(e => e.InvoiceNo, "uq_purchase_invoice_no").IsUnique();

            entity.Property(e => e.PurchaseInvoiceId).HasColumnName("purchase_invoice_id");
            entity.Property(e => e.AttachmentsJson)
                .HasDefaultValueSql("'[]'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("attachments_json");
            entity.Property(e => e.BalanceAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("balance_amount");
            entity.Property(e => e.BillingAddressId).HasColumnName("billing_address_id");
            entity.Property(e => e.CancelReason).HasColumnName("cancel_reason");
            entity.Property(e => e.CancelledBy).HasColumnName("cancelled_by");
            entity.Property(e => e.CancelledOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("cancelled_on");
            entity.Property(e => e.CessAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("cess_amount");
            entity.Property(e => e.CgstAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("cgst_amount");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.DiscountAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("discount_amount");
            entity.Property(e => e.DueDate).HasColumnName("due_date");
            entity.Property(e => e.ExchangeRate)
                .HasPrecision(10, 4)
                .HasDefaultValueSql("1")
                .HasColumnName("exchange_rate");
            entity.Property(e => e.FinYearId).HasColumnName("fin_year_id");
            entity.Property(e => e.GlPostedBy).HasColumnName("gl_posted_by");
            entity.Property(e => e.GlPostedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("gl_posted_on");
            entity.Property(e => e.GrandTotal)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("grand_total");
            entity.Property(e => e.GrnDate).HasColumnName("grn_date");
            entity.Property(e => e.GrnNo)
                .HasMaxLength(100)
                .HasColumnName("grn_no");
            entity.Property(e => e.IgstAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("igst_amount");
            entity.Property(e => e.InternalNotes).HasColumnName("internal_notes");
            entity.Property(e => e.InvoiceDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("invoice_date");
            entity.Property(e => e.InvoiceNo)
                .HasMaxLength(50)
                .HasColumnName("invoice_no");
            entity.Property(e => e.IsCancelled)
                .HasDefaultValue(false)
                .HasColumnName("is_cancelled");
            entity.Property(e => e.IsImport)
                .HasDefaultValue(false)
                .HasColumnName("is_import");
            entity.Property(e => e.IsPostedToGl)
                .HasDefaultValue(false)
                .HasColumnName("is_posted_to_gl");
            entity.Property(e => e.IsReverseCharge)
                .HasDefaultValue(false)
                .HasColumnName("is_reverse_charge");
            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.PaidAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("paid_amount");
            entity.Property(e => e.PartyId).HasColumnName("party_id");
            entity.Property(e => e.PaymentTermId).HasColumnName("payment_term_id");
            entity.Property(e => e.PlaceOfSupply)
                .HasMaxLength(150)
                .HasColumnName("place_of_supply");
            entity.Property(e => e.PoDate).HasColumnName("po_date");
            entity.Property(e => e.PoNo)
                .HasMaxLength(100)
                .HasColumnName("po_no");
            entity.Property(e => e.RoundOff)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("round_off");
            entity.Property(e => e.SgstAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("sgst_amount");
            entity.Property(e => e.ShippingAddressId).HasColumnName("shipping_address_id");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'DRAFT'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.SubtotalAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("subtotal_amount");
            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            entity.Property(e => e.SupplierInvoiceDate).HasColumnName("supplier_invoice_date");
            entity.Property(e => e.SupplierInvoiceNo)
                .HasMaxLength(100)
                .HasColumnName("supplier_invoice_no");
            entity.Property(e => e.TaxableAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("taxable_amount");
            entity.Property(e => e.TdsAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("tds_amount");
            entity.Property(e => e.TotalTaxAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("total_tax_amount");

            entity.HasOne(d => d.Company).WithMany(p => p.TrnPurchaseInvoices)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_pi_company");

            entity.HasOne(d => d.FinYear).WithMany(p => p.TrnPurchaseInvoices)
                .HasForeignKey(d => d.FinYearId)
                .HasConstraintName("fk_pi_fin_year");

            entity.HasOne(d => d.Party).WithMany(p => p.TrnPurchaseInvoices)
                .HasForeignKey(d => d.PartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_pi_party");

            entity.HasOne(d => d.PaymentTerm).WithMany(p => p.TrnPurchaseInvoices)
                .HasForeignKey(d => d.PaymentTermId)
                .HasConstraintName("fk_pi_payment_term");
        });

        modelBuilder.Entity<TrnPurchaseInvoiceItem>(entity =>
        {
            entity.HasKey(e => e.PurchaseItemId).HasName("trn_purchase_invoice_item_pkey");

            entity.ToTable("trn_purchase_invoice_item", "press_db", tb => tb.HasComment("Purchase invoice line items with full GST breakup (CGST/SGST/IGST/CESS) per item."));

            entity.HasIndex(e => e.PurchaseInvoiceId, "idx_pi_item_header");

            entity.Property(e => e.PurchaseItemId).HasColumnName("purchase_item_id");
            entity.Property(e => e.AccountHeadId).HasColumnName("account_head_id");
            entity.Property(e => e.CessAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("cess_amount");
            entity.Property(e => e.CessPercent)
                .HasPrecision(6, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("cess_percent");
            entity.Property(e => e.CgstAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("cgst_amount");
            entity.Property(e => e.CgstPercent)
                .HasPrecision(6, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("cgst_percent");
            entity.Property(e => e.CostCenterId).HasColumnName("cost_center_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DiscountAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("discount_amount");
            entity.Property(e => e.DiscountPercent)
                .HasPrecision(6, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("discount_percent");
            entity.Property(e => e.HsnSacCode)
                .HasMaxLength(20)
                .HasColumnName("hsn_sac_code");
            entity.Property(e => e.IgstAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("igst_amount");
            entity.Property(e => e.IgstPercent)
                .HasPrecision(6, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("igst_percent");
            entity.Property(e => e.ItemId).HasColumnName("item_id");
            entity.Property(e => e.ItemSequence)
                .HasDefaultValue(1)
                .HasColumnName("item_sequence");
            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.LineTotal)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("line_total");
            entity.Property(e => e.PurchaseInvoiceId).HasColumnName("purchase_invoice_id");
            entity.Property(e => e.Quantity)
                .HasPrecision(14, 4)
                .HasDefaultValueSql("0")
                .HasColumnName("quantity");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.SgstAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("sgst_amount");
            entity.Property(e => e.SgstPercent)
                .HasPrecision(6, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("sgst_percent");
            entity.Property(e => e.TaxCategoryId).HasColumnName("tax_category_id");
            entity.Property(e => e.TaxableValue)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("taxable_value");
            entity.Property(e => e.TotalTaxAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("total_tax_amount");
            entity.Property(e => e.UnitRate)
                .HasPrecision(14, 4)
                .HasDefaultValueSql("0")
                .HasColumnName("unit_rate");
            entity.Property(e => e.UomId).HasColumnName("uom_id");

            entity.HasOne(d => d.AccountHead).WithMany(p => p.TrnPurchaseInvoiceItems)
                .HasForeignKey(d => d.AccountHeadId)
                .HasConstraintName("fk_pi_item_account");

            entity.HasOne(d => d.PurchaseInvoice).WithMany(p => p.TrnPurchaseInvoiceItems)
                .HasForeignKey(d => d.PurchaseInvoiceId)
                .HasConstraintName("fk_pi_item_header");
        });

        modelBuilder.Entity<TrnPurchaseOrder>(entity =>
        {
            entity.HasKey(e => e.PurchaseOrderId).HasName("trn_purchase_order_pkey");

            entity.ToTable("trn_purchase_order", "press_db", tb => tb.HasComment("Purchase order header. Part of AP flow: PO → GRN → Purchase Invoice → Payment. Supports GST and approval workflow."));

            entity.HasIndex(e => e.PoDate, "idx_po_date");

            entity.HasIndex(e => e.PartyId, "idx_po_party");

            entity.HasIndex(e => e.Status, "idx_po_status");

            entity.HasIndex(e => e.PoNo, "uq_po_no").IsUnique();

            entity.Property(e => e.PurchaseOrderId).HasColumnName("purchase_order_id");
            entity.Property(e => e.ApprovedBy).HasColumnName("approved_by");
            entity.Property(e => e.ApprovedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("approved_on");
            entity.Property(e => e.AttachmentsJson)
                .HasDefaultValueSql("'[]'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("attachments_json");
            entity.Property(e => e.BillingAddressId).HasColumnName("billing_address_id");
            entity.Property(e => e.CancelReason).HasColumnName("cancel_reason");
            entity.Property(e => e.CancelledBy).HasColumnName("cancelled_by");
            entity.Property(e => e.CancelledOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("cancelled_on");
            entity.Property(e => e.CessAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("cess_amount");
            entity.Property(e => e.CgstAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("cgst_amount");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.DiscountAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("discount_amount");
            entity.Property(e => e.ExchangeRate)
                .HasPrecision(10, 4)
                .HasDefaultValueSql("1")
                .HasColumnName("exchange_rate");
            entity.Property(e => e.ExpectedDeliveryDate).HasColumnName("expected_delivery_date");
            entity.Property(e => e.FinYearId).HasColumnName("fin_year_id");
            entity.Property(e => e.GrandTotal)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("grand_total");
            entity.Property(e => e.IgstAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("igst_amount");
            entity.Property(e => e.InternalNotes).HasColumnName("internal_notes");
            entity.Property(e => e.IsApproved)
                .HasDefaultValue(false)
                .HasColumnName("is_approved");
            entity.Property(e => e.IsCancelled)
                .HasDefaultValue(false)
                .HasColumnName("is_cancelled");
            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.PartyId).HasColumnName("party_id");
            entity.Property(e => e.PaymentTermId).HasColumnName("payment_term_id");
            entity.Property(e => e.PoDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("po_date");
            entity.Property(e => e.PoNo)
                .HasMaxLength(50)
                .HasColumnName("po_no");
            entity.Property(e => e.RoundOff)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("round_off");
            entity.Property(e => e.SgstAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("sgst_amount");
            entity.Property(e => e.ShippingAddressId).HasColumnName("shipping_address_id");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'DRAFT'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.SubtotalAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("subtotal_amount");
            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            entity.Property(e => e.TaxableAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("taxable_amount");
            entity.Property(e => e.TermsConditions).HasColumnName("terms_conditions");
            entity.Property(e => e.TotalTaxAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("total_tax_amount");

            entity.HasOne(d => d.Company).WithMany(p => p.TrnPurchaseOrders)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_po_company");

            entity.HasOne(d => d.Party).WithMany(p => p.TrnPurchaseOrders)
                .HasForeignKey(d => d.PartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_po_party");
        });

        modelBuilder.Entity<TrnPurchaseOrderItem>(entity =>
        {
            entity.HasKey(e => e.PoItemId).HasName("trn_purchase_order_item_pkey");

            entity.ToTable("trn_purchase_order_item", "press_db", tb => tb.HasComment("Purchase order line items with GST breakup. Tracks received vs pending quantities."));

            entity.HasIndex(e => e.PurchaseOrderId, "idx_poi_header");

            entity.Property(e => e.PoItemId).HasColumnName("po_item_id");
            entity.Property(e => e.CessAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("cess_amount");
            entity.Property(e => e.CessPercent)
                .HasPrecision(6, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("cess_percent");
            entity.Property(e => e.CgstAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("cgst_amount");
            entity.Property(e => e.CgstPercent)
                .HasPrecision(6, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("cgst_percent");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DiscountAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("discount_amount");
            entity.Property(e => e.DiscountPercent)
                .HasPrecision(6, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("discount_percent");
            entity.Property(e => e.HsnSacCode)
                .HasMaxLength(20)
                .HasColumnName("hsn_sac_code");
            entity.Property(e => e.IgstAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("igst_amount");
            entity.Property(e => e.IgstPercent)
                .HasPrecision(6, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("igst_percent");
            entity.Property(e => e.ItemId).HasColumnName("item_id");
            entity.Property(e => e.ItemSequence)
                .HasDefaultValue(1)
                .HasColumnName("item_sequence");
            entity.Property(e => e.LineTotal)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("line_total");
            entity.Property(e => e.PendingQuantity)
                .HasPrecision(14, 4)
                .HasDefaultValueSql("0")
                .HasColumnName("pending_quantity");
            entity.Property(e => e.PurchaseOrderId).HasColumnName("purchase_order_id");
            entity.Property(e => e.Quantity)
                .HasPrecision(14, 4)
                .HasDefaultValueSql("0")
                .HasColumnName("quantity");
            entity.Property(e => e.ReceivedQuantity)
                .HasPrecision(14, 4)
                .HasDefaultValueSql("0")
                .HasColumnName("received_quantity");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.SgstAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("sgst_amount");
            entity.Property(e => e.SgstPercent)
                .HasPrecision(6, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("sgst_percent");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'OPEN'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.TaxCategoryId).HasColumnName("tax_category_id");
            entity.Property(e => e.TaxableValue)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("taxable_value");
            entity.Property(e => e.TotalTaxAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("total_tax_amount");
            entity.Property(e => e.UnitRate)
                .HasPrecision(14, 4)
                .HasDefaultValueSql("0")
                .HasColumnName("unit_rate");
            entity.Property(e => e.UomId).HasColumnName("uom_id");

            entity.HasOne(d => d.PurchaseOrder).WithMany(p => p.TrnPurchaseOrderItems)
                .HasForeignKey(d => d.PurchaseOrderId)
                .HasConstraintName("fk_poi_header");
        });

        modelBuilder.Entity<TrnQuotation>(entity =>
        {
            entity.HasKey(e => e.QuotationId).HasName("trn_quotation_pkey");

            entity.ToTable("trn_quotation", "press_db");

            entity.HasIndex(e => e.CompanyId, "idx_quotation_company");

            entity.HasIndex(e => e.QuotationDate, "idx_quotation_date");

            entity.HasIndex(e => e.EnquiryId, "idx_quotation_enquiry");

            entity.HasIndex(e => e.PartyId, "idx_quotation_party");

            entity.HasIndex(e => e.Status, "idx_quotation_status");

            entity.HasIndex(e => e.QuotationNo, "trn_quotation_quotation_no_key").IsUnique();

            entity.Property(e => e.QuotationId).HasColumnName("quotation_id");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.DiscountAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("discount_amount");
            entity.Property(e => e.EnquiryId).HasColumnName("enquiry_id");
            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(50)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.NetAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("net_amount");
            entity.Property(e => e.PartyId).HasColumnName("party_id");
            entity.Property(e => e.PartyRefDate).HasColumnName("party_ref_date");
            entity.Property(e => e.PartyRefNo)
                .HasMaxLength(30)
                .HasColumnName("party_ref_no");
            entity.Property(e => e.QuotationDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("quotation_date");
            entity.Property(e => e.QuotationNo)
                .HasMaxLength(30)
                .HasColumnName("quotation_no");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'DRAFT'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.TaxAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("tax_amount");
            entity.Property(e => e.TaxableAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("taxable_amount");
            entity.Property(e => e.TermsConditions).HasColumnName("terms_conditions");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("total_amount");
            entity.Property(e => e.ValidTill).HasColumnName("valid_till");

            entity.HasOne(d => d.Company).WithMany(p => p.TrnQuotations)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_quotation_company");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.TrnQuotations)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_quotation_created_by");

            entity.HasOne(d => d.Enquiry).WithMany(p => p.TrnQuotations)
                .HasForeignKey(d => d.EnquiryId)
                .HasConstraintName("fk_quotation_enquiry");

            entity.HasOne(d => d.Party).WithMany(p => p.TrnQuotations)
                .HasForeignKey(d => d.PartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_quotation_party");
        });

        modelBuilder.Entity<TrnQuotationItem>(entity =>
        {
            entity.HasKey(e => e.QuotationItemId).HasName("trn_quotation_item_pkey");

            entity.ToTable("trn_quotation_item", "press_db");

            entity.HasIndex(e => e.JobTypeId, "idx_qitem_job_type");

            entity.HasIndex(e => e.PrintProductTypeId, "idx_qitem_product_type");

            entity.HasIndex(e => e.UomId, "idx_qitem_uom");

            entity.HasIndex(e => e.EnquiryItemId, "idx_quot_item_enq_item");

            entity.HasIndex(e => e.QuotationId, "idx_quot_item_quotation");

            entity.HasIndex(e => e.RateCalculatorId, "idx_quot_item_rate_calc");

            entity.Property(e => e.QuotationItemId).HasColumnName("quotation_item_id");
            entity.Property(e => e.CalcRefNo)
                .HasMaxLength(50)
                .HasColumnName("calc_ref_no");
            entity.Property(e => e.CgstAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("cgst_amount");
            entity.Property(e => e.CgstPercent)
                .HasPrecision(6, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("cgst_percent");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.DiscountAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("discount_amount");
            entity.Property(e => e.DiscountPercent)
                .HasPrecision(6, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("discount_percent");
            entity.Property(e => e.EnquiryItemId).HasColumnName("enquiry_item_id");
            entity.Property(e => e.GrossAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("gross_amount");
            entity.Property(e => e.IgstAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("igst_amount");
            entity.Property(e => e.IgstPercent)
                .HasPrecision(6, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("igst_percent");
            entity.Property(e => e.ItemSequence)
                .HasDefaultValue(1)
                .HasColumnName("item_sequence");
            entity.Property(e => e.JobTypeId).HasColumnName("job_type_id");
            entity.Property(e => e.JobTypeName)
                .HasMaxLength(100)
                .HasColumnName("job_type_name");
            entity.Property(e => e.NetAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("net_amount");
            entity.Property(e => e.NoOfPages).HasColumnName("no_of_pages");
            entity.Property(e => e.PrintProductTypeId).HasColumnName("print_product_type_id");
            entity.Property(e => e.PrintingMethod)
                .HasMaxLength(30)
                .HasColumnName("printing_method");
            entity.Property(e => e.ProductDescription).HasColumnName("product_description");
            entity.Property(e => e.ProductName)
                .HasMaxLength(150)
                .HasColumnName("product_name");
            entity.Property(e => e.ProductSizeName)
                .HasMaxLength(100)
                .HasColumnName("product_size_name");
            entity.Property(e => e.ProductTypeName)
                .HasMaxLength(100)
                .HasColumnName("product_type_name");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.QuotationId).HasColumnName("quotation_id");
            entity.Property(e => e.RateCalculatorId).HasColumnName("rate_calculator_id");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.SgstAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("sgst_amount");
            entity.Property(e => e.SgstPercent)
                .HasPrecision(6, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("sgst_percent");
            entity.Property(e => e.TaxableValue)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("taxable_value");
            entity.Property(e => e.TotalTaxAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("total_tax_amount");
            entity.Property(e => e.TrimHeightMm)
                .HasPrecision(8, 2)
                .HasColumnName("trim_height_mm");
            entity.Property(e => e.TrimWidthMm)
                .HasPrecision(8, 2)
                .HasColumnName("trim_width_mm");
            entity.Property(e => e.UnitRate)
                .HasPrecision(14, 4)
                .HasDefaultValueSql("0")
                .HasColumnName("unit_rate");
            entity.Property(e => e.UomId).HasColumnName("uom_id");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.TrnQuotationItems)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_quotation_item_created_by");

            entity.HasOne(d => d.EnquiryItem).WithMany(p => p.TrnQuotationItems)
                .HasForeignKey(d => d.EnquiryItemId)
                .HasConstraintName("fk_quotation_item_enquiry_item");

            entity.HasOne(d => d.JobType).WithMany(p => p.TrnQuotationItems)
                .HasForeignKey(d => d.JobTypeId)
                .HasConstraintName("fk_qitem_job_type");

            entity.HasOne(d => d.PrintProductType).WithMany(p => p.TrnQuotationItems)
                .HasForeignKey(d => d.PrintProductTypeId)
                .HasConstraintName("fk_qitem_product_type");

            entity.HasOne(d => d.Quotation).WithMany(p => p.TrnQuotationItems)
                .HasForeignKey(d => d.QuotationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_quotation_item_hdr");

            entity.HasOne(d => d.RateCalculator).WithMany(p => p.TrnQuotationItems)
                .HasForeignKey(d => d.RateCalculatorId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_quotation_item_rate_calc");
        });

        modelBuilder.Entity<TrnQuotationTimeline>(entity =>
        {
            entity.HasKey(e => e.TimelineId).HasName("trn_quotation_timeline_pkey");

            entity.ToTable("trn_quotation_timeline", "press_db");

            entity.HasIndex(e => e.CreatedOn, "idx_quotation_timeline_created_on").IsDescending();

            entity.HasIndex(e => e.QuotationId, "idx_quotation_timeline_quotation_id");

            entity.HasIndex(e => e.NewStatus, "idx_quotation_timeline_status");

            entity.Property(e => e.TimelineId).HasColumnName("timeline_id");
            entity.Property(e => e.AssignedToUserId).HasColumnName("assigned_to_user_id");
            entity.Property(e => e.AttachmentUrl).HasColumnName("attachment_url");
            entity.Property(e => e.CommunicationMode)
                .HasMaxLength(50)
                .HasColumnName("communication_mode");
            entity.Property(e => e.CommunicationReference).HasColumnName("communication_reference");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.EnquiryId).HasColumnName("enquiry_id");
            entity.Property(e => e.EventCode)
                .HasMaxLength(50)
                .HasColumnName("event_code");
            entity.Property(e => e.EventDescription).HasColumnName("event_description");
            entity.Property(e => e.EventTitle)
                .HasMaxLength(200)
                .HasColumnName("event_title");
            entity.Property(e => e.EventType)
                .HasMaxLength(50)
                .HasColumnName("event_type");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.NewAmount)
                .HasPrecision(18, 2)
                .HasColumnName("new_amount");
            entity.Property(e => e.NewStatus)
                .HasMaxLength(50)
                .HasColumnName("new_status");
            entity.Property(e => e.OldAmount)
                .HasPrecision(18, 2)
                .HasColumnName("old_amount");
            entity.Property(e => e.OldStatus)
                .HasMaxLength(50)
                .HasColumnName("old_status");
            entity.Property(e => e.QuotationId).HasColumnName("quotation_id");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
            entity.Property(e => e.UpdatedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_on");

            entity.HasOne(d => d.Enquiry).WithMany(p => p.TrnQuotationTimelines)
                .HasForeignKey(d => d.EnquiryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_quotation_timeline_enquiry");

            entity.HasOne(d => d.Quotation).WithMany(p => p.TrnQuotationTimelines)
                .HasForeignKey(d => d.QuotationId)
                .HasConstraintName("fk_quotation_timeline_quotation");
        });

        modelBuilder.Entity<TrnReceipt>(entity =>
        {
            entity.HasKey(e => e.ReceiptId).HasName("trn_receipt_pkey");

            entity.ToTable("trn_receipt", "press_db");

            entity.HasIndex(e => e.CompanyId, "idx_receipt_company");

            entity.HasIndex(e => e.ReceiptDate, "idx_receipt_date");

            entity.HasIndex(e => e.PaymentMode, "idx_receipt_mode");

            entity.HasIndex(e => e.PartyId, "idx_receipt_party");

            entity.HasIndex(e => e.Status, "idx_receipt_status");

            entity.HasIndex(e => e.ReceiptNo, "trn_receipt_receipt_no_key").IsUnique();

            entity.Property(e => e.ReceiptId).HasColumnName("receipt_id");
            entity.Property(e => e.Amount)
                .HasPrecision(18, 2)
                .HasColumnName("amount");
            entity.Property(e => e.BankId).HasColumnName("bank_id");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(50)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.PartyId).HasColumnName("party_id");
            entity.Property(e => e.PaymentMode)
                .HasMaxLength(30)
                .HasColumnName("payment_mode");
            entity.Property(e => e.ReceiptDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("receipt_date");
            entity.Property(e => e.ReceiptNo)
                .HasMaxLength(30)
                .HasColumnName("receipt_no");
            entity.Property(e => e.ReferenceDate).HasColumnName("reference_date");
            entity.Property(e => e.ReferenceNo)
                .HasMaxLength(100)
                .HasColumnName("reference_no");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'POSTED'::character varying")
                .HasColumnName("status");

            entity.HasOne(d => d.Bank).WithMany(p => p.TrnReceipts)
                .HasForeignKey(d => d.BankId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_receipt_bank");

            entity.HasOne(d => d.Company).WithMany(p => p.TrnReceipts)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_receipt_company");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.TrnReceipts)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_receipt_created_by");

            entity.HasOne(d => d.Party).WithMany(p => p.TrnReceipts)
                .HasForeignKey(d => d.PartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_receipt_party");
        });

        modelBuilder.Entity<TrnReceiptAllocation>(entity =>
        {
            entity.HasKey(e => e.ReceiptAllocationId).HasName("trn_receipt_allocation_pkey");

            entity.ToTable("trn_receipt_allocation", "press_db");

            entity.HasIndex(e => e.SalesInvoiceId, "idx_receipt_alloc_invoice");

            entity.HasIndex(e => e.ReceiptId, "idx_receipt_alloc_receipt");

            entity.Property(e => e.ReceiptAllocationId).HasColumnName("receipt_allocation_id");
            entity.Property(e => e.AllocatedAmount)
                .HasPrecision(18, 2)
                .HasColumnName("allocated_amount");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.ReceiptId).HasColumnName("receipt_id");
            entity.Property(e => e.SalesInvoiceId).HasColumnName("sales_invoice_id");
            entity.Property(e => e.UnallocatedAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("unallocated_amount");

            entity.HasOne(d => d.Receipt).WithMany(p => p.TrnReceiptAllocations)
                .HasForeignKey(d => d.ReceiptId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_receipt_alloc_receipt");

            entity.HasOne(d => d.SalesInvoice).WithMany(p => p.TrnReceiptAllocations)
                .HasForeignKey(d => d.SalesInvoiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_receipt_alloc_invoice");
        });

        modelBuilder.Entity<TrnSalesInvoice>(entity =>
        {
            entity.HasKey(e => e.SalesInvoiceId).HasName("trn_sales_invoice_pkey");

            entity.ToTable("trn_sales_invoice", "press_db", tb => tb.HasComment("Sales invoice header for goods/services sold to customers. Linked to job, quotation. Supports GST (CGST/SGST/IGST), e-Invoice IRN, e-Way Bill."));

            entity.HasIndex(e => e.BillingAddressId, "idx_sinv_billing_addr");

            entity.HasIndex(e => e.CompanyId, "idx_sinv_company");

            entity.HasIndex(e => e.InvoiceDate, "idx_sinv_date");

            entity.HasIndex(e => e.DueDate, "idx_sinv_due_date");

            entity.HasIndex(e => e.FinYearId, "idx_sinv_fin_year");

            entity.HasIndex(e => e.IsPostedToGl, "idx_sinv_gl_posted").HasFilter("(is_posted_to_gl = false)");

            entity.HasIndex(e => e.JobId, "idx_sinv_job");

            entity.HasIndex(e => e.PartyId, "idx_sinv_party");

            entity.HasIndex(e => new { e.PartyId, e.Status }, "idx_sinv_party_status");

            entity.HasIndex(e => e.QuotationId, "idx_sinv_quotation");

            entity.HasIndex(e => e.ShippingAddressId, "idx_sinv_shipping_addr");

            entity.HasIndex(e => e.Status, "idx_sinv_status");

            entity.HasIndex(e => e.InvoiceNo, "uq_sales_invoice_no").IsUnique();

            entity.Property(e => e.SalesInvoiceId).HasColumnName("sales_invoice_id");
            entity.Property(e => e.AttachmentsJson)
                .HasDefaultValueSql("'[]'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("attachments_json");
            entity.Property(e => e.BalanceAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("balance_amount");
            entity.Property(e => e.BillingAddressId).HasColumnName("billing_address_id");
            entity.Property(e => e.CancelReason).HasColumnName("cancel_reason");
            entity.Property(e => e.CancelledBy).HasColumnName("cancelled_by");
            entity.Property(e => e.CancelledOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("cancelled_on");
            entity.Property(e => e.CessAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("cess_amount");
            entity.Property(e => e.CgstAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("cgst_amount");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.DiscountAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("discount_amount");
            entity.Property(e => e.DispatchThrough)
                .HasMaxLength(200)
                .HasColumnName("dispatch_through");
            entity.Property(e => e.DueDate).HasColumnName("due_date");
            entity.Property(e => e.EInvoiceAckDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("e_invoice_ack_date");
            entity.Property(e => e.EInvoiceAckNo)
                .HasMaxLength(50)
                .HasColumnName("e_invoice_ack_no");
            entity.Property(e => e.EInvoiceIrn)
                .HasMaxLength(100)
                .HasColumnName("e_invoice_irn");
            entity.Property(e => e.EWayBillNo)
                .HasMaxLength(50)
                .HasColumnName("e_way_bill_no");
            entity.Property(e => e.ExchangeRate)
                .HasPrecision(10, 4)
                .HasDefaultValueSql("1")
                .HasColumnName("exchange_rate");
            entity.Property(e => e.ExportType)
                .HasMaxLength(30)
                .HasColumnName("export_type");
            entity.Property(e => e.FinYearId).HasColumnName("fin_year_id");
            entity.Property(e => e.GlPostedBy).HasColumnName("gl_posted_by");
            entity.Property(e => e.GlPostedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("gl_posted_on");
            entity.Property(e => e.GrandTotal)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("grand_total");
            entity.Property(e => e.IgstAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("igst_amount");
            entity.Property(e => e.InternalNotes).HasColumnName("internal_notes");
            entity.Property(e => e.InvoiceDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("invoice_date");
            entity.Property(e => e.InvoiceNo)
                .HasMaxLength(50)
                .HasColumnName("invoice_no");
            entity.Property(e => e.IsCancelled)
                .HasDefaultValue(false)
                .HasColumnName("is_cancelled");
            entity.Property(e => e.IsExport)
                .HasDefaultValue(false)
                .HasColumnName("is_export");
            entity.Property(e => e.IsPostedToGl)
                .HasDefaultValue(false)
                .HasColumnName("is_posted_to_gl");
            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.LutNo)
                .HasMaxLength(50)
                .HasColumnName("lut_no");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.PaidAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("paid_amount");
            entity.Property(e => e.PartyId).HasColumnName("party_id");
            entity.Property(e => e.PaymentTermId).HasColumnName("payment_term_id");
            entity.Property(e => e.PlaceOfSupply)
                .HasMaxLength(150)
                .HasColumnName("place_of_supply");
            entity.Property(e => e.PoDate).HasColumnName("po_date");
            entity.Property(e => e.PoNo)
                .HasMaxLength(100)
                .HasColumnName("po_no");
            entity.Property(e => e.QuotationId).HasColumnName("quotation_id");
            entity.Property(e => e.RoundOff)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("round_off");
            entity.Property(e => e.SalesPerson)
                .HasMaxLength(200)
                .HasColumnName("sales_person");
            entity.Property(e => e.SgstAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("sgst_amount");
            entity.Property(e => e.ShippingAddressId).HasColumnName("shipping_address_id");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'DRAFT'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.SubtotalAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("subtotal_amount");
            entity.Property(e => e.TaxableAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("taxable_amount");
            entity.Property(e => e.TermsConditions).HasColumnName("terms_conditions");
            entity.Property(e => e.TotalTaxAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("total_tax_amount");
            entity.Property(e => e.VehicleNo)
                .HasMaxLength(50)
                .HasColumnName("vehicle_no");

            entity.HasOne(d => d.BillingAddress).WithMany(p => p.TrnSalesInvoiceBillingAddresses)
                .HasForeignKey(d => d.BillingAddressId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_sales_inv_billing_address");

            entity.HasOne(d => d.Company).WithMany(p => p.TrnSalesInvoices)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_sales_inv_company");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.TrnSalesInvoices)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_sales_inv_created_by");

            entity.HasOne(d => d.Currency).WithMany(p => p.TrnSalesInvoices)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_sales_inv_currency");

            entity.HasOne(d => d.FinYear).WithMany(p => p.TrnSalesInvoices)
                .HasForeignKey(d => d.FinYearId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_sales_inv_fin_year");

            entity.HasOne(d => d.Job).WithMany(p => p.TrnSalesInvoices)
                .HasForeignKey(d => d.JobId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_sales_inv_job");

            entity.HasOne(d => d.Party).WithMany(p => p.TrnSalesInvoices)
                .HasForeignKey(d => d.PartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_sales_inv_party");

            entity.HasOne(d => d.PaymentTerm).WithMany(p => p.TrnSalesInvoices)
                .HasForeignKey(d => d.PaymentTermId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_sales_inv_payment_term");

            entity.HasOne(d => d.Quotation).WithMany(p => p.TrnSalesInvoices)
                .HasForeignKey(d => d.QuotationId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_sales_inv_quotation");

            entity.HasOne(d => d.ShippingAddress).WithMany(p => p.TrnSalesInvoiceShippingAddresses)
                .HasForeignKey(d => d.ShippingAddressId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_sales_inv_shipping_address");
        });

        modelBuilder.Entity<TrnSalesInvoiceItem>(entity =>
        {
            entity.HasKey(e => e.InvoiceItemId).HasName("trn_sales_invoice_item_pkey");

            entity.ToTable("trn_sales_invoice_item", "press_db", tb => tb.HasComment("Sales invoice line items with full GST breakup (CGST/SGST/IGST/CESS) per item."));

            entity.HasIndex(e => e.AccountHeadId, "idx_sinv_item_acct_head");

            entity.HasIndex(e => e.HsnSacCode, "idx_sinv_item_hsn");

            entity.HasIndex(e => e.SalesInvoiceId, "idx_sinv_item_invoice");

            entity.HasIndex(e => e.ItemId, "idx_sinv_item_item");

            entity.HasIndex(e => e.JobId, "idx_sinv_item_job");

            entity.HasIndex(e => e.TaxCategoryId, "idx_sinv_item_tax_cat");

            entity.Property(e => e.InvoiceItemId).HasColumnName("invoice_item_id");
            entity.Property(e => e.AccountHeadId).HasColumnName("account_head_id");
            entity.Property(e => e.CessAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("cess_amount");
            entity.Property(e => e.CessPercent)
                .HasPrecision(6, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("cess_percent");
            entity.Property(e => e.CgstAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("cgst_amount");
            entity.Property(e => e.CgstPercent)
                .HasPrecision(6, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("cgst_percent");
            entity.Property(e => e.CostCenterId).HasColumnName("cost_center_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DiscountAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("discount_amount");
            entity.Property(e => e.DiscountPercent)
                .HasPrecision(6, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("discount_percent");
            entity.Property(e => e.HsnSacCode)
                .HasMaxLength(20)
                .HasColumnName("hsn_sac_code");
            entity.Property(e => e.IgstAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("igst_amount");
            entity.Property(e => e.IgstPercent)
                .HasPrecision(6, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("igst_percent");
            entity.Property(e => e.ItemId).HasColumnName("item_id");
            entity.Property(e => e.ItemSequence)
                .HasDefaultValue(1)
                .HasColumnName("item_sequence");
            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.LineTotal)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("line_total");
            entity.Property(e => e.Quantity)
                .HasPrecision(14, 4)
                .HasDefaultValueSql("0")
                .HasColumnName("quantity");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.SalesInvoiceId).HasColumnName("sales_invoice_id");
            entity.Property(e => e.SgstAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("sgst_amount");
            entity.Property(e => e.SgstPercent)
                .HasPrecision(6, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("sgst_percent");
            entity.Property(e => e.TaxCategoryId).HasColumnName("tax_category_id");
            entity.Property(e => e.TaxableValue)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("taxable_value");
            entity.Property(e => e.TotalTaxAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("total_tax_amount");
            entity.Property(e => e.UnitRate)
                .HasPrecision(14, 4)
                .HasDefaultValueSql("0")
                .HasColumnName("unit_rate");
            entity.Property(e => e.UomId).HasColumnName("uom_id");

            entity.HasOne(d => d.AccountHead).WithMany(p => p.TrnSalesInvoiceItems)
                .HasForeignKey(d => d.AccountHeadId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_sales_inv_item_account");

            entity.HasOne(d => d.Job).WithMany(p => p.TrnSalesInvoiceItems)
                .HasForeignKey(d => d.JobId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_sales_inv_item_job");

            entity.HasOne(d => d.SalesInvoice).WithMany(p => p.TrnSalesInvoiceItems)
                .HasForeignKey(d => d.SalesInvoiceId)
                .HasConstraintName("fk_sales_inv_item_header");

            entity.HasOne(d => d.TaxCategory).WithMany(p => p.TrnSalesInvoiceItems)
                .HasForeignKey(d => d.TaxCategoryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_sales_inv_item_tax_category");
        });

        modelBuilder.Entity<TrnStockLedger>(entity =>
        {
            entity.HasKey(e => e.LedgerId).HasName("trn_stock_ledger_pkey");

            entity.ToTable("trn_stock_ledger", "press_db");

            entity.HasIndex(e => e.TransactionDate, "idx_trn_stock_ledger_date");

            entity.HasIndex(e => e.JobId, "idx_trn_stock_ledger_job");

            entity.HasIndex(e => new { e.MaterialCategory, e.MaterialId }, "idx_trn_stock_ledger_mat");

            entity.HasIndex(e => new { e.ReferenceType, e.ReferenceId }, "idx_trn_stock_ledger_ref");

            entity.HasIndex(e => e.TransactionType, "idx_trn_stock_ledger_type");

            entity.Property(e => e.LedgerId).HasColumnName("ledger_id");
            entity.Property(e => e.Amount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("amount");
            entity.Property(e => e.BalanceQuantity)
                .HasPrecision(14, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("balance_quantity");
            entity.Property(e => e.CompanyId)
                .HasDefaultValue(1)
                .HasColumnName("company_id");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.JobNo)
                .HasMaxLength(30)
                .HasColumnName("job_no");
            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.MaterialCategory)
                .HasMaxLength(30)
                .HasColumnName("material_category");
            entity.Property(e => e.MaterialCode)
                .HasMaxLength(50)
                .HasColumnName("material_code");
            entity.Property(e => e.MaterialId).HasColumnName("material_id");
            entity.Property(e => e.MaterialName)
                .HasMaxLength(200)
                .HasColumnName("material_name");
            entity.Property(e => e.QuantityIn)
                .HasPrecision(14, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("quantity_in");
            entity.Property(e => e.QuantityOut)
                .HasPrecision(14, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("quantity_out");
            entity.Property(e => e.Rate)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("rate");
            entity.Property(e => e.ReferenceId).HasColumnName("reference_id");
            entity.Property(e => e.ReferenceNo)
                .HasMaxLength(30)
                .HasColumnName("reference_no");
            entity.Property(e => e.ReferenceType)
                .HasMaxLength(30)
                .HasColumnName("reference_type");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.TransactionDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("transaction_date");
            entity.Property(e => e.TransactionType)
                .HasMaxLength(30)
                .HasColumnName("transaction_type");
            entity.Property(e => e.Uom)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Pcs'::character varying")
                .HasColumnName("uom");
        });

        modelBuilder.Entity<TrnStoreIssue>(entity =>
        {
            entity.HasKey(e => e.IssueId).HasName("trn_store_issue_pkey");

            entity.ToTable("trn_store_issue", "press_db");

            entity.HasIndex(e => e.IssueDate, "idx_trn_store_issue_date");

            entity.HasIndex(e => e.JobId, "idx_trn_store_issue_job");

            entity.HasIndex(e => e.Status, "idx_trn_store_issue_status");

            entity.HasIndex(e => e.IssueType, "idx_trn_store_issue_type");

            entity.HasIndex(e => e.IssueNo, "trn_store_issue_issue_no_key").IsUnique();

            entity.Property(e => e.IssueId).HasColumnName("issue_id");
            entity.Property(e => e.ApprovedBy).HasColumnName("approved_by");
            entity.Property(e => e.ApprovedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("approved_on");
            entity.Property(e => e.CompanyId)
                .HasDefaultValue(1)
                .HasColumnName("company_id");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.FromLocationId).HasColumnName("from_location_id");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IssueDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("issue_date");
            entity.Property(e => e.IssueNo)
                .HasMaxLength(30)
                .HasColumnName("issue_no");
            entity.Property(e => e.IssueType)
                .HasMaxLength(30)
                .HasDefaultValueSql("'JOB'::character varying")
                .HasColumnName("issue_type");
            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.JobNo)
                .HasMaxLength(30)
                .HasColumnName("job_no");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(50)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.RateCalcId).HasColumnName("rate_calc_id");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'DRAFT'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.ToLocationId).HasColumnName("to_location_id");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("total_amount");
            entity.Property(e => e.TotalItems)
                .HasDefaultValue(0)
                .HasColumnName("total_items");
        });

        modelBuilder.Entity<TrnStoreIssueItem>(entity =>
        {
            entity.HasKey(e => e.IssueItemId).HasName("trn_store_issue_item_pkey");

            entity.ToTable("trn_store_issue_item", "press_db");

            entity.HasIndex(e => e.MaterialCategory, "idx_trn_store_issue_item_cat");

            entity.HasIndex(e => e.IssueId, "idx_trn_store_issue_item_issue");

            entity.Property(e => e.IssueItemId).HasColumnName("issue_item_id");
            entity.Property(e => e.Amount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("amount");
            entity.Property(e => e.AvailableStock)
                .HasPrecision(14, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("available_stock");
            entity.Property(e => e.BomQuantity)
                .HasPrecision(14, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("bom_quantity");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.ForPart)
                .HasMaxLength(50)
                .HasColumnName("for_part");
            entity.Property(e => e.IsSelected)
                .HasDefaultValue(true)
                .HasColumnName("is_selected");
            entity.Property(e => e.IssueId).HasColumnName("issue_id");
            entity.Property(e => e.IssuedQuantity)
                .HasPrecision(14, 3)
                .HasColumnName("issued_quantity");
            entity.Property(e => e.ItemSequence)
                .HasDefaultValue(1)
                .HasColumnName("item_sequence");
            entity.Property(e => e.MaterialCategory)
                .HasMaxLength(30)
                .HasColumnName("material_category");
            entity.Property(e => e.MaterialCode)
                .HasMaxLength(50)
                .HasColumnName("material_code");
            entity.Property(e => e.MaterialId).HasColumnName("material_id");
            entity.Property(e => e.MaterialName)
                .HasMaxLength(200)
                .HasColumnName("material_name");
            entity.Property(e => e.Rate)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("rate");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.Specification).HasColumnName("specification");
            entity.Property(e => e.Uom)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Pcs'::character varying")
                .HasColumnName("uom");

            entity.HasOne(d => d.Issue).WithMany(p => p.TrnStoreIssueItems)
                .HasForeignKey(d => d.IssueId)
                .HasConstraintName("trn_store_issue_item_issue_id_fkey");
        });

        modelBuilder.Entity<TrnStoreReceive>(entity =>
        {
            entity.HasKey(e => e.ReceiveId).HasName("trn_store_receive_pkey");

            entity.ToTable("trn_store_receive", "press_db");

            entity.HasIndex(e => e.ReceiveDate, "idx_trn_store_receive_date");

            entity.HasIndex(e => e.GrnId, "idx_trn_store_receive_grn");

            entity.HasIndex(e => e.Status, "idx_trn_store_receive_status");

            entity.HasIndex(e => e.ReceiveType, "idx_trn_store_receive_type");

            entity.HasIndex(e => e.ReceiveNo, "trn_store_receive_receive_no_key").IsUnique();

            entity.Property(e => e.ReceiveId).HasColumnName("receive_id");
            entity.Property(e => e.ApprovedBy).HasColumnName("approved_by");
            entity.Property(e => e.ApprovedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("approved_on");
            entity.Property(e => e.CompanyId)
                .HasDefaultValue(1)
                .HasColumnName("company_id");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.GrnId).HasColumnName("grn_id");
            entity.Property(e => e.GrnNo)
                .HasMaxLength(30)
                .HasColumnName("grn_no");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.JobNo)
                .HasMaxLength(30)
                .HasColumnName("job_no");
            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(50)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.ReceiveDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("receive_date");
            entity.Property(e => e.ReceiveNo)
                .HasMaxLength(30)
                .HasColumnName("receive_no");
            entity.Property(e => e.ReceiveType)
                .HasMaxLength(30)
                .HasDefaultValueSql("'PURCHASE'::character varying")
                .HasColumnName("receive_type");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'DRAFT'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            entity.Property(e => e.SupplierName)
                .HasMaxLength(200)
                .HasColumnName("supplier_name");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("total_amount");
            entity.Property(e => e.TotalItems)
                .HasDefaultValue(0)
                .HasColumnName("total_items");
        });

        modelBuilder.Entity<TrnStoreReceiveItem>(entity =>
        {
            entity.HasKey(e => e.ReceiveItemId).HasName("trn_store_receive_item_pkey");

            entity.ToTable("trn_store_receive_item", "press_db");

            entity.HasIndex(e => e.MaterialCategory, "idx_trn_store_receive_item_cat");

            entity.HasIndex(e => e.ReceiveId, "idx_trn_store_receive_item_rcv");

            entity.Property(e => e.ReceiveItemId).HasColumnName("receive_item_id");
            entity.Property(e => e.AcceptedQuantity)
                .HasPrecision(14, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("accepted_quantity");
            entity.Property(e => e.Amount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("amount");
            entity.Property(e => e.BatchNo)
                .HasMaxLength(50)
                .HasColumnName("batch_no");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.ExpiryDate).HasColumnName("expiry_date");
            entity.Property(e => e.ForPart)
                .HasMaxLength(50)
                .HasColumnName("for_part");
            entity.Property(e => e.IsSelected)
                .HasDefaultValue(true)
                .HasColumnName("is_selected");
            entity.Property(e => e.ItemSequence)
                .HasDefaultValue(1)
                .HasColumnName("item_sequence");
            entity.Property(e => e.MaterialCategory)
                .HasMaxLength(30)
                .HasColumnName("material_category");
            entity.Property(e => e.MaterialCode)
                .HasMaxLength(50)
                .HasColumnName("material_code");
            entity.Property(e => e.MaterialId).HasColumnName("material_id");
            entity.Property(e => e.MaterialName)
                .HasMaxLength(200)
                .HasColumnName("material_name");
            entity.Property(e => e.OrderedQuantity)
                .HasPrecision(14, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("ordered_quantity");
            entity.Property(e => e.Rate)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("rate");
            entity.Property(e => e.ReceiveId).HasColumnName("receive_id");
            entity.Property(e => e.ReceivedQuantity)
                .HasPrecision(14, 3)
                .HasColumnName("received_quantity");
            entity.Property(e => e.RejectedQuantity)
                .HasPrecision(14, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("rejected_quantity");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.Specification).HasColumnName("specification");
            entity.Property(e => e.Uom)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Pcs'::character varying")
                .HasColumnName("uom");

            entity.HasOne(d => d.Receive).WithMany(p => p.TrnStoreReceiveItems)
                .HasForeignKey(d => d.ReceiveId)
                .HasConstraintName("trn_store_receive_item_receive_id_fkey");
        });

        modelBuilder.Entity<TrnStoreTimeline>(entity =>
        {
            entity.HasKey(e => e.TimelineId).HasName("trn_store_timeline_pkey");

            entity.ToTable("trn_store_timeline", "press_db");

            entity.HasIndex(e => new { e.Module, e.ReferenceId }, "idx_trn_store_timeline_module");

            entity.HasIndex(e => e.EventType, "idx_trn_store_timeline_type");

            entity.Property(e => e.TimelineId).HasColumnName("timeline_id");
            entity.Property(e => e.AttachmentUrl).HasColumnName("attachment_url");
            entity.Property(e => e.CreatedBy)
                .HasDefaultValue(0L)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.EventCode)
                .HasMaxLength(50)
                .HasColumnName("event_code");
            entity.Property(e => e.EventDescription).HasColumnName("event_description");
            entity.Property(e => e.EventTitle)
                .HasMaxLength(200)
                .HasColumnName("event_title");
            entity.Property(e => e.EventType)
                .HasMaxLength(50)
                .HasColumnName("event_type");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Module)
                .HasMaxLength(30)
                .HasColumnName("module");
            entity.Property(e => e.NewStatus)
                .HasMaxLength(20)
                .HasColumnName("new_status");
            entity.Property(e => e.OldStatus)
                .HasMaxLength(20)
                .HasColumnName("old_status");
            entity.Property(e => e.ReferenceId).HasColumnName("reference_id");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
        });

        modelBuilder.Entity<TrnTaxLedger>(entity =>
        {
            entity.HasKey(e => e.TaxLedgerId).HasName("trn_tax_ledger_pkey");

            entity.ToTable("trn_tax_ledger", "press_db", tb => tb.HasComment("Tax ledger for GST compliance. One row per tax component per voucher line. Powers GSTR-1, GSTR-2B, GSTR-3B, and ITC reports. direction_id 1=Output (payable), 2=Input (ITC)."));

            entity.HasIndex(e => new { e.CompanyId, e.TaxPeriod }, "idx_tl_company_period");

            entity.HasIndex(e => e.DirectionId, "idx_tl_direction");

            entity.HasIndex(e => e.HsnSacCode, "idx_tl_hsn");

            entity.HasIndex(e => e.PartyId, "idx_tl_party");

            entity.HasIndex(e => new { e.VoucherType, e.VoucherId }, "idx_tl_voucher");

            entity.Property(e => e.TaxLedgerId).HasColumnName("tax_ledger_id");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.DirectionId).HasColumnName("direction_id");
            entity.Property(e => e.FinYearId).HasColumnName("fin_year_id");
            entity.Property(e => e.HsnSacCode)
                .HasMaxLength(20)
                .HasColumnName("hsn_sac_code");
            entity.Property(e => e.IsExempt)
                .HasDefaultValue(false)
                .HasColumnName("is_exempt");
            entity.Property(e => e.IsNilRated)
                .HasDefaultValue(false)
                .HasColumnName("is_nil_rated");
            entity.Property(e => e.IsReverseCharge)
                .HasDefaultValue(false)
                .HasColumnName("is_reverse_charge");
            entity.Property(e => e.ItcCategory)
                .HasMaxLength(30)
                .HasComment("ITC category: INPUTS, CAPITAL_GOODS, INPUT_SERVICES, INELIGIBLE")
                .HasColumnName("itc_category");
            entity.Property(e => e.ItcEligible)
                .HasDefaultValue(true)
                .HasColumnName("itc_eligible");
            entity.Property(e => e.PartyGstin)
                .HasMaxLength(20)
                .HasColumnName("party_gstin");
            entity.Property(e => e.PartyId).HasColumnName("party_id");
            entity.Property(e => e.PlaceOfSupply)
                .HasMaxLength(150)
                .HasColumnName("place_of_supply");
            entity.Property(e => e.PostingDate).HasColumnName("posting_date");
            entity.Property(e => e.TaxAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("tax_amount");
            entity.Property(e => e.TaxComponentId).HasColumnName("tax_component_id");
            entity.Property(e => e.TaxPeriod)
                .HasMaxLength(10)
                .HasComment("GST return period in MMYYYY format e.g. 072025 for July 2025.")
                .HasColumnName("tax_period");
            entity.Property(e => e.TaxRate)
                .HasPrecision(8, 4)
                .HasDefaultValueSql("0")
                .HasColumnName("tax_rate");
            entity.Property(e => e.TaxableValue)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("taxable_value");
            entity.Property(e => e.TransactionTypeId).HasColumnName("transaction_type_id");
            entity.Property(e => e.VoucherDate).HasColumnName("voucher_date");
            entity.Property(e => e.VoucherId).HasColumnName("voucher_id");
            entity.Property(e => e.VoucherNo)
                .HasMaxLength(50)
                .HasColumnName("voucher_no");
            entity.Property(e => e.VoucherType)
                .HasMaxLength(50)
                .HasColumnName("voucher_type");

            entity.HasOne(d => d.Company).WithMany(p => p.TrnTaxLedgers)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_tl_company");

            entity.HasOne(d => d.Direction).WithMany(p => p.TrnTaxLedgers)
                .HasForeignKey(d => d.DirectionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_tl_direction");

            entity.HasOne(d => d.TaxComponent).WithMany(p => p.TrnTaxLedgers)
                .HasForeignKey(d => d.TaxComponentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_tl_tax_component");

            entity.HasOne(d => d.TransactionType).WithMany(p => p.TrnTaxLedgers)
                .HasForeignKey(d => d.TransactionTypeId)
                .HasConstraintName("fk_tl_transaction_type");
        });

        modelBuilder.Entity<TrnTdsLedger>(entity =>
        {
            entity.HasKey(e => e.TdsId).HasName("trn_tds_ledger_pkey");

            entity.ToTable("trn_tds_ledger", "press_db", tb => tb.HasComment("TDS (Tax Deducted at Source) ledger. Tracks TDS deducted on payments to suppliers/vendors. Powers TDS return filing (26Q/27Q), certificate generation."));

            entity.HasIndex(e => e.CompanyId, "idx_tds_company");

            entity.HasIndex(e => e.PartyId, "idx_tds_party");

            entity.HasIndex(e => e.TdsSection, "idx_tds_section");

            entity.HasIndex(e => new { e.VoucherType, e.VoucherId }, "idx_tds_voucher");

            entity.Property(e => e.TdsId).HasColumnName("tds_id");
            entity.Property(e => e.BaseAmount)
                .HasPrecision(18, 2)
                .HasColumnName("base_amount");
            entity.Property(e => e.BsrCode)
                .HasMaxLength(20)
                .HasColumnName("bsr_code");
            entity.Property(e => e.CertificateNo)
                .HasMaxLength(50)
                .HasColumnName("certificate_no");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.DepositChallanNo)
                .HasMaxLength(50)
                .HasColumnName("deposit_challan_no");
            entity.Property(e => e.DepositDate).HasColumnName("deposit_date");
            entity.Property(e => e.EducationCess)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("education_cess");
            entity.Property(e => e.FinYearId).HasColumnName("fin_year_id");
            entity.Property(e => e.IsDeposited)
                .HasDefaultValue(false)
                .HasColumnName("is_deposited");
            entity.Property(e => e.IsReturnFiled)
                .HasDefaultValue(false)
                .HasColumnName("is_return_filed");
            entity.Property(e => e.Narration).HasColumnName("narration");
            entity.Property(e => e.PartyId).HasColumnName("party_id");
            entity.Property(e => e.Quarter)
                .HasMaxLength(10)
                .HasComment("TDS quarter: Q1 (Apr-Jun), Q2 (Jul-Sep), Q3 (Oct-Dec), Q4 (Jan-Mar)")
                .HasColumnName("quarter");
            entity.Property(e => e.SurchargeAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("surcharge_amount");
            entity.Property(e => e.TdsAmount)
                .HasPrecision(18, 2)
                .HasColumnName("tds_amount");
            entity.Property(e => e.TdsRate)
                .HasPrecision(6, 3)
                .HasColumnName("tds_rate");
            entity.Property(e => e.TdsSection)
                .HasMaxLength(20)
                .HasComment("TDS section: 194C (Contractor), 194J (Professional), 194I (Rent), 194H (Commission), 194A (Interest), etc.")
                .HasColumnName("tds_section");
            entity.Property(e => e.TotalTdsAmount)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("total_tds_amount");
            entity.Property(e => e.VoucherDate).HasColumnName("voucher_date");
            entity.Property(e => e.VoucherId).HasColumnName("voucher_id");
            entity.Property(e => e.VoucherNo)
                .HasMaxLength(50)
                .HasColumnName("voucher_no");
            entity.Property(e => e.VoucherType)
                .HasMaxLength(50)
                .HasColumnName("voucher_type");

            entity.HasOne(d => d.Company).WithMany(p => p.TrnTdsLedgers)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_tds_company");

            entity.HasOne(d => d.Party).WithMany(p => p.TrnTdsLedgers)
                .HasForeignKey(d => d.PartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_tds_party");
        });

        modelBuilder.Entity<TrnUserAccessLog>(entity =>
        {
            entity.HasKey(e => e.AccessLogId).HasName("trn_user_access_log_pkey");

            entity.ToTable("trn_user_access_log", "press_db");

            entity.HasIndex(e => e.LoginTime, "idx_access_log_login_time");

            entity.HasIndex(e => e.UserId, "idx_access_log_user");

            entity.Property(e => e.AccessLogId).HasColumnName("access_log_id");
            entity.Property(e => e.DeviceInfo).HasColumnName("device_info");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(50)
                .HasColumnName("ip_address");
            entity.Property(e => e.LoginLocation)
                .HasMaxLength(100)
                .HasColumnName("login_location");
            entity.Property(e => e.LoginTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("login_time");
            entity.Property(e => e.LogoutTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("logout_time");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        modelBuilder.Entity<TrnUserActivityLog>(entity =>
        {
            entity.HasKey(e => e.ActivityLogId).HasName("trn_user_activity_log_pkey");

            entity.ToTable("trn_user_activity_log", "press_db", tb => tb.HasComment("Generic user activity log capturing all actions across the complete ERP system. Tracks CRUD operations, approvals, status changes, exports, prints, logins, navigation, and any user-initiated action. Supports audit trail with before/after value snapshots. Uses JSONB for flexible activity metadata."));

            entity.HasIndex(e => e.IsArchived, "idx_activity_archived").HasFilter("(is_archived = false)");

            entity.HasIndex(e => e.CompanyId, "idx_activity_company");

            entity.HasIndex(e => e.CorrelationId, "idx_activity_correlation");

            entity.HasIndex(e => new { e.EntityType, e.EntityId }, "idx_activity_entity");

            entity.HasIndex(e => e.JobId, "idx_activity_job");

            entity.HasIndex(e => e.Module, "idx_activity_module");

            entity.HasIndex(e => e.ActivityOn, "idx_activity_on");

            entity.HasIndex(e => e.ProcessId, "idx_activity_process");

            entity.HasIndex(e => e.Severity, "idx_activity_severity");

            entity.HasIndex(e => e.ActivityType, "idx_activity_type");

            entity.HasIndex(e => e.UserId, "idx_activity_user");

            entity.Property(e => e.ActivityLogId).HasColumnName("activity_log_id");
            entity.Property(e => e.ActivityCategory)
                .HasMaxLength(50)
                .HasDefaultValueSql("'DATA'::character varying")
                .HasComment("Category: DATA (CRUD), AUTH (login/logout), NAVIGATION (page views), REPORT (report generation), APPROVAL (workflow), COMMUNICATION (email/sms/whatsapp), SYSTEM (background jobs)")
                .HasColumnName("activity_category");
            entity.Property(e => e.ActivityData)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasComment("Flexible JSONB for context-specific metadata: report parameters, export format, filter criteria, approval remarks, etc.")
                .HasColumnType("jsonb")
                .HasColumnName("activity_data");
            entity.Property(e => e.ActivityOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("activity_on");
            entity.Property(e => e.ActivityType)
                .HasMaxLength(50)
                .HasComment("Action type: CREATE, UPDATE, DELETE, VIEW, APPROVE, REJECT, PRINT, EXPORT, IMPORT, LOGIN, LOGOUT, STATUS_CHANGE, ASSIGN, UPLOAD, DOWNLOAD, SEND, CANCEL, CLOSE, REOPEN")
                .HasColumnName("activity_type");
            entity.Property(e => e.ChangedFields).HasColumnName("changed_fields");
            entity.Property(e => e.Channel)
                .HasMaxLength(20)
                .HasDefaultValueSql("'WEB'::character varying")
                .HasColumnName("channel");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.CorrelationId)
                .HasMaxLength(50)
                .HasColumnName("correlation_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DeviceInfo)
                .HasMaxLength(200)
                .HasColumnName("device_info");
            entity.Property(e => e.DurationMs).HasColumnName("duration_ms");
            entity.Property(e => e.EntityCode)
                .HasMaxLength(100)
                .HasColumnName("entity_code");
            entity.Property(e => e.EntityId).HasColumnName("entity_id");
            entity.Property(e => e.EntityType)
                .HasMaxLength(100)
                .HasColumnName("entity_type");
            entity.Property(e => e.ErrorMessage).HasColumnName("error_message");
            entity.Property(e => e.HttpMethod)
                .HasMaxLength(10)
                .HasColumnName("http_method");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(50)
                .HasColumnName("ip_address");
            entity.Property(e => e.IsArchived)
                .HasDefaultValue(false)
                .HasColumnName("is_archived");
            entity.Property(e => e.IsSuccess)
                .HasDefaultValue(true)
                .HasColumnName("is_success");
            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.Module)
                .HasMaxLength(50)
                .HasComment("ERP module: JOB, ENQUIRY, QUOTATION, USER_MGMT, MASTER, CRM, DISPATCH, PAYMENT, QUALITY, STOCK, MACHINE_SCHEDULE, RATE_CALC, REPORT, AUTH, NOTIFICATION, DASHBOARD, SETTINGS")
                .HasColumnName("module");
            entity.Property(e => e.NewValues)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasComment("JSONB snapshot of new field values after a CREATE or UPDATE. Used for audit trail and change history.")
                .HasColumnType("jsonb")
                .HasColumnName("new_values");
            entity.Property(e => e.OldValues)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasComment("JSONB snapshot of previous field values before an UPDATE or DELETE. Used for audit trail and change history.")
                .HasColumnType("jsonb")
                .HasColumnName("old_values");
            entity.Property(e => e.ProcessId).HasColumnName("process_id");
            entity.Property(e => e.RelatedEntityCode)
                .HasMaxLength(100)
                .HasColumnName("related_entity_code");
            entity.Property(e => e.RelatedEntityId).HasColumnName("related_entity_id");
            entity.Property(e => e.RelatedEntityType)
                .HasMaxLength(100)
                .HasColumnName("related_entity_type");
            entity.Property(e => e.RequestPath)
                .HasMaxLength(500)
                .HasColumnName("request_path");
            entity.Property(e => e.SessionId)
                .HasMaxLength(100)
                .HasColumnName("session_id");
            entity.Property(e => e.Severity)
                .HasMaxLength(20)
                .HasDefaultValueSql("'INFO'::character varying")
                .HasComment("INFO: normal operations, WARNING: unusual activity, CRITICAL: security-sensitive actions, AUDIT: compliance-required logging")
                .HasColumnName("severity");
            entity.Property(e => e.SubModule)
                .HasMaxLength(100)
                .HasColumnName("sub_module");
            entity.Property(e => e.SubprocessId).HasColumnName("subprocess_id");
            entity.Property(e => e.Title)
                .HasMaxLength(300)
                .HasColumnName("title");
            entity.Property(e => e.UserAgent)
                .HasMaxLength(500)
                .HasColumnName("user_agent");
            entity.Property(e => e.UserCode)
                .HasMaxLength(50)
                .HasColumnName("user_code");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.UserName)
                .HasMaxLength(100)
                .HasColumnName("user_name");
        });

        modelBuilder.Entity<TrnUserNotification>(entity =>
        {
            entity.HasKey(e => e.UserNotificationId).HasName("trn_user_notification_pkey");

            entity.ToTable("trn_user_notification", "press_db");

            entity.HasIndex(e => e.ActionRequired, "idx_user_notif_action_req").HasFilter("(action_required = true)");

            entity.HasIndex(e => e.Module, "idx_user_notif_module");

            entity.HasIndex(e => e.NotificationId, "idx_user_notif_notif");

            entity.HasIndex(e => new { e.UserId, e.IsRead }, "idx_user_notif_read").HasFilter("(is_read = false)");

            entity.HasIndex(e => e.UserId, "idx_user_notif_user");

            entity.Property(e => e.UserNotificationId).HasColumnName("user_notification_id");
            entity.Property(e => e.ActionLabel)
                .HasMaxLength(50)
                .HasColumnName("action_label");
            entity.Property(e => e.ActionRequired)
                .HasDefaultValue(false)
                .HasColumnName("action_required");
            entity.Property(e => e.ActionUrl)
                .HasMaxLength(255)
                .HasColumnName("action_url");
            entity.Property(e => e.AiGenerated)
                .HasDefaultValue(false)
                .HasColumnName("ai_generated");
            entity.Property(e => e.Color)
                .HasMaxLength(20)
                .HasColumnName("color");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.DismissedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dismissed_at");
            entity.Property(e => e.EventType)
                .HasMaxLength(50)
                .HasColumnName("event_type");
            entity.Property(e => e.ExpiresAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("expires_at");
            entity.Property(e => e.Icon)
                .HasMaxLength(50)
                .HasColumnName("icon");
            entity.Property(e => e.IsDismissed)
                .HasDefaultValue(false)
                .HasColumnName("is_dismissed");
            entity.Property(e => e.IsRead)
                .HasDefaultValue(false)
                .HasColumnName("is_read");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.Module)
                .HasMaxLength(50)
                .HasColumnName("module");
            entity.Property(e => e.NotificationId).HasColumnName("notification_id");
            entity.Property(e => e.Priority)
                .HasMaxLength(20)
                .HasDefaultValueSql("'NORMAL'::character varying")
                .HasColumnName("priority");
            entity.Property(e => e.ReadAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("read_at");
            entity.Property(e => e.ReferenceId).HasColumnName("reference_id");
            entity.Property(e => e.ReferenceUrl)
                .HasMaxLength(255)
                .HasColumnName("reference_url");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Notification).WithMany(p => p.TrnUserNotifications)
                .HasForeignKey(d => d.NotificationId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_user_notif_notification");

            entity.HasOne(d => d.User).WithMany(p => p.TrnUserNotifications)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("fk_user_notif_user");
        });

        modelBuilder.Entity<TrnWorkspaceTask>(entity =>
        {
            entity.HasKey(e => e.WorkspaceTaskId).HasName("trn_workspace_task_pkey");

            entity.ToTable("trn_workspace_task", "press_db", tb => tb.HasComment("Consolidated workspace task table aggregating tasks, approvals and follow-ups from all ERP modules. Provides a single source for the My Workspace dashboard. Rows are created by triggers or application logic when tasks/approvals are assigned."));

            entity.HasIndex(e => new { e.WorkflowBatchId, e.IsBlocking, e.TaskStatus }, "idx_workspace_task_blocking").HasFilter("(workflow_batch_id IS NOT NULL)");

            entity.HasIndex(e => new { e.JobId, e.TaskStatus }, "idx_workspace_task_queued").HasFilter("((task_status)::text = 'QUEUED'::text)");

            entity.HasIndex(e => new { e.WorkflowBatchId, e.SequenceNo }, "idx_workspace_task_sequence").HasFilter("(workflow_batch_id IS NOT NULL)");

            entity.HasIndex(e => e.WorkflowBatchId, "idx_workspace_task_workflow_batch").HasFilter("(workflow_batch_id IS NOT NULL)");

            entity.HasIndex(e => new { e.UserId, e.CreatedOn }, "idx_ws_task_created").IsDescending(false, true);

            entity.HasIndex(e => e.DueDate, "idx_ws_task_due_date").HasFilter("((task_status)::text = ANY ((ARRAY['PENDING'::character varying, 'IN_PROGRESS'::character varying])::text[]))");

            entity.HasIndex(e => e.JobId, "idx_ws_task_job").HasFilter("(job_id IS NOT NULL)");

            entity.HasIndex(e => e.UserId, "idx_ws_task_overdue").HasFilter("((is_overdue = true) AND ((task_status)::text <> ALL ((ARRAY['COMPLETED'::character varying, 'CANCELLED'::character varying])::text[])))");

            entity.HasIndex(e => new { e.SourceTable, e.SourceId }, "idx_ws_task_source");

            entity.HasIndex(e => new { e.UserId, e.Priority }, "idx_ws_task_user_priority");

            entity.HasIndex(e => new { e.UserId, e.TaskStatus }, "idx_ws_task_user_status");

            entity.HasIndex(e => new { e.UserId, e.TaskType }, "idx_ws_task_user_type");

            entity.Property(e => e.WorkspaceTaskId).HasColumnName("workspace_task_id");
            entity.Property(e => e.ActionUrl)
                .HasMaxLength(300)
                .HasColumnName("action_url");
            entity.Property(e => e.ApprovalLevel).HasColumnName("approval_level");
            entity.Property(e => e.ApprovalTypeId).HasColumnName("approval_type_id");
            entity.Property(e => e.AssignedBy).HasColumnName("assigned_by");
            entity.Property(e => e.AssignedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("assigned_on");
            entity.Property(e => e.CompletedBy).HasColumnName("completed_by");
            entity.Property(e => e.CompletedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("completed_on");
            entity.Property(e => e.CompletionRemarks).HasColumnName("completion_remarks");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DueDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("due_date");
            entity.Property(e => e.IsArchived)
                .HasDefaultValue(false)
                .HasColumnName("is_archived");
            entity.Property(e => e.IsBlocking)
                .HasDefaultValue(true)
                .HasComment("If TRUE, this task blocks workflow progression. If FALSE, workflow can proceed to next step even while this task is pending. Inherited from workflow step but can be overridden.")
                .HasColumnName("is_blocking");
            entity.Property(e => e.IsOverdue)
                .HasDefaultValue(false)
                .HasColumnName("is_overdue");
            entity.Property(e => e.IsRead)
                .HasDefaultValue(false)
                .HasColumnName("is_read");
            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.JobNo)
                .HasMaxLength(50)
                .HasColumnName("job_no");
            entity.Property(e => e.Metadata)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("metadata");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.PartyName)
                .HasMaxLength(200)
                .HasColumnName("party_name");
            entity.Property(e => e.Priority)
                .HasMaxLength(20)
                .HasDefaultValueSql("'NORMAL'::character varying")
                .HasColumnName("priority");
            entity.Property(e => e.ProcessCode)
                .HasMaxLength(30)
                .HasColumnName("process_code");
            entity.Property(e => e.ProcessId).HasColumnName("process_id");
            entity.Property(e => e.ReadAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("read_at");
            entity.Property(e => e.SequenceNo)
                .HasComment("Sequence number within the workflow. Used for ordering tasks in pre-generated workflow.")
                .HasColumnName("sequence_no");
            entity.Property(e => e.SlaHours)
                .HasPrecision(8, 2)
                .HasColumnName("sla_hours");
            entity.Property(e => e.SourceId).HasColumnName("source_id");
            entity.Property(e => e.SourceNo)
                .HasMaxLength(50)
                .HasColumnName("source_no");
            entity.Property(e => e.SourceTable)
                .HasMaxLength(50)
                .HasComment("Origin table: trn_job, trn_enquiry, trn_quotation, trn_challan, trn_purchase_order, trn_sales_invoice, etc.")
                .HasColumnName("source_table");
            entity.Property(e => e.SubprocessCode)
                .HasMaxLength(30)
                .HasColumnName("subprocess_code");
            entity.Property(e => e.SubprocessId).HasColumnName("subprocess_id");
            entity.Property(e => e.TaskStatus)
                .HasMaxLength(30)
                .HasDefaultValueSql("'PENDING'::character varying")
                .HasComment("PENDING, IN_PROGRESS, COMPLETED, OVERDUE, CANCELLED, REJECTED, APPROVED")
                .HasColumnName("task_status");
            entity.Property(e => e.TaskType)
                .HasMaxLength(30)
                .HasComment("TASK: assigned work item, APPROVAL: pending approval request, REVIEW: QC/review item, FOLLOW_UP: CRM/payment follow-up")
                .HasColumnName("task_type");
            entity.Property(e => e.Title)
                .HasMaxLength(300)
                .HasColumnName("title");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.WorkflowBatchId)
                .HasComment("Workflow batch ID to group all tasks belonging to the same workflow instance.")
                .HasColumnName("workflow_batch_id");
            entity.Property(e => e.WorkflowStepId)
                .HasComment("Reference to the workflow step that generated this task. Null for ad-hoc tasks.")
                .HasColumnName("workflow_step_id");
            entity.Property(e => e.WorkflowTemplateId)
                .HasComment("Reference to the workflow template. Null for ad-hoc tasks.")
                .HasColumnName("workflow_template_id");

            entity.HasOne(d => d.AssignedByNavigation).WithMany(p => p.TrnWorkspaceTaskAssignedByNavigations)
                .HasForeignKey(d => d.AssignedBy)
                .HasConstraintName("fk_workspace_task_assigned_by");

            entity.HasOne(d => d.Department).WithMany(p => p.TrnWorkspaceTasks)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("fk_workspace_task_department");

            entity.HasOne(d => d.Process).WithMany(p => p.TrnWorkspaceTasks)
                .HasForeignKey(d => d.ProcessId)
                .HasConstraintName("fk_workspace_task_process");

            entity.HasOne(d => d.User).WithMany(p => p.TrnWorkspaceTaskUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_workspace_task_user");

            entity.HasOne(d => d.WorkflowStep).WithMany(p => p.TrnWorkspaceTasks)
                .HasForeignKey(d => d.WorkflowStepId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_workspace_task_workflow_step");

            entity.HasOne(d => d.WorkflowTemplate).WithMany(p => p.TrnWorkspaceTasks)
                .HasForeignKey(d => d.WorkflowTemplateId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_workspace_task_workflow_template");
        });

        modelBuilder.Entity<TrnWorkspaceTaskItem>(entity =>
        {
            entity.HasKey(e => e.TaskItemId).HasName("trn_workspace_task_item_pkey");

            entity.ToTable("trn_workspace_task_item", "press_db", tb => tb.HasComment("Item-level task tracking for parallel execution. Each job item (e.g. Cover Page, Book Content) gets independent task rows per process (Design, CTP, PostPress), enabling simultaneous work across items."));

            entity.HasIndex(e => new { e.AssignedUserId, e.TaskStatus }, "idx_wti_assigned_user");

            entity.HasIndex(e => new { e.JobId, e.JobItemId }, "idx_wti_job_item");

            entity.HasIndex(e => new { e.ProcessCode, e.TaskStatus }, "idx_wti_process_status");

            entity.HasIndex(e => e.WorkspaceTaskId, "idx_wti_workspace_task");

            entity.Property(e => e.TaskItemId).HasColumnName("task_item_id");
            entity.Property(e => e.AssignedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("assigned_on");
            entity.Property(e => e.AssignedUserId).HasColumnName("assigned_user_id");
            entity.Property(e => e.CompletedBy).HasColumnName("completed_by");
            entity.Property(e => e.CompletedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("completed_on");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.ItemDescription).HasColumnName("item_description");
            entity.Property(e => e.ItemName)
                .HasMaxLength(300)
                .HasColumnName("item_name");
            entity.Property(e => e.ItemSequence)
                .HasDefaultValue(1)
                .HasColumnName("item_sequence");
            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.JobItemId).HasColumnName("job_item_id");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_on");
            entity.Property(e => e.ParentTaskItemId)
                .HasComment("Links to the upstream item task that triggered this one (e.g. Design Cover → CTP Cover)")
                .HasColumnName("parent_task_item_id");
            entity.Property(e => e.ProcessCode)
                .HasMaxLength(30)
                .HasColumnName("process_code");
            entity.Property(e => e.ProcessName)
                .HasMaxLength(200)
                .HasColumnName("process_name");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.StartedBy).HasColumnName("started_by");
            entity.Property(e => e.StartedOn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("started_on");
            entity.Property(e => e.TaskStatus)
                .HasMaxLength(30)
                .HasDefaultValueSql("'NOT_STARTED'::character varying")
                .HasComment("NOT_STARTED, RUNNING, COMPLETED, CLOSED — tracked per item independently")
                .HasColumnName("task_status");
            entity.Property(e => e.WorkData)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("work_data");
            entity.Property(e => e.WorkspaceTaskId).HasColumnName("workspace_task_id");

            entity.HasOne(d => d.AssignedUser).WithMany(p => p.TrnWorkspaceTaskItems)
                .HasForeignKey(d => d.AssignedUserId)
                .HasConstraintName("fk_wti_assigned_user");

            entity.HasOne(d => d.Job).WithMany(p => p.TrnWorkspaceTaskItems)
                .HasForeignKey(d => d.JobId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_wti_job");

            entity.HasOne(d => d.JobItem).WithMany(p => p.TrnWorkspaceTaskItems)
                .HasForeignKey(d => d.JobItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_wti_job_item");

            entity.HasOne(d => d.ParentTaskItem).WithMany(p => p.InverseParentTaskItem)
                .HasForeignKey(d => d.ParentTaskItemId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_wti_parent_task_item");

            entity.HasOne(d => d.WorkspaceTask).WithMany(p => p.TrnWorkspaceTaskItems)
                .HasForeignKey(d => d.WorkspaceTaskId)
                .HasConstraintName("fk_wti_workspace_task");
        });

        modelBuilder.Entity<TxnNotification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("txn_notification_pkey");

            entity.ToTable("txn_notification", "press_db", tb => tb.HasComment("Internal notifications sent to roles (SALES, MANAGEMENT, ADMIN) or specific users. Used for estimation alerts, job updates, etc."));

            entity.HasIndex(e => e.CreatedAt, "idx_notif_created").IsDescending();

            entity.HasIndex(e => e.ReferenceNo, "idx_notif_ref");

            entity.HasIndex(e => e.TargetRole, "idx_notif_role");

            entity.HasIndex(e => e.NotificationType, "idx_notif_type");

            entity.HasIndex(e => e.IsRead, "idx_notif_unread").HasFilter("(is_read = false)");

            entity.HasIndex(e => e.TargetUserId, "idx_notif_user");

            entity.Property(e => e.NotificationId).HasColumnName("notification_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.IsRead)
                .HasDefaultValue(false)
                .HasColumnName("is_read");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.NotificationType)
                .HasMaxLength(50)
                .HasColumnName("notification_type");
            entity.Property(e => e.ReadAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("read_at");
            entity.Property(e => e.ReferenceNo)
                .HasMaxLength(50)
                .HasColumnName("reference_no");
            entity.Property(e => e.TargetRole)
                .HasMaxLength(50)
                .HasColumnName("target_role");
            entity.Property(e => e.TargetUserId).HasColumnName("target_user_id");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");
        });

        modelBuilder.Entity<TxnUserActivity>(entity =>
        {
            entity.HasKey(e => e.ActivityId).HasName("txn_user_activity_pkey");

            entity.ToTable("txn_user_activity", "press_db", tb => tb.HasComment("Logs all user activities â€” estimation sends, WhatsApp shares, prints, logins, etc."));

            entity.HasIndex(e => e.ActivityType, "idx_ua_activity_type");

            entity.HasIndex(e => e.CreatedAt, "idx_ua_created_at").IsDescending();

            entity.HasIndex(e => e.ReferenceNo, "idx_ua_reference_no");

            entity.Property(e => e.ActivityId).HasColumnName("activity_id");
            entity.Property(e => e.ActivityType)
                .HasMaxLength(50)
                .HasColumnName("activity_type");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .HasDefaultValueSql("'system'::character varying")
                .HasColumnName("created_by");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ReferenceNo)
                .HasMaxLength(50)
                .HasColumnName("reference_no");
        });

        modelBuilder.Entity<UserLoginLog>(entity =>
        {
            entity.HasKey(e => e.Logid).HasName("user_login_log_pkey");

            entity.ToTable("user_login_log", "press_db");

            entity.HasIndex(e => e.Channel, "idx_login_log_channel");

            entity.HasIndex(e => e.Loginat, "idx_login_log_login_at");

            entity.HasIndex(e => e.Userid, "idx_login_log_user");

            entity.Property(e => e.Logid).HasColumnName("logid");
            entity.Property(e => e.Channel)
                .HasMaxLength(20)
                .HasColumnName("channel");
            entity.Property(e => e.Deviceid)
                .HasMaxLength(200)
                .HasColumnName("deviceid");
            entity.Property(e => e.Ipaddress)
                .HasMaxLength(45)
                .HasColumnName("ipaddress");
            entity.Property(e => e.Loginat)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("loginat");
            entity.Property(e => e.Logoutat)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("logoutat");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.User).WithMany(p => p.UserLoginLogs)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_login_log_user");
        });

        modelBuilder.Entity<VwJobCostingMasterJson>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_job_costing_master_json", "press_db");

            entity.Property(e => e.JobCostingMasterJson)
                .HasColumnType("jsonb")
                .HasColumnName("job_costing_master_json");
        });

        modelBuilder.Entity<VwMstItem>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_mst_items", "press_db");

            entity.Property(e => e.CurrentStock)
                .HasPrecision(14, 2)
                .HasColumnName("current_stock");
            entity.Property(e => e.GstRate)
                .HasPrecision(5, 2)
                .HasColumnName("gst_rate");
            entity.Property(e => e.HsnCode)
                .HasColumnType("character varying")
                .HasColumnName("hsn_code");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.ItemCategory)
                .HasColumnType("character varying")
                .HasColumnName("item_category");
            entity.Property(e => e.ItemCode)
                .HasColumnType("character varying")
                .HasColumnName("item_code");
            entity.Property(e => e.ItemDescription).HasColumnName("item_description");
            entity.Property(e => e.ItemGroup).HasColumnName("item_group");
            entity.Property(e => e.ItemId).HasColumnName("item_id");
            entity.Property(e => e.ItemName)
                .HasColumnType("character varying")
                .HasColumnName("item_name");
            entity.Property(e => e.LastPurchaseDate).HasColumnName("last_purchase_date");
            entity.Property(e => e.LastPurchaseRate)
                .HasPrecision(14, 2)
                .HasColumnName("last_purchase_rate");
            entity.Property(e => e.PurchaseRate).HasColumnName("purchase_rate");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.ReorderLevel)
                .HasPrecision(14, 2)
                .HasColumnName("reorder_level");
            entity.Property(e => e.SourceId).HasColumnName("source_id");
            entity.Property(e => e.SourceTable).HasColumnName("source_table");
            entity.Property(e => e.Uom)
                .HasMaxLength(20)
                .HasColumnName("uom");
        });
        modelBuilder.HasSequence("mst_process_processid_seq", "press_db");

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
