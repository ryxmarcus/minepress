using erp.minepress.agentic.ai.Interfaces;
using Microsoft.Extensions.Logging;

namespace erp.minepress.agentic.ai.Services;

public class SpeechToTextService : ISpeechToTextService
{
    private readonly ILogger<SpeechToTextService> _logger;

    public SpeechToTextService(ILogger<SpeechToTextService> logger)
    {
        _logger = logger;
    }

    public Task<string> ConvertSpeechToTextAsync(Stream audioStream, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("SpeechToTextService is a stub. Configure a speech provider (e.g., Azure Cognitive Services, OpenAI Whisper) to enable this feature.");
        return Task.FromResult("Speech-to-text is not yet configured. Please provide text input.");
    }
}
