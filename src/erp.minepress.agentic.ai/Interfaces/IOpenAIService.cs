using erp.minepress.agentic.ai.Models;

namespace erp.minepress.agentic.ai.Interfaces;

public interface IOpenAIService
{
    Task<IntentResult> DetectIntentAsync(string userInput, IReadOnlyList<ToolDefinition> availableTools, CancellationToken cancellationToken = default);
    Task<IntentResult> DetectIntentAsync(string userInput, IReadOnlyList<ToolDefinition> availableTools, IReadOnlyList<ConversationMessage>? conversationHistory, CancellationToken cancellationToken = default);
    Task<string> ChatAsync(IReadOnlyList<ConversationMessage> messages, CancellationToken cancellationToken = default);
}
