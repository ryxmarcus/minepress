using erp.minepress.domain.Job;

namespace erp.minepress.application.Common.Interfaces;

public interface IJobRateCalculatorRepository : IRepository<JobRateCalculatorEntity, long>
{
    Task<JobRateCalculatorEntity?> GetByRefNoAsync(string calcRefNo, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<JobRateCalculatorEntity>> GetByPartyIdAsync(int partyId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<JobRateCalculatorEntity>> GetByStatusAsync(string status, CancellationToken cancellationToken = default);
    Task<string> GenerateRefNoAsync(CancellationToken cancellationToken = default);
}
