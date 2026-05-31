using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class ErrorLog
{
    public long ErrorId { get; set; }

    public DateTime? ErrorTime { get; set; }

    public string? UserName { get; set; }

    public string? DatabaseName { get; set; }

    public string? FunctionName { get; set; }

    public string? ProcessCode { get; set; }

    public string? ErrorMessage { get; set; }

    public string? ErrorDetail { get; set; }

    public string? ErrorHint { get; set; }

    public string? ErrorContext { get; set; }

    public string? ErrorState { get; set; }

    public string? InputParameters { get; set; }

    public DateTime? CreatedAt { get; set; }
}
