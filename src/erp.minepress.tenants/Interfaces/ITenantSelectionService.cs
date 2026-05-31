using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using erp.minepress.tenants.Models;

namespace erp.minepress.tenants.Interfaces;

public interface ITenantSelectionService
{
    Task<IEnumerable<TenantSelectionItem>> GetTenantSelectionAsync(CancellationToken cancellationToken = default);
}
