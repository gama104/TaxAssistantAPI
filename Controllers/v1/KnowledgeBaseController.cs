using Microsoft.AspNetCore.Mvc;
using IRSAssistantAPI.Infrastructure.Services;
using IRSAssistantAPI.Application.Features.KnowledgeBase.Queries;
using Microsoft.Extensions.Logging;
using MediatR;

namespace IRSAssistantAPI.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class KnowledgeBaseController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<KnowledgeBaseController> _logger;

    public KnowledgeBaseController(
        IMediator mediator,
        ILogger<KnowledgeBaseController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Gets the complete database schema knowledge base
    /// </summary>
    [HttpGet("schema")]
    public async Task<IActionResult> GetDatabaseSchema()
    {
        try
        {
            var schema = await _mediator.Send(new GetDatabaseSchemaQuery());
            return Ok(new { 
                message = "Database schema knowledge base retrieved successfully",
                schema = schema,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving database schema");
            return StatusCode(500, new { error = "Failed to retrieve database schema", details = ex.Message });
        }
    }

    /// <summary>
    /// Gets a summary of the database schema
    /// </summary>
    [HttpGet("schema/summary")]
    public async Task<IActionResult> GetSchemaSummary()
    {
        try
        {
            var summary = await _mediator.Send(new GetSchemaSummaryQuery());
            return Ok(new { 
                message = "Database schema summary retrieved successfully",
                summary = summary,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving schema summary");
            return StatusCode(500, new { error = "Failed to retrieve schema summary", details = ex.Message });
        }
    }

    /// <summary>
    /// Tests the knowledge base by generating a sample SQL query
    /// </summary>
    [HttpPost("test-query")]
    public IActionResult TestKnowledgeBase([FromBody] TestQueryRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Query))
            {
                return BadRequest(new { error = "Query is required" });
            }

            // This would normally use the AI Agent, but for testing we'll return a mock response
            var mockResponse = new
            {
                originalQuery = request.Query,
                generatedSQL = "SELECT * FROM TaxReturns WHERE TaxYear = 2023",
                explanation = "This is a test response. In production, the AI Agent would generate the actual SQL query using the knowledge base.",
                timestamp = DateTime.UtcNow
            };

            return Ok(new { 
                message = "Knowledge base test completed successfully",
                result = mockResponse
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing knowledge base");
            return StatusCode(500, new { error = "Failed to test knowledge base", details = ex.Message });
        }
    }
}

public class TestQueryRequest
{
    public string Query { get; set; } = string.Empty;
}
