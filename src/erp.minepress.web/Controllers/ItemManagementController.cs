using erp.minepress.infrastructure.ErrorLogging;
using erp.minepress.persistence.Context;
using erp.minepress.persistence.Models;
using erp.minepress.web.Helpers;
using erp.minepress.web.Services;
using erp.minepress.notification.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace erp.minepress.web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemManagementController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<ItemManagementController> _logger;
    private readonly INotificationService _notification;
    private readonly IUserActivityService _activity;
    private readonly ISystemErrorLogger _systemErrorLogger;

    public ItemManagementController(
        ApplicationDbContext db,
        ILogger<ItemManagementController> logger,
        INotificationService notification,
        IUserActivityService activity,
        ISystemErrorLogger systemErrorLogger)
    {
        _db = db;
        _logger = logger;
        _notification = notification;
        _activity = activity;
        _systemErrorLogger = systemErrorLogger;
    }

    private async Task AuditExceptionAsync(Exception ex, string additionalData, string severity = "Error")
        => await _systemErrorLogger.LogAsync(ex, HttpContext, severity, additionalData);

    private UserSessionData? CurrentUser =>
        HttpContext.Session.GetObject<UserSessionData>("CurrentUser");

    // ═══════════════════════════════════════════════════════════════
    // KPIs
    // ═══════════════════════════════════════════════════════════════
    [HttpGet("kpis")]
    public async Task<IActionResult> GetKpis()
    {
        try
        {
            var items = await _db.VwMstItems.ToListAsync();

            var total = items.Count;
            var active = items.Count(i => i.IsActive == true);
            var inactive = total - active;
            var lowStock = items.Count(i => i.IsActive == true
                && i.ReorderLevel.HasValue && i.CurrentStock.HasValue
                && i.CurrentStock < i.ReorderLevel);

            var groups = items.GroupBy(i => i.ItemGroup ?? "OTHER")
                .Select(g => new { group = g.Key, count = g.Count() })
                .ToDictionary(g => g.group, g => g.count);

            // AI insights
            var outOfStock = items.Count(i => i.IsActive == true && (i.CurrentStock ?? 0) == 0);
            var noRate = items.Count(i => i.IsActive == true && (!i.PurchaseRate.HasValue || i.PurchaseRate == 0));
            var noHsn = items.Count(i => i.IsActive == true && string.IsNullOrWhiteSpace(i.HsnCode));
            var staleItems = items.Count(i => i.IsActive == true
                && i.LastPurchaseDate.HasValue
                && i.LastPurchaseDate.Value < DateOnly.FromDateTime(DateTime.Now.AddMonths(-6)));

            return Ok(new
            {
                total,
                active,
                inactive,
                lowStock,
                groups,
                ai = new { outOfStock, noRate, noHsn, staleItems }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading item KPIs");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // List Items (paginated, filtered)
    // ═══════════════════════════════════════════════════════════════
    [HttpGet("items")]
    public async Task<IActionResult> GetItems(
        string? q, string? group, string? category, string? status,
        string? uom, string? stock, int page = 1, int size = 25)
    {
        try
        {
            var query = _db.VwMstItems.AsQueryable();

            if (!string.IsNullOrWhiteSpace(group) && group != "ALL")
                query = query.Where(i => i.ItemGroup == group);

            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(i => i.ItemCategory == category);

            if (!string.IsNullOrWhiteSpace(status))
            {
                var isActive = status == "true";
                query = query.Where(i => i.IsActive == isActive);
            }

            if (!string.IsNullOrWhiteSpace(uom))
                query = query.Where(i => i.Uom == uom);

            if (stock == "low")
                query = query.Where(i => i.ReorderLevel.HasValue && i.CurrentStock.HasValue
                    && i.CurrentStock < i.ReorderLevel && i.CurrentStock > 0);
            else if (stock == "out")
                query = query.Where(i => !i.CurrentStock.HasValue || i.CurrentStock == 0);

            if (!string.IsNullOrWhiteSpace(q))
            {
                var search = q.Trim().ToLower();
                query = query.Where(i =>
                    (i.ItemCode != null && i.ItemCode.ToLower().Contains(search)) ||
                    (i.ItemName != null && i.ItemName.ToLower().Contains(search)) ||
                    (i.ItemCategory != null && i.ItemCategory.ToLower().Contains(search)) ||
                    (i.HsnCode != null && i.HsnCode.ToLower().Contains(search))
                );
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(i => i.ItemGroup).ThenBy(i => i.ItemCode)
                .Skip((page - 1) * size).Take(size)
                .Select(i => new
                {
                    i.ItemId,
                    i.ItemGroup,
                    i.ItemCode,
                    i.ItemName,
                    i.ItemDescription,
                    i.ItemCategory,
                    i.Uom,
                    i.PurchaseRate,
                    i.ReorderLevel,
                    i.CurrentStock,
                    i.HsnCode,
                    i.GstRate,
                    i.LastPurchaseRate,
                    lastPurchaseDate = i.LastPurchaseDate.HasValue ? i.LastPurchaseDate.Value.ToString("yyyy-MM-dd") : null,
                    i.IsActive,
                    i.Remarks,
                    i.SourceTable,
                    i.SourceId
                })
                .ToListAsync();

            return Ok(new { totalCount, page, size, items });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading items");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // Get Item Detail (from source table)
    // ═══════════════════════════════════════════════════════════════
    [HttpGet("items/{group}/{code}")]
    public async Task<IActionResult> GetItemDetail(string group, string code)
    {
        try
        {
            object? detail = group.ToUpper() switch
            {
                "CHEMICAL" => await _db.MstChemicals.FirstOrDefaultAsync(c => c.ChemicalCode == code),
                "INK" => await _db.MstInks.FirstOrDefaultAsync(i => i.InkCode == code),
                "PAPER" => await _db.MstPapers.FirstOrDefaultAsync(p => p.PaperCode == code),
                "PLATE" => await _db.MstPlates.FirstOrDefaultAsync(p => p.PlateCode == code),
                "OTHER" => await _db.MstOtherItems.FirstOrDefaultAsync(o => o.ItemCode == code),
                _ => null
            };

            if (detail == null)
                return NotFound(new { message = "Item not found" });

            return Ok(new { group = group.ToUpper(), detail });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading item detail");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // Filters (distinct groups, categories, UOMs)
    // ═══════════════════════════════════════════════════════════════
    [HttpGet("filters")]
    public async Task<IActionResult> GetFilters()
    {
        try
        {
            var categories = await _db.VwMstItems
                .Where(i => i.ItemCategory != null)
                .Select(i => i.ItemCategory!)
                .Distinct().OrderBy(c => c).ToListAsync();

            var uoms = await _db.VwMstItems
                .Where(i => i.Uom != null)
                .Select(i => i.Uom!)
                .Distinct().OrderBy(u => u).ToListAsync();

            return Ok(new { categories, uoms });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading filters");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // Create Item (saves to source table)
    // ═══════════════════════════════════════════════════════════════
    [HttpPost("items")]
    public async Task<IActionResult> CreateItem([FromBody] ItemDto dto)
    {
        try
        {
            // ── Field-level validation ──
            var errors = ValidateItem(dto);
            if (errors.Count > 0)
                return BadRequest(new { message = errors[0], errors });

            switch (dto.ItemGroup!.ToUpper())
            {
                case "CHEMICAL":
                    if (await _db.MstChemicals.AnyAsync(c => c.ChemicalCode == dto.ItemCode))
                        return BadRequest(new { message = $"Chemical code '{dto.ItemCode}' already exists." });
                    _db.MstChemicals.Add(MapToChemical(dto));
                    break;

                case "INK":
                    if (await _db.MstInks.AnyAsync(i => i.InkCode == dto.ItemCode))
                        return BadRequest(new { message = $"Ink code '{dto.ItemCode}' already exists." });
                    _db.MstInks.Add(MapToInk(dto));
                    break;

                case "PAPER":
                    if (await _db.MstPapers.AnyAsync(p => p.PaperCode == dto.ItemCode))
                        return BadRequest(new { message = $"Paper code '{dto.ItemCode}' already exists." });
                    _db.MstPapers.Add(MapToPaper(dto));
                    break;

                case "PLATE":
                    if (await _db.MstPlates.AnyAsync(p => p.PlateCode == dto.ItemCode))
                        return BadRequest(new { message = $"Plate code '{dto.ItemCode}' already exists." });
                    _db.MstPlates.Add(MapToPlate(dto));
                    break;

                case "OTHER":
                    if (await _db.MstOtherItems.AnyAsync(o => o.ItemCode == dto.ItemCode))
                        return BadRequest(new { message = $"Item code '{dto.ItemCode}' already exists." });
                    _db.MstOtherItems.Add(MapToOtherItem(dto));
                    break;

                default:
                    return BadRequest(new { message = "Invalid item group." });
            }

            await _db.SaveChangesAsync();

            // ── Activity Log ──
            try
            {
                var session = CurrentUser;
                if (session != null)
                {
                    var logEntry = ActivityLogEntry.FromUser(session, "ITEM_MGMT", "CREATE", $"Created {dto.ItemGroup} item {dto.ItemCode} — {dto.ItemName}");
                    logEntry.EntityType = "ITEM";
                    logEntry.EntityCode = dto.ItemCode;
                    logEntry.Description = $"New {dto.ItemGroup} item created: {dto.ItemName} ({dto.ItemCode}), UOM: {dto.Uom}, Rate: {dto.PurchaseRate}";
                    await _activity.LogActivityAsync(logEntry);
                }
            }
            catch (Exception actEx)
            {
                _logger.LogWarning(actEx, "Failed to log item creation activity");
            }

            // ── Store Email Notification (fire-and-forget) ──
            _ = Task.Run(async () =>
            {
                try
                {
                    var storeEmail = await GetStoreUserEmailAsync();
                    if (!string.IsNullOrWhiteSpace(storeEmail))
                    {
                        var body = BuildItemCreatedEmailBody(dto.ItemGroup!, dto.ItemCode!, dto.ItemName!, dto.Uom, dto.PurchaseRate, dto.HsnCode);
                        await _notification.SendEmailAsync(storeEmail, $"New Item Created — {dto.ItemName} ({dto.ItemCode})", body);
                    }
                }
                catch (Exception emailEx)
                {
                    _logger.LogWarning(emailEx, "Failed to send item creation email notification");
                }
            });

            return Ok(new { message = "Item created successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating item");
            return StatusCode(500, new { message = ex.InnerException?.Message ?? ex.Message });
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // Update Item (saves to source table)
    // ═══════════════════════════════════════════════════════════════
    [HttpPut("items/{group}/{code}")]
    public async Task<IActionResult> UpdateItem(string group, string code, [FromBody] ItemDto dto)
    {
        try
        {
            // ── Field-level validation (update mode) ──
            var errors = ValidateItem(dto, isUpdate: true);
            if (errors.Count > 0)
                return BadRequest(new { message = errors[0], errors });

            switch (group.ToUpper())
            {
                case "CHEMICAL":
                    var chem = await _db.MstChemicals.FirstOrDefaultAsync(c => c.ChemicalCode == code);
                    if (chem == null) return NotFound(new { message = "Chemical not found." });
                    UpdateChemical(chem, dto);
                    break;

                case "INK":
                    var ink = await _db.MstInks.FirstOrDefaultAsync(i => i.InkCode == code);
                    if (ink == null) return NotFound(new { message = "Ink not found." });
                    UpdateInk(ink, dto);
                    break;

                case "PAPER":
                    var paper = await _db.MstPapers.FirstOrDefaultAsync(p => p.PaperCode == code);
                    if (paper == null) return NotFound(new { message = "Paper not found." });
                    UpdatePaper(paper, dto);
                    break;

                case "PLATE":
                    var plate = await _db.MstPlates.FirstOrDefaultAsync(p => p.PlateCode == code);
                    if (plate == null) return NotFound(new { message = "Plate not found." });
                    UpdatePlate(plate, dto);
                    break;

                case "OTHER":
                    var other = await _db.MstOtherItems.FirstOrDefaultAsync(o => o.ItemCode == code);
                    if (other == null) return NotFound(new { message = "Item not found." });
                    UpdateOtherItem(other, dto);
                    break;

                default:
                    return BadRequest(new { message = "Invalid item group." });
            }

            await _db.SaveChangesAsync();
            return Ok(new { message = "Item updated successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating item");
            return StatusCode(500, new { message = ex.InnerException?.Message ?? ex.Message });
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // Toggle Active
    // ═══════════════════════════════════════════════════════════════
    [HttpPost("items/{group}/{code}/toggle")]
    public async Task<IActionResult> ToggleItem(string group, string code)
    {
        try
        {
            bool newStatus;
            switch (group.ToUpper())
            {
                case "CHEMICAL":
                    var chem = await _db.MstChemicals.FirstOrDefaultAsync(c => c.ChemicalCode == code);
                    if (chem == null) return NotFound();
                    chem.IsActive = !(chem.IsActive ?? true);
                    newStatus = chem.IsActive ?? false;
                    break;
                case "INK":
                    var ink = await _db.MstInks.FirstOrDefaultAsync(i => i.InkCode == code);
                    if (ink == null) return NotFound();
                    ink.IsActive = !(ink.IsActive ?? true);
                    newStatus = ink.IsActive ?? false;
                    break;
                case "PAPER":
                    var paper = await _db.MstPapers.FirstOrDefaultAsync(p => p.PaperCode == code);
                    if (paper == null) return NotFound();
                    paper.IsActive = !(paper.IsActive ?? true);
                    newStatus = paper.IsActive ?? false;
                    break;
                case "PLATE":
                    var plate = await _db.MstPlates.FirstOrDefaultAsync(p => p.PlateCode == code);
                    if (plate == null) return NotFound();
                    plate.IsActive = !(plate.IsActive ?? true);
                    newStatus = plate.IsActive ?? false;
                    break;
                case "OTHER":
                    var other = await _db.MstOtherItems.FirstOrDefaultAsync(o => o.ItemCode == code);
                    if (other == null) return NotFound();
                    other.IsActive = !(other.IsActive ?? true);
                    newStatus = other.IsActive ?? false;
                    break;
                default:
                    return BadRequest(new { message = "Invalid group." });
            }

            await _db.SaveChangesAsync();
            return Ok(new { message = newStatus ? "Item activated." : "Item deactivated.", isActive = newStatus });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling item");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // Validation
    // ═══════════════════════════════════════════════════════════════
    private static readonly string[] ValidItemGroups = ["CHEMICAL", "INK", "PAPER", "PLATE", "OTHER"];

    private static List<string> ValidateItem(ItemDto dto, bool isUpdate = false)
    {
        var errors = new List<string>();

        // Required fields
        if (string.IsNullOrWhiteSpace(dto.ItemGroup))
            errors.Add("Item group is required");
        if (!isUpdate && string.IsNullOrWhiteSpace(dto.ItemCode))
            errors.Add("Item code is required");
        if (string.IsNullOrWhiteSpace(dto.ItemName))
            errors.Add("Item name is required");
        if (string.IsNullOrWhiteSpace(dto.Uom))
            errors.Add("Unit of measure (UOM) is required");

        // ItemGroup enum
        if (!string.IsNullOrWhiteSpace(dto.ItemGroup) && !ValidItemGroups.Contains(dto.ItemGroup.ToUpper()))
            errors.Add($"Item group must be one of: {string.Join(", ", ValidItemGroups)}");

        // String length limits
        if (!string.IsNullOrWhiteSpace(dto.ItemCode) && dto.ItemCode.Length > 30)
            errors.Add("Item code must be 30 characters or less");
        if (!string.IsNullOrWhiteSpace(dto.ItemName) && dto.ItemName.Length > 150)
            errors.Add("Item name must be 150 characters or less");
        if (!string.IsNullOrWhiteSpace(dto.ItemCategory) && dto.ItemCategory.Length > 50)
            errors.Add("Item category must be 50 characters or less");
        if (!string.IsNullOrWhiteSpace(dto.Uom) && dto.Uom.Length > 20)
            errors.Add("UOM must be 20 characters or less");
        if (!string.IsNullOrWhiteSpace(dto.HsnCode) && dto.HsnCode.Length > 20)
            errors.Add("HSN code must be 20 characters or less");
        if (!string.IsNullOrWhiteSpace(dto.Remarks) && dto.Remarks.Length > 500)
            errors.Add("Remarks must be 500 characters or less");

        // Format: ItemCode — alphanumeric, underscores, hyphens only
        if (!string.IsNullOrWhiteSpace(dto.ItemCode) && !Regex.IsMatch(dto.ItemCode, @"^[A-Za-z0-9_\-]+$"))
            errors.Add("Item code must contain only letters, digits, underscores or hyphens");

        // Format: HSN — digits only
        if (!string.IsNullOrWhiteSpace(dto.HsnCode) && !Regex.IsMatch(dto.HsnCode, @"^[0-9]+$"))
            errors.Add("HSN code must contain only digits");

        // Numeric range checks
        if (dto.PurchaseRate.HasValue && dto.PurchaseRate < 0)
            errors.Add("Purchase rate cannot be negative");
        if (dto.ReorderLevel.HasValue && dto.ReorderLevel < 0)
            errors.Add("Reorder level cannot be negative");
        if (dto.CurrentStock.HasValue && dto.CurrentStock < 0)
            errors.Add("Current stock cannot be negative");
        if (dto.LastPurchaseRate.HasValue && dto.LastPurchaseRate < 0)
            errors.Add("Last purchase rate cannot be negative");
        if (dto.GstRate.HasValue && (dto.GstRate < 0 || dto.GstRate > 100))
            errors.Add("GST rate must be between 0 and 100");

        // Group-specific validations
        var group = dto.ItemGroup?.ToUpper();

        if (group == "PAPER")
        {
            if (dto.Gsm.HasValue && dto.Gsm <= 0)
                errors.Add("GSM must be a positive number");
            if (dto.SheetLength.HasValue && dto.SheetLength <= 0)
                errors.Add("Sheet length must be a positive number");
            if (dto.SheetWidth.HasValue && dto.SheetWidth <= 0)
                errors.Add("Sheet width must be a positive number");
        }

        if (group == "PLATE")
        {
            if (dto.Thickness.HasValue && dto.Thickness <= 0)
                errors.Add("Plate thickness must be a positive number");
            if (dto.PlateLength.HasValue && dto.PlateLength <= 0)
                errors.Add("Plate length must be a positive number");
            if (dto.PlateWidth.HasValue && dto.PlateWidth <= 0)
                errors.Add("Plate width must be a positive number");
            if (dto.MaxImpressions.HasValue && dto.MaxImpressions <= 0)
                errors.Add("Max impressions must be a positive number");
            if (dto.ProcessingCost.HasValue && dto.ProcessingCost < 0)
                errors.Add("Processing cost cannot be negative");
        }

        if (group == "INK")
        {
            if (dto.Coverage.HasValue && dto.Coverage <= 0)
                errors.Add("Coverage must be a positive number");
            if (dto.InkWastage.HasValue && (dto.InkWastage < 0 || dto.InkWastage > 100))
                errors.Add("Ink wastage must be between 0 and 100");
        }

        if (group == "CHEMICAL")
        {
            if (dto.ChemShelfLife.HasValue && dto.ChemShelfLife <= 0)
                errors.Add("Shelf life must be a positive number");
        }

        return errors;
    }

    // ═══════════════════════════════════════════════════════════════
    // Mapping helpers
    // ═══════════════════════════════════════════════════════════════

    private static MstChemical MapToChemical(ItemDto d) => new()
    {
        ChemicalCode = d.ItemCode!,
        ChemicalName = d.ItemName!,
        ChemicalCategory = d.ItemCategory,
        ChemicalType = d.ChemicalType,
        ProcessStage = d.ProcessStage,
        Manufacturer = d.ChemManufacturer,
        Brand = d.ChemBrand,
        DilutionRatio = d.DilutionRatio,
        ShelfLifeMonths = d.ChemShelfLife,
        Hazardous = d.Hazardous,
        Uom = d.Uom,
        RatePerUnit = d.PurchaseRate,
        ReorderLevel = d.ReorderLevel,
        CurrentStock = d.CurrentStock,
        HsnCode = d.HsnCode,
        GstRate = d.GstRate,
        LastPurchaseRate = d.LastPurchaseRate,
        IsActive = true,
        Remarks = d.Remarks
    };

    private static void UpdateChemical(MstChemical c, ItemDto d)
    {
        c.ChemicalName = d.ItemName ?? c.ChemicalName;
        c.ChemicalCategory = d.ItemCategory ?? c.ChemicalCategory;
        c.ChemicalType = d.ChemicalType ?? c.ChemicalType;
        c.ProcessStage = d.ProcessStage ?? c.ProcessStage;
        c.Manufacturer = d.ChemManufacturer ?? c.Manufacturer;
        c.Brand = d.ChemBrand ?? c.Brand;
        c.DilutionRatio = d.DilutionRatio ?? c.DilutionRatio;
        if (d.ChemShelfLife.HasValue) c.ShelfLifeMonths = d.ChemShelfLife;
        if (d.Hazardous.HasValue) c.Hazardous = d.Hazardous;
        c.Uom = d.Uom ?? c.Uom;
        if (d.PurchaseRate.HasValue) c.RatePerUnit = d.PurchaseRate;
        if (d.ReorderLevel.HasValue) c.ReorderLevel = d.ReorderLevel;
        if (d.CurrentStock.HasValue) c.CurrentStock = d.CurrentStock;
        c.HsnCode = d.HsnCode ?? c.HsnCode;
        if (d.GstRate.HasValue) c.GstRate = d.GstRate;
        if (d.LastPurchaseRate.HasValue) c.LastPurchaseRate = d.LastPurchaseRate;
        c.Remarks = d.Remarks ?? c.Remarks;
    }

    private static MstInk MapToInk(ItemDto d) => new()
    {
        InkCode = d.ItemCode!,
        InkName = d.ItemName!,
        InkCategory = d.ItemCategory,
        InkType = d.InkType,
        ColorName = d.ColorName,
        PantoneCode = d.PantoneCode,
        Manufacturer = d.InkManufacturer,
        CoverageSqMPerKg = d.Coverage,
        WastagePercent = d.InkWastage,
        Uom = d.Uom,
        CostPerKg = d.PurchaseRate,
        ReorderLevel = d.ReorderLevel,
        CurrentStock = d.CurrentStock,
        HsnCode = d.HsnCode,
        GstRate = d.GstRate,
        LastPurchaseRate = d.LastPurchaseRate,
        IsActive = true,
        Remarks = d.Remarks
    };

    private static void UpdateInk(MstInk ink, ItemDto d)
    {
        ink.InkName = d.ItemName ?? ink.InkName;
        ink.InkCategory = d.ItemCategory ?? ink.InkCategory;
        ink.InkType = d.InkType ?? ink.InkType;
        ink.ColorName = d.ColorName ?? ink.ColorName;
        ink.PantoneCode = d.PantoneCode ?? ink.PantoneCode;
        ink.Manufacturer = d.InkManufacturer ?? ink.Manufacturer;
        if (d.Coverage.HasValue) ink.CoverageSqMPerKg = d.Coverage;
        if (d.InkWastage.HasValue) ink.WastagePercent = d.InkWastage;
        ink.Uom = d.Uom ?? ink.Uom;
        if (d.PurchaseRate.HasValue) ink.CostPerKg = d.PurchaseRate;
        if (d.ReorderLevel.HasValue) ink.ReorderLevel = d.ReorderLevel;
        if (d.CurrentStock.HasValue) ink.CurrentStock = d.CurrentStock;
        ink.HsnCode = d.HsnCode ?? ink.HsnCode;
        if (d.GstRate.HasValue) ink.GstRate = d.GstRate;
        if (d.LastPurchaseRate.HasValue) ink.LastPurchaseRate = d.LastPurchaseRate;
        ink.Remarks = d.Remarks ?? ink.Remarks;
    }

    private static MstPaper MapToPaper(ItemDto d) => new()
    {
        PaperCode = d.ItemCode!,
        PaperName = d.ItemName!,
        PaperCategory = d.ItemCategory,
        PaperType = d.PaperType,
        PaperFinish = d.PaperFinish,
        Gsm = d.Gsm ?? 0,
        GrainDirection = d.GrainDir,
        SheetLengthMm = d.SheetLength,
        SheetWidthMm = d.SheetWidth,
        SupplierName = d.PaperSupplier,
        BrandName = d.PaperBrand,
        Uom = d.Uom,
        CostPerKg = d.PurchaseRate,
        ReorderLevel = d.ReorderLevel,
        CurrentStock = d.CurrentStock,
        HsnCode = d.HsnCode,
        GstRate = d.GstRate,
        LastPurchaseRate = d.LastPurchaseRate,
        IsActive = true,
        Remarks = d.Remarks
    };

    private static void UpdatePaper(MstPaper p, ItemDto d)
    {
        p.PaperName = d.ItemName ?? p.PaperName;
        p.PaperCategory = d.ItemCategory ?? p.PaperCategory;
        p.PaperType = d.PaperType ?? p.PaperType;
        p.PaperFinish = d.PaperFinish ?? p.PaperFinish;
        if (d.Gsm.HasValue) p.Gsm = d.Gsm.Value;
        p.GrainDirection = d.GrainDir ?? p.GrainDirection;
        if (d.SheetLength.HasValue) p.SheetLengthMm = d.SheetLength;
        if (d.SheetWidth.HasValue) p.SheetWidthMm = d.SheetWidth;
        p.SupplierName = d.PaperSupplier ?? p.SupplierName;
        p.BrandName = d.PaperBrand ?? p.BrandName;
        p.Uom = d.Uom ?? p.Uom;
        if (d.PurchaseRate.HasValue) p.CostPerKg = d.PurchaseRate;
        if (d.ReorderLevel.HasValue) p.ReorderLevel = d.ReorderLevel;
        if (d.CurrentStock.HasValue) p.CurrentStock = d.CurrentStock;
        p.HsnCode = d.HsnCode ?? p.HsnCode;
        if (d.GstRate.HasValue) p.GstRate = d.GstRate;
        if (d.LastPurchaseRate.HasValue) p.LastPurchaseRate = d.LastPurchaseRate;
        p.Remarks = d.Remarks ?? p.Remarks;
    }

    private static MstPlate MapToPlate(ItemDto d) => new()
    {
        PlateCode = d.ItemCode!,
        PlateName = d.ItemName!,
        PlateType = d.PlateType,
        ThicknessMm = d.Thickness,
        MaxImpressions = d.MaxImpressions,
        PlateLengthMm = d.PlateLength,
        PlateWidthMm = d.PlateWidth,
        ProcessingCost = d.ProcessingCost,
        Uom = d.Uom,
        PlateCost = d.PurchaseRate,
        ReorderLevel = d.ReorderLevel,
        CurrentStock = d.CurrentStock,
        HsnCode = d.HsnCode,
        GstRate = d.GstRate,
        LastPurchaseRate = d.LastPurchaseRate,
        IsActive = true,
        Remarks = d.Remarks,
        CreatedAt = DateTime.UtcNow
    };

    private static void UpdatePlate(MstPlate pl, ItemDto d)
    {
        pl.PlateName = d.ItemName ?? pl.PlateName;
        pl.PlateType = d.PlateType ?? pl.PlateType;
        if (d.Thickness.HasValue) pl.ThicknessMm = d.Thickness;
        if (d.MaxImpressions.HasValue) pl.MaxImpressions = d.MaxImpressions;
        if (d.PlateLength.HasValue) pl.PlateLengthMm = d.PlateLength;
        if (d.PlateWidth.HasValue) pl.PlateWidthMm = d.PlateWidth;
        if (d.ProcessingCost.HasValue) pl.ProcessingCost = d.ProcessingCost;
        pl.Uom = d.Uom ?? pl.Uom;
        if (d.PurchaseRate.HasValue) pl.PlateCost = d.PurchaseRate;
        if (d.ReorderLevel.HasValue) pl.ReorderLevel = d.ReorderLevel;
        if (d.CurrentStock.HasValue) pl.CurrentStock = d.CurrentStock;
        pl.HsnCode = d.HsnCode ?? pl.HsnCode;
        if (d.GstRate.HasValue) pl.GstRate = d.GstRate;
        if (d.LastPurchaseRate.HasValue) pl.LastPurchaseRate = d.LastPurchaseRate;
        pl.Remarks = d.Remarks ?? pl.Remarks;
    }

    private static MstOtherItem MapToOtherItem(ItemDto d) => new()
    {
        ItemCode = d.ItemCode!,
        ItemName = d.ItemName!,
        ItemCategory = d.ItemCategory,
        ItemType = d.OtherItemType,
        Description = d.OtherDesc,
        SupplierName = d.OtherSupplier,
        Brand = d.OtherBrand,
        Uom = d.Uom,
        RatePerUnit = d.PurchaseRate,
        ReorderLevel = d.ReorderLevel,
        CurrentStock = d.CurrentStock,
        HsnCode = d.HsnCode,
        GstRate = d.GstRate,
        LastPurchaseRate = d.LastPurchaseRate,
        IsActive = true,
        Remarks = d.Remarks,
        CreatedOn = DateTime.UtcNow
    };

    private static void UpdateOtherItem(MstOtherItem o, ItemDto d)
    {
        o.ItemName = d.ItemName ?? o.ItemName;
        o.ItemCategory = d.ItemCategory ?? o.ItemCategory;
        o.ItemType = d.OtherItemType ?? o.ItemType;
        o.Description = d.OtherDesc ?? o.Description;
        o.SupplierName = d.OtherSupplier ?? o.SupplierName;
        o.Brand = d.OtherBrand ?? o.Brand;
        o.Uom = d.Uom ?? o.Uom;
        if (d.PurchaseRate.HasValue) o.RatePerUnit = d.PurchaseRate;
        if (d.ReorderLevel.HasValue) o.ReorderLevel = d.ReorderLevel;
        if (d.CurrentStock.HasValue) o.CurrentStock = d.CurrentStock;
        o.HsnCode = d.HsnCode ?? o.HsnCode;
        if (d.GstRate.HasValue) o.GstRate = d.GstRate;
        if (d.LastPurchaseRate.HasValue) o.LastPurchaseRate = d.LastPurchaseRate;
        o.Remarks = d.Remarks ?? o.Remarks;
        o.ModifiedOn = DateTime.UtcNow;
    }

    // ═══════════════════════════════════════════════════════════════
    // Email & Notification Helpers
    // ═══════════════════════════════════════════════════════════════
    private async Task<string?> GetStoreUserEmailAsync()
    {
        var storeUser = await _db.MstUsers
            .Where(u => u.Isdeleted != true && u.Isactive == true && u.Emailid != null
                && u.Department != null && u.Department.DeptName != null
                && (u.Department.DeptName.ToUpper().Contains("STORE") || u.Department.DeptName.ToUpper().Contains("INVENTORY")))
            .Select(u => u.Emailid)
            .FirstOrDefaultAsync();
        return storeUser;
    }

    private static string BuildItemCreatedEmailBody(string group, string code, string name, string? uom, decimal? rate, string? hsn)
    {
        return $@"
<!DOCTYPE html>
<html><head><meta charset='utf-8'/></head>
<body style='font-family:Segoe UI,Helvetica,Arial,sans-serif;margin:0;padding:0;background:#f4f6fa;'>
<div style='max-width:600px;margin:40px auto;background:#fff;border-radius:12px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,.08);'>
  <div style='background:linear-gradient(135deg,#0ea5e9,#6366f1);padding:28px 40px;text-align:center;'>
    <h1 style='color:#fff;margin:0;font-size:22px;'>📦 New Item Created</h1>
    <p style='color:rgba(255,255,255,.8);margin:8px 0 0;font-size:14px;'>A new item has been added to inventory</p>
  </div>
  <div style='padding:28px 40px;'>
    <div style='background:#f0f9ff;border:1px solid #bae6fd;border-radius:8px;padding:20px;margin:0 0 16px;'>
      <table style='width:100%;border-collapse:collapse;'>
        <tr><td style='padding:8px 0;color:#64748b;width:120px;'>Group</td><td style='padding:8px 0;font-weight:600;color:#0c4a6e;'>{group}</td></tr>
        <tr><td style='padding:8px 0;color:#64748b;'>Item Code</td><td style='padding:8px 0;font-weight:600;color:#0c4a6e;'>{code}</td></tr>
        <tr><td style='padding:8px 0;color:#64748b;'>Item Name</td><td style='padding:8px 0;font-weight:600;color:#0c4a6e;'>{name}</td></tr>
        {(uom != null ? $"<tr><td style='padding:8px 0;color:#64748b;'>UOM</td><td style='padding:8px 0;font-weight:600;color:#0c4a6e;'>{uom}</td></tr>" : "")}
        {(rate.HasValue ? $"<tr><td style='padding:8px 0;color:#64748b;'>Rate</td><td style='padding:8px 0;font-weight:600;color:#0c4a6e;'>₹{rate}</td></tr>" : "")}
        {(hsn != null ? $"<tr><td style='padding:8px 0;color:#64748b;'>HSN</td><td style='padding:8px 0;font-weight:600;color:#0c4a6e;'>{hsn}</td></tr>" : "")}
      </table>
    </div>
    <p style='color:#64748b;font-size:13px;'>Please update store records accordingly. This is an automated notification from MinePress ERP.</p>
  </div>
  <div style='background:#f8fafc;padding:14px 40px;text-align:center;border-top:1px solid #e2e8f0;'>
    <p style='margin:0;color:#94a3b8;font-size:12px;'>MinePress ERP — Powered by AI</p>
  </div>
</div>
</body></html>";
    }

    // ═══════════════════════════════════════════════════════════════
    // DTO
    // ═══════════════════════════════════════════════════════════════
    public class ItemDto
    {
        // Common
        public string? ItemGroup { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? ItemCategory { get; set; }
        public string? Uom { get; set; }
        public decimal? PurchaseRate { get; set; }
        public decimal? ReorderLevel { get; set; }
        public decimal? CurrentStock { get; set; }
        public string? HsnCode { get; set; }
        public decimal? GstRate { get; set; }
        public decimal? LastPurchaseRate { get; set; }
        public string? Remarks { get; set; }

        // Chemical
        public string? ChemicalType { get; set; }
        public string? ProcessStage { get; set; }
        public string? ChemManufacturer { get; set; }
        public string? ChemBrand { get; set; }
        public string? DilutionRatio { get; set; }
        public int? ChemShelfLife { get; set; }
        public bool? Hazardous { get; set; }

        // Ink
        public string? InkType { get; set; }
        public string? ColorName { get; set; }
        public string? PantoneCode { get; set; }
        public string? InkManufacturer { get; set; }
        public decimal? Coverage { get; set; }
        public decimal? InkWastage { get; set; }

        // Paper
        public string? PaperType { get; set; }
        public string? PaperFinish { get; set; }
        public int? Gsm { get; set; }
        public string? GrainDir { get; set; }
        public int? SheetLength { get; set; }
        public int? SheetWidth { get; set; }
        public string? PaperSupplier { get; set; }
        public string? PaperBrand { get; set; }

        // Plate
        public string? PlateType { get; set; }
        public decimal? Thickness { get; set; }
        public int? MaxImpressions { get; set; }
        public int? PlateLength { get; set; }
        public int? PlateWidth { get; set; }
        public decimal? ProcessingCost { get; set; }

        // Other
        public string? OtherItemType { get; set; }
        public string? OtherDesc { get; set; }
        public string? OtherSupplier { get; set; }
        public string? OtherBrand { get; set; }
    }
}
