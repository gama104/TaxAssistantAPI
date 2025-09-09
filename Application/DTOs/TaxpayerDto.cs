namespace IRSAssistantAPI.Application.DTOs;

public class TaxpayerDto
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
    public string? LastLoginAt { get; set; }
}
