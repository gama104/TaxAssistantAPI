using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IRSAssistantAPI.Domain.Entities;

public class Asset
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public Guid TaxpayerId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Type { get; set; } = string.Empty; // "Stock Portfolio", "Crypto", "Car", "401k", "IRA", "Savings"
    
    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public int PurchaseYear { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal PurchaseValue { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal CurrentValue { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal? AnnualReturn { get; set; } // Percentage or amount
    
    [MaxLength(1000)]
    public string? Notes { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    [ForeignKey("TaxpayerId")]
    public virtual Taxpayer Taxpayer { get; set; } = null!;
}
