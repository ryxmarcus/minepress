using erp.minepress.infrastructure.ErrorLogging;
using erp.minepress.notification.Interfaces;
using erp.minepress.persistence.Context;
using erp.minepress.persistence.Models;
using erp.minepress.web.Helpers;
using erp.minepress.web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace erp.minepress.web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RateCalculatorController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IUserActivityService _activityService;
    private readonly INotificationService _notifier;
    private readonly ILogger<RateCalculatorController> _logger;
    private readonly ISystemErrorLogger _systemErrorLogger;

    public RateCalculatorController(
        ApplicationDbContext db,
        IUserActivityService activityService,
        INotificationService notifier,
        ILogger<RateCalculatorController> logger,
        ISystemErrorLogger systemErrorLogger)
    {
        _db = db;
        _activityService = activityService;
        _notifier = notifier;
        _logger = logger;
        _systemErrorLogger = systemErrorLogger;
    }

    private async Task AuditExceptionAsync(Exception ex, string additionalData, string severity = "Error")
        => await _systemErrorLogger.LogAsync(ex, HttpContext, severity, additionalData);

    [HttpGet("config")]
    public IActionResult GetConfig()
    {
        var config = new
        {
            costing_rules = new
            {
                ink_rules = new { wastage_percent = 5m, startup_ink_grams = 200m },
                base_rules = new { currency = "INR", decimal_precision = 2, minimum_job_value = 500m, rounding_strategy = "nearest_10" },
                paper_rules = new { storage_loss_percent = 0.5m, cutting_wastage_percent = 2m, handling_wastage_percent = 1m, minimum_order_qty_sheets = 500m },
                plate_rules = new { replate_percent = 2m, minimum_plate_count = 1, plate_wastage_percent = 3m },
                pricing_rules = new { gst_percent = 18m, round_off_to = 10m },
                printing_rules = new { perfecting_factor = 1.6m, double_side_factor = 1.8m, color_registration_wastage_percent = 2m },
                postpress_rules = new { packing_cost_per_unit = 2m, binding_wastage_percent = 2m, finishing_wastage_percent = 2m },
                production_rules = new { shift_hours = 8, downtime_percent = 10m, plate_change_time_minutes = 15m, machine_efficiency_percent = 85m }
            },
            job_type_dynamic_fields = new Dictionary<string, object>
            {
                ["FULL_OFFSET"] = new { category = "FULL", printing_mode = "OFFSET", workflow = new[] { "DESIGN", "DTP", "CTP", "PRINTING", "BINDING", "FINISHING", "DISPATCH" }, fields = new { required = new[] { "customer_id", "product_type_id", "product_part_id", "quantity", "paper_id", "machine_id", "color_count", "printing_side", "plate_count" }, optional = new[] { "lamination_type", "binding_type", "finishing_type" }, conditional = new[] { new { field = "binding_type", required_if = "is_binding_required=true" }, new { field = "lamination_type", required_if = "is_finishing_required=true" } } }, costing_dependencies = new[] { "paper", "ink", "plate", "machine", "binding", "finishing" }, rules = new { min_qty = 500, wastage_percent = 3 }, modules = new { design = true, dtp = true, ctp = true, printing = true, binding = true, finishing = true } },
                ["FULL_DIGITAL"] = new { category = "FULL", printing_mode = "DIGITAL", workflow = new[] { "DESIGN", "DTP", "PRINTING", "BINDING", "FINISHING", "DISPATCH" }, fields = new { required = new[] { "customer_id", "product_type_id", "quantity", "machine_id", "color_mode" }, optional = new[] { "paper_id", "finishing_type" } }, costing_dependencies = new[] { "machine", "click" }, rules = new { wastage_percent = 1 } },
                ["FULL_SCREEN"] = new { category = "FULL", printing_mode = "SCREEN", workflow = new[] { "DESIGN", "DTP", "PRINTING", "DRYING", "FINISHING" }, fields = new { required = new[] { "customer_id", "quantity", "screen_count", "color_count" } }, costing_dependencies = new[] { "screen", "ink", "labour" }, rules = new { wastage_percent = 5 } },
                ["FULL_FLEX"] = new { category = "FULL", printing_mode = "FLEX", workflow = new[] { "DESIGN", "PRINTING", "FINISHING" }, fields = new { required = new[] { "customer_id", "width", "height", "quantity" } }, costing_dependencies = new[] { "area", "material" }, rules = new { area_based = true } },
                ["FULL_UV"] = new { category = "FULL", printing_mode = "UV", workflow = new[] { "DESIGN", "PRINTING", "CURING", "FINISHING" }, fields = new { required = new[] { "customer_id", "material_type", "quantity" } }, costing_dependencies = new[] { "uv_ink", "machine" }, rules = new { uv_multiplier = 1.5 } },
                ["DESIGN_ONLY"] = new { category = "SERVICE", workflow = new[] { "DESIGN" }, fields = new { required = new[] { "customer_id", "design_type", "pages" }, optional = new[] { "software", "reference_file" } }, costing_dependencies = new[] { "design" } },
                ["DTP_ONLY"] = new { category = "SERVICE", workflow = new[] { "DTP" }, fields = new { required = new[] { "customer_id", "pages", "page_size" } }, costing_dependencies = new[] { "dtp" } },
                ["CTP_ONLY"] = new { category = "PREPRESS", workflow = new[] { "CTP" }, fields = new { required = new[] { "plate_size", "plate_count" } }, costing_dependencies = new[] { "plate" } },
                ["PROOF_ONLY"] = new { category = "PREPRESS", workflow = new[] { "PRINTING" }, fields = new { required = new[] { "proof_type", "quantity" } }, costing_dependencies = new[] { "proof" } },
                ["PRINT_OFFSET"] = new { category = "PRINT_ONLY", printing_mode = "OFFSET", fields = new { required = new[] { "machine_id", "paper_id", "quantity", "color_count" } }, costing_dependencies = new[] { "machine", "ink" } },
                ["PRINT_DIGITAL"] = new { category = "PRINT_ONLY", printing_mode = "DIGITAL", fields = new { required = new[] { "machine_id", "quantity", "color_mode" } }, costing_dependencies = new[] { "click" } },
                ["PRINT_SCREEN"] = new { category = "PRINT_ONLY", printing_mode = "SCREEN", fields = new { required = new[] { "screen_count", "quantity" } }, costing_dependencies = new[] { "screen" } },
                ["PRINT_FLEX"] = new { category = "PRINT_ONLY", printing_mode = "FLEX", fields = new { required = new[] { "width", "height", "quantity" } }, costing_dependencies = new[] { "area" } },
                ["PRINT_UV"] = new { category = "PRINT_ONLY", printing_mode = "UV", fields = new { required = new[] { "material_type", "quantity" } }, costing_dependencies = new[] { "uv" } },
                ["BINDING_ONLY"] = new { category = "POST", workflow = new[] { "BINDING" }, fields = new { required = new[] { "binding_type", "quantity" } }, costing_dependencies = new[] { "binding" } },
                ["FINISH_ONLY"] = new { category = "POST", workflow = new[] { "FINISHING" }, fields = new { required = new[] { "finishing_type", "quantity" } }, costing_dependencies = new[] { "finishing" } },
                ["LAMINATION"] = new { category = "POST", fields = new { required = new[] { "lamination_type", "paper_id", "quantity" } }, costing_dependencies = new[] { "lamination" } },
                ["CUTTING"] = new { category = "POST", fields = new { required = new[] { "quantity", "cut_size" } }, costing_dependencies = new[] { "cutting" } },
                ["FOLDING"] = new { category = "POST", fields = new { required = new[] { "quantity", "fold_type" } }, costing_dependencies = new[] { "folding" } },
                ["PACKAGING"] = new { category = "POST", fields = new { required = new[] { "quantity", "package_type" } }, costing_dependencies = new[] { "packaging" } },
                ["OUT_PRINT"] = new { category = "OUTSOURCE", fields = new { required = new[] { "vendor_id", "job_description", "cost" } }, costing_dependencies = new[] { "vendor" } },
                ["OUT_BIND"] = new { category = "OUTSOURCE", fields = new { required = new[] { "vendor_id", "binding_type", "cost" } }, costing_dependencies = new[] { "vendor" } },
                ["OUT_FINISH"] = new { category = "OUTSOURCE", fields = new { required = new[] { "vendor_id", "finishing_type", "cost" } }, costing_dependencies = new[] { "vendor" } },
                ["JOB_WORK"] = new { category = "LABOUR", fields = new { required = new[] { "labour_type", "hours", "rate" } }, costing_dependencies = new[] { "labour" } }
            }
        };
        return Ok(config);
    }

    [HttpGet("jobtypes")]
    public async Task<IActionResult> GetJobTypes()
    {
        var items = await _db.MstJobTypes
            .Where(x => x.Isactive == true)
            .OrderBy(x => x.Jobtypename)
            .Select(x => new
            {
                x.Jobtypeid,
                x.Jobtypecode,
                x.Jobtypename,
                x.Description,
                x.Printingmode,
                x.Isdesignrequired,
                x.Isdtprequired,
                x.Isctprequired,
                x.Isprintingrequired,
                x.Isbindingrequired,
                x.Isfinishingrequired,
                x.Issingleprocess,
                x.Isfullprocess,
                x.Iscustomermaterial,
                x.Isinhousematerial,
                x.Isoutsourcejob
            })
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("producttypes")]
    public async Task<IActionResult> GetProductTypes()
    {
        var items = await _db.MstPrintProductTypes
            .Where(x => x.Isactive == true)
            .OrderBy(x => x.Productname)
            .Select(x => new
            {
                x.Printproducttypeid,
                x.Productcode,
                x.Productname,
                x.Category,
                x.Iscustomsize,
                x.Isbindingrequired,
                x.Isprintingrequired,
                x.Isfinishingrequired
            })
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("productsizes")]
    public async Task<IActionResult> GetProductSizes()
    {
        var items = await _db.MstPrintProductSizes
            .Where(x => x.Isactive == true)
            .OrderBy(x => x.Sizename)
            .Select(x => new
            {
                x.Productsizeid,
                x.Sizecode,
                x.Sizename,
                x.Widthmm,
                x.Heightmm,
                x.Category,
                x.Isstandard
            })
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("productparts/{productTypeId}")]
    public async Task<IActionResult> GetProductParts(int productTypeId)
    {
        var items = await _db.MstProductParts
            .Where(x => x.Printproducttypeid == productTypeId && x.Isactive == true)
            .OrderBy(x => x.Displayorder)
            .Select(x => new
            {
                x.Productpartid,
                x.Partcode,
                x.Partname,
                x.Ispagebased,
                x.Defaultpages,
                x.Requirespaper,
                x.Requiresplate,
                x.Requiresprinting,
                x.Requiresbinding,
                x.Requiresfinishing
            })
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("papers")]
    public async Task<IActionResult> GetPapers([FromQuery] string? productType)
    {
        var query = _db.MstPapers
            .Where(x => x.IsActive == true);

        if (!string.IsNullOrEmpty(productType))
        {
            var filteredQuery = query.Where(x =>
                x.SupportedJobTypes == null ||
                x.SupportedJobTypes.ToLower() == "all" ||
                x.SupportedJobTypes.ToLower().Contains(productType.ToLower()));

            if (await filteredQuery.AnyAsync())
                query = filteredQuery;
        }

        var items = await query
            .OrderBy(x => x.PaperName)
            .Select(x => new
            {
                x.PaperId,
                x.PaperCode,
                x.PaperName,
                x.PaperCategory,
                x.PaperType,
                x.Gsm,
                x.SheetLengthMm,
                x.SheetWidthMm,
                x.CostPerKg,
                x.CostPerSheet,
                x.GrainDirection,
                x.SupportedUsage
            })
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("machines")]
    public async Task<IActionResult> GetMachines([FromQuery] string? category)
    {
        var query = _db.MstMachines
            .Where(x => x.IsActive == true);

        if (!string.IsNullOrEmpty(category))
        {
            var filteredQuery = query.Where(x =>
                x.MachineCategory == null ||
                x.MachineCategory == category);

            if (await filteredQuery.AnyAsync())
                query = filteredQuery;
        }

        var items = await query
            .OrderBy(x => x.MachineName)
            .Select(x => new
            {
                x.MachineId,
                x.MachineCode,
                x.MachineName,
                x.MachineCategory,
                x.MachineType,
                x.MaxColors,
                x.PrintingSide,
                x.MaxSpeed,
                x.SpeedUnit,
                x.HourlyRunningCost,
                x.MaxSheetLengthMm,
                x.MaxSheetWidthMm
            })
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("inks")]
    public async Task<IActionResult> GetInks()
    {
        var items = await _db.MstInks
            .Where(x => x.IsActive == true)
            .OrderBy(x => x.InkName)
            .Select(x => new
            {
                x.InkCode,
                x.InkName,
                x.InkCategory,
                x.ColorName,
                x.DryingType,
                x.ConsumptionGsm,
                x.CostPerKg
            })
            .ToListAsync();
        return Ok(items);
    }

    // ── Rate Calculator List ──
    [HttpGet("list")]
    public async Task<IActionResult> GetCalculationList()
    {
        var list = await _db.HybJobRateCalculators
            .Include(r => r.JobType)
            .Include(r => r.ProductType)
            .Include(r => r.ProductSize)
            .Include(r => r.Party)
            .Include(r => r.Enquiry)
            .Include(r => r.Quotation)
            .Include(r => r.Job)
            .Include(r => r.CreatedByNavigation)
            .OrderByDescending(r => r.RateCalcId)
            .Select(r => new
            {
                r.RateCalcId,
                r.CalcRefNo,
                r.Quantity,
                r.TotalPages,
                r.TrimWidthMm,
                r.TrimHeightMm,
                r.PrintingMode,
                r.GrandTotal,
                r.TaxAmount,
                r.NetTotal,
                r.CostPerUnit,
                r.Status,
                r.Version,
                ValidityDate = r.ValidityDate.HasValue ? r.ValidityDate.Value.ToString("dd-MMM-yyyy") : null,
                r.ParentCalcId,
                CreatedOn = r.CreatedOn.ToString("dd-MMM-yyyy HH:mm"),
                CreatedBy = r.CreatedByNavigation != null ? r.CreatedByNavigation.Name : "",
                JobTypeName = r.JobType != null ? r.JobType.Jobtypename : null,
                ProductTypeName = r.ProductType != null ? r.ProductType.Productname : null,
                ProductSizeName = r.ProductSize != null ? r.ProductSize.Sizename : null,
                PartyName = r.Party != null ? r.Party.Name : null,
                PartyCode = r.Party != null ? r.Party.Code : null,
                EnquiryNo = r.Enquiry != null ? r.Enquiry.EnquiryNo : null,
                r.EnquiryId,
                QuotationNo = r.Quotation != null ? r.Quotation.QuotationNo : null,
                r.QuotationId,
                JobNo = r.Job != null ? r.Job.JobNo : null,
                r.JobId
            })
            .ToListAsync();

        return Ok(list);
    }

    // ── Rate Calculator Detail ──
    [HttpGet("detail/{id:long}")]
    public async Task<IActionResult> GetCalculationDetail(long id)
    {
        var calc = await _db.HybJobRateCalculators
            .Include(r => r.JobType)
            .Include(r => r.ProductType)
            .Include(r => r.ProductSize)
            .Include(r => r.Party)
            .Include(r => r.Enquiry)
            .Include(r => r.Quotation)
            .Include(r => r.Job)
            .Include(r => r.CreatedByNavigation)
            .FirstOrDefaultAsync(r => r.RateCalcId == id);

        if (calc == null)
            return NotFound(new { message = "Rate calculation not found." });

        var result = new
        {
            calc.RateCalcId,
            calc.CalcRefNo,
            calc.Quantity,
            calc.TotalPages,
            calc.TrimWidthMm,
            calc.TrimHeightMm,
            calc.PrintingMode,
            calc.IsCustomerMaterial,
            calc.GrandTotal,
            calc.TaxAmount,
            calc.NetTotal,
            calc.CostPerUnit,
            calc.Status,
            calc.Version,
            ValidityDate = calc.ValidityDate?.ToString("dd-MMM-yyyy"),
            calc.ParentCalcId,
            calc.InternalRemarks,
            calc.ClientRemarks,
            CreatedOn = calc.CreatedOn.ToString("dd-MMM-yyyy HH:mm"),
            CreatedBy = calc.CreatedByNavigation?.Name ?? "",
            JobTypeName = calc.JobType?.Jobtypename,
            JobTypeCode = calc.JobType?.Jobtypecode,
            ProductTypeName = calc.ProductType?.Productname,
            ProductTypeCode = calc.ProductType?.Productcode,
            ProductSizeName = calc.ProductSize?.Sizename,
            PartyName = calc.Party?.Name,
            PartyCode = calc.Party?.Code,
            PartyEmail = calc.Party?.Email,
            EnquiryNo = calc.Enquiry?.EnquiryNo,
            calc.EnquiryId,
            QuotationNo = calc.Quotation?.QuotationNo,
            calc.QuotationId,
            JobNo = calc.Job?.JobNo,
            calc.JobId,
            // JSONB data as raw strings — parsed client-side
            calc.PartsData,
            calc.CostBreakdown,
            calc.BomData,
            calc.AiInsights,
            calc.RecommendedMachines,
            calc.CalcInputSnapshot
        };

        return Ok(result);
    }

    [HttpGet("plates")]
    public async Task<IActionResult> GetPlates()
    {
        var items = await _db.MstPlates
            .Where(x => x.IsActive == true)
            .OrderBy(x => x.PlateName)
            .Select(x => new
            {
                x.PlateId,
                x.PlateCode,
                x.PlateName,
                x.PlateType,
                x.PlateLengthMm,
                x.PlateWidthMm,
                x.PlateCost,
                x.ProcessingCost,
                x.MaxImpressions
            })
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("bindings")]
    public async Task<IActionResult> GetBindings([FromQuery] string? productType)
    {
        var query = _db.MstBindings
            .Where(x => x.IsActive == true);

        if (!string.IsNullOrEmpty(productType))
        {
            var filteredQuery = query.Where(x =>
                x.SupportedJobTypes == null ||
                x.SupportedJobTypes.ToLower() == "all" ||
                x.SupportedJobTypes.ToLower().Contains(productType.ToLower()));

            if (await filteredQuery.AnyAsync())
                query = filteredQuery;
        }

        var items = await query
            .OrderBy(x => x.BindingName)
            .Select(x => new
            {
                x.BindingId,
                x.BindingCode,
                x.BindingName,
                x.BindingCategory,
                x.BindingType,
                x.CostPerBook,
                x.SetupCost,
                x.MinPages,
                x.MaxPages
            })
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("finishings")]
    public async Task<IActionResult> GetFinishings([FromQuery] string? productType)
    {
        var query = _db.MstFinishings
            .Where(x => x.IsActive == true);

        if (!string.IsNullOrEmpty(productType))
        {
            var filteredQuery = query.Where(x =>
                x.SupportedJobTypes == null ||
                x.SupportedJobTypes.ToLower() == "all" ||
                x.SupportedJobTypes.ToLower().Contains(productType.ToLower()));

            if (await filteredQuery.AnyAsync())
                query = filteredQuery;
        }

        var items = await query
            .OrderBy(x => x.FinishingName)
            .Select(x => new
            {
                x.FinishingId,
                x.FinishingCode,
                x.FinishingName,
                x.FinishingCategory,
                x.FinishingType,
                x.MaxSpeedPerHour,
                SupportedJobTypes = x.SupportedJobTypes
            })
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("designings")]
    public async Task<IActionResult> GetDesignings([FromQuery] string? productType)
    {
        var query = _db.MstDesignings
            .Where(x => x.IsActive == true);

        if (!string.IsNullOrEmpty(productType))
        {
            var filteredQuery = query.Where(x =>
                x.JobTypesSupported == null ||
                x.JobTypesSupported.ToLower() == "all" ||
                x.JobTypesSupported.ToLower().Contains(productType.ToLower()));

            if (await filteredQuery.AnyAsync())
                query = filteredQuery;
        }

        var items = await query
            .OrderBy(x => x.DesignName)
            .Select(x => new
            {
                x.DesigningId,
                x.DesignCode,
                x.DesignName,
                x.DesignCategory,
                x.DesignType,
                x.BaseCost,
                x.CostUnit,
                x.AvgTimeHours
            })
            .ToListAsync();
        return Ok(items);
    }

    [HttpPost("ai-recommend")]
    public async Task<IActionResult> GetAiRecommendations([FromBody] RateCalcAiRecommendRequest request)
    {
        if (request.JobTypeId <= 0)
            return BadRequest(new { message = "Job type is required for smart recommendation." });

        var jobType = await _db.MstJobTypes.AsNoTracking().FirstOrDefaultAsync(x => x.Jobtypeid == request.JobTypeId);
        if (jobType == null)
            return NotFound(new { message = "Job type not found." });

        var productType = request.ProductTypeId.HasValue
            ? await _db.MstPrintProductTypes.AsNoTracking().FirstOrDefaultAsync(x => x.Printproducttypeid == request.ProductTypeId.Value)
            : null;

        var productSize = request.ProductSizeId.HasValue
            ? await _db.MstPrintProductSizes.AsNoTracking().FirstOrDefaultAsync(x => x.Productsizeid == request.ProductSizeId.Value)
            : null;

        var trimWidth = request.TrimWidthMm > 0 ? request.TrimWidthMm : (productSize?.Widthmm ?? 0);
        var trimHeight = request.TrimHeightMm > 0 ? request.TrimHeightMm : (productSize?.Heightmm ?? 0);
        var printingMode = (request.PrintingMode ?? jobType.Printingmode ?? "OFFSET").ToUpperInvariant();
        var printingSides = request.PrintingSides > 0 ? request.PrintingSides : 2;
        var totalPages = request.TotalPages > 0 ? request.TotalPages : 2;
        var quantity = request.Quantity > 0 ? request.Quantity : 1;

        var partDetails = (request.PartDetails ?? [])
            .Where(p => p.ProductPartId > 0 && p.NoOfPages > 0)
            .ToList();

        var maxColors = partDetails.Count > 0
            ? partDetails.Max(p => p.Colors > 0 ? p.Colors : 4)
            : 4;

        var recommendations = new
        {
            machineId = (long?)null,
            plateId = (long?)null,
            inkCodes = new List<string>(),
            bindingIds = new List<long>(),
            finishingIds = new List<long>(),
            partPapers = new List<object>(),
            globalPaperId = (long?)null,
            insights = new List<string>(),
            warnings = new List<string>()
        };

        var insights = recommendations.insights;
        var warnings = recommendations.warnings;

        // Paper recommendations (global + part-wise)
        var papersQuery = _db.MstPapers.AsNoTracking().Where(x => x.IsActive == true);
        if (!string.IsNullOrWhiteSpace(productType?.Productname))
        {
            var pt = productType.Productname.ToLower();
            var filtered = papersQuery.Where(x => x.SupportedJobTypes == null || x.SupportedJobTypes.ToLower() == "all" || x.SupportedJobTypes.ToLower().Contains(pt));
            if (await filtered.AnyAsync())
                papersQuery = filtered;
        }

        var papers = await papersQuery
            .OrderBy(x => x.PaperName)
            .Select(x => new
            {
                x.PaperId,
                x.PaperName,
                x.SupportedUsage,
                x.CostPerSheet,
                x.CostPerKg,
                x.Gsm,
                x.SheetWidthMm,
                x.SheetLengthMm
            })
            .ToListAsync();

        static decimal PaperCostScore(decimal? costPerSheet, decimal? costPerKg)
        {
            if (costPerSheet.HasValue && costPerSheet.Value > 0) return costPerSheet.Value;
            if (costPerKg.HasValue && costPerKg.Value > 0) return costPerKg.Value;
            return decimal.MaxValue;
        }

        var paperAssignments = new List<object>();
        if (partDetails.Count > 0)
        {
            foreach (var part in partDetails)
            {
                var partName = (part.PartName ?? string.Empty).ToLowerInvariant();
                var isCover = partName.Contains("cover");

                var partPaperCandidates = papers
                    .Where(p => isCover
                        ? (p.SupportedUsage ?? string.Empty).ToLower() == "cover"
                        : (p.SupportedUsage ?? string.Empty).ToLower() != "cover")
                    .ToList();

                if (partPaperCandidates.Count == 0)
                    partPaperCandidates = papers;

                var selectedPaper = partPaperCandidates
                    .OrderBy(p => PaperCostScore(p.CostPerSheet, p.CostPerKg))
                    .FirstOrDefault();

                if (selectedPaper != null)
                {
                    paperAssignments.Add(new
                    {
                        part.ProductPartId,
                        paperId = selectedPaper.PaperId,
                        paperName = selectedPaper.PaperName
                    });
                }
            }
        }

        var globalPaper = papers
            .Where(p => (p.SupportedUsage ?? string.Empty).ToLower() != "cover")
            .OrderBy(p => PaperCostScore(p.CostPerSheet, p.CostPerKg))
            .FirstOrDefault() ?? papers.OrderBy(p => PaperCostScore(p.CostPerSheet, p.CostPerKg)).FirstOrDefault();

        if (globalPaper != null)
            insights.Add($"Smart paper picked: {globalPaper.PaperName} ({globalPaper.Gsm} GSM)." );
        else
            warnings.Add("No active paper master found for this product type.");

        // Machine recommendation
        long? machineId = null;
        string MapModeToMachineCategory(string mode)
        {
            return mode switch
            {
                "OFFSET" => "OFFSET",
                "DIGITAL" => "DIGITAL",
                "SCREEN" => "SCREEN",
                "FLEX" => "FLEX",
                "UV" => "OFFSET",
                _ => string.Empty
            };
        }

        if (jobType.Isprintingrequired == true)
        {
            var category = MapModeToMachineCategory(printingMode);
            var machineQuery = _db.MstMachines.AsNoTracking().Where(x => x.IsActive == true);
            if (!string.IsNullOrEmpty(category))
            {
                var filtered = machineQuery.Where(x => x.MachineCategory == null || x.MachineCategory == category);
                if (await filtered.AnyAsync())
                    machineQuery = filtered;
            }

            var machines = await machineQuery
                .Select(m => new
                {
                    m.MachineId,
                    m.MachineName,
                    m.MaxSheetWidthMm,
                    m.MaxSheetLengthMm,
                    m.MaxColors,
                    m.PrintingSide,
                    m.HourlyRunningCost,
                    m.MaxSpeed
                })
                .ToListAsync();

            if (machines.Count > 0)
            {
                var maxHourly = Math.Max(1m, machines.Max(m => m.HourlyRunningCost ?? 0));
                var requiredImpressions = Math.Ceiling(quantity * (decimal)totalPages / Math.Max(1, printingSides));

                machineId = machines
                    .Select(m =>
                    {
                        var score = 0m;
                        var mw = m.MaxSheetWidthMm ?? 0;
                        var ml = m.MaxSheetLengthMm ?? 0;
                        var fits = trimWidth > 0 && trimHeight > 0 && ((mw >= trimWidth && ml >= trimHeight) || (mw >= trimHeight && ml >= trimWidth));
                        if (fits) score += 50;
                        if ((m.MaxColors ?? 0) >= maxColors) score += 25;
                        if (printingSides == 2 && (m.PrintingSide ?? "").Contains("2")) score += 10;
                        if ((m.MaxSpeed ?? 0) > 0)
                        {
                            var prodHours = requiredImpressions / (decimal)m.MaxSpeed.Value;
                            score += prodHours <= 8 ? 10 : prodHours <= 16 ? 6 : 2;
                        }

                        var costRatio = 1m - ((m.HourlyRunningCost ?? 0) / maxHourly);
                        score += Math.Max(0, costRatio * 15m);

                        return new { m, score };
                    })
                    .OrderByDescending(x => x.score)
                    .Select(x => x.m.MachineId)
                    .FirstOrDefault();

                if (machineId > 0)
                {
                    var machineName = machines.First(x => x.MachineId == machineId).MachineName;
                    insights.Add($"Machine optimized for size {trimWidth}×{trimHeight} mm, {maxColors} color(s), and quantity {quantity}: {machineName}.");
                }
            }

            if (!machineId.HasValue || machineId <= 0)
                warnings.Add("No active machine matched current mode/size constraints.");
        }

        // Plate recommendation
        long? plateId = null;
        if (jobType.Isctprequired == true)
        {
            var plate = await _db.MstPlates.AsNoTracking()
                .Where(x => x.IsActive == true)
                .OrderBy(x => (x.PlateCost ?? 0) + (x.ProcessingCost ?? 0))
                .Select(x => new { x.PlateId, x.PlateName })
                .FirstOrDefaultAsync();

            if (plate != null)
            {
                plateId = plate.PlateId;
                insights.Add($"Plate selected by lowest total plate + processing cost: {plate.PlateName}.");
            }
            else
            {
                warnings.Add("No active plate master found.");
            }
        }

        // Ink recommendation
        var inkCodes = new List<string>();
        if (jobType.Isprintingrequired == true)
        {
            var modePrefixMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["OFFSET"] = "INK_OFF_",
                ["DIGITAL"] = "INK_DIG_",
                ["SCREEN"] = "INK_SCR_",
                ["FLEX"] = "INK_FLEXO_",
                ["UV"] = "INK_UV_"
            };

            modePrefixMap.TryGetValue(printingMode, out var modePrefix);

            var inks = await _db.MstInks.AsNoTracking()
                .Where(x => x.IsActive == true)
                .OrderBy(x => x.InkName)
                .Select(x => new { x.InkCode, x.ColorName })
                .ToListAsync();

            var modeInks = string.IsNullOrEmpty(modePrefix)
                ? inks
                : inks.Where(x => (x.InkCode ?? string.Empty).ToUpper().StartsWith(modePrefix.ToUpper())).ToList();

            if (modeInks.Count == 0)
                modeInks = inks;

            string[] preferredOrder = ["black", "cyan", "magenta", "yellow"];
            foreach (var color in preferredOrder)
            {
                if (inkCodes.Count >= maxColors) break;
                var ink = modeInks.FirstOrDefault(i => (i.ColorName ?? string.Empty).ToLower().Contains(color));
                if (ink != null && !inkCodes.Contains(ink.InkCode ?? string.Empty))
                    inkCodes.Add(ink.InkCode ?? string.Empty);
            }

            if (inkCodes.Count < maxColors)
            {
                foreach (var ink in modeInks)
                {
                    if (inkCodes.Count >= maxColors) break;
                    if (!string.IsNullOrWhiteSpace(ink.InkCode) && !inkCodes.Contains(ink.InkCode))
                        inkCodes.Add(ink.InkCode);
                }
            }

            if (inkCodes.Count > 0)
                insights.Add($"Ink profile auto-matched for {printingMode} with {inkCodes.Count} color channel(s).");
            else
                warnings.Add("No active inks found for recommendation.");
        }

        // Binding recommendation
        var bindingIds = new List<long>();
        if (jobType.Isbindingrequired == true)
        {
            var bindingQuery = _db.MstBindings.AsNoTracking().Where(x => x.IsActive == true);
            if (!string.IsNullOrWhiteSpace(productType?.Productname))
            {
                var pt = productType.Productname.ToLower();
                var filtered = bindingQuery.Where(x => x.SupportedJobTypes == null || x.SupportedJobTypes.ToLower() == "all" || x.SupportedJobTypes.ToLower().Contains(pt));
                if (await filtered.AnyAsync())
                    bindingQuery = filtered;
            }

            var binding = await bindingQuery
                .Where(x => (!x.MinPages.HasValue || totalPages >= x.MinPages.Value) && (!x.MaxPages.HasValue || totalPages <= x.MaxPages.Value))
                .OrderBy(x => x.CostPerBook ?? 0)
                .Select(x => new { x.BindingId, x.BindingName })
                .FirstOrDefaultAsync();

            if (binding == null)
            {
                binding = await bindingQuery
                    .OrderBy(x => x.CostPerBook ?? 0)
                    .Select(x => new { x.BindingId, x.BindingName })
                    .FirstOrDefaultAsync();
            }

            if (binding != null)
            {
                bindingIds.Add(binding.BindingId);
                insights.Add($"Binding selected by page range and cost: {binding.BindingName}.");
            }
        }

        // Finishing recommendation
        var finishingIds = new List<long>();
        if (jobType.Isfinishingrequired == true)
        {
            var finishingQuery = _db.MstFinishings.AsNoTracking().Where(x => x.IsActive == true);
            if (!string.IsNullOrWhiteSpace(productType?.Productname))
            {
                var pt = productType.Productname.ToLower();
                var filtered = finishingQuery.Where(x => x.SupportedJobTypes == null || x.SupportedJobTypes.ToLower() == "all" || x.SupportedJobTypes.ToLower().Contains(pt));
                if (await filtered.AnyAsync())
                    finishingQuery = filtered;
            }

            var finishing = await finishingQuery
                .OrderBy(x => (x.SetupCost ?? 0) + (x.CostPerSheet ?? 0))
                .Select(x => new { x.FinishingId, x.FinishingName })
                .FirstOrDefaultAsync();

            if (finishing != null)
            {
                finishingIds.Add(finishing.FinishingId);
                insights.Add($"Finishing selected by lowest setup + running cost: {finishing.FinishingName}.");
            }
        }

        return Ok(new
        {
            machineId,
            plateId,
            inkCodes,
            bindingIds,
            finishingIds,
            partPapers = paperAssignments,
            globalPaperId = globalPaper?.PaperId,
            insights,
            warnings
        });
    }

    [HttpPost("calculate")]
    public async Task<IActionResult> Calculate([FromBody] RateCalcRequest request)
    {
        // ── Costing Rules ──
        const decimal PaperCuttingWastePct = 2m;
        const decimal PaperHandlingWastePct = 1m;
        const decimal PaperStorageLossPct = 0.5m;
        const decimal MinOrderSheets = 500m;
        const decimal InkWastagePct = 5m;
        const decimal StartupInkGrams = 200m;
        const decimal PlateWastagePct = 3m;
        const decimal ReplatePct = 2m;
        const int MinPlateCount = 1;
        const decimal PerfectingFactor = 1.6m;
        const decimal DoubleSideFactor = 1.8m;
        const decimal ColorRegWastagePct = 2m;
        const decimal MachineEfficiencyPct = 85m;
        const decimal DowntimePct = 10m;
        const decimal PlateChangeMinutes = 15m;
        const decimal BindingWastagePct = 2m;
        const decimal FinishingWastagePct = 2m;
        const decimal PackingCostPerUnit = 2m;
        const decimal GstPercent = 18m;
        const decimal RoundOffTo = 10m;
        const decimal MinJobValue = 500m;

        decimal paperCost = 0, plateCost = 0, inkCost = 0, machineCost = 0;
        decimal finishingCost = 0, bindingCost = 0, designingCost = 0, packingCost = 0;
        var breakdown = new List<object>();
        var bom = new List<object>();
        var appliedRules = new List<object>();

        // ── Lookup Job Type Flags ──
        var jobType = await _db.MstJobTypes.FindAsync(request.JobTypeId);
        bool isDesignRequired = jobType?.Isdesignrequired ?? false;
        bool isDtpRequired = jobType?.Isdtprequired ?? false;
        bool isCtpRequired = jobType?.Isctprequired ?? false;
        bool isPrintingRequired = jobType?.Isprintingrequired ?? false;
        bool isBindingRequired = jobType?.Isbindingrequired ?? false;
        bool isFinishingRequired = jobType?.Isfinishingrequired ?? false;
        bool isFullProcess = jobType?.Isfullprocess ?? false;
        bool isSingleProcess = jobType?.Issingleprocess ?? false;
        bool isCustomerMaterial = request.IsCustomerMaterial || (jobType?.Iscustomermaterial ?? false);
        bool isOutsourceJob = jobType?.Isoutsourcejob ?? false;

        var partDetails = (request.PartDetails ?? [])
            .Where(p => p.ProductPartId > 0 && p.NoOfPages > 0)
            .ToList();
        var effectiveTotalPages = partDetails.Count > 0 ? partDetails.Sum(p => p.NoOfPages) : request.TotalPages;

        // Sides-aware: single side = 1 page per sheet face, both sides = 2 pages per sheet
        var printingSides = request.PrintingSides > 0 ? request.PrintingSides : 1;
        var pagesPerSheet = (decimal)printingSides; // 1 for single, 2 for both

        appliedRules.Add(new { rule = "Printing Sides", detail = $"{(printingSides == 2 ? "Both Sides" : "Single Side")} — {pagesPerSheet} page(s) per sheet face", impact = $"{effectiveTotalPages} pages → {Math.Ceiling(effectiveTotalPages / pagesPerSheet):N0} impressions" });

        // ── Paper Cost (with full rules) — per-part or global ──
        if (isPrintingRequired && !isCustomerMaterial)
        {
            // Build a list of (paperId, pages, colors, label) entries for costing
            var paperEntries = new List<(long paperId, int pages, int colors, string label)>();

            if (partDetails.Count > 0)
            {
                foreach (var part in partDetails)
                {
                    var pid = part.PaperId ?? request.PaperId;
                    if (pid.HasValue)
                        paperEntries.Add((pid.Value, part.NoOfPages, part.Colors, part.PartName ?? "Part"));
                }
            }
            else if (request.PaperId.HasValue)
            {
                paperEntries.Add((request.PaperId.Value, effectiveTotalPages, request.Colors, "All"));
            }

            foreach (var entry in paperEntries)
            {
                var paper = await _db.MstPapers.FindAsync(entry.paperId);
                if (paper == null) continue;

                var sheetArea = (paper.SheetLengthMm ?? 0) * (paper.SheetWidthMm ?? 0) / 1_000_000m;
                var trimArea = request.TrimWidthMm * request.TrimHeightMm / 1_000_000m;
                var upsPerSheet = trimArea > 0 ? Math.Floor(sheetArea / trimArea) : 1;
                if (upsPerSheet < 1) upsPerSheet = 1;

                var baseSheets = Math.Ceiling(request.Quantity * (entry.pages / pagesPerSheet) / upsPerSheet);

                // Apply paper wastage rules: cutting + handling + storage
                var totalWastagePct = PaperCuttingWastePct + PaperHandlingWastePct + PaperStorageLossPct;
                // Color registration wastage
                if (entry.colors > 1)
                    totalWastagePct += ColorRegWastagePct;
                var wastageSheets = Math.Ceiling(baseSheets * totalWastagePct / 100m);
                var totalSheets = baseSheets + wastageSheets;

                // Enforce minimum order quantity
                if (totalSheets < MinOrderSheets)
                    totalSheets = MinOrderSheets;

                var weightPerSheet = sheetArea * paper.Gsm / 1000m;
                var totalWeightKg = totalSheets * weightPerSheet;
                var partPaperCost = paper.CostPerSheet.HasValue
                    ? totalSheets * paper.CostPerSheet.Value
                    : totalWeightKg * (paper.CostPerKg ?? 0);
                paperCost += partPaperCost;

                appliedRules.Add(new { rule = "Paper Wastage", detail = $"[{entry.label}] Cutting {PaperCuttingWastePct}% + Handling {PaperHandlingWastePct}% + Storage {PaperStorageLossPct}%" + (entry.colors > 1 ? $" + Color Reg. {ColorRegWastagePct}%" : ""), impact = $"+{wastageSheets:N0} sheets" });
                if (baseSheets + wastageSheets < MinOrderSheets)
                    appliedRules.Add(new { rule = "Min Order Qty", detail = $"[{entry.label}] Enforced minimum {MinOrderSheets:N0} sheets", impact = $"Adjusted from {baseSheets + wastageSheets:N0}" });

                breakdown.Add(new { icon = "📄", name = $"Paper ({entry.label})", category = "Material", detail = $"{paper.PaperName} ({paper.Gsm} GSM) × {totalSheets:N0} sheets (incl. {totalWastagePct}% wastage)", amount = partPaperCost });
                bom.Add(new { category = "Material", item_group = "PAPER", item = paper.PaperName, item_code = paper.PaperCode, specification = $"{paper.Gsm} GSM ({entry.label})", quantity = paper.CostPerSheet.HasValue ? totalSheets : totalWeightKg, unit = paper.CostPerSheet.HasValue ? "Sheets" : "Kg", rate = paper.CostPerSheet ?? paper.CostPerKg ?? 0, amount = partPaperCost });
            }
        }

        // ── Plate Cost (with full rules) ──
        if (request.PlateId.HasValue && isCtpRequired)
        {
            var plate = await _db.MstPlates.FindAsync(request.PlateId.Value);
            if (plate != null)
            {
                var sides = request.PrintingSides > 0 ? request.PrintingSides : 1;
                var basePlates = partDetails.Count > 0
                    ? partDetails.Sum(x => Math.Max(1, x.Colors) * sides)
                    : (request.Colors > 0 ? request.Colors : 4) * sides;

                // Apply plate wastage and replate rules
                var wastagePlates = (int)Math.Ceiling(basePlates * PlateWastagePct / 100m);
                var replatePlates = (int)Math.Ceiling(basePlates * ReplatePct / 100m);
                var totalPlates = basePlates + wastagePlates + replatePlates;
                if (totalPlates < MinPlateCount) totalPlates = MinPlateCount;

                // Deduct plates already received from customer
                if (request.PlatesReceived > 0)
                {
                    var deducted = Math.Min(request.PlatesReceived, totalPlates);
                    totalPlates = Math.Max(0, totalPlates - deducted);
                    appliedRules.Add(new { rule = "Plates Received", detail = $"Customer provided {request.PlatesReceived} plate(s) — deducted from required total", impact = $"Reduced to {totalPlates} plates to make" });
                }

                var plateRate = (plate.PlateCost ?? 0) + (plate.ProcessingCost ?? 0);
                plateCost = totalPlates * plateRate;

                appliedRules.Add(new { rule = "Plate Wastage", detail = $"Wastage {PlateWastagePct}% + Replate {ReplatePct}%, Min count {MinPlateCount}", impact = $"{basePlates} → {totalPlates} plates" });

                breakdown.Add(new { icon = "🔲", name = "Plates", category = "Prepress", detail = $"{plate.PlateName} × {totalPlates} plates (incl. {PlateWastagePct}% waste + {ReplatePct}% replate)", amount = plateCost });
                bom.Add(new { category = "Material", item_group = "PLATE", item = plate.PlateName, item_code = plate.PlateCode, specification = plate.PlateType, quantity = totalPlates, unit = "Plates", rate = plateRate, amount = plateCost });
            }
        }

        // ── Ink Cost (with wastage + startup rules — per-part color-aware) ──
        if (request.InkCodes?.Any() == true && isPrintingRequired)
        {
            var sheetArea = request.TrimWidthMm * request.TrimHeightMm / 1_000_000m;

            // Pre-load and sort all selected inks by CMYK order (Black last for 1C selection)
            var allSelectedInks = new List<(persistence.Models.MstInk ink, int cmykOrder)>();
            foreach (var inkCode in request.InkCodes.Distinct())
            {
                var ink = await _db.MstInks.FirstOrDefaultAsync(x => x.InkCode == inkCode);
                if (ink == null) continue;
                var colorLower = (ink.ColorName ?? "").ToLower();
                int order = colorLower.Contains("cyan") ? 0
                    : colorLower.Contains("magenta") ? 1
                    : colorLower.Contains("yellow") ? 2
                    : colorLower.Contains("black") || colorLower.Contains("key") ? 3
                    : 4; // spot colors
                allSelectedInks.Add((ink, order));
            }
            allSelectedInks = allSelectedInks.OrderBy(x => x.cmykOrder).ToList();

            if (partDetails.Count > 0)
            {
                foreach (var part in partDetails)
                {
                    var partArea = sheetArea * request.Quantity * (part.NoOfPages / pagesPerSheet);

                    // Determine which inks apply to this part based on its color count
                    var partColors = Math.Max(1, part.Colors);
                    List<persistence.Models.MstInk> partInks;
                    if (partColors == 1)
                    {
                        // 1C = Black/Key only
                        var blackInk = allSelectedInks.FirstOrDefault(x => x.cmykOrder == 3).ink;
                        partInks = blackInk != null ? [blackInk] : allSelectedInks.Take(1).Select(x => x.ink).ToList();
                    }
                    else
                    {
                        // Take up to partColors inks (CMYK first, then spots)
                        partInks = allSelectedInks.Take(Math.Min(partColors, allSelectedInks.Count)).Select(x => x.ink).ToList();
                    }

                    foreach (var ink in partInks)
                    {
                        var baseInkKg = partArea * (ink.ConsumptionGsm ?? 0) / 1000m;
                        var wastageKg = baseInkKg * InkWastagePct / 100m;
                        var startupKg = StartupInkGrams / 1000m;
                        var inkKg = baseInkKg + wastageKg + startupKg;
                        var cost = inkKg * (ink.CostPerKg ?? 0);
                        inkCost += cost;
                        breakdown.Add(new { icon = "🎨", name = $"Ink - {ink.ColorName}", category = "Material", detail = $"{part.PartName} × {inkKg:N2} kg (incl. {InkWastagePct}% waste + {StartupInkGrams}g startup)", amount = cost });
                        bom.Add(new { category = "Material", item_group = "INK", item = ink.InkName, item_code = ink.InkCode, specification = $"{ink.ColorName} ({part.PartName})", quantity = inkKg, unit = "Kg", rate = ink.CostPerKg ?? 0, amount = cost });
                    }
                }
            }
            else
            {
                foreach (var entry in allSelectedInks)
                {
                    var ink = entry.ink;
                    var totalArea = sheetArea * request.Quantity * (effectiveTotalPages / pagesPerSheet);
                    var baseInkKg = totalArea * (ink.ConsumptionGsm ?? 0) / 1000m;
                    var wastageKg = baseInkKg * InkWastagePct / 100m;
                    var startupKg = StartupInkGrams / 1000m;
                    var inkKg = baseInkKg + wastageKg + startupKg;
                    var cost = inkKg * (ink.CostPerKg ?? 0);
                    inkCost += cost;
                    breakdown.Add(new { icon = "🎨", name = $"Ink - {ink.ColorName}", category = "Material", detail = $"{ink.InkName} × {inkKg:N2} kg (incl. {InkWastagePct}% waste + {StartupInkGrams}g startup)", amount = cost });
                    bom.Add(new { category = "Material", item_group = "INK", item = ink.InkName, item_code = ink.InkCode, specification = ink.ColorName, quantity = inkKg, unit = "Kg", rate = ink.CostPerKg ?? 0, amount = cost });
                }
            }
            appliedRules.Add(new { rule = "Ink Wastage", detail = $"{InkWastagePct}% wastage + {StartupInkGrams}g startup per ink", impact = $"Applied to {request.InkCodes.Count} ink(s)" });
        }

        // ── Machine Cost (with efficiency, downtime, plate change, perfecting rules) ──
        if (request.MachineId.HasValue && isPrintingRequired)
        {
            var machine = await _db.MstMachines.FindAsync(request.MachineId.Value);
            if (machine != null)
            {
                var speed = machine.MaxSpeed ?? 10000;
                // Apply machine efficiency
                var effectiveSpeed = speed * MachineEfficiencyPct / 100m;
                if (effectiveSpeed < 1) effectiveSpeed = 1;

                var setupMins = machine.SetupTimeMinutes ?? 30;
                var trimArea = request.TrimWidthMm * request.TrimHeightMm / 1_000_000m;
                var sheetArea = (machine.MaxSheetLengthMm ?? 700) * (machine.MaxSheetWidthMm ?? 500) / 1_000_000m;
                var upsPerSheet = trimArea > 0 ? Math.Floor(sheetArea / trimArea) : 1;
                if (upsPerSheet < 1) upsPerSheet = 1;
                var totalSheets = Math.Ceiling(request.Quantity * (effectiveTotalPages / pagesPerSheet) / upsPerSheet);

                // Apply perfecting/double-side factor (extra machine time for second pass)
                if (request.PrintingSides == 2)
                {
                    var sidesFactor = (request.PrintingMode?.ToUpper() == "OFFSET") ? PerfectingFactor : DoubleSideFactor;
                    totalSheets = Math.Ceiling(totalSheets * sidesFactor);
                    appliedRules.Add(new { rule = "Perfecting Factor", detail = $"{(request.PrintingMode?.ToUpper() == "OFFSET" ? "Offset perfecting" : "Double-side pass")} factor {sidesFactor}x for machine runtime", impact = $"Adjusted sheet impressions" });
                }

                var runHours = totalSheets / effectiveSpeed;
                var setupHours = setupMins / 60m;

                // Plate change time
                var plateChanges = request.Colors > 1 ? (request.Colors - 1) : 0;
                var plateChangeHours = plateChanges * PlateChangeMinutes / 60m;

                // Downtime
                var productionHours = runHours + setupHours + plateChangeHours;
                var downtimeHours = productionHours * DowntimePct / 100m;
                var totalHours = productionHours + downtimeHours;

                machineCost = totalHours * (machine.HourlyRunningCost ?? 0);

                appliedRules.Add(new { rule = "Machine Efficiency", detail = $"{MachineEfficiencyPct}% efficiency, {DowntimePct}% downtime, {PlateChangeMinutes}min/plate change", impact = $"{totalHours:N2} hrs total" });

                breakdown.Add(new { icon = "⚙️", name = "Machine", category = "Processing", detail = $"{machine.MachineName} × {totalHours:N2} hrs (eff. {MachineEfficiencyPct}%, down {DowntimePct}%)", amount = machineCost });
                bom.Add(new { category = "Processing", item_group = "OTHER", item = machine.MachineName, item_code = machine.MachineCode, specification = machine.MachineCategory, quantity = totalHours, unit = "Hours", rate = machine.HourlyRunningCost ?? 0, amount = machineCost });
            }
        }

        // ── Binding Cost (with wastage rule, guarded by flag) ──
        if (request.BindingIds?.Any() == true && isBindingRequired)
        {
            foreach (var bindingId in request.BindingIds.Distinct())
            {
                var binding = await _db.MstBindings.FindAsync(bindingId);
                if (binding == null) continue;

                var baseQty = request.Quantity;
                var wastageQty = (int)Math.Ceiling(baseQty * BindingWastagePct / 100m);
                var totalQty = baseQty + wastageQty;
                var cost = totalQty * (binding.CostPerBook ?? 0) + (binding.SetupCost ?? 0);
                bindingCost += cost;
                breakdown.Add(new { icon = "📚", name = "Binding", category = "Postpress", detail = $"{binding.BindingName} × {totalQty} copies (incl. {BindingWastagePct}% wastage)", amount = cost });
                bom.Add(new { category = "Postpress", item_group = "OTHER", item = binding.BindingName, item_code = binding.BindingCode, specification = binding.BindingType, quantity = totalQty, unit = "Copies", rate = binding.CostPerBook ?? 0, amount = cost });
            }
            appliedRules.Add(new { rule = "Binding Wastage", detail = $"{BindingWastagePct}% extra copies for wastage", impact = $"Applied to {request.BindingIds.Count} binding(s)" });
        }

        // ── Finishing Cost (with wastage rule, guarded by flag — per copy, not per sheet) ──
        if (request.FinishingIds?.Any() == true && isFinishingRequired)
        {
            foreach (var finId in request.FinishingIds.Distinct())
            {
                var finishing = await _db.MstFinishings.FindAsync(finId);
                if (finishing != null)
                {
                    var costPerSheet = finishing.CostPerSheet ?? 0;
                    var setupCost = finishing.SetupCost ?? 0;
                    var baseCopies = (decimal)request.Quantity;
                    var wastageCopies = Math.Ceiling(baseCopies * FinishingWastagePct / 100m);
                    var totalCopies = baseCopies + wastageCopies;
                    var cost = totalCopies * costPerSheet + setupCost;
                    finishingCost += cost;
                    breakdown.Add(new { icon = "✨", name = $"Finishing - {finishing.FinishingName}", category = "Postpress", detail = $"{totalCopies:N0} copies (incl. {FinishingWastagePct}% wastage)", amount = cost });
                    bom.Add(new { category = "Postpress", item_group = "OTHER", item = finishing.FinishingName, item_code = finishing.FinishingCode, specification = finishing.FinishingType, quantity = totalCopies, unit = "Copies", rate = costPerSheet, amount = cost });
                }
            }
            appliedRules.Add(new { rule = "Finishing Wastage", detail = $"{FinishingWastagePct}% extra copies for wastage", impact = $"Applied to {request.FinishingIds.Count} process(es)" });
        }

        // ── Designing Cost (guarded by flag) ──
        if (request.DesigningIds?.Any() == true && (isDesignRequired || isDtpRequired))
        {
            foreach (var designingId in request.DesigningIds.Distinct())
            {
                var designing = await _db.MstDesignings.FindAsync(designingId);
                if (designing == null) continue;

                var cost = designing.BaseCost ?? 0;
                designingCost += cost;
                breakdown.Add(new { icon = "🎨", name = "Designing", category = "Prepress", detail = designing.DesignName, amount = cost });
                bom.Add(new { category = "Prepress", item_group = "OTHER", item = designing.DesignName, item_code = designing.DesignCode, specification = designing.DesignType, quantity = 1, unit = "Job", rate = designing.BaseCost ?? 0, amount = cost });
            }
        }

        // ── Outsource Cost (vendor-provided cost) ──
        if (isOutsourceJob && request.OutsourceCost.HasValue && request.OutsourceCost.Value > 0)
        {
            var osCost = request.OutsourceCost.Value;
            breakdown.Add(new { icon = "🏭", name = "Outsource", category = "External", detail = $"Vendor job cost", amount = osCost });
            bom.Add(new { category = "External", item_group = "OTHER", item = "Outsource Job", item_code = "", specification = jobType?.Jobtypename ?? "Outsource", quantity = 1m, unit = "Job", rate = osCost, amount = osCost });
            machineCost += osCost; // included in machine cost bucket
        }

        // ── Labour Cost (job work) ──
        if (request.LabourHours.HasValue && request.LabourRate.HasValue && request.LabourHours.Value > 0)
        {
            var labourCost = request.LabourHours.Value * request.LabourRate.Value;
            breakdown.Add(new { icon = "👷", name = "Labour", category = "Service", detail = $"{request.LabourHours.Value:N1} hrs × ₹{request.LabourRate.Value:N0}/hr", amount = labourCost });
            bom.Add(new { category = "Service", item_group = "OTHER", item = "Job Work Labour", item_code = "", specification = "Manual", quantity = request.LabourHours.Value, unit = "Hours", rate = request.LabourRate.Value, amount = labourCost });
            machineCost += labourCost;
        }

        // ── Packing Cost ──
        if (isFullProcess && request.Quantity > 0)
        {
            packingCost = request.Quantity * PackingCostPerUnit;
            breakdown.Add(new { icon = "📦", name = "Packing", category = "Postpress", detail = $"{request.Quantity} × ₹{PackingCostPerUnit}/unit", amount = packingCost });
            bom.Add(new { category = "Postpress", item_group = "OTHER", item = "Packing", item_code = "", specification = "Standard", quantity = (decimal)request.Quantity, unit = "Units", rate = PackingCostPerUnit, amount = packingCost });
            appliedRules.Add(new { rule = "Packing Cost", detail = $"₹{PackingCostPerUnit}/unit for full process jobs", impact = $"₹{packingCost:N2}" });
        }

        // ── Totals (with rounding, min job value, GST) ──
        var grandTotal = paperCost + plateCost + inkCost + machineCost + finishingCost + bindingCost + designingCost + packingCost;

        // Enforce minimum job value
        if (grandTotal < MinJobValue && grandTotal > 0)
        {
            appliedRules.Add(new { rule = "Min Job Value", detail = $"Minimum ₹{MinJobValue:N0} enforced", impact = $"Adjusted from ₹{grandTotal:N2}" });
            grandTotal = MinJobValue;
        }

        // Round to nearest 10
        grandTotal = Math.Ceiling(grandTotal / RoundOffTo) * RoundOffTo;

        var taxRate = GstPercent / 100m;
        var taxAmount = Math.Round(grandTotal * taxRate, 2);
        var netTotal = grandTotal + taxAmount;

        // Round net total
        netTotal = Math.Ceiling(netTotal / RoundOffTo) * RoundOffTo;

        var costPerUnit = request.Quantity > 0 ? Math.Round(netTotal / request.Quantity, 2) : 0;

        appliedRules.Add(new { rule = "GST", detail = $"{GstPercent}% tax applied", impact = $"₹{taxAmount:N2}" });
        appliedRules.Add(new { rule = "Rounding", detail = $"Rounded to nearest ₹{RoundOffTo:N0}", impact = $"Net ₹{netTotal:N2}" });

        breakdown.Add(new { icon = "📊", name = "Subtotal", category = "Total", detail = "", amount = grandTotal });
        breakdown.Add(new { icon = "🏛️", name = $"GST ({GstPercent}%)", category = "Tax", detail = "", amount = taxAmount });
        breakdown.Add(new { icon = "💰", name = "Net Total", category = "Grand Total", detail = "", amount = netTotal });

        return Ok(new
        {
            paperCost,
            plateCost,
            inkCost,
            machineCost,
            finishingCost,
            bindingCost,
            designingCost,
            packingCost,
            grandTotal,
            taxAmount,
            netTotal,
            costPerUnit,
            breakdown,
            bom,
            appliedRules,
            jobTypeFlags = new
            {
                isDesignRequired,
                isDtpRequired,
                isCtpRequired,
                isPrintingRequired,
                isBindingRequired,
                isFinishingRequired,
                isFullProcess,
                isSingleProcess,
                isCustomerMaterial,
                isOutsourceJob
            }
        });
    }

    [HttpPost("send-estimation")]
    public async Task<IActionResult> SendEstimation([FromBody] SendEstimationRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.CustomerEmail))
            return BadRequest("Customer email is required.");

        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return Unauthorized();

        // ── Save to HybJobRateCalculator before sharing ──
        var qty = int.TryParse(req.Quantity, out var q) ? q : 0;
        var rateCalc = new HybJobRateCalculator
        {
            CalcRefNo = req.RefNo,
            JobTypeId = req.JobTypeId,
            ProductTypeId = req.ProductTypeId,
            ProductSizeId = req.ProductSizeId,
            PartyId = req.PartyId,
            Quantity = qty,
            TotalPages = req.TotalPages,
            TrimWidthMm = req.TrimWidthMm,
            TrimHeightMm = req.TrimHeightMm,
            PrintingMode = req.PrintingMode,
            IsCustomerMaterial = req.IsCustomerMaterial,
            GrandTotal = req.GrandTotal,
            TaxAmount = req.TaxAmount,
            NetTotal = req.NetTotal,
            CostPerUnit = req.CostPerUnit,
            PartsData = req.PartsData,
            CostBreakdown = req.CostBreakdown,
            BomData = req.BomData,
            RecommendedMachines = req.RecommendedMachines,
            CalcInputSnapshot = req.CalcInputSnapshot,
            ConfigData = req.ConfigData,
            Status = "SHARED",
            Version = 1,
            CreatedBy = user.UserId,
            CreatedOn = DateTime.UtcNow,
            ClientRemarks = $"Sent via Email to {req.CustomerName} ({req.CustomerEmail})"
        };
        _db.HybJobRateCalculators.Add(rateCalc);
        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "Estimation {RefNo} (ID:{Id}) sent via Email to {Email} for {Customer}. Net Total: {NetTotal}",
            req.RefNo, rateCalc.RateCalcId, req.CustomerEmail, req.CustomerName, req.NetTotal);

        // ── Send actual email to customer ──
        try
        {
            var subject = $"Estimation {req.RefNo} — {req.JobType ?? "Print Job"}";
            var body = BuildEstimationEmailHtml(req);
            await _notifier.SendEmailAsync(req.CustomerEmail, subject, body);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send estimation email to {Email}", req.CustomerEmail);
        }

        // Log user activity
        var activity = ActivityLogEntry.FromUser(user, "RATE_CALCULATOR", "ESTIMATION_EMAIL",
            $"Estimation {req.RefNo} Sent via Email");
        activity.SubModule = "ESTIMATION";
        activity.ActivityCategory = "SEND";
        activity.EntityType = "ESTIMATION";
        activity.EntityId = rateCalc.RateCalcId;
        activity.EntityCode = req.RefNo;
        activity.Description = $"Estimation sent to {req.CustomerName} ({req.CustomerEmail}) — Net: ₹{req.NetTotal}";
        activity.Severity = "INFO";
        await _activityService.LogActivityAsync(activity);

        // Notify sales team, management, admin
        await _activityService.LogNotificationAsync(new UserNotificationEntry
        {
            UserId = user.UserId,
            Title = $"Estimation {req.RefNo}",
            Message = $"Estimation sent to {req.CustomerName} via Email. Job: {req.JobType}, Net Total: ₹{req.NetTotal}",
            Icon = "bi-envelope-check",
            Color = "azure",
            Module = "RATE_CALCULATOR",
            EventType = "ESTIMATION_SENT",
            ReferenceId = (int)rateCalc.RateCalcId,
            ReferenceUrl = "/RateCalculator",
            Priority = "NORMAL",
            ActionLabel = "View Estimation"
        });

        return Ok(new { message = "Estimation sent successfully", refNo = req.RefNo, rateCalcId = rateCalc.RateCalcId });
    }

    [HttpPost("log-estimation-activity")]
    public async Task<IActionResult> LogEstimationActivity([FromBody] LogEstimationActivityRequest req)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return Unauthorized();

        // ── Save to HybJobRateCalculator before logging ──
        var qty = int.TryParse(req.Quantity, out var q) ? q : 0;
        var rateCalc = new HybJobRateCalculator
        {
            CalcRefNo = req.RefNo,
            JobTypeId = req.JobTypeId,
            ProductTypeId = req.ProductTypeId,
            ProductSizeId = req.ProductSizeId,
            PartyId = req.PartyId,
            Quantity = qty,
            TotalPages = req.TotalPages,
            TrimWidthMm = req.TrimWidthMm,
            TrimHeightMm = req.TrimHeightMm,
            PrintingMode = req.PrintingMode,
            IsCustomerMaterial = req.IsCustomerMaterial,
            GrandTotal = req.GrandTotal,
            TaxAmount = req.TaxAmount,
            NetTotal = req.NetTotal,
            CostPerUnit = req.CostPerUnit,
            PartsData = req.PartsData,
            CostBreakdown = req.CostBreakdown,
            BomData = req.BomData,
            RecommendedMachines = req.RecommendedMachines,
            CalcInputSnapshot = req.CalcInputSnapshot,
            ConfigData = req.ConfigData,
            Status = "SHARED",
            Version = 1,
            CreatedBy = user.UserId,
            CreatedOn = DateTime.UtcNow,
            ClientRemarks = $"Shared via {req.Channel} to {req.CustomerName}"
        };
        _db.HybJobRateCalculators.Add(rateCalc);
        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "Estimation {RefNo} (ID:{Id}) shared via {Channel} to {Customer} ({Phone}). Net Total: {NetTotal}",
            req.RefNo, rateCalc.RateCalcId, req.Channel, req.CustomerName, req.CustomerPhone, req.NetTotal);

        // Log user activity
        var activity = ActivityLogEntry.FromUser(user, "RATE_CALCULATOR",
            $"ESTIMATION_{req.Channel.ToUpper()}", $"Estimation {req.RefNo} Shared via {req.Channel}");
        activity.SubModule = "ESTIMATION";
        activity.ActivityCategory = "SHARE";
        activity.EntityType = "ESTIMATION";
        activity.EntityId = rateCalc.RateCalcId;
        activity.EntityCode = req.RefNo;
        activity.Description = $"Estimation shared via {req.Channel} to {req.CustomerName} ({req.CustomerPhone}) — Net: ₹{req.NetTotal}";
        activity.Severity = "INFO";
        await _activityService.LogActivityAsync(activity);

        // Notify sales team
        await _activityService.LogNotificationAsync(new UserNotificationEntry
        {
            UserId = user.UserId,
            Title = $"Estimation {req.RefNo}",
            Message = $"Estimation shared via {req.Channel} to {req.CustomerName}. Job: {req.JobType}, Net Total: ₹{req.NetTotal}",
            Icon = "bi-share",
            Color = "cyan",
            Module = "RATE_CALCULATOR",
            EventType = "ESTIMATION_SHARED",
            ReferenceId = (int)rateCalc.RateCalcId,
            ReferenceUrl = "/RateCalculator",
            Priority = "NORMAL",
            ActionLabel = "View Estimation"
        });

        return Ok(new { rateCalcId = rateCalc.RateCalcId });
    }

    // ── Email HTML Builder ──
    private static string BuildEstimationEmailHtml(SendEstimationRequest req)
    {
        var breakdownHtml = "";
        if (req.IncludeBreakdown && req.Breakdown?.Count > 0)
        {
            breakdownHtml = @"
            <h3 style='color:#1e3a5f;margin-top:24px;'>Cost Breakdown</h3>
            <table style='width:100%;border-collapse:collapse;font-size:14px;'>
            <thead><tr style='background:#f0f4f8;'>
                <th style='padding:8px;text-align:left;border-bottom:2px solid #ddd;'>Item</th>
                <th style='padding:8px;text-align:left;border-bottom:2px solid #ddd;'>Category</th>
                <th style='padding:8px;text-align:right;border-bottom:2px solid #ddd;'>Amount</th>
            </tr></thead><tbody>";

            foreach (var item in req.Breakdown)
            {
                breakdownHtml += $@"
                <tr>
                    <td style='padding:6px 8px;border-bottom:1px solid #eee;'>{item.Name}</td>
                    <td style='padding:6px 8px;border-bottom:1px solid #eee;'>{item.Category}</td>
                    <td style='padding:6px 8px;border-bottom:1px solid #eee;text-align:right;'>₹{item.Amount:N2}</td>
                </tr>";
            }
            breakdownHtml += "</tbody></table>";
        }

        return $@"
        <div style='font-family:Segoe UI,Arial,sans-serif;max-width:600px;margin:0 auto;'>
            <div style='background:linear-gradient(135deg,#1e3a5f,#2563eb);padding:24px;border-radius:12px 12px 0 0;'>
                <h1 style='color:#fff;margin:0;font-size:22px;'>📊 Rate Estimation</h1>
                <p style='color:#c7d2fe;margin:6px 0 0;font-size:14px;'>Reference: {req.RefNo}</p>
            </div>
            <div style='padding:24px;background:#fff;border:1px solid #e5e7eb;border-top:none;border-radius:0 0 12px 12px;'>
                <p style='font-size:15px;color:#374151;'>Dear <strong>{req.CustomerName}</strong>,</p>
                <p style='font-size:14px;color:#6b7280;'>Please find below the estimation details for your print job:</p>
                <table style='width:100%;font-size:14px;margin:16px 0;'>
                    <tr><td style='padding:6px 0;color:#6b7280;'>Job Type:</td><td style='padding:6px 0;font-weight:600;'>{req.JobType ?? "—"}</td></tr>
                    <tr><td style='padding:6px 0;color:#6b7280;'>Product:</td><td style='padding:6px 0;font-weight:600;'>{req.ProductType ?? "—"}</td></tr>
                    <tr><td style='padding:6px 0;color:#6b7280;'>Quantity:</td><td style='padding:6px 0;font-weight:600;'>{req.Quantity ?? "—"}</td></tr>
                    <tr><td style='padding:6px 0;color:#6b7280;'>Size:</td><td style='padding:6px 0;font-weight:600;'>{req.Size ?? "—"}</td></tr>
                </table>
                <div style='background:#f0f9ff;border-radius:10px;padding:16px;margin:16px 0;'>
                    <table style='width:100%;font-size:15px;'>
                        <tr><td style='padding:4px 0;color:#374151;'>Subtotal:</td><td style='text-align:right;font-weight:600;'>₹{req.GrandTotal:N2}</td></tr>
                        <tr><td style='padding:4px 0;color:#374151;'>GST (18%):</td><td style='text-align:right;font-weight:600;'>₹{req.TaxAmount:N2}</td></tr>
                        <tr style='border-top:2px solid #bfdbfe;'><td style='padding:8px 0;color:#1e3a5f;font-size:17px;'><strong>Net Total:</strong></td><td style='text-align:right;font-size:17px;color:#1e3a5f;'><strong>₹{req.NetTotal:N2}</strong></td></tr>
                        <tr><td style='padding:4px 0;color:#6b7280;'>Cost Per Unit:</td><td style='text-align:right;color:#6b7280;'>₹{req.CostPerUnit:N2}</td></tr>
                    </table>
                </div>
                {breakdownHtml}
                <p style='font-size:13px;color:#9ca3af;margin-top:24px;'>This estimation is valid for 15 days from the date of generation. Terms & conditions apply.</p>
            </div>
        </div>";
    }
}

public class RateCalcRequest
{
    public int JobTypeId { get; set; }
    public int? ProductTypeId { get; set; }
    public int? ProductSizeId { get; set; }
    public int Quantity { get; set; }
    public int TotalPages { get; set; } = 2;
    public decimal TrimWidthMm { get; set; }
    public decimal TrimHeightMm { get; set; }
    public string? PrintingMode { get; set; }
    public int Colors { get; set; } = 4;
    public int PrintingSides { get; set; } = 1;
    public long? PaperId { get; set; }
    public long? MachineId { get; set; }
    public long? PlateId { get; set; }
    public List<string>? InkCodes { get; set; }
    public List<long>? FinishingIds { get; set; }
    public List<long>? BindingIds { get; set; }
    public List<long>? DesigningIds { get; set; }
    public List<RateCalcPartDetail>? PartDetails { get; set; }
    public bool IsCustomerMaterial { get; set; }
    public int PlatesReceived { get; set; } = 0;
    public decimal? OutsourceCost { get; set; }
    public decimal? LabourHours { get; set; }
    public decimal? LabourRate { get; set; }
    public decimal? AreaWidthFt { get; set; }
    public decimal? AreaHeightFt { get; set; }
}

public class RateCalcAiRecommendRequest
{
    public int JobTypeId { get; set; }
    public int? ProductTypeId { get; set; }
    public int? ProductSizeId { get; set; }
    public int Quantity { get; set; }
    public int TotalPages { get; set; } = 2;
    public decimal TrimWidthMm { get; set; }
    public decimal TrimHeightMm { get; set; }
    public string? PrintingMode { get; set; }
    public int PrintingSides { get; set; } = 2;
    public List<RateCalcPartDetail>? PartDetails { get; set; }
}

public class RateCalcPartDetail
{
    public int ProductPartId { get; set; }
    public string? PartName { get; set; }
    public int NoOfPages { get; set; }
    public int Colors { get; set; } = 4;
    public long? PaperId { get; set; }
}

public class SendEstimationRequest
{
    public string CustomerName { get; set; } = "";
    public string? CustomerPhone { get; set; }
    public string CustomerEmail { get; set; } = "";
    public string RefNo { get; set; } = "";
    public string? JobType { get; set; }
    public string? ProductType { get; set; }
    public string? Quantity { get; set; }
    public string? Size { get; set; }
    public bool IncludeBreakdown { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal NetTotal { get; set; }
    public decimal CostPerUnit { get; set; }
    public List<EstimationBreakdownItem>? Breakdown { get; set; }

    // ── Calculation input fields for saving to HybJobRateCalculator ──
    public int? JobTypeId { get; set; }
    public int? ProductTypeId { get; set; }
    public int? ProductSizeId { get; set; }
    public int? PartyId { get; set; }
    public int TotalPages { get; set; }
    public decimal TrimWidthMm { get; set; }
    public decimal TrimHeightMm { get; set; }
    public string? PrintingMode { get; set; }
    public bool IsCustomerMaterial { get; set; }
    public string? PartsData { get; set; }
    public string? CostBreakdown { get; set; }
    public string? BomData { get; set; }
    public string? RecommendedMachines { get; set; }
    public string? CalcInputSnapshot { get; set; }
    public string? ConfigData { get; set; }
}

public class EstimationBreakdownItem
{
    public string? Name { get; set; }
    public string? Category { get; set; }
    public string? Detail { get; set; }
    public decimal Amount { get; set; }
}

public class LogEstimationActivityRequest
{
    public string Channel { get; set; } = "";
    public string RefNo { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string? CustomerPhone { get; set; }
    public string? CustomerEmail { get; set; }
    public string? JobType { get; set; }
    public string? ProductType { get; set; }
    public string? Quantity { get; set; }
    public decimal NetTotal { get; set; }

    // ── Calculation input fields for saving to HybJobRateCalculator ──
    public int? JobTypeId { get; set; }
    public int? ProductTypeId { get; set; }
    public int? ProductSizeId { get; set; }
    public int? PartyId { get; set; }
    public int TotalPages { get; set; }
    public decimal TrimWidthMm { get; set; }
    public decimal TrimHeightMm { get; set; }
    public string? PrintingMode { get; set; }
    public bool IsCustomerMaterial { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal CostPerUnit { get; set; }
    public string? PartsData { get; set; }
    public string? CostBreakdown { get; set; }
    public string? BomData { get; set; }
    public string? RecommendedMachines { get; set; }
    public string? CalcInputSnapshot { get; set; }
    public string? ConfigData { get; set; }
}