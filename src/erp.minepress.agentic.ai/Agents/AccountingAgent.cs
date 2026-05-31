using erp.minepress.agentic.ai.Models;
using erp.minepress.bff.service.Interfaces;
using Microsoft.Extensions.Logging;

namespace erp.minepress.agentic.ai.Agents;

public class AccountingAgent : BaseAgent
{
    private readonly IAiDataService _data;

    public AccountingAgent(ILogger<AccountingAgent> logger, IAiDataService data) : base(logger)
    {
        _data = data;
    }

    public override string AgentName => "AccountingAgent";

    public override IReadOnlyList<string> SupportedIntents =>
        ["get_receipts", "get_payments", "get_expense_vouchers", "get_outstanding_summary",
         "get_challans", "get_challans_by_job", "get_proforma_invoices",
         "get_machine_breakdowns", "get_breakdowns_by_machine"];

    public override Task<AgentResult> ExecuteAsync(string tool, Dictionary<string, object?> parameters, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("AccountingAgent executing tool {Tool}", tool);

        if (ToolMatches(tool, "GetReceipts")) return GetReceiptsAsync(parameters, cancellationToken);
        if (ToolMatches(tool, "GetPayments")) return GetPaymentsAsync(parameters, cancellationToken);
        if (ToolMatches(tool, "GetExpenseVouchers")) return GetExpenseVouchersAsync(parameters, cancellationToken);
        if (ToolMatches(tool, "GetOutstandingSummary")) return GetOutstandingSummaryAsync(cancellationToken);
        if (ToolMatches(tool, "GetChallans")) return GetChallansAsync(parameters, cancellationToken);
        if (ToolMatches(tool, "GetChallansByJob")) return GetChallansByJobAsync(parameters, cancellationToken);
        if (ToolMatches(tool, "GetProformaInvoices")) return GetProformaInvoicesAsync(parameters, cancellationToken);
        if (ToolMatches(tool, "GetMachineBreakdowns")) return GetMachineBreakdownsAsync(parameters, cancellationToken);
        if (ToolMatches(tool, "GetBreakdownsByMachine")) return GetBreakdownsByMachineAsync(parameters, cancellationToken);

        return Task.FromResult(AgentResult.Fail($"Unknown tool: {tool}"));
    }

    private async Task<AgentResult> GetReceiptsAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var from = ParseDate(GetStringParameter(parameters, "from"));
        var to = ParseDate(GetStringParameter(parameters, "to"));
        var limit = GetIntParameter(parameters, "limit", 20);
        var receipts = await _data.GetReceiptsAsync(from, to, limit, ct);
        return AgentResult.Ok(receipts, "GetReceipts", $"Found {receipts.Count} receipt(s)");
    }

    private async Task<AgentResult> GetPaymentsAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var from = ParseDate(GetStringParameter(parameters, "from"));
        var to = ParseDate(GetStringParameter(parameters, "to"));
        var limit = GetIntParameter(parameters, "limit", 20);
        var payments = await _data.GetPaymentsAsync(from, to, limit, ct);
        return AgentResult.Ok(payments, "GetPayments", $"Found {payments.Count} payment(s)");
    }

    private async Task<AgentResult> GetExpenseVouchersAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var category = GetStringParameter(parameters, "category");
        var limit = GetIntParameter(parameters, "limit", 20);
        var vouchers = await _data.GetExpenseVouchersAsync(category, limit, ct);
        return AgentResult.Ok(vouchers, "GetExpenseVouchers", $"Found {vouchers.Count} expense voucher(s)");
    }

    private async Task<AgentResult> GetOutstandingSummaryAsync(CancellationToken ct)
    {
        var summary = await _data.GetOutstandingSummaryAsync(ct);
        return AgentResult.Ok(summary, "GetOutstandingSummary",
            $"Outstanding — Receivable: ₹{summary.TotalReceivable:N2} ({summary.ReceivableCount}), Payable: ₹{summary.TotalPayable:N2} ({summary.PayableCount})");
    }

    private async Task<AgentResult> GetChallansAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var status = GetStringParameter(parameters, "status");
        var limit = GetIntParameter(parameters, "limit", 20);
        var challans = await _data.GetAllChallansAsync(status, limit, ct);
        return AgentResult.Ok(challans, "GetChallans", $"Found {challans.Count} challan(s)");
    }

    private async Task<AgentResult> GetChallansByJobAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var jobNo = GetStringParameter(parameters, "jobNo");
        if (string.IsNullOrEmpty(jobNo))
            return AgentResult.Fail("Missing required parameter: jobNo");

        var challans = await _data.GetChallansByJobNoAsync(jobNo, ct);
        return challans.Count > 0
            ? AgentResult.Ok(challans, "GetChallansByJob", $"Found {challans.Count} challan(s) for job {jobNo}")
            : AgentResult.Fail($"No challans found for job {jobNo}");
    }

    private async Task<AgentResult> GetProformaInvoicesAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var status = GetStringParameter(parameters, "status");
        var limit = GetIntParameter(parameters, "limit", 20);
        var invoices = await _data.GetAllProformaInvoicesAsync(status, limit, ct);
        return AgentResult.Ok(invoices, "GetProformaInvoices", $"Found {invoices.Count} proforma invoice(s)");
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

    private static DateOnly? ParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        return DateOnly.TryParse(value, out var d) ? d : null;
    }
}
