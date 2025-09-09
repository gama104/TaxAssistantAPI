using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IRSAssistantAPI.Domain.Entities;

public class ChatMessage
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public Guid ChatSessionId { get; set; }
    
    [Required]
    [MaxLength(20)]
    public string Role { get; set; } = string.Empty; // "user", "assistant"
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    public string? SqlQuery { get; set; }
    
    public string? RawData { get; set; } // JSON serialized query results
    
    public decimal? Confidence { get; set; }
    
    public int? ExecutionTimeMs { get; set; }
    
    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    [ForeignKey("ChatSessionId")]
    public virtual ChatSession ChatSession { get; set; } = null!;
}
