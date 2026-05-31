using erp.minepress.domain.Enums;
using erp.minepress.persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace erp.minepress.web.Services;

/// <summary>
/// Centralized implementation that generates document serial numbers
/// by calling press_db.fn_get_next_document_number in PostgreSQL.
/// </summary>
public class DocumentNumberService : IDocumentNumberService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<DocumentNumberService> _logger;

    public DocumentNumberService(ApplicationDbContext db, ILogger<DocumentNumberService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<string> GenerateNextNumberAsync(DocumentProcessCode processCode)
        => GenerateNextNumberAsync(processCode.ToString());

    /// <inheritdoc />
    public async Task<string> GenerateNextNumberAsync(string processCode)
    {
        try
        {
            var conn = _db.Database.GetDbConnection();
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT press_db.fn_get_next_document_number(@p_process_code)";

            var param = cmd.CreateParameter();
            param.ParameterName = "p_process_code";
            param.Value = processCode;
            cmd.Parameters.Add(param);

            var result = await cmd.ExecuteScalarAsync();
            return result?.ToString()
                ?? throw new InvalidOperationException($"Failed to generate document number for {processCode}.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "fn_get_next_document_number failed for {ProcessCode}. Using fallback.", processCode);

            // Fallback: use local sequence table
            var seq = await _db.MstDocumentSequences
                .FirstOrDefaultAsync(s => s.ProcessCode == processCode && s.IsActive == true);

            if (seq != null)
            {
                seq.CurrentNumber++;
                seq.UpdatedAt = DateTime.Now;
                var number = $"{seq.Prefix}{seq.CurrentNumber.ToString().PadLeft(seq.PaddingLength, '0')}{seq.Suffix}";
                await _db.SaveChangesAsync();
                return number;
            }

            return $"{processCode.Replace("_", "-")}-{DateTime.Now:yyyyMMddHHmmss}";
        }
    }
}
