using erp.minepress.application.Common.Interfaces;
using erp.minepress.domain.Machine;
using erp.minepress.persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace erp.minepress.persistence.Repositories;

public class MachineRepository : Repository<MachineEntity, long>, IMachineRepository
{
    public MachineRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<MachineEntity>> GetByDepartmentAsync(string departmentCode, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking()
            .Where(x => x.DepartmentCode == departmentCode && x.IsActive)
            .OrderBy(x => x.AutoSelectPriority)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MachineEntity>> GetCompatibleMachinesAsync(
        int sheetLengthMm, int sheetWidthMm, int gsm, int colors,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking()
            .Where(x => x.IsActive
                && x.MaxSheetLengthMm >= sheetLengthMm
                && x.MaxSheetWidthMm >= sheetWidthMm
                && (x.MinGsm == null || x.MinGsm <= gsm)
                && (x.MaxGsm == null || x.MaxGsm >= gsm)
                && (x.MaxColors == null || x.MaxColors >= colors))
            .OrderBy(x => x.AutoSelectPriority)
            .ToListAsync(cancellationToken);
    }
}
