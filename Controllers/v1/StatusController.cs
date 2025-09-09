using IRSAssistantAPI.Infrastructure.Services;
using IRSAssistantAPI.Application.Features.Status.Queries;
using Microsoft.AspNetCore.Mvc;
using MediatR;

namespace IRSAssistantAPI.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class StatusController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<StatusController> _logger;

    public StatusController(IMediator mediator, ILogger<StatusController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get comprehensive system status for frontend display
    /// </summary>
    /// <returns>Detailed system status information</returns>
    [HttpGet]
    [ProducesResponseType(typeof(SystemStatus), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SystemStatus>> GetSystemStatus()
    {
        try
        {
            var status = await _mediator.Send(new GetSystemStatusQuery());
            
            // Set appropriate HTTP status code based on overall status
            var statusCode = status.OverallStatus switch
            {
                "Healthy" => StatusCodes.Status200OK,
                "Degraded" => StatusCodes.Status200OK, // Still functional but with issues
                "Critical" => StatusCodes.Status503ServiceUnavailable,
                _ => StatusCodes.Status500InternalServerError
            };

            return StatusCode(statusCode, status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting system status");
            
            var errorStatus = new SystemStatus
            {
                Timestamp = DateTime.UtcNow,
                OverallStatus = "Critical",
                Services = new List<ServiceStatus>
                {
                    new ServiceStatus
                    {
                        Name = "Status Service",
                        Status = "Critical",
                        Description = "Status service encountered an error",
                        Issue = ex.Message,
                        LastChecked = DateTime.UtcNow
                    }
                },
                Issues = new List<string> { $"Status service error: {ex.Message}" }
            };

            return StatusCode(StatusCodes.Status500InternalServerError, errorStatus);
        }
    }

    /// <summary>
    /// Get a simplified status for quick checks
    /// </summary>
    /// <returns>Simplified status information</returns>
    [HttpGet("summary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<object>> GetStatusSummary()
    {
        try
        {
            var status = await _mediator.Send(new GetSystemStatusQuery());
            
            return Ok(new
            {
                Status = status.OverallStatus,
                Timestamp = status.Timestamp,
                ServiceCount = status.Services.Count,
                HealthyServices = status.Services.Count(s => s.Status == "Healthy"),
                UnhealthyServices = status.Services.Count(s => s.Status != "Healthy"),
                Issues = status.Issues
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting status summary");
            return Ok(new
            {
                Status = "Error",
                Timestamp = DateTime.UtcNow,
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// Get status of a specific service
    /// </summary>
    /// <param name="serviceName">Name of the service to check</param>
    /// <returns>Status of the specific service</returns>
    [HttpGet("{serviceName}")]
    [ProducesResponseType(typeof(ServiceStatus), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceStatus>> GetServiceStatus(string serviceName)
    {
        try
        {
            var status = await _mediator.Send(new GetSystemStatusQuery());
            var service = status.Services.FirstOrDefault(s => 
                s.Name.Equals(serviceName, StringComparison.OrdinalIgnoreCase));

            if (service == null)
            {
                return NotFound(new { Message = $"Service '{serviceName}' not found" });
            }

            return Ok(service);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting service status for {ServiceName}", serviceName);
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = ex.Message });
        }
    }
}
