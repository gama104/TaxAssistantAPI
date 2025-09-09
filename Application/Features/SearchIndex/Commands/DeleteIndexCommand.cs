using IRSAssistantAPI.Infrastructure.Services;
using MediatR;

namespace IRSAssistantAPI.Application.Features.SearchIndex.Commands;

public record DeleteIndexCommand : IRequest<bool>;

public class DeleteIndexCommandHandler : IRequestHandler<DeleteIndexCommand, bool>
{
    private readonly SearchIndexManagementService _indexManagementService;
    private readonly ILogger<DeleteIndexCommandHandler> _logger;

    public DeleteIndexCommandHandler(SearchIndexManagementService indexManagementService, ILogger<DeleteIndexCommandHandler> logger)
    {
        _indexManagementService = indexManagementService;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteIndexCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Deleting search index");
            await _indexManagementService.DeleteIndexAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting search index");
            throw;
        }
    }
}
