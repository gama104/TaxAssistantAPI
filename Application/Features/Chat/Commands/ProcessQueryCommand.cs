using IRSAssistantAPI.Application.Common.Interfaces;
using IRSAssistantAPI.Application.DTOs;
using MediatR;

namespace IRSAssistantAPI.Application.Features.Chat.Commands;

public record ProcessQueryCommand(string UserQuery, string SessionId, string TaxpayerId) : IRequest<QueryResponseDto>;

public class ProcessQueryCommandHandler : IRequestHandler<ProcessQueryCommand, QueryResponseDto>
{
    private readonly IAzureOpenAIService _azureOpenAIService;
    private readonly IDbContext _context;

    public ProcessQueryCommandHandler(IAzureOpenAIService azureOpenAIService, IDbContext context)
    {
        _azureOpenAIService = azureOpenAIService;
        _context = context;
    }

    public async Task<QueryResponseDto> Handle(ProcessQueryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Parse GUIDs
            var sessionId = Guid.Parse(request.SessionId);
            var taxpayerId = Guid.Parse(request.TaxpayerId);

            // Ensure chat session exists
            var session = await _context.ChatSessions.FindAsync(sessionId);
            if (session == null)
            {
                // Create a new session if it doesn't exist
                session = new Domain.Entities.ChatSession
                {
                    Id = sessionId,
                    Title = "Tax Data Analysis",
                    TaxpayerId = taxpayerId,
                    CreatedAt = DateTime.UtcNow,
                    LastActivityAt = DateTime.UtcNow,
                    IsActive = true
                };
                _context.ChatSessions.Add(session);
            }

            // Process the query using Azure OpenAI
            var response = await _azureOpenAIService.ProcessQueryAsync(
                request.UserQuery,
                sessionId,
                taxpayerId,
                cancellationToken);

            // Save user message
            var userMessage = new Domain.Entities.ChatMessage
            {
                Id = Guid.NewGuid(),
                ChatSessionId = sessionId,
                Role = "user",
                Content = request.UserQuery,
                CreatedAt = DateTime.UtcNow
            };

            // Save assistant response
            var assistantMessage = new Domain.Entities.ChatMessage
            {
                Id = Guid.NewGuid(),
                ChatSessionId = sessionId,
                Role = "assistant",
                Content = response.Response,
                SqlQuery = response.SqlQuery,
                RawData = response.Data != null ? System.Text.Json.JsonSerializer.Serialize(response.Data) : null,
                Confidence = response.Confidence,
                ExecutionTimeMs = response.ExecutionTimeMs,
                ErrorMessage = response.ErrorMessage,
                CreatedAt = DateTime.UtcNow
            };

            // Temporarily disable chat message saving to avoid connection string issues
            // _context.ChatMessages.Add(userMessage);
            // _context.ChatMessages.Add(assistantMessage);

            // Update session last activity
            session.LastActivityAt = DateTime.UtcNow;

            // await _context.SaveChangesAsync(cancellationToken);

            return response;
        }
        catch (Exception ex)
        {
            return new QueryResponseDto
            {
                Response = "I apologize, but I encountered an error processing your request.",
                ErrorMessage = ex.Message,
                Timestamp = DateTime.UtcNow
            };
        }
    }
}
