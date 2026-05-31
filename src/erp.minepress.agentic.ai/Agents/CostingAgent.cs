using erp.minepress.agentic.ai.Models;
using erp.minepress.application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace erp.minepress.agentic.ai.Agents;

public class CostingAgent : BaseAgent
{
    private readonly ICostingEngine? _costingEngine;

    public CostingAgent(ILogger<CostingAgent> logger, ICostingEngine? costingEngine = null) : base(logger)
    {
        _costingEngine = costingEngine;
    }

    public override string AgentName => "CostingAgent";

    public override IReadOnlyList<string> SupportedIntents => ["calculate_cost"];

    public override async Task<AgentResult> ExecuteAsync(string tool, Dictionary<string, object?> parameters, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("CostingAgent executing tool {Tool}", tool);

        if (ToolMatches(tool, "CalculateJobCost")) return await CalculateJobCostAsync(parameters, cancellationToken);

        return AgentResult.Fail($"Unknown tool: {tool}");
    }

    private async Task<AgentResult> CalculateJobCostAsync(Dictionary<string, object?> parameters, CancellationToken cancellationToken)
    {
        if (_costingEngine is null)
        {
            return AgentResult.Fail("Costing engine is not available in this application.");
        }

        var quantity = GetIntParameter(parameters, "quantity");
        var jobType = GetStringParameter(parameters, "jobType");
        var paperType = GetStringParameter(parameters, "paperType");
        var colorMode = GetStringParameter(parameters, "colorMode");

        if (quantity <= 0)
        {
            return AgentResult.Fail("Missing required parameter: quantity must be greater than 0.");
        }

        try
        {
            var request = new CostEstimationRequest
            {
                Quantity = quantity,
                TotalPages = 4,
                TrimWidthMm = 210,
                TrimHeightMm = 297,
                PrintingMode = jobType
            };

            var estimation = await _costingEngine.CalculateCostAsync(request, cancellationToken);

            var result = new
            {
                materialCost = estimation.PaperCost + estimation.InkCost + estimation.PlateCost,
                printingCost = estimation.MachineCost,
                finishingCost = estimation.FinishingCost + estimation.BindingCost,
                totalCost = estimation.GrandTotal,
                costPerUnit = estimation.CostPerUnit,
                breakdown = estimation.Breakdown
            };

            return AgentResult.Ok(result, "CalculateJobCost", $"Cost calculated: Total ₹{estimation.GrandTotal:N2}");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error calculating job cost");
            return AgentResult.Fail($"Error calculating cost: {ex.Message}");
        }
    }
}
