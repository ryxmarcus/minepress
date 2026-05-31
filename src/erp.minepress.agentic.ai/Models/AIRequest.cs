namespace erp.minepress.agentic.ai.Models;

public class AIRequest
{
    public string InputType { get; set; } = "text";
    public string InputData { get; set; } = string.Empty;
    public string? OutputFormat { get; set; }
    public string? DeliveryChannel { get; set; }
    public string? DeliveryAddress { get; set; }
    /// <summary>
    /// Optional agent name to route directly (e.g. "JobAgent", "CostingAgent").
    /// When null or "auto", the IntentAgent detects the best agent automatically.
    /// </summary>
    public string? SelectedAgent { get; set; }

    /// <summary>
    /// Logged-in user's display name, populated server-side from session.
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// Previous conversation messages for multi-turn context.
    /// Each entry has Role ("user" or "assistant") and Content.
    /// </summary>
    public List<ConversationMessage>? ConversationHistory { get; set; }
}
