using erp.minepress.notification.Configuration;
using erp.minepress.notification.Interfaces;
using erp.minepress.notification.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Security.Cryptography;
using System.Text;

namespace erp.minepress.notification.Services.Channels;

public class EmailChannelProvider : INotificationChannelProvider
{
    private readonly SmtpSettings _settings;
    private readonly ILogger<EmailChannelProvider> _logger;

    public EmailChannelProvider(IOptions<SmtpSettings> settings, ILogger<EmailChannelProvider> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public NotificationChannel Channel => NotificationChannel.Email;

    public async Task<NotificationResult> SendAsync(NotificationRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
            message.To.Add(MailboxAddress.Parse(request.Recipient));
            message.Subject = request.Subject ?? "MinePress ERP Notification";

            ApplyThreadHeaders(message, request.EmailThreadKey);

            var bodyBuilder = new BodyBuilder { HtmlBody = request.Body };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();

            var socketOptions = _settings.Port == 465
                ? SecureSocketOptions.SslOnConnect
                : SecureSocketOptions.StartTls;

            await client.ConnectAsync(_settings.Host, _settings.Port, socketOptions, cancellationToken);

            if (!string.IsNullOrEmpty(_settings.Username))
            {
                await client.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);
            }

            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("Email sent to {Recipient}: {Subject}", request.Recipient, request.Subject);
            return NotificationResult.Success(NotificationChannel.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Recipient}", request.Recipient);
            return NotificationResult.Failure(NotificationChannel.Email, ex.Message);
        }
    }

    private static void ApplyThreadHeaders(MimeMessage message, string? emailThreadKey)
    {
        if (string.IsNullOrWhiteSpace(emailThreadKey))
        {
            return;
        }

        var rootMessageId = BuildRootMessageId(emailThreadKey);
        message.Headers["In-Reply-To"] = rootMessageId;
        message.Headers["References"] = rootMessageId;
        message.Headers["Thread-Topic"] = emailThreadKey;
    }

    private static string BuildRootMessageId(string emailThreadKey)
    {
        var normalized = emailThreadKey.Trim().ToUpperInvariant();
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
        var token = Convert.ToHexString(hash)[..24].ToLowerInvariant();
        return $"<{token}@minepress.thread>";
    }
}
