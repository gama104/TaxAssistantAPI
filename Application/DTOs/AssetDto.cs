namespace IRSAssistantAPI.Application.DTOs;

public class AssetDto
{
    public string Id { get; set; } = string.Empty;
    public string TaxpayerId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int PurchaseYear { get; set; }
    public decimal PurchaseValue { get; set; }
    public decimal CurrentValue { get; set; }
    public decimal AnnualReturn { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;
}
