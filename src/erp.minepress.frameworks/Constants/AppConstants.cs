namespace erp.minepress.frameworks.Constants;

public static class AppConstants
{
    public const string DefaultSchema = "press_db";
    public const string DefaultCurrency = "INR";
    public const int DefaultPageSize = 25;
    public const int MaxPageSize = 100;

    public static class Roles
    {
        public const string SystemAdmin = "SYSTEM_ADMIN";
        public const string ProductionUser = "PRODUCTION_USER";
        public const string SalesUser = "SALES_USER";
        public const string AccountsUser = "ACCOUNTS_USER";
        public const string ClientUser = "CLIENT_USER";
    }

    public static class Status
    {
        public const string Draft = "DRAFT";
        public const string Active = "ACTIVE";
        public const string Approved = "APPROVED";
        public const string Rejected = "REJECTED";
        public const string Completed = "COMPLETED";
        public const string Cancelled = "CANCELLED";
    }
}
