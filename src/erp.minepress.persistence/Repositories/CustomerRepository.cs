using erp.minepress.application.Common.Interfaces;
using erp.minepress.domain.Customer;
using erp.minepress.persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace erp.minepress.persistence.Repositories;

public class CustomerRepository : Repository<CustomerEntity, int>, ICustomerRepository
{
    public CustomerRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<CustomerEntity?> GetByPartyIdAsync(int partyId, CancellationToken cancellationToken = default)
    {
        return await DbSet.Include(c => c.Party).FirstOrDefaultAsync(x => x.PartyId == partyId, cancellationToken);
    }

    public async Task<IReadOnlyList<CustomerEntity>> GetActiveCustomersAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().Include(c => c.Party).Where(x => x.IsActive).ToListAsync(cancellationToken);
    }
}
