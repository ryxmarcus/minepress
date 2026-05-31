using erp.minepress.infrastructure.ErrorLogging;
using erp.minepress.persistence.Context;
using erp.minepress.persistence.Models;
using erp.minepress.web.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace erp.minepress.web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkflowController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<WorkflowController> _logger;
    private readonly ISystemErrorLogger _systemErrorLogger;

    public WorkflowController(ApplicationDbContext db, ILogger<WorkflowController> logger, ISystemErrorLogger systemErrorLogger)
    {
        _db = db;
        _logger = logger;
        _systemErrorLogger = systemErrorLogger;
    }

    private async Task AuditExceptionAsync(Exception ex, string additionalData, string severity = "Error")
        => await _systemErrorLogger.LogAsync(ex, HttpContext, severity, additionalData);

    // ── Lookups ──
    [HttpGet("lookups")]
    public async Task<IActionResult> GetLookups()
    {
        var jobTypes = await _db.MstJobTypes
            .Where(j => j.Isactive == true)
            .OrderBy(j => j.Jobtypename)
            .Select(j => new { id = j.Jobtypeid, code = j.Jobtypecode, name = j.Jobtypename })
            .ToListAsync();

        var productTypes = await _db.MstPrintProductTypes
            .Where(p => p.Isactive == true)
            .OrderBy(p => p.Productname)
            .Select(p => new { id = p.Printproducttypeid, code = p.Productcode, name = p.Productname, category = p.Category })
            .ToListAsync();

        var processes = await _db.MstProcesses
            .Where(p => p.Isactive)
            .OrderBy(p => p.Sequenceno)
            .Select(p => new { id = p.Processid, code = p.Processcode, name = p.Processname, departmentId = p.Departmentid })
            .ToListAsync();

        var departments = await _db.MstDepartments
            .Where(d => d.IsActive == true)
            .OrderBy(d => d.DeptName)
            .Select(d => new { id = d.DeptId, code = d.DeptCode, name = d.DeptName, isProduction = d.IsProduction })
            .ToListAsync();

        var users = await _db.MstUsers
            .Where(u => u.Isactive == true)
            .OrderBy(u => u.Name)
            .Select(u => new { id = u.Userid, code = u.Usercode, name = u.Name, departmentId = u.Departmentid, isApprovalUser = u.Isapprovaluser })
            .ToListAsync();

        var approvalTypes = await _db.MstApprovalTypes
            .Where(a => a.Isactive == true)
            .OrderBy(a => a.Approvalname)
            .Select(a => new { id = a.Approvaltypeid, code = a.Approvalcode, name = a.Approvalname })
            .ToListAsync();

        var approvalLevels = await _db.MstApprovalLevels
            .Where(a => a.Isactive == true)
            .OrderBy(a => a.Sequenceno)
            .Select(a => new { id = a.Approvallevelid, name = a.Levelname, sequence = a.Sequenceno })
            .ToListAsync();

        return Ok(new
        {
            jobTypes,
            productTypes,
            processes,
            departments,
            users,
            approvalTypes,
            approvalLevels
        });
    }

    // ── Template List ──
    [HttpGet("templates")]
    public async Task<IActionResult> GetTemplates()
    {
        var list = await _db.MstWorkflowTemplates
            .Include(t => t.JobType)
            .Include(t => t.PrintProductType)
            .Where(t => t.IsActive)
            .OrderByDescending(t => t.CreatedOn)
            .Select(t => new
            {
                t.WorkflowTemplateId,
                t.WorkflowCode,
                t.WorkflowName,
                t.Description,
                t.JobTypeId,
                jobTypeName = t.JobType != null ? t.JobType.Jobtypename : "",
                t.PrintProductTypeId,
                productTypeName = t.PrintProductType != null ? t.PrintProductType.Productname : "",
                t.IsDefault,
                t.Version,
                t.IsActive,
                t.CreatedBy,
                createdOn = t.CreatedOn.HasValue ? t.CreatedOn.Value.ToString("dd-MMM-yyyy HH:mm") : "",
                stepCount = t.MstWorkflowSteps.Count(s => s.IsActive),
                connectionCount = t.MstWorkflowConnections.Count(c => c.IsActive)
            })
            .ToListAsync();

        return Ok(list);
    }

    // ── Get Single Template with Steps & Connections ──
    [HttpGet("templates/{id}")]
    public async Task<IActionResult> GetTemplate(long id)
    {
        var template = await _db.MstWorkflowTemplates
            .Include(t => t.MstWorkflowSteps.Where(s => s.IsActive))
                .ThenInclude(s => s.Department)
            .Include(t => t.MstWorkflowSteps.Where(s => s.IsActive))
                .ThenInclude(s => s.AssignedUser)
            .Include(t => t.MstWorkflowSteps.Where(s => s.IsActive))
                .ThenInclude(s => s.Process)
            .Include(t => t.MstWorkflowSteps.Where(s => s.IsActive))
                .ThenInclude(s => s.ApprovalType)
            .Include(t => t.MstWorkflowSteps.Where(s => s.IsActive))
                .ThenInclude(s => s.ApprovalLevel)
            .Include(t => t.MstWorkflowConnections.Where(c => c.IsActive))
            .Include(t => t.JobType)
            .Include(t => t.PrintProductType)
            .FirstOrDefaultAsync(t => t.WorkflowTemplateId == id);

        if (template == null)
            return NotFound(new { message = "Workflow template not found." });

        return Ok(new
        {
            template.WorkflowTemplateId,
            template.WorkflowCode,
            template.WorkflowName,
            template.Description,
            template.JobTypeId,
            jobTypeName = template.JobType?.Jobtypename,
            template.PrintProductTypeId,
            productTypeName = template.PrintProductType?.Productname,
            template.IsDefault,
            template.Version,
            template.IsActive,
            steps = template.MstWorkflowSteps.OrderBy(s => s.SequenceNo).Select(s => new
            {
                s.WorkflowStepId,
                s.StepCode,
                s.StepName,
                s.StepType,
                s.SequenceNo,
                s.ProcessId,
                processName = s.Process?.Processname,
                s.SubProcessId,
                s.DepartmentId,
                departmentName = s.Department?.DeptName,
                s.AssignedUserId,
                assignedUserName = s.AssignedUser?.Name,
                s.AssignmentRule,
                s.ApprovalTypeId,
                approvalTypeName = s.ApprovalType?.Approvalname,
                s.ApprovalLevelId,
                approvalLevelName = s.ApprovalLevel?.Levelname,
                s.IsMandatory,
                s.SlaHours,
                s.EscalateAfterHours,
                s.EscalateTo,
                s.NotifyVendor,
                s.NotifySupplier,
                s.NotifyCustomer,
                s.NotifyAssignedUser,
                s.NotifyDeptHead,
                s.SendEmail,
                s.SendSms,
                s.SendWhatsapp,
                s.SendPushNotification,
                s.CanvasX,
                s.CanvasY,
                s.NodeColor
            }),
            connections = template.MstWorkflowConnections.OrderBy(c => c.SequenceNo).Select(c => new
            {
                c.ConnectionId,
                c.FromStepId,
                c.ToStepId,
                c.ConditionExpression,
                c.Label,
                c.SequenceNo
            })
        });
    }

    // ── Create Template ──
    [HttpPost("templates")]
    public async Task<IActionResult> CreateTemplate([FromBody] WorkflowTemplateSaveDto dto)
    {
        var user = HttpContext.Session.GetObject<UserSessionData>("CurrentUser");
        var userName = user?.Name ?? "System";

        var exists = await _db.MstWorkflowTemplates.AnyAsync(t => t.WorkflowCode == dto.WorkflowCode && t.IsActive);
        if (exists)
            return BadRequest(new { message = $"Workflow code '{dto.WorkflowCode}' already exists." });

        var template = new MstWorkflowTemplate
        {
            WorkflowCode = dto.WorkflowCode,
            WorkflowName = dto.WorkflowName,
            Description = dto.Description,
            JobTypeId = dto.JobTypeId,
            PrintProductTypeId = dto.PrintProductTypeId,
            IsDefault = dto.IsDefault,
            Version = 1,
            IsActive = true,
            CreatedBy = userName,
            CreatedOn = DateTime.Now
        };

        _db.MstWorkflowTemplates.Add(template);
        await _db.SaveChangesAsync();

        if (dto.Steps != null)
        {
            var stepIdMap = new Dictionary<string, long>();
            foreach (var stepDto in dto.Steps)
            {
                var step = MapStepFromDto(stepDto, template.WorkflowTemplateId, userName);
                _db.MstWorkflowSteps.Add(step);
                await _db.SaveChangesAsync();
                stepIdMap[stepDto.TempId ?? stepDto.StepCode] = step.WorkflowStepId;
            }

            if (dto.Connections != null)
            {
                foreach (var connDto in dto.Connections)
                {
                    var fromId = stepIdMap.GetValueOrDefault(connDto.FromTempId);
                    var toId = stepIdMap.GetValueOrDefault(connDto.ToTempId);
                    if (fromId == 0 || toId == 0) continue;

                    _db.MstWorkflowConnections.Add(new MstWorkflowConnection
                    {
                        WorkflowTemplateId = template.WorkflowTemplateId,
                        FromStepId = fromId,
                        ToStepId = toId,
                        ConditionExpression = connDto.ConditionExpression,
                        Label = connDto.Label,
                        SequenceNo = connDto.SequenceNo,
                        IsActive = true
                    });
                }
                await _db.SaveChangesAsync();
            }
        }

        return Ok(new { id = template.WorkflowTemplateId, message = "Workflow created successfully." });
    }

    // ── Update Template ──
    [HttpPut("templates/{id}")]
    public async Task<IActionResult> UpdateTemplate(long id, [FromBody] WorkflowTemplateSaveDto dto)
    {
        var user = HttpContext.Session.GetObject<UserSessionData>("CurrentUser");
        var userName = user?.Name ?? "System";

        var template = await _db.MstWorkflowTemplates
            .Include(t => t.MstWorkflowSteps)
            .Include(t => t.MstWorkflowConnections)
            .FirstOrDefaultAsync(t => t.WorkflowTemplateId == id);

        if (template == null)
            return NotFound(new { message = "Workflow template not found." });

        template.WorkflowName = dto.WorkflowName;
        template.Description = dto.Description;
        template.JobTypeId = dto.JobTypeId;
        template.PrintProductTypeId = dto.PrintProductTypeId;
        template.IsDefault = dto.IsDefault;
        template.Version += 1;
        template.ModifiedBy = userName;
        template.ModifiedOn = DateTime.Now;

        _db.MstWorkflowConnections.RemoveRange(template.MstWorkflowConnections);
        _db.MstWorkflowSteps.RemoveRange(template.MstWorkflowSteps);
        await _db.SaveChangesAsync();

        var stepIdMap = new Dictionary<string, long>();
        if (dto.Steps != null)
        {
            foreach (var stepDto in dto.Steps)
            {
                var step = MapStepFromDto(stepDto, template.WorkflowTemplateId, userName);
                _db.MstWorkflowSteps.Add(step);
                await _db.SaveChangesAsync();
                stepIdMap[stepDto.TempId ?? stepDto.StepCode] = step.WorkflowStepId;
            }
        }

        if (dto.Connections != null)
        {
            foreach (var connDto in dto.Connections)
            {
                var fromId = stepIdMap.GetValueOrDefault(connDto.FromTempId);
                var toId = stepIdMap.GetValueOrDefault(connDto.ToTempId);
                if (fromId == 0 || toId == 0) continue;

                _db.MstWorkflowConnections.Add(new MstWorkflowConnection
                {
                    WorkflowTemplateId = template.WorkflowTemplateId,
                    FromStepId = fromId,
                    ToStepId = toId,
                    ConditionExpression = connDto.ConditionExpression,
                    Label = connDto.Label,
                    SequenceNo = connDto.SequenceNo,
                    IsActive = true
                });
            }
            await _db.SaveChangesAsync();
        }

        return Ok(new { id = template.WorkflowTemplateId, message = "Workflow updated successfully." });
    }

    // ── Delete Template (soft) ──
    [HttpDelete("templates/{id}")]
    public async Task<IActionResult> DeleteTemplate(long id)
    {
        var template = await _db.MstWorkflowTemplates.FindAsync(id);
        if (template == null)
            return NotFound(new { message = "Workflow template not found." });

        template.IsActive = false;
        template.ModifiedOn = DateTime.Now;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Workflow deleted successfully." });
    }

    // ── Duplicate Template ──
    [HttpPost("templates/{id}/duplicate")]
    public async Task<IActionResult> DuplicateTemplate(long id)
    {
        var user = HttpContext.Session.GetObject<UserSessionData>("CurrentUser");
        var userName = user?.Name ?? "System";

        var source = await _db.MstWorkflowTemplates
            .Include(t => t.MstWorkflowSteps.Where(s => s.IsActive))
            .Include(t => t.MstWorkflowConnections.Where(c => c.IsActive))
            .FirstOrDefaultAsync(t => t.WorkflowTemplateId == id);

        if (source == null)
            return NotFound(new { message = "Workflow template not found." });

        var newCode = source.WorkflowCode + "-COPY";
        var copyNum = 1;
        while (await _db.MstWorkflowTemplates.AnyAsync(t => t.WorkflowCode == newCode && t.IsActive))
        {
            copyNum++;
            newCode = $"{source.WorkflowCode}-COPY{copyNum}";
        }

        var newTemplate = new MstWorkflowTemplate
        {
            WorkflowCode = newCode,
            WorkflowName = source.WorkflowName + " (Copy)",
            Description = source.Description,
            JobTypeId = source.JobTypeId,
            PrintProductTypeId = source.PrintProductTypeId,
            IsDefault = false,
            Version = 1,
            IsActive = true,
            CreatedBy = userName,
            CreatedOn = DateTime.Now
        };
        _db.MstWorkflowTemplates.Add(newTemplate);
        await _db.SaveChangesAsync();

        var stepIdMap = new Dictionary<long, long>();
        foreach (var step in source.MstWorkflowSteps.OrderBy(s => s.SequenceNo))
        {
            var newStep = new MstWorkflowStep
            {
                WorkflowTemplateId = newTemplate.WorkflowTemplateId,
                ProcessId = step.ProcessId,
                SubProcessId = step.SubProcessId,
                StepCode = step.StepCode,
                StepName = step.StepName,
                StepType = step.StepType,
                SequenceNo = step.SequenceNo,
                DepartmentId = step.DepartmentId,
                AssignedUserId = step.AssignedUserId,
                AssignmentRule = step.AssignmentRule,
                ApprovalTypeId = step.ApprovalTypeId,
                ApprovalLevelId = step.ApprovalLevelId,
                IsMandatory = step.IsMandatory,
                SlaHours = step.SlaHours,
                EscalateAfterHours = step.EscalateAfterHours,
                EscalateTo = step.EscalateTo,
                NotifyVendor = step.NotifyVendor,
                NotifySupplier = step.NotifySupplier,
                NotifyCustomer = step.NotifyCustomer,
                NotifyAssignedUser = step.NotifyAssignedUser,
                NotifyDeptHead = step.NotifyDeptHead,
                SendEmail = step.SendEmail,
                SendSms = step.SendSms,
                SendWhatsapp = step.SendWhatsapp,
                SendPushNotification = step.SendPushNotification,
                CanvasX = step.CanvasX,
                CanvasY = step.CanvasY,
                NodeColor = step.NodeColor,
                IsActive = true,
                CreatedBy = userName,
                CreatedOn = DateTime.Now
            };
            _db.MstWorkflowSteps.Add(newStep);
            await _db.SaveChangesAsync();
            stepIdMap[step.WorkflowStepId] = newStep.WorkflowStepId;
        }

        foreach (var conn in source.MstWorkflowConnections)
        {
            if (!stepIdMap.ContainsKey(conn.FromStepId) || !stepIdMap.ContainsKey(conn.ToStepId)) continue;
            _db.MstWorkflowConnections.Add(new MstWorkflowConnection
            {
                WorkflowTemplateId = newTemplate.WorkflowTemplateId,
                FromStepId = stepIdMap[conn.FromStepId],
                ToStepId = stepIdMap[conn.ToStepId],
                ConditionExpression = conn.ConditionExpression,
                Label = conn.Label,
                SequenceNo = conn.SequenceNo,
                IsActive = true
            });
        }
        await _db.SaveChangesAsync();

        return Ok(new { id = newTemplate.WorkflowTemplateId, message = "Workflow duplicated successfully." });
    }

    // ── AI Suggest ──
    [HttpPost("ai-suggest")]
    public async Task<IActionResult> AiSuggest([FromBody] AiSuggestDto dto)
    {
        var processes = await _db.MstProcesses
            .Where(p => p.Isactive)
            .OrderBy(p => p.Sequenceno)
            .Include(p => p.Department)
            
            .ToListAsync();

        MstJobType? jobType = null;
        if (dto.JobTypeId.HasValue)
            jobType = await _db.MstJobTypes.FindAsync(dto.JobTypeId.Value);

        var suggestions = new List<object>();
        int seq = 1;

        suggestions.Add(new { stepType = "START", stepName = "Start", stepCode = "START", sequenceNo = seq++, canvasX = 400.0, canvasY = 50.0, nodeColor = "#4CAF50" });

        foreach (var proc in processes)
        {
            bool include = true;
            if (jobType != null)
            {
                if (proc.Processcode.Contains("DESIGN", StringComparison.OrdinalIgnoreCase) && jobType.Isdesignrequired != true) include = false;
                if (proc.Processcode.Contains("PRINT", StringComparison.OrdinalIgnoreCase) && jobType.Isprintingrequired != true) include = false;
                if (proc.Processcode.Contains("BIND", StringComparison.OrdinalIgnoreCase) && jobType.Isbindingrequired != true) include = false;
                if (proc.Processcode.Contains("FINISH", StringComparison.OrdinalIgnoreCase) && jobType.Isfinishingrequired != true) include = false;
            }

            if (!include) continue;

            double yPos = 50.0 + seq * 120.0;

            suggestions.Add(new
            {
                stepType = "PROCESS",
                stepName = proc.Processname,
                stepCode = proc.Processcode,
                sequenceNo = seq++,
                processId = proc.Processid,
                departmentId = proc.Departmentid,
                departmentName = proc.Department?.DeptName,
                canvasX = 400.0,
                canvasY = yPos,
                nodeColor = "#2196F3",
                isMandatory = proc.Ismandatory ?? false,
                assignmentRule = "AUTO"
            });

            if (proc.Isapprovalrequired == true)
            {
                yPos = 50.0 + seq * 120.0;
                suggestions.Add(new
                {
                    stepType = "APPROVAL",
                    stepName = proc.Processname + " Approval",
                    stepCode = proc.Processcode + "_APPROVAL",
                    sequenceNo = seq++,
                    processId = proc.Processid,
                    departmentId = proc.Departmentid,
                    departmentName = proc.Department?.DeptName,
                    canvasX = 400.0,
                    canvasY = yPos,
                    nodeColor = "#FF9800",
                    isMandatory = true,
                    assignmentRule = "DEPT_HEAD"
                });
            }

            }

        suggestions.Add(new { stepType = "END", stepName = "End", stepCode = "END", sequenceNo = seq++, canvasX = 400.0, canvasY = 50.0 + seq * 120.0, nodeColor = "#607D8B" });

        return Ok(new { suggestions, message = $"AI suggested {suggestions.Count} steps based on job type configuration." });
    }

    // ── Private helpers ──
    private static MstWorkflowStep MapStepFromDto(WorkflowStepSaveDto dto, long templateId, string userName)
    {
        return new MstWorkflowStep
        {
            WorkflowTemplateId = templateId,
            ProcessId = dto.ProcessId,
            SubProcessId = dto.SubProcessId,
            StepCode = dto.StepCode,
            StepName = dto.StepName,
            StepType = dto.StepType,
            SequenceNo = dto.SequenceNo,
            DepartmentId = dto.DepartmentId,
            AssignedUserId = dto.AssignedUserId,
            AssignmentRule = dto.AssignmentRule,
            ApprovalTypeId = dto.ApprovalTypeId,
            ApprovalLevelId = dto.ApprovalLevelId,
            IsMandatory = dto.IsMandatory,
            SlaHours = dto.SlaHours,
            EscalateAfterHours = dto.EscalateAfterHours,
            EscalateTo = dto.EscalateTo,
            NotifyVendor = dto.NotifyVendor,
            NotifySupplier = dto.NotifySupplier,
            NotifyCustomer = dto.NotifyCustomer,
            NotifyAssignedUser = dto.NotifyAssignedUser,
            NotifyDeptHead = dto.NotifyDeptHead,
            SendEmail = dto.SendEmail,
            SendSms = dto.SendSms,
            SendWhatsapp = dto.SendWhatsapp,
            SendPushNotification = dto.SendPushNotification,
            CanvasX = dto.CanvasX,
            CanvasY = dto.CanvasY,
            NodeColor = dto.NodeColor,
            IsActive = true,
            CreatedBy = userName,
            CreatedOn = DateTime.Now
        };
    }
}

// ── DTOs ──
public class WorkflowTemplateSaveDto
{
    public string WorkflowCode { get; set; } = "";
    public string WorkflowName { get; set; } = "";
    public string? Description { get; set; }
    public int? JobTypeId { get; set; }
    public int? PrintProductTypeId { get; set; }
    public bool IsDefault { get; set; }
    public List<WorkflowStepSaveDto>? Steps { get; set; }
    public List<WorkflowConnectionSaveDto>? Connections { get; set; }
}

public class WorkflowStepSaveDto
{
    public string? TempId { get; set; }
    public int? ProcessId { get; set; }
    public int? SubProcessId { get; set; }
    public string StepCode { get; set; } = "";
    public string StepName { get; set; } = "";
    public string StepType { get; set; } = "PROCESS";
    public int SequenceNo { get; set; }
    public long? DepartmentId { get; set; }
    public long? AssignedUserId { get; set; }
    public string? AssignmentRule { get; set; }
    public int? ApprovalTypeId { get; set; }
    public int? ApprovalLevelId { get; set; }
    public bool IsMandatory { get; set; }
    public decimal? SlaHours { get; set; }
    public decimal? EscalateAfterHours { get; set; }
    public string? EscalateTo { get; set; }
    public bool NotifyVendor { get; set; }
    public bool NotifySupplier { get; set; }
    public bool NotifyCustomer { get; set; }
    public bool NotifyAssignedUser { get; set; }
    public bool NotifyDeptHead { get; set; }
    public bool SendEmail { get; set; }
    public bool SendSms { get; set; }
    public bool SendWhatsapp { get; set; }
    public bool SendPushNotification { get; set; }
    public double CanvasX { get; set; }
    public double CanvasY { get; set; }
    public string? NodeColor { get; set; }
}

public class WorkflowConnectionSaveDto
{
    public string FromTempId { get; set; } = "";
    public string ToTempId { get; set; } = "";
    public string? ConditionExpression { get; set; }
    public string? Label { get; set; }
    public int SequenceNo { get; set; }
}

public class AiSuggestDto
{
    public int? JobTypeId { get; set; }
    public int? PrintProductTypeId { get; set; }
}
