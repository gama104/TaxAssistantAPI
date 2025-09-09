namespace IRSAssistantAPI.Application.DTOs;

public class ChatMessageDto
{
    public string Id { get; set; } = string.Empty;
    public string ChatSessionId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? SqlQuery { get; set; }
    public object? Data { get; set; }
    public decimal? Confidence { get; set; }
    public int? ExecutionTimeMs { get; set; }
    public string? ErrorMessage { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
}
