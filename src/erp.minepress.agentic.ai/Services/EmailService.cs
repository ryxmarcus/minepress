using erp.minepress.agentic.ai.Interfaces;
using Microsoft.Extensions.Logging;

namespace erp.minepress.agentic.ai.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string to, string subject, string body, byte[]? attachment = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("EmailService: Would send email to {To} with subject '{Subject}'. Configure SMTP or SendGrid to enable.", to, subject);
        return Task.CompletedTask;
    }
}
