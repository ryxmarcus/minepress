using erp.minepress.agentic.ai.Interfaces;
using Microsoft.Extensions.Logging;

namespace erp.minepress.agentic.ai.Services;

public class PdfService : IPdfService
{
    private readonly ILogger<PdfService> _logger;

    public PdfService(ILogger<PdfService> logger)
    {
        _logger = logger;
    }

    public Task<byte[]> GeneratePdfAsync<T>(string templateName, T model, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("PdfService is a stub. Configure a PDF library (e.g., QuestPDF, iTextSharp) to enable PDF generation.");
        return Task.FromResult(Array.Empty<byte>());
    }
}
