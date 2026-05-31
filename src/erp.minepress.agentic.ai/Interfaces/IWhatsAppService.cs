namespace erp.minepress.agentic.ai.Interfaces;

public interface IWhatsAppService
{
    Task SendMessageAsync(string phoneNumber, string message, byte[]? attachment = null, CancellationToken cancellationToken = default);
}
