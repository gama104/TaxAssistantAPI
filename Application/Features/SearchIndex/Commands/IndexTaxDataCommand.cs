using IRSAssistantAPI.Infrastructure.Services;
using MediatR;

namespace IRSAssistantAPI.Application.Features.SearchIndex.Commands;

public record IndexTaxDataCommand : IRequest<bool>;

public class IndexTaxDataCommandHandler : IRequestHandler<IndexTaxDataCommand, bool>
{
    private readonly TaxDataIndexingService _indexingService;
    private readonly ILogger<IndexTaxDataCommandHandler> _logger;

    public IndexTaxDataCommandHandler(TaxDataIndexingService indexingService, ILogger<IndexTaxDataCommandHandler> logger)
    {
        _indexingService = indexingService;
        _logger = logger;
    }

    public async Task<bool> Handle(IndexTaxDataCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Indexing tax data");
            await _indexingService.IndexAllTaxDataAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing tax data");
            throw;
        }
    }
}
