using erp.minepress.agentic.ai.Agents;
using erp.minepress.agentic.ai.Configuration;
using erp.minepress.agentic.ai.Interfaces;
using erp.minepress.agentic.ai.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace erp.minepress.agentic.ai;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAgenticAiServices(this IServiceCollection services, IConfiguration? configuration = null)
    {
        // Existing AI agent service
        services.AddScoped<IAiAgentService, AiAgentService>();

        // Configuration
        if (configuration is not null)
        {
            services.Configure<OpenAISettings>(configuration.GetSection(OpenAISettings.SectionName));
        }
        else
        {
            services.Configure<OpenAISettings>(_ => { });
        }

        // OpenAI HttpClient
        services.AddHttpClient<IOpenAIService, OpenAIService>();

        // Tool definitions
        services.AddSingleton<IToolDefinitionProvider, ToolDefinitionProvider>();

        // DbContext automation engine
        services.AddScoped<IDbContextIntentGenerator, DbContextIntentGenerator>();

        // Dynamic entity service (generic CRUD for any entity)
        services.AddScoped<IDynamicEntityService, DynamicEntityService>();

        // Analytics service
        services.AddScoped<IAnalyticsService, AnalyticsService>();

        // Query cache
        services.AddMemoryCache();
        services.AddSingleton<QueryCacheService>();

        // AI activity logger
        services.AddScoped<AiActivityLogger>();

        // Agents
        services.AddScoped<IntentAgent>();
        services.AddScoped<DynamicFallbackAgent>();
        services.AddScoped<IAgent, JobAgent>();
        services.AddScoped<IAgent, CostingAgent>();
        services.AddScoped<IAgent, MachineAgent>();
        services.AddScoped<IAgent, BillingAgent>();
        services.AddScoped<IAgent, DeliveryAgent>();
        services.AddScoped<IAgent, VendorAgent>();
        services.AddScoped<IAgent, ReportingAgent>();
        services.AddScoped<IAgent, CustomerAgent>();
        services.AddScoped<IAgent, EnquiryAgent>();
        services.AddScoped<IAgent, QuotationAgent>();
        services.AddScoped<IAgent, PurchaseAgent>();
        services.AddScoped<IAgent, HRAgent>();
        services.AddScoped<IAgent, StoreAgent>();
        services.AddScoped<IAgent, AccountingAgent>();
        services.AddScoped<IAgent, AnalyticsAgent>();

        // Agent router
        services.AddScoped<IAgentRouter, AgentRouter>();

        // Delivery & document services
        services.AddScoped<ISpeechToTextService, SpeechToTextService>();
        services.AddScoped<IPdfService, PdfService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IWhatsAppService, WhatsAppService>();
        services.AddScoped<IResponseFormatter, ResponseFormatter>();

        // Core orchestrator
        services.AddScoped<IAIOrchestratorService, AIOrchestratorService>();

        return services;
    }
}
