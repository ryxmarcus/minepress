using erp.minepress.agentic.ai.Models;

namespace erp.minepress.agentic.ai.Interfaces;

public interface IResponseFormatter
{
    FormattedResponse Format(object? data, string outputFormat, string? message = null);
}

public class FormattedResponse
{
    public string Format { get; set; } = "text";
    public string? TextContent { get; set; }
    public TableResponse? TableContent { get; set; }
    public object? RawData { get; set; }
}

public class TableResponse
{
    public List<string> Headers { get; set; } = [];
    public List<List<string>> Rows { get; set; } = [];
}
