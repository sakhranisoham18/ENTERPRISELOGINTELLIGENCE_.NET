using EnterpriseLogIntelligence.Data;
using EnterpriseLogIntelligence.Hubs;
using EnterpriseLogIntelligence.Repositories;
using EnterpriseLogIntelligence.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDb"));
builder.Services.AddSingleton<MongoDbContext>();

var storageProvider = builder.Configuration["Storage:Provider"] ?? "InMemory";
var useMongo = storageProvider.Equals("MongoDB", StringComparison.OrdinalIgnoreCase);

if (useMongo)
{
    builder.Services.AddSingleton<MongoLogRepository>();
    builder.Services.AddSingleton<ILogRepository>(sp => sp.GetRequiredService<MongoLogRepository>());
    builder.Services.AddSingleton<IAlertRepository, MongoAlertRepository>();
    builder.Services.AddSingleton<IPatternRepository, MongoPatternRepository>();
    builder.Services.AddSingleton<IUserRepository, MongoUserRepository>();
    builder.Services.AddSingleton<IAuditLogRepository, MongoAuditLogRepository>();
}
else
{
    builder.Services.AddSingleton<InMemoryLogRepository>();
    builder.Services.AddSingleton<ILogRepository>(sp => sp.GetRequiredService<InMemoryLogRepository>());
    builder.Services.AddSingleton<IAlertRepository, InMemoryAlertRepository>();
    builder.Services.AddSingleton<IPatternRepository, InMemoryPatternRepository>();
    builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();
    builder.Services.AddSingleton<IAuditLogRepository, InMemoryAuditLogRepository>();
}

builder.Services.AddSingleton<IPasswordHashService, PasswordHashService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<AlertService>();
builder.Services.AddScoped<IAlertService>(sp => sp.GetRequiredService<AlertService>());
builder.Services.AddScoped<IAlertEvaluationService>(sp => sp.GetRequiredService<AlertService>());
builder.Services.AddScoped<ILogIngestionService, LogIngestionService>();
builder.Services.AddScoped<ILogGeneratorService, LogGeneratorService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IPatternService, PatternService>();
builder.Services.AddScoped<ILogParserService, LogParserService>();
builder.Services.AddScoped<StartupDataService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });

builder.Services.AddAuthorization();
builder.Services.AddSignalR();
builder.Services.AddHealthChecks().AddCheck<MongoHealthCheck>("mongodb");
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health", new HealthCheckOptions { Predicate = _ => true }).AllowAnonymous();
app.MapHub<DashboardHub>("/hubs/dashboard");
app.MapControllerRoute(name: "default", pattern: "{controller=Dashboard}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<StartupDataService>();
    await initializer.InitializeAsync();
}

app.Run();
