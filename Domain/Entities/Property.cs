using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IRSAssistantAPI.Domain.Entities;

public class Property
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public Guid TaxpayerId { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string Address { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Type { get; set; } = string.Empty; // "Primary Residence", "Rental Property", "Vacation Home"
    
    [Required]
    public int PurchaseYear { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal PurchasePrice { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal CurrentValue { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal MortgageBalance { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal? RentalIncome { get; set; } // Only for rental properties
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal? Expenses { get; set; } // Property-related expenses
    
    [MaxLength(1000)]
    public string? Notes { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    [ForeignKey("TaxpayerId")]
    public virtual Taxpayer Taxpayer { get; set; } = null!;
}
