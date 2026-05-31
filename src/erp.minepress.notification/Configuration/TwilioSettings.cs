namespace erp.minepress.notification.Configuration;

public class TwilioSettings
{
    public const string SectionName = "Notification:Twilio";

    public string AccountSid { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
    public string SmsFromNumber { get; set; } = string.Empty;
    public string WhatsAppFromNumber { get; set; } = "whatsapp:+14155238886";
}
