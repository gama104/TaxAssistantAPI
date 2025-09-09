using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IRSAssistantAPI.Domain.Entities;

public class ChatSession
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public Guid TaxpayerId { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; }
    
    public DateTime? LastActivityAt { get; set; }
    
    [Required]
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    [ForeignKey("TaxpayerId")]
    public virtual Taxpayer Taxpayer { get; set; } = null!;
    public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}
