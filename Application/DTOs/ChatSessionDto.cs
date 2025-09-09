namespace IRSAssistantAPI.Application.DTOs;

public class ChatSessionDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string TaxpayerId { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
    public string? LastActivityAt { get; set; }
    public bool IsActive { get; set; }
    public List<ChatMessageDto> Messages { get; set; } = new();
}
