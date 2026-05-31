namespace erp.minepress.agentic.ai.Models;

public class AIResponse
{
    public string Intent { get; set; } = string.Empty;
    public string? Agent { get; set; }
    public string? ToolExecuted { get; set; }
    public string OutputFormat { get; set; } = "text";
    public object? Data { get; set; }
    public byte[]? PdfFile { get; set; }
    public string Status { get; set; } = "success";
    public string? Message { get; set; }
    public string? DeliveryChannel { get; set; }
    public bool DeliveryCompleted { get; set; }
}
