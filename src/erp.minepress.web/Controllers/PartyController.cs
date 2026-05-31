using erp.minepress.infrastructure.ErrorLogging;
using erp.minepress.notification.Interfaces;
using erp.minepress.notification.Models;
using erp.minepress.persistence.Context;
using erp.minepress.persistence.Models;
using erp.minepress.web.Helpers;
using erp.minepress.web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace erp.minepress.web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PartyController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IUserActivityService _activityService;
    private readonly IEnumerable<INotificationChannelProvider> _channelProviders;
    private readonly ILogger<PartyController> _logger;
    private readonly ISystemErrorLogger _systemErrorLogger;

    public PartyController(
        ApplicationDbContext db,
        IUserActivityService activityService,
        IEnumerable<INotificationChannelProvider> channelProviders,
        ILogger<PartyController> logger,
        ISystemErrorLogger systemErrorLogger)
    {
        _db = db;
        _activityService = activityService;
        _channelProviders = channelProviders;
        _logger = logger;
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
        var total = await _db.MstParties.CountAsync(p => p.IsActive);
        var customers = await _db.MstPartyRoles.CountAsync(r => r.RoleType == "Customer" && r.IsActive);
        var suppliers = await _db.MstPartyRoles.CountAsync(r => r.RoleType == "Supplier" && r.IsActive);
        var vendors = await _db.MstPartyRoles.CountAsync(r => r.RoleType == "Vendor" && r.IsActive);

        return Ok(new { total, customers, suppliers, vendors });
    }

    // ═══════════════════════════════════════════════════════════════
    // LIST (with search, filter, pagination)
    // ═══════════════════════════════════════════════════════════════

    [HttpGet("list")]
    public async Task<IActionResult> GetList(
        [FromQuery] string? q,
        [FromQuery] string? role,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20)
    {
        var query = _db.MstParties
            .Include(p => p.MstPartyRoles)
            .AsQueryable();

        // Search
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(term) ||
                (p.Code != null && p.Code.ToLower().Contains(term)) ||
                (p.Email != null && p.Email.ToLower().Contains(term)) ||
                (p.Gstno != null && p.Gstno.ToLower().Contains(term)));
        }

        // Role filter
        if (!string.IsNullOrWhiteSpace(role))
        {
            query = query.Where(p => p.MstPartyRoles.Any(r => r.RoleType == role && r.IsActive));
        }

        // Status filter
        if (!string.IsNullOrWhiteSpace(status) && bool.TryParse(status, out var isActive))
        {
            query = query.Where(p => p.IsActive == isActive);
        }

        var total = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(total / (double)size);

        var items = await query
            .OrderByDescending(p => p.CreatedOn)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Code,
                p.Email,
                p.Mobile,
                GstNo = p.Gstno,
                p.IsActive,
                Roles = p.MstPartyRoles
                    .Where(r => r.IsActive)
                    .Select(r => r.RoleType)
                    .ToList()
            })
            .ToListAsync();

        return Ok(new { items, total, totalPages, page, size });
    }

    // ═══════════════════════════════════════════════════════════════
    // DETAIL (for drawer + edit)
    // ═══════════════════════════════════════════════════════════════

    [HttpGet("detail/{id}")]
    public async Task<IActionResult> GetDetail(int id)
    {
        var party = await _db.MstParties
            .Include(p => p.MstPartyRoles)
            .Include(p => p.MstPartyContacts)
            .Include(p => p.MstPartyAddresses).ThenInclude(a => a.State)
            .Include(p => p.MstPartyAddresses).ThenInclude(a => a.City)
            .Include(p => p.MstPartyBanks)
            .Include(p => p.MstCustomers)
            .Include(p => p.MstSuppliers)
            .Include(p => p.MstVendors)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (party == null)
            return NotFound(new { message = "Party not found." });

        // City name for primary address
        string? cityName = null;
        if (party.CityId.HasValue)
        {
            cityName = await _db.MstCities
                .Where(c => c.Id == party.CityId.Value)
                .Select(c => c.Name)
                .FirstOrDefaultAsync();
        }

        var customer = party.MstCustomers.FirstOrDefault(c => c.IsActive == true);
        var supplier = party.MstSuppliers.FirstOrDefault(s => s.IsActive == true);
        var vendor = party.MstVendors.FirstOrDefault(v => v.IsActive == true);

        return Ok(new
        {
            party.Id,
            party.Name,
            party.Code,
            party.Email,
            party.Mobile,
            GstNo = party.Gstno,
            PanNo = party.PanNo,
            party.IsActive,
            party.Address1,
            party.Address2,
            party.CityId,
            CityName = cityName,
            party.Pin,
            Roles = party.MstPartyRoles
                .Where(r => r.IsActive)
                .Select(r => r.RoleType)
                .ToList(),
            Contacts = party.MstPartyContacts
                .Where(c => c.IsActive)
                .Select(c => new
                {
                    c.ContactName,
                    c.Designation,
                    c.Email,
                    c.Mobile
                }).ToList(),
            Addresses = party.MstPartyAddresses
                .Where(a => a.IsActive == true)
                .Select(a => new
                {
                    a.AddressType,
                    a.AddressLabel,
                    a.IsDefault,
                    a.AddressLine1,
                    a.AddressLine2,
                    a.StateId,
                    StateName = a.State != null ? a.State.Name : null,
                    a.CityId,
                    CityName = a.City != null ? a.City.Name : null,
                    a.PostalCode,
                    a.Gstin,
                    a.ContactPersonName,
                    a.ContactPhone,
                    a.ContactEmail
                }).ToList(),
            Banks = party.MstPartyBanks.Select(b => new
            {
                b.BankName,
                b.BranchName,
                b.AccountNo,
                b.IfscCode,
                b.MicrNo
            }).ToList(),
            Customer = customer != null ? new
            {
                customer.CustomerType,
                customer.CustomerGroup,
                customer.PaymentTerms,
                customer.MaxCreditLimit,
                customer.Salesperson
            } : null,
            Supplier = supplier != null ? new
            {
                supplier.SupplierTypeId,
                supplier.TdsApplicable,
                supplier.TdsRate,
                supplier.PaymentCycleDays,
                supplier.Remarks
            } : null,
            Vendor = vendor != null ? new
            {
                vendor.VendorTypeId,
                vendor.ContractStartDate,
                vendor.ContractEndDate,
                vendor.ContractValue,
                vendor.ServiceArea,
                vendor.Remarks
            } : null
        });
    }

    // ═══════════════════════════════════════════════════════════════
    // SAVE (Create + Update — full workflow)
    // ═══════════════════════════════════════════════════════════════

    [HttpPost("save")]
    public async Task<IActionResult> Save([FromBody] PartySaveDto dto)
    {
        var errors = ValidateParty(dto);
        if (errors.Count > 0)
            return BadRequest(new { message = "Validation failed.", errors });

        using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            var isNew = dto.Id == 0;
            MstParty party;

            if (isNew)
            {
                // Auto-generate code if not provided
                if (string.IsNullOrWhiteSpace(dto.Code))
                {
                    var lastCode = await _db.MstParties
                        .Where(p => p.Code != null && p.Code.StartsWith("PTY-"))
                        .OrderByDescending(p => p.Code)
                        .Select(p => p.Code)
                        .FirstOrDefaultAsync();
                    var nextNum = 1;
                    if (lastCode != null && int.TryParse(lastCode.AsSpan(4), out var parsed))
                        nextNum = parsed + 1;
                    dto.Code = $"PTY-{nextNum:D5}";
                }

                // Check duplicate code
                if (await _db.MstParties.AnyAsync(p => p.Code == dto.Code))
                    return BadRequest(new { message = $"Party code '{dto.Code}' already exists.", errors = Array.Empty<string>() });

                party = new MstParty
                {
                    Name = dto.Name!,
                    Code = dto.Code,
                    Email = dto.Email,
                    Mobile = dto.Mobile,
                    Gstno = dto.GstNo,
                    PanNo = dto.PanNo,
                    Address1 = dto.Address1,
                    Address2 = dto.Address2,
                    CityId = dto.CityId,
                    Pin = dto.Pin,
                    IsActive = dto.IsActive,
                    CreatedOn = DateTime.Now
                };
                _db.MstParties.Add(party);
                await _db.SaveChangesAsync();

                // ── Auto-create user_master record for party login ──
                await CreatePartyUserAsync(party);
            }
            else
            {
                party = await _db.MstParties.FindAsync(dto.Id);
                if (party == null)
                    return NotFound(new { message = "Party not found." });

                party.Name = dto.Name!;
                party.Code = dto.Code;
                party.Email = dto.Email;
                party.Mobile = dto.Mobile;
                party.Gstno = dto.GstNo;
                party.PanNo = dto.PanNo;
                party.Address1 = dto.Address1;
                party.Address2 = dto.Address2;
                party.CityId = dto.CityId;
                party.Pin = dto.Pin;
                party.IsActive = dto.IsActive;
                await _db.SaveChangesAsync();
            }

            // ── Roles ──
            await SyncRoles(party.Id, dto.Roles ?? []);

            // ── Contacts ──
            await SyncContacts(party.Id, dto.Contacts ?? []);

            // ── Addresses ──
            await SyncAddresses(party.Id, dto.Addresses ?? []);

            // ── Banks ──
            await SyncBanks(party.Id, dto.Banks ?? []);

            // ── Customer config ──
            await SyncCustomer(party.Id, dto.Roles ?? [], dto.Customer);

            // ── Supplier config ──
            await SyncSupplier(party.Id, dto.Roles ?? [], dto.Supplier);

            // ── Vendor config ──
            await SyncVendor(party.Id, dto.Roles ?? [], dto.Vendor);

            await tx.CommitAsync();

            // ── User Activity Log ──
            await LogActivity(party, isNew);

            // ── Notifications ──
            var welcomeMailSent = false;
            var accountsDeptNotified = false;

            if (dto.SendWelcomeMail && !string.IsNullOrWhiteSpace(party.Email))
            {
                welcomeMailSent = await SendWelcomeMail(party);
            }

            if (dto.NotifyAccountsDept)
            {
                accountsDeptNotified = await NotifyAccountsDepartment(party, isNew);
            }

            return Ok(new
            {
                id = party.Id,
                name = party.Name,
                code = party.Code,
                roles = dto.Roles ?? [],
                welcomeMailSent,
                accountsDeptNotified
            });
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            _logger.LogError(ex, "Error saving party {PartyName}", dto.Name);
            var errorMsg = ex.InnerException?.Message ?? ex.Message;
            return StatusCode(500, new { message = "An error occurred while saving the party.", errors = new[] { errorMsg } });
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // TOGGLE STATUS
    // ═══════════════════════════════════════════════════════════════

    [HttpPost("toggle-status/{id}")]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var party = await _db.MstParties.FindAsync(id);
        if (party == null)
            return NotFound(new { message = "Party not found." });

        party.IsActive = !party.IsActive;
        await _db.SaveChangesAsync();

        // Activity log
        var user = CurrentUser;
        if (user != null)
        {
            await _activityService.LogActivityAsync(new ActivityLogEntry
            {
                UserId = user.UserId,
                UserCode = user.UserCode,
                UserName = user.Name,
                Module = "MAINTENANCE",
                SubModule = "PARTY",
                ActivityType = party.IsActive ? "ACTIVATE" : "DEACTIVATE",
                EntityType = "MstParty",
                EntityId = party.Id,
                EntityCode = party.Code,
                Title = $"Party {(party.IsActive ? "activated" : "deactivated")}: {party.Name}",
                CompanyId = user.CompanyId,
                LocationId = user.LocationId
            });
        }

        return Ok(new { success = true, isActive = party.IsActive });
    }

    // ═══════════════════════════════════════════════════════════════
    // LOOKUPS
    // ═══════════════════════════════════════════════════════════════

    [HttpGet("lookups")]
    public async Task<IActionResult> GetLookups()
    {
        var customerTypes = await _db.MstCustomerTypes
            .Where(t => t.IsActive == true)
            .Select(t => new { t.Id, t.Name })
            .ToListAsync();

        var customerGroups = await _db.MstCustomerGroups
            .Where(g => g.IsActive == true)
            .Select(g => new { g.Id, g.Name })
            .ToListAsync();

        var paymentTerms = await _db.MstCustomerPaymentTerms
            .Where(p => p.IsActive == true)
            .Select(p => new { p.Id, p.Name })
            .ToListAsync();

        var supplierTypes = await _db.MstSupplierTypes
            .Where(s => s.IsActive == true)
            .Select(s => new { s.Id, s.Name })
            .ToListAsync();

        var vendorTypes = await _db.MstVendorTypes
            .Where(v => v.IsActive == true)
            .Select(v => new { v.Id, v.Name })
            .ToListAsync();

        return Ok(new { customerTypes, customerGroups, paymentTerms, supplierTypes, vendorTypes });
    }

    [HttpGet("cities")]
    public async Task<IActionResult> SearchCities([FromQuery] string? q)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 1)
            return Ok(Array.Empty<object>());

        var term = q.Trim().ToLower();
        var results = await _db.MstCities
            .Include(c => c.State)
            .Where(c => c.IsActive && c.Name.ToLower().Contains(term))
            .OrderBy(c => c.Name)
            .Take(30)
            .Select(c => new
            {
                c.Id,
                c.Name,
                StateName = c.State.Name
            })
            .ToListAsync();

        return Ok(results);
    }

    [HttpGet("states")]
    public async Task<IActionResult> GetStates()
    {
        var states = await _db.MstStates
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .Select(s => new { s.Id, s.Name })
            .ToListAsync();
        return Ok(states);
    }

    [HttpGet("cities-by-state/{stateId}")]
    public async Task<IActionResult> GetCitiesByState(int stateId)
    {
        var cities = await _db.MstCities
            .Where(c => c.StateId == stateId && c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new { c.Id, c.Name })
            .ToListAsync();
        return Ok(cities);
    }

    // ═══════════════════════════════════════════════════════════════
    // PRIVATE — Sync child entities
    // ═══════════════════════════════════════════════════════════════

    private async Task ResetSequenceIfNeeded(string table, string keyColumn)
    {
        try
        {
            await _db.Database.ExecuteSqlRawAsync(
                $"SELECT setval(pg_get_serial_sequence('press_db.{table}', '{keyColumn}'), COALESCE((SELECT MAX(\"{keyColumn}\") FROM press_db.{table}), 0))");
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Sequence reset skipped for {Table}.{Column}", table, keyColumn);
        }
    }

    private async Task SyncRoles(int partyId, List<string> roles)
    {
        var existing = await _db.MstPartyRoles.Where(r => r.PartyId == partyId).ToListAsync();

        // Deactivate removed
        foreach (var ex in existing)
        {
            ex.IsActive = roles.Contains(ex.RoleType);
        }

        // Add new
        foreach (var role in roles)
        {
            if (!existing.Any(e => e.RoleType == role))
            {
                _db.MstPartyRoles.Add(new MstPartyRole
                {
                    PartyId = partyId,
                    RoleType = role,
                    IsActive = true
                });
            }
        }
        await _db.SaveChangesAsync();
    }

    private async Task SyncContacts(int partyId, List<ContactDto> contacts)
    {
        var existing = await _db.MstPartyContacts.Where(c => c.PartyId == partyId).ToListAsync();
        _db.MstPartyContacts.RemoveRange(existing);

        foreach (var c in contacts)
        {
            _db.MstPartyContacts.Add(new MstPartyContact
            {
                PartyId = partyId,
                ContactName = c.ContactName,
                Designation = c.Designation,
                Email = c.Email,
                Mobile = c.Mobile,
                IsActive = true
            });
        }
        await _db.SaveChangesAsync();
    }

    private async Task SyncAddresses(int partyId, List<AddressDto> addresses)
    {
        // Soft-delete existing (cannot hard-delete — referenced by TrnSalesInvoice FK)
        var existing = await _db.MstPartyAddresses
            .Where(a => a.PartyId == partyId && a.IsActive == true)
            .ToListAsync();
        foreach (var ex in existing) ex.IsActive = false;

        var user = CurrentUser;
        foreach (var a in addresses)
        {
            _db.MstPartyAddresses.Add(new MstPartyAddress
            {
                PartyId = partyId,
                AddressType = a.AddressType ?? "Billing",
                AddressLabel = a.AddressLabel,
                IsDefault = a.IsDefault,
                IsActive = true,
                AddressLine1 = a.AddressLine1 ?? string.Empty,
                AddressLine2 = a.AddressLine2,
                StateId = a.StateId,
                CityId = a.CityId,
                PostalCode = a.PostalCode,
                Gstin = a.Gstin,
                ContactPersonName = a.ContactPersonName,
                ContactPhone = a.ContactPhone,
                ContactEmail = a.ContactEmail,
                CreatedBy = user?.UserCode,
                CreatedOn = DateTime.Now
            });
        }
        await _db.SaveChangesAsync();
    }

    private async Task SyncBanks(int partyId, List<BankDto> banks)
    {
        var existing = await _db.MstPartyBanks.Where(b => b.PartyId == partyId).ToListAsync();
        _db.MstPartyBanks.RemoveRange(existing);

        foreach (var b in banks)
        {
            _db.MstPartyBanks.Add(new MstPartyBank
            {
                PartyId = partyId,
                BankName = b.BankName,
                BranchName = b.BranchName,
                AccountNo = b.AccountNo,
                IfscCode = b.IfscCode,
                MicrNo = b.MicrNo
            });
        }
        await _db.SaveChangesAsync();
    }

    private async Task SyncCustomer(int partyId, List<string> roles, CustomerConfigDto? config)
    {
        var existing = await _db.MstCustomers.FirstOrDefaultAsync(c => c.PartyId == partyId);

        if (roles.Contains("Customer"))
        {
            if (existing == null)
            {
                await ResetSequenceIfNeeded("mst_customer", "id");
                existing = new MstCustomer { PartyId = partyId, IsActive = true };
                _db.MstCustomers.Add(existing);
            }
            existing.IsActive = true;
            if (config != null)
            {
                existing.CustomerType = config.CustomerType;
                existing.CustomerGroup = config.CustomerGroup;
                existing.PaymentTerms = config.PaymentTerms;
                existing.MaxCreditLimit = config.MaxCreditLimit;
                existing.Salesperson = config.Salesperson;
            }
        }
        else if (existing != null)
        {
            existing.IsActive = false;
        }
        await _db.SaveChangesAsync();
    }

    private async Task SyncSupplier(int partyId, List<string> roles, SupplierConfigDto? config)
    {
        var existing = await _db.MstSuppliers.FirstOrDefaultAsync(s => s.PartyId == partyId);

        if (roles.Contains("Supplier"))
        {
            if (existing == null)
            {
                await ResetSequenceIfNeeded("mst_supplier", "supplier_id");
                existing = new MstSupplier { PartyId = partyId, IsActive = true, CreatedOn = DateTime.Now };
                _db.MstSuppliers.Add(existing);
            }
            existing.IsActive = true;
            if (config != null)
            {
                existing.SupplierTypeId = config.SupplierTypeId;
                existing.TdsApplicable = config.TdsApplicable;
                existing.TdsRate = config.TdsRate;
                existing.PaymentCycleDays = config.PaymentCycleDays;
                existing.Remarks = config.Remarks;
            }
            existing.ModifiedOn = DateTime.Now;
        }
        else if (existing != null)
        {
            existing.IsActive = false;
            existing.ModifiedOn = DateTime.Now;
        }
        await _db.SaveChangesAsync();
    }

    private async Task SyncVendor(int partyId, List<string> roles, VendorConfigDto? config)
    {
        var existing = await _db.MstVendors.FirstOrDefaultAsync(v => v.PartyId == partyId);

        if (roles.Contains("Vendor"))
        {
            if (existing == null)
            {
                await ResetSequenceIfNeeded("mst_vendor", "vendor_id");
                existing = new MstVendor { PartyId = partyId, IsActive = true, CreatedOn = DateTime.Now };
                _db.MstVendors.Add(existing);
            }
            existing.IsActive = true;
            if (config != null)
            {
                existing.VendorTypeId = config.VendorTypeId;
                existing.ContractStartDate = config.ContractStartDate != null ? DateOnly.Parse(config.ContractStartDate) : null;
                existing.ContractEndDate = config.ContractEndDate != null ? DateOnly.Parse(config.ContractEndDate) : null;
                existing.ContractValue = config.ContractValue;
                existing.ServiceArea = config.ServiceArea;
                existing.Remarks = config.Remarks;
            }
            existing.ModifiedOn = DateTime.Now;
        }
        else if (existing != null)
        {
            existing.IsActive = false;
            existing.ModifiedOn = DateTime.Now;
        }
        await _db.SaveChangesAsync();
    }

    // ═══════════════════════════════════════════════════════════════
    // PRIVATE — Validation
    // ═══════════════════════════════════════════════════════════════

    private static List<string> ValidateParty(PartySaveDto dto)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(dto.Name))
            errors.Add("Party name is required.");

        if (dto.Roles == null || dto.Roles.Count == 0)
            errors.Add("At least one role must be assigned.");

        if (!string.IsNullOrWhiteSpace(dto.Email) &&
            !System.Text.RegularExpressions.Regex.IsMatch(dto.Email, @"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
            errors.Add("Invalid email address format.");

        if (!string.IsNullOrWhiteSpace(dto.GstNo) &&
            !System.Text.RegularExpressions.Regex.IsMatch(dto.GstNo, @"^\d{2}[A-Z]{5}\d{4}[A-Z]{1}[A-Z\d]{1}[Z]{1}[A-Z\d]{1}$"))
            errors.Add("Invalid GST number format.");

        if (!string.IsNullOrWhiteSpace(dto.PanNo) &&
            !System.Text.RegularExpressions.Regex.IsMatch(dto.PanNo, @"^[A-Z]{5}\d{4}[A-Z]$"))
            errors.Add("Invalid PAN number format.");

        if (dto.Mobile.HasValue && (dto.Mobile < 1000000000 || dto.Mobile > 9999999999999))
            errors.Add("Invalid mobile number.");

        return errors;
    }

    // ═══════════════════════════════════════════════════════════════
    // PRIVATE — Activity Logging
    // ═══════════════════════════════════════════════════════════════

    private async Task LogActivity(MstParty party, bool isNew)
    {
        var user = CurrentUser;
        if (user == null) return;

        try
        {
            await _activityService.LogActivityAsync(new ActivityLogEntry
            {
                UserId = user.UserId,
                UserCode = user.UserCode,
                UserName = user.Name,
                Module = "MAINTENANCE",
                SubModule = "PARTY",
                ActivityType = isNew ? "CREATE" : "UPDATE",
                ActivityCategory = "DATA",
                EntityType = "MstParty",
                EntityId = party.Id,
                EntityCode = party.Code,
                Title = $"Party {(isNew ? "created" : "updated")}: {party.Name}",
                Description = $"Party '{party.Name}' (Code: {party.Code}) was {(isNew ? "created" : "updated")}.",
                NewValues = JsonSerializer.Serialize(new
                {
                    party.Name,
                    party.Code,
                    party.Email,
                    party.Mobile,
                    party.IsActive
                }),
                CompanyId = user.CompanyId,
                LocationId = user.LocationId,
                Severity = "INFO"
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to log activity for party {PartyId}", party.Id);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // PRIVATE — Notifications
    // ═══════════════════════════════════════════════════════════════

    private async Task<bool> SendWelcomeMail(MstParty party)
    {
        try
        {
            var emailProvider = _channelProviders.FirstOrDefault(p => p.Channel == NotificationChannel.Email);
            if (emailProvider == null) return false;

            var roles = await _db.MstPartyRoles
                .Where(r => r.PartyId == party.Id && r.IsActive)
                .Select(r => r.RoleType)
                .ToListAsync();

            var rolesText = string.Join(", ", roles);

            var body = $@"
<div style='font-family:Segoe UI,Arial,sans-serif; max-width:600px; margin:0 auto;'>
    <div style='background:linear-gradient(135deg,#6366f1,#8b5cf6); padding:24px 32px; border-radius:12px 12px 0 0;'>
        <h1 style='color:#fff; margin:0; font-size:22px;'>Welcome to MinePress ERP!</h1>
        <p style='color:rgba(255,255,255,.8); margin:6px 0 0; font-size:14px;'>Your account has been created</p>
    </div>
    <div style='background:#fff; padding:28px 32px; border:1px solid #e5e7eb; border-top:none; border-radius:0 0 12px 12px;'>
        <p style='color:#374151; font-size:15px; margin:0 0 16px;'>Dear <strong>{System.Net.WebUtility.HtmlEncode(party.Name)}</strong>,</p>
        <p style='color:#6b7280; font-size:14px; line-height:1.6; margin:0 0 16px;'>
            We are pleased to inform you that your party profile has been successfully registered in our ERP system with the following details:
        </p>
        <table style='width:100%; font-size:14px; color:#374151; border-collapse:collapse; margin:0 0 20px;'>
            <tr><td style='padding:8px 12px; background:#f9fafb; border:1px solid #e5e7eb; font-weight:600; width:140px;'>Party Code</td><td style='padding:8px 12px; border:1px solid #e5e7eb;'>{System.Net.WebUtility.HtmlEncode(party.Code ?? "—")}</td></tr>
            <tr><td style='padding:8px 12px; background:#f9fafb; border:1px solid #e5e7eb; font-weight:600;'>Roles</td><td style='padding:8px 12px; border:1px solid #e5e7eb;'>{System.Net.WebUtility.HtmlEncode(rolesText)}</td></tr>
            <tr><td style='padding:8px 12px; background:#f9fafb; border:1px solid #e5e7eb; font-weight:600;'>Email</td><td style='padding:8px 12px; border:1px solid #e5e7eb;'>{System.Net.WebUtility.HtmlEncode(party.Email ?? "—")}</td></tr>
            {(party.Gstno != null ? $"<tr><td style='padding:8px 12px; background:#f9fafb; border:1px solid #e5e7eb; font-weight:600;'>GSTIN</td><td style='padding:8px 12px; border:1px solid #e5e7eb;'>{System.Net.WebUtility.HtmlEncode(party.Gstno)}</td></tr>" : "")}
        </table>
        <p style='color:#6b7280; font-size:13px; line-height:1.6;'>
            If you have any questions, please feel free to reach out to us. We look forward to a great business relationship.
        </p>
        <p style='color:#6b7280; font-size:13px; margin:20px 0 0;'>Regards,<br/><strong>MinePress ERP Team</strong></p>
    </div>
</div>";

            var result = await emailProvider.SendAsync(new NotificationRequest
            {
                Recipient = party.Email!,
                Subject = $"Welcome to MinePress ERP — {party.Name}",
                Body = body,
                Channel = NotificationChannel.Email,
                Module = "MAINTENANCE",
                EventType = "PARTY_WELCOME"
            });

            return result.IsSuccess;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send welcome mail to party {PartyId}", party.Id);
            return false;
        }
    }

    private async Task<bool> NotifyAccountsDepartment(MstParty party, bool isNew)
    {
        try
        {
            var emailProvider = _channelProviders.FirstOrDefault(p => p.Channel == NotificationChannel.Email);
            if (emailProvider == null) return false;

            // Find accounts department email(s) — look for users in ACCOUNTS dept
            var accountEmails = await _db.MstUsers
                .Include(u => u.Department)
                .Where(u => u.Isactive == true &&
                       u.Department != null &&
                       (u.Department.DeptName.Contains("Account") || u.Department.DeptCode == "ACC"))
                .Select(u => u.Emailid)
                .Where(e => e != null)
                .Distinct()
                .Take(10)
                .ToListAsync();

            if (accountEmails.Count == 0) return false;

            var roles = await _db.MstPartyRoles
                .Where(r => r.PartyId == party.Id && r.IsActive)
                .Select(r => r.RoleType)
                .ToListAsync();

            var rolesText = string.Join(", ", roles);
            var action = isNew ? "New Party Created" : "Party Updated";

            var body = $@"
<div style='font-family:Segoe UI,Arial,sans-serif; max-width:600px; margin:0 auto;'>
    <div style='background:linear-gradient(135deg,#0d6efd,#0b5ed7); padding:20px 28px; border-radius:10px 10px 0 0;'>
        <h2 style='color:#fff; margin:0; font-size:18px;'><span style='margin-right:8px;'>📋</span>{action}</h2>
    </div>
    <div style='background:#fff; padding:24px 28px; border:1px solid #e5e7eb; border-top:none; border-radius:0 0 10px 10px;'>
        <table style='width:100%; font-size:13px; color:#374151; border-collapse:collapse; margin:0 0 16px;'>
            <tr><td style='padding:6px 10px; background:#f9fafb; border:1px solid #e5e7eb; font-weight:600; width:130px;'>Party Name</td><td style='padding:6px 10px; border:1px solid #e5e7eb;'>{System.Net.WebUtility.HtmlEncode(party.Name)}</td></tr>
            <tr><td style='padding:6px 10px; background:#f9fafb; border:1px solid #e5e7eb; font-weight:600;'>Code</td><td style='padding:6px 10px; border:1px solid #e5e7eb;'>{System.Net.WebUtility.HtmlEncode(party.Code ?? "—")}</td></tr>
            <tr><td style='padding:6px 10px; background:#f9fafb; border:1px solid #e5e7eb; font-weight:600;'>Roles</td><td style='padding:6px 10px; border:1px solid #e5e7eb;'>{System.Net.WebUtility.HtmlEncode(rolesText)}</td></tr>
            <tr><td style='padding:6px 10px; background:#f9fafb; border:1px solid #e5e7eb; font-weight:600;'>GST No</td><td style='padding:6px 10px; border:1px solid #e5e7eb;'>{System.Net.WebUtility.HtmlEncode(party.Gstno ?? "—")}</td></tr>
            <tr><td style='padding:6px 10px; background:#f9fafb; border:1px solid #e5e7eb; font-weight:600;'>Email</td><td style='padding:6px 10px; border:1px solid #e5e7eb;'>{System.Net.WebUtility.HtmlEncode(party.Email ?? "—")}</td></tr>
            <tr><td style='padding:6px 10px; background:#f9fafb; border:1px solid #e5e7eb; font-weight:600;'>Mobile</td><td style='padding:6px 10px; border:1px solid #e5e7eb;'>{party.Mobile?.ToString() ?? "—"}</td></tr>
        </table>
        <p style='color:#9ca3af; font-size:12px; margin:0;'>This is an automated notification from MinePress ERP.</p>
    </div>
</div>";

            var sent = false;
            foreach (var email in accountEmails)
            {
                if (string.IsNullOrWhiteSpace(email)) continue;
                var result = await emailProvider.SendAsync(new NotificationRequest
                {
                    Recipient = email!,
                    Subject = $"[MinePress] {action}: {party.Name} ({party.Code})",
                    Body = body,
                    Channel = NotificationChannel.Email,
                    Module = "MAINTENANCE",
                    EventType = isNew ? "PARTY_CREATED" : "PARTY_UPDATED"
                });
                if (result.IsSuccess) sent = true;
            }

            return sent;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to notify accounts dept for party {PartyId}", party.Id);
            return false;
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // AUTO-CREATE USER FOR PARTY LOGIN
    // ═══════════════════════════════════════════════════════════════

    private async Task CreatePartyUserAsync(MstParty party)
    {
        // Check if a user already exists for this party
        var existingUser = await _db.MstUsers
            .FirstOrDefaultAsync(u => u.UserType == "PARTY" && u.RefId == party.Id);
        if (existingUser != null) return;

        // Generate usercode from party code (e.g., PTY-00001 → PTY00001)
        var userCode = ( $"P{party.Id}").Replace("-", "").ToUpper();

        // Default password = party code (SHA-256 hashed)
        var defaultPassword = party.Code ?? $"Party@{party.Id}";
        var passwordHash = ComputeSha256(defaultPassword);

        var partyUser = new MstUser
        {
            Usercode = userCode,
            Username = party.Name,
            Passwordhash = passwordHash,
            Name = party.Name,
            Emailid = party.Email,
            Mobileno = party.Mobile?.ToString(),
            Locationid = 1,
            Departmentid = 9999,// department for party users
            Designationid = 1017,
            UserType = "PARTY",
            RefId = party.Id,
            UserCategory = "EXTERNAL",
            Isactive = true,
            Islocked = false,
            Isdeleted = false,
            Issystemadmin = false,
            Iswebaccessallowed = true,
            Createdby = CurrentUser?.UserCode ?? "SYSTEM",
            Createdat = DateTime.UtcNow
        };

        _db.MstUsers.Add(partyUser);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Auto-created party user {UserCode} for party {PartyId} ({PartyName})",
            userCode, party.Id, party.Name);
    }

    private static string ComputeSha256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        var sb = new StringBuilder();
        foreach (var b in bytes)
            sb.Append(b.ToString("x2"));
        return sb.ToString();
    }

    // ═══════════════════════════════════════════════════════════════
    // DTOs
    // ═══════════════════════════════════════════════════════════════

    public class PartySaveDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? Email { get; set; }
        public long? Mobile { get; set; }
        public bool IsActive { get; set; } = true;
        public string? GstNo { get; set; }
        public string? PanNo { get; set; }
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
        public int? CityId { get; set; }
        public string? Pin { get; set; }
        public List<string>? Roles { get; set; }
        public List<ContactDto>? Contacts { get; set; }
        public List<AddressDto>? Addresses { get; set; }
        public List<BankDto>? Banks { get; set; }
        public bool SendWelcomeMail { get; set; }
        public bool NotifyAccountsDept { get; set; }
        public CustomerConfigDto? Customer { get; set; }
        public SupplierConfigDto? Supplier { get; set; }
        public VendorConfigDto? Vendor { get; set; }
    }

    public class ContactDto
    {
        public string? ContactName { get; set; }
        public string? Designation { get; set; }
        public string? Email { get; set; }
        public long? Mobile { get; set; }
    }

    public class AddressDto
    {
        public string? AddressType { get; set; }
        public string? AddressLabel { get; set; }
        public bool IsDefault { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public int? StateId { get; set; }
        public int? CityId { get; set; }
        public string? PostalCode { get; set; }
        public string? Gstin { get; set; }
        public string? ContactPersonName { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
    }

    public class BankDto
    {
        public string? BankName { get; set; }
        public string? BranchName { get; set; }
        public string? AccountNo { get; set; }
        public string? IfscCode { get; set; }
        public string? MicrNo { get; set; }
    }

    public class CustomerConfigDto
    {
        public int? CustomerType { get; set; }
        public int? CustomerGroup { get; set; }
        public int? PaymentTerms { get; set; }
        public decimal? MaxCreditLimit { get; set; }
        public string? Salesperson { get; set; }
    }

    public class SupplierConfigDto
    {
        public int? SupplierTypeId { get; set; }
        public bool? TdsApplicable { get; set; }
        public decimal? TdsRate { get; set; }
        public int? PaymentCycleDays { get; set; }
        public string? Remarks { get; set; }
    }

    public class VendorConfigDto
    {
        public int? VendorTypeId { get; set; }
        public string? ContractStartDate { get; set; }
        public string? ContractEndDate { get; set; }
        public decimal? ContractValue { get; set; }
        public string? ServiceArea { get; set; }
        public string? Remarks { get; set; }
    }
}
