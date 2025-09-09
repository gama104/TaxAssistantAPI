using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace IRSAssistantAPI.Infrastructure.Services;

public class SecurityAuditService
{
    private readonly ILogger<SecurityAuditService> _logger;

    public SecurityAuditService(ILogger<SecurityAuditService> logger)
    {
        _logger = logger;
    }

    public void LogSecurityViolation(string violation, string userQuery, string? sqlQuery = null, string? taxpayerId = null, string? sessionId = null)
    {
        var auditEntry = new
        {
            Timestamp = DateTime.UtcNow,
            EventType = "SecurityViolation",
            Violation = violation,
            UserQuery = userQuery,
            SqlQuery = sqlQuery,
            TaxpayerId = taxpayerId,
            SessionId = sessionId,
            Severity = "HIGH"
        };

        _logger.LogWarning("🚨 SECURITY VIOLATION: {AuditEntry}", JsonSerializer.Serialize(auditEntry));
    }

    public void LogQueryExecution(string userQuery, string sqlQuery, string taxpayerId, string sessionId, bool success, int resultCount)
    {
        var auditEntry = new
        {
            Timestamp = DateTime.UtcNow,
            EventType = "QueryExecution",
            UserQuery = userQuery,
            SqlQuery = sqlQuery,
            TaxpayerId = taxpayerId,
            SessionId = sessionId,
            Success = success,
            ResultCount = resultCount
        };

        _logger.LogInformation("📊 QUERY EXECUTION: {AuditEntry}", JsonSerializer.Serialize(auditEntry));
    }
}
