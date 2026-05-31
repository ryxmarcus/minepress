using erp.minepress.agentic.ai.Models;
using erp.minepress.bff.service.Interfaces;
using Microsoft.Extensions.Logging;

namespace erp.minepress.agentic.ai.Agents;

public class MachineAgent : BaseAgent
{
    private readonly IAiDataService _data;

    public MachineAgent(ILogger<MachineAgent> logger, IAiDataService data) : base(logger)
    {
        _data = data;
    }

    public override string AgentName => "MachineAgent";

    public override IReadOnlyList<string> SupportedIntents =>
        ["allocate_machine", "get_machines", "get_machine_allocations", "get_machine_breakdowns"];

    public override Task<AgentResult> ExecuteAsync(string tool, Dictionary<string, object?> parameters, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("MachineAgent executing tool {Tool}", tool);

        if (ToolMatches(tool, "GetAllMachines")) return GetAllMachinesAsync(parameters, cancellationToken);
        if (ToolMatches(tool, "AllocateMachine")) return AllocateMachineAsync(parameters, cancellationToken);
        if (ToolMatches(tool, "GetMachineAllocations")) return GetMachineAllocationsAsync(parameters, cancellationToken);
        if (ToolMatches(tool, "GetMachineBreakdowns")) return GetMachineBreakdownsAsync(parameters, cancellationToken);
        if (ToolMatches(tool, "GetBreakdownsByMachine")) return GetBreakdownsByMachineAsync(parameters, cancellationToken);

        return Task.FromResult(AgentResult.Fail($"Unknown tool: {tool}"));
    }

    private async Task<AgentResult> GetAllMachinesAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var machineType = GetStringParameter(parameters, "machineType");
        var machines = await _data.GetAvailableMachinesAsync(machineType, ct);
        return AgentResult.Ok(machines, "GetAllMachines", $"Found {machines.Count} machine(s)");
    }

    private async Task<AgentResult> GetMachineAllocationsAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var jobId = GetStringParameter(parameters, "jobId");
        if (string.IsNullOrEmpty(jobId))
            return AgentResult.Fail("Missing required parameter: jobId");

        var allocations = await _data.GetMachineAllocationsForJobAsync(jobId, ct);
        return AgentResult.Ok(allocations, "GetMachineAllocations", $"Found {allocations.Count} allocation(s) for job {jobId}");
    }

    private async Task<AgentResult> GetMachineBreakdownsAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var status = GetStringParameter(parameters, "status");
        var limit = GetIntParameter(parameters, "limit", 20);
        var breakdowns = await _data.GetMachineBreakdownsAsync(status, limit, ct);
        return AgentResult.Ok(breakdowns, "GetMachineBreakdowns", $"Found {breakdowns.Count} breakdown record(s)");
    }

    private async Task<AgentResult> GetBreakdownsByMachineAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var machineId = GetIntParameter(parameters, "machineId");
        if (machineId <= 0)
            return AgentResult.Fail("Missing required parameter: machineId");

        var breakdowns = await _data.GetBreakdownsByMachineAsync(machineId, ct);
        return breakdowns.Count > 0
            ? AgentResult.Ok(breakdowns, "GetBreakdownsByMachine", $"Found {breakdowns.Count} breakdown(s) for machine {machineId}")
            : AgentResult.Fail($"No breakdowns found for machine {machineId}");
    }

    private async Task<AgentResult> AllocateMachineAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var jobId = GetStringParameter(parameters, "jobId");
        var machineType = GetStringParameter(parameters, "machineType");

        if (string.IsNullOrEmpty(jobId))
        {
            return AgentResult.Fail("Missing required parameter: jobId");
        }

        // Check existing allocations
        var existing = await _data.GetMachineAllocationsForJobAsync(jobId, ct);
        if (existing.Count > 0)
        {
            return AgentResult.Ok(existing, "AllocateMachine",
                $"Job {jobId} already has {existing.Count} machine allocation(s). Here are the details:");
        }

        // Find available machines
        var machines = await _data.GetAvailableMachinesAsync(machineType, ct);
        if (machines.Count == 0)
        {
            return AgentResult.Fail($"No available machines found{(machineType != null ? $" of type '{machineType}'" : "")}.");
        }

        // Pick the best available (fewest active allocations)
        var bestMachine = machines.OrderBy(m => m.ActiveAllocations).First();

        var allocation = await _data.AllocateMachineAsync(jobId, bestMachine.MachineId, null, ct);

        if (allocation is null)
            return AgentResult.Fail($"Failed to allocate machine to job '{jobId}'. Job may not exist.");

        var result = new
        {
            allocation.AllocationId,
            allocation.JobNo,
            allocation.MachineCode,
            allocation.MachineName,
            allocation.ProcessCode,
            allocation.PlannedQuantity,
            allocation.AllocationStatus,
            allocation.PlannedStartTime
        };

        Logger.LogInformation("Allocated machine {MachineCode} to job {JobId}", bestMachine.MachineCode, jobId);
        return AgentResult.Ok(result, "AllocateMachine", $"Machine {bestMachine.MachineName} ({bestMachine.MachineCode}) allocated to job {jobId}");
    }
}
