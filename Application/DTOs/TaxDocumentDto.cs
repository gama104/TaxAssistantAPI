namespace IRSAssistantAPI.Application.DTOs;

public class TaxDocumentDto
{
    public string Id { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public int TaxYear { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string MimeType { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string UploadedAt { get; set; } = string.Empty;
    public string? ProcessedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public string TaxpayerId { get; set; } = string.Empty;
}
