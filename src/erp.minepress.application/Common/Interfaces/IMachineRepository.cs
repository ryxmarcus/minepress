using erp.minepress.domain.Machine;

namespace erp.minepress.application.Common.Interfaces;

public interface IMachineRepository : IRepository<MachineEntity, long>
{
    Task<IReadOnlyList<MachineEntity>> GetByDepartmentAsync(string departmentCode, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MachineEntity>> GetCompatibleMachinesAsync(int sheetLengthMm, int sheetWidthMm, int gsm, int colors, CancellationToken cancellationToken = default);
}
