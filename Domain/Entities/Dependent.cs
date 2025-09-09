using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IRSAssistantAPI.Domain.Entities;

public class Dependent
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public Guid TaxpayerId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public DateTime DateOfBirth { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Relationship { get; set; } = string.Empty; // "Daughter", "Son", "Other Dependent"
    
    [Required]
    public bool EligibleForCredit { get; set; } = true;
    
    [MaxLength(20)]
    public string? Ssn { get; set; }
    
    [MaxLength(500)]
    public string? Notes { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    [ForeignKey("TaxpayerId")]
    public virtual Taxpayer Taxpayer { get; set; } = null!;
}
