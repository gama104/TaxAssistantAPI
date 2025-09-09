using Microsoft.Extensions.Diagnostics.HealthChecks;
using MediatR;

namespace IRSAssistantAPI.Application.Features.Health.Queries;

public record GetHealthQuery : IRequest<HealthReport>;

public class GetHealthQueryHandler : IRequestHandler<GetHealthQuery, HealthReport>
{
    private readonly HealthCheckService _healthCheckService;
    private readonly ILogger<GetHealthQueryHandler> _logger;

    public GetHealthQueryHandler(HealthCheckService healthCheckService, ILogger<GetHealthQueryHandler> logger)
    {
        _healthCheckService = healthCheckService;
        _logger = logger;
    }

    public async Task<HealthReport> Handle(GetHealthQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Checking health status");
            return await _healthCheckService.CheckHealthAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking health status");
            throw;
        }
    }
}
