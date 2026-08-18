using Application.Features.Common.Persons.DTOs;
using Application.Features.Football.Referees.Queries;
using Application.Features.Football.Teams.DTOs;
using Application.Features.Football.Players.DTOs;
using Application.Features.Football.Referees.DTOs;
using Application.Features.Football.TeamManagers.DTOs;
using Application.Features.Football.Teams.Mappings;
using Application.Features.Football.Players.Mappings;
using Application.Features.Football.Referees.Mappings;
using Application.Features.Football.TeamManagers.Mappings;
using Application.Common;
using Domain.Common;
using Application.Services.Common;
using Domain.Entities.Football.Teams;
using Domain.Repositories.Football;
using Domain.Repositories.Common;
using Domain.Entities.Common;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Features.Common.Users.Mappings;
using Application.Features.Common.Persons.Mappings;
using Application.Features.Common.Clubs.Mappings;
using Application.Features.Common.Divisions.Mappings;
using Application.Features.Common.News.Mappings;

namespace Application.Features.Football.Referees.Handlers;

/// <summary>
/// Handler for retrieving paginated football referees with filtering support
/// </summary>
public class GetAllFootballRefereesHandler : BasePagedQueryHandler<GetAllFootballRefereesQuery, FootballRefereeDto>,
    IRequestHandler<GetAllFootballRefereesQuery, Result<PagedResult<FootballRefereeDto>>>
{
    private readonly IFootballRefereeRepository _refereeRepository;
    private readonly IPersonRepository _personRepository;

    /// <summary>
    /// Initializes a new instance of the GetAllFootballRefereesHandler class
    /// </summary>
    /// <param name="refereeRepository">The football referee repository</param>
    /// <param name="personRepository">The person repository</param>
    /// <param name="paginationService">The pagination service</param>
    /// <param name="logger">The logger</param>
    public GetAllFootballRefereesHandler(
        IFootballRefereeRepository refereeRepository,
        IPersonRepository personRepository,
        IPaginationService paginationService,
        ILogger<GetAllFootballRefereesHandler> logger) : base(paginationService, logger)
    {
        _refereeRepository = refereeRepository;
        _personRepository = personRepository;
    }

    /// <summary>
    /// Handles the GetAllFootballRefereesQuery request
    /// </summary>
    /// <param name="request">The query containing pagination and filtering parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A paginated collection of football referees as DTOs wrapped in a Result</returns>
    public async Task<Result<PagedResult<FootballRefereeDto>>> Handle(GetAllFootballRefereesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Check for cancellation before starting
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation("Retrieving football referees - Page: {Page}, PageSize: {PageSize}, IsActive: {IsActive}, SearchTerm: {SearchTerm}, LicenseExpiringWithinDays: {LicenseExpiringWithinDays}", 
                request.Page, request.PageSize, request.IsActive, request.SearchTerm, request.LicenseExpiringWithinDays);

            // Validate pagination parameters using base handler
            Result<PaginationValidationResult> validationResult = ValidatePaginationParameters(
                request.Page, request.PageSize, GetAllFootballRefereesQuery.ResourceKey);
            
            if (validationResult.IsFailure)
            {
                return Result<PagedResult<FootballRefereeDto>>.Failure(validationResult.Error!);
            }

            int actualPageSize = validationResult.Data!.ActualPageSize;

            // Check for cancellation before database operations
            cancellationToken.ThrowIfCancellationRequested();

            // Get paginated referees using database-level pagination
            PagedResult<FootballReferee> pagedReferees = await _refereeRepository.GetPagedAsync(
                request.Page,
                actualPageSize,
                request.IsActive,
                request.SearchTerm,
                request.LicenseExpiringWithinDays,
                cancellationToken);

            // Check for cancellation after database operations
            cancellationToken.ThrowIfCancellationRequested();

            // Load Person data for each referee
            List<FootballRefereeDto> refereeDtos = new List<FootballRefereeDto>();
            foreach (FootballReferee referee in pagedReferees.Items)
            {
                // Get the associated person
                Person? person = await _personRepository.GetByIdAsync(referee.PersonId);
                if (person != null)
                {
                    // Create DTO with real person data
                    FootballRefereeDto refereeDto = new FootballRefereeDto(
                        referee.Id,
                        referee.PersonId,
                        PersonMapper.ToDto(person),
                        referee.IsActive,
                        referee.LicenseIssueDate,
                        referee.LicenseExpiryDate,
                        referee.MatchesOfficiated
                    );
                    refereeDtos.Add(refereeDto);
                }
                else
                {
                    _logger.LogWarning("Person with ID {PersonId} not found for referee {RefereeId}, using placeholder", referee.PersonId, referee.Id);
                    
                    // Create DTO with placeholder person data
                    PersonDto placeholderPerson = new PersonDto(
                        referee.PersonId,
                        "Unknown",
                        "Person",
                        DateTime.MinValue,
                        "Unknown Person",
                        Domain.Enums.Common.PersonRole.User,
                        false,
                        null,
                        null
                    );
                    
                    FootballRefereeDto refereeDto = new FootballRefereeDto(
                        referee.Id,
                        referee.PersonId,
                        placeholderPerson,
                        referee.IsActive,
                        referee.LicenseIssueDate,
                        referee.LicenseExpiryDate,
                        referee.MatchesOfficiated
                    );
                    refereeDtos.Add(refereeDto);
                }
            }
            
            PagedResult<FootballRefereeDto> pagedResult = CreatePagedResult(
                refereeDtos, 
                pagedReferees.TotalCount, 
                pagedReferees.Page, 
                pagedReferees.PageSize);
            
            _logger.LogInformation("Successfully retrieved {Count} football referees out of {TotalCount} total", 
                pagedReferees.ItemCount, pagedReferees.TotalCount);

            return Result<PagedResult<FootballRefereeDto>>.Success(pagedResult);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Football referees retrieval was cancelled - Page: {Page}, PageSize: {PageSize}", 
                request.Page, request.PageSize);
            throw; // Re-throw to let the framework handle it
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving football referees");
            return Result<PagedResult<FootballRefereeDto>>.Failure("An error occurred while retrieving football referees.");
        }
    }
} 
