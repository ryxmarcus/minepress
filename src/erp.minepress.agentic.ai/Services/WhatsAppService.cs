using erp.minepress.agentic.ai.Interfaces;
using Microsoft.Extensions.Logging;

namespace erp.minepress.agentic.ai.Services;

public class WhatsAppService : IWhatsAppService
{
    private readonly ILogger<WhatsAppService> _logger;

    public WhatsAppService(ILogger<WhatsAppService> logger)
    {
        _logger = logger;
    }

    public Task SendMessageAsync(string phoneNumber, string message, byte[]? attachment = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("WhatsAppService: Would send message to {Phone}. Configure WhatsApp Business API to enable.", phoneNumber);
        return Task.CompletedTask;
    }
}
