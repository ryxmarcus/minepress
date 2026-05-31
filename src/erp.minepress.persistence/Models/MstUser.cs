using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstUser
{
    public long Userid { get; set; }

    public string Usercode { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string Passwordhash { get; set; } = null!;

    public int Locationid { get; set; }

    public string Name { get; set; } = null!;

    public string? Mobileno { get; set; }

    public string? Emailid { get; set; }

    public long Departmentid { get; set; }

    public long Designationid { get; set; }

    public long? Reportinguserid { get; set; }

    public string? Employeecode { get; set; }

    public DateOnly? Joiningdate { get; set; }

    public DateOnly? Exitdate { get; set; }

    public int? Approvallevel { get; set; }

    public decimal? Approvallimit { get; set; }

    public bool? Canoverride { get; set; }

    public bool? Issystemadmin { get; set; }

    public bool? Isproductionuser { get; set; }

    public bool? Isapprovaluser { get; set; }

    public bool? Isclientuser { get; set; }

    public bool? Ismobileaccessallowed { get; set; }

    public bool? Iswebaccessallowed { get; set; }

    public string? Deviceid { get; set; }

    public string? Devicetype { get; set; }

    public string? Deviceosversion { get; set; }

    public string? Appversion { get; set; }

    public string? Registredmobile { get; set; }

    public bool? Isdevicebindingenabled { get; set; }

    public bool? Ismultideviceallowed { get; set; }

    public DateTime? Lastdeviceloginat { get; set; }

    public string? Lastdeviceip { get; set; }

    public string? Refreshtoken { get; set; }

    public DateTime? Refreshtokenexpiry { get; set; }

    public int? Mobilesessiontimeoutmin { get; set; }

    public bool? Isotprequired { get; set; }

    public int? Otp { get; set; }

    public int? Otpexpiryminutes { get; set; }

    public DateTime? Lastotpverifiedat { get; set; }

    public bool? Isgeorestrictionenabled { get; set; }

    public string? Allowedgeolocation { get; set; }

    public string? Allowediprange { get; set; }

    public int? Allowedlocationrangemeter { get; set; }

    public bool? Isactive { get; set; }

    public bool? Islocked { get; set; }

    public bool? Isdeleted { get; set; }

    public bool? Ispermissiononleave { get; set; }

    public DateTime? Lastloginat { get; set; }

    public DateTime? Lastpasswordchange { get; set; }

    public int? Failedlogincount { get; set; }

    public DateTime? Lastfailedloginat { get; set; }

    public TimeOnly? Accessfromtime { get; set; }

    public TimeOnly? Accesstotime { get; set; }

    public string Createdby { get; set; } = null!;

    public DateTime? Createdat { get; set; }

    public string? Updatedby { get; set; }

    public DateTime? Updatedat { get; set; }

    public long? EmployeeId { get; set; }

    /// <summary>
    /// FK to shift type - used by AI agent to set accessfromtime/accesstotime automatically
    /// </summary>
    public int? ShiftTypeId { get; set; }

    /// <summary>
    /// User configuration health score (0-100) calculated by AI Smart Agent
    /// </summary>
    public int? AiHealthScore { get; set; }

    /// <summary>
    /// Last time AI agent reviewed this user configuration
    /// </summary>
    public DateTime? AiLastReviewedAt { get; set; }

    /// <summary>
    /// Number of active AI alerts for this user
    /// </summary>
    public int? AiAlertCount { get; set; }

    /// <summary>
    /// Whether AI agent auto-configured roles/permissions based on dept+designation
    /// </summary>
    public bool? AiAutoConfigured { get; set; }

    /// <summary>
    /// Primary company assignment for multi-company ERP setups
    /// </summary>
    public int? CompanyId { get; set; }

    /// <summary>
    /// AI classification: INTERNAL, CLIENT, VENDOR, CONTRACTOR, TEMPORARY
    /// </summary>
    public string? UserCategory { get; set; }

    public DateTime? PasswordExpiresAt { get; set; }

    public bool? MustChangePassword { get; set; }

    /// <summary>
    /// JSON array of previous password hashes to prevent reuse
    /// </summary>
    public string? PasswordHistory { get; set; }

    /// <summary>
    /// Maximum simultaneous login sessions allowed
    /// </summary>
    public int? MaxConcurrentSessions { get; set; }

    /// <summary>
    /// User classification: EMPLOYEE, CUSTOMER, VENDOR, ADMIN, OTHER. Determines entity linkage and access scope
    /// </summary>
    public string UserType { get; set; } = null!;

    /// <summary>
    /// Reference ID to source entity: employee_id (mst_employee), party_id (mst_party), or other entity based on user_type
    /// </summary>
    public long? RefId { get; set; }

    public virtual MstCompany? Company { get; set; }

    public virtual MstDepartment Department { get; set; } = null!;

    public virtual MstDesignation Designation { get; set; } = null!;

    public virtual MstEmployee? Employee { get; set; }

    public virtual ICollection<HrReimbursement> HrReimbursementApprovedByNavigations { get; set; } = new List<HrReimbursement>();

    public virtual ICollection<HrReimbursement> HrReimbursementCreatedByNavigations { get; set; } = new List<HrReimbursement>();

    public virtual ICollection<HybJobRateCalculator> HybJobRateCalculators { get; set; } = new List<HybJobRateCalculator>();

    public virtual ICollection<MstUser> InverseReportinguser { get; set; } = new List<MstUser>();

    public virtual MstLocation Location { get; set; } = null!;

    public virtual ICollection<MapUserPermission> MapUserPermissions { get; set; } = new List<MapUserPermission>();

    public virtual ICollection<MapUserRole> MapUserRoles { get; set; } = new List<MapUserRole>();

    public virtual ICollection<MstNotificationPreference> MstNotificationPreferences { get; set; } = new List<MstNotificationPreference>();

    public virtual ICollection<MstWorkflowStep> MstWorkflowSteps { get; set; } = new List<MstWorkflowStep>();

    public virtual MstWorkspaceConfig? MstWorkspaceConfig { get; set; }

    public virtual MstUser? Reportinguser { get; set; }

    public virtual MstShiftType? ShiftType { get; set; }

    public virtual ICollection<TrnAiAgentActivity> TrnAiAgentActivities { get; set; } = new List<TrnAiAgentActivity>();

    public virtual ICollection<TrnChallan> TrnChallans { get; set; } = new List<TrnChallan>();

    public virtual ICollection<TrnEnquiry> TrnEnquiries { get; set; } = new List<TrnEnquiry>();

    public virtual ICollection<TrnGatePass> TrnGatePasses { get; set; } = new List<TrnGatePass>();

    public virtual ICollection<TrnJob> TrnJobAssignedToNavigations { get; set; } = new List<TrnJob>();

    public virtual ICollection<TrnJob> TrnJobCreatedByNavigations { get; set; } = new List<TrnJob>();

    public virtual ICollection<TrnJobItem> TrnJobItems { get; set; } = new List<TrnJobItem>();

    public virtual ICollection<TrnNotification> TrnNotifications { get; set; } = new List<TrnNotification>();

    public virtual ICollection<TrnPayment> TrnPayments { get; set; } = new List<TrnPayment>();

    public virtual ICollection<TrnQuotationItem> TrnQuotationItems { get; set; } = new List<TrnQuotationItem>();

    public virtual ICollection<TrnQuotation> TrnQuotations { get; set; } = new List<TrnQuotation>();

    public virtual ICollection<TrnReceipt> TrnReceipts { get; set; } = new List<TrnReceipt>();

    public virtual ICollection<TrnSalesInvoice> TrnSalesInvoices { get; set; } = new List<TrnSalesInvoice>();

    public virtual ICollection<TrnUserNotification> TrnUserNotifications { get; set; } = new List<TrnUserNotification>();

    public virtual ICollection<TrnWorkspaceTask> TrnWorkspaceTaskAssignedByNavigations { get; set; } = new List<TrnWorkspaceTask>();

    public virtual ICollection<TrnWorkspaceTaskItem> TrnWorkspaceTaskItems { get; set; } = new List<TrnWorkspaceTaskItem>();

    public virtual ICollection<TrnWorkspaceTask> TrnWorkspaceTaskUsers { get; set; } = new List<TrnWorkspaceTask>();

    public virtual ICollection<UserLoginLog> UserLoginLogs { get; set; } = new List<UserLoginLog>();
}
