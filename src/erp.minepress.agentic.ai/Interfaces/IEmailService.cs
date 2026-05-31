namespace erp.minepress.agentic.ai.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, byte[]? attachment = null, CancellationToken cancellationToken = default);
}
