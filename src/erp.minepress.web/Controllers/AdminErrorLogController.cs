using erp.minepress.persistence.Context;
using erp.minepress.web.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace erp.minepress.web.Controllers;

[ApiController]
[Route("api/admin/error-logs")]
public class AdminErrorLogController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<AdminErrorLogController> _logger;
    private readonly IHttpContextAccessor _httpCtx;

    public AdminErrorLogController(
        ApplicationDbContext db,
        ILogger<AdminErrorLogController> logger,
        IHttpContextAccessor httpCtx)
    {
        _db = db;
        _logger = logger;
        _httpCtx = httpCtx;
    }

    // GET /api/admin/error-logs?page=1&pageSize=25&severity=&layer=&isReviewed=&search=&dateFrom=&dateTo=
    [HttpGet]
    public async Task<IActionResult> List(
        int page = 1,
        int pageSize = 25,
        string? severity = null,
        string? layer = null,
        bool? isReviewed = null,
        string? search = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null)
    {
        try
        {
            var query = _db.SysErrorLogs.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(severity))
                query = query.Where(e => e.Severity == severity);

            if (!string.IsNullOrWhiteSpace(layer))
                query = query.Where(e => e.Layer == layer);

            if (isReviewed.HasValue)
                query = query.Where(e => e.IsReviewed == isReviewed.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                query = query.Where(e =>
                    e.Message.ToLower().Contains(s) ||
                    e.RequestPath.ToLower().Contains(s) ||
                    e.UserName.ToLower().Contains(s) ||
                    e.ExceptionType.ToLower().Contains(s) ||
                    e.Source.ToLower().Contains(s));
            }

            if (dateFrom.HasValue)
                query = query.Where(e => e.CreatedOn >= dateFrom.Value.ToUniversalTime());

            if (dateTo.HasValue)
                query = query.Where(e => e.CreatedOn <= dateTo.Value.ToUniversalTime().AddDays(1));

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(e => e.CreatedOn)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new
                {
                    e.ErrorLogId,
                    e.Severity,
                    e.Layer,
                    e.ExceptionType,
                    e.Message,
                    e.RequestPath,
                    e.HttpMethod,
                    e.UserName,
                    e.UserId,
                    e.IpAddress,
                    e.IsReviewed,
                    e.CreatedOn
                })
                .ToListAsync();

            return Ok(new { items, totalCount, page, pageSize });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load error log list");
            return StatusCode(500, new { message = "Failed to load error logs" });
        }
    }

    // GET /api/admin/error-logs/{id}
    [HttpGet("{id:long}")]
    public async Task<IActionResult> Detail(long id)
    {
        try
        {
            var entry = await _db.SysErrorLogs.AsNoTracking()
                .Where(e => e.ErrorLogId == id)
                .Select(e => new
                {
                    e.ErrorLogId,
                    e.Layer,
                    e.Source,
                    e.MethodName,
                    e.ExceptionType,
                    e.Message,
                    e.StackTrace,
                    e.InnerException,
                    e.RequestPath,
                    e.HttpMethod,
                    e.RequestData,
                    e.UserId,
                    e.UserName,
                    e.IpAddress,
                    e.UserAgent,
                    e.CorrelationId,
                    e.TenantKey,
                    e.Severity,
                    e.AdditionalData,
                    e.CreatedOn,
                    e.MachineName,
                    e.AppVersion,
                    e.IsReviewed,
                    e.ReviewNotes,
                    e.ReviewedBy,
                    e.ReviewedOn
                })
                .FirstOrDefaultAsync();

            if (entry == null)
                return NotFound(new { message = "Error log entry not found" });

            return Ok(entry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load error log detail for id {Id}", id);
            return StatusCode(500, new { message = "Failed to load error log detail" });
        }
    }

    // GET /api/admin/error-logs/stats
    [HttpGet("stats")]
    public async Task<IActionResult> Stats()
    {
        try
        {
            var since = DateTime.UtcNow.AddDays(-30);

            var critical = await _db.SysErrorLogs.CountAsync(e => e.Severity == "Critical" && e.CreatedOn >= since);
            var error = await _db.SysErrorLogs.CountAsync(e => e.Severity == "Error" && e.CreatedOn >= since);
            var pendingReview = await _db.SysErrorLogs.CountAsync(e => !e.IsReviewed);
            var reviewed = await _db.SysErrorLogs.CountAsync(e => e.IsReviewed);

            return Ok(new { critical, error, pendingReview, reviewed });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load error log stats");
            return StatusCode(500, new { message = "Failed to load stats" });
        }
    }

    // POST /api/admin/error-logs/{id}/review
    [HttpPost("{id:long}/review")]
    public async Task<IActionResult> MarkReviewed(long id, [FromBody] ReviewRequest req)
    {
        try
        {
            var entry = await _db.SysErrorLogs.FindAsync(id);
            if (entry == null)
                return NotFound(new { message = "Error log entry not found" });

            var user = _httpCtx.HttpContext?.Session.GetCurrentUser();

            entry.IsReviewed = true;
            entry.ReviewNotes = req.Notes ?? string.Empty;
            entry.ReviewedBy = user?.UserName ?? "System";
            entry.ReviewedOn = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return Ok(new { message = "Marked as reviewed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark error log {Id} as reviewed", id);
            return StatusCode(500, new { message = "Failed to mark as reviewed" });
        }
    }

    public record ReviewRequest(string? Notes);
}
