using erp.minepress.domain.Enums;

namespace erp.minepress.web.Services;

/// <summary>
/// Centralized service for generating document serial numbers
/// via press_db.fn_get_next_document_number.
/// </summary>
public interface IDocumentNumberService
{
    /// <summary>
    /// Generates the next document number for the given process code.
    /// </summary>
    Task<string> GenerateNextNumberAsync(DocumentProcessCode processCode);

    /// <summary>
    /// Generates the next document number for the given process code string.
    /// </summary>
    Task<string> GenerateNextNumberAsync(string processCode);
}
