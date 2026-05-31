using erp.minepress.agentic.ai.Interfaces;
using erp.minepress.agentic.ai.Models;
using Microsoft.Extensions.Logging;

namespace erp.minepress.agentic.ai.Agents;

public class IntentAgent : BaseAgent
{
    private readonly IOpenAIService _openAIService;
    private readonly IToolDefinitionProvider _toolProvider;

    public IntentAgent(
        ILogger<IntentAgent> logger,
        IOpenAIService openAIService,
        IToolDefinitionProvider toolProvider) : base(logger)
    {
        _openAIService = openAIService;
        _toolProvider = toolProvider;
    }

    public override string AgentName => "IntentAgent";

    public override IReadOnlyList<string> SupportedIntents => [];

    public async Task<IntentResult> DetectIntentAsync(string userInput, CancellationToken cancellationToken = default)
    {
        return await DetectIntentAsync(userInput, null, cancellationToken);
    }

    public async Task<IntentResult> DetectIntentAsync(string userInput, IReadOnlyList<ConversationMessage>? conversationHistory, CancellationToken cancellationToken = default)
    {
        var allTools = _toolProvider.GetModules()
            .SelectMany(m => m.Tools)
            .ToList();

        Logger.LogInformation("IntentAgent detecting intent for input: {Input}", userInput);

        var result = await _openAIService.DetectIntentAsync(userInput, allTools, conversationHistory, cancellationToken);

        Logger.LogInformation("IntentAgent detected intent: {Intent}, agent: {Agent}, tool: {Tool}, confidence: {Confidence}",
            result.Intent, result.Agent, result.Tool, result.Confidence);

        return result;
    }

    public override Task<AgentResult> ExecuteAsync(string tool, Dictionary<string, object?> parameters, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(AgentResult.Fail("IntentAgent does not execute tools directly. Use DetectIntentAsync instead."));
    }
}
