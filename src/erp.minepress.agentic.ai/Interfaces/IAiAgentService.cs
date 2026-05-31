namespace erp.minepress.agentic.ai.Interfaces;

public interface IAiAgentService
{
    Task<AiInsight> GetCostPredictionAsync(CostPredictionRequest request, CancellationToken cancellationToken = default);
    Task<AiInsight> GetSchedulingRecommendationAsync(SchedulingRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AiInsight>> GetJobRecommendationsAsync(long jobId, CancellationToken cancellationToken = default);
}

public record CostPredictionRequest
{
    public int? JobTypeId { get; init; }
    public int Quantity { get; init; }
    public int TotalPages { get; init; }
    public decimal TrimWidthMm { get; init; }
    public decimal TrimHeightMm { get; init; }
}

public record SchedulingRequest
{
    public long JobId { get; init; }
    public DateTime RequiredDate { get; init; }
    public int Priority { get; init; }
}

public record AiInsight
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Severity { get; init; } = "info";
    public decimal? Confidence { get; init; }
}
