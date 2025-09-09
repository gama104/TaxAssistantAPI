using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IRSAssistantAPI.Domain.Entities;

public class TaxDocument
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string DocumentType { get; set; } = string.Empty; // W-2, 1099, 1040, etc.
    
    [Required]
    public int TaxYear { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string FileName { get; set; } = string.Empty;
    
    [Required]
    public long FileSize { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string MimeType { get; set; } = string.Empty;
    
    [Required]
    public string Content { get; set; } = string.Empty; // JSON serialized extracted data
    
    [Required]
    public DateTime UploadedAt { get; set; }
    
    [Required]
    public DateTime ProcessedAt { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Processed"; // Processed, Failed, Pending
    
    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }
    
    [Required]
    public Guid TaxpayerId { get; set; }
    
    // Navigation properties
    public virtual Taxpayer Taxpayer { get; set; } = null!;
    public virtual ICollection<ChatSession> ChatSessions { get; set; } = new List<ChatSession>();
}
