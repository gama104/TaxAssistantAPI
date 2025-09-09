using Microsoft.AspNetCore.Mvc;
using IRSAssistantAPI.Application.DTOs;
using IRSAssistantAPI.Application.Features.Taxpayers.Queries;
using MediatR;

namespace IRSAssistantAPI.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class TaxpayersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TaxpayersController> _logger;

    public TaxpayersController(IMediator mediator, ILogger<TaxpayersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaxpayerDto>>> GetTaxpayers()
    {
        try
        {
            var taxpayers = await _mediator.Send(new GetTaxpayersQuery());
            return Ok(taxpayers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching taxpayers");
            return StatusCode(500, "An error occurred while fetching taxpayers");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TaxpayerDto>> GetTaxpayer(Guid id)
    {
        try
        {
            var taxpayer = await _mediator.Send(new GetTaxpayerByIdQuery(id));
            
            if (taxpayer == null)
            {
                return NotFound();
            }

            return Ok(taxpayer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching taxpayer {Id}", id);
            return StatusCode(500, "An error occurred while fetching the taxpayer");
        }
    }
}
