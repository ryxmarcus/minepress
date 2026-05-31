namespace erp.minepress.agentic.ai.Interfaces;

public interface ISpeechToTextService
{
    Task<string> ConvertSpeechToTextAsync(Stream audioStream, CancellationToken cancellationToken = default);
}
