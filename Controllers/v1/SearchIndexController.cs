using Microsoft.AspNetCore.Mvc;
using IRSAssistantAPI.Infrastructure.Services;
using IRSAssistantAPI.Application.Features.SearchIndex.Commands;
using IRSAssistantAPI.Application.Features.SearchIndex.Queries;
using Microsoft.Extensions.Logging;
using MediatR;

namespace IRSAssistantAPI.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class SearchIndexController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<SearchIndexController> _logger;

    public SearchIndexController(
        IMediator mediator,
        ILogger<SearchIndexController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Creates the tax data search index
    /// </summary>
    [HttpPost("create")]
    public async Task<IActionResult> CreateIndex()
    {
        try
        {
            var result = await _mediator.Send(new CreateIndexCommand());
            return Ok(new { message = "Search index created successfully", success = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating search index");
            return StatusCode(500, new { error = "Failed to create search index", details = ex.Message });
        }
    }

    /// <summary>
    /// Checks if the search index exists
    /// </summary>
    [HttpGet("exists")]
    public async Task<IActionResult> IndexExists()
    {
        try
        {
            var exists = await _mediator.Send(new IndexExistsQuery());
            return Ok(new { exists });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if index exists");
            return StatusCode(500, new { error = "Failed to check index existence", details = ex.Message });
        }
    }

    /// <summary>
    /// Gets the document count in the search index
    /// </summary>
    [HttpGet("count")]
    public async Task<IActionResult> GetDocumentCount()
    {
        try
        {
            var count = await _mediator.Send(new GetDocumentCountQuery());
            return Ok(new { documentCount = count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting document count");
            return StatusCode(500, new { error = "Failed to get document count", details = ex.Message });
        }
    }

    /// <summary>
    /// Indexes all tax data from the database
    /// </summary>
    [HttpPost("index-data")]
    public async Task<IActionResult> IndexTaxData()
    {
        try
        {
            var result = await _mediator.Send(new IndexTaxDataCommand());
            return Ok(new { message = "Tax data indexed successfully", success = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing tax data");
            return StatusCode(500, new { error = "Failed to index tax data", details = ex.Message });
        }
    }

    /// <summary>
    /// Deletes the search index
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> DeleteIndex()
    {
        try
        {
            var result = await _mediator.Send(new DeleteIndexCommand());
            return Ok(new { message = "Search index deleted successfully", success = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting search index");
            return StatusCode(500, new { error = "Failed to delete search index", details = ex.Message });
        }
    }
}
