using IRSAssistantAPI.Application.DTOs;
using IRSAssistantAPI.Application.Features.Chat.Commands;
using IRSAssistantAPI.Application.Features.Chat.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IRSAssistantAPI.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class ChatController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IMediator mediator, ILogger<ChatController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Process a user query and get AI response
    /// </summary>
    /// <param name="request">The query request</param>
    /// <returns>AI response with SQL query and data</returns>
    [HttpPost("query")]
    [ProducesResponseType(typeof(QueryResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<QueryResponseDto>> ProcessQuery([FromBody] ProcessQueryRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.UserQuery))
            {
                return BadRequest("User query cannot be empty");
            }

            var command = new ProcessQueryCommand(request.UserQuery, request.SessionId.ToString(), request.TaxpayerId.ToString());
            var result = await _mediator.Send(command);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing query: {Query}", request.UserQuery);
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    /// <summary>
    /// Create a new chat session
    /// </summary>
    /// <param name="request">The create session request</param>
    /// <returns>Created chat session</returns>
    [HttpPost("sessions")]
    [ProducesResponseType(typeof(ChatSessionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ChatSessionDto>> CreateSession([FromBody] CreateSessionRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return BadRequest("Session title cannot be empty");
            }

            var command = new CreateChatSessionCommand(request.Title, request.TaxpayerId);
            var result = await _mediator.Send(command);

            return CreatedAtAction(nameof(GetSession), new { sessionId = result.Id, version = "1.0" }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating session for taxpayer: {TaxpayerId}", request.TaxpayerId);
            return StatusCode(500, "An error occurred while creating the session");
        }
    }

    /// <summary>
    /// Get a specific chat session with messages
    /// </summary>
    /// <param name="sessionId">The session ID</param>
    /// <param name="taxpayerId">The taxpayer ID</param>
    /// <returns>Chat session with messages</returns>
    [HttpGet("sessions/{sessionId}")]
    [ProducesResponseType(typeof(ChatSessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ChatSessionDto>> GetSession(Guid sessionId, [FromQuery] Guid taxpayerId)
    {
        try
        {
            var query = new GetChatSessionQuery(sessionId, taxpayerId);
            var result = await _mediator.Send(query);

            if (result == null)
            {
                return NotFound("Session not found");
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting session: {SessionId}", sessionId);
            return StatusCode(500, "An error occurred while retrieving the session");
        }
    }

    /// <summary>
    /// Get all chat sessions for a user
    /// </summary>
    /// <param name="taxpayerId">The taxpayer ID</param>
    /// <returns>List of user's chat sessions</returns>
    [HttpGet("sessions")]
    [ProducesResponseType(typeof(List<ChatSessionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<ChatSessionDto>>> GetUserSessions([FromQuery] Guid taxpayerId)
    {
        try
        {
            var query = new GetUserChatSessionsQuery(taxpayerId);
            var result = await _mediator.Send(query);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sessions for taxpayer: {TaxpayerId}", taxpayerId);
            return StatusCode(500, "An error occurred while retrieving sessions");
        }
    }

    /// <summary>
    /// Process a query for the frontend (simplified endpoint)
    /// </summary>
    /// <param name="request">The query request</param>
    /// <returns>AI response</returns>
    [HttpPost("process-query")]
    [ProducesResponseType(typeof(QueryResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<QueryResponseDto>> ProcessQuerySimple([FromBody] ProcessQuerySimpleRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Query))
            {
                return BadRequest("Query cannot be empty");
            }

            var command = new ProcessQueryCommand(request.Query, request.SessionId, request.TaxpayerId);
            var result = await _mediator.Send(command);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing query: {Query}", request.Query);
            return StatusCode(500, "An error occurred while processing your request");
        }
    }
}

// Request DTOs
public record ProcessQueryRequest(string UserQuery, Guid SessionId, Guid TaxpayerId);
public record CreateSessionRequest(string Title, Guid TaxpayerId);
public record ProcessQuerySimpleRequest(string Query, string SessionId, string TaxpayerId);
