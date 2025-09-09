namespace IRSAssistantAPI.Application.DTOs;

public class IncomeSourceDto
{
    public string Id { get; set; } = string.Empty;
    public string ReturnId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;
}
