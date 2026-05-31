using erp.minepress.bff.service.Interfaces;
using erp.minepress.persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace erp.minepress.bff.service.Services;

public class AiDataService : IAiDataService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<AiDataService> _logger;

    public AiDataService(ApplicationDbContext db, ILogger<AiDataService> logger)
    {
        _db = db;
        _logger = logger;
    }

    // ══════════════════════════ JOBS ══════════════════════════

    public async Task<AiJobDto?> GetJobByNoAsync(string jobNo, CancellationToken ct = default)
    {
        var job = await _db.TrnJobs
            .Include(j => j.Party)
            .Include(j => j.JobType)
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.JobNo == jobNo, ct);

        return job is null ? null : MapJob(job);
    }

    public async Task<IReadOnlyList<AiJobDto>> GetJobsByStatusAsync(string? statusCode, int limit = 20, CancellationToken ct = default)
    {
        var query = _db.TrnJobs
            .Include(j => j.Party)
            .Include(j => j.JobType)
            .AsNoTracking()
            .OrderByDescending(j => j.CreatedOn);

        var filtered = string.IsNullOrWhiteSpace(statusCode)
            ? query
            : query.Where(j => j.StatusCode != null && j.StatusCode.ToLower() == statusCode.ToLower());

        var jobs = await filtered.Take(limit).ToListAsync(ct);
        return jobs.Select(MapJob).ToList();
    }

    public async Task<AiJobDto?> CreateJobAsync(AiCreateJobRequest request, CancellationToken ct = default)
    {
        var jobNo = await GenerateDocumentNumberAsync("JOB", ct);

        var job = new persistence.Models.TrnJob
        {
            JobNo = jobNo,
            JobDate = DateOnly.FromDateTime(DateTime.Today),
            CompanyId = request.CompanyId,
            ProductName = request.ProductName,
            ProductDescription = request.ProductDescription ?? $"{request.ColorMode ?? "Color"} {request.PaperSize ?? "A4"} - {request.ProductName}",
            Quantity = request.Quantity,
            StatusCode = "created",
            Priority = request.Priority ?? "Normal",
            ProgressPercent = 0,
            CurrentStage = "Job Created",
            CreatedBy = request.CreatedByUserId,
            CreatedOn = DateTime.UtcNow
        };

        if (!string.IsNullOrEmpty(request.CustomerName))
        {
            var party = await _db.MstParties
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Name.ToLower().Contains(request.CustomerName.ToLower()) && p.IsActive, ct);

            if (party is not null)
                job.PartyId = party.Id;
        }

        if (!string.IsNullOrEmpty(request.JobType))
        {
            var jt = await _db.MstJobTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Jobtypename.ToLower().Contains(request.JobType.ToLower()), ct);

            if (jt is not null)
                job.JobTypeId = jt.Jobtypeid;
        }

        _db.TrnJobs.Add(job);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("AI created job {JobNo} for customer {Customer}", jobNo, request.CustomerName);
        return await GetJobByNoAsync(jobNo, ct);
    }

    public async Task<AiJobDto?> UpdateJobAsync(string jobNo, string? statusCode, int? quantity, string? priority, CancellationToken ct = default)
    {
        var job = await _db.TrnJobs.FirstOrDefaultAsync(j => j.JobNo == jobNo, ct);
        if (job is null) return null;

        if (!string.IsNullOrEmpty(statusCode))
            job.StatusCode = statusCode;

        if (quantity.HasValue && quantity > 0)
            job.Quantity = quantity.Value;

        if (!string.IsNullOrEmpty(priority))
            job.Priority = priority;

        job.ModifiedOn = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("AI updated job {JobNo}: status={Status}", jobNo, statusCode);

        return await GetJobByNoAsync(jobNo, ct);
    }

    // ══════════════════════════ MACHINES ══════════════════════════

    public async Task<IReadOnlyList<AiMachineDto>> GetAvailableMachinesAsync(string? machineType = null, CancellationToken ct = default)
    {
        var query = _db.MstMachines
            .Where(m => m.IsActive == true)
            .AsNoTracking();

        if (!string.IsNullOrEmpty(machineType))
        {
            query = query.Where(m =>
                (m.MachineType != null && m.MachineType.ToLower().Contains(machineType.ToLower())) ||
                (m.MachineCategory != null && m.MachineCategory.ToLower().Contains(machineType.ToLower())));
        }

        var machines = await query.OrderBy(m => m.MachineName).ToListAsync(ct);

        var machineIds = machines.Select(m => m.MachineId).ToList();
        var activeCounts = await _db.TrnJobMachineAllocations
            .Where(a => machineIds.Contains(a.MachineId) && a.AllocationStatus == "active")
            .GroupBy(a => a.MachineId)
            .Select(g => new { MachineId = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var countMap = activeCounts.ToDictionary(x => x.MachineId, x => x.Count);

        return machines.Select(m => new AiMachineDto
        {
            MachineId = m.MachineId,
            MachineCode = m.MachineCode,
            MachineName = m.MachineName,
            MachineType = m.MachineType,
            MachineCategory = m.MachineCategory,
            MaxColors = m.MaxColors,
            MaxSpeed = m.MaxSpeed,
            HourlyRunningCost = m.HourlyRunningCost,
            IsActive = m.IsActive,
            ActiveAllocations = countMap.GetValueOrDefault(m.MachineId, 0)
        }).ToList();
    }

    public async Task<AiMachineAllocationDto?> AllocateMachineAsync(string jobNo, long? machineId, string? processCode, CancellationToken ct = default)
    {
        var job = await _db.TrnJobs.AsNoTracking().FirstOrDefaultAsync(j => j.JobNo == jobNo, ct);
        if (job is null) return null;

        long resolvedMachineId;
        if (machineId.HasValue)
        {
            resolvedMachineId = machineId.Value;
        }
        else
        {
            var bestMachine = await _db.MstMachines
                .Where(m => m.IsActive == true)
                .OrderBy(m => m.AutoSelectPriority)
                .FirstOrDefaultAsync(ct);

            if (bestMachine is null)
                return null;

            resolvedMachineId = bestMachine.MachineId;
        }

        var machine = await _db.MstMachines.AsNoTracking()
            .FirstOrDefaultAsync(m => m.MachineId == resolvedMachineId, ct);

        if (machine is null) return null;

        var allocation = new persistence.Models.TrnJobMachineAllocation
        {
            JobId = job.JobId,
            JobNo = job.JobNo,
            MachineId = resolvedMachineId,
            MachineCode = machine.MachineCode,
            MachineName = machine.MachineName,
            ProcessCode = processCode ?? "PRINTING",
            ProcessName = processCode ?? "Printing",
            PlannedQuantity = job.Quantity,
            CompletedQuantity = 0,
            AllocationStatus = "active",
            PlannedStartTime = DateTime.UtcNow,
            CreatedBy = "AI-Agent",
            CreatedOn = DateTime.UtcNow,
            IsActive = true
        };

        _db.TrnJobMachineAllocations.Add(allocation);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("AI allocated machine {MachineCode} to job {JobNo}", machine.MachineCode, jobNo);

        return MapAllocation(allocation);
    }

    public async Task<IReadOnlyList<AiMachineAllocationDto>> GetMachineAllocationsForJobAsync(string jobNo, CancellationToken ct = default)
    {
        var allocations = await _db.TrnJobMachineAllocations
            .Where(a => a.JobNo == jobNo)
            .AsNoTracking()
            .OrderByDescending(a => a.CreatedOn)
            .ToListAsync(ct);

        return allocations.Select(MapAllocation).ToList();
    }

    // ══════════════════════════ BILLING ══════════════════════════

    public async Task<AiInvoiceDto?> GetInvoiceByJobNoAsync(string jobNo, CancellationToken ct = default)
    {
        var job = await _db.TrnJobs.AsNoTracking().FirstOrDefaultAsync(j => j.JobNo == jobNo, ct);
        if (job is null) return null;

        var invoice = await _db.TrnSalesInvoices
            .Include(i => i.Party)
            .AsNoTracking()
            .Where(i => i.JobId == job.JobId)
            .OrderByDescending(i => i.CreatedOn)
            .FirstOrDefaultAsync(ct);

        return invoice is null ? null : MapInvoice(invoice, jobNo);
    }

    public async Task<IReadOnlyList<AiInvoiceDto>> GetRecentInvoicesAsync(int limit = 20, CancellationToken ct = default)
    {
        var invoices = await _db.TrnSalesInvoices
            .Include(i => i.Party)
            .AsNoTracking()
            .OrderByDescending(i => i.CreatedOn)
            .Take(limit)
            .ToListAsync(ct);

        return invoices.Select(i => MapInvoice(i, null)).ToList();
    }

    // ══════════════════════════ DELIVERY ══════════════════════════

    public async Task<AiGatePassDto?> CreateGatePassAsync(string jobNo, string? vehicleNo, string? driverName, string? driverContact, CancellationToken ct = default)
    {
        var job = await _db.TrnJobs.AsNoTracking().FirstOrDefaultAsync(j => j.JobNo == jobNo, ct);
        if (job is null) return null;

        var gpNo = await GenerateDocumentNumberAsync("GP", ct);

        var gp = new persistence.Models.TrnGatePass
        {
            GatePassNo = gpNo,
            GatePassDate = DateOnly.FromDateTime(DateTime.Today),
            GatepassType = "Outward",
            CompanyId = job.CompanyId,
            ReferenceType = "Job",
            ReferenceNo = jobNo,
            VehicleNo = vehicleNo,
            DriverName = driverName,
            DriverContact = driverContact,
            Purpose = $"Delivery for job {jobNo}",
            TotalQuantity = job.Quantity,
            Status = "Created",
            CreatedBy = job.CreatedBy,
            CreatedOn = DateTime.UtcNow
        };

        _db.TrnGatePasses.Add(gp);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("AI created gate pass {GpNo} for job {JobNo}", gpNo, jobNo);

        return MapGatePass(gp);
    }

    public async Task<IReadOnlyList<AiGatePassDto>> GetGatePassesByJobNoAsync(string jobNo, CancellationToken ct = default)
    {
        var passes = await _db.TrnGatePasses
            .Where(g => g.ReferenceNo == jobNo)
            .AsNoTracking()
            .OrderByDescending(g => g.CreatedOn)
            .ToListAsync(ct);

        return passes.Select(MapGatePass).ToList();
    }

    // ══════════════════════════ VENDOR ══════════════════════════

    public async Task<IReadOnlyList<AiVendorOutsourceDto>> GetOutsourcesByJobNoAsync(string jobNo, CancellationToken ct = default)
    {
        var job = await _db.TrnJobs.AsNoTracking().FirstOrDefaultAsync(j => j.JobNo == jobNo, ct);
        if (job is null) return [];

        var outsources = await _db.TrnJobOutsources
            .Where(o => o.JobId == job.JobId)
            .AsNoTracking()
            .OrderByDescending(o => o.CreatedOn)
            .ToListAsync(ct);

        return outsources.Select(o => MapOutsource(o, jobNo)).ToList();
    }

    public async Task<AiVendorOutsourceDto?> CreateVendorJobAsync(string jobNo, long vendorId, string? processType, decimal? quantity, CancellationToken ct = default)
    {
        var job = await _db.TrnJobs.AsNoTracking().FirstOrDefaultAsync(j => j.JobNo == jobNo, ct);
        if (job is null) return null;

        var osNo = await GenerateDocumentNumberAsync("OS", ct);

        var outsource = new persistence.Models.TrnJobOutsource
        {
            OutsourceNo = osNo,
            OutsourceDate = DateOnly.FromDateTime(DateTime.Today),
            JobId = job.JobId,
            VendorId = vendorId,
            ProcessType = processType ?? "General",
            TotalQuantity = quantity ?? job.Quantity,
            Status = "Created",
            ExpectedDeliveryDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7)),
            CreatedBy = job.CreatedBy,
            CreatedOn = DateTime.UtcNow
        };

        _db.TrnJobOutsources.Add(outsource);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("AI created vendor job {OsNo} for job {JobNo}", osNo, jobNo);

        return MapOutsource(outsource, jobNo);
    }

    // ══════════════════════════ REPORTING ══════════════════════════

    public async Task<AiReportSummaryDto> GetReportSummaryAsync(DateOnly? from, DateOnly? to, CancellationToken ct = default)
    {
        var fromDate = from ?? DateOnly.FromDateTime(DateTime.Today.AddDays(-30));
        var toDate = to ?? DateOnly.FromDateTime(DateTime.Today);

        var jobs = await _db.TrnJobs
            .Where(j => j.JobDate >= fromDate && j.JobDate <= toDate)
            .AsNoTracking()
            .ToListAsync(ct);

        var invoices = await _db.TrnSalesInvoices
            .Where(i => i.InvoiceDate >= fromDate && i.InvoiceDate <= toDate)
            .AsNoTracking()
            .ToListAsync(ct);

        var gatePasses = await _db.TrnGatePasses
            .Where(g => g.GatePassDate >= fromDate && g.GatePassDate <= toDate)
            .AsNoTracking()
            .CountAsync(ct);

        var allocations = await _db.TrnJobMachineAllocations
            .Where(a => a.CreatedOn >= fromDate.ToDateTime(TimeOnly.MinValue) && a.CreatedOn <= toDate.ToDateTime(TimeOnly.MaxValue))
            .AsNoTracking()
            .CountAsync(ct);

        var vendorJobs = await _db.TrnJobOutsources
            .Where(o => o.OutsourceDate >= fromDate && o.OutsourceDate <= toDate)
            .AsNoTracking()
            .CountAsync(ct);

        var statusGroups = jobs
            .GroupBy(j => j.StatusCode ?? "unknown")
            .Select(g => new AiJobStatusCount { StatusCode = g.Key, Count = g.Count() })
            .ToList();

        return new AiReportSummaryDto
        {
            TotalJobs = jobs.Count,
            ActiveJobs = jobs.Count(j => j.StatusCode is "created" or "in_progress" or "printing"),
            CompletedJobs = jobs.Count(j => j.StatusCode is "completed" or "delivered"),
            CancelledJobs = jobs.Count(j => j.StatusCode is "cancelled"),
            TotalRevenue = invoices.Sum(i => i.GrandTotal ?? 0),
            TotalOutstanding = invoices.Sum(i => i.BalanceAmount ?? 0),
            TotalInvoices = invoices.Count,
            TotalGatePasses = gatePasses,
            TotalMachineAllocations = allocations,
            TotalVendorJobs = vendorJobs,
            FromDate = fromDate,
            ToDate = toDate,
            JobsByStatus = statusGroups
        };
    }

    // ══════════════════════════ CUSTOMER / PARTY ══════════════════════════

    public async Task<IReadOnlyList<AiCustomerDto>> GetAllCustomersAsync(int limit = 50, CancellationToken ct = default)
    {
        var parties = await _db.MstParties
            .Where(p => p.IsActive)
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .Take(limit)
            .ToListAsync(ct);

        var partyIds = parties.Select(p => p.Id).ToList();
        var customers = await _db.MstCustomers
            .Include(c => c.CustomerTypeNavigation)
            .Include(c => c.CustomerGroupNavigation)
            .Where(c => partyIds.Contains(c.PartyId))
            .AsNoTracking()
            .ToListAsync(ct);

        var custMap = customers.ToDictionary(c => c.PartyId);

        return parties.Select(p => MapCustomer(p, custMap.GetValueOrDefault(p.Id))).ToList();
    }

    public async Task<AiCustomerDto?> GetCustomerByIdAsync(int partyId, CancellationToken ct = default)
    {
        var party = await _db.MstParties.AsNoTracking().FirstOrDefaultAsync(p => p.Id == partyId, ct);
        if (party is null) return null;

        var customer = await _db.MstCustomers
            .Include(c => c.CustomerTypeNavigation)
            .Include(c => c.CustomerGroupNavigation)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.PartyId == partyId, ct);

        return MapCustomer(party, customer);
    }

    public async Task<IReadOnlyList<AiCustomerDto>> SearchCustomersAsync(string keyword, CancellationToken ct = default)
    {
        var kw = keyword.ToLower();
        var parties = await _db.MstParties
            .Where(p => p.Name.ToLower().Contains(kw) ||
                        (p.Code != null && p.Code.ToLower().Contains(kw)) ||
                        (p.Gstno != null && p.Gstno.ToLower().Contains(kw)) ||
                        (p.Email != null && p.Email.ToLower().Contains(kw)))
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .Take(20)
            .ToListAsync(ct);

        var partyIds = parties.Select(p => p.Id).ToList();
        var customers = await _db.MstCustomers
            .Include(c => c.CustomerTypeNavigation)
            .Include(c => c.CustomerGroupNavigation)
            .Where(c => partyIds.Contains(c.PartyId))
            .AsNoTracking()
            .ToListAsync(ct);

        var custMap = customers.ToDictionary(c => c.PartyId);
        return parties.Select(p => MapCustomer(p, custMap.GetValueOrDefault(p.Id))).ToList();
    }

    public async Task<IReadOnlyList<AiJobDto>> GetCustomerJobsAsync(int partyId, int limit = 20, CancellationToken ct = default)
    {
        var jobs = await _db.TrnJobs
            .Include(j => j.Party)
            .Include(j => j.JobType)
            .Where(j => j.PartyId == partyId)
            .AsNoTracking()
            .OrderByDescending(j => j.CreatedOn)
            .Take(limit)
            .ToListAsync(ct);

        return jobs.Select(MapJob).ToList();
    }

    public async Task<IReadOnlyList<AiInvoiceDto>> GetCustomerInvoicesAsync(int partyId, int limit = 20, CancellationToken ct = default)
    {
        var invoices = await _db.TrnSalesInvoices
            .Include(i => i.Party)
            .Where(i => i.PartyId == partyId)
            .AsNoTracking()
            .OrderByDescending(i => i.CreatedOn)
            .Take(limit)
            .ToListAsync(ct);

        return invoices.Select(i => MapInvoice(i, null)).ToList();
    }

    public async Task<AiCustomerSummaryDto> GetCustomerSummaryAsync(int partyId, CancellationToken ct = default)
    {
        var party = await _db.MstParties.AsNoTracking().FirstOrDefaultAsync(p => p.Id == partyId, ct);
        var jobs = await _db.TrnJobs.Where(j => j.PartyId == partyId).AsNoTracking().ToListAsync(ct);
        var invoices = await _db.TrnSalesInvoices.Where(i => i.PartyId == partyId).AsNoTracking().ToListAsync(ct);
        var enquiries = await _db.TrnEnquiries.Where(e => e.PartyId == partyId).AsNoTracking().CountAsync(ct);
        var quotations = await _db.TrnQuotations.Where(q => q.PartyId == partyId).AsNoTracking().CountAsync(ct);

        return new AiCustomerSummaryDto
        {
            PartyId = partyId,
            CustomerName = party?.Name,
            TotalJobs = jobs.Count,
            ActiveJobs = jobs.Count(j => j.StatusCode is "created" or "in_progress" or "printing"),
            CompletedJobs = jobs.Count(j => j.StatusCode is "completed" or "delivered"),
            TotalInvoices = invoices.Count,
            TotalRevenue = invoices.Sum(i => i.GrandTotal ?? 0),
            TotalOutstanding = invoices.Sum(i => i.BalanceAmount ?? 0),
            TotalEnquiries = enquiries,
            TotalQuotations = quotations
        };
    }

    // ══════════════════════════ EMPLOYEE ══════════════════════════

    public async Task<IReadOnlyList<AiEmployeeDto>> GetAllEmployeesAsync(int limit = 50, CancellationToken ct = default)
    {
        var employees = await _db.MstEmployees
            .Include(e => e.Dept)
            .Include(e => e.Designation)
            .Where(e => e.IsActive == true)
            .AsNoTracking()
            .OrderBy(e => e.FirstName)
            .Take(limit)
            .ToListAsync(ct);

        return employees.Select(MapEmployee).ToList();
    }

    public async Task<AiEmployeeDto?> GetEmployeeByIdAsync(long employeeId, CancellationToken ct = default)
    {
        var emp = await _db.MstEmployees
            .Include(e => e.Dept)
            .Include(e => e.Designation)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.EmployeeId == employeeId, ct);

        return emp is null ? null : MapEmployee(emp);
    }

    public async Task<IReadOnlyList<AiEmployeeDto>> SearchEmployeesAsync(string keyword, CancellationToken ct = default)
    {
        var kw = keyword.ToLower();
        var employees = await _db.MstEmployees
            .Include(e => e.Dept)
            .Include(e => e.Designation)
            .Where(e => (e.FirstName != null && e.FirstName.ToLower().Contains(kw)) ||
                        (e.LastName != null && e.LastName.ToLower().Contains(kw)) ||
                        e.EmpCode.ToLower().Contains(kw) ||
                        (e.Email1 != null && e.Email1.ToLower().Contains(kw)))
            .AsNoTracking()
            .OrderBy(e => e.FirstName)
            .Take(20)
            .ToListAsync(ct);

        return employees.Select(MapEmployee).ToList();
    }

    public async Task<IReadOnlyList<AiEmployeeDto>> GetEmployeesByDepartmentAsync(string departmentName, CancellationToken ct = default)
    {
        var kw = departmentName.ToLower();
        var employees = await _db.MstEmployees
            .Include(e => e.Dept)
            .Include(e => e.Designation)
            .Where(e => e.IsActive == true && e.Dept != null && e.Dept.DeptName.ToLower().Contains(kw))
            .AsNoTracking()
            .OrderBy(e => e.FirstName)
            .ToListAsync(ct);

        return employees.Select(MapEmployee).ToList();
    }

    // ══════════════════════════ HR ══════════════════════════

    public async Task<IReadOnlyList<AiLeaveRequestDto>> GetLeaveRequestsAsync(string? status = null, int limit = 20, CancellationToken ct = default)
    {
        var query = _db.HrLeaveRequests.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(l => l.Status.ToLower() == status.ToLower());

        var leaves = await query
            .OrderByDescending(l => l.CreatedOn)
            .Take(limit)
            .ToListAsync(ct);

        var empIds = leaves.Select(l => l.EmployeeId).Distinct().ToList();
        var empMap = await GetEmployeeNameMapAsync(empIds, ct);

        var leaveTypeIds = leaves.Select(l => l.LeaveTypeId).Distinct().ToList();
        var leaveTypes = await _db.HrLeaveTypes
            .Where(lt => leaveTypeIds.Contains(lt.LeaveTypeId))
            .AsNoTracking()
            .ToDictionaryAsync(lt => lt.LeaveTypeId, lt => lt.LeaveName, ct);

        return leaves.Select(l => new AiLeaveRequestDto
        {
            LeaveId = l.LeaveId,
            LeaveNo = l.LeaveNo,
            EmployeeId = l.EmployeeId,
            EmployeeName = empMap.GetValueOrDefault(l.EmployeeId),
            LeaveType = leaveTypes.GetValueOrDefault(l.LeaveTypeId),
            FromDate = l.FromDate,
            ToDate = l.ToDate,
            TotalDays = l.TotalDays,
            Reason = l.Reason,
            Status = l.Status
        }).ToList();
    }

    public async Task<IReadOnlyList<AiLeaveRequestDto>> GetEmployeeLeaveRequestsAsync(long employeeId, CancellationToken ct = default)
    {
        var leaves = await _db.HrLeaveRequests
            .Where(l => l.EmployeeId == employeeId)
            .AsNoTracking()
            .OrderByDescending(l => l.CreatedOn)
            .Take(20)
            .ToListAsync(ct);

        var empMap = await GetEmployeeNameMapAsync([employeeId], ct);

        var leaveTypeIds = leaves.Select(l => l.LeaveTypeId).Distinct().ToList();
        var leaveTypes = await _db.HrLeaveTypes
            .Where(lt => leaveTypeIds.Contains(lt.LeaveTypeId))
            .AsNoTracking()
            .ToDictionaryAsync(lt => lt.LeaveTypeId, lt => lt.LeaveName, ct);

        return leaves.Select(l => new AiLeaveRequestDto
        {
            LeaveId = l.LeaveId,
            LeaveNo = l.LeaveNo,
            EmployeeId = l.EmployeeId,
            EmployeeName = empMap.GetValueOrDefault(l.EmployeeId),
            LeaveType = leaveTypes.GetValueOrDefault(l.LeaveTypeId),
            FromDate = l.FromDate,
            ToDate = l.ToDate,
            TotalDays = l.TotalDays,
            Reason = l.Reason,
            Status = l.Status
        }).ToList();
    }

    public async Task<IReadOnlyList<AiAttendanceDto>> GetAttendanceAsync(long? employeeId = null, DateOnly? date = null, int limit = 20, CancellationToken ct = default)
    {
        var query = _db.HybEmployeeAttendances
            .Include(a => a.Employee)
            .AsNoTracking()
            .AsQueryable();

        if (employeeId.HasValue)
            query = query.Where(a => a.EmployeeId == employeeId.Value);

        if (date.HasValue)
            query = query.Where(a => a.AttendanceDate == date.Value);

        var attendances = await query
            .OrderByDescending(a => a.AttendanceDate)
            .Take(limit)
            .ToListAsync(ct);

        return attendances.Select(a => new AiAttendanceDto
        {
            AttendanceId = a.AttendanceId,
            EmployeeId = a.EmployeeId,
            EmployeeName = a.Employee != null ? $"{a.Employee.FirstName} {a.Employee.LastName}".Trim() : null,
            AttendanceDate = a.AttendanceDate,
            Status = a.Status,
            CheckIn = a.CheckIn.HasValue ? TimeOnly.FromDateTime(a.CheckIn.Value) : null,
            CheckOut = a.CheckOut.HasValue ? TimeOnly.FromDateTime(a.CheckOut.Value) : null,
            TotalHours = a.TotalHours
        }).ToList();
    }

    public async Task<IReadOnlyList<AiLoanDto>> GetLoansAsync(string? status = null, int limit = 20, CancellationToken ct = default)
    {
        var query = _db.HrLoans.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(l => l.Status.ToLower() == status.ToLower());

        var loans = await query
            .OrderByDescending(l => l.CreatedOn)
            .Take(limit)
            .ToListAsync(ct);

        var empIds = loans.Select(l => l.EmployeeId).Distinct().ToList();
        var empMap = await GetEmployeeNameMapAsync(empIds, ct);

        return loans.Select(l => new AiLoanDto
        {
            LoanId = l.LoanId,
            LoanNo = l.LoanNo,
            EmployeeId = l.EmployeeId,
            EmployeeName = empMap.GetValueOrDefault(l.EmployeeId),
            LoanType = l.LoanType,
            LoanAmount = l.LoanAmount,
            PaidAmount = l.RecoveredAmount,
            BalanceAmount = l.OutstandingAmount,
            Status = l.Status
        }).ToList();
    }

    public async Task<IReadOnlyList<AiOvertimeDto>> GetOvertimesAsync(long? employeeId, int limit = 20, CancellationToken ct = default)
    {
        var query = _db.HrOvertimes.AsNoTracking().AsQueryable();

        if (employeeId.HasValue)
            query = query.Where(o => o.EmployeeId == employeeId.Value);

        var overtimes = await query
            .OrderByDescending(o => o.OtDate)
            .Take(limit)
            .ToListAsync(ct);

        var empIds = overtimes.Select(o => o.EmployeeId).Distinct().ToList();
        var empMap = await GetEmployeeNameMapAsync(empIds, ct);

        return overtimes.Select(o => new AiOvertimeDto
        {
            OvertimeId = o.OtId,
            EmployeeId = o.EmployeeId,
            EmployeeName = empMap.GetValueOrDefault(o.EmployeeId),
            OvertimeDate = o.OtDate,
            Hours = o.OtHours,
            Reason = o.OtReason,
            Status = o.Status
        }).ToList();
    }

    public async Task<IReadOnlyList<AiReimbursementDto>> GetReimbursementsAsync(string? status = null, int limit = 20, CancellationToken ct = default)
    {
        var query = _db.HrReimbursements.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(r => r.Status.ToLower() == status.ToLower());

        var reimbursements = await query
            .OrderByDescending(r => r.CreatedOn)
            .Take(limit)
            .ToListAsync(ct);

        var empIds = reimbursements.Select(r => r.EmployeeId).Distinct().ToList();
        var empMap = await GetEmployeeNameMapAsync(empIds, ct);

        return reimbursements.Select(r => new AiReimbursementDto
        {
            ReimbursementId = r.ReimbursementId,
            EmployeeId = r.EmployeeId,
            EmployeeName = empMap.GetValueOrDefault(r.EmployeeId),
            Category = r.ReimbursementType,
            Amount = r.ClaimAmount,
            Description = r.Description,
            Status = r.Status
        }).ToList();
    }

    public async Task<AiHrSummaryDto> GetHrSummaryAsync(CancellationToken ct = default)
    {
        var totalEmp = await _db.MstEmployees.AsNoTracking().CountAsync(ct);
        var activeEmp = await _db.MstEmployees.Where(e => e.IsActive == true).AsNoTracking().CountAsync(ct);
        var pendingLeaves = await _db.HrLeaveRequests.Where(l => l.Status == "Pending").AsNoTracking().CountAsync(ct);
        var today = DateOnly.FromDateTime(DateTime.Today);
        var approvedToday = await _db.HrLeaveRequests
            .Where(l => l.Status == "Approved" && l.FromDate <= today && l.ToDate >= today)
            .AsNoTracking()
            .CountAsync(ct);
        var activeLoans = await _db.HrLoans.Where(l => l.Status == "Active" || l.Status == "Disbursed").AsNoTracking().CountAsync(ct);
        var pendingReimb = await _db.HrReimbursements.Where(r => r.Status == "Pending").AsNoTracking().ToListAsync(ct);

        return new AiHrSummaryDto
        {
            TotalEmployees = totalEmp,
            ActiveEmployees = activeEmp,
            PendingLeaves = pendingLeaves,
            ApprovedLeavesToday = approvedToday,
            ActiveLoans = activeLoans,
            PendingReimbursements = pendingReimb.Count,
            TotalPendingReimbursementAmount = pendingReimb.Sum(r => r.ClaimAmount)
        };
    }

    // ══════════════════════════ ENQUIRY ══════════════════════════

    public async Task<IReadOnlyList<AiEnquiryDto>> GetAllEnquiriesAsync(string? status = null, int limit = 20, CancellationToken ct = default)
    {
        var query = _db.TrnEnquiries
            .Include(e => e.Party)
            .Include(e => e.TrnEnquiryItems)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(e => e.Status != null && e.Status.ToLower() == status.ToLower());

        var enquiries = await query
            .OrderByDescending(e => e.CreatedOn)
            .Take(limit)
            .ToListAsync(ct);

        return enquiries.Select(MapEnquiry).ToList();
    }

    public async Task<AiEnquiryDto?> GetEnquiryByIdAsync(long enquiryId, CancellationToken ct = default)
    {
        var enquiry = await _db.TrnEnquiries
            .Include(e => e.Party)
            .Include(e => e.TrnEnquiryItems)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.EnquiryId == enquiryId, ct);

        return enquiry is null ? null : MapEnquiry(enquiry);
    }

    public async Task<IReadOnlyList<AiEnquiryDto>> SearchEnquiriesAsync(string keyword, CancellationToken ct = default)
    {
        var kw = keyword.ToLower();
        var enquiries = await _db.TrnEnquiries
            .Include(e => e.Party)
            .Include(e => e.TrnEnquiryItems)
            .Where(e => e.EnquiryNo.ToLower().Contains(kw) ||
                        e.Party.Name.ToLower().Contains(kw) ||
                        (e.ContactPerson != null && e.ContactPerson.ToLower().Contains(kw)))
            .AsNoTracking()
            .OrderByDescending(e => e.CreatedOn)
            .Take(20)
            .ToListAsync(ct);

        return enquiries.Select(MapEnquiry).ToList();
    }

    // ══════════════════════════ QUOTATION ══════════════════════════

    public async Task<IReadOnlyList<AiQuotationDto>> GetAllQuotationsAsync(string? status = null, int limit = 20, CancellationToken ct = default)
    {
        var query = _db.TrnQuotations
            .Include(q => q.Party)
            .Include(q => q.Enquiry)
            .Include(q => q.TrnQuotationItems)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(q => q.Status != null && q.Status.ToLower() == status.ToLower());

        var quotations = await query
            .OrderByDescending(q => q.CreatedOn)
            .Take(limit)
            .ToListAsync(ct);

        return quotations.Select(MapQuotation).ToList();
    }

    public async Task<AiQuotationDto?> GetQuotationByIdAsync(long quotationId, CancellationToken ct = default)
    {
        var quotation = await _db.TrnQuotations
            .Include(q => q.Party)
            .Include(q => q.Enquiry)
            .Include(q => q.TrnQuotationItems)
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.QuotationId == quotationId, ct);

        return quotation is null ? null : MapQuotation(quotation);
    }

    public async Task<IReadOnlyList<AiQuotationDto>> SearchQuotationsAsync(string keyword, CancellationToken ct = default)
    {
        var kw = keyword.ToLower();
        var quotations = await _db.TrnQuotations
            .Include(q => q.Party)
            .Include(q => q.Enquiry)
            .Include(q => q.TrnQuotationItems)
            .Where(q => q.QuotationNo.ToLower().Contains(kw) ||
                        q.Party.Name.ToLower().Contains(kw))
            .AsNoTracking()
            .OrderByDescending(q => q.CreatedOn)
            .Take(20)
            .ToListAsync(ct);

        return quotations.Select(MapQuotation).ToList();
    }

    // ══════════════════════════ PURCHASE ══════════════════════════

    public async Task<IReadOnlyList<AiPurchaseOrderDto>> GetAllPurchaseOrdersAsync(string? status = null, int limit = 20, CancellationToken ct = default)
    {
        var query = _db.TrnPurchaseOrders
            .Include(po => po.Party)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(po => po.Status.ToLower() == status.ToLower());

        var orders = await query
            .OrderByDescending(po => po.CreatedOn)
            .Take(limit)
            .ToListAsync(ct);

        return orders.Select(MapPurchaseOrder).ToList();
    }

    public async Task<AiPurchaseOrderDto?> GetPurchaseOrderByIdAsync(long poId, CancellationToken ct = default)
    {
        var po = await _db.TrnPurchaseOrders
            .Include(p => p.Party)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PurchaseOrderId == poId, ct);

        return po is null ? null : MapPurchaseOrder(po);
    }

    public async Task<IReadOnlyList<AiPurchaseOrderDto>> SearchPurchaseOrdersAsync(string keyword, CancellationToken ct = default)
    {
        var kw = keyword.ToLower();
        var orders = await _db.TrnPurchaseOrders
            .Include(po => po.Party)
            .Where(po => po.PoNo.ToLower().Contains(kw) ||
                         po.Party.Name.ToLower().Contains(kw))
            .AsNoTracking()
            .OrderByDescending(po => po.CreatedOn)
            .Take(20)
            .ToListAsync(ct);

        return orders.Select(MapPurchaseOrder).ToList();
    }

    public async Task<IReadOnlyList<AiGoodsReceiptDto>> GetGoodsReceiptsAsync(string? status = null, int limit = 20, CancellationToken ct = default)
    {
        var query = _db.TrnGoodsReceipts
            .Include(g => g.Party)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(g => g.Status.ToLower() == status.ToLower());

        var receipts = await query
            .OrderByDescending(g => g.CreatedOn)
            .Take(limit)
            .ToListAsync(ct);

        return receipts.Select(g => new AiGoodsReceiptDto
        {
            GrnId = g.GrnId,
            GrnNo = g.GrnNo,
            GrnDate = g.GrnDate,
            SupplierName = g.Party?.Name,
            PoNo = g.PoNo,
            TotalQuantity = g.TotalQuantity,
            TotalAcceptedQty = g.TotalAcceptedQty,
            TotalRejectedQty = g.TotalRejectedQty,
            Status = g.Status,
            IsQualityChecked = g.IsQualityChecked
        }).ToList();
    }

    public async Task<IReadOnlyList<AiPurchaseInvoiceDto>> GetPurchaseInvoicesAsync(string? status = null, int limit = 20, CancellationToken ct = default)
    {
        var query = _db.TrnPurchaseInvoices
            .Include(pi => pi.Party)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(pi => pi.Status != null && pi.Status.ToLower() == status.ToLower());

        var invoices = await query
            .OrderByDescending(pi => pi.CreatedOn)
            .Take(limit)
            .ToListAsync(ct);

        return invoices.Select(pi => new AiPurchaseInvoiceDto
        {
            PurchaseInvoiceId = pi.PurchaseInvoiceId,
            InvoiceNo = pi.InvoiceNo,
            InvoiceDate = pi.InvoiceDate,
            SupplierName = pi.Party?.Name,
            GrandTotal = pi.GrandTotal,
            PaidAmount = pi.PaidAmount,
            BalanceAmount = pi.BalanceAmount,
            Status = pi.Status
        }).ToList();
    }

    // ══════════════════════════ ACCOUNTING ══════════════════════════

    public async Task<IReadOnlyList<AiReceiptDto>> GetReceiptsAsync(DateOnly? from = null, DateOnly? to = null, int limit = 20, CancellationToken ct = default)
    {
        var query = _db.TrnReceipts
            .Include(r => r.Party)
            .AsNoTracking()
            .AsQueryable();

        if (from.HasValue)
            query = query.Where(r => r.ReceiptDate >= from.Value);
        if (to.HasValue)
            query = query.Where(r => r.ReceiptDate <= to.Value);

        var receipts = await query
            .OrderByDescending(r => r.CreatedOn)
            .Take(limit)
            .ToListAsync(ct);

        return receipts.Select(r => new AiReceiptDto
        {
            ReceiptId = r.ReceiptId,
            ReceiptNo = r.ReceiptNo,
            ReceiptDate = r.ReceiptDate,
            PartyName = r.Party?.Name,
            PaymentMode = r.PaymentMode,
            Amount = r.Amount,
            Status = r.Status
        }).ToList();
    }

    public async Task<IReadOnlyList<AiPaymentDto>> GetPaymentsAsync(DateOnly? from = null, DateOnly? to = null, int limit = 20, CancellationToken ct = default)
    {
        var query = _db.TrnPayments
            .Include(p => p.Party)
            .AsNoTracking()
            .AsQueryable();

        if (from.HasValue)
            query = query.Where(p => p.PaymentDate >= from.Value);
        if (to.HasValue)
            query = query.Where(p => p.PaymentDate <= to.Value);

        var payments = await query
            .OrderByDescending(p => p.CreatedOn)
            .Take(limit)
            .ToListAsync(ct);

        return payments.Select(p => new AiPaymentDto
        {
            PaymentId = p.PaymentId,
            PaymentNo = p.PaymentNo,
            PaymentDate = p.PaymentDate,
            PartyName = p.Party?.Name,
            PaymentMode = p.PaymentMode,
            Amount = p.Amount,
            Status = p.Status
        }).ToList();
    }

    public async Task<IReadOnlyList<AiExpenseVoucherDto>> GetExpenseVouchersAsync(string? category = null, int limit = 20, CancellationToken ct = default)
    {
        var query = _db.TrnExpenseVouchers.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(category))
            query = query.Where(ev => ev.ExpenseCategory != null && ev.ExpenseCategory.ToLower().Contains(category.ToLower()));

        var vouchers = await query
            .OrderByDescending(ev => ev.VoucherDate)
            .Take(limit)
            .ToListAsync(ct);

        return vouchers.Select(ev => new AiExpenseVoucherDto
        {
            ExpenseVoucherId = ev.ExpenseVoucherId,
            VoucherNo = ev.VoucherNo,
            VoucherDate = ev.VoucherDate,
            ExpenseCategory = ev.ExpenseCategory,
            GrandTotal = ev.GrandTotal,
            Status = ev.Status
        }).ToList();
    }

    public async Task<AiOutstandingSummaryDto> GetOutstandingSummaryAsync(CancellationToken ct = default)
    {
        var arList = await _db.TrnArOutstandings
            .Include(a => a.Party)
            .Where(a => a.OutstandingAmount > 0)
            .AsNoTracking()
            .ToListAsync(ct);

        var apList = await _db.TrnApOutstandings
            .Include(a => a.Party)
            .Where(a => a.OutstandingAmount > 0)
            .AsNoTracking()
            .ToListAsync(ct);

        var topAr = arList
            .GroupBy(a => new { a.PartyId, Name = a.Party?.Name })
            .Select(g => new AiOutstandingPartyDto
            {
                PartyId = g.Key.PartyId,
                PartyName = g.Key.Name,
                Amount = g.Sum(x => x.OutstandingAmount ?? 0)
            })
            .OrderByDescending(x => x.Amount)
            .Take(10)
            .ToList();

        var topAp = apList
            .GroupBy(a => new { a.PartyId, Name = a.Party?.Name })
            .Select(g => new AiOutstandingPartyDto
            {
                PartyId = g.Key.PartyId,
                PartyName = g.Key.Name,
                Amount = g.Sum(x => x.OutstandingAmount ?? 0)
            })
            .OrderByDescending(x => x.Amount)
            .Take(10)
            .ToList();

        return new AiOutstandingSummaryDto
        {
            TotalReceivable = arList.Sum(a => a.OutstandingAmount ?? 0),
            TotalPayable = apList.Sum(a => a.OutstandingAmount ?? 0),
            ReceivableCount = arList.Count,
            PayableCount = apList.Count,
            TopReceivables = topAr,
            TopPayables = topAp
        };
    }

    // ══════════════════════════ STORE / INVENTORY ══════════════════════════

    public async Task<IReadOnlyList<AiStoreIssueDto>> GetStoreIssuesAsync(string? status = null, int limit = 20, CancellationToken ct = default)
    {
        var query = _db.TrnStoreIssues.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(si => si.Status.ToLower() == status.ToLower());

        var issues = await query
            .OrderByDescending(si => si.CreatedOn)
            .Take(limit)
            .ToListAsync(ct);

        return issues.Select(si => new AiStoreIssueDto
        {
            IssueId = si.IssueId,
            IssueNo = si.IssueNo,
            IssueDate = si.IssueDate,
            IssueType = si.IssueType,
            JobNo = si.JobNo,
            TotalItems = si.TotalItems,
            TotalAmount = si.TotalAmount,
            Status = si.Status
        }).ToList();
    }

    public async Task<IReadOnlyList<AiStoreReceiveDto>> GetStoreReceivesAsync(string? status = null, int limit = 20, CancellationToken ct = default)
    {
        var query = _db.TrnStoreReceives.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(sr => sr.Status.ToLower() == status.ToLower());

        var receives = await query
            .OrderByDescending(sr => sr.CreatedOn)
            .Take(limit)
            .ToListAsync(ct);

        return receives.Select(sr => new AiStoreReceiveDto
        {
            ReceiveId = sr.ReceiveId,
            ReceiveNo = sr.ReceiveNo,
            ReceiveDate = sr.ReceiveDate,
            ReceiveType = sr.ReceiveType,
            GrnNo = sr.GrnNo,
            SupplierName = sr.SupplierName,
            TotalItems = sr.TotalItems,
            TotalAmount = sr.TotalAmount,
            Status = sr.Status
        }).ToList();
    }

    public async Task<IReadOnlyList<AiMaterialDto>> GetAllMaterialsAsync(string? category = null, CancellationToken ct = default)
    {
        var query = _db.MstMaterials
            .Where(m => m.IsActive == true)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrEmpty(category))
            query = query.Where(m => m.MaterialCategory != null && m.MaterialCategory.ToLower().Contains(category.ToLower()));

        var materials = await query
            .OrderBy(m => m.MaterialName)
            .ToListAsync(ct);

        return materials.Select(m => new AiMaterialDto
        {
            MaterialCode = m.MaterialCode,
            MaterialName = m.MaterialName,
            MaterialCategory = m.MaterialCategory,
            UnitOfMeasure = m.UnitOfMeasure,
            RatePerUnit = m.RatePerUnit,
            ReorderLevel = m.ReorderLevel,
            IsActive = m.IsActive ?? false
        }).ToList();
    }

    public async Task<IReadOnlyList<AiMaterialDto>> SearchMaterialsAsync(string keyword, CancellationToken ct = default)
    {
        var kw = keyword.ToLower();
        var materials = await _db.MstMaterials
            .Where(m => m.MaterialName.ToLower().Contains(kw) ||
                        m.MaterialCode.ToLower().Contains(kw) ||
                        (m.MaterialCategory != null && m.MaterialCategory.ToLower().Contains(kw)))
            .AsNoTracking()
            .OrderBy(m => m.MaterialName)
            .Take(20)
            .ToListAsync(ct);

        return materials.Select(m => new AiMaterialDto
        {
            MaterialCode = m.MaterialCode,
            MaterialName = m.MaterialName,
            MaterialCategory = m.MaterialCategory,
            UnitOfMeasure = m.UnitOfMeasure,
            RatePerUnit = m.RatePerUnit,
            ReorderLevel = m.ReorderLevel,
            IsActive = m.IsActive ?? false
        }).ToList();
    }

    // ══════════════════════════ CHALLAN ══════════════════════════

    public async Task<IReadOnlyList<AiChallanDto>> GetAllChallansAsync(string? status = null, int limit = 20, CancellationToken ct = default)
    {
        var query = _db.TrnChallans
            .Include(c => c.Party)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(c => c.Status != null && c.Status.ToLower() == status.ToLower());

        var challans = await query
            .OrderByDescending(c => c.CreatedOn)
            .Take(limit)
            .ToListAsync(ct);

        return challans.Select(MapChallan).ToList();
    }

    public async Task<IReadOnlyList<AiChallanDto>> GetChallansByJobNoAsync(string jobNo, CancellationToken ct = default)
    {
        var job = await _db.TrnJobs.AsNoTracking().FirstOrDefaultAsync(j => j.JobNo == jobNo, ct);
        if (job is null) return [];

        var challans = await _db.TrnChallans
            .Include(c => c.Party)
            .Where(c => c.JobId == job.JobId)
            .AsNoTracking()
            .OrderByDescending(c => c.CreatedOn)
            .ToListAsync(ct);

        return challans.Select(MapChallan).ToList();
    }

    // ══════════════════════════ PROFORMA INVOICE ══════════════════════════

    public async Task<IReadOnlyList<AiProformaInvoiceDto>> GetAllProformaInvoicesAsync(string? status = null, int limit = 20, CancellationToken ct = default)
    {
        var query = _db.TrnProformaInvoices
            .Include(pi => pi.Party)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(pi => pi.Status != null && pi.Status.ToLower() == status.ToLower());

        var proformas = await query
            .OrderByDescending(pi => pi.CreatedOn)
            .Take(limit)
            .ToListAsync(ct);

        return proformas.Select(pi => new AiProformaInvoiceDto
        {
            ProformaInvoiceId = pi.ProformaInvoiceId,
            ProformaNo = pi.ProformaNo,
            ProformaDate = pi.ProformaDate,
            CustomerName = pi.Party?.Name,
            SubtotalAmount = pi.SubtotalAmount,
            GrandTotal = pi.GrandTotal,
            ValidTill = pi.ValidTill,
            Status = pi.Status
        }).ToList();
    }

    // ══════════════════════════ MACHINE BREAKDOWN ══════════════════════════

    public async Task<IReadOnlyList<AiMachineBreakdownDto>> GetMachineBreakdownsAsync(string? status = null, int limit = 20, CancellationToken ct = default)
    {
        var query = _db.TrnMachineBreakdowns
            .Include(b => b.Machine)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(b => b.BreakdownStatus != null && b.BreakdownStatus.ToLower() == status.ToLower());

        var breakdowns = await query
            .OrderByDescending(b => b.BreakdownStartTime)
            .Take(limit)
            .ToListAsync(ct);

        return breakdowns.Select(MapBreakdown).ToList();
    }

    public async Task<IReadOnlyList<AiMachineBreakdownDto>> GetBreakdownsByMachineAsync(long machineId, CancellationToken ct = default)
    {
        var breakdowns = await _db.TrnMachineBreakdowns
            .Include(b => b.Machine)
            .Where(b => b.MachineId == machineId)
            .AsNoTracking()
            .OrderByDescending(b => b.BreakdownStartTime)
            .Take(20)
            .ToListAsync(ct);

        return breakdowns.Select(MapBreakdown).ToList();
    }

    // ══════════════════════════ PRINTING MASTERS ══════════════════════════

    public async Task<IReadOnlyList<AiPaperDto>> GetAllPapersAsync(CancellationToken ct = default)
    {
        var papers = await _db.MstPapers
            .Where(p => p.IsActive == true)
            .AsNoTracking()
            .OrderBy(p => p.PaperName)
            .ToListAsync(ct);

        return papers.Select(p => new AiPaperDto
        {
            PaperId = (int)p.PaperId,
            PaperName = p.PaperName,
            PaperType = p.PaperType,
            Gsm = p.Gsm,
            Size = p.SheetSizeName,
            RatePerKg = p.CostPerKg,
            IsActive = p.IsActive ?? false
        }).ToList();
    }

    public async Task<IReadOnlyList<AiInkDto>> GetAllInksAsync(CancellationToken ct = default)
    {
        var inks = await _db.MstInks
            .Where(i => i.IsActive == true)
            .AsNoTracking()
            .OrderBy(i => i.InkName)
            .ToListAsync(ct);

        return inks.Select(i => new AiInkDto
        {
            InkName = i.InkName,
            InkType = i.InkType,
            Color = i.ColorName,
            RatePerKg = i.CostPerKg,
            IsActive = i.IsActive ?? false
        }).ToList();
    }

    public async Task<IReadOnlyList<AiPlateDto>> GetAllPlatesAsync(CancellationToken ct = default)
    {
        var plates = await _db.MstPlates
            .Where(p => p.IsActive == true)
            .AsNoTracking()
            .OrderBy(p => p.PlateName)
            .ToListAsync(ct);

        return plates.Select(p => new AiPlateDto
        {
            PlateId = (int)p.PlateId,
            PlateName = p.PlateName,
            PlateType = p.PlateType,
            Size = $"{p.PlateLengthMm}x{p.PlateWidthMm}mm",
            Rate = p.PlateCost,
            IsActive = p.IsActive ?? false
        }).ToList();
    }

    public async Task<IReadOnlyList<AiBindingDto>> GetAllBindingsAsync(CancellationToken ct = default)
    {
        var bindings = await _db.MstBindings
            .Where(b => b.IsActive == true)
            .AsNoTracking()
            .OrderBy(b => b.BindingName)
            .ToListAsync(ct);

        return bindings.Select(b => new AiBindingDto
        {
            BindingId = (int)b.BindingId,
            BindingName = b.BindingName,
            BindingType = b.BindingType,
            RatePerUnit = b.CostPerBook,
            IsActive = b.IsActive ?? false
        }).ToList();
    }

    public async Task<IReadOnlyList<AiFinishingDto>> GetAllFinishingsAsync(CancellationToken ct = default)
    {
        var finishings = await _db.MstFinishings
            .Where(f => f.IsActive == true)
            .AsNoTracking()
            .OrderBy(f => f.FinishingName)
            .ToListAsync(ct);

        return finishings.Select(f => new AiFinishingDto
        {
            FinishingId = (int)f.FinishingId,
            FinishingName = f.FinishingName,
            FinishingType = f.FinishingType,
            RatePerUnit = f.CostPerSheet,
            IsActive = f.IsActive ?? false
        }).ToList();
    }

    // ══════════════════════════ HELPERS ══════════════════════════

    private async Task<string> GenerateDocumentNumberAsync(string prefix, CancellationToken ct)
    {
        var sequence = await _db.MstDocumentSequences
            .FirstOrDefaultAsync(s => s.ProcessCode == prefix && s.IsActive == true, ct);

        if (sequence is not null)
        {
            sequence.CurrentNumber++;
            sequence.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);

            var number = sequence.CurrentNumber.ToString().PadLeft(sequence.PaddingLength, '0');
            var year = DateTime.Today.Year;
            return $"{sequence.Prefix ?? prefix}-{year}-{number}";
        }

        return $"{prefix}-{DateTime.Today:yyyy}-{Random.Shared.Next(1, 9999):D4}";
    }

    private async Task<Dictionary<long, string>> GetEmployeeNameMapAsync(List<long> employeeIds, CancellationToken ct)
    {
        if (employeeIds.Count == 0) return [];

        var employees = await _db.MstEmployees
            .Where(e => employeeIds.Contains(e.EmployeeId))
            .AsNoTracking()
            .Select(e => new { e.EmployeeId, e.FirstName, e.LastName })
            .ToListAsync(ct);

        return employees.ToDictionary(
            e => e.EmployeeId,
            e => $"{e.FirstName} {e.LastName}".Trim());
    }

    // ══════════════════════════ MAPPERS ══════════════════════════

    private static AiJobDto MapJob(persistence.Models.TrnJob j) => new()
    {
        JobId = j.JobId,
        JobNo = j.JobNo,
        JobDate = j.JobDate,
        CustomerName = j.Party?.Name,
        ProductName = j.ProductName,
        ProductDescription = j.ProductDescription,
        Quantity = j.Quantity,
        JobType = j.JobType?.Jobtypename,
        StatusCode = j.StatusCode,
        CurrentStage = j.CurrentStage,
        ProgressPercent = j.ProgressPercent,
        Priority = j.Priority,
        EstimatedCost = j.EstimatedCost,
        NetAmount = j.NetAmount,
        DeliveryDate = j.DeliveryDate,
        CreatedOn = j.CreatedOn
    };

    private static AiMachineAllocationDto MapAllocation(persistence.Models.TrnJobMachineAllocation a) => new()
    {
        AllocationId = a.AllocationId,
        JobId = a.JobId,
        JobNo = a.JobNo,
        MachineId = a.MachineId,
        MachineCode = a.MachineCode,
        MachineName = a.MachineName,
        ProcessCode = a.ProcessCode,
        ProcessName = a.ProcessName,
        PlannedQuantity = a.PlannedQuantity,
        CompletedQuantity = a.CompletedQuantity,
        AllocationStatus = a.AllocationStatus,
        PlannedStartTime = a.PlannedStartTime,
        PlannedEndTime = a.PlannedEndTime
    };

    private static AiInvoiceDto MapInvoice(persistence.Models.TrnSalesInvoice i, string? jobNo) => new()
    {
        SalesInvoiceId = i.SalesInvoiceId,
        InvoiceNo = i.InvoiceNo,
        InvoiceDate = i.InvoiceDate,
        CustomerName = i.Party?.Name,
        JobNo = jobNo,
        SubtotalAmount = i.SubtotalAmount,
        TotalTaxAmount = i.TotalTaxAmount,
        GrandTotal = i.GrandTotal,
        PaidAmount = i.PaidAmount,
        BalanceAmount = i.BalanceAmount,
        Status = i.Status
    };

    private static AiGatePassDto MapGatePass(persistence.Models.TrnGatePass g) => new()
    {
        GatePassId = g.GatePassId,
        GatePassNo = g.GatePassNo,
        GatePassDate = g.GatePassDate,
        GatepassType = g.GatepassType,
        ReferenceNo = g.ReferenceNo,
        VehicleNo = g.VehicleNo,
        DriverName = g.DriverName,
        DriverContact = g.DriverContact,
        Purpose = g.Purpose,
        Status = g.Status,
        TotalQuantity = g.TotalQuantity
    };

    private static AiVendorOutsourceDto MapOutsource(persistence.Models.TrnJobOutsource o, string? jobNo) => new()
    {
        OutsourceId = o.OutsourceId,
        OutsourceNo = o.OutsourceNo,
        OutsourceDate = o.OutsourceDate,
        JobNo = jobNo,
        VendorId = o.VendorId,
        ProcessType = o.ProcessType,
        TotalQuantity = o.TotalQuantity,
        TotalAmount = o.TotalAmount,
        Status = o.Status,
        ExpectedDeliveryDate = o.ExpectedDeliveryDate
    };

    private static AiCustomerDto MapCustomer(persistence.Models.MstParty p, persistence.Models.MstCustomer? c) => new()
    {
        PartyId = p.Id,
        Code = p.Code,
        Name = p.Name,
        GstNo = p.Gstno,
        PanNo = p.PanNo,
        Phone = p.Mobile?.ToString(),
        Email = p.Email,
        IsActive = p.IsActive,
        CustomerType = c?.CustomerTypeNavigation?.Name,
        CustomerGroup = c?.CustomerGroupNavigation?.Name
    };

    private static AiEmployeeDto MapEmployee(persistence.Models.MstEmployee e) => new()
    {
        EmployeeId = e.EmployeeId,
        EmpCode = e.EmpCode,
        FirstName = e.FirstName,
        LastName = e.LastName,
        FullName = $"{e.FirstName} {e.LastName}".Trim(),
        Department = e.Dept?.DeptName,
        Designation = e.Designation?.DesignationName,
        Phone = e.MobileNo1,
        Email = e.Email1,
        DateOfJoining = e.DateOfJoining,
        IsActive = e.IsActive ?? false
    };

    private static AiEnquiryDto MapEnquiry(persistence.Models.TrnEnquiry e) => new()
    {
        EnquiryId = e.EnquiryId,
        EnquiryNo = e.EnquiryNo,
        EnquiryDate = e.EnquiryDate,
        CustomerName = e.Party?.Name,
        ContactPerson = e.ContactPerson,
        ContactMobile = e.ContactMobile,
        EnquirySource = e.EnquirySource,
        Priority = e.Priority,
        Status = e.Status,
        ExpectedDeliveryDate = e.ExpectedDeliveryDate,
        Remarks = e.Remarks,
        ItemCount = e.TrnEnquiryItems?.Count ?? 0
    };

    private static AiQuotationDto MapQuotation(persistence.Models.TrnQuotation q) => new()
    {
        QuotationId = q.QuotationId,
        QuotationNo = q.QuotationNo,
        QuotationDate = q.QuotationDate,
        CustomerName = q.Party?.Name,
        EnquiryId = q.EnquiryId,
        EnquiryNo = q.Enquiry?.EnquiryNo,
        TotalAmount = q.TotalAmount,
        NetAmount = q.NetAmount,
        ValidTill = q.ValidTill,
        Status = q.Status,
        ItemCount = q.TrnQuotationItems?.Count ?? 0
    };

    private static AiPurchaseOrderDto MapPurchaseOrder(persistence.Models.TrnPurchaseOrder po) => new()
    {
        PurchaseOrderId = po.PurchaseOrderId,
        PoNo = po.PoNo,
        PoDate = po.PoDate,
        SupplierName = po.Party?.Name,
        GrandTotal = po.GrandTotal,
        Status = po.Status,
        IsApproved = po.IsApproved,
        ExpectedDeliveryDate = po.ExpectedDeliveryDate
    };

    private static AiChallanDto MapChallan(persistence.Models.TrnChallan c) => new()
    {
        ChallanId = c.ChallanId,
        ChallanNo = c.ChallanNo,
        ChallanDate = c.ChallanDate,
        CustomerName = c.Party?.Name,
        VehicleNo = c.VehicleNo,
        TotalQty = c.TotalQty,
        TotalAmount = c.TotalAmount,
        Status = c.Status
    };

    private static AiMachineBreakdownDto MapBreakdown(persistence.Models.TrnMachineBreakdown b) => new()
    {
        BreakdownId = b.BreakdownId,
        MachineId = b.MachineId,
        MachineName = b.Machine?.MachineName,
        FaultCode = b.FaultCode,
        FaultDescription = b.FaultDescription,
        FaultCategory = b.FaultCategory,
        SeverityLevel = b.SeverityLevel,
        BreakdownStartTime = b.BreakdownStartTime,
        BreakdownEndTime = b.BreakdownEndTime,
        DowntimeMinutes = b.DowntimeMinutes,
        BreakdownStatus = b.BreakdownStatus
    };

    // ══════════════════════════ NEW — Jobs Search ══════════════════════════

    public async Task<IReadOnlyList<AiJobDto>> SearchJobsAsync(string keyword, int limit = 20, CancellationToken ct = default)
    {
        var lower = keyword.ToLower();
        var jobs = await _db.TrnJobs
            .Include(j => j.Party)
            .Include(j => j.JobType)
            .AsNoTracking()
            .Where(j =>
                (j.JobNo != null && j.JobNo.ToLower().Contains(lower)) ||
                (j.Party != null && j.Party.Name != null && j.Party.Name.ToLower().Contains(lower)) ||
                (j.ProductName != null && j.ProductName.ToLower().Contains(lower)))
            .OrderByDescending(j => j.CreatedOn)
            .Take(limit)
            .ToListAsync(ct);

        return jobs.Select(MapJob).ToList();
    }

    // ══════════════════════════ NEW — Vendors ══════════════════════════

    public async Task<IReadOnlyList<AiVendorDto>> GetAllVendorsAsync(int limit = 50, CancellationToken ct = default)
    {
        var vendors = await _db.MstVendors
            .Include(v => v.Party)
            .AsNoTracking()
            .Where(v => v.IsActive == true)
            .OrderBy(v => v.Party!.Name)
            .Take(limit)
            .ToListAsync(ct);

        return vendors.Select(v => new AiVendorDto
        {
            VendorId = v.VendorId,
            VendorCode = v.Party?.Code,
            VendorName = v.Party?.Name,
            VendorType = v.ServiceArea,
            ContactPerson = null,
            Phone = v.Party?.Mobile?.ToString(),
            Email = v.Party?.Email,
            City = null,
            GstNo = v.Party?.Gstno,
            IsActive = v.IsActive ?? false
        }).ToList();
    }

    public async Task<IReadOnlyList<AiVendorDto>> SearchVendorsAsync(string keyword, CancellationToken ct = default)
    {
        var lower = keyword.ToLower();
        var vendors = await _db.MstVendors
            .Include(v => v.Party)
            .AsNoTracking()
            .Where(v =>
                (v.Party != null && v.Party.Name.ToLower().Contains(lower)) ||
                (v.Party != null && v.Party.Code != null && v.Party.Code.ToLower().Contains(lower)))
            .OrderBy(v => v.Party!.Name)
            .Take(50)
            .ToListAsync(ct);

        return vendors.Select(v => new AiVendorDto
        {
            VendorId = v.VendorId,
            VendorCode = v.Party?.Code,
            VendorName = v.Party?.Name,
            VendorType = v.ServiceArea,
            ContactPerson = null,
            Phone = v.Party?.Mobile?.ToString(),
            Email = v.Party?.Email,
            City = null,
            GstNo = v.Party?.Gstno,
            IsActive = v.IsActive ?? false
        }).ToList();
    }

    // ══════════════════════════ NEW — Gate Passes ══════════════════════════

    public async Task<IReadOnlyList<AiGatePassDto>> GetAllGatePassesAsync(string? status = null, int limit = 20, CancellationToken ct = default)
    {
        var query = _db.TrnGatePasses
            .AsNoTracking()
            .OrderByDescending(g => g.GatePassDate);

        var filtered = string.IsNullOrWhiteSpace(status)
            ? query
            : query.Where(g => g.Status != null && g.Status.ToLower() == status.ToLower());

        var passes = await filtered.Take(limit).ToListAsync(ct);
        return passes.Select(g => new AiGatePassDto
        {
            GatePassId = g.GatePassId,
            GatePassNo = g.GatePassNo,
            GatePassDate = g.GatePassDate,
            GatepassType = g.GatepassType,
            ReferenceNo = g.ReferenceNo,
            VehicleNo = g.VehicleNo,
            DriverName = g.DriverName,
            DriverContact = g.DriverContact,
            Purpose = g.Purpose,
            Status = g.Status,
            TotalQuantity = g.TotalQuantity
        }).ToList();
    }

    // ══════════════════════════ NEW — Billing Extended ══════════════════════════

    public async Task<IReadOnlyList<AiCreditNoteDto>> GetCreditNotesAsync(string? status = null, int limit = 20, CancellationToken ct = default)
    {
        var query = _db.TrnCreditNotes
            .Include(c => c.Party)
            .AsNoTracking()
            .OrderByDescending(c => c.CreditNoteDate);

        var filtered = string.IsNullOrWhiteSpace(status)
            ? query
            : query.Where(c => c.Status != null && c.Status.ToLower() == status.ToLower());

        var notes = await filtered.Take(limit).ToListAsync(ct);
        return notes.Select(c => new AiCreditNoteDto
        {
            CreditNoteId = c.CreditNoteId,
            CreditNoteNo = c.CreditNoteNo,
            CreditNoteDate = c.CreditNoteDate,
            CustomerName = c.Party?.Name,
            InvoiceNo = c.OriginalInvoiceNo,
            Reason = c.Reason,
            GrandTotal = c.GrandTotal,
            Status = c.Status
        }).ToList();
    }

    public async Task<IReadOnlyList<AiDebitNoteDto>> GetDebitNotesAsync(string? status = null, int limit = 20, CancellationToken ct = default)
    {
        var query = _db.TrnDebitNotes
            .Include(d => d.Party)
            .AsNoTracking()
            .OrderByDescending(d => d.DebitNoteDate);

        var filtered = string.IsNullOrWhiteSpace(status)
            ? query
            : query.Where(d => d.Status != null && d.Status.ToLower() == status.ToLower());

        var notes = await filtered.Take(limit).ToListAsync(ct);
        return notes.Select(d => new AiDebitNoteDto
        {
            DebitNoteId = d.DebitNoteId,
            DebitNoteNo = d.DebitNoteNo,
            DebitNoteDate = d.DebitNoteDate,
            SupplierName = d.Party?.Name,
            InvoiceNo = d.OriginalInvoiceNo,
            Reason = d.Reason,
            GrandTotal = d.GrandTotal,
            Status = d.Status
        }).ToList();
    }

    // ══════════════════════════ NEW — HR Extended ══════════════════════════

    public async Task<IReadOnlyList<AiBonusDto>> GetBonusesAsync(string? status = null, int limit = 20, CancellationToken ct = default)
    {
        var query = _db.HrBonus
            .AsNoTracking()
            .OrderByDescending(b => b.BonusDate);

        var filtered = string.IsNullOrWhiteSpace(status)
            ? query
            : query.Where(b => b.Status.ToLower() == status.ToLower());

        var items = await filtered.Take(limit).ToListAsync(ct);
        return items.Select(b => new AiBonusDto
        {
            BonusId = b.BonusId,
            BonusNo = b.BonusNo,
            BonusDate = b.BonusDate,
            EmployeeId = b.EmployeeId,
            EmployeeName = null,
            BonusType = b.BonusType,
            BonusAmount = b.BonusAmount,
            Status = b.Status
        }).ToList();
    }

    public async Task<IReadOnlyList<AiIncentiveDto>> GetIncentivesAsync(string? status = null, int limit = 20, CancellationToken ct = default)
    {
        var query = _db.HrIncentives
            .AsNoTracking()
            .OrderByDescending(i => i.IncentiveDate);

        var filtered = string.IsNullOrWhiteSpace(status)
            ? query
            : query.Where(i => i.Status.ToLower() == status.ToLower());

        var items = await filtered.Take(limit).ToListAsync(ct);
        return items.Select(i => new AiIncentiveDto
        {
            IncentiveId = i.IncentiveId,
            IncentiveNo = i.IncentiveNo,
            IncentiveDate = i.IncentiveDate,
            EmployeeId = i.EmployeeId,
            EmployeeName = null,
            IncentiveType = i.IncentiveType,
            Amount = i.IncentiveAmount,
            Status = i.Status
        }).ToList();
    }

    public async Task<IReadOnlyList<AiHolidayDto>> GetHolidaysAsync(int? year = null, CancellationToken ct = default)
    {
        var targetYear = year ?? DateTime.Now.Year;
        var holidays = await _db.HrHolidays
            .AsNoTracking()
            .Where(h => h.HolidayDate.Year == targetYear)
            .OrderBy(h => h.HolidayDate)
            .ToListAsync(ct);

        return holidays.Select(h => new AiHolidayDto
        {
            HolidayId = h.HolidayId,
            HolidayName = h.HolidayName,
            HolidayDate = h.HolidayDate,
            HolidayType = h.HolidayType,
            IsOptional = h.IsOptional ?? false
        }).ToList();
    }

    public async Task<IReadOnlyList<AiMedicalClaimDto>> GetMedicalClaimsAsync(string? status = null, int limit = 20, CancellationToken ct = default)
    {
        var query = _db.HrMedicalClaims
            .AsNoTracking()
            .OrderByDescending(m => m.ClaimDate);

        var filtered = string.IsNullOrWhiteSpace(status)
            ? query
            : query.Where(m => m.Status.ToLower() == status.ToLower());

        var items = await filtered.Take(limit).ToListAsync(ct);
        return items.Select(m => new AiMedicalClaimDto
        {
            ClaimId = m.MedicalClaimId,
            ClaimNo = m.ClaimNo,
            EmployeeId = m.EmployeeId,
            EmployeeName = null,
            ClaimAmount = m.ClaimAmount,
            ApprovedAmount = m.ApprovedAmount,
            Description = m.Description,
            Status = m.Status
        }).ToList();
    }

    public async Task<IReadOnlyList<AiResignationDto>> GetResignationsAsync(string? status = null, int limit = 20, CancellationToken ct = default)
    {
        var query = _db.HrResignations
            .AsNoTracking()
            .OrderByDescending(r => r.ResignationDate);

        var filtered = string.IsNullOrWhiteSpace(status)
            ? query
            : query.Where(r => r.Status.ToLower() == status.ToLower());

        var items = await filtered.Take(limit).ToListAsync(ct);
        return items.Select(r => new AiResignationDto
        {
            ResignationId = r.ResignationId,
            EmployeeId = r.EmployeeId,
            EmployeeName = null,
            ResignationDate = r.ResignationDate,
            LastWorkingDate = r.LastWorkingDay,
            Reason = r.ResignationReason,
            Status = r.Status
        }).ToList();
    }

    public async Task<IReadOnlyList<AiSalaryAdvanceDto>> GetSalaryAdvancesAsync(string? status = null, int limit = 20, CancellationToken ct = default)
    {
        var query = _db.HrSalaryAdvances
            .AsNoTracking()
            .OrderByDescending(s => s.AdvanceDate);

        var filtered = string.IsNullOrWhiteSpace(status)
            ? query
            : query.Where(s => s.Status.ToLower() == status.ToLower());

        var items = await filtered.Take(limit).ToListAsync(ct);
        return items.Select(s => new AiSalaryAdvanceDto
        {
            AdvanceId = s.AdvanceId,
            AdvanceNo = s.AdvanceNo,
            EmployeeId = s.EmployeeId,
            EmployeeName = null,
            Amount = s.AdvanceAmount,
            Reason = s.Reason,
            Status = s.Status
        }).ToList();
    }

    public async Task<IReadOnlyList<AiTransferDto>> GetTransfersAsync(string? status = null, int limit = 20, CancellationToken ct = default)
    {
        var query = _db.HrTransfers
            .AsNoTracking()
            .OrderByDescending(t => t.TransferDate);

        var filtered = string.IsNullOrWhiteSpace(status)
            ? query
            : query.Where(t => t.Status.ToLower() == status.ToLower());

        var items = await filtered.Take(limit).ToListAsync(ct);
        return items.Select(t => new AiTransferDto
        {
            TransferId = t.TransferId,
            EmployeeId = t.EmployeeId,
            EmployeeName = null,
            FromDepartment = t.FromDeptId?.ToString(),
            ToDepartment = t.ToDeptId.ToString(),
            FromDesignation = null,
            ToDesignation = null,
            TransferDate = t.TransferDate,
            Status = t.Status
        }).ToList();
    }

    public async Task<IReadOnlyList<AiTravelExpenseDto>> GetTravelExpensesAsync(string? status = null, int limit = 20, CancellationToken ct = default)
    {
        var query = _db.HrTravelExpenses
            .AsNoTracking()
            .OrderByDescending(t => t.TravelDate);

        var filtered = string.IsNullOrWhiteSpace(status)
            ? query
            : query.Where(t => t.Status.ToLower() == status.ToLower());

        var items = await filtered.Take(limit).ToListAsync(ct);
        return items.Select(t => new AiTravelExpenseDto
        {
            TravelExpenseId = t.TravelId,
            EmployeeId = t.EmployeeId,
            EmployeeName = null,
            TravelPurpose = t.Purpose,
            FromDate = t.TravelDate,
            ToDate = t.ReturnDate,
            ClaimAmount = t.ClaimAmount,
            ApprovedAmount = t.ApprovedAmount,
            Status = t.Status
        }).ToList();
    }

    public async Task<IReadOnlyList<AiVacancyDto>> GetVacanciesAsync(string? status = null, int limit = 20, CancellationToken ct = default)
    {
        var query = _db.HrVacancies
            .AsNoTracking()
            .OrderByDescending(v => v.CreatedOn);

        var filtered = string.IsNullOrWhiteSpace(status)
            ? query
            : query.Where(v => v.Status.ToLower() == status.ToLower());

        var items = await filtered.Take(limit).ToListAsync(ct);
        return items.Select(v => new AiVacancyDto
        {
            VacancyId = v.VacancyId,
            VacancyTitle = v.VacancyNo,
            Department = v.DeptId.ToString(),
            Designation = v.DesignationId.ToString(),
            Positions = v.Positions,
            PostedDate = DateOnly.FromDateTime(v.CreatedOn),
            ClosingDate = v.TargetDate,
            Status = v.Status
        }).ToList();
    }

    // ══════════════════════════ NEW — Store Extended ══════════════════════════

    public async Task<IReadOnlyList<AiStockLedgerDto>> GetStockLedgerAsync(string? materialCode = null, int limit = 50, CancellationToken ct = default)
    {
        var query = _db.TrnStockLedgers
            .AsNoTracking()
            .OrderByDescending(s => s.TransactionDate);

        var filtered = string.IsNullOrWhiteSpace(materialCode)
            ? query
            : query.Where(s => s.MaterialCode != null && s.MaterialCode.ToLower() == materialCode.ToLower());

        var items = await filtered.Take(limit).ToListAsync(ct);
        return items.Select(s => new AiStockLedgerDto
        {
            StockLedgerId = s.LedgerId,
            MaterialCode = s.MaterialCode,
            MaterialName = s.MaterialName,
            TransactionType = s.TransactionType,
            Quantity = (s.QuantityIn ?? 0) - (s.QuantityOut ?? 0),
            Rate = s.Rate,
            Amount = s.Amount,
            TransactionDate = s.TransactionDate,
            ReferenceNo = s.ReferenceNo
        }).ToList();
    }

    public async Task<IReadOnlyList<AiChemicalDto>> GetAllChemicalsAsync(CancellationToken ct = default)
    {
        var chemicals = await _db.MstChemicals
            .AsNoTracking()
            .Where(c => c.IsActive == true)
            .OrderBy(c => c.ChemicalName)
            .ToListAsync(ct);

        return chemicals.Select(c => new AiChemicalDto
        {
            ChemicalId = 0,
            ChemicalName = c.ChemicalName,
            ChemicalType = c.ChemicalType,
            RatePerUnit = c.RatePerUnit,
            UnitOfMeasure = c.Uom,
            IsActive = c.IsActive ?? false
        }).ToList();
    }

    // ══════════════════════════ NEW — Accounting Extended ══════════════════════════

    public async Task<IReadOnlyList<AiCreditNoteDto>> GetAccountingCreditNotesAsync(string? status = null, int limit = 20, CancellationToken ct = default)
        => await GetCreditNotesAsync(status, limit, ct);

    public async Task<IReadOnlyList<AiDebitNoteDto>> GetAccountingDebitNotesAsync(string? status = null, int limit = 20, CancellationToken ct = default)
        => await GetDebitNotesAsync(status, limit, ct);

    public async Task<IReadOnlyList<AiBankReceiptDto>> GetBankReceiptsAsync(DateOnly? from = null, DateOnly? to = null, int limit = 20, CancellationToken ct = default)
    {
        var query = _db.TrnBankReceipts
            .Include(r => r.BankAccount)
            .AsNoTracking()
            .OrderByDescending(r => r.ReceiptDate);

        if (from.HasValue)
            query = (IOrderedQueryable<persistence.Models.TrnBankReceipt>)query.Where(r => r.ReceiptDate >= from.Value);
        if (to.HasValue)
            query = (IOrderedQueryable<persistence.Models.TrnBankReceipt>)query.Where(r => r.ReceiptDate <= to.Value);

        var items = await query.Take(limit).ToListAsync(ct);
        return items.Select(r => new AiBankReceiptDto
        {
            BankReceiptId = r.BankReceiptId,
            ReceiptNo = r.ReceiptNo,
            ReceiptDate = r.ReceiptDate,
            PartyName = r.ReceivedFrom,
            BankName = r.BankAccount.BankName,
            PaymentMode = r.PaymentMode,
            Amount = r.Amount,
            Status = r.Status
        }).ToList();
    }

    public async Task<IReadOnlyList<AiBankPaymentDto>> GetBankPaymentsAsync(DateOnly? from = null, DateOnly? to = null, int limit = 20, CancellationToken ct = default)
    {
        var query = _db.TrnBankPayments
            .Include(p => p.BankAccount)
            .AsNoTracking()
            .OrderByDescending(p => p.PaymentDate);

        if (from.HasValue)
            query = (IOrderedQueryable<persistence.Models.TrnBankPayment>)query.Where(p => p.PaymentDate >= from.Value);
        if (to.HasValue)
            query = (IOrderedQueryable<persistence.Models.TrnBankPayment>)query.Where(p => p.PaymentDate <= to.Value);

        var items = await query.Take(limit).ToListAsync(ct);
        return items.Select(p => new AiBankPaymentDto
        {
            BankPaymentId = p.BankPaymentId,
            PaymentNo = p.PaymentNo,
            PaymentDate = p.PaymentDate,
            PartyName = p.PaidTo,
            BankName = p.BankAccount.BankName,
            PaymentMode = p.PaymentMode,
            Amount = p.Amount,
            Status = p.Status
        }).ToList();
    }

    public async Task<IReadOnlyList<AiJournalVoucherDto>> GetJournalVouchersAsync(DateOnly? from = null, DateOnly? to = null, int limit = 20, CancellationToken ct = default)
    {
        var query = _db.TrnJournalVouchers
            .AsNoTracking()
            .OrderByDescending(j => j.JournalDate);

        if (from.HasValue)
            query = (IOrderedQueryable<persistence.Models.TrnJournalVoucher>)query.Where(j => j.JournalDate >= from.Value);
        if (to.HasValue)
            query = (IOrderedQueryable<persistence.Models.TrnJournalVoucher>)query.Where(j => j.JournalDate <= to.Value);

        var items = await query.Take(limit).ToListAsync(ct);
        return items.Select(j => new AiJournalVoucherDto
        {
            JournalVoucherId = j.JournalId,
            VoucherNo = j.JournalNo,
            VoucherDate = j.JournalDate,
            Narration = j.Narration,
            TotalDebit = j.TotalDebit ?? 0,
            TotalCredit = j.TotalCredit ?? 0,
            Status = j.Status
        }).ToList();
    }

    public async Task<IReadOnlyList<AiLedgerEntryDto>> GetLedgerEntriesAsync(string? accountHead = null, DateOnly? from = null, DateOnly? to = null, int limit = 50, CancellationToken ct = default)
    {
        var query = _db.TrnLedgers
            .Include(l => l.AccountHead)
            .AsNoTracking()
            .OrderByDescending(l => l.TransactionDate);

        if (!string.IsNullOrWhiteSpace(accountHead))
            query = (IOrderedQueryable<persistence.Models.TrnLedger>)query.Where(l => l.AccountHead.AccountName.ToLower().Contains(accountHead.ToLower()));
        if (from.HasValue)
            query = (IOrderedQueryable<persistence.Models.TrnLedger>)query.Where(l => l.TransactionDate >= from.Value);
        if (to.HasValue)
            query = (IOrderedQueryable<persistence.Models.TrnLedger>)query.Where(l => l.TransactionDate <= to.Value);

        var items = await query.Take(limit).ToListAsync(ct);
        return items.Select(l => new AiLedgerEntryDto
        {
            LedgerId = l.LedgerId,
            TransactionDate = l.TransactionDate,
            AccountHead = l.AccountHead.AccountName,
            Narration = l.Remarks,
            DebitAmount = l.DebitAmount ?? 0,
            CreditAmount = l.CreditAmount ?? 0,
            RunningBalance = (l.DebitAmount ?? 0) - (l.CreditAmount ?? 0),
            ReferenceNo = l.ReferenceNo,
            TransactionType = l.VoucherType != null ? l.VoucherType.ToString() : null
        }).ToList();
    }
}
