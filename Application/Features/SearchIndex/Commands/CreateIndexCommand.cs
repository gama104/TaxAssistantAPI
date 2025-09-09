using IRSAssistantAPI.Infrastructure.Services;
using MediatR;

namespace IRSAssistantAPI.Application.Features.SearchIndex.Commands;

public record CreateIndexCommand : IRequest<bool>;

public class CreateIndexCommandHandler : IRequestHandler<CreateIndexCommand, bool>
{
    private readonly SearchIndexManagementService _indexManagementService;
    private readonly ILogger<CreateIndexCommandHandler> _logger;

    public CreateIndexCommandHandler(SearchIndexManagementService indexManagementService, ILogger<CreateIndexCommandHandler> logger)
    {
        _indexManagementService = indexManagementService;
        _logger = logger;
    }

    public async Task<bool> Handle(CreateIndexCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating search index");
            await _indexManagementService.CreateTaxDataIndexAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating search index");
            throw;
        }
    }
}
