using erp.minepress.agentic.ai.Models;
using erp.minepress.bff.service.Interfaces;
using Microsoft.Extensions.Logging;

namespace erp.minepress.agentic.ai.Agents;

public class JobAgent : BaseAgent
{
    private readonly IAiDataService _data;

    public JobAgent(ILogger<JobAgent> logger, IAiDataService data) : base(logger)
    {
        _data = data;
    }

    public override string AgentName => "JobAgent";

    public override IReadOnlyList<string> SupportedIntents =>
        ["create_job", "update_job", "get_job_details", "get_jobs", "search_job", "get_jobs_by_status"];

    public override Task<AgentResult> ExecuteAsync(string tool, Dictionary<string, object?> parameters, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("JobAgent executing tool {Tool}", tool);

        if (ToolMatches(tool, "GetAllJobs")) return GetAllJobsAsync(parameters, cancellationToken);
        if (ToolMatches(tool, "SearchJob")) return SearchJobAsync(parameters, cancellationToken);
        if (ToolMatches(tool, "GetJobDetails")) return GetJobDetailsAsync(parameters, cancellationToken);
        if (ToolMatches(tool, "CreatePrintJob")) return CreatePrintJobAsync(parameters, cancellationToken);
        if (ToolMatches(tool, "UpdatePrintJob")) return UpdatePrintJobAsync(parameters, cancellationToken);

        return Task.FromResult(AgentResult.Fail($"Unknown tool: {tool}"));
    }

    private async Task<AgentResult> GetAllJobsAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var status = GetStringParameter(parameters, "status");
        var limit = GetIntParameter(parameters, "limit", 20);
        var jobs = await _data.GetJobsByStatusAsync(status, limit, ct);

        var statusInfo = string.IsNullOrEmpty(status) ? "" : $" with status '{status}'";
        return AgentResult.Ok(jobs, "GetAllJobs", $"Found {jobs.Count} job(s){statusInfo}");
    }

    private async Task<AgentResult> SearchJobAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var keyword = GetStringParameter(parameters, "keyword") ?? GetStringParameter(parameters, "jobId");
        if (string.IsNullOrEmpty(keyword))
            return AgentResult.Fail("Missing required parameter: keyword");

        var jobs = await _data.SearchJobsAsync(keyword, 20, ct);
        return jobs.Count > 0
            ? AgentResult.Ok(jobs, "SearchJob", $"Found {jobs.Count} job(s) matching '{keyword}'")
            : AgentResult.Fail($"No jobs found matching '{keyword}'");
    }

    private async Task<AgentResult> CreatePrintJobAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var customerName = GetStringParameter(parameters, "customerName");
        var jobType = GetStringParameter(parameters, "jobType");
        var productType = GetStringParameter(parameters, "productType");
        var quantity = GetIntParameter(parameters, "quantity");
        var paperSize = GetStringParameter(parameters, "paperSize");
        var colorMode = GetStringParameter(parameters, "colorMode");
        var priority = GetStringParameter(parameters, "priority");

        if (string.IsNullOrEmpty(customerName) || string.IsNullOrEmpty(productType) || quantity <= 0)
        {
            return AgentResult.Fail("Missing required parameters: customerName, productType, and quantity are required.");
        }

        var job = await _data.CreateJobAsync(new AiCreateJobRequest
        {
            CustomerName = customerName,
            ProductName = productType,
            Quantity = quantity,
            JobType = jobType ?? "Offset",
            PaperSize = paperSize ?? "A4",
            ColorMode = colorMode ?? "Color",
            Priority = priority ?? "Normal",
            CreatedByUserId = 1,
            CompanyId = 1
        }, ct);

        if (job is null)
            return AgentResult.Fail("Failed to create job in the database.");

        Logger.LogInformation("Created job {JobNo} for {Customer}", job.JobNo, customerName);
        return AgentResult.Ok(job, "CreatePrintJob", $"Job {job.JobNo} created successfully for {quantity} {productType}(s)");
    }

    private async Task<AgentResult> GetJobDetailsAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var jobId = GetStringParameter(parameters, "jobId");

        if (string.IsNullOrEmpty(jobId))
        {
            return AgentResult.Fail("Missing required parameter: jobId");
        }

        var job = await _data.GetJobByNoAsync(jobId, ct);

        if (job is null)
        {
            // Try searching by status if jobId looks like a status keyword
            var jobs = await _data.GetJobsByStatusAsync(jobId, 10, ct);
            if (jobs.Count > 0)
                return AgentResult.Ok(jobs, "GetJobDetails", $"Found {jobs.Count} job(s) with status '{jobId}'");

            return AgentResult.Fail($"Job '{jobId}' not found.");
        }

        return AgentResult.Ok(job, "GetJobDetails", $"Job {job.JobNo}: {job.ProductName} — Status: {job.StatusCode}, Qty: {job.Quantity}");
    }

    private async Task<AgentResult> UpdatePrintJobAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var jobId = GetStringParameter(parameters, "jobId");

        if (string.IsNullOrEmpty(jobId))
        {
            return AgentResult.Fail("Missing required parameter: jobId");
        }

        var status = GetStringParameter(parameters, "status");
        var quantity = GetIntParameter(parameters, "quantity");
        var priority = GetStringParameter(parameters, "priority");

        var updated = await _data.UpdateJobAsync(jobId, status, quantity > 0 ? quantity : null, priority, ct);

        if (updated is null)
            return AgentResult.Fail($"Job '{jobId}' not found.");

        Logger.LogInformation("Updated job {JobNo}", jobId);
        return AgentResult.Ok(updated, "UpdatePrintJob", $"Job {updated.JobNo} updated successfully — Status: {updated.StatusCode}");
    }
}
