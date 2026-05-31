using erp.minepress.application.Common.Interfaces;
using erp.minepress.domain.Paper;
using erp.minepress.persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace erp.minepress.persistence.Repositories;

public class PaperRepository : Repository<PaperEntity, long>, IPaperRepository
{
    public PaperRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<PaperEntity>> GetByGsmAsync(int gsm, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking()
            .Where(x => x.Gsm == gsm && x.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PaperEntity>> GetCompatiblePapersAsync(
        string? jobType, string? usage,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking().Where(x => x.IsActive);

        if (!string.IsNullOrEmpty(jobType))
            query = query.Where(x => x.SupportedJobTypes != null && x.SupportedJobTypes.Contains(jobType));

        if (!string.IsNullOrEmpty(usage))
            query = query.Where(x => x.SupportedUsage != null && x.SupportedUsage.Contains(usage));

        return await query.ToListAsync(cancellationToken);
    }
}
