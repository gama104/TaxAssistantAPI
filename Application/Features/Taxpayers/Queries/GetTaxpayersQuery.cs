using IRSAssistantAPI.Application.DTOs;
using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using IRSAssistantAPI.Application.Common.Interfaces;

namespace IRSAssistantAPI.Application.Features.Taxpayers.Queries;

public record GetTaxpayersQuery : IRequest<IEnumerable<TaxpayerDto>>;

public class GetTaxpayersQueryHandler : IRequestHandler<GetTaxpayersQuery, IEnumerable<TaxpayerDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetTaxpayersQueryHandler> _logger;

    public GetTaxpayersQueryHandler(IDbContext context, IMapper mapper, ILogger<GetTaxpayersQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<TaxpayerDto>> Handle(GetTaxpayersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("=== FETCHING TAXPAYERS - START ===");
            
            var canConnect = await _context.Database.CanConnectAsync();
            _logger.LogInformation("Database connection status: {CanConnect}", canConnect);
            
            if (!canConnect)
            {
                _logger.LogError("Cannot connect to database!");
                return new List<TaxpayerDto>();
            }
            
            var totalCount = await _context.Taxpayers.CountAsync();
            _logger.LogInformation("Total taxpayers in database: {TotalCount}", totalCount);
            
            if (totalCount == 0)
            {
                _logger.LogWarning("No taxpayers found in database!");
                return new List<TaxpayerDto>();
            }
            
            var taxpayers = await _context.Taxpayers
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Found {Count} taxpayers from query", taxpayers.Count);
            
            if (taxpayers.Any())
            {
                var firstTaxpayer = taxpayers.First();
                _logger.LogInformation("First taxpayer: {FirstName} {LastName} - IsActive: {IsActive}", 
                    firstTaxpayer.FirstName, firstTaxpayer.LastName, firstTaxpayer.IsActive);
            }
            
            var mappedTaxpayers = _mapper.Map<IEnumerable<TaxpayerDto>>(taxpayers);
            _logger.LogInformation("Mapped {Count} taxpayers to DTOs", mappedTaxpayers.Count());
            
            if (mappedTaxpayers.Any())
            {
                var firstMapped = mappedTaxpayers.First();
                _logger.LogInformation("First mapped taxpayer: {FirstName} {LastName}", 
                    firstMapped.FirstName, firstMapped.LastName);
            }
            
            _logger.LogInformation("=== FETCHING TAXPAYERS - END ===");
            return mappedTaxpayers;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching taxpayers");
            throw;
        }
    }
}
