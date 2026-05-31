using System.Text.RegularExpressions;
using erp.minepress.agentic.ai.Interfaces;
using Microsoft.Extensions.Logging;

namespace erp.minepress.agentic.ai.Services;

public class AgentRouter : IAgentRouter
{
    private readonly Dictionary<string, IAgent> _agentsByName;
    private readonly Dictionary<string, IAgent> _agentsByIntent;
    private readonly ILogger<AgentRouter> _logger;

    public AgentRouter(IEnumerable<IAgent> agents, ILogger<AgentRouter> logger)
    {
        _logger = logger;
        _agentsByName = new Dictionary<string, IAgent>(StringComparer.OrdinalIgnoreCase);
        _agentsByIntent = new Dictionary<string, IAgent>(StringComparer.OrdinalIgnoreCase);

        foreach (var agent in agents)
        {
            _agentsByName[agent.AgentName] = agent;
            foreach (var intent in agent.SupportedIntents)
            {
                _agentsByIntent[intent] = agent;
                // Also register normalized form for flexible matching
                var normalized = Normalize(intent);
                _agentsByIntent.TryAdd(normalized, agent);
            }
        }

        _logger.LogInformation("AgentRouter initialized with {Count} agent(s): {Agents}",
            _agentsByName.Count, string.Join(", ", _agentsByName.Keys));
    }

    public IAgent? ResolveAgent(string agentName)
    {
        if (string.IsNullOrWhiteSpace(agentName))
            return null;

        _agentsByName.TryGetValue(agentName, out var agent);
        if (agent is null)
            _logger.LogWarning("No agent found for name: {AgentName}", agentName);
        return agent;
    }

    public IAgent? ResolveAgentByIntent(string intent)
    {
        if (string.IsNullOrWhiteSpace(intent))
            return null;

        if (_agentsByIntent.TryGetValue(intent, out var agent))
            return agent;

        // Try normalized form
        var normalized = Normalize(intent);
        if (_agentsByIntent.TryGetValue(normalized, out agent))
            return agent;

        _logger.LogWarning("No agent found for intent: {Intent}", intent);
        return null;
    }

    public IReadOnlyList<string> GetRegisteredAgentNames() => _agentsByName.Keys.ToList();

    private static string Normalize(string name) =>
        Regex.Replace(name, @"[\s_\-]", "").ToLowerInvariant();
}
