namespace IRSAssistantAPI.Application.DTOs;

public class QueryResponseDto
{
    public string Response { get; set; } = string.Empty;
    public string? SqlQuery { get; set; }
    public object? Data { get; set; }
    public decimal? Confidence { get; set; }
    public int? ExecutionTimeMs { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
