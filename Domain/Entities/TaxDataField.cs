using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IRSAssistantAPI.Domain.Entities;

public class TaxDataField
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public Guid TaxDocumentId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string FieldName { get; set; } = string.Empty; // "wages", "federal_tax_withheld", etc.
    
    [Required]
    [MaxLength(200)]
    public string DisplayName { get; set; } = string.Empty; // "Wages, tips, other compensation"
    
    [Required]
    public string Value { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string DataType { get; set; } = "string"; // string, number, date, currency
    
    [MaxLength(100)]
    public string? Category { get; set; } // "income", "deductions", "credits", etc.
    
    public decimal? NumericValue { get; set; }
    
    public DateTime? DateValue { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; }
    
    [Required]
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    [ForeignKey("TaxDocumentId")]
    public virtual TaxDocument TaxDocument { get; set; } = null!;
}
