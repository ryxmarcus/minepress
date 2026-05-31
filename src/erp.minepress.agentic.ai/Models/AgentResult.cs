namespace erp.minepress.agentic.ai.Models;

public class AgentResult
{
    public bool Success { get; set; }
    public string? ToolExecuted { get; set; }
    public object? Data { get; set; }
    public string? Message { get; set; }
    public string? ErrorMessage { get; set; }

    public static AgentResult Ok(object? data, string tool, string? message = null) => new()
    {
        Success = true,
        Data = data,
        ToolExecuted = tool,
        Message = message
    };

    public static AgentResult Fail(string error) => new()
    {
        Success = false,
        ErrorMessage = error
    };
}
