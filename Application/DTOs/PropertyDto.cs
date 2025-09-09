namespace IRSAssistantAPI.Application.DTOs;

public class PropertyDto
{
    public string Id { get; set; } = string.Empty;
    public string TaxpayerId { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string PropertyType { get; set; } = string.Empty;
    public decimal PurchasePrice { get; set; }
    public decimal CurrentValue { get; set; }
    public int PurchaseYear { get; set; }
    public decimal MortgageBalance { get; set; }
    public decimal RentalIncome { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;
}
