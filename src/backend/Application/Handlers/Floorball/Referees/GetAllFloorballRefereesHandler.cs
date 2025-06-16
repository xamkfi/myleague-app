using Application.Queries.Floorball.Referee;
using Application.DTOs.Floorball;
using Application.Mappings.Floorball;
using Application.Common;
using Domain.Common;
using Application.Handlers.Common;
using Application.Services.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using Domain.Repositories.Common;
using Domain.Entities.Common;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Mappings.Common;

namespace Application.Handlers.Floorball.Referees;

/// <summary>
/// Handler for retrieving paginated floorball referees with filtering support
/// </summary>
public class GetAllFloorballRefereesHandler : BasePagedQueryHandler<GetAllFloorballRefereesQuery, FloorballRefereeDto>,
    IRequestHandler<GetAllFloorballRefereesQuery, Result<PagedResult<FloorballRefereeDto>>>
{
    private readonly IFloorballRefereeRepository _refereeRepository;
    private readonly IPersonRepository _personRepository;

    /// <summary>
    /// Initializes a new instance of the GetAllFloorballRefereesHandler class
    /// </summary>
    /// <param name="refereeRepository">The floorball referee repository</param>
    /// <param name="personRepository">The person repository</param>
    /// <param name="paginationService">The pagination service</param>
    /// <param name="logger">The logger</param>
    public GetAllFloorballRefereesHandler(
        IFloorballRefereeRepository refereeRepository,
        IPersonRepository personRepository,
        IPaginationService paginationService,
        ILogger<GetAllFloorballRefereesHandler> logger) : base(paginationService, logger)
    {
        _refereeRepository = refereeRepository;
        _personRepository = personRepository;
    }

    /// <summary>
    /// Handles the GetAllFloorballRefereesQuery request
    /// </summary>
    /// <param name="request">The query containing pagination and filtering parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A paginated collection of floorball referees as DTOs wrapped in a Result</returns>
    public async Task<Result<PagedResult<FloorballRefereeDto>>> Handle(GetAllFloorballRefereesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Check for cancellation before starting
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation("Retrieving floorball referees - Page: {Page}, PageSize: {PageSize}, IsActive: {IsActive}, SearchTerm: {SearchTerm}, LicenseExpiringWithinDays: {LicenseExpiringWithinDays}", 
                request.Page, request.PageSize, request.IsActive, request.SearchTerm, request.LicenseExpiringWithinDays);

            // Validate pagination parameters using base handler
            Result<PaginationValidationResult> validationResult = ValidatePaginationParameters(
                request.Page, request.PageSize, GetAllFloorballRefereesQuery.ResourceKey);
            
            if (validationResult.IsFailure)
            {
                return Result<PagedResult<FloorballRefereeDto>>.Failure(validationResult.Error!);
            }

            int actualPageSize = validationResult.Data!.ActualPageSize;

            // Check for cancellation before database operations
            cancellationToken.ThrowIfCancellationRequested();

            // Get paginated referees using database-level pagination
            PagedResult<FloorballReferee> pagedReferees = await _refereeRepository.GetPagedAsync(
                request.Page,
                actualPageSize,
                request.IsActive,
                request.SearchTerm,
                request.LicenseExpiringWithinDays,
                cancellationToken);
            
            // Load all persons for DTO mapping (since Person navigation is ignored in FloorballReferee)
            IEnumerable<Person> persons = await _personRepository.GetAllAsync();
            Dictionary<Guid, Person> personDictionary = new Dictionary<Guid, Person>();
            foreach (Person person in persons)
            {
                personDictionary[person.Id] = person;
            }

            // Check for cancellation after database operations
            cancellationToken.ThrowIfCancellationRequested();

            // Map to DTOs
            List<FloorballRefereeDto> refereeDtos = new List<FloorballRefereeDto>();
            foreach (FloorballReferee referee in pagedReferees.Items)
            {
                if (personDictionary.TryGetValue(referee.PersonId, out Person? person))
                {
                    FloorballRefereeDto refereeDto = new FloorballRefereeDto(
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
                    _logger.LogWarning("Person with ID {PersonId} not found for referee {RefereeId}", referee.PersonId, referee.Id);
                }
            }
            
            PagedResult<FloorballRefereeDto> pagedResult = CreatePagedResult(
                refereeDtos, 
                pagedReferees.TotalCount, 
                pagedReferees.Page, 
                pagedReferees.PageSize);
            
            _logger.LogInformation("Successfully retrieved {Count} floorball referees out of {TotalCount} total", 
                pagedReferees.ItemCount, pagedReferees.TotalCount);

            return Result<PagedResult<FloorballRefereeDto>>.Success(pagedResult);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Floorball referees retrieval was cancelled - Page: {Page}, PageSize: {PageSize}", 
                request.Page, request.PageSize);
            throw; // Re-throw to let the framework handle it
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving floorball referees");
            return Result<PagedResult<FloorballRefereeDto>>.Failure("An error occurred while retrieving floorball referees.");
        }
    }
} 