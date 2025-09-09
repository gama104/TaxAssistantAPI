using IRSAssistantAPI.Infrastructure.Services;
using MediatR;

namespace IRSAssistantAPI.Application.Features.KnowledgeBase.Queries;

public record GetSchemaSummaryQuery : IRequest<string>;

public class GetSchemaSummaryQueryHandler : IRequestHandler<GetSchemaSummaryQuery, string>
{
    private readonly DatabaseSchemaService _schemaService;
    private readonly ILogger<GetSchemaSummaryQueryHandler> _logger;

    public GetSchemaSummaryQueryHandler(DatabaseSchemaService schemaService, ILogger<GetSchemaSummaryQueryHandler> logger)
    {
        _schemaService = schemaService;
        _logger = logger;
    }

    public Task<string> Handle(GetSchemaSummaryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting schema summary");
            return Task.FromResult(_schemaService.GetSchemaSummary());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting schema summary");
            throw;
        }
    }
}
