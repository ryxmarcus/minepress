using erp.minepress.agentic.ai.Models;
using erp.minepress.bff.service.Interfaces;
using Microsoft.Extensions.Logging;

namespace erp.minepress.agentic.ai.Agents;

public class HRAgent : BaseAgent
{
    private readonly IAiDataService _data;

    public HRAgent(ILogger<HRAgent> logger, IAiDataService data) : base(logger)
    {
        _data = data;
    }

    public override string AgentName => "HRAgent";

    public override IReadOnlyList<string> SupportedIntents =>
        ["get_employees", "search_employee", "get_employee_details", "get_employees_by_department",
         "get_leave_requests", "get_employee_leaves", "get_attendance",
         "get_loans", "get_overtimes", "get_reimbursements", "get_hr_summary"];

    public override Task<AgentResult> ExecuteAsync(string tool, Dictionary<string, object?> parameters, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("HRAgent executing tool {Tool}", tool);

        if (ToolMatches(tool, "GetAllEmployees")) return GetAllEmployeesAsync(parameters, cancellationToken);
        if (ToolMatches(tool, "SearchEmployee")) return SearchEmployeeAsync(parameters, cancellationToken);
        if (ToolMatches(tool, "GetEmployeeDetails")) return GetEmployeeDetailsAsync(parameters, cancellationToken);
        if (ToolMatches(tool, "GetEmployeesByDepartment")) return GetEmployeesByDepartmentAsync(parameters, cancellationToken);
        if (ToolMatches(tool, "GetLeaveRequests")) return GetLeaveRequestsAsync(parameters, cancellationToken);
        if (ToolMatches(tool, "GetEmployeeLeaves")) return GetEmployeeLeavesAsync(parameters, cancellationToken);
        if (ToolMatches(tool, "GetAttendance")) return GetAttendanceAsync(parameters, cancellationToken);
        if (ToolMatches(tool, "GetLoans")) return GetLoansAsync(parameters, cancellationToken);
        if (ToolMatches(tool, "GetOvertimes")) return GetOvertimesAsync(parameters, cancellationToken);
        if (ToolMatches(tool, "GetReimbursements")) return GetReimbursementsAsync(parameters, cancellationToken);
        if (ToolMatches(tool, "GetHRSummary")) return GetHRSummaryAsync(cancellationToken);

        return Task.FromResult(AgentResult.Fail($"Unknown tool: {tool}"));
    }

    private async Task<AgentResult> GetAllEmployeesAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var limit = GetIntParameter(parameters, "limit", 50);
        var employees = await _data.GetAllEmployeesAsync(limit, ct);
        return AgentResult.Ok(employees, "GetAllEmployees", $"Found {employees.Count} employee(s)");
    }

    private async Task<AgentResult> SearchEmployeeAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var keyword = GetStringParameter(parameters, "keyword") ?? GetStringParameter(parameters, "employeeName");
        if (string.IsNullOrEmpty(keyword))
            return AgentResult.Fail("Missing required parameter: keyword or employeeName");

        var employees = await _data.SearchEmployeesAsync(keyword, ct);
        return employees.Count > 0
            ? AgentResult.Ok(employees, "SearchEmployee", $"Found {employees.Count} employee(s) matching '{keyword}'")
            : AgentResult.Fail($"No employees found matching '{keyword}'");
    }

    private async Task<AgentResult> GetEmployeeDetailsAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var employeeId = GetIntParameter(parameters, "employeeId");
        if (employeeId <= 0)
            return AgentResult.Fail("Missing required parameter: employeeId");

        var employee = await _data.GetEmployeeByIdAsync(employeeId, ct);
        return employee is not null
            ? AgentResult.Ok(employee, "GetEmployeeDetails", $"Employee: {employee.FullName} ({employee.EmpCode}) — {employee.Department}")
            : AgentResult.Fail($"Employee with ID {employeeId} not found");
    }

    private async Task<AgentResult> GetEmployeesByDepartmentAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var department = GetStringParameter(parameters, "department");
        if (string.IsNullOrEmpty(department))
            return AgentResult.Fail("Missing required parameter: department");

        var employees = await _data.GetEmployeesByDepartmentAsync(department, ct);
        return AgentResult.Ok(employees, "GetEmployeesByDepartment", $"Found {employees.Count} employee(s) in '{department}' department");
    }

    private async Task<AgentResult> GetLeaveRequestsAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var status = GetStringParameter(parameters, "status");
        var limit = GetIntParameter(parameters, "limit", 20);
        var leaves = await _data.GetLeaveRequestsAsync(status, limit, ct);

        var statusInfo = string.IsNullOrEmpty(status) ? "" : $" with status '{status}'";
        return AgentResult.Ok(leaves, "GetLeaveRequests", $"Found {leaves.Count} leave request(s){statusInfo}");
    }

    private async Task<AgentResult> GetEmployeeLeavesAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var employeeId = GetIntParameter(parameters, "employeeId");
        if (employeeId <= 0)
            return AgentResult.Fail("Missing required parameter: employeeId");

        var leaves = await _data.GetEmployeeLeaveRequestsAsync(employeeId, ct);
        return AgentResult.Ok(leaves, "GetEmployeeLeaves", $"Found {leaves.Count} leave request(s) for employee");
    }

    private async Task<AgentResult> GetAttendanceAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var employeeId = GetIntParameter(parameters, "employeeId");
        var dateStr = GetStringParameter(parameters, "date");
        var limit = GetIntParameter(parameters, "limit", 50);

        long? empId = employeeId > 0 ? employeeId : null;
        DateOnly? date = DateOnly.TryParse(dateStr, out var d) ? d : null;

        var attendance = await _data.GetAttendanceAsync(empId, date, limit, ct);
        return AgentResult.Ok(attendance, "GetAttendance", $"Found {attendance.Count} attendance record(s)");
    }

    private async Task<AgentResult> GetLoansAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var status = GetStringParameter(parameters, "status");
        var limit = GetIntParameter(parameters, "limit", 20);
        var loans = await _data.GetLoansAsync(status, limit, ct);
        return AgentResult.Ok(loans, "GetLoans", $"Found {loans.Count} loan(s)");
    }

    private async Task<AgentResult> GetOvertimesAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var employeeId = GetIntParameter(parameters, "employeeId");
        var limit = GetIntParameter(parameters, "limit", 20);

        long? empId = employeeId > 0 ? employeeId : null;

        var overtimes = await _data.GetOvertimesAsync(empId, limit, ct);
        return AgentResult.Ok(overtimes, "GetOvertimes", $"Found {overtimes.Count} overtime record(s)");
    }

    private async Task<AgentResult> GetReimbursementsAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var status = GetStringParameter(parameters, "status");
        var limit = GetIntParameter(parameters, "limit", 20);
        var reimbursements = await _data.GetReimbursementsAsync(status, limit, ct);
        return AgentResult.Ok(reimbursements, "GetReimbursements", $"Found {reimbursements.Count} reimbursement(s)");
    }

    private async Task<AgentResult> GetHRSummaryAsync(CancellationToken ct)
    {
        var summary = await _data.GetHrSummaryAsync(ct);
        return AgentResult.Ok(summary, "GetHRSummary",
            $"HR Summary: {summary.ActiveEmployees}/{summary.TotalEmployees} active employees, {summary.PendingLeaves} pending leaves, {summary.ActiveLoans} active loans");
    }
}
