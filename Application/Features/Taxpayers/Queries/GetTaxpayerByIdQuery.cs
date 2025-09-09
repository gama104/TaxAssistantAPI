using IRSAssistantAPI.Application.DTOs;
using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using IRSAssistantAPI.Application.Common.Interfaces;

namespace IRSAssistantAPI.Application.Features.Taxpayers.Queries;

public record GetTaxpayerByIdQuery(Guid Id) : IRequest<TaxpayerDto?>;

public class GetTaxpayerByIdQueryHandler : IRequestHandler<GetTaxpayerByIdQuery, TaxpayerDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetTaxpayerByIdQueryHandler> _logger;

    public GetTaxpayerByIdQueryHandler(IDbContext context, IMapper mapper, ILogger<GetTaxpayerByIdQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<TaxpayerDto?> Handle(GetTaxpayerByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Fetching taxpayer with ID: {Id}", request.Id);
            
            var taxpayer = await _context.Taxpayers
                .Where(t => t.Id == request.Id && t.IsActive)
                .FirstOrDefaultAsync(cancellationToken);

            if (taxpayer == null)
            {
                _logger.LogWarning("Taxpayer with ID {Id} not found", request.Id);
                return null;
            }

            _logger.LogInformation("Found taxpayer: {FirstName} {LastName}", taxpayer.FirstName, taxpayer.LastName);
            
            return _mapper.Map<TaxpayerDto>(taxpayer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching taxpayer with ID: {Id}", request.Id);
            throw;
        }
    }
}
