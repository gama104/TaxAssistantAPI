using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using IRSAssistantAPI.Application.Features.Health.Queries;
using MediatR;

namespace IRSAssistantAPI.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<HealthController> _logger;

    public HealthController(IMediator mediator, ILogger<HealthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get the health status of the API
    /// </summary>
    /// <returns>Health status information</returns>
    [HttpGet]
    [ProducesResponseType(typeof(HealthReport), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HealthReport), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<HealthReport>> GetHealth()
    {
        try
        {
            var healthReport = await _mediator.Send(new GetHealthQuery());
            
            var statusCode = healthReport.Status == HealthStatus.Healthy ? 200 : 503;
            
            return StatusCode(statusCode, new
            {
                Status = healthReport.Status.ToString(),
                Duration = healthReport.TotalDuration,
                Checks = healthReport.Entries.Select(entry => new
                {
                    Name = entry.Key,
                    Status = entry.Value.Status.ToString(),
                    Duration = entry.Value.Duration,
                    Description = entry.Value.Description,
                    Data = entry.Value.Data
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking health status");
            return StatusCode(503, new { Status = "Unhealthy", Error = ex.Message });
        }
    }

    /// <summary>
    /// Get a simple health check for load balancers
    /// </summary>
    /// <returns>Simple OK response</returns>
    [HttpGet("ping")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Ping()
    {
        return Ok(new { Status = "OK", Timestamp = DateTime.UtcNow });
    }
}
