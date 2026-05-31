namespace erp.minepress.agentic.ai.Configuration;

public class OpenAISettings
{
    public const string SectionName = "OpenAI";

    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-4o";
    public string BaseUrl { get; set; } = "https://api.openai.com/v1";
    public double Temperature { get; set; } = 0.2;
    public int MaxTokens { get; set; } = 2048;
}
