using erp.minepress.notification.Configuration;
using erp.minepress.notification.Interfaces;
using erp.minepress.notification.Services;
using erp.minepress.notification.Services.Channels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace erp.minepress.notification;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNotificationServices(this IServiceCollection services, IConfiguration? configuration = null)
    {
        if (configuration is not null)
        {
            services.Configure<SmtpSettings>(configuration.GetSection(SmtpSettings.SectionName));
            services.Configure<TwilioSettings>(configuration.GetSection(TwilioSettings.SectionName));
        }

        services.AddScoped<INotificationChannelProvider, EmailChannelProvider>();
        services.AddScoped<INotificationChannelProvider, SmsChannelProvider>();
        services.AddScoped<INotificationChannelProvider, WhatsAppChannelProvider>();
        services.AddScoped<INotificationChannelProvider, InAppChannelProvider>();

        services.AddSingleton<IInAppNotificationStore, InMemoryInAppNotificationStore>();

        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<INotificationTemplateEngine, NotificationTemplateEngine>();
        services.AddScoped<INotificationDispatcher, ProcessNotificationDispatcher>();

        return services;
    }
}
