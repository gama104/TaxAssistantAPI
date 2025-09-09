using IRSAssistantAPI.Infrastructure.Services;
using MediatR;

namespace IRSAssistantAPI.Application.Features.SearchIndex.Queries;

public record IndexExistsQuery : IRequest<bool>;

public class IndexExistsQueryHandler : IRequestHandler<IndexExistsQuery, bool>
{
    private readonly SearchIndexManagementService _indexManagementService;
    private readonly ILogger<IndexExistsQueryHandler> _logger;

    public IndexExistsQueryHandler(SearchIndexManagementService indexManagementService, ILogger<IndexExistsQueryHandler> logger)
    {
        _indexManagementService = indexManagementService;
        _logger = logger;
    }

    public async Task<bool> Handle(IndexExistsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Checking if index exists");
            return await _indexManagementService.IndexExistsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if index exists");
            throw;
        }
    }
}
