using System.Text;
using System.Text.Json;
using erp.minepress.agentic.ai.Interfaces;
using Microsoft.Extensions.Logging;

namespace erp.minepress.agentic.ai.Services;

public class ResponseFormatter : IResponseFormatter
{
    private readonly ILogger<ResponseFormatter> _logger;

    public ResponseFormatter(ILogger<ResponseFormatter> logger)
    {
        _logger = logger;
    }

    public FormattedResponse Format(object? data, string outputFormat, string? message = null)
    {
        return outputFormat.ToLowerInvariant() switch
        {
            "table" => FormatAsTable(data, message),
            "text" => FormatAsText(data, message),
            _ => FormatAsText(data, message)
        };
    }

    private FormattedResponse FormatAsText(object? data, string? message)
    {
        if (data is null)
        {
            return new FormattedResponse
            {
                Format = "text",
                TextContent = message ?? "No data available.",
                RawData = data
            };
        }

        var sb = new StringBuilder();

        if (!string.IsNullOrEmpty(message))
            sb.AppendLine(message);

        try
        {
            using var doc = JsonDocument.Parse(JsonSerializer.Serialize(data));
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                if (prop.Name.Equals("message", StringComparison.OrdinalIgnoreCase))
                    continue;

                var displayName = FormatPropertyName(prop.Name);
                var value = prop.Value.ValueKind == JsonValueKind.Null ? "N/A" : prop.Value.ToString();
                sb.AppendLine($"{displayName}: {value}");
            }
        }
        catch
        {
            sb.AppendLine(data.ToString());
        }

        return new FormattedResponse
        {
            Format = "text",
            TextContent = sb.ToString().TrimEnd(),
            RawData = data
        };
    }

    private FormattedResponse FormatAsTable(object? data, string? message)
    {
        var table = new TableResponse();

        if (data is null)
        {
            return new FormattedResponse
            {
                Format = "table",
                TextContent = message ?? "No data available.",
                TableContent = table,
                RawData = data
            };
        }

        try
        {
            var json = JsonSerializer.Serialize(data);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.Array)
            {
                var first = true;
                foreach (var item in root.EnumerateArray())
                {
                    if (item.ValueKind != JsonValueKind.Object) continue;

                    if (first)
                    {
                        table.Headers = item.EnumerateObject()
                            .Select(p => FormatPropertyName(p.Name))
                            .ToList();
                        first = false;
                    }

                    table.Rows.Add(item.EnumerateObject()
                        .Select(p => p.Value.ValueKind == JsonValueKind.Null ? "N/A" : p.Value.ToString())
                        .ToList());
                }
            }
            else if (root.ValueKind == JsonValueKind.Object)
            {
                // Check if there's an array property (e.g., "data", "items", "breakdown")
                var arrayProp = root.EnumerateObject()
                    .FirstOrDefault(p => p.Value.ValueKind == JsonValueKind.Array);

                if (arrayProp.Value.ValueKind == JsonValueKind.Array && arrayProp.Value.GetArrayLength() > 0)
                {
                    var first = true;
                    foreach (var item in arrayProp.Value.EnumerateArray())
                    {
                        if (item.ValueKind != JsonValueKind.Object) continue;

                        if (first)
                        {
                            table.Headers = item.EnumerateObject()
                                .Select(p => FormatPropertyName(p.Name))
                                .ToList();
                            first = false;
                        }

                        table.Rows.Add(item.EnumerateObject()
                            .Select(p => p.Value.ValueKind == JsonValueKind.Null ? "N/A" : p.Value.ToString())
                            .ToList());
                    }
                }
                else
                {
                    // Single object → two-column table (Field, Value)
                    table.Headers = ["Field", "Value"];
                    foreach (var prop in root.EnumerateObject())
                    {
                        if (prop.Name.Equals("message", StringComparison.OrdinalIgnoreCase))
                            continue;

                        var value = prop.Value.ValueKind == JsonValueKind.Null ? "N/A" : prop.Value.ToString();
                        table.Rows.Add([FormatPropertyName(prop.Name), value]);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to format data as table");
            table.Headers = ["Data"];
            table.Rows.Add([data.ToString() ?? "N/A"]);
        }

        return new FormattedResponse
        {
            Format = "table",
            TextContent = message,
            TableContent = table,
            RawData = data
        };
    }

    private static string FormatPropertyName(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;

        var sb = new StringBuilder();
        sb.Append(char.ToUpper(name[0]));

        for (var i = 1; i < name.Length; i++)
        {
            if (char.IsUpper(name[i]) && i > 0 && char.IsLower(name[i - 1]))
                sb.Append(' ');
            sb.Append(name[i]);
        }

        return sb.ToString();
    }
}
