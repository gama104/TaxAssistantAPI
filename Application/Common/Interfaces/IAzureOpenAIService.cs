using IRSAssistantAPI.Application.DTOs;

namespace IRSAssistantAPI.Application.Common.Interfaces;

public interface IAzureOpenAIService
{
    // Clean interface with only the method we actually use
    Task<QueryResponseDto> ProcessQueryAsync(string userQuery, Guid sessionId, Guid? taxpayerId = null, CancellationToken cancellationToken = default);
}
