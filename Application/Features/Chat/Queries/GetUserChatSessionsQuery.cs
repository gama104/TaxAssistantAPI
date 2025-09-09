using IRSAssistantAPI.Application.Common.Interfaces;
using IRSAssistantAPI.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IRSAssistantAPI.Application.Features.Chat.Queries;

public record GetUserChatSessionsQuery(Guid TaxpayerId) : IRequest<List<ChatSessionDto>>;

public class GetUserChatSessionsQueryHandler : IRequestHandler<GetUserChatSessionsQuery, List<ChatSessionDto>>
{
    private readonly IDbContext _context;

    public GetUserChatSessionsQueryHandler(IDbContext context)
    {
        _context = context;
    }

    public async Task<List<ChatSessionDto>> Handle(GetUserChatSessionsQuery request, CancellationToken cancellationToken)
    {
        var sessions = await _context.ChatSessions
            .Where(s => s.TaxpayerId == request.TaxpayerId && s.IsActive)
            .OrderByDescending(s => s.LastActivityAt ?? s.CreatedAt)
            .Select(s => new ChatSessionDto
            {
                Id = s.Id.ToString(),
                Title = s.Title,
                TaxpayerId = s.TaxpayerId.ToString(),
                CreatedAt = s.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                LastActivityAt = s.LastActivityAt.HasValue ? s.LastActivityAt.Value.ToString("yyyy-MM-ddTHH:mm:ssZ") : null,
                IsActive = s.IsActive,
                Messages = new List<ChatMessageDto>()
            })
            .ToListAsync(cancellationToken);

        return sessions;
    }
}
