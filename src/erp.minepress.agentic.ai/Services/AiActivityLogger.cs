using erp.minepress.agentic.ai.Models;
using erp.minepress.persistence.Context;
using erp.minepress.persistence.Models;
using Microsoft.Extensions.Logging;

namespace erp.minepress.agentic.ai.Services;

/// <summary>
/// Persists AI agent activity logs to the TrnAiAgentActivity table.
/// Tracks all agent executions for auditing and analytics.
/// </summary>
public class AiActivityLogger
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<AiActivityLogger> _logger;

    public AiActivityLogger(ApplicationDbContext dbContext, ILogger<AiActivityLogger> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Log an AI agent activity to the database.
    /// </summary>
    public async Task LogActivityAsync(AiLogEntry logEntry, long? userId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var activity = new TrnAiAgentActivity
            {
                AgentName = logEntry.Agent ?? "Unknown",
                AgentAction = logEntry.Tool ?? logEntry.Intent ?? "Unknown",
                Module = ResolveModuleFromAgent(logEntry.Agent),
                UserId = userId,
                InputJson = logEntry.UserQuery ?? logEntry.InputType,
                OutputJson = logEntry.OutputFormat,
                ConfidenceScore = logEntry.Confidence,
                ExecutionTimeMs = (int)logEntry.DurationMs,
                CreatedOn = logEntry.Timestamp
            };

            if (logEntry.Error is not null)
            {
                activity.Feedback = $"Error: {logEntry.Error}";
                activity.WasAccepted = false;
            }

            _dbContext.TrnAiAgentActivities.Add(activity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogDebug("AI activity logged: {Agent}/{Tool} in {Duration}ms",
                logEntry.Agent, logEntry.Tool, logEntry.DurationMs);
        }
        catch (Exception ex)
        {
            // Don't let logging failures break the main flow
            _logger.LogWarning(ex, "Failed to log AI activity for {Agent}/{Tool}", logEntry.Agent, logEntry.Tool);
        }
    }

    /// <summary>
    /// Log a quick activity entry from individual parameters.
    /// </summary>
    public async Task LogAsync(string agent, string tool, long durationMs, string? error = null,
        long? userId = null, CancellationToken cancellationToken = default)
    {
        var entry = new AiLogEntry
        {
            Agent = agent,
            Tool = tool,
            DurationMs = durationMs,
            Error = error,
            Timestamp = DateTime.UtcNow
        };

        await LogActivityAsync(entry, userId, cancellationToken);
    }

    private static string? ResolveModuleFromAgent(string? agentName)
    {
        if (string.IsNullOrEmpty(agentName)) return null;

        // Strip "Agent" suffix to get module name
        return agentName.EndsWith("Agent", StringComparison.Ordinal) && agentName.Length > 5
            ? agentName[..^5]
            : agentName;
    }
}
