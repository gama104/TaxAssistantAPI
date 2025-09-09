namespace IRSAssistantAPI.Application.DTOs;

public class DependentDto
{
    public string Id { get; set; } = string.Empty;
    public string TaxpayerId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Relationship { get; set; } = string.Empty;
    public bool EligibleForCredit { get; set; }
    public string Ssn { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;
}
