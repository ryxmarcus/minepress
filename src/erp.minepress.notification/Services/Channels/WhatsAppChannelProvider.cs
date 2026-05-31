using erp.minepress.notification.Configuration;
using erp.minepress.notification.Interfaces;
using erp.minepress.notification.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace erp.minepress.notification.Services.Channels;

public class WhatsAppChannelProvider : INotificationChannelProvider
{
    private readonly TwilioSettings _settings;
    private readonly ILogger<WhatsAppChannelProvider> _logger;

    public WhatsAppChannelProvider(IOptions<TwilioSettings> settings, ILogger<WhatsAppChannelProvider> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public NotificationChannel Channel => NotificationChannel.WhatsApp;

    public async Task<NotificationResult> SendAsync(NotificationRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            TwilioClient.Init(_settings.AccountSid, _settings.AuthToken);

            var toNumber = request.Recipient.StartsWith("whatsapp:")
                ? request.Recipient
                : $"whatsapp:{request.Recipient}";

            var messageResource = await MessageResource.CreateAsync(
                to: new PhoneNumber(toNumber),
                from: new PhoneNumber(_settings.WhatsAppFromNumber),
                body: request.Body
            );

            _logger.LogInformation("WhatsApp sent to {Recipient}, SID: {Sid}", request.Recipient, messageResource.Sid);
            return NotificationResult.Success(NotificationChannel.WhatsApp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send WhatsApp to {Recipient}", request.Recipient);
            return NotificationResult.Failure(NotificationChannel.WhatsApp, ex.Message);
        }
    }
}
