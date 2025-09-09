using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IRSAssistantAPI.Domain.Entities;

public class TaxReturn
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public Guid TaxpayerId { get; set; }
    
    [Required]
    public int TaxYear { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string FilingStatus { get; set; } = string.Empty; // "Married Filing Joint", "Single", etc.
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal AGI { get; set; } // Adjusted Gross Income
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalIncome { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxableIncome { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Deductions { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxLiability { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxCredits { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxPaid { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Refund { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal BalanceDue { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    [ForeignKey("TaxpayerId")]
    public virtual Taxpayer Taxpayer { get; set; } = null!;
    public virtual ICollection<IncomeSource> IncomeSources { get; set; } = new List<IncomeSource>();
}
