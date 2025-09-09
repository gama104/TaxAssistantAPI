using IRSAssistantAPI.Infrastructure.Services;
using MediatR;

namespace IRSAssistantAPI.Application.Features.Status.Queries;

public record GetSystemStatusQuery : IRequest<SystemStatus>;

public class GetSystemStatusQueryHandler : IRequestHandler<GetSystemStatusQuery, SystemStatus>
{
    private readonly SystemStatusService _statusService;
    private readonly ILogger<GetSystemStatusQueryHandler> _logger;

    public GetSystemStatusQueryHandler(SystemStatusService statusService, ILogger<GetSystemStatusQueryHandler> logger)
    {
        _statusService = statusService;
        _logger = logger;
    }

    public async Task<SystemStatus> Handle(GetSystemStatusQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting system status");
            return await _statusService.GetSystemStatusAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting system status");
            throw;
        }
    }
}
