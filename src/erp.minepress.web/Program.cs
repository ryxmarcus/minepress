using erp.minepress.application;
using erp.minepress.infrastructure;
using erp.minepress.notification;
using erp.minepress.agentic.ai;
using erp.minepress.bff.service;
using erp.minepress.persistence;
using erp.minepress.persistence.Context;
using erp.minepress.printingcostingengine;
using erp.minepress.frameworks.Authentication;
using erp.minepress.tenants;
using erp.minepress.tenants.Middleware;
using erp.minepress.infrastructure.Middleware;
using erp.minepress.web.Filters;
using erp.minepress.web.Services;
using Microsoft.EntityFrameworkCore;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

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

// Session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".MinePress.Session";
});

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AddFolderApplicationModelConvention("/", model =>
    {
        model.Filters.Add(new AuthRequiredFilter());
    });
});
builder.Services.AddControllers(options =>
{
    options.Filters.Add<erp.minepress.web.Filters.GlobalExceptionFilter>();
});

builder.Services.AddNotificationServices(builder.Configuration);
builder.Services.AddPersistenceServices();
builder.Services.AddPrintingCostingEngine();
builder.Services.AddInfrastructureServices();
builder.Services.AddApplicationServices();
builder.Services.AddBffServices();
builder.Services.AddAgenticAiServices(builder.Configuration);
builder.Services.AddTenantServices(builder.Configuration);

// JWT Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserActivityService, UserActivityService>();
builder.Services.AddScoped<IDocumentNumberService, DocumentNumberService>();
builder.Services.AddScoped<IWorkspaceProcessEngine, WorkspaceProcessEngine>();
builder.Services.AddScoped<IItemTaskService, ItemTaskService>();
builder.Services.AddScoped<IMenuService, MenuService>();

// Register tenant-scoped data protection key repository
builder.Services.AddSingleton<Microsoft.AspNetCore.DataProtection.Repositories.IXmlRepository, erp.minepress.tenants.TenantDataProtectionXmlRepository>();
builder.Services.AddDataProtection();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

// Redirect root to demo tenant select page as default
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/" || string.IsNullOrWhiteSpace(context.Request.Path))
    {
        context.Response.Redirect("/Tenants/Demo/Select");
        return;
    }

    await next();
});

// Ensure tenant middleware runs before authentication so data-protection and cookie
// decoding can use tenant-scoped key repository.
app.UseMiddleware<TenantConnectionContextMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseJwtAuthGuard();

app.UseMiddleware<GlobalExceptionLoggingMiddleware>();
app.MapRazorPages();
app.MapControllers();

app.Run();
