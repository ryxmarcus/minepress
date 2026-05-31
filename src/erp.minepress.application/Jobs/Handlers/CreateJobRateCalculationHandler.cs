using erp.minepress.application.Common.Interfaces;
using erp.minepress.application.Common.Models;
using erp.minepress.application.Jobs.Dto;
using erp.minepress.domain.Job;

namespace erp.minepress.application.Jobs.Commands;

public class CreateJobRateCalculationHandler
    : ICommandHandler<CreateJobRateCalculationCommand, Result<JobRateCalculatorDto>>
{
    private readonly IJobRateCalculatorRepository _repository;
    private readonly ICostingEngine _costingEngine;
    private readonly IUnitOfWork _unitOfWork;

    public CreateJobRateCalculationHandler(
        IJobRateCalculatorRepository repository,
        ICostingEngine costingEngine,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _costingEngine = costingEngine;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<JobRateCalculatorDto>> HandleAsync(
        CreateJobRateCalculationCommand command,
        CancellationToken cancellationToken = default)
    {
        var refNo = await _repository.GenerateRefNoAsync(cancellationToken);

        var costRequest = new CostEstimationRequest
        {
            Quantity = command.Quantity,
            TotalPages = command.TotalPages,
            TrimWidthMm = command.TrimWidthMm ?? 0,
            TrimHeightMm = command.TrimHeightMm ?? 0,
            PrintingMode = command.PrintingMode,
            JobTypeId = command.JobTypeId,
            ProductTypeId = command.ProductTypeId,
            IsCustomerMaterial = command.IsCustomerMaterial
        };

        var costResult = await _costingEngine.CalculateCostAsync(costRequest, cancellationToken);

        var entity = new JobRateCalculatorEntity
        {
            CalcRefNo = refNo,
            PartyId = command.PartyId,
            JobTypeId = command.JobTypeId,
            ProductTypeId = command.ProductTypeId,
            ProductSizeId = command.ProductSizeId,
            Quantity = command.Quantity,
            TotalPages = command.TotalPages,
            TrimWidthMm = command.TrimWidthMm,
            TrimHeightMm = command.TrimHeightMm,
            PrintingMode = command.PrintingMode,
            IsCustomerMaterial = command.IsCustomerMaterial,
            GrandTotal = costResult.GrandTotal,
            TaxAmount = costResult.TaxAmount,
            NetTotal = costResult.NetTotal,
            CostPerUnit = costResult.CostPerUnit,
            InternalRemarks = command.InternalRemarks,
            ClientRemarks = command.ClientRemarks,
            CreatedBy = command.CreatedBy,
            Status = "DRAFT"
        };

        var created = await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new JobRateCalculatorDto
        {
            Id = created.Id,
            CalcRefNo = created.CalcRefNo,
            PartyId = created.PartyId,
            JobTypeId = created.JobTypeId,
            ProductTypeId = created.ProductTypeId,
            ProductSizeId = created.ProductSizeId,
            Quantity = created.Quantity,
            TotalPages = created.TotalPages,
            TrimWidthMm = created.TrimWidthMm,
            TrimHeightMm = created.TrimHeightMm,
            PrintingMode = created.PrintingMode,
            IsCustomerMaterial = created.IsCustomerMaterial,
            GrandTotal = created.GrandTotal,
            TaxAmount = created.TaxAmount,
            NetTotal = created.NetTotal,
            CostPerUnit = created.CostPerUnit,
            Status = created.Status,
            Version = created.Version,
            CreatedOn = created.CreatedOn
        };

        return Result<JobRateCalculatorDto>.Success(dto);
    }
}
