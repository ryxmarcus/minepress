using erp.minepress.agentic.ai.Models;
using erp.minepress.bff.service.Interfaces;
using Microsoft.Extensions.Logging;

namespace erp.minepress.agentic.ai.Agents;

public class CustomerAgent : BaseAgent
{
    private readonly IAiDataService _data;

    public CustomerAgent(ILogger<CustomerAgent> logger, IAiDataService data) : base(logger)
    {
        _data = data;
    }

    public override string AgentName => "CustomerAgent";

    public override IReadOnlyList<string> SupportedIntents =>
        ["get_customers", "search_customer", "get_customer_details", "get_customer_summary",
         "get_customer_jobs", "get_customer_invoices"];

    public override Task<AgentResult> ExecuteAsync(string tool, Dictionary<string, object?> parameters, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("CustomerAgent executing tool {Tool}", tool);

        if (ToolMatches(tool, "GetAllCustomers")) return GetAllCustomersAsync(parameters, cancellationToken);
        if (ToolMatches(tool, "SearchCustomer")) return SearchCustomerAsync(parameters, cancellationToken);
        if (ToolMatches(tool, "GetCustomerDetails")) return GetCustomerDetailsAsync(parameters, cancellationToken);
        if (ToolMatches(tool, "GetCustomerSummary")) return GetCustomerSummaryAsync(parameters, cancellationToken);
        if (ToolMatches(tool, "GetCustomerJobs")) return GetCustomerJobsAsync(parameters, cancellationToken);
        if (ToolMatches(tool, "GetCustomerInvoices")) return GetCustomerInvoicesAsync(parameters, cancellationToken);

        return Task.FromResult(AgentResult.Fail($"Unknown tool: {tool}"));
    }

    private async Task<AgentResult> GetAllCustomersAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var limit = GetIntParameter(parameters, "limit", 20);
        var customers = await _data.GetAllCustomersAsync(limit, ct);
        return AgentResult.Ok(customers, "GetAllCustomers", $"Found {customers.Count} customer(s)");
    }

    private async Task<AgentResult> SearchCustomerAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var keyword = GetStringParameter(parameters, "keyword") ?? GetStringParameter(parameters, "customerName");
        if (string.IsNullOrEmpty(keyword))
            return AgentResult.Fail("Missing required parameter: keyword or customerName");

        var customers = await _data.SearchCustomersAsync(keyword, ct);
        return customers.Count > 0
            ? AgentResult.Ok(customers, "SearchCustomer", $"Found {customers.Count} customer(s) matching '{keyword}'")
            : AgentResult.Fail($"No customers found matching '{keyword}'");
    }

    private async Task<AgentResult> GetCustomerDetailsAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var partyId = GetIntParameter(parameters, "partyId");
        if (partyId <= 0)
            return AgentResult.Fail("Missing required parameter: partyId");

        var customer = await _data.GetCustomerByIdAsync(partyId, ct);
        return customer is not null
            ? AgentResult.Ok(customer, "GetCustomerDetails", $"Customer: {customer.Name} ({customer.Code})")
            : AgentResult.Fail($"Customer with ID {partyId} not found");
    }

    private async Task<AgentResult> GetCustomerSummaryAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var partyId = GetIntParameter(parameters, "partyId");
        if (partyId <= 0)
            return AgentResult.Fail("Missing required parameter: partyId");

        var summary = await _data.GetCustomerSummaryAsync(partyId, ct);
        return AgentResult.Ok(summary, "GetCustomerSummary",
            $"Customer '{summary.CustomerName}': {summary.TotalJobs} jobs, {summary.TotalInvoices} invoices, Revenue ₹{summary.TotalRevenue:N2}");
    }

    private async Task<AgentResult> GetCustomerJobsAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var partyId = GetIntParameter(parameters, "partyId");
        if (partyId <= 0)
            return AgentResult.Fail("Missing required parameter: partyId");

        var jobs = await _data.GetCustomerJobsAsync(partyId, 20, ct);
        return AgentResult.Ok(jobs, "GetCustomerJobs", $"Found {jobs.Count} job(s) for customer");
    }

    private async Task<AgentResult> GetCustomerInvoicesAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var partyId = GetIntParameter(parameters, "partyId");
        if (partyId <= 0)
            return AgentResult.Fail("Missing required parameter: partyId");

        var invoices = await _data.GetCustomerInvoicesAsync(partyId, 20, ct);
        return AgentResult.Ok(invoices, "GetCustomerInvoices", $"Found {invoices.Count} invoice(s) for customer");
    }
}
