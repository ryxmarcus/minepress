using erp.minepress.application;
using erp.minepress.persistence;
using erp.minepress.printingcostingengine;
using erp.minepress.infrastructure;
using erp.minepress.notification;
using erp.minepress.tenants;
using erp.minepress.agentic.ai;
using erp.minepress.persistence.Context;
using erp.minepress.infrastructure.Middleware;
using erp.minepress.tenants.Middleware;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();

    var tenantConnectionString = httpContextAccessor.HttpContext?.Items[TenantConnectionConstants.TenantConnectionStringItemKey] as string;
    var bootstrapConnectionString = configuration.GetTenantCatalogConnectionString();
    var effectiveConnectionString = string.IsNullOrWhiteSpace(tenantConnectionString)
        ? bootstrapConnectionString
        : tenantConnectionString;

    options.UseNpgsql(effectiveConnectionString, npgsqlOptions =>
    {
        npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "press_db");
    });
});

builder.Services.AddPersistenceServices();
builder.Services.AddApplicationServices();
builder.Services.AddPrintingCostingEngine();
builder.Services.AddInfrastructureServices();
builder.Services.AddNotificationServices(builder.Configuration);
builder.Services.AddTenantServices(builder.Configuration);
builder.Services.AddAgenticAiServices(builder.Configuration);
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseMiddleware<GlobalExceptionLoggingMiddleware>();
app.UseMiddleware<TenantConnectionContextMiddleware>();
app.UseAuthorization();
app.MapControllers();

app.Run();
