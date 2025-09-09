using IRSAssistantAPI.Infrastructure.Services;
using MediatR;

namespace IRSAssistantAPI.Application.Features.KnowledgeBase.Queries;

public record GetDatabaseSchemaQuery : IRequest<string>;

public class GetDatabaseSchemaQueryHandler : IRequestHandler<GetDatabaseSchemaQuery, string>
{
    private readonly DatabaseSchemaService _schemaService;
    private readonly ILogger<GetDatabaseSchemaQueryHandler> _logger;

    public GetDatabaseSchemaQueryHandler(DatabaseSchemaService schemaService, ILogger<GetDatabaseSchemaQueryHandler> logger)
    {
        _schemaService = schemaService;
        _logger = logger;
    }

    public Task<string> Handle(GetDatabaseSchemaQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting database schema");
            return Task.FromResult(_schemaService.GetDatabaseSchemaDocument());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting database schema");
            throw;
        }
    }
}
