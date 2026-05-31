namespace erp.minepress.agentic.ai.Models;

public class ConversationMessage
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;

    public static ConversationMessage System(string content) => new() { Role = "system", Content = content };
    public static ConversationMessage User(string content) => new() { Role = "user", Content = content };
    public static ConversationMessage Assistant(string content) => new() { Role = "assistant", Content = content };
}
