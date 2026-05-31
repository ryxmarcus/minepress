using erp.minepress.agentic.ai.Models;
using erp.minepress.bff.service.Interfaces;
using Microsoft.Extensions.Logging;

namespace erp.minepress.agentic.ai.Agents;

public class BillingAgent : BaseAgent
{
    private readonly IAiDataService _data;

    public BillingAgent(ILogger<BillingAgent> logger, IAiDataService data) : base(logger)
    {
        _data = data;
    }

    public override string AgentName => "BillingAgent";

    public override IReadOnlyList<string> SupportedIntents =>
        ["generate_invoice", "get_invoices", "search_invoice", "get_credit_notes", "get_debit_notes"];

    public override Task<AgentResult> ExecuteAsync(string tool, Dictionary<string, object?> parameters, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("BillingAgent executing tool {Tool}", tool);

        if (ToolMatches(tool, "GetAllInvoices")) return GetAllInvoicesAsync(parameters, cancellationToken);
        if (ToolMatches(tool, "GetInvoiceByJob")) return GetInvoiceByJobAsync(parameters, cancellationToken);
        if (ToolMatches(tool, "GenerateInvoice")) return GenerateInvoiceAsync(parameters, cancellationToken);
        if (ToolMatches(tool, "GetCreditNotes")) return GetCreditNotesAsync(parameters, cancellationToken);
        if (ToolMatches(tool, "GetDebitNotes")) return GetDebitNotesAsync(parameters, cancellationToken);

        return Task.FromResult(AgentResult.Fail($"Unknown tool: {tool}"));
    }

    private async Task<AgentResult> GenerateInvoiceAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var jobId = GetStringParameter(parameters, "jobId");

        if (string.IsNullOrEmpty(jobId))
        {
            // Return recent invoices if no job specified
            var recent = await _data.GetRecentInvoicesAsync(10, ct);
            if (recent.Count == 0)
                return AgentResult.Fail("No invoices found.");

            return AgentResult.Ok(recent, "GenerateInvoice", $"Found {recent.Count} recent invoice(s).");
        }

        var invoice = await _data.GetInvoiceByJobNoAsync(jobId, ct);

        if (invoice is null)
        {
            return AgentResult.Fail($"No invoice found for job '{jobId}'. The invoice may not have been generated yet.");
        }

        Logger.LogInformation("Retrieved invoice {InvoiceNo} for job {JobId}", invoice.InvoiceNo, jobId);
        return AgentResult.Ok(invoice, "GenerateInvoice",
            $"Invoice {invoice.InvoiceNo} for job {jobId}: Total ₹{invoice.GrandTotal:N2}, Status: {invoice.Status}");
    }

    private async Task<AgentResult> GetAllInvoicesAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var limit = GetIntParameter(parameters, "limit", 20);
        var invoices = await _data.GetRecentInvoicesAsync(limit, ct);
        return AgentResult.Ok(invoices, "GetAllInvoices", $"Found {invoices.Count} invoice(s)");
    }

    private async Task<AgentResult> GetInvoiceByJobAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var jobId = GetStringParameter(parameters, "jobId");
        if (string.IsNullOrEmpty(jobId))
            return AgentResult.Fail("Missing required parameter: jobId");

        var invoice = await _data.GetInvoiceByJobNoAsync(jobId, ct);
        return invoice is not null
            ? AgentResult.Ok(invoice, "GetInvoiceByJob", $"Invoice {invoice.InvoiceNo} for job {jobId}: Total ₹{invoice.GrandTotal:N2}, Status: {invoice.Status}")
            : AgentResult.Fail($"No invoice found for job '{jobId}'");
    }

    private async Task<AgentResult> GetCreditNotesAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var status = GetStringParameter(parameters, "status");
        var limit = GetIntParameter(parameters, "limit", 20);
        var notes = await _data.GetCreditNotesAsync(status, limit, ct);
        return AgentResult.Ok(notes, "GetCreditNotes", $"Found {notes.Count} credit note(s)");
    }

    private async Task<AgentResult> GetDebitNotesAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var status = GetStringParameter(parameters, "status");
        var limit = GetIntParameter(parameters, "limit", 20);
        var notes = await _data.GetDebitNotesAsync(status, limit, ct);
        return AgentResult.Ok(notes, "GetDebitNotes", $"Found {notes.Count} debit note(s)");
    }
}
