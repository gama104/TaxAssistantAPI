using IRSAssistantAPI.Application.Common.Interfaces;
using IRSAssistantAPI.Infrastructure.Data;
using IRSAssistantAPI.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Polly;
using Polly.Extensions.Http;
using AutoMapper;

var builder = WebApplication.CreateBuilder(args);

// Add local configuration file (for development only)
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

// Configure simple logging for proof of concept
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container
builder.Services.AddControllers();

// API Versioning (simplified for now)
builder.Services.AddApiVersioning(opt =>
{
    opt.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    opt.AssumeDefaultVersionWhenUnspecified = true;
});

builder.Services.AddVersionedApiExplorer(setup =>
{
    setup.GroupNameFormat = "'v'VVV";
    setup.SubstituteApiVersionInUrl = true;
});

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "IRS Assistant API", Version = "v1" });
    // XML comments disabled for proof of concept
});

// Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

// Register DbContext interface
builder.Services.AddScoped<IDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

// AutoMapper
builder.Services.AddAutoMapper(typeof(IRSAssistantAPI.Application.Mappings.MappingProfile));

// FluentValidation (uncomment when you add validators)
// builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

// Application Services
builder.Services.AddScoped<DatabaseSchemaService>();
builder.Services.AddScoped<StructuredResponseService>();
builder.Services.AddScoped<SystemStatusService>();
builder.Services.AddScoped<SecurityAuditService>();
builder.Services.AddScoped<IAzureOpenAIService, AIAgentService>();
builder.Services.AddScoped<SearchIndexManagementService>();
builder.Services.AddScoped<TaxDataIndexingService>();

// HTTP Client with Polly for resilience (commented out - AIAgentService uses Azure SDK directly)
// builder.Services.AddHttpClient<IAzureOpenAIService, AIAgentService>()
//     .AddPolicyHandler(GetRetryPolicy())
//     .AddPolicyHandler(GetTimeoutPolicy());

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>()
    .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")!);

// Caching
builder.Services.AddMemoryCache();
// Redis cache (optional - uncomment if you have Redis configured)
// builder.Services.AddStackExchangeRedisCache(options =>
// {
//     options.Configuration = builder.Configuration.GetConnectionString("Redis");
// });

// Application Insights (optional - uncomment if you have Application Insights configured)
// builder.Services.AddApplicationInsightsTelemetry(builder.Configuration.GetConnectionString("ApplicationInsights"));

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "IRS Assistant API v1");
        c.RoutePrefix = "swagger"; // Set Swagger UI at /swagger
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Add a simple test endpoint
app.MapGet("/", () => "IRS Assistant API is running! Go to /swagger for API documentation.");
app.MapGet("/health", () => "API is healthy!");
app.MapGet("/debug-config", (IConfiguration config) => new
{
    AzureOpenAIEndpoint = config["AzureOpenAI:Endpoint"],
    AzureOpenAIKey = config["AzureOpenAI:Key"],
    AzureOpenAIDeployment = config["AzureOpenAI:DeploymentName"],
    ConnectionString = config.GetConnectionString("DefaultConnection")
});

// Health check endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false
});

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.EnsureCreatedAsync();
}

app.Run();

// Polly policy methods
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryCount, context) =>
            {
                Console.WriteLine($"Retry {retryCount} after {timespan} seconds");
            });
}

static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy()
{
    return Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(30));
}
