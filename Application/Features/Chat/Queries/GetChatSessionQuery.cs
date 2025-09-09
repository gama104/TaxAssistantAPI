using IRSAssistantAPI.Application.Common.Interfaces;
using IRSAssistantAPI.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IRSAssistantAPI.Application.Features.Chat.Queries;

public record GetChatSessionQuery(Guid SessionId, Guid TaxpayerId) : IRequest<ChatSessionDto?>;

public class GetChatSessionQueryHandler : IRequestHandler<GetChatSessionQuery, ChatSessionDto?>
{
    private readonly IDbContext _context;

    public GetChatSessionQueryHandler(IDbContext context)
    {
        _context = context;
    }

    public async Task<ChatSessionDto?> Handle(GetChatSessionQuery request, CancellationToken cancellationToken)
    {
        var session = await _context.ChatSessions
            .Include(s => s.Messages)
            .FirstOrDefaultAsync(s => s.Id == request.SessionId && s.TaxpayerId == request.TaxpayerId, cancellationToken);

        if (session == null)
            return null;

        var messages = session.Messages
            .OrderBy(m => m.CreatedAt)
            .Select(m => new ChatMessageDto
            {
                Id = m.Id.ToString(),
                ChatSessionId = m.ChatSessionId.ToString(),
                Role = m.Role,
                Content = m.Content,
                SqlQuery = m.SqlQuery,
                Data = m.RawData != null ? System.Text.Json.JsonSerializer.Deserialize<object>(m.RawData) : null,
                Confidence = m.Confidence,
                ExecutionTimeMs = m.ExecutionTimeMs,
                ErrorMessage = m.ErrorMessage,
                CreatedAt = m.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
            })
            .ToList();

        return new ChatSessionDto
        {
            Id = session.Id.ToString(),
            Title = session.Title,
            TaxpayerId = session.TaxpayerId.ToString(),
            CreatedAt = session.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            LastActivityAt = session.LastActivityAt?.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            IsActive = session.IsActive,
            Messages = messages
        };
    }
}
