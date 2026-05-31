using erp.minepress.agentic.ai.Interfaces;
using erp.minepress.agentic.ai.Models;
using Microsoft.AspNetCore.Mvc;

namespace erp.minepress.webapi.Controllers;

[Route("api/ai")]
public class AiController : BaseApiController
{
    private readonly IAIOrchestratorService _orchestrator;

    public AiController(IAIOrchestratorService orchestrator)
    {
        _orchestrator = orchestrator;
    }

    [HttpPost("query")]
    public async Task<IActionResult> Query([FromBody] AIRequest request, CancellationToken cancellationToken)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.InputData))
        {
            return ErrorResponse<AIResponse>("InputData is required.");
        }

        var response = await _orchestrator.ProcessAsync(request, cancellationToken);

        if (response.Status == "error")
        {
            return ErrorResponse<AIResponse>(response.Message ?? "An error occurred.");
        }

        return OkResponse(response);
    }

    /// <summary>
    /// Scans DbContext and returns entity metadata. Pipeline Step 4.
    /// </summary>
    [HttpGet("admin/scan-entities")]
    public IActionResult ScanEntities([FromServices] IDbContextIntentGenerator generator)
    {
        var entities = generator.ScanEntities();
        return OkResponse(new
        {
            totalEntities = entities.Count,
            modules = entities.GroupBy(e => e.Module).Select(g => new
            {
                module = g.Key,
                count = g.Count(),
                entities = g.Select(e => e.EntityName).ToList()
            }).OrderBy(m => m.module)
        });
    }

    /// <summary>
    /// Generates full intent catalog from DbContext. Pipeline Step 7.
    /// </summary>
    [HttpGet("admin/intent-catalog")]
    public IActionResult GetIntentCatalog([FromServices] IDbContextIntentGenerator generator)
    {
        var catalog = generator.GenerateIntentCatalog();
        return OkResponse(catalog);
    }

    /// <summary>
    /// Regenerates tool definitions from DbContext scan. Pipeline Step 8.
    /// </summary>
    [HttpPost("admin/regenerate-tools")]
    public async Task<IActionResult> RegenerateTools(
        [FromServices] IDbContextIntentGenerator generator,
        [FromServices] IToolDefinitionProvider toolProvider,
        CancellationToken cancellationToken)
    {
        var generated = generator.GenerateToolDefinitions();
        toolProvider.RefreshFromDbContext(generated);
        await toolProvider.SaveToFileAsync(cancellationToken);
        return OkResponse(new
        {
            message = "Tool definitions regenerated",
            modules = toolProvider.GetModules().Count,
            tools = toolProvider.GetModules().Sum(m => m.Tools.Count)
        });
    }

    /// <summary>
    /// Returns current tool definitions. Pipeline Step 6.
    /// </summary>
    [HttpGet("admin/tool-definitions")]
    public IActionResult GetToolDefinitions([FromServices] IToolDefinitionProvider toolProvider)
    {
        var modules = toolProvider.GetModules();
        return OkResponse(new
        {
            totalModules = modules.Count,
            totalTools = modules.Sum(m => m.Tools.Count),
            modules = modules.Select(m => new { m.Module, m.Agent, toolCount = m.Tools.Count })
        });
    }
}
