using IRSAssistantAPI.Application.Common.Interfaces;
using IRSAssistantAPI.Application.DTOs;
using MediatR;

namespace IRSAssistantAPI.Application.Features.Chat.Commands;

public record CreateChatSessionCommand(string Title, Guid TaxpayerId) : IRequest<ChatSessionDto>;

public class CreateChatSessionCommandHandler : IRequestHandler<CreateChatSessionCommand, ChatSessionDto>
{
    private readonly IDbContext _context;

    public CreateChatSessionCommandHandler(IDbContext context)
    {
        _context = context;
    }

    public async Task<ChatSessionDto> Handle(CreateChatSessionCommand request, CancellationToken cancellationToken)
    {
        var session = new Domain.Entities.ChatSession
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            TaxpayerId = request.TaxpayerId,
            CreatedAt = DateTime.UtcNow,
            LastActivityAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.ChatSessions.Add(session);
        await _context.SaveChangesAsync(cancellationToken);

        return new ChatSessionDto
        {
            Id = session.Id.ToString(),
            Title = session.Title,
            TaxpayerId = session.TaxpayerId.ToString(),
            CreatedAt = session.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            LastActivityAt = session.LastActivityAt?.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            IsActive = session.IsActive,
            Messages = new List<ChatMessageDto>()
        };
    }
}
