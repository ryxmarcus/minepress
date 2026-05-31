using erp.minepress.agentic.ai.Models;
using erp.minepress.bff.service.Interfaces;
using Microsoft.Extensions.Logging;

namespace erp.minepress.agentic.ai.Agents;

public class PurchaseAgent : BaseAgent
{
    private readonly IAiDataService _data;

    public PurchaseAgent(ILogger<PurchaseAgent> logger, IAiDataService data) : base(logger)
    {
        _data = data;
    }

    public override string AgentName => "PurchaseAgent";

    public override IReadOnlyList<string> SupportedIntents =>
        ["get_purchase_orders", "search_purchase_order", "get_purchase_order_details",
         "get_goods_receipts", "get_purchase_invoices"];

    public override Task<AgentResult> ExecuteAsync(string tool, Dictionary<string, object?> parameters, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("PurchaseAgent executing tool {Tool}", tool);

        if (ToolMatches(tool, "GetAllPurchaseOrders")) return GetAllPurchaseOrdersAsync(parameters, cancellationToken);
        if (ToolMatches(tool, "SearchPurchaseOrder")) return SearchPurchaseOrderAsync(parameters, cancellationToken);
        if (ToolMatches(tool, "GetPurchaseOrderDetails")) return GetPurchaseOrderDetailsAsync(parameters, cancellationToken);
        if (ToolMatches(tool, "GetGoodsReceipts")) return GetGoodsReceiptsAsync(parameters, cancellationToken);
        if (ToolMatches(tool, "GetPurchaseInvoices")) return GetPurchaseInvoicesAsync(parameters, cancellationToken);

        return Task.FromResult(AgentResult.Fail($"Unknown tool: {tool}"));
    }

    private async Task<AgentResult> GetAllPurchaseOrdersAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var status = GetStringParameter(parameters, "status");
        var limit = GetIntParameter(parameters, "limit", 20);
        var orders = await _data.GetAllPurchaseOrdersAsync(status, limit, ct);

        var statusInfo = string.IsNullOrEmpty(status) ? "" : $" with status '{status}'";
        return AgentResult.Ok(orders, "GetAllPurchaseOrders", $"Found {orders.Count} purchase order(s){statusInfo}");
    }

    private async Task<AgentResult> SearchPurchaseOrderAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var keyword = GetStringParameter(parameters, "keyword") ?? GetStringParameter(parameters, "poNo");
        if (string.IsNullOrEmpty(keyword))
            return AgentResult.Fail("Missing required parameter: keyword or poNo");

        var orders = await _data.SearchPurchaseOrdersAsync(keyword, ct);
        return orders.Count > 0
            ? AgentResult.Ok(orders, "SearchPurchaseOrder", $"Found {orders.Count} PO(s) matching '{keyword}'")
            : AgentResult.Fail($"No purchase orders found matching '{keyword}'");
    }

    private async Task<AgentResult> GetPurchaseOrderDetailsAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var poId = GetIntParameter(parameters, "poId");
        if (poId <= 0)
            return AgentResult.Fail("Missing required parameter: poId");

        var po = await _data.GetPurchaseOrderByIdAsync(poId, ct);
        return po is not null
            ? AgentResult.Ok(po, "GetPurchaseOrderDetails", $"PO {po.PoNo}: {po.SupplierName} — Total: ₹{po.GrandTotal:N2}, Status: {po.Status}")
            : AgentResult.Fail($"Purchase order with ID {poId} not found");
    }

    private async Task<AgentResult> GetGoodsReceiptsAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var status = GetStringParameter(parameters, "status");
        var limit = GetIntParameter(parameters, "limit", 20);
        var receipts = await _data.GetGoodsReceiptsAsync(status, limit, ct);
        return AgentResult.Ok(receipts, "GetGoodsReceipts", $"Found {receipts.Count} GRN(s)");
    }

    private async Task<AgentResult> GetPurchaseInvoicesAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var status = GetStringParameter(parameters, "status");
        var limit = GetIntParameter(parameters, "limit", 20);
        var invoices = await _data.GetPurchaseInvoicesAsync(status, limit, ct);
        return AgentResult.Ok(invoices, "GetPurchaseInvoices", $"Found {invoices.Count} purchase invoice(s)");
    }
}
