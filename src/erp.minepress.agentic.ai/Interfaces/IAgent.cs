using erp.minepress.agentic.ai.Models;

namespace erp.minepress.agentic.ai.Interfaces;

public interface IAgent
{
    string AgentName { get; }
    IReadOnlyList<string> SupportedIntents { get; }
    Task<AgentResult> ExecuteAsync(string tool, Dictionary<string, object?> parameters, CancellationToken cancellationToken = default);
}
