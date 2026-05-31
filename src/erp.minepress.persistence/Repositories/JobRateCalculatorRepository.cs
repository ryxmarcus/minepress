using erp.minepress.application.Common.Interfaces;
using erp.minepress.domain.Job;
using erp.minepress.persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace erp.minepress.persistence.Repositories;

public class JobRateCalculatorRepository : Repository<JobRateCalculatorEntity, long>, IJobRateCalculatorRepository
{
    public JobRateCalculatorRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<JobRateCalculatorEntity?> GetByRefNoAsync(string calcRefNo, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(x => x.CalcRefNo == calcRefNo, cancellationToken);
    }

    public async Task<IReadOnlyList<JobRateCalculatorEntity>> GetByPartyIdAsync(int partyId, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().Where(x => x.PartyId == partyId).OrderByDescending(x => x.CreatedOn).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<JobRateCalculatorEntity>> GetByStatusAsync(string status, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().Where(x => x.Status == status).OrderByDescending(x => x.CreatedOn).ToListAsync(cancellationToken);
    }

    public Task<string> GenerateRefNoAsync(CancellationToken cancellationToken = default)
    {
        var refNo = $"RC-{DateTime.UtcNow:yyyyMMdd-HHmmss}";
        return Task.FromResult(refNo);
    }
}
