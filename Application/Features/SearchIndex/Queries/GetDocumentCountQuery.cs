using IRSAssistantAPI.Infrastructure.Services;
using MediatR;

namespace IRSAssistantAPI.Application.Features.SearchIndex.Queries;

public record GetDocumentCountQuery : IRequest<long>;

public class GetDocumentCountQueryHandler : IRequestHandler<GetDocumentCountQuery, long>
{
    private readonly SearchIndexManagementService _indexManagementService;
    private readonly ILogger<GetDocumentCountQueryHandler> _logger;

    public GetDocumentCountQueryHandler(SearchIndexManagementService indexManagementService, ILogger<GetDocumentCountQueryHandler> logger)
    {
        _indexManagementService = indexManagementService;
        _logger = logger;
    }

    public async Task<long> Handle(GetDocumentCountQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting document count");
            return await _indexManagementService.GetDocumentCountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting document count");
            throw;
        }
    }
}
