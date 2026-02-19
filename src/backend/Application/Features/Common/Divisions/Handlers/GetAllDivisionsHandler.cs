using Application.Queries.Common;
using Application.DTOs.Common;
using Application.Mappings.Common;
using Application.Common;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Handlers.Common;

/// <summary>
/// Handler for retrieving all divisions
/// </summary>
public class GetAllDivisionsHandler : IRequestHandler<GetAllDivisionsQuery, Result<IEnumerable<DivisionDto>>>
{
    private readonly IDivisionRepository _divisionRepository;
    private readonly ILogger<GetAllDivisionsHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetAllDivisionsHandler class
    /// </summary>
    /// <param name="divisionRepository">The division repository</param>
    /// <param name="logger">The logger</param>
    public GetAllDivisionsHandler(IDivisionRepository divisionRepository, ILogger<GetAllDivisionsHandler> logger)
    {
        _divisionRepository = divisionRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetAllDivisionsQuery request
    /// </summary>
    /// <param name="request">The query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>All divisions as DTOs wrapped in a Result</returns>
    public async Task<Result<IEnumerable<DivisionDto>>> Handle(GetAllDivisionsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving all divisions");
            IEnumerable<Domain.Entities.Common.Division> divisions = await _divisionRepository.GetAllAsync();
            
            IEnumerable<DivisionDto> divisionDtos = DivisionMapper.ToDtos(divisions);
            _logger.LogInformation("Successfully retrieved {Count} divisions", divisionDtos.Count());

            return Result<IEnumerable<DivisionDto>>.Success(divisionDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving all divisions");
            return Result<IEnumerable<DivisionDto>>.Failure("An error occurred while retrieving divisions.");
        }
    }
} 