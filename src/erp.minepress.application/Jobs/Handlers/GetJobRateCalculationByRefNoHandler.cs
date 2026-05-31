using erp.minepress.application.Common.Interfaces;
using erp.minepress.application.Common.Models;
using erp.minepress.application.Jobs.Dto;

namespace erp.minepress.application.Jobs.Queries;

public class GetJobRateCalculationByRefNoHandler
    : IQueryHandler<GetJobRateCalculationByRefNoQuery, Result<JobRateCalculatorDto>>
{
    private readonly IJobRateCalculatorRepository _repository;

    public GetJobRateCalculationByRefNoHandler(IJobRateCalculatorRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<JobRateCalculatorDto>> HandleAsync(
        GetJobRateCalculationByRefNoQuery query,
        CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByRefNoAsync(query.CalcRefNo, cancellationToken);

        if (entity is null)
            return Result<JobRateCalculatorDto>.Failure($"Rate calculation with ref '{query.CalcRefNo}' not found.");

        var dto = new JobRateCalculatorDto
        {
            Id = entity.Id,
            CalcRefNo = entity.CalcRefNo,
            EnquiryId = entity.EnquiryId,
            QuotationId = entity.QuotationId,
            JobId = entity.JobId,
            PartyId = entity.PartyId,
            JobTypeId = entity.JobTypeId,
            ProductTypeId = entity.ProductTypeId,
            ProductSizeId = entity.ProductSizeId,
            Quantity = entity.Quantity,
            TotalPages = entity.TotalPages,
            TrimWidthMm = entity.TrimWidthMm,
            TrimHeightMm = entity.TrimHeightMm,
            PrintingMode = entity.PrintingMode,
            IsCustomerMaterial = entity.IsCustomerMaterial,
            GrandTotal = entity.GrandTotal,
            TaxAmount = entity.TaxAmount,
            NetTotal = entity.NetTotal,
            CostPerUnit = entity.CostPerUnit,
            Status = entity.Status,
            Version = entity.Version,
            CreatedOn = entity.CreatedOn
        };

        return Result<JobRateCalculatorDto>.Success(dto);
    }
}
