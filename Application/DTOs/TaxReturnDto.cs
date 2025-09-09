namespace IRSAssistantAPI.Application.DTOs;

public class TaxReturnDto
{
    public string Id { get; set; } = string.Empty;
    public string TaxpayerId { get; set; } = string.Empty;
    public int TaxYear { get; set; }
    public string FilingStatus { get; set; } = string.Empty;
    public decimal AGI { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TaxableIncome { get; set; }
    public decimal Deductions { get; set; }
    public decimal TaxLiability { get; set; }
    public decimal TaxCredits { get; set; }
    public decimal TaxPaid { get; set; }
    public decimal Refund { get; set; }
    public decimal BalanceDue { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;
}
