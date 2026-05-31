using System.Text.Json;
using erp.minepress.agentic.ai.Interfaces;
using erp.minepress.agentic.ai.Models;
using erp.minepress.infrastructure.ErrorLogging;
using erp.minepress.notification.Interfaces;
using erp.minepress.web.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace erp.minepress.web.Controllers;

[ApiController]
[Route("api/agentic-ai")]
public class AgenticAiController : ControllerBase
{
    private readonly IAIOrchestratorService _orchestrator;
    private readonly IResponseFormatter _formatter;
    private readonly IDbContextIntentGenerator _intentGenerator;
    private readonly IToolDefinitionProvider _toolProvider;
    private readonly INotificationService _notificationService;
    private readonly ISystemErrorLogger _systemErrorLogger;
    private readonly ILogger<AgenticAiController> _logger;

    public AgenticAiController(
        IAIOrchestratorService orchestrator,
        IResponseFormatter formatter,
        IDbContextIntentGenerator intentGenerator,
        IToolDefinitionProvider toolProvider,
        INotificationService notificationService,
        ISystemErrorLogger systemErrorLogger,
        ILogger<AgenticAiController> logger)
    {
        _orchestrator = orchestrator;
        _formatter = formatter;
        _intentGenerator = intentGenerator;
        _toolProvider = toolProvider;
        _notificationService = notificationService;
        _systemErrorLogger = systemErrorLogger;
        _logger = logger;
    }

    private async Task AuditExceptionAsync(Exception ex, string additionalData, string severity = "Error")
        => await _systemErrorLogger.LogAsync(ex, HttpContext, severity, additionalData);

    [HttpPost("query")]
    public async Task<IActionResult> Query([FromBody] AIRequest request, CancellationToken cancellationToken)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return Unauthorized(new { success = false, message = "Not authenticated." });

        if (request is null || string.IsNullOrWhiteSpace(request.InputData))
            return BadRequest(new { success = false, message = "InputData is required." });

        // Inject session user name so agents can personalise responses
        request.UserName = user.Name;

        var response = await _orchestrator.ProcessAsync(request, cancellationToken);

        if (response.Status == "error")
            return Ok(new { success = false, message = response.Message, data = response });

        // Apply response formatting if needed
        if (!string.IsNullOrEmpty(request.OutputFormat) &&
            request.OutputFormat.Equals("table", StringComparison.OrdinalIgnoreCase))
        {
            var formatted = _formatter.Format(response.Data, "table", response.Message);
            response.Data = formatted.TableContent;
        }

        return Ok(new { success = true, message = response.Message, data = response });
    }

    // ══════════════════════════ Admin / DbContext Pipeline ══════════════════════════

    /// <summary>
    /// Scans DbContext and returns discovered entity metadata.
    /// Pipeline Step 4: Custom DbContext Intent Generator — entity discovery.
    /// </summary>
    [HttpGet("admin/scan-entities")]
    public IActionResult ScanEntities()
    {
        var entities = _intentGenerator.ScanEntities();
        return Ok(new
        {
            success = true,
            message = $"Discovered {entities.Count} entities from DbContext",
            data = new
            {
                totalEntities = entities.Count,
                modules = entities.GroupBy(e => e.Module).Select(g => new
                {
                    module = g.Key,
                    count = g.Count(),
                    entities = g.Select(e => e.EntityName).ToList()
                }).OrderBy(m => m.module)
            }
        });
    }

    /// <summary>
    /// Generates the full intent catalog from DbContext schema.
    /// Pipeline Step 7: AI Intent Generator — intent generation from schema.
    /// </summary>
    [HttpGet("admin/intent-catalog")]
    public IActionResult GetIntentCatalog()
    {
        var catalog = _intentGenerator.GenerateIntentCatalog();
        return Ok(new
        {
            success = true,
            message = $"Generated {catalog.TotalIntents} intents from {catalog.TotalEntities} entities",
            data = catalog
        });
    }

    /// <summary>
    /// Regenerates tool definitions from DbContext scan, merges with baseline, and persists to disk.
    /// Pipeline Step 8: Tool Definitions Generation — auto-generated from schema.
    /// </summary>
    [HttpPost("admin/regenerate-tools")]
    public async Task<IActionResult> RegenerateToolDefinitions(CancellationToken cancellationToken)
    {
        var generated = _intentGenerator.GenerateToolDefinitions();
        _toolProvider.RefreshFromDbContext(generated);
        await _toolProvider.SaveToFileAsync(cancellationToken);

        var modules = _toolProvider.GetModules();
        return Ok(new
        {
            success = true,
            message = $"Tool definitions regenerated: {modules.Count} modules, {modules.Sum(m => m.Tools.Count)} tools",
            data = new
            {
                version = generated.Version,
                modules = modules.Select(m => new { m.Module, m.Agent, toolCount = m.Tools.Count })
            }
        });
    }

    /// <summary>
    /// Returns current loaded tool definitions.
    /// Pipeline Step 6: Swagger/OpenAPI-style tool metadata.
    /// </summary>
    [HttpGet("admin/tool-definitions")]
    public IActionResult GetToolDefinitions()
    {
        var modules = _toolProvider.GetModules();
        return Ok(new
        {
            success = true,
            data = new
            {
                totalModules = modules.Count,
                totalTools = modules.Sum(m => m.Tools.Count),
                modules = modules.Select(m => new
                {
                    m.Module,
                    m.Agent,
                    toolCount = m.Tools.Count,
                    tools = m.Tools.Select(t => new { t.Name, t.Description })
                })
            }
        });
    }

    /// <summary>
    /// Returns registered agents and their supported intents.
    /// </summary>
    [HttpGet("admin/agents")]
    public IActionResult GetAgents([FromServices] IAgentRouter agentRouter)
    {
        var names = agentRouter.GetRegisteredAgentNames();
        return Ok(new
        {
            success = true,
            data = new
            {
                totalAgents = names.Count,
                agents = names
            }
        });
    }

    // ══════════════════════════ Share ══════════════════════════

    /// <summary>
    /// Shares an AI response via email or WhatsApp using the notification service.
    /// </summary>
    [HttpPost("share")]
    public async Task<IActionResult> Share([FromBody] AiShareRequest request, CancellationToken cancellationToken)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user is null)
            return Unauthorized(new { success = false, message = "Not authenticated." });

        if (string.IsNullOrWhiteSpace(request.Channel))
            return BadRequest(new { success = false, message = "Share channel is required." });

        if (string.IsNullOrWhiteSpace(request.Recipient))
            return BadRequest(new { success = false, message = "Recipient is required." });

        if (string.IsNullOrWhiteSpace(request.Content))
            return BadRequest(new { success = false, message = "Content is required." });

        var subject = !string.IsNullOrWhiteSpace(request.Subject)
            ? request.Subject
            : $"MinePress AI — {request.Intent ?? "Shared Result"}";

        var body = FormatShareBody(request.Content, request.Intent, request.Agent, user.Name);

        var channel = request.Channel.ToLowerInvariant();

        if (channel == "email")
        {
            var result = await _notificationService.SendEmailAsync(
                request.Recipient, subject, body, cancellationToken);

            return Ok(new
            {
                success = result.IsSuccess,
                message = result.IsSuccess
                    ? $"Email sent to {request.Recipient}"
                    : $"Failed to send email: {result.ErrorMessage}"
            });
        }

        if (channel == "whatsapp")
        {
            var result = await _notificationService.SendWhatsAppAsync(
                request.Recipient, body, cancellationToken);

            return Ok(new
            {
                success = result.IsSuccess,
                message = result.IsSuccess
                    ? $"WhatsApp message sent to {request.Recipient}"
                    : $"Failed to send WhatsApp: {result.ErrorMessage}"
            });
        }

        return BadRequest(new { success = false, message = $"Unsupported share channel: {request.Channel}" });
    }

    private static string FormatShareBody(string content, string? intent, string? agent, string senderName)
    {
        return $"""
            MinePress ERP — AI Result
            ─────────────────────────
            Query: {intent ?? "N/A"}
            Agent: {agent ?? "N/A"}
            Shared by: {senderName}
            ─────────────────────────

            {content}

            ─────────────────────────
            Generated by MinePress Agentic AI
            """;
    }
}

public class AiShareRequest
{
    public string Channel { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string? Intent { get; set; }
    public string? Agent { get; set; }
}
