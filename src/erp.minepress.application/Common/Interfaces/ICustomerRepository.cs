using erp.minepress.domain.Customer;

namespace erp.minepress.application.Common.Interfaces;

public interface ICustomerRepository : IRepository<CustomerEntity, int>
{
    Task<CustomerEntity?> GetByPartyIdAsync(int partyId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CustomerEntity>> GetActiveCustomersAsync(CancellationToken cancellationToken = default);
}
