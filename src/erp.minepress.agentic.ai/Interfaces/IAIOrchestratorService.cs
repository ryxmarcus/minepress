using erp.minepress.agentic.ai.Models;

namespace erp.minepress.agentic.ai.Interfaces;

public interface IAIOrchestratorService
{
    Task<AIResponse> ProcessAsync(AIRequest request, CancellationToken cancellationToken = default);
}
