namespace erp.minepress.agentic.ai.Models;

public class AiLogEntry
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? InputType { get; set; }
    public string? UserQuery { get; set; }
    public string? UserName { get; set; }
    public string? Intent { get; set; }
    public string? Agent { get; set; }
    public string? Tool { get; set; }
    public string? OutputFormat { get; set; }
    public string? DeliveryChannel { get; set; }
    public string? Error { get; set; }
    public long DurationMs { get; set; }
    public decimal? Confidence { get; set; }
}
