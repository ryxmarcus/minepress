namespace erp.minepress.agentic.ai.Models;

public class IntentResult
{
    public string Intent { get; set; } = string.Empty;
    public string Agent { get; set; } = string.Empty;
    public string Tool { get; set; } = string.Empty;
    public Dictionary<string, object?> Parameters { get; set; } = [];
    public string? ClarificationNeeded { get; set; }
    public decimal Confidence { get; set; }
}
