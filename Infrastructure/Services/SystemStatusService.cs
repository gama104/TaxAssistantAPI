using IRSAssistantAPI.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace IRSAssistantAPI.Infrastructure.Services;

public class SystemStatusService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SystemStatusService> _logger;
    private readonly IDbContext _dbContext;

    public SystemStatusService(
        IConfiguration configuration,
        ILogger<SystemStatusService> logger,
        IDbContext dbContext)
    {
        _configuration = configuration;
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<SystemStatus> GetSystemStatusAsync()
    {
        var status = new SystemStatus
        {
            Timestamp = DateTime.UtcNow,
            OverallStatus = "Healthy",
            Services = new List<ServiceStatus>()
        };

        // Check Database Connection
        var dbStatus = await CheckDatabaseStatusAsync();
        status.Services.Add(dbStatus);

        // Check AI Agent Service (includes Azure OpenAI configuration)
        var aiStatus = CheckAIAgentStatus();
        status.Services.Add(aiStatus);

        // Check Application Configuration
        var configStatus = CheckConfigurationStatus();
        status.Services.Add(configStatus);

        // Determine overall status
        var unhealthyServices = status.Services.Where(s => s.Status != "Healthy").ToList();
        if (unhealthyServices.Any())
        {
            status.OverallStatus = unhealthyServices.Any(s => s.Status == "Critical") ? "Critical" : "Degraded";
            status.Issues = unhealthyServices.Select(s => s.Issue).Where(i => !string.IsNullOrEmpty(i)).ToList();
        }

        return status;
    }

    private async Task<ServiceStatus> CheckDatabaseStatusAsync()
    {
        try
        {
            // First check if connection string is configured
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString) || connectionString.Contains("your-") || connectionString.Contains("mssqllocaldb"))
            {
                return new ServiceStatus
                {
                    Name = "Database",
                    Status = "Critical",
                    Description = "Database not configured",
                    Issue = "No database connection string found. Please configure the DefaultConnection in appsettings.json",
                    LastChecked = DateTime.UtcNow,
                    Details = new Dictionary<string, object>
                    {
                        { "ConnectionString", "Not configured" },
                        { "Provider", "SQL Server" },
                        { "Status", "Not set up" }
                    }
                };
            }

            // Try to connect to the database
            var canConnect = await _dbContext.CanConnectAsync();
            
            if (canConnect)
            {
                return new ServiceStatus
                {
                    Name = "Database",
                    Status = "Healthy",
                    Description = "Database connection is working",
                    LastChecked = DateTime.UtcNow,
                    Details = new Dictionary<string, object>
                    {
                        { "ConnectionString", "Configured" },
                        { "Provider", "SQL Server" },
                        { "Status", "Connected" }
                    }
                };
            }
            else
            {
                return new ServiceStatus
                {
                    Name = "Database",
                    Status = "Critical",
                    Description = "Cannot connect to database",
                    Issue = "Database connection failed. Please check connection string and ensure database is running.",
                    LastChecked = DateTime.UtcNow,
                    Details = new Dictionary<string, object>
                    {
                        { "ConnectionString", "Configured but not accessible" },
                        { "Provider", "SQL Server" },
                        { "Status", "Connection failed" }
                    }
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return new ServiceStatus
            {
                Name = "Database",
                Status = "Critical",
                Description = "Database health check failed",
                Issue = $"Database error: {ex.Message}",
                LastChecked = DateTime.UtcNow,
                Details = new Dictionary<string, object>
                {
                    { "Error", ex.Message },
                    { "ExceptionType", ex.GetType().Name },
                    { "Status", "Error" }
                }
            };
        }
    }

    private ServiceStatus CheckAIAgentStatus()
    {
        try
        {
            // Check if AI Agent service is properly configured
            var endpoint = _configuration["AzureOpenAI:Endpoint"];
            var key = _configuration["AzureOpenAI:Key"];
            var deployment = _configuration["AzureOpenAI:DeploymentName"];

            var hasEndpoint = !IsPlaceholderValue(endpoint);
            var hasKey = !IsPlaceholderValue(key);
            var hasDeployment = !IsPlaceholderValue(deployment);

            if (hasEndpoint && hasKey && hasDeployment)
            {
                return new ServiceStatus
                {
                    Name = "AI Agent",
                    Status = "Healthy",
                    Description = "AI Agent with Azure OpenAI is configured and ready",
                    LastChecked = DateTime.UtcNow,
                    Details = new Dictionary<string, object>
                    {
                        { "AzureOpenAIEndpoint", MaskSensitiveData(endpoint) },
                        { "AzureOpenAIKey", "Configured" },
                        { "AzureOpenAIDeployment", deployment },
                        { "Status", "Ready for natural language queries" }
                    }
                };
            }
            else
            {
                var missing = new List<string>();
                if (!hasEndpoint) missing.Add("Azure OpenAI Endpoint");
                if (!hasKey) missing.Add("Azure OpenAI Key");
                if (!hasDeployment) missing.Add("Azure OpenAI Deployment");

                return new ServiceStatus
                {
                    Name = "AI Agent",
                    Status = "Critical",
                    Description = "AI Agent not configured",
                    Issue = $"Missing Azure OpenAI configuration: {string.Join(", ", missing)}. Please configure Azure OpenAI settings in appsettings.json",
                    LastChecked = DateTime.UtcNow,
                    Details = new Dictionary<string, object>
                    {
                        { "AzureOpenAIEndpoint", hasEndpoint ? "Configured" : "Missing" },
                        { "AzureOpenAIKey", hasKey ? "Configured" : "Missing" },
                        { "AzureOpenAIDeployment", hasDeployment ? deployment : "Missing" },
                        { "Status", "Not set up - cannot process queries" }
                    }
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI Agent configuration check failed");
            return new ServiceStatus
            {
                Name = "AI Agent",
                Status = "Critical",
                Description = "AI Agent configuration check failed",
                Issue = $"Configuration error: {ex.Message}",
                LastChecked = DateTime.UtcNow,
                Details = new Dictionary<string, object>
                {
                    { "Error", ex.Message },
                    { "ExceptionType", ex.GetType().Name },
                    { "Status", "Error" }
                }
            };
        }
    }


    private ServiceStatus CheckConfigurationStatus()
    {
        try
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var hasConnectionString = !string.IsNullOrEmpty(connectionString);

            if (hasConnectionString)
            {
                return new ServiceStatus
                {
                    Name = "Configuration",
                    Status = "Healthy",
                    Description = "Configuration is complete",
                    LastChecked = DateTime.UtcNow,
                    Details = new Dictionary<string, object>
                    {
                        { "ConnectionString", "Configured" },
                        { "Environment", _configuration["ASPNETCORE_ENVIRONMENT"] ?? "Not set" },
                        { "Status", "Ready" }
                    }
                };
            }
            else
            {
                return new ServiceStatus
                {
                    Name = "Configuration",
                    Status = "Critical",
                    Description = "Configuration is incomplete",
                    Issue = "Database connection string is missing. Please configure DefaultConnection in appsettings.json",
                    LastChecked = DateTime.UtcNow,
                    Details = new Dictionary<string, object>
                    {
                        { "ConnectionString", "Missing" },
                        { "Environment", _configuration["ASPNETCORE_ENVIRONMENT"] ?? "Not set" },
                        { "Status", "Not set up" }
                    }
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Configuration check failed");
            return new ServiceStatus
            {
                Name = "Configuration",
                Status = "Critical",
                Description = "Configuration check failed",
                Issue = $"Configuration error: {ex.Message}",
                LastChecked = DateTime.UtcNow,
                Details = new Dictionary<string, object>
                {
                    { "Error", ex.Message },
                    { "ExceptionType", ex.GetType().Name },
                    { "Status", "Error" }
                }
            };
        }
    }

    private string MaskSensitiveData(string value)
    {
        if (string.IsNullOrEmpty(value) || value.Length < 8)
            return "***";
        
        return value.Substring(0, 4) + "***" + value.Substring(value.Length - 4);
    }

    private bool IsPlaceholderValue(string value)
    {
        if (string.IsNullOrEmpty(value))
            return true;
            
        var placeholderValues = new[]
        {
            "your-resource",
            "your-dev-resource",
            "your-azure-openai-key",
            "your-dev-azure-openai-key",
            "your-search-service",
            "your-search-key",
            "your-application-insights-connection-string"
        };
        
        return placeholderValues.Any(placeholder => value.Contains(placeholder));
    }
}

public class SystemStatus
{
    public DateTime Timestamp { get; set; }
    public string OverallStatus { get; set; } = string.Empty;
    public List<ServiceStatus> Services { get; set; } = new();
    public List<string> Issues { get; set; } = new();
}

public class ServiceStatus
{
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // Healthy, Degraded, Critical
    public string Description { get; set; } = string.Empty;
    public string? Issue { get; set; }
    public DateTime LastChecked { get; set; }
    public Dictionary<string, object> Details { get; set; } = new();
}
