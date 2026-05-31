using System.Diagnostics;
using System.Text.Json;
using erp.minepress.agentic.ai.Agents;
using erp.minepress.agentic.ai.Interfaces;
using erp.minepress.agentic.ai.Models;
using Microsoft.Extensions.Logging;

namespace erp.minepress.agentic.ai.Services;

public class AIOrchestratorService : IAIOrchestratorService
{
    private readonly IntentAgent _intentAgent;
    private readonly IAgentRouter _agentRouter;
    private readonly DynamicFallbackAgent _dynamicFallbackAgent;
    private readonly IPdfService _pdfService;
    private readonly IEmailService _emailService;
    private readonly IWhatsAppService _whatsAppService;
    private readonly ISpeechToTextService _speechToTextService;
    private readonly AiActivityLogger _activityLogger;
    private readonly ILogger<AIOrchestratorService> _logger;

    public AIOrchestratorService(
        IntentAgent intentAgent,
        IAgentRouter agentRouter,
        DynamicFallbackAgent dynamicFallbackAgent,
        IPdfService pdfService,
        IEmailService emailService,
        IWhatsAppService whatsAppService,
        ISpeechToTextService speechToTextService,
        AiActivityLogger activityLogger,
        ILogger<AIOrchestratorService> logger)
    {
        _intentAgent = intentAgent;
        _agentRouter = agentRouter;
        _dynamicFallbackAgent = dynamicFallbackAgent;
        _pdfService = pdfService;
        _emailService = emailService;
        _whatsAppService = whatsAppService;
        _speechToTextService = speechToTextService;
        _activityLogger = activityLogger;
        _logger = logger;
    }

    public async Task<AIResponse> ProcessAsync(AIRequest request, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        var logEntry = new AiLogEntry { InputType = request.InputType };

        try
        {
            // Step 1: Handle input (speech → text conversion if needed)
            var userInput = await ResolveInputAsync(request, cancellationToken);
            logEntry.UserQuery = userInput;
            logEntry.UserName = request.UserName;

            if (string.IsNullOrWhiteSpace(userInput))
            {
                return CreateErrorResponse("No input provided. Please type or speak your request.");
            }

            _logger.LogInformation("Processing AI request: InputType={InputType}, Input={Input}", request.InputType, userInput);

            // Step 2: Call LLM for intent detection
            var intentResult = await _intentAgent.DetectIntentAsync(userInput, request.ConversationHistory, cancellationToken);
            logEntry.Intent = intentResult.Intent;
            logEntry.Agent = intentResult.Agent;
            logEntry.Tool = intentResult.Tool;
            logEntry.Confidence = intentResult.Confidence;

            // Step 2a: Handle conversational / non-actionable intents (hello, thanks, etc.)
            if (IsConversationalIntent(intentResult))
            {
                return new AIResponse
                {
                    Intent = intentResult.Intent,
                    Agent = "IntentAgent",
                    Status = "success",
                    Message = intentResult.ClarificationNeeded
                             ?? "Hello! I'm your MinePress ERP assistant. I can help you with jobs, customers, invoices, machines, reports, and more. What would you like to know?",
                    OutputFormat = "text"
                };
            }

            // Step 2b: If a specific agent was selected, override the detected agent
            var isManualAgent = !string.IsNullOrWhiteSpace(request.SelectedAgent) &&
                                !request.SelectedAgent.Equals("auto", StringComparison.OrdinalIgnoreCase);

            if (isManualAgent)
            {
                intentResult.Agent = request.SelectedAgent!;
                logEntry.Agent = request.SelectedAgent!;
                _logger.LogInformation("Agent manually selected: {Agent}", request.SelectedAgent);
            }

            // Step 3: Check if clarification is needed
            if (!string.IsNullOrEmpty(intentResult.ClarificationNeeded) && intentResult.Confidence < 0.5m)
            {
                return new AIResponse
                {
                    Intent = intentResult.Intent,
                    Agent = intentResult.Agent,
                    Status = "clarification_needed",
                    Message = intentResult.ClarificationNeeded,
                    OutputFormat = "text"
                };
            }

            // Step 4: Route to the correct agent
            var agent = _agentRouter.ResolveAgent(intentResult.Agent)
                        ?? _agentRouter.ResolveAgentByIntent(intentResult.Intent);

            // Step 5: Execute tool via agent (with dynamic fallback)
            var toolName = !string.IsNullOrWhiteSpace(intentResult.Tool) ? intentResult.Tool : intentResult.Intent;
            AgentResult agentResult;

            if (agent is not null)
            {
                agentResult = await agent.ExecuteAsync(toolName, intentResult.Parameters, cancellationToken);

                // If the agent couldn't handle the tool, fall back to dynamic resolution
                if (!agentResult.Success && agentResult.ErrorMessage is not null &&
                    agentResult.ErrorMessage.Contains("Unknown tool", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation(
                        "Agent {Agent} could not handle tool '{Tool}', falling back to DynamicFallbackAgent",
                        agent.AgentName, toolName);

                    agentResult = await _dynamicFallbackAgent.ExecuteAsync(
                        toolName, intentResult.Parameters, cancellationToken);
                    logEntry.Agent = $"{agent.AgentName}→DynamicFallback";
                }
            }
            else
            {
                // No registered agent found — use dynamic fallback directly
                _logger.LogInformation(
                    "No agent for intent '{Intent}', using DynamicFallbackAgent for tool '{Tool}'",
                    intentResult.Intent, toolName);

                agentResult = await _dynamicFallbackAgent.ExecuteAsync(
                    toolName, intentResult.Parameters, cancellationToken);
                logEntry.Agent = "DynamicFallback";
            }

            if (!agentResult.Success)
            {
                return CreateErrorResponse(agentResult.ErrorMessage ?? "Agent execution failed.");
            }

            // Step 6: Format response
            var response = new AIResponse
            {
                Intent = intentResult.Intent,
                Agent = logEntry.Agent ?? intentResult.Agent,
                ToolExecuted = agentResult.ToolExecuted,
                OutputFormat = request.OutputFormat ?? "text",
                Data = agentResult.Data,
                Status = "success",
                Message = agentResult.Message
            };

            // Step 7: Generate PDF if requested
            if (string.Equals(request.OutputFormat, "pdf", StringComparison.OrdinalIgnoreCase))
            {
                response.PdfFile = await _pdfService.GeneratePdfAsync(
                    toolName, agentResult.Data!, cancellationToken);
            }

            // Step 8: Deliver via channel if specified
            if (!string.IsNullOrEmpty(request.DeliveryChannel) && !string.IsNullOrEmpty(request.DeliveryAddress))
            {
                await DeliverAsync(request, response, cancellationToken);
                response.DeliveryChannel = request.DeliveryChannel;
                response.DeliveryCompleted = true;
            }

            logEntry.OutputFormat = response.OutputFormat;
            logEntry.DeliveryChannel = request.DeliveryChannel;

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing AI request");
            logEntry.Error = ex.Message;
            return CreateErrorResponse($"An error occurred: {ex.Message}");
        }
        finally
        {
            sw.Stop();
            logEntry.DurationMs = sw.ElapsedMilliseconds;
            _logger.LogInformation(
                "AI Request completed: Intent={Intent}, Agent={Agent}, Tool={Tool}, Duration={Duration}ms, Error={Error}",
                logEntry.Intent, logEntry.Agent, logEntry.Tool, logEntry.DurationMs, logEntry.Error);

            // Persist activity to database for auditing and analytics
            await _activityLogger.LogActivityAsync(logEntry, cancellationToken: cancellationToken);
        }
    }

    /// <summary>
    /// Detects whether the intent is conversational (greetings, thanks, general chat)
    /// rather than an actionable ERP query.
    /// </summary>
    private static bool IsConversationalIntent(IntentResult result)
    {
        if (string.IsNullOrWhiteSpace(result.Intent))
            return true;

        var intent = result.Intent.ToLowerInvariant();

        // Explicit conversational intents
        if (intent is "greeting" or "hello" or "chat" or "thanks" or "goodbye" or "help"
            or "unknown" or "conversational" or "general" or "smalltalk" or "small_talk")
            return true;

        // No tool means it's not actionable
        if (string.IsNullOrWhiteSpace(result.Tool) && result.Confidence < 0.3m)
            return true;

        return false;
    }

    private async Task<string> ResolveInputAsync(AIRequest request, CancellationToken cancellationToken)
    {
        if (string.Equals(request.InputType, "speech", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrEmpty(request.InputData))
                return string.Empty;

            var audioBytes = Convert.FromBase64String(request.InputData);
            using var audioStream = new MemoryStream(audioBytes);
            return await _speechToTextService.ConvertSpeechToTextAsync(audioStream, cancellationToken);
        }

        return request.InputData;
    }

    private async Task DeliverAsync(AIRequest request, AIResponse response, CancellationToken cancellationToken)
    {
        var channel = request.DeliveryChannel!.ToLowerInvariant();
        var address = request.DeliveryAddress!;

        var messageBody = response.Message ?? JsonSerializer.Serialize(response.Data);

        switch (channel)
        {
            case "email":
                await _emailService.SendEmailAsync(
                    address,
                    $"MinePress ERP - {response.Intent}",
                    messageBody,
                    response.PdfFile,
                    cancellationToken);
                break;

            case "whatsapp":
                await _whatsAppService.SendMessageAsync(
                    address,
                    messageBody,
                    response.PdfFile,
                    cancellationToken);
                break;

            default:
                _logger.LogWarning("Unknown delivery channel: {Channel}", channel);
                break;
        }
    }

    private static AIResponse CreateErrorResponse(string message)
    {
        return new AIResponse
        {
            Intent = "error",
            Status = "error",
            Message = message,
            OutputFormat = "text"
        };
    }
}
