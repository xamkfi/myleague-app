using Application.Queries.Common;
using Application.DTOs.Common;
using Application.Mappings.Common;
using Application.Common;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Handlers.Common;

/// <summary>
/// Handler for retrieving divisions by sport type
/// </summary>
public class GetDivisionsBySportTypeHandler : IRequestHandler<GetDivisionsBySportTypeQuery, Result<IEnumerable<DivisionDto>>>
{
    private readonly IDivisionRepository _divisionRepository;
    private readonly ILogger<GetDivisionsBySportTypeHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetDivisionsBySportTypeHandler class
    /// </summary>
    /// <param name="divisionRepository">The division repository</param>
    /// <param name="logger">The logger</param>
    public GetDivisionsBySportTypeHandler(IDivisionRepository divisionRepository, ILogger<GetDivisionsBySportTypeHandler> logger)
    {
        _divisionRepository = divisionRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetDivisionsBySportTypeQuery request
    /// </summary>
    /// <param name="request">The query containing sport type and filter options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Divisions filtered by sport type as DTOs wrapped in a Result</returns>
    public async Task<Result<IEnumerable<DivisionDto>>> Handle(GetDivisionsBySportTypeQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving divisions for sport type: {SportType}, ActiveOnly: {ActiveOnly}", request.SportType, request.ActiveOnly);
            
            IEnumerable<Division> divisions = request.ActiveOnly 
                ? await _divisionRepository.GetActiveBySportTypeAsync(request.SportType)
                : await _divisionRepository.GetBySportTypeAsync(request.SportType);
            
            IEnumerable<DivisionDto> divisionDtos = DivisionMapper.ToDtos(divisions);
            _logger.LogInformation("Successfully retrieved {Count} divisions for {SportType}", divisionDtos.Count(), request.SportType);

            return Result<IEnumerable<DivisionDto>>.Success(divisionDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving divisions for sport type: {SportType}", request.SportType);
            return Result<IEnumerable<DivisionDto>>.Failure("An error occurred while retrieving divisions by sport type.");
        }
    }
} 
