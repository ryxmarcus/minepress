namespace erp.minepress.agentic.ai.Interfaces;

public interface IAgentRouter
{
    IAgent? ResolveAgent(string agentName);
    IAgent? ResolveAgentByIntent(string intent);
    IReadOnlyList<string> GetRegisteredAgentNames();
}
