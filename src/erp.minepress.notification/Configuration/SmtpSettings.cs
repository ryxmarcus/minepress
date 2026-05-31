namespace erp.minepress.notification.Configuration;

public class SmtpSettings
{
    public const string SectionName = "Notification:Smtp";

    public string Host { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public bool UseSsl { get; set; } = true;
    public string FromName { get; set; } = "MinePress ERP";
    public string FromEmail { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
