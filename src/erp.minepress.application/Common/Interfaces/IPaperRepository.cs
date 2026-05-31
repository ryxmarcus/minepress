using erp.minepress.domain.Paper;

namespace erp.minepress.application.Common.Interfaces;

public interface IPaperRepository : IRepository<PaperEntity, long>
{
    Task<IReadOnlyList<PaperEntity>> GetByGsmAsync(int gsm, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaperEntity>> GetCompatiblePapersAsync(string? jobType, string? usage, CancellationToken cancellationToken = default);
}
