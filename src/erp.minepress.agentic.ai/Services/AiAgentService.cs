using erp.minepress.agentic.ai.Interfaces;
using Microsoft.Extensions.Logging;

namespace erp.minepress.agentic.ai.Services;

public class AiAgentService : IAiAgentService
{
    private readonly ILogger<AiAgentService> _logger;

    public AiAgentService(ILogger<AiAgentService> logger)
    {
        _logger = logger;
    }

    public Task<AiInsight> GetCostPredictionAsync(CostPredictionRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("AI: Generating cost prediction for JobType={JobTypeId}, Qty={Quantity}", request.JobTypeId, request.Quantity);

        return Task.FromResult(new AiInsight
        {
            Title = "Cost Prediction",
            Description = "AI cost prediction is not yet configured. Connect an LLM provider to enable this feature.",
            Severity = "info",
            Confidence = 0
        });
    }

    public Task<AiInsight> GetSchedulingRecommendationAsync(SchedulingRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("AI: Generating scheduling recommendation for Job={JobId}", request.JobId);

        return Task.FromResult(new AiInsight
        {
            Title = "Scheduling Recommendation",
            Description = "AI scheduling is not yet configured. Connect an LLM provider to enable this feature.",
            Severity = "info",
            Confidence = 0
        });
    }

    public Task<IReadOnlyList<AiInsight>> GetJobRecommendationsAsync(long jobId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("AI: Generating job recommendations for Job={JobId}", jobId);

        IReadOnlyList<AiInsight> insights =
        [
            new AiInsight
            {
                Title = "Job Recommendations",
                Description = "AI job recommendations are not yet configured. Connect an LLM provider to enable this feature.",
                Severity = "info",
                Confidence = 0
            }
        ];

        return Task.FromResult(insights);
    }
}
