using erp.minepress.agentic.ai.Models;
using erp.minepress.bff.service.Interfaces;
using Microsoft.Extensions.Logging;

namespace erp.minepress.agentic.ai.Agents;

public class DeliveryAgent : BaseAgent
{
    private readonly IAiDataService _data;

    public DeliveryAgent(ILogger<DeliveryAgent> logger, IAiDataService data) : base(logger)
    {
        _data = data;
    }

    public override string AgentName => "DeliveryAgent";

    public override IReadOnlyList<string> SupportedIntents =>
        ["create_gate_pass", "update_delivery", "get_gate_passes", "get_gate_pass_details"];

    public override Task<AgentResult> ExecuteAsync(string tool, Dictionary<string, object?> parameters, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("DeliveryAgent executing tool {Tool}", tool);

        if (ToolMatches(tool, "GetAllGatePasses")) return GetAllGatePassesAsync(parameters, cancellationToken);
        if (ToolMatches(tool, "GetGatePassesByJob")) return GetGatePassesByJobAsync(parameters, cancellationToken);
        if (ToolMatches(tool, "CreateGatePass")) return CreateGatePassAsync(parameters, cancellationToken);
        if (ToolMatches(tool, "UpdateDelivery")) return UpdateDeliveryAsync(parameters, cancellationToken);

        return Task.FromResult(AgentResult.Fail($"Unknown tool: {tool}"));
    }

    private async Task<AgentResult> CreateGatePassAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var jobId = GetStringParameter(parameters, "jobId");
        var vehicleNumber = GetStringParameter(parameters, "vehicleNumber");
        var driverName = GetStringParameter(parameters, "driverName");
        var driverContact = GetStringParameter(parameters, "driverContact");

        if (string.IsNullOrEmpty(jobId))
        {
            return AgentResult.Fail("Missing required parameter: jobId");
        }

        // Check if gate pass already exists for this job
        var existing = await _data.GetGatePassesByJobNoAsync(jobId, ct);
        if (existing.Count > 0)
        {
            return AgentResult.Ok(existing, "CreateGatePass",
                $"Job {jobId} already has {existing.Count} gate pass(es). Here are the details:");
        }

        var gatePass = await _data.CreateGatePassAsync(jobId, vehicleNumber, driverName, driverContact, ct);

        if (gatePass is null)
            return AgentResult.Fail($"Failed to create gate pass. Job '{jobId}' not found.");

        Logger.LogInformation("Created gate pass {GpNo} for job {JobId}", gatePass.GatePassNo, jobId);
        return AgentResult.Ok(gatePass, "CreateGatePass", $"Gate pass {gatePass.GatePassNo} created for job {jobId}");
    }

    private async Task<AgentResult> UpdateDeliveryAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var jobId = GetStringParameter(parameters, "jobId");
        var deliveryStatus = GetStringParameter(parameters, "deliveryStatus");

        if (string.IsNullOrEmpty(jobId))
        {
            return AgentResult.Fail("Missing required parameter: jobId");
        }

        // Update the job status to reflect delivery
        var newStatus = deliveryStatus switch
        {
            "dispatched" or "Dispatched" => "dispatched",
            "delivered" or "Delivered" => "delivered",
            _ => deliveryStatus ?? "dispatched"
        };

        var updated = await _data.UpdateJobAsync(jobId, newStatus, null, null, ct);

        if (updated is null)
            return AgentResult.Fail($"Job '{jobId}' not found.");

        var result = new
        {
            updated.JobNo,
            updated.StatusCode,
            updated.ProductName,
            updated.CustomerName,
            DeliveryStatus = newStatus,
            UpdatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm")
        };

        Logger.LogInformation("Updated delivery for job {JobId} to {Status}", jobId, newStatus);
        return AgentResult.Ok(result, "UpdateDelivery", $"Delivery status for job {jobId} updated to '{newStatus}'");
    }

    private async Task<AgentResult> GetAllGatePassesAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var status = GetStringParameter(parameters, "status");
        var limit = GetIntParameter(parameters, "limit", 20);
        var passes = await _data.GetAllGatePassesAsync(status, limit, ct);
        return AgentResult.Ok(passes, "GetAllGatePasses", $"Found {passes.Count} gate pass(es)");
    }

    private async Task<AgentResult> GetGatePassesByJobAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var jobId = GetStringParameter(parameters, "jobId");
        if (string.IsNullOrEmpty(jobId))
            return AgentResult.Fail("Missing required parameter: jobId");

        var passes = await _data.GetGatePassesByJobNoAsync(jobId, ct);
        return passes.Count > 0
            ? AgentResult.Ok(passes, "GetGatePassesByJob", $"Found {passes.Count} gate pass(es) for job {jobId}")
            : AgentResult.Fail($"No gate passes found for job {jobId}");
    }
}
