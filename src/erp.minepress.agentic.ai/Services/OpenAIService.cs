using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using erp.minepress.agentic.ai.Configuration;
using erp.minepress.agentic.ai.Interfaces;
using erp.minepress.agentic.ai.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace erp.minepress.agentic.ai.Services;

public class OpenAIService : IOpenAIService
{
    private readonly HttpClient _httpClient;
    private readonly OpenAISettings _settings;
    private readonly ILogger<OpenAIService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public OpenAIService(HttpClient httpClient, IOptions<OpenAISettings> settings, ILogger<OpenAIService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;

        _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
    }

    public async Task<IntentResult> DetectIntentAsync(string userInput, IReadOnlyList<ToolDefinition> availableTools, CancellationToken cancellationToken = default)
    {
        return await DetectIntentAsync(userInput, availableTools, null, cancellationToken);
    }

    public async Task<IntentResult> DetectIntentAsync(string userInput, IReadOnlyList<ToolDefinition> availableTools, IReadOnlyList<ConversationMessage>? conversationHistory, CancellationToken cancellationToken = default)
    {
        var toolList = string.Join("\n", availableTools.Select(t => $"- {t.Name}: {t.Description}"));

        var systemPrompt = $$"""
            You are an AI Decision Engine for a Printing ERP system called MinePress.

            Your responsibilities:
            1. Understand user intent from their input (text may be in English, Hindi, or Hinglish)
            2. Extract entities and parameters
            3. Select the correct tool from the available tools list below
            4. Fill parameters from the user input
            5. Ask clarification if required parameters are missing
            6. Never guess required values
            7. When user asks for "last", "recent", "latest" records, use the list/GetAll tool with limit=1
            8. Use conversation history to resolve pronouns and context from prior messages

            CRITICAL RULES FOR TOOL NAMES:
            - The "tool" field MUST exactly match one of the tool names from the "Available tools" list below
            - Tool names are in PascalCase (e.g., "GetAllCustomers", "SearchCustomer", "GetTopCustomers")
            - Do NOT use snake_case for tool names. Intent is snake_case, tool is PascalCase.
            - If no tool matches exactly, use the closest matching tool name from the list
            - If the user query does not match any business data operation, use intent "greeting" with tool "" and confidence 0.1

            CONVERSATIONAL INPUTS:
            - For greetings (hello, hi, hey, namaste), use intent "greeting", agent "IntentAgent", tool "", confidence 0.1
            - For thanks (thank you, thanks, dhanyawad), use intent "thanks", agent "IntentAgent", tool "", confidence 0.1
            - For help requests (help, what can you do), use intent "help", agent "IntentAgent", tool "", confidence 0.1
            - Set clarificationNeeded to a friendly response message for these conversational inputs

            Available tools:
            {{toolList}}

            Respond ONLY with valid JSON in this exact format:
            {
              "intent": "<intent_name_snake_case>",
              "agent": "<AgentName in PascalCase>",
              "tool": "<ToolName from Available tools list in PascalCase, or empty string for conversational>",
              "parameters": { <extracted parameters as key-value pairs> },
              "clarificationNeeded": "<question to ask user if required params missing, friendly message for chat, or null>",
              "confidence": <0.0 to 1.0>
            }
            """;

        var messages = new List<ConversationMessage>
        {
            ConversationMessage.System(systemPrompt)
        };

        // Include conversation history for multi-turn context
        if (conversationHistory is { Count: > 0 })
        {
            // Limit to last 10 messages to stay within token budget
            var recent = conversationHistory.Count > 10
                ? conversationHistory.Skip(conversationHistory.Count - 10).ToList()
                : conversationHistory;

            messages.AddRange(recent);
        }

        messages.Add(ConversationMessage.User(userInput));

        var responseText = await ChatAsync(messages, cancellationToken);

        try
        {
            var cleaned = responseText.Trim();
            if (cleaned.StartsWith("```"))
            {
                cleaned = cleaned.Split('\n', 2).Length > 1 ? cleaned.Split('\n', 2)[1] : cleaned;
                cleaned = cleaned.TrimEnd('`').Trim();
            }

            var result = JsonSerializer.Deserialize<IntentResult>(cleaned, JsonOptions);
            return result ?? new IntentResult { Intent = "unknown", Agent = "IntentAgent", Confidence = 0 };
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse LLM intent response: {Response}", responseText);
            return new IntentResult
            {
                Intent = "unknown",
                Agent = "IntentAgent",
                Confidence = 0,
                ClarificationNeeded = responseText
            };
        }
    }

    public async Task<string> ChatAsync(IReadOnlyList<ConversationMessage> messages, CancellationToken cancellationToken = default)
    {
        var requestBody = new
        {
            model = _settings.Model,
            messages = messages.Select(m => new { role = m.Role, content = m.Content }),
            temperature = _settings.Temperature,
            max_tokens = _settings.MaxTokens
        };

        var json = JsonSerializer.Serialize(requestBody, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _logger.LogDebug("Sending request to OpenAI model {Model}", _settings.Model);

        var response = await _httpClient.PostAsync("/v1/chat/completions", content, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("OpenAI API error {StatusCode}: {Body}", response.StatusCode, responseBody);
            throw new InvalidOperationException($"OpenAI API returned {response.StatusCode}: {responseBody}");
        }

        using var doc = JsonDocument.Parse(responseBody);
        var messageContent = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return messageContent ?? string.Empty;
    }
}
