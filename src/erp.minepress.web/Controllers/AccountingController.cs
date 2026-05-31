using erp.minepress.infrastructure.ErrorLogging;
using erp.minepress.notification.Enums;
using erp.minepress.notification.Interfaces;
using erp.minepress.notification.Models;
using erp.minepress.persistence.Context;
using erp.minepress.persistence.Models;
using erp.minepress.web.Helpers;
using erp.minepress.web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace erp.minepress.web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountingController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly INotificationDispatcher _notificationDispatcher;
    private readonly IUserActivityService _activityService;
    private readonly IDocumentNumberService _documentNumberService;
    private readonly ILogger<AccountingController> _logger;
    private readonly ISystemErrorLogger _systemErrorLogger;

    public AccountingController(
        ApplicationDbContext db,
        INotificationDispatcher notificationDispatcher,
        IUserActivityService activityService,
        IDocumentNumberService documentNumberService,
        ILogger<AccountingController> logger,
        ISystemErrorLogger systemErrorLogger)
    {
        _db = db;
        _notificationDispatcher = notificationDispatcher;
        _activityService = activityService;
        _documentNumberService = documentNumberService;
        _logger = logger;
        _systemErrorLogger = systemErrorLogger;
    }

    private async Task AuditExceptionAsync(Exception ex, string additionalData, string severity = "Error")
        => await _systemErrorLogger.LogAsync(ex, HttpContext, severity, additionalData);

    // ═══════════════════════════════════════════════════════════════
    // DASHBOARD
    // ═══════════════════════════════════════════════════════════════

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var totalReceivable = await _db.TrnArOutstandings
            .Where(a => a.Status == "OPEN")
            .SumAsync(a => (decimal?)a.OriginalAmount - (decimal?)a.PaidAmount - (decimal?)a.AdjustedAmount - (decimal?)a.WriteOffAmount) ?? 0;

        var totalPayable = await _db.TrnApOutstandings
            .Where(a => a.Status == "OPEN")
            .SumAsync(a => (decimal?)a.OriginalAmount - (decimal?)a.PaidAmount - (decimal?)a.AdjustedAmount - (decimal?)a.TdsAmount - (decimal?)a.WriteOffAmount) ?? 0;

        var overdueReceivable = await _db.TrnArOutstandings
            .Where(a => a.Status == "OPEN" && a.DueDate < today)
            .CountAsync();

        var overduePayable = await _db.TrnApOutstandings
            .Where(a => a.Status == "OPEN" && a.DueDate < today)
            .CountAsync();

        var thisMonthStart = new DateOnly(today.Year, today.Month, 1);

        var salesThisMonth = await _db.TrnSalesInvoices
            .Where(i => i.InvoiceDate >= thisMonthStart && i.Status != "CANCELLED")
            .SumAsync(i => (decimal?)i.GrandTotal) ?? 0;

        var purchaseThisMonth = await _db.TrnPurchaseInvoices
            .Where(i => i.InvoiceDate >= thisMonthStart && i.Status != "CANCELLED")
            .SumAsync(i => (decimal?)i.GrandTotal) ?? 0;

        var receiptsThisMonth = await _db.TrnReceipts
            .Where(r => r.ReceiptDate >= thisMonthStart && r.Status != "CANCELLED")
            .SumAsync(r => (decimal?)r.Amount) ?? 0;

        var paymentsThisMonth = await _db.TrnPayments
            .Where(p => p.PaymentDate >= thisMonthStart && p.Status != "CANCELLED")
            .SumAsync(p => (decimal?)p.Amount) ?? 0;

        var pendingSalesInvoices = await _db.TrnSalesInvoices
            .Where(i => i.Status == "DRAFT")
            .CountAsync();

        var pendingPurchaseInvoices = await _db.TrnPurchaseInvoices
            .Where(i => i.Status == "DRAFT")
            .CountAsync();

        var pendingExpenseApprovals = await _db.TrnExpenseVouchers
            .Where(e => e.Status == "PENDING_APPROVAL" && e.IsApproved != true)
            .CountAsync();

        // Recent transactions
        var recentTransactions = await _db.TrnSalesInvoices
            .Where(i => i.Status != "CANCELLED")
            .OrderByDescending(i => i.CreatedOn)
            .Take(5)
            .Select(i => new
            {
                Type = "Sales Invoice",
                Icon = "bi-receipt",
                Color = "primary",
                DocNo = i.InvoiceNo,
                Date = i.InvoiceDate.ToString("dd-MMM-yyyy"),
                PartyName = i.Party != null ? i.Party.Name : "",
                Amount = i.GrandTotal,
                i.Status
            })
            .ToListAsync();

        return Ok(new
        {
            TotalReceivable = totalReceivable,
            TotalPayable = totalPayable,
            OverdueReceivable = overdueReceivable,
            OverduePayable = overduePayable,
            SalesThisMonth = salesThisMonth,
            PurchaseThisMonth = purchaseThisMonth,
            ReceiptsThisMonth = receiptsThisMonth,
            PaymentsThisMonth = paymentsThisMonth,
            PendingSalesInvoices = pendingSalesInvoices,
            PendingPurchaseInvoices = pendingPurchaseInvoices,
            PendingExpenseApprovals = pendingExpenseApprovals,
            NetFlow = receiptsThisMonth - paymentsThisMonth,
            RecentTransactions = recentTransactions
        });
    }

    // ═══════════════════════════════════════════════════════════════
    // SALES INVOICE
    // ═══════════════════════════════════════════════════════════════

    [HttpGet("sales-invoices")]
    public async Task<IActionResult> GetSalesInvoices()
    {
        var list = await _db.TrnSalesInvoices
            .Include(i => i.Party)
            .Include(i => i.TrnSalesInvoiceItems)
            .OrderByDescending(i => i.SalesInvoiceId)
            .Select(i => new
            {
                i.SalesInvoiceId,
                i.InvoiceNo,
                InvoiceDate = i.InvoiceDate.ToString("dd-MMM-yyyy"),
                CustomerName = i.Party != null ? i.Party.Name : "",
                CustomerCode = i.Party != null ? i.Party.Code : "",
                i.GrandTotal,
                i.PaidAmount,
                i.BalanceAmount,
                i.Status,
                ItemCount = i.TrnSalesInvoiceItems.Count,
                i.JobId,
                CreatedOn = i.CreatedOn.HasValue ? i.CreatedOn.Value.ToString("dd-MMM-yyyy HH:mm") : ""
            })
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("sales-invoice/{id:long}")]
    public async Task<IActionResult> GetSalesInvoiceDetail(long id)
    {
        var inv = await _db.TrnSalesInvoices
            .Include(i => i.Party)
            .Include(i => i.Company)
            .Include(i => i.TrnSalesInvoiceItems)
            .Include(i => i.Job)
            .Include(i => i.PaymentTerm)
            .FirstOrDefaultAsync(i => i.SalesInvoiceId == id);

        if (inv == null)
            return NotFound(new { message = "Sales invoice not found." });

        var user = HttpContext.Session.GetCurrentUser();
        if (user != null)
        {
            var activity = ActivityLogEntry.FromUser(user, "ACCOUNTING", "VIEW", $"Viewed Sales Invoice {inv.InvoiceNo}");
            activity.EntityType = "SALES_INVOICE";
            activity.EntityId = inv.SalesInvoiceId;
            activity.EntityCode = inv.InvoiceNo;
            await _activityService.LogActivityAsync(activity);
        }

        var companyAddress = inv.Company != null
            ? $"{inv.Company.AddressLine1}{(inv.Company.AddressLine2 != null ? ", " + inv.Company.AddressLine2 : "")}"
            : "";

        return Ok(new
        {
            inv.SalesInvoiceId,
            inv.InvoiceNo,
            InvoiceDate = inv.InvoiceDate.ToString("dd-MMM-yyyy"),
            DueDate = inv.DueDate?.ToString("dd-MMM-yyyy"),
            CustomerName = inv.Party?.Name,
            CustomerCode = inv.Party?.Code,
            CustomerGst = inv.Party?.Gstno,
            CustomerEmail = inv.Party?.Email,
            CustomerAddress = inv.Party?.Address1,
            CustomerPhone = inv.Party?.Mobile?.ToString(),
            CompanyName = inv.Company?.Name,
            CompanyGstin = inv.Company?.Gstin,
            CompanyAddress = companyAddress,
            CompanyEmail = inv.Company?.EmailId,
            CompanyPhone = inv.Company?.ContactNo,
            inv.PartyId,
            inv.JobId,
            JobNo = inv.Job?.JobNo,
            PaymentTermName = inv.PaymentTerm?.Name,
            inv.PlaceOfSupply,
            inv.SubtotalAmount,
            inv.DiscountAmount,
            inv.TaxableAmount,
            inv.CgstAmount,
            inv.SgstAmount,
            inv.IgstAmount,
            inv.CessAmount,
            inv.TotalTaxAmount,
            inv.RoundOff,
            inv.GrandTotal,
            inv.PaidAmount,
            inv.BalanceAmount,
            inv.Status,
            inv.IsCancelled,
            inv.IsPostedToGl,
            inv.EInvoiceIrn,
            inv.TermsConditions,
            inv.InternalNotes,
            CreatedOn = inv.CreatedOn?.ToString("dd-MMM-yyyy HH:mm"),
            Items = inv.TrnSalesInvoiceItems
                .OrderBy(it => it.ItemSequence)
                .Select(it => new
                {
                    it.InvoiceItemId,
                    it.ItemSequence,
                    it.Description,
                    it.HsnSacCode,
                    it.Quantity,
                    it.UnitRate,
                    it.DiscountPercent,
                    it.DiscountAmount,
                    it.TaxableValue,
                    it.CgstPercent,
                    it.CgstAmount,
                    it.SgstPercent,
                    it.SgstAmount,
                    it.IgstPercent,
                    it.IgstAmount,
                    it.TotalTaxAmount,
                    it.LineTotal
                })
        });
    }

    [HttpPost("sales-invoice/save")]
    public async Task<IActionResult> SaveSalesInvoice([FromBody] SalesInvoiceSaveRequest request)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return Unauthorized(new { message = "Session expired. Please login again." });

        using var txn = await _db.Database.BeginTransactionAsync();
        try
        {
            TrnSalesInvoice invoice;
            bool isNew = request.SalesInvoiceId == 0;

            if (isNew)
            {
                var invoiceNo = await _documentNumberService.GenerateNextNumberAsync("SALES_INVOICE");
                invoice = new TrnSalesInvoice
                {
                    InvoiceNo = invoiceNo,
                    InvoiceDate = DateOnly.Parse(request.InvoiceDate),
                    CompanyId = user.CompanyId ?? 1,
                    PartyId = request.PartyId,
                    JobId = request.JobId,
                    QuotationId = request.QuotationId,
                    PlaceOfSupply = request.PlaceOfSupply,
                    PaymentTermId = request.PaymentTermId,
                    Status = "DRAFT",
                    CreatedBy = user.UserId,
                    CreatedOn = DateTime.Now
                };

                if (!string.IsNullOrEmpty(request.DueDate))
                    invoice.DueDate = DateOnly.Parse(request.DueDate);

                _db.TrnSalesInvoices.Add(invoice);
                await _db.SaveChangesAsync();
            }
            else
            {
                invoice = await _db.TrnSalesInvoices
                    .Include(i => i.TrnSalesInvoiceItems)
                    .FirstOrDefaultAsync(i => i.SalesInvoiceId == request.SalesInvoiceId);

                if (invoice == null)
                    return NotFound(new { message = "Invoice not found." });

                invoice.InvoiceDate = DateOnly.Parse(request.InvoiceDate);
                invoice.PartyId = request.PartyId;
                invoice.JobId = request.JobId;
                invoice.QuotationId = request.QuotationId;
                invoice.PlaceOfSupply = request.PlaceOfSupply;
                invoice.PaymentTermId = request.PaymentTermId;
                invoice.ModifiedBy = user.UserCode;
                invoice.ModifiedOn = DateTime.Now;

                if (!string.IsNullOrEmpty(request.DueDate))
                    invoice.DueDate = DateOnly.Parse(request.DueDate);

                // Remove old items
                _db.TrnSalesInvoiceItems.RemoveRange(invoice.TrnSalesInvoiceItems);
            }

            // Add items
            decimal subtotal = 0, totalDiscount = 0, totalTax = 0;
            int seq = 1;
            foreach (var item in request.Items)
            {
                var lineItem = new TrnSalesInvoiceItem
                {
                    SalesInvoiceId = invoice.SalesInvoiceId,
                    ItemSequence = seq++,
                    Description = item.Description,
                    HsnSacCode = item.HsnSacCode,
                    Quantity = item.Quantity,
                    UnitRate = item.UnitRate,
                    DiscountPercent = item.DiscountPercent,
                    DiscountAmount = item.DiscountAmount,
                    TaxableValue = item.TaxableValue,
                    CgstPercent = item.CgstPercent,
                    CgstAmount = item.CgstAmount,
                    SgstPercent = item.SgstPercent,
                    SgstAmount = item.SgstAmount,
                    IgstPercent = item.IgstPercent,
                    IgstAmount = item.IgstAmount,
                    TotalTaxAmount = item.TotalTaxAmount,
                    LineTotal = item.LineTotal
                };
                _db.TrnSalesInvoiceItems.Add(lineItem);
                subtotal += item.TaxableValue;
                totalDiscount += item.DiscountAmount;
                totalTax += item.TotalTaxAmount;
            }

            invoice.SubtotalAmount = subtotal + totalDiscount;
            invoice.DiscountAmount = totalDiscount;
            invoice.TaxableAmount = subtotal;
            invoice.CgstAmount = request.Items.Sum(i => i.CgstAmount);
            invoice.SgstAmount = request.Items.Sum(i => i.SgstAmount);
            invoice.IgstAmount = request.Items.Sum(i => i.IgstAmount);
            invoice.TotalTaxAmount = totalTax;
            invoice.RoundOff = request.RoundOff;
            invoice.GrandTotal = subtotal + totalTax + request.RoundOff;
            invoice.BalanceAmount = invoice.GrandTotal - (invoice.PaidAmount ?? 0);
            invoice.TermsConditions = request.TermsConditions;
            invoice.InternalNotes = request.InternalNotes;

            await _db.SaveChangesAsync();

            // Job timeline entry
            if (invoice.JobId.HasValue)
            {
                _db.TrnJobTimelines.Add(new TrnJobTimeline
                {
                    JobId = invoice.JobId.Value,
                    EventType = "INVOICE",
                    EventCode = SubProcessCode.CreateSalesInvoice,
                    EventTitle = isNew ? "Sales Invoice Created" : "Sales Invoice Updated",
                    EventDescription = $"Invoice {invoice.InvoiceNo} — ₹{invoice.GrandTotal:N2}",
                    NewStatus = invoice.Status,
                    NewAmount = invoice.GrandTotal,
                    CreatedBy = user.UserId,
                    CreatedOn = DateTime.Now,
                    IsActive = true
                });
                await _db.SaveChangesAsync();
            }

            // Activity log
            var activity = ActivityLogEntry.FromUser(user, "ACCOUNTING",
                isNew ? "CREATE" : "UPDATE",
                $"{(isNew ? "Created" : "Updated")} Sales Invoice {invoice.InvoiceNo}");
            activity.EntityType = "SALES_INVOICE";
            activity.EntityId = invoice.SalesInvoiceId;
            activity.EntityCode = invoice.InvoiceNo;
            activity.Description = $"Sales Invoice {invoice.InvoiceNo} for ₹{invoice.GrandTotal:N2}";
            await _activityService.LogActivityAsync(activity);

            await txn.CommitAsync();

            return Ok(new
            {
                message = isNew ? "Sales invoice created successfully." : "Sales invoice updated successfully.",
                invoice.SalesInvoiceId,
                invoice.InvoiceNo
            });
        }
        catch (Exception ex)
        {
            await txn.RollbackAsync();
            _logger.LogError(ex, "Error saving sales invoice");
            return StatusCode(500, new { message = "Failed to save sales invoice." });
        }
    }

    [HttpPost("sales-invoice/{id:long}/send-email")]
    public async Task<IActionResult> SendSalesInvoiceEmail(long id)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return Unauthorized(new { message = "Session expired." });

        var inv = await _db.TrnSalesInvoices
            .Include(i => i.Party)
            .Include(i => i.Company)
            .FirstOrDefaultAsync(i => i.SalesInvoiceId == id);

        if (inv == null)
            return NotFound(new { message = "Invoice not found." });

        var customerEmail = inv.Party?.Email;
        if (string.IsNullOrWhiteSpace(customerEmail))
            return BadRequest(new { message = "Customer does not have an email address. Please update the customer record and try again." });

        try
        {
            var config = await _db.MstProcessNotificationConfigs
                .FirstOrDefaultAsync(c => c.ProcessCode == "ACC_RECV"
                    && c.SubprocessCode == SubProcessCode.SendSalesInvoice
                    && c.IsActive);

            var dispatchConfig = config != null
                ? MapToDispatchConfig(config)
                : new ProcessNotificationConfig
                {
                    RecipientType = RecipientType.Both,
                    NotifyClientEmail = true,
                    IsActive = true,
                    ProcessCode = "ACC_RECV",
                    SubProcessCode = SubProcessCode.SendSalesInvoice
                };

            var defaultSubject = $"Invoice {inv.InvoiceNo} from {inv.Company?.Name}";
            var defaultBody = $"Dear {inv.Party?.Name ?? "Customer"},<br/><br/>"
                + $"Please find Invoice <b>{inv.InvoiceNo}</b> dated {inv.InvoiceDate:dd-MMM-yyyy} for <b>₹{inv.GrandTotal:N2}</b>."
                + (inv.DueDate.HasValue ? $"<br/><br/>Due Date: {inv.DueDate.Value:dd-MMM-yyyy}" : "")
                + $"<br/><br/>Thank you,<br/>{inv.Company?.Name ?? ""}";

            var subjectText = defaultSubject;
            var bodyText = defaultBody;

            if (config != null)
            {
                var template = await _db.MstNotificationTemplates
                    .FirstOrDefaultAsync(t => t.TemplateCode == config.TemplateCode && t.IsActive == true);
                if (template != null)
                {
                    subjectText = template.SubjectTemplate ?? defaultSubject;
                    bodyText = template.BodyTemplate ?? defaultBody;
                }
            }

            var context = new NotificationContext
            {
                ClientEmail = customerEmail,
                ClientPhone = inv.Party?.Mobile?.ToString(),
                Variables = new Dictionary<string, string>
                {
                    ["InvoiceNo"] = inv.InvoiceNo,
                    ["InvoiceDate"] = inv.InvoiceDate.ToString("dd-MMM-yyyy"),
                    ["CustomerName"] = inv.Party?.Name ?? "",
                    ["GrandTotal"] = inv.GrandTotal?.ToString("N2") ?? "0.00",
                    ["CompanyName"] = inv.Company?.Name ?? "",
                    ["DueDate"] = inv.DueDate?.ToString("dd-MMM-yyyy") ?? ""
                }
            };

            var notifTemplate = new NotificationTemplate
            {
                Channel = NotificationChannel.Email,
                SubjectTemplate = subjectText,
                BodyTemplate = bodyText
            };

            await _notificationDispatcher.DispatchAsync(dispatchConfig, notifTemplate, context);

            // Activity log — only after successful dispatch
            var activity = ActivityLogEntry.FromUser(user, "ACCOUNTING", "EMAIL",
                $"Sent Sales Invoice {inv.InvoiceNo} to {customerEmail}");
            activity.EntityType = "SALES_INVOICE";
            activity.EntityId = inv.SalesInvoiceId;
            activity.EntityCode = inv.InvoiceNo;
            await _activityService.LogActivityAsync(activity);

            // Job timeline — only after successful dispatch
            if (inv.JobId.HasValue)
            {
                _db.TrnJobTimelines.Add(new TrnJobTimeline
                {
                    JobId = inv.JobId.Value,
                    EventType = "COMMUNICATION",
                    EventCode = SubProcessCode.SendSalesInvoice,
                    EventTitle = "Invoice Emailed to Customer",
                    EventDescription = $"Invoice {inv.InvoiceNo} emailed to {customerEmail}",
                    CommunicationMode = "EMAIL",
                    CommunicationReference = customerEmail,
                    CreatedBy = user.UserId,
                    CreatedOn = DateTime.Now,
                    IsActive = true
                });
                await _db.SaveChangesAsync();
            }

            return Ok(new { message = $"Invoice emailed to {customerEmail}." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending sales invoice email for {InvoiceNo}", inv.InvoiceNo);
            return StatusCode(500, new { message = "Failed to send email. Please try again later." });
        }
    }

    [HttpPost("sales-invoice/{id:long}/cancel")]
    public async Task<IActionResult> CancelSalesInvoice(long id, [FromBody] CancelRequest request)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) return Unauthorized(new { message = "Session expired." });

        var inv = await _db.TrnSalesInvoices.FindAsync(id);
        if (inv == null) return NotFound(new { message = "Invoice not found." });

        inv.Status = "CANCELLED";
        inv.IsCancelled = true;
        inv.CancelledBy = user.UserId;
        inv.CancelledOn = DateTime.Now;
        inv.CancelReason = request.Reason;
        await _db.SaveChangesAsync();

        var activity = ActivityLogEntry.FromUser(user, "ACCOUNTING", "CANCEL", $"Cancelled Sales Invoice {inv.InvoiceNo}");
        activity.EntityType = "SALES_INVOICE";
        activity.EntityId = inv.SalesInvoiceId;
        activity.EntityCode = inv.InvoiceNo;
        await _activityService.LogActivityAsync(activity);

        return Ok(new { message = "Invoice cancelled." });
    }

    [HttpPost("sales-invoice/{id:long}/post")]
    public async Task<IActionResult> PostSalesInvoice(long id)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) return Unauthorized(new { message = "Session expired." });

        var inv = await _db.TrnSalesInvoices
            .Include(i => i.Party)
            .FirstOrDefaultAsync(i => i.SalesInvoiceId == id);

        if (inv == null) return NotFound(new { message = "Invoice not found." });

        if (inv.Status != "DRAFT")
            return BadRequest(new { message = $"Only DRAFT invoices can be posted. Current status: {inv.Status}" });

        using var txn = await _db.Database.BeginTransactionAsync();
        try
        {
            // Update invoice status
            inv.Status = "POSTED";
            inv.IsPostedToGl = true;
            inv.GlPostedOn = DateTime.Now;
            inv.GlPostedBy = user.UserId;
            inv.BalanceAmount = inv.GrandTotal - (inv.PaidAmount ?? 0);

            // Create AR Outstanding entry so it appears in Receipt allocation
            var existingAr = await _db.TrnArOutstandings
                .FirstOrDefaultAsync(a => a.DocumentType == "SALES_INVOICE"
                    && a.DocumentId == inv.SalesInvoiceId);

            if (existingAr == null)
            {
                _db.TrnArOutstandings.Add(new TrnArOutstanding
                {
                    CompanyId = inv.CompanyId,
                    PartyId = inv.PartyId,
                    FinYearId = inv.FinYearId,
                    DocumentType = "SALES_INVOICE",
                    DocumentId = inv.SalesInvoiceId,
                    DocumentNo = inv.InvoiceNo,
                    DocumentDate = inv.InvoiceDate,
                    DueDate = inv.DueDate,
                    CurrencyId = inv.CurrencyId,
                    OriginalAmount = inv.GrandTotal ?? 0,
                    PaidAmount = inv.PaidAmount ?? 0,
                    AdjustedAmount = 0,
                    WriteOffAmount = 0,
                    OutstandingAmount = (inv.GrandTotal ?? 0) - (inv.PaidAmount ?? 0),
                    OverdueDays = inv.DueDate.HasValue
                        ? Math.Max(0, (int)(DateOnly.FromDateTime(DateTime.Today).DayNumber - inv.DueDate.Value.DayNumber))
                        : 0,
                    AgingBucket = "CURRENT",
                    IsFullySettled = false,
                    Status = "OPEN",
                    CreatedOn = DateTime.Now
                });
            }

            await _db.SaveChangesAsync();

            // Job timeline entry
            if (inv.JobId.HasValue)
            {
                _db.TrnJobTimelines.Add(new TrnJobTimeline
                {
                    JobId = inv.JobId.Value,
                    EventType = "INVOICE",
                    EventCode = SubProcessCode.PostSalesInvoice,
                    EventTitle = "Sales Invoice Posted to Accounts",
                    EventDescription = $"Invoice {inv.InvoiceNo} — ₹{inv.GrandTotal:N2} posted to accounts receivable",
                    NewStatus = "POSTED",
                    NewAmount = inv.GrandTotal,
                    CreatedBy = user.UserId,
                    CreatedOn = DateTime.Now,
                    IsActive = true
                });
                await _db.SaveChangesAsync();
            }

            // Activity log
            var activity = ActivityLogEntry.FromUser(user, "ACCOUNTING", "POST",
                $"Posted Sales Invoice {inv.InvoiceNo} to accounts");
            activity.EntityType = "SALES_INVOICE";
            activity.EntityId = inv.SalesInvoiceId;
            activity.EntityCode = inv.InvoiceNo;
            activity.Description = $"Sales Invoice {inv.InvoiceNo} — ₹{inv.GrandTotal:N2} posted to AR";
            await _activityService.LogActivityAsync(activity);

            await txn.CommitAsync();

            return Ok(new
            {
                message = $"Invoice {inv.InvoiceNo} posted to accounts successfully. It is now available for receipt collection.",
                inv.SalesInvoiceId,
                inv.InvoiceNo,
                Status = "POSTED"
            });
        }
        catch (Exception ex)
        {
            await txn.RollbackAsync();
            _logger.LogError(ex, "Error posting sales invoice {InvoiceNo}", inv.InvoiceNo);
            return StatusCode(500, new { message = "Failed to post invoice to accounts." });
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // PURCHASE INVOICE
    // ═══════════════════════════════════════════════════════════════

    [HttpGet("purchase-invoices")]
    public async Task<IActionResult> GetPurchaseInvoices()
    {
        var list = await _db.TrnPurchaseInvoices
            .Include(i => i.Party)
            .Include(i => i.TrnPurchaseInvoiceItems)
            .OrderByDescending(i => i.PurchaseInvoiceId)
            .Select(i => new
            {
                i.PurchaseInvoiceId,
                i.InvoiceNo,
                InvoiceDate = i.InvoiceDate.ToString("dd-MMM-yyyy"),
                SupplierName = i.Party != null ? i.Party.Name : "",
                SupplierCode = i.Party != null ? i.Party.Code : "",
                i.SupplierInvoiceNo,
                i.GrandTotal,
                i.PaidAmount,
                i.BalanceAmount,
                i.Status,
                ItemCount = i.TrnPurchaseInvoiceItems.Count,
                CreatedOn = i.CreatedOn.HasValue ? i.CreatedOn.Value.ToString("dd-MMM-yyyy HH:mm") : ""
            })
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("purchase-invoice/{id:long}")]
    public async Task<IActionResult> GetPurchaseInvoiceDetail(long id)
    {
        var inv = await _db.TrnPurchaseInvoices
            .Include(i => i.Party)
            .Include(i => i.Company)
            .Include(i => i.TrnPurchaseInvoiceItems)
            .FirstOrDefaultAsync(i => i.PurchaseInvoiceId == id);

        if (inv == null)
            return NotFound(new { message = "Purchase invoice not found." });

        return Ok(new
        {
            inv.PurchaseInvoiceId,
            inv.InvoiceNo,
            InvoiceDate = inv.InvoiceDate.ToString("dd-MMM-yyyy"),
            DueDate = inv.DueDate?.ToString("dd-MMM-yyyy"),
            SupplierName = inv.Party?.Name,
            SupplierCode = inv.Party?.Code,
            SupplierGst = inv.Party?.Gstno,
            inv.SupplierInvoiceNo,
            inv.PartyId,
            inv.SubtotalAmount,
            inv.DiscountAmount,
            inv.TaxableAmount,
            inv.CgstAmount,
            inv.SgstAmount,
            inv.IgstAmount,
            inv.TotalTaxAmount,
            inv.TdsAmount,
            inv.RoundOff,
            inv.GrandTotal,
            inv.PaidAmount,
            inv.BalanceAmount,
            inv.Status,
            CompanyName = inv.Company?.Name,
            Items = inv.TrnPurchaseInvoiceItems
                .OrderBy(it => it.ItemSequence)
                .Select(it => new
                {
                    it.PurchaseItemId,
                    it.ItemSequence,
                    it.Description,
                    it.HsnSacCode,
                    it.Quantity,
                    it.UnitRate,
                    it.DiscountPercent,
                    it.DiscountAmount,
                    it.TaxableValue,
                    it.CgstPercent,
                    it.CgstAmount,
                    it.SgstPercent,
                    it.SgstAmount,
                    it.IgstPercent,
                    it.IgstAmount,
                    it.TotalTaxAmount,
                    it.LineTotal
                })
        });
    }

    // ═══════════════════════════════════════════════════════════════
    // RECEIPTS (Payments from Customers)
    // ═══════════════════════════════════════════════════════════════

    [HttpGet("receipts")]
    public async Task<IActionResult> GetReceipts()
    {
        var list = await _db.TrnReceipts
            .Include(r => r.Party)
            .OrderByDescending(r => r.ReceiptId)
            .Select(r => new
            {
                r.ReceiptId,
                r.ReceiptNo,
                ReceiptDate = r.ReceiptDate.ToString("dd-MMM-yyyy"),
                CustomerName = r.Party != null ? r.Party.Name : "",
                r.Amount,
                r.PaymentMode,
                r.Status,
                CreatedOn = r.CreatedOn.HasValue ? r.CreatedOn.Value.ToString("dd-MMM-yyyy HH:mm") : ""
            })
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("receipt/{id:long}")]
    public async Task<IActionResult> GetReceiptDetail(long id)
    {
        var receipt = await _db.TrnReceipts
            .Include(r => r.Party)
            .Include(r => r.Company)
            .Include(r => r.Bank)
            .Include(r => r.TrnReceiptAllocations)
                .ThenInclude(a => a.SalesInvoice)
            .FirstOrDefaultAsync(r => r.ReceiptId == id);

        if (receipt == null)
            return NotFound(new { message = "Receipt not found." });

        return Ok(new
        {
            receipt.ReceiptId,
            receipt.ReceiptNo,
            ReceiptDate = receipt.ReceiptDate.ToString("dd-MMM-yyyy"),
            receipt.Amount,
            receipt.PaymentMode,
            receipt.ReferenceNo,
            ReferenceDate = receipt.ReferenceDate?.ToString("dd-MMM-yyyy"),
            receipt.Remarks,
            receipt.Status,
            CustomerName = receipt.Party?.Name,
            CustomerCode = receipt.Party?.Code,
            CustomerEmail = receipt.Party?.Email,
            CustomerPhone = receipt.Party?.Mobile?.ToString(),
            CustomerGst = receipt.Party?.Gstno,
            CompanyName = receipt.Company?.Name,
            CompanyEmail = receipt.Company?.EmailId,
            CompanyPhone = receipt.Company?.ContactNo,
            CompanyGstin = receipt.Company?.Gstin,
            CompanyAddress = receipt.Company != null
                ? $"{receipt.Company.AddressLine1}{(receipt.Company.AddressLine2 != null ? ", " + receipt.Company.AddressLine2 : "")}"
                : "",
            BankName = receipt.Bank?.BankName,
            BankAccount = receipt.Bank?.AccountName,
            CreatedOn = receipt.CreatedOn?.ToString("dd-MMM-yyyy HH:mm"),
            Allocations = receipt.TrnReceiptAllocations.Select(a => new
            {
                a.ReceiptAllocationId,
                a.SalesInvoiceId,
                InvoiceNo = a.SalesInvoice?.InvoiceNo,
                InvoiceDate = a.SalesInvoice?.InvoiceDate.ToString("dd-MMM-yyyy"),
                InvoiceTotal = a.SalesInvoice?.GrandTotal,
                a.AllocatedAmount
            })
        });
    }

    [HttpPost("receipt/{id:long}/send-email")]
    public async Task<IActionResult> SendReceiptEmail(long id)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) return Unauthorized(new { message = "Session expired." });

        var receipt = await _db.TrnReceipts
            .Include(r => r.Party)
            .Include(r => r.Company)
            .FirstOrDefaultAsync(r => r.ReceiptId == id);

        if (receipt == null) return NotFound(new { message = "Receipt not found." });

        var customerEmail = receipt.Party?.Email;
        if (string.IsNullOrWhiteSpace(customerEmail))
            return BadRequest(new { message = "Customer does not have an email address. Please update the customer record and try again." });

        try
        {
            var notifConfig = await _db.MstProcessNotificationConfigs
                .FirstOrDefaultAsync(c => c.ProcessCode == "ACC_RECV"
                    && c.SubprocessCode == SubProcessCode.ReceivePayment
                    && c.IsActive);

            var dispatchConfig = notifConfig != null
                ? MapToDispatchConfig(notifConfig)
                : new ProcessNotificationConfig
                {
                    RecipientType = RecipientType.Both,
                    NotifyClientEmail = true,
                    IsActive = true,
                    ProcessCode = "ACC_RECV",
                    SubProcessCode = SubProcessCode.ReceivePayment
                };

            var defaultSubject = $"Payment Receipt {receipt.ReceiptNo} from {receipt.Company?.Name}";
            var defaultBody = $"Dear {receipt.Party?.Name ?? "Customer"},<br/><br/>"
                + $"We have received your payment of <b>₹{receipt.Amount:N2}</b> via {receipt.PaymentMode ?? "Cash"}."
                + $"<br/><br/>Receipt No: <b>{receipt.ReceiptNo}</b>"
                + $"<br/>Date: {receipt.ReceiptDate:dd-MMM-yyyy}"
                + (!string.IsNullOrEmpty(receipt.ReferenceNo) ? $"<br/>Reference: {receipt.ReferenceNo}" : "")
                + $"<br/><br/>Thank you,<br/>{receipt.Company?.Name ?? ""}";

            var subjectText = defaultSubject;
            var bodyText = defaultBody;

            if (notifConfig != null)
            {
                var template = await _db.MstNotificationTemplates
                    .FirstOrDefaultAsync(t => t.TemplateCode == notifConfig.TemplateCode && t.IsActive == true);
                if (template != null)
                {
                    subjectText = template.SubjectTemplate ?? defaultSubject;
                    bodyText = template.BodyTemplate ?? defaultBody;
                }
            }

            var context = new NotificationContext
            {
                ClientEmail = customerEmail,
                ClientPhone = receipt.Party?.Mobile?.ToString(),
                Variables = new Dictionary<string, string>
                {
                    ["ReceiptNo"] = receipt.ReceiptNo,
                    ["ReceiptDate"] = receipt.ReceiptDate.ToString("dd-MMM-yyyy"),
                    ["CustomerName"] = receipt.Party?.Name ?? "",
                    ["Amount"] = receipt.Amount.ToString("N2"),
                    ["PaymentMode"] = receipt.PaymentMode ?? "",
                    ["CompanyName"] = receipt.Company?.Name ?? "",
                    ["ReferenceNo"] = receipt.ReferenceNo ?? ""
                }
            };

            var notifTemplate = new NotificationTemplate
            {
                Channel = NotificationChannel.Email,
                SubjectTemplate = subjectText,
                BodyTemplate = bodyText
            };

            await _notificationDispatcher.DispatchAsync(dispatchConfig, notifTemplate, context);

            // Activity log — only after successful dispatch
            var activity = ActivityLogEntry.FromUser(user, "ACCOUNTING", "EMAIL",
                $"Sent Receipt {receipt.ReceiptNo} to {customerEmail}");
            activity.EntityType = "RECEIPT";
            activity.EntityId = receipt.ReceiptId;
            activity.EntityCode = receipt.ReceiptNo;
            await _activityService.LogActivityAsync(activity);

            return Ok(new { message = $"Receipt emailed to {customerEmail}." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending receipt email for {ReceiptNo}", receipt.ReceiptNo);
            return StatusCode(500, new { message = "Failed to send email. Please try again later." });
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // PAYMENTS (Payments to Suppliers)
    // ═══════════════════════════════════════════════════════════════

    [HttpGet("payments")]
    public async Task<IActionResult> GetPayments()
    {
        var list = await _db.TrnPayments
            .Include(p => p.Party)
            .OrderByDescending(p => p.PaymentId)
            .Select(p => new
            {
                p.PaymentId,
                p.PaymentNo,
                PaymentDate = p.PaymentDate.ToString("dd-MMM-yyyy"),
                SupplierName = p.Party != null ? p.Party.Name : "",
                p.Amount,
                p.PaymentMode,
                p.Status,
                CreatedOn = p.CreatedOn.HasValue ? p.CreatedOn.Value.ToString("dd-MMM-yyyy HH:mm") : ""
            })
            .ToListAsync();

        return Ok(list);
    }

    // ═══════════════════════════════════════════════════════════════
    // CREDIT NOTES
    // ═══════════════════════════════════════════════════════════════

    [HttpGet("credit-notes")]
    public async Task<IActionResult> GetCreditNotes()
    {
        var list = await _db.TrnCreditNotes
            .Include(c => c.Party)
            .OrderByDescending(c => c.CreditNoteId)
            .Select(c => new
            {
                c.CreditNoteId,
                c.CreditNoteNo,
                CreditNoteDate = c.CreditNoteDate.ToString("dd-MMM-yyyy"),
                CustomerName = c.Party != null ? c.Party.Name : "",
                c.CreditNoteType,
                c.GrandTotal,
                c.AdjustedAmount,
                c.UnadjustedAmount,
                c.Status,
                c.OriginalInvoiceNo
            })
            .ToListAsync();

        return Ok(list);
    }

    // ═══════════════════════════════════════════════════════════════
    // DEBIT NOTES
    // ═══════════════════════════════════════════════════════════════

    [HttpGet("debit-notes")]
    public async Task<IActionResult> GetDebitNotes()
    {
        var list = await _db.TrnDebitNotes
            .Include(d => d.Party)
            .OrderByDescending(d => d.DebitNoteId)
            .Select(d => new
            {
                d.DebitNoteId,
                d.DebitNoteNo,
                DebitNoteDate = d.DebitNoteDate.ToString("dd-MMM-yyyy"),
                SupplierName = d.Party != null ? d.Party.Name : "",
                d.DebitNoteType,
                d.GrandTotal,
                d.AdjustedAmount,
                d.UnadjustedAmount,
                d.Status,
                d.OriginalInvoiceNo
            })
            .ToListAsync();

        return Ok(list);
    }

    // ═══════════════════════════════════════════════════════════════
    // JOURNAL VOUCHERS
    // ═══════════════════════════════════════════════════════════════

    [HttpGet("journal-vouchers")]
    public async Task<IActionResult> GetJournalVouchers()
    {
        var list = await _db.TrnJournalVouchers
            .OrderByDescending(j => j.JournalId)
            .Select(j => new
            {
                j.JournalId,
                j.JournalNo,
                JournalDate = j.JournalDate.ToString("dd-MMM-yyyy"),
                j.JournalType,
                j.TotalDebit,
                j.TotalCredit,
                j.Narration,
                j.Status,
                j.IsAutoGenerated,
                j.SourceVoucherNo
            })
            .ToListAsync();

        return Ok(list);
    }

    // ═══════════════════════════════════════════════════════════════
    // EXPENSE VOUCHERS
    // ═══════════════════════════════════════════════════════════════

    [HttpGet("expense-vouchers")]
    public async Task<IActionResult> GetExpenseVouchers()
    {
        var list = await _db.TrnExpenseVouchers
            .OrderByDescending(e => e.ExpenseVoucherId)
            .Select(e => new
            {
                e.ExpenseVoucherId,
                e.VoucherNo,
                VoucherDate = e.VoucherDate.ToString("dd-MMM-yyyy"),
                e.ExpenseCategory,
                e.PaymentMode,
                e.GrandTotal,
                e.Status,
                e.IsApproved,
                e.Narration
            })
            .ToListAsync();

        return Ok(list);
    }

    // ═══════════════════════════════════════════════════════════════
    // BANK RECONCILIATION
    // ═══════════════════════════════════════════════════════════════

    [HttpGet("bank-reconciliations")]
    public async Task<IActionResult> GetBankReconciliations()
    {
        var list = await _db.TrnBankReconciliations
            .OrderByDescending(r => r.ReconciliationId)
            .Select(r => new
            {
                r.ReconciliationId,
                r.ReconciliationNo,
                StatementDate = r.StatementDate.ToString("dd-MMM-yyyy"),
                r.StatementBalance,
                r.BookBalance,
                r.ReconciledBalance,
                r.DifferenceAmount,
                r.TotalItems,
                r.ReconciledItems,
                r.PendingItems,
                r.Status
            })
            .ToListAsync();

        return Ok(list);
    }

    // ═══════════════════════════════════════════════════════════════
    // PURCHASE ORDERS
    // ═══════════════════════════════════════════════════════════════

    [HttpGet("purchase-orders")]
    public async Task<IActionResult> GetPurchaseOrders()
    {
        var list = await _db.TrnPurchaseOrders
            .Include(p => p.Party)
            .OrderByDescending(p => p.PurchaseOrderId)
            .Select(p => new
            {
                p.PurchaseOrderId,
                p.PoNo,
                PoDate = p.PoDate.ToString("dd-MMM-yyyy"),
                SupplierName = p.Party != null ? p.Party.Name : "",
                p.GrandTotal,
                p.Status,
                p.IsApproved,
                ExpectedDelivery = p.ExpectedDeliveryDate.HasValue ? p.ExpectedDeliveryDate.Value.ToString("dd-MMM-yyyy") : ""
            })
            .ToListAsync();

        return Ok(list);
    }

    // ═══════════════════════════════════════════════════════════════
    // GOODS RECEIPT NOTES
    // ═══════════════════════════════════════════════════════════════

    [HttpGet("goods-receipts")]
    public async Task<IActionResult> GetGoodsReceipts()
    {
        var list = await _db.TrnGoodsReceipts
            .Include(g => g.Party)
            .OrderByDescending(g => g.GrnId)
            .Select(g => new
            {
                g.GrnId,
                g.GrnNo,
                GrnDate = g.GrnDate.ToString("dd-MMM-yyyy"),
                SupplierName = g.Party != null ? g.Party.Name : "",
                g.PoNo,
                g.TotalQuantity,
                g.TotalAcceptedQty,
                g.TotalRejectedQty,
                g.Status
            })
            .ToListAsync();

        return Ok(list);
    }

    // ═══════════════════════════════════════════════════════════════
    // AR / AP OUTSTANDING
    // ═══════════════════════════════════════════════════════════════

    [HttpGet("ar-outstanding")]
    public async Task<IActionResult> GetArOutstanding([FromQuery] int? partyId)
    {
        var query = _db.TrnArOutstandings
            .Include(a => a.Party)
            .Where(a => a.Status == "OPEN");

        if (partyId.HasValue)
            query = query.Where(a => a.PartyId == partyId.Value);

        var list = await query
            .OrderBy(a => a.DueDate)
            .Select(a => new
            {
                a.ArId,
                CustomerName = a.Party != null ? a.Party.Name : "",
                a.DocumentType,
                a.DocumentNo,
                DocumentDate = a.DocumentDate.ToString("dd-MMM-yyyy"),
                DueDate = a.DueDate.HasValue ? a.DueDate.Value.ToString("dd-MMM-yyyy") : "",
                a.OriginalAmount,
                a.PaidAmount,
                a.AdjustedAmount,
                a.OutstandingAmount,
                a.OverdueDays,
                a.AgingBucket,
                a.Status
            })
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("ap-outstanding")]
    public async Task<IActionResult> GetApOutstanding([FromQuery] int? partyId)
    {
        var query = _db.TrnApOutstandings
            .Include(a => a.Party)
            .Where(a => a.Status == "OPEN");

        if (partyId.HasValue)
            query = query.Where(a => a.PartyId == partyId.Value);

        var list = await query
            .OrderBy(a => a.DueDate)
            .Select(a => new
            {
                a.ApId,
                SupplierName = a.Party != null ? a.Party.Name : "",
                a.DocumentType,
                a.DocumentNo,
                DocumentDate = a.DocumentDate.ToString("dd-MMM-yyyy"),
                DueDate = a.DueDate.HasValue ? a.DueDate.Value.ToString("dd-MMM-yyyy") : "",
                a.OriginalAmount,
                a.PaidAmount,
                a.AdjustedAmount,
                a.TdsAmount,
                a.OutstandingAmount,
                a.OverdueDays,
                a.AgingBucket,
                a.Status
            })
            .ToListAsync();

        return Ok(list);
    }

    // ═══════════════════════════════════════════════════════════════
    // OUTSOURCE PAYMENT (for outsource list integration)
    // ═══════════════════════════════════════════════════════════════

    [HttpPost("outsource-payment")]
    public async Task<IActionResult> RecordOutsourcePayment([FromBody] OutsourcePaymentRequest request)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) return Unauthorized(new { message = "Session expired." });

        var outsource = await _db.TrnJobOutsources
            .Include(o => o.Job)
            .FirstOrDefaultAsync(o => o.OutsourceId == request.OutsourceId);

        if (outsource == null) return NotFound(new { message = "Outsource not found." });

        using var txn = await _db.Database.BeginTransactionAsync();
        try
        {
            var paymentNo = await _documentNumberService.GenerateNextNumberAsync("PAYMENT");

            var payment = new TrnPayment
            {
                PaymentNo = paymentNo,
                PaymentDate = DateOnly.FromDateTime(DateTime.Today),
                CompanyId = user.CompanyId ?? 1,
                PartyId = (int)outsource.VendorId,
                Amount = request.Amount,
                PaymentMode = request.PaymentMode ?? "BANK_TRANSFER",
                Status = "POSTED",
                Remarks = $"Payment for Outsource {outsource.OutsourceNo}",
                CreatedBy = user.UserId,
                CreatedOn = DateTime.Now
            };

            _db.TrnPayments.Add(payment);
            await _db.SaveChangesAsync();

            // Activity log
            var activity = ActivityLogEntry.FromUser(user, "ACCOUNTING", "CREATE",
                $"Payment {paymentNo} for outsource {outsource.OutsourceNo}");
            activity.EntityType = "PAYMENT";
            activity.EntityId = payment.PaymentId;
            activity.EntityCode = payment.PaymentNo;
            activity.JobId = outsource.JobId;
            await _activityService.LogActivityAsync(activity);

            // Job timeline
            if (outsource.JobId > 0)
            {
                _db.TrnJobTimelines.Add(new TrnJobTimeline
                {
                    JobId = outsource.JobId,
                    EventType = "PAYMENT",
                    EventCode = SubProcessCode.MakePayment,
                    EventTitle = "Outsource Payment Made",
                    EventDescription = $"Payment {paymentNo} — ₹{request.Amount:N2} for {outsource.OutsourceNo}",
                    NewAmount = request.Amount,
                    CreatedBy = user.UserId,
                    CreatedOn = DateTime.Now,
                    IsActive = true
                });
                await _db.SaveChangesAsync();
            }

            await txn.CommitAsync();
            return Ok(new { message = $"Payment {paymentNo} recorded.", payment.PaymentId, payment.PaymentNo });
        }
        catch (Exception ex)
        {
            await txn.RollbackAsync();
            _logger.LogError(ex, "Error recording outsource payment");
            return StatusCode(500, new { message = "Failed to record payment." });
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // CREATE INVOICE FROM JOB / ENQUIRY
    // ═══════════════════════════════════════════════════════════════

    [HttpGet("job-invoice-data/{jobId:long}")]
    public async Task<IActionResult> GetJobInvoiceData(long jobId)
    {
        var job = await _db.TrnJobs
            .Include(j => j.Party)
            .Include(j => j.Company)
            .Include(j => j.TrnJobItems)
            .FirstOrDefaultAsync(j => j.JobId == jobId);

        if (job == null) return NotFound(new { message = "Job not found." });

        return Ok(new
        {
            job.JobId,
            job.JobNo,
            job.PartyId,
            CustomerName = job.Party?.Name,
            CompanyId = job.CompanyId,
            Items = job.TrnJobItems.OrderBy(i => i.ItemSequence).Select(i => new
            {
                Description = $"{i.ProductName} {i.ProductDescription}".Trim(),
                HsnSacCode = "",
                Quantity = i.Quantity ?? 0,
                UnitRate = i.UnitRate ?? 0,
                DiscountPercent = i.DiscountPercent ?? 0,
                DiscountAmount = i.DiscountAmount ?? 0,
                TaxableValue = i.TaxableValue ?? 0,
                CgstPercent = i.CgstPercent ?? 0,
                CgstAmount = i.CgstAmount ?? 0,
                SgstPercent = i.SgstPercent ?? 0,
                SgstAmount = i.SgstAmount ?? 0,
                IgstPercent = i.IgstPercent ?? 0,
                IgstAmount = i.IgstAmount ?? 0,
                TotalTaxAmount = i.TotalTaxAmount ?? 0,
                LineTotal = i.NetAmount ?? 0
            })
        });
    }

    [HttpGet("enquiry-invoice-data/{enquiryId:long}")]
    public async Task<IActionResult> GetEnquiryInvoiceData(long enquiryId)
    {
        var enquiry = await _db.TrnEnquiries
            .Include(e => e.Party)
            .Include(e => e.TrnEnquiryItems)
            .FirstOrDefaultAsync(e => e.EnquiryId == enquiryId);

        if (enquiry == null) return NotFound(new { message = "Enquiry not found." });

        return Ok(new
        {
            enquiry.EnquiryId,
            enquiry.EnquiryNo,
            enquiry.PartyId,
            CustomerName = enquiry.Party?.Name,
            CompanyId = enquiry.CompanyId,
            Items = enquiry.TrnEnquiryItems.OrderBy(i => i.ItemSequence).Select(i => new
            {
                Description = $"{i.ProductName} {i.ProductDescription}".Trim(),
                HsnSacCode = "",
                Quantity = (decimal)i.Quantity,
                UnitRate = 0m,
                DiscountPercent = 0m,
                DiscountAmount = 0m,
                TaxableValue = 0m,
                CgstPercent = 0m,
                CgstAmount = 0m,
                SgstPercent = 0m,
                SgstAmount = 0m,
                IgstPercent = 0m,
                IgstAmount = 0m,
                TotalTaxAmount = 0m,
                LineTotal = 0m
            })
        });
    }

    // ═══════════════════════════════════════════════════════════════
    // LOOKUPS (for dropdowns)
    // ═══════════════════════════════════════════════════════════════

    [HttpGet("parties")]
    public async Task<IActionResult> GetParties([FromQuery] string? q)
    {
        var query = _db.MstParties.Where(p => p.IsActive);
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(p => p.Name.Contains(q) || (p.Code != null && p.Code.Contains(q)));

        var list = await query
            .OrderBy(p => p.Name)
            .Take(50)
            .Select(p => new { id = p.Id, text = p.Name, code = p.Code })
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("bank-accounts")]
    public async Task<IActionResult> GetBankAccounts()
    {
        var list = await _db.MstBankAccounts
            .Where(b => b.IsActive == true)
            .OrderBy(b => b.AccountName)
            .Select(b => new { id = b.BankAccountId, text = $"{b.AccountName} — {b.BankName}", accountNo = b.AccountNo })
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("employees")]
    public async Task<IActionResult> GetEmployees([FromQuery] string? q)
    {
        var query = _db.MstEmployees.Where(e => e.IsActive == true);
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(e =>
                (e.FirstName != null && e.FirstName.Contains(q)) ||
                (e.LastName != null && e.LastName.Contains(q)) ||
                e.EmpCode.Contains(q));

        var list = await query
            .OrderBy(e => e.FirstName)
            .Take(50)
            .Select(e => new
            {
                id = e.EmployeeId,
                text = (e.FirstName ?? "") + " " + (e.LastName ?? ""),
                code = e.EmpCode,
                email = e.Email1,
                mobile = e.MobileNo1
            })
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("outsource-vendors")]
    public async Task<IActionResult> GetOutsourceVendors([FromQuery] string? q)
    {
        var query = from o in _db.TrnJobOutsources
                    join p in _db.MstParties on o.VendorId equals p.Id
                    where p.IsActive
                    select new { o.OutsourceId, o.OutsourceNo, o.JobId, VendorName = p.Name, VendorCode = p.Code, VendorEmail = p.Email, VendorMobile = p.Mobile, p.Id };

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(x => x.VendorName.Contains(q) || x.OutsourceNo.Contains(q) || (x.VendorCode != null && x.VendorCode.Contains(q)));

        var list = await query
            .OrderByDescending(x => x.OutsourceId)
            .Take(50)
            .Select(x => new
            {
                id = x.OutsourceId,
                text = $"{x.VendorName} — {x.OutsourceNo}",
                vendorName = x.VendorName,
                outsourceNo = x.OutsourceNo,
                jobId = x.JobId,
                partyId = x.Id,
                email = x.VendorEmail,
                mobile = x.VendorMobile
            })
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("payment-terms")]
    public async Task<IActionResult> GetPaymentTerms()
    {
        var list = await _db.MstPaymentTerms
            .Where(t => t.IsActive == true)
            .OrderBy(t => t.Name)
            .Select(t => new { id = t.PaymentTermId, text = t.Name, code = t.Code, dueDays = t.DueDays })
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("party-detail/{partyId:int}")]
    public async Task<IActionResult> GetPartyDetail(int partyId)
    {
        var party = await _db.MstParties
            .FirstOrDefaultAsync(p => p.Id == partyId);

        if (party == null) return NotFound(new { message = "Party not found." });

        var addresses = await _db.MstPartyAddresses
            .Where(a => a.PartyId == partyId && a.IsActive == true)
            .Include(a => a.State)
            .Include(a => a.City)
            .OrderByDescending(a => a.IsDefault)
            .Select(a => new
            {
                a.AddressId,
                a.AddressType,
                a.AddressLabel,
                a.IsDefault,
                a.AddressLine1,
                a.AddressLine2,
                a.Landmark,
                StateName = a.State != null ? a.State.Name : "",
                StateId = a.StateId,
                GstStateCode = a.State != null ? a.State.GstStateCode : "",
                CityName = a.City != null ? a.City.Name : "",
                a.DistrictName,
                a.PostalCode,
                a.Gstin,
                a.ContactPersonName,
                a.ContactPhone,
                a.ContactEmail
            })
            .ToListAsync();

        var billingAddr = addresses.FirstOrDefault(a => a.AddressType == "BILLING")
                       ?? addresses.FirstOrDefault(a => a.IsDefault == true)
                       ?? addresses.FirstOrDefault();
        var shippingAddr = addresses.FirstOrDefault(a => a.AddressType == "SHIPPING")
                        ?? billingAddr;

        // Get company state for SGST/IGST logic
        var user = HttpContext.Session.GetCurrentUser();
        var companyStateId = await _db.MstCompanies
            .Where(c => c.Id == (user != null ? user.CompanyId ?? 1 : 1))
            .Select(c => c.StateId)
            .FirstOrDefaultAsync();

        var isSameState = billingAddr?.StateId != null && billingAddr.StateId == companyStateId;

        return Ok(new
        {
            party.Id,
            party.Name,
            party.Code,
            party.Email,
            Mobile = party.Mobile?.ToString(),
            party.Gstno,
            party.PanNo,
            billingAddress = billingAddr,
            shippingAddress = shippingAddr,
            addresses,
            companyStateId,
            isSameState,
            taxType = isSameState ? "INTRA_STATE" : "INTER_STATE"
        });
    }

    [HttpGet("pending-jobs/{partyId:int}")]
    public async Task<IActionResult> GetPendingJobs(int partyId)
    {
        // Jobs for this party that don't have a fully-paid sales invoice
        var invoicedJobIds = await _db.TrnSalesInvoices
            .Where(i => i.PartyId == partyId && i.Status != "CANCELLED")
            .Select(i => i.JobId)
            .Where(id => id != null)
            .ToListAsync();

        var jobs = await _db.TrnJobs
            .Include(j => j.TrnJobItems)
            .Where(j => j.PartyId == partyId
                && j.StatusCode != "CANCELLED"
                && !invoicedJobIds.Contains(j.JobId))
            .OrderByDescending(j => j.JobDate)
            .Select(j => new
            {
                j.JobId,
                j.JobNo,
                JobDate = j.JobDate.ToString("dd-MMM-yyyy"),
                j.ProductName,
                j.ProductDescription,
                j.Quantity,
                j.StatusCode,
                j.CurrentStage,
                j.ProgressPercent,
                GrossAmount = j.GrossAmount ?? 0,
                DiscountAmount = j.DiscountAmount ?? 0,
                TaxableAmount = j.TaxableAmount ?? 0,
                TaxAmount = j.TaxAmount ?? 0,
                NetAmount = j.NetAmount ?? 0,
                Items = j.TrnJobItems.OrderBy(i => i.ItemSequence).Select(i => new
                {
                    Description = $"{i.ProductName} {i.ProductDescription}".Trim(),
                    i.HsnSacCode,
                    Quantity = i.Quantity ?? 0,
                    UnitRate = i.UnitRate ?? 0,
                    DiscountPercent = i.DiscountPercent ?? 0,
                    DiscountAmount = i.DiscountAmount ?? 0,
                    TaxableValue = i.TaxableValue ?? 0,
                    CgstPercent = i.CgstPercent ?? 0,
                    CgstAmount = i.CgstAmount ?? 0,
                    SgstPercent = i.SgstPercent ?? 0,
                    SgstAmount = i.SgstAmount ?? 0,
                    IgstPercent = i.IgstPercent ?? 0,
                    IgstAmount = i.IgstAmount ?? 0,
                    TotalTaxAmount = i.TotalTaxAmount ?? 0,
                    LineTotal = i.NetAmount ?? 0
                })
            })
            .ToListAsync();

        return Ok(jobs);
    }

    // ═══════════════════════════════════════════════════════════════
    // SAVE — RECEIPT
    // ═══════════════════════════════════════════════════════════════

    [HttpPost("receipt/save")]
    public async Task<IActionResult> SaveReceipt([FromBody] ReceiptSaveRequest request)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) return Unauthorized(new { message = "Session expired." });

        // ── Pre-transaction validation ──────────────────────────────

        // 1. Receipt amount
        if (request.Amount <= 0)
            return BadRequest(new { message = "Receipt amount must be greater than zero." });

        // 2. Receipt date
        if (string.IsNullOrWhiteSpace(request.ReceiptDate) || !DateOnly.TryParse(request.ReceiptDate, out var parsedReceiptDate))
            return BadRequest(new { message = "Invalid or missing receipt date." });

        if (parsedReceiptDate > DateOnly.FromDateTime(DateTime.Today))
            return BadRequest(new { message = "Receipt date cannot be a future date." });

        // 3. Reference date (optional, but validate if provided)
        DateOnly? parsedRefDate = null;
        if (!string.IsNullOrWhiteSpace(request.ReferenceDate))
        {
            if (!DateOnly.TryParse(request.ReferenceDate, out var refDate))
                return BadRequest(new { message = "Invalid reference date format." });
            parsedRefDate = refDate;
        }

        // 4. Party / customer
        if (request.PartyId <= 0)
            return BadRequest(new { message = "Customer is required." });

        var party = await _db.MstParties.FirstOrDefaultAsync(p => p.Id == request.PartyId);
        if (party == null)
            return BadRequest(new { message = "Selected customer does not exist." });

        if (!party.IsActive)
            return BadRequest(new { message = $"Customer '{party.Name}' is inactive and cannot receive payments." });

        // 5. Payment mode
        var validPaymentModes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "CASH", "BANK_TRANSFER", "CHEQUE", "UPI", "NEFT", "RTGS" };
        var paymentMode = request.PaymentMode ?? "CASH";
        if (!validPaymentModes.Contains(paymentMode))
            return BadRequest(new { message = $"Invalid payment mode '{paymentMode}'." });

        // 6. Bank account required for non-cash modes
        if (paymentMode != "CASH")
        {
            if (!request.BankId.HasValue || request.BankId <= 0)
                return BadRequest(new { message = $"Bank account is required for {paymentMode} payment mode." });

            var bankExists = await _db.MstBankAccounts.AnyAsync(b => b.BankAccountId == request.BankId && b.IsActive == true);
            if (!bankExists)
                return BadRequest(new { message = "Selected bank account does not exist or is inactive." });
        }

        // 7. Allocation-level validations (before touching any data)
        var validAllocations = request.Allocations?.Where(a => a.AllocatedAmount > 0).ToList() ?? [];

        if (validAllocations.Any(a => a.AllocatedAmount < 0))
            return BadRequest(new { message = "Allocated amount cannot be negative." });

        var totalAllocated = validAllocations.Sum(a => a.AllocatedAmount);
        if (totalAllocated > request.Amount)
            return BadRequest(new { message = $"Total allocated amount (₹{totalAllocated:N2}) exceeds the receipt amount (₹{request.Amount:N2})." });

        // Check for duplicate allocations (same invoice/AR referenced twice)
        var duplicateInvoiceIds = validAllocations
            .Where(a => a.InvoiceId > 0)
            .GroupBy(a => a.InvoiceId)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();
        if (duplicateInvoiceIds.Count > 0)
            return BadRequest(new { message = "Duplicate invoice allocations are not allowed." });

        var duplicateArIds = validAllocations
            .Where(a => a.ArId > 0)
            .GroupBy(a => a.ArId)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();
        if (duplicateArIds.Count > 0)
            return BadRequest(new { message = "Duplicate AR outstanding allocations are not allowed." });

        // 8. Validate each allocation against the invoice and AR records
        var allocationErrors = new List<string>();
        var resolvedAllocations = new List<(ReceiptAllocationRequest Alloc, long SalesInvoiceId, TrnSalesInvoice Invoice, TrnArOutstanding? ArRecord)>();

        foreach (var alloc in validAllocations)
        {
            long salesInvoiceId = alloc.InvoiceId;
            TrnArOutstanding? arRecord = null;

            // Validate AR outstanding record
            if (alloc.ArId > 0)
            {
                arRecord = await _db.TrnArOutstandings.FindAsync(alloc.ArId);
                if (arRecord == null)
                {
                    allocationErrors.Add($"AR outstanding record (ID: {alloc.ArId}) not found.");
                    continue;
                }

                if (arRecord.Status != "OPEN")
                {
                    allocationErrors.Add($"AR record for '{arRecord.DocumentNo}' is already {arRecord.Status}.");
                    continue;
                }

                if (arRecord.PartyId != request.PartyId)
                {
                    allocationErrors.Add($"AR record '{arRecord.DocumentNo}' does not belong to the selected customer.");
                    continue;
                }

                var arOutstanding = arRecord.OriginalAmount - (arRecord.PaidAmount ?? 0) - (arRecord.AdjustedAmount ?? 0) - (arRecord.WriteOffAmount ?? 0);
                if (alloc.AllocatedAmount > arOutstanding)
                {
                    allocationErrors.Add($"Allocated amount (₹{alloc.AllocatedAmount:N2}) exceeds outstanding balance (₹{arOutstanding:N2}) for '{arRecord.DocumentNo}'.");
                    continue;
                }

                salesInvoiceId = arRecord.DocumentId;

                // Cross-check: if InvoiceId was also provided, it must match
                if (alloc.InvoiceId > 0 && alloc.InvoiceId != salesInvoiceId)
                {
                    allocationErrors.Add($"Invoice ID mismatch for AR record '{arRecord.DocumentNo}'. Expected invoice {salesInvoiceId} but received {alloc.InvoiceId}.");
                    continue;
                }
            }

            if (salesInvoiceId <= 0)
            {
                allocationErrors.Add("Allocation is missing both AR ID and Invoice ID.");
                continue;
            }

            // Validate the sales invoice
            var invoice = await _db.TrnSalesInvoices.FindAsync(salesInvoiceId);
            if (invoice == null)
            {
                allocationErrors.Add($"Sales Invoice (ID: {salesInvoiceId}) not found.");
                continue;
            }

            if (invoice.PartyId != request.PartyId)
            {
                allocationErrors.Add($"Invoice '{invoice.InvoiceNo}' does not belong to the selected customer.");
                continue;
            }

            if (invoice.IsCancelled == true || invoice.Status == "CANCELLED")
            {
                allocationErrors.Add($"Invoice '{invoice.InvoiceNo}' is cancelled and cannot receive payments.");
                continue;
            }

            if (invoice.Status == "DRAFT")
            {
                allocationErrors.Add($"Invoice '{invoice.InvoiceNo}' is still in DRAFT status. Post it to accounts before collecting payment.");
                continue;
            }

            if (invoice.Status == "PAID")
            {
                allocationErrors.Add($"Invoice '{invoice.InvoiceNo}' is already fully paid.");
                continue;
            }

            var invoiceBalance = (invoice.GrandTotal ?? 0) - (invoice.PaidAmount ?? 0);
            if (invoiceBalance <= 0)
            {
                allocationErrors.Add($"Invoice '{invoice.InvoiceNo}' has no outstanding balance.");
                continue;
            }

            if (alloc.AllocatedAmount > invoiceBalance)
            {
                allocationErrors.Add($"Allocated amount (₹{alloc.AllocatedAmount:N2}) exceeds invoice balance (₹{invoiceBalance:N2}) for '{invoice.InvoiceNo}'.");
                continue;
            }

            resolvedAllocations.Add((alloc, salesInvoiceId, invoice, arRecord));
        }

        if (allocationErrors.Count > 0)
            return BadRequest(new { message = "Allocation validation failed.", errors = allocationErrors });

        // ── All validations passed — begin transaction ──────────────

        using var txn = await _db.Database.BeginTransactionAsync();
        try
        {
            var receiptNo = await _documentNumberService.GenerateNextNumberAsync("RECEIPT");

            var receipt = new TrnReceipt
            {
                ReceiptNo = receiptNo,
                ReceiptDate = parsedReceiptDate,
                CompanyId = user.CompanyId ?? 1,
                PartyId = request.PartyId,
                PaymentMode = paymentMode,
                ReferenceNo = request.ReferenceNo,
                ReferenceDate = parsedRefDate,
                BankId = request.BankId,
                Amount = request.Amount,
                Remarks = request.Remarks,
                Status = "POSTED",
                CreatedBy = user.UserId,
                CreatedOn = DateTime.Now
            };

            _db.TrnReceipts.Add(receipt);
            await _db.SaveChangesAsync();

            // Allocations — update AR outstanding & invoice paid/balance amounts
            var allocatedInvoiceIds = new List<long>();
            if (resolvedAllocations.Count > 0)
            {
                foreach (var (alloc, salesInvoiceId, invoice, arRecord) in resolvedAllocations)
                {
                    // Update AR outstanding
                    if (arRecord != null)
                    {
                        arRecord.PaidAmount = (arRecord.PaidAmount ?? 0) + alloc.AllocatedAmount;
                        arRecord.OutstandingAmount = arRecord.OriginalAmount - (arRecord.PaidAmount ?? 0) - (arRecord.AdjustedAmount ?? 0) - (arRecord.WriteOffAmount ?? 0);
                        arRecord.LastPaymentDate = DateOnly.FromDateTime(DateTime.Today);
                        arRecord.ModifiedOn = DateTime.Now;
                        if (arRecord.OutstandingAmount <= 0)
                        {
                            arRecord.OutstandingAmount = 0;
                            arRecord.IsFullySettled = true;
                            arRecord.Status = "CLOSED";
                        }
                    }

                    _db.TrnReceiptAllocations.Add(new TrnReceiptAllocation
                    {
                        ReceiptId = receipt.ReceiptId,
                        SalesInvoiceId = salesInvoiceId,
                        AllocatedAmount = alloc.AllocatedAmount,
                        CreatedOn = DateTime.Now
                    });

                    allocatedInvoiceIds.Add(salesInvoiceId);

                    // Update SalesInvoice paid & balance amounts
                    invoice.PaidAmount = (invoice.PaidAmount ?? 0) + alloc.AllocatedAmount;
                    invoice.BalanceAmount = (invoice.GrandTotal ?? 0) - (invoice.PaidAmount ?? 0);
                    if (invoice.BalanceAmount <= 0)
                    {
                        invoice.BalanceAmount = 0;
                        invoice.Status = "PAID";
                    }
                    else if (invoice.PaidAmount > 0)
                    {
                        invoice.Status = "PARTIALLY_PAID";
                    }
                }
                await _db.SaveChangesAsync();
            }

            // Job timeline entries for allocated invoices
            var jobInvoices = await _db.TrnSalesInvoices
                .Where(i => allocatedInvoiceIds.Contains(i.SalesInvoiceId) && i.JobId != null)
                .Select(i => new { i.SalesInvoiceId, i.InvoiceNo, i.JobId })
                .ToListAsync();

            foreach (var ji in jobInvoices)
            {
                _db.TrnJobTimelines.Add(new TrnJobTimeline
                {
                    JobId = ji.JobId!.Value,
                    EventType = "PAYMENT",
                    EventCode = SubProcessCode.ReceivePayment,
                    EventTitle = "Payment Received",
                    EventDescription = $"Receipt {receiptNo} — ₹{request.Amount:N2} (Invoice {ji.InvoiceNo})",
                    NewAmount = request.Amount,
                    CreatedBy = user.UserId,
                    CreatedOn = DateTime.Now,
                    IsActive = true
                });
            }
            if (jobInvoices.Count > 0)
                await _db.SaveChangesAsync();

            // Activity log
            var activity = ActivityLogEntry.FromUser(user, "ACCOUNTING", "CREATE",
                $"Receipt {receiptNo} — ₹{request.Amount:N2}");
            activity.EntityType = "RECEIPT";
            activity.EntityId = receipt.ReceiptId;
            activity.EntityCode = receipt.ReceiptNo;
            activity.Description = $"Receipt {receiptNo} from {party.Name} for ₹{request.Amount:N2}";
            await _activityService.LogActivityAsync(activity);

            await txn.CommitAsync();

            // Post-commit: Send email notifications (non-blocking)
            _ = Task.Run(async () =>
            {
                try
                {
                    var notifParty = await _db.MstParties.FindAsync(request.PartyId);
                    var company = await _db.MstCompanies.FindAsync(user.CompanyId ?? 1);

                    var notifConfig = await _db.MstProcessNotificationConfigs
                        .FirstOrDefaultAsync(c => c.ProcessCode == "ACC_RECV"
                            && c.SubprocessCode == SubProcessCode.ReceivePayment
                            && c.IsActive);

                    if (notifConfig != null)
                    {
                        var template = await _db.MstNotificationTemplates
                            .FirstOrDefaultAsync(t => t.TemplateCode == notifConfig.TemplateCode && t.IsActive == true);

                        if (template != null)
                        {
                            var context = new NotificationContext
                            {
                                ClientEmail = notifParty?.Email,
                                ClientPhone = notifParty?.Mobile?.ToString(),
                                Variables = new Dictionary<string, string>
                                {
                                    ["ReceiptNo"] = receiptNo,
                                    ["ReceiptDate"] = receipt.ReceiptDate.ToString("dd-MMM-yyyy"),
                                    ["CustomerName"] = notifParty?.Name ?? "",
                                    ["Amount"] = request.Amount.ToString("N2"),
                                    ["PaymentMode"] = request.PaymentMode ?? "CASH",
                                    ["CompanyName"] = company?.Name ?? "",
                                    ["ReferenceNo"] = request.ReferenceNo ?? ""
                                }
                            };

                            var notifTemplate = new NotificationTemplate
                            {
                                Channel = NotificationChannel.Email,
                                SubjectTemplate = template.SubjectTemplate ?? $"Payment Receipt {receiptNo} from {company?.Name}",
                                BodyTemplate = template.BodyTemplate ?? ""
                            };

                            await _notificationDispatcher.DispatchAsync(
                                MapToDispatchConfig(notifConfig), notifTemplate, context);
                        }
                    }
                }
                catch (Exception nex) { _logger.LogWarning(nex, "Receipt notification failed for {ReceiptNo}", receiptNo); }
            });

            return Ok(new
            {
                message = $"Receipt {receiptNo} saved.",
                receipt.ReceiptId,
                receipt.ReceiptNo,
                ReceiptDate = receipt.ReceiptDate.ToString("dd-MMM-yyyy"),
                receipt.Amount,
                PaymentMode = receipt.PaymentMode,
                CustomerName = party.Name,
                AllocatedCount = allocatedInvoiceIds.Count
            });
        }
        catch (Exception ex)
        {
            await txn.RollbackAsync();
            _logger.LogError(ex, "Error saving receipt");
            return StatusCode(500, new { message = "Failed to save receipt." });
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // SAVE — PAYMENT
    // ═══════════════════════════════════════════════════════════════

    [HttpPost("payment/save")]
    public async Task<IActionResult> SavePayment([FromBody] PaymentSaveRequest request)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) return Unauthorized(new { message = "Session expired." });

        using var txn = await _db.Database.BeginTransactionAsync();
        try
        {
            var paymentNo = await _documentNumberService.GenerateNextNumberAsync("PAYMENT");
            var paymentTo = request.PaymentTo ?? "CUSTOMER";

            var payment = new TrnPayment
            {
                PaymentNo = paymentNo,
                PaymentDate = DateOnly.Parse(request.PaymentDate),
                CompanyId = user.CompanyId ?? 1,
                PartyId = request.PartyId,
                PaymentMode = request.PaymentMode ?? "BANK_TRANSFER",
                ReferenceNo = request.ReferenceNo,
                ReferenceDate = string.IsNullOrWhiteSpace(request.ReferenceDate) ? null : DateOnly.Parse(request.ReferenceDate),
                BankId = request.BankId,
                Amount = request.Amount,
                Remarks = request.Remarks,
                Status = "POSTED",
                CreatedBy = user.UserId,
                CreatedOn = DateTime.Now
            };

            _db.TrnPayments.Add(payment);
            await _db.SaveChangesAsync();

            // Allocations — only include items with a positive amount
            var allocatedCount = 0;
            if (request.Allocations?.Count > 0)
            {
                foreach (var alloc in request.Allocations.Where(a => a.AllocatedAmount > 0))
                {
                    _db.TrnPaymentAllocations.Add(new TrnPaymentAllocation
                    {
                        PaymentId = payment.PaymentId,
                        PaymentAgainst = alloc.DocumentType,
                        RefId = alloc.DocumentId,
                        RefNo = alloc.DocumentNo,
                        AllocatedAmount = alloc.AllocatedAmount,
                        CreatedOn = DateTime.Now
                    });
                    allocatedCount++;
                }
                await _db.SaveChangesAsync();
            }

            // Outsource timeline — if paid to outsource vendor
            if (paymentTo == "OUTSOURCE_VENDOR" && request.OutsourceId.HasValue)
            {
                var outsource = await _db.TrnJobOutsources.FindAsync(request.OutsourceId.Value);
                if (outsource != null)
                {
                    var vendor = await _db.MstParties.FindAsync((int)outsource.VendorId);
                    _db.TrnOutsourceTimelines.Add(new TrnOutsourceTimeline
                    {
                        OutsourceId = outsource.OutsourceId,
                        JobId = outsource.JobId,
                        VendorId = outsource.VendorId,
                        VendorName = vendor?.Name,
                        EventType = "PAYMENT",
                        EventCode = SubProcessCode.MakePayment,
                        EventTitle = "Payment Made to Vendor",
                        EventDescription = $"Payment {paymentNo} — ₹{request.Amount:N2} via {request.PaymentMode ?? "BANK_TRANSFER"}",
                        NewAmount = request.Amount,
                        Remarks = request.Remarks,
                        CreatedBy = user.UserId,
                        CreatedOn = DateTime.Now,
                        IsActive = true
                    });
                    await _db.SaveChangesAsync();
                }
            }

            // Resolve payee name for response
            string payeeName = request.PayeeName ?? "";
            if (paymentTo == "CUSTOMER")
            {
                var party = await _db.MstParties.FindAsync(request.PartyId);
                payeeName = party?.Name ?? "";
            }
            else if (paymentTo == "EMPLOYEE" && request.EmployeeId.HasValue)
            {
                var emp = await _db.MstEmployees.FindAsync(request.EmployeeId.Value);
                payeeName = emp != null ? $"{emp.FirstName} {emp.LastName}".Trim() : "";
            }
            else if (paymentTo == "OUTSOURCE_VENDOR" && request.OutsourceId.HasValue)
            {
                var outsource = await _db.TrnJobOutsources.FindAsync(request.OutsourceId.Value);
                if (outsource != null)
                {
                    var vendor = await _db.MstParties.FindAsync((int)outsource.VendorId);
                    payeeName = vendor?.Name ?? "";
                }
            }

            // Activity log
            var activity = ActivityLogEntry.FromUser(user, "ACCOUNTING", "CREATE",
                $"Payment {paymentNo} — ₹{request.Amount:N2} to {payeeName} ({paymentTo})");
            activity.EntityType = "PAYMENT";
            activity.EntityId = payment.PaymentId;
            activity.EntityCode = payment.PaymentNo;
            activity.Description = $"Payment {paymentNo} to {payeeName} for ₹{request.Amount:N2} via {request.PaymentMode ?? "CASH"}" +
                (request.PaymentAgainst != null ? $" against {request.PaymentAgainst}" : "");
            await _activityService.LogActivityAsync(activity);

            await txn.CommitAsync();

            // Post-commit: Send email notifications (non-blocking)
            _ = Task.Run(async () =>
            {
                try
                {
                    var company = await _db.MstCompanies.FindAsync(user.CompanyId ?? 1);
                    string? recipientEmail = null;
                    string? recipientPhone = null;
                    string recipientName = payeeName;

                    if (paymentTo == "CUSTOMER")
                    {
                        var party = await _db.MstParties.FindAsync(request.PartyId);
                        recipientEmail = party?.Email;
                        recipientPhone = party?.Mobile?.ToString();
                    }
                    else if (paymentTo == "EMPLOYEE" && request.EmployeeId.HasValue)
                    {
                        var emp = await _db.MstEmployees.FindAsync(request.EmployeeId.Value);
                        recipientEmail = emp?.Email1;
                        recipientPhone = emp?.MobileNo1;
                    }
                    else if (paymentTo == "OUTSOURCE_VENDOR" && request.OutsourceId.HasValue)
                    {
                        var outsource = await _db.TrnJobOutsources.FindAsync(request.OutsourceId.Value);
                        if (outsource != null)
                        {
                            var vendor = await _db.MstParties.FindAsync((int)outsource.VendorId);
                            recipientEmail = vendor?.Email;
                            recipientPhone = vendor?.Mobile?.ToString();
                        }
                    }

                    var notifConfig = await _db.MstProcessNotificationConfigs
                        .FirstOrDefaultAsync(c => c.ProcessCode == "ACC_PAY"
                            && c.SubprocessCode == SubProcessCode.MakePayment
                            && c.IsActive);

                    if (notifConfig != null && !string.IsNullOrWhiteSpace(recipientEmail))
                    {
                        var template = await _db.MstNotificationTemplates
                            .FirstOrDefaultAsync(t => t.TemplateCode == notifConfig.TemplateCode && t.IsActive == true);

                        if (template != null)
                        {
                            var context = new NotificationContext
                            {
                                ClientEmail = recipientEmail,
                                ClientPhone = recipientPhone,
                                Variables = new Dictionary<string, string>
                                {
                                    ["PaymentNo"] = paymentNo,
                                    ["PaymentDate"] = payment.PaymentDate.ToString("dd-MMM-yyyy"),
                                    ["PayeeName"] = recipientName,
                                    ["Amount"] = request.Amount.ToString("N2"),
                                    ["PaymentMode"] = request.PaymentMode ?? "CASH",
                                    ["CompanyName"] = company?.Name ?? "",
                                    ["ReferenceNo"] = request.ReferenceNo ?? "",
                                    ["PaymentTo"] = paymentTo,
                                    ["PaymentAgainst"] = request.PaymentAgainst ?? ""
                                }
                            };

                            var notifTemplate = new NotificationTemplate
                            {
                                Channel = NotificationChannel.Email,
                                SubjectTemplate = template.SubjectTemplate ?? $"Payment {paymentNo} from {company?.Name}",
                                BodyTemplate = template.BodyTemplate ?? ""
                            };

                            await _notificationDispatcher.DispatchAsync(
                                MapToDispatchConfig(notifConfig), notifTemplate, context);
                        }
                    }
                }
                catch (Exception nex) { _logger.LogWarning(nex, "Payment notification failed for {PaymentNo}", paymentNo); }
            });

            return Ok(new
            {
                message = $"Payment {paymentNo} saved.",
                payment.PaymentId,
                payment.PaymentNo,
                PaymentDate = payment.PaymentDate.ToString("dd-MMM-yyyy"),
                payment.Amount,
                PaymentMode = payment.PaymentMode,
                PayeeName = payeeName,
                PaymentTo = paymentTo,
                PaymentAgainst = request.PaymentAgainst ?? "",
                AllocatedCount = allocatedCount
            });
        }
        catch (Exception ex)
        {
            await txn.RollbackAsync();
            _logger.LogError(ex, "Error saving payment");
            return StatusCode(500, new { message = "Failed to save payment." });
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // SAVE — EXPENSE VOUCHER
    // ═══════════════════════════════════════════════════════════════

    [HttpPost("expense-voucher/save")]
    public async Task<IActionResult> SaveExpenseVoucher([FromBody] ExpenseVoucherSaveRequest request)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) return Unauthorized(new { message = "Session expired." });

        using var txn = await _db.Database.BeginTransactionAsync();
        try
        {
            var voucherNo = await _documentNumberService.GenerateNextNumberAsync("EXPENSE_VOUCHER");

            var voucher = new TrnExpenseVoucher
            {
                VoucherNo = voucherNo,
                VoucherDate = DateOnly.Parse(request.VoucherDate),
                CompanyId = user.CompanyId ?? 1,
                ExpenseCategory = request.ExpenseCategory,
                PartyId = request.PartyId,
                PaymentMode = request.PaymentMode ?? "CASH",
                BankAccountId = request.BankAccountId,
                ReferenceNo = request.ReferenceNo,
                SubtotalAmount = request.SubtotalAmount,
                TaxableAmount = request.TaxableAmount,
                CgstAmount = request.CgstAmount,
                SgstAmount = request.SgstAmount,
                IgstAmount = request.IgstAmount,
                CessAmount = request.CessAmount,
                TotalTaxAmount = request.TotalTaxAmount,
                TdsAmount = request.TdsAmount,
                GrandTotal = request.GrandTotal,
                Narration = request.Narration,
                Status = request.Status ?? "DRAFT",
                CreatedBy = user.UserId,
                CreatedOn = DateTime.Now
            };

            _db.TrnExpenseVouchers.Add(voucher);
            await _db.SaveChangesAsync();

            // Line items
            if (request.Items?.Count > 0)
            {
                int seq = 1;
                foreach (var item in request.Items)
                {
                    _db.TrnExpenseVoucherItems.Add(new TrnExpenseVoucherItem
                    {
                        ExpenseVoucherId = voucher.ExpenseVoucherId,
                        ItemSequence = seq++,
                        AccountHeadId = item.AccountHeadId,
                        Description = item.Description ?? "",
                        HsnSacCode = item.HsnSacCode,
                        Amount = item.Amount,
                        CgstPercent = item.CgstPercent,
                        CgstAmount = item.CgstAmount,
                        SgstPercent = item.SgstPercent,
                        SgstAmount = item.SgstAmount,
                        IgstPercent = item.IgstPercent,
                        IgstAmount = item.IgstAmount,
                        CessPercent = item.CessPercent,
                        CessAmount = item.CessAmount,
                        TotalTaxAmount = item.TotalTaxAmount,
                        LineTotal = item.LineTotal,
                        CostCenterId = item.CostCenterId,
                        JobId = item.JobId,
                        Remarks = item.Remarks
                    });
                }
                await _db.SaveChangesAsync();
            }

            // Activity log
            var activity = ActivityLogEntry.FromUser(user, "ACCOUNTING", "CREATE",
                $"Expense Voucher {voucherNo} — ₹{request.GrandTotal:N2}");
            activity.EntityType = "EXPENSE_VOUCHER";
            activity.EntityId = voucher.ExpenseVoucherId;
            activity.EntityCode = voucher.VoucherNo;
            await _activityService.LogActivityAsync(activity);

            // Notification
            try
            {
                var notifConfig = await _db.MstProcessNotificationConfigs
                    .FirstOrDefaultAsync(c => c.ProcessCode == "ACCOUNTING" && c.SubprocessCode == SubProcessCode.RecordExpense && c.IsActive);
                if (notifConfig != null)
                {
                    var template = await _db.MstNotificationTemplates
                        .FirstOrDefaultAsync(t => t.TemplateCode == notifConfig.TemplateCode && t.IsActive == true);
                    if (template != null)
                    {
                        var notifTemplate = new NotificationTemplate
                        {
                            Channel = NotificationChannel.Email,
                            SubjectTemplate = template.SubjectTemplate ?? $"Expense Voucher {voucherNo}",
                            BodyTemplate = template.BodyTemplate ?? ""
                        };
                        var context = new NotificationContext
                        {
                            Variables = new Dictionary<string, string>
                            {
                                ["VoucherNo"] = voucherNo,
                                ["Amount"] = request.GrandTotal?.ToString("N2") ?? "0.00",
                                ["Category"] = request.ExpenseCategory ?? "",
                                ["CreatedBy"] = user.UserCode
                            }
                        };
                        await _notificationDispatcher.DispatchAsync(
                            MapToDispatchConfig(notifConfig), notifTemplate, context);
                    }
                }
            }
            catch (Exception nex) { _logger.LogWarning(nex, "Expense voucher notification failed"); }

            await txn.CommitAsync();
            return Ok(new { message = $"Expense Voucher {voucherNo} saved.", voucher.ExpenseVoucherId, voucher.VoucherNo });
        }
        catch (Exception ex)
        {
            await txn.RollbackAsync();
            _logger.LogError(ex, "Error saving expense voucher");
            return StatusCode(500, new { message = "Failed to save expense voucher." });
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // STATES
    // ═══════════════════════════════════════════════════════════════

    [HttpGet("states")]
    public async Task<IActionResult> GetStates()
    {
        var list = await _db.MstStates
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .Select(s => new { s.Id, s.Name, s.Code, s.GstStateCode, s.IsUnionTerritory })
            .ToListAsync();

        return Ok(list);
    }

    // ═══════════════════════════════════════════════════════════════
    // HELPERS
    // ═══════════════════════════════════════════════════════════════

    private static ProcessNotificationConfig MapToDispatchConfig(MstProcessNotificationConfig dbConfig)
    {
        return new ProcessNotificationConfig
        {
            ConfigId = (int)dbConfig.ConfigId,
            ProcessCode = dbConfig.ProcessCode,
            SubProcessCode = dbConfig.SubprocessCode,
            EventLabel = dbConfig.EventLabel,
            IsActive = dbConfig.IsActive,
            RecipientType = string.Equals(dbConfig.RecipientType, "BOTH", StringComparison.OrdinalIgnoreCase)
                ? RecipientType.Both
                : RecipientType.Internal,
            NotifyAssignee = dbConfig.NotifyAssignee,
            NotifyDeptHead = dbConfig.NotifyDeptHead,
            NotifySupervisor = dbConfig.NotifySupervisor,
            NotifyApprover = dbConfig.NotifyApprover,
            NotifyInternalEmail = dbConfig.NotifyInternalEmail,
            NotifyInternalSms = dbConfig.NotifyInternalSms,
            NotifyInternalWhatsApp = dbConfig.NotifyInternalWhatsapp,
            NotifyClientEmail = dbConfig.NotifyClientEmail,
            NotifyClientSms = dbConfig.NotifyClientSms,
            NotifyClientWhatsApp = dbConfig.NotifyClientWhatsapp,
            NotifyPush = dbConfig.NotifyPush,
        };
    }
}

// ═══════════════════════════════════════════════════════════════
// REQUEST MODELS
// ═══════════════════════════════════════════════════════════════

public class SalesInvoiceSaveRequest
{
    public long SalesInvoiceId { get; set; }
    public string InvoiceDate { get; set; } = "";
    public string? DueDate { get; set; }
    public int PartyId { get; set; }
    public long? JobId { get; set; }
    public long? QuotationId { get; set; }
    public string? PlaceOfSupply { get; set; }
    public int? PaymentTermId { get; set; }
    public decimal RoundOff { get; set; }
    public string? TermsConditions { get; set; }
    public string? InternalNotes { get; set; }
    public List<SalesInvoiceItemRequest> Items { get; set; } = [];
}

public class SalesInvoiceItemRequest
{
    public string Description { get; set; } = "";
    public string? HsnSacCode { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitRate { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxableValue { get; set; }
    public decimal CgstPercent { get; set; }
    public decimal CgstAmount { get; set; }
    public decimal SgstPercent { get; set; }
    public decimal SgstAmount { get; set; }
    public decimal IgstPercent { get; set; }
    public decimal IgstAmount { get; set; }
    public decimal TotalTaxAmount { get; set; }
    public decimal LineTotal { get; set; }
}

public class CancelRequest
{
    public string? Reason { get; set; }
}

public class OutsourcePaymentRequest
{
    public long OutsourceId { get; set; }
    public decimal Amount { get; set; }
    public string? PaymentMode { get; set; }
}

public class ReceiptSaveRequest
{
    public string ReceiptDate { get; set; } = "";
    public int PartyId { get; set; }
    public string? PaymentMode { get; set; }
    public decimal Amount { get; set; }
    public int? BankId { get; set; }
    public string? ReferenceNo { get; set; }
    public string? ReferenceDate { get; set; }
    public string? Remarks { get; set; }
    public List<ReceiptAllocationRequest>? Allocations { get; set; }
}

public class ReceiptAllocationRequest
{
    public long ArId { get; set; }
    public long InvoiceId { get; set; }
    public string? DocumentNo { get; set; }
    public decimal AllocatedAmount { get; set; }
}

public class PaymentSaveRequest
{
    public string PaymentDate { get; set; } = "";
    public int PartyId { get; set; }
    public string? PaymentMode { get; set; }
    public decimal Amount { get; set; }
    public int? BankId { get; set; }
    public string? ReferenceNo { get; set; }
    public string? ReferenceDate { get; set; }
    public string? Remarks { get; set; }
    public string? PaymentTo { get; set; }        // CUSTOMER, EMPLOYEE, OUTSOURCE_VENDOR, OTHER
    public string? PaymentAgainst { get; set; }    // VENDOR_INVOICE, PURCHASE_BILL, etc.
    public long? EmployeeId { get; set; }
    public long? OutsourceId { get; set; }
    public string? PayeeName { get; set; }         // For OTHER type
    public List<PaymentAllocationRequest>? Allocations { get; set; }
}

public class PaymentAllocationRequest
{
    public string? DocumentType { get; set; }
    public long DocumentId { get; set; }
    public string? DocumentNo { get; set; }
    public decimal AllocatedAmount { get; set; }
}

public class ExpenseVoucherSaveRequest
{
    public string VoucherDate { get; set; } = "";
    public string? ExpenseCategory { get; set; }
    public int? PartyId { get; set; }
    public string? PaymentMode { get; set; }
    public int? BankAccountId { get; set; }
    public string? ReferenceNo { get; set; }
    public decimal? SubtotalAmount { get; set; }
    public decimal? TaxableAmount { get; set; }
    public decimal? CgstAmount { get; set; }
    public decimal? SgstAmount { get; set; }
    public decimal? IgstAmount { get; set; }
    public decimal? CessAmount { get; set; }
    public decimal? TotalTaxAmount { get; set; }
    public decimal? TdsAmount { get; set; }
    public decimal? GrandTotal { get; set; }
    public string? Narration { get; set; }
    public string? Status { get; set; }
    public List<ExpenseVoucherItemRequest>? Items { get; set; }
}

public class ExpenseVoucherItemRequest
{
    public long AccountHeadId { get; set; }
    public string? Description { get; set; }
    public string? HsnSacCode { get; set; }
    public decimal? Amount { get; set; }
    public decimal? CgstPercent { get; set; }
    public decimal? CgstAmount { get; set; }
    public decimal? SgstPercent { get; set; }
    public decimal? SgstAmount { get; set; }
    public decimal? IgstPercent { get; set; }
    public decimal? IgstAmount { get; set; }
    public decimal? CessPercent { get; set; }
    public decimal? CessAmount { get; set; }
    public decimal? TotalTaxAmount { get; set; }
    public decimal? LineTotal { get; set; }
    public int? CostCenterId { get; set; }
    public long? JobId { get; set; }
    public string? Remarks { get; set; }
}
