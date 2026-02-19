using Application.Queries.Common;
using Application.DTOs.Common;
using Application.Mappings.Common;
using Application.Common;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Handlers.Common;

/// <summary>
/// Handler for retrieving a division by ID
/// </summary>
public class GetDivisionByIdHandler : IRequestHandler<GetDivisionByIdQuery, Result<DivisionDto>>
{
    private readonly IDivisionRepository _divisionRepository;
    private readonly ILogger<GetDivisionByIdHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetDivisionByIdHandler class
    /// </summary>
    /// <param name="divisionRepository">The division repository</param>
    /// <param name="logger">The logger</param>
    public GetDivisionByIdHandler(IDivisionRepository divisionRepository, ILogger<GetDivisionByIdHandler> logger)
    {
        _divisionRepository = divisionRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetDivisionByIdQuery request
    /// </summary>
    /// <param name="request">The query containing the division ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The division as a DTO wrapped in a Result</returns>
    public async Task<Result<DivisionDto>> Handle(GetDivisionByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving division with ID: {DivisionId}", request.Id);
            Division? division = await _divisionRepository.GetByIdAsync(request.Id);
            
            if (division == null)
            {
                _logger.LogWarning("Division with ID {DivisionId} not found", request.Id);
                return Result<DivisionDto>.Failure($"Division with ID {request.Id} not found.");
            }

            DivisionDto divisionDto = DivisionMapper.ToDto(division);
            _logger.LogInformation("Successfully retrieved division: {DivisionName}", division.Name);

            return Result<DivisionDto>.Success(divisionDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving division with ID: {DivisionId}", request.Id);
            return Result<DivisionDto>.Failure("An error occurred while retrieving the division.");
        }
    }
} 