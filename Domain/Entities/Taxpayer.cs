using System.ComponentModel.DataAnnotations;

namespace IRSAssistantAPI.Domain.Entities;

public class Taxpayer
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;
    
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }
    
    [MaxLength(20)]
    public string? Ssn { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; }
    
    public DateTime? LastLoginAt { get; set; }
    
    [Required]
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual ICollection<TaxReturn> TaxReturns { get; set; } = new List<TaxReturn>();
    public virtual ICollection<Property> Properties { get; set; } = new List<Property>();
    public virtual ICollection<Asset> Assets { get; set; } = new List<Asset>();
    public virtual ICollection<Dependent> Dependents { get; set; } = new List<Dependent>();
    public virtual ICollection<TaxDocument> TaxDocuments { get; set; } = new List<TaxDocument>();
    public virtual ICollection<ChatSession> ChatSessions { get; set; } = new List<ChatSession>();
}
