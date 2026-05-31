using erp.minepress.notification.Configuration;
using erp.minepress.notification.Interfaces;
using erp.minepress.notification.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace erp.minepress.notification.Services.Channels;

public class SmsChannelProvider : INotificationChannelProvider
{
    private readonly TwilioSettings _settings;
    private readonly ILogger<SmsChannelProvider> _logger;

    public SmsChannelProvider(IOptions<TwilioSettings> settings, ILogger<SmsChannelProvider> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public NotificationChannel Channel => NotificationChannel.Sms;

    public async Task<NotificationResult> SendAsync(NotificationRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            TwilioClient.Init(_settings.AccountSid, _settings.AuthToken);

            var messageResource = await MessageResource.CreateAsync(
                to: new PhoneNumber(request.Recipient),
                from: new PhoneNumber(_settings.SmsFromNumber),
                body: request.Body
            );

            _logger.LogInformation("SMS sent to {Recipient}, SID: {Sid}", request.Recipient, messageResource.Sid);
            return NotificationResult.Success(NotificationChannel.Sms);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {Recipient}", request.Recipient);
            return NotificationResult.Failure(NotificationChannel.Sms, ex.Message);
        }
    }
}
