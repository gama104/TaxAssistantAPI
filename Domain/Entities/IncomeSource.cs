using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IRSAssistantAPI.Domain.Entities;

public class IncomeSource
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public Guid ReturnId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Type { get; set; } = string.Empty; // "Wages", "Interest", "Dividends", "Capital Gains", "Rental Income", "Business Income"
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    [ForeignKey("ReturnId")]
    public virtual TaxReturn TaxReturn { get; set; } = null!;
}
