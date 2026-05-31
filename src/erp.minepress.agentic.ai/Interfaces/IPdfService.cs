namespace erp.minepress.agentic.ai.Interfaces;

public interface IPdfService
{
    Task<byte[]> GeneratePdfAsync<T>(string templateName, T model, CancellationToken cancellationToken = default);
}
