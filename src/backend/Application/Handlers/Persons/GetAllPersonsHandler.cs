using Application.Queries.Persons;
using Application.DTOs.Common;
using Application.Mappings.Common;
using Application.Common;
using Domain.Common;
using Application.Handlers.Common;
using Application.Services.Common;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using MediatR;

namespace Application.Handlers.Persons
{
    /// <summary>
    /// Handler for retrieving all persons with pagination
    /// </summary>
    public class GetAllPersonsHandler : BasePagedQueryHandler<GetAllPersonsQuery, PersonDto>,
        IRequestHandler<GetAllPersonsQuery, Result<PagedResult<PersonDto>>>
    {
        private readonly IPersonRepository _personRepository;

        /// <summary>
        /// Initializes a new instance of the GetAllPersonsHandler class
        /// </summary>
        /// <param name="personRepository">The person repository</param>
        /// <param name="logger">The logger</param>
        /// <param name="paginationService">The pagination service</param>
        public GetAllPersonsHandler(
            IPersonRepository personRepository,
            ILogger<GetAllPersonsHandler> logger,
            IPaginationService paginationService) : base(paginationService, logger)
        {
            _personRepository = personRepository;
        }

        /// <summary>
        /// Handles the GetAllPersonsQuery request
        /// </summary>
        /// <param name="request">The query request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A Result containing paginated persons</returns>
        public async Task<Result<PagedResult<PersonDto>>> Handle(
            GetAllPersonsQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Retrieving persons - Page: {Page}, PageSize: {PageSize}",
                    request.page, request.pageSize);

                // Validate pagination parameters
                Result<PaginationValidationResult> validationResult = ValidatePaginationParameters(
                    request.page,
                    request.pageSize,
                    GetAllPersonsQuery.ResourceKey);

                if (validationResult.IsFailure)
                {
                    return Result<PagedResult<PersonDto>>.Failure(validationResult.Error!);
                }

                int actualPageSize = validationResult.Data!.ActualPageSize;

                // Get total count before pagination
                int totalCount = await _personRepository.GetCountAsync(
                    request.firstName,
                    request.lastName,
                    request.birthDate,
                    request.isRegistered,
                    cancellationToken);

                // Get persons
                IEnumerable<Person> persons = await _personRepository.GetAllAsync(
                    request.page,
                    actualPageSize,
                    request.firstName,
                    request.lastName,
                    request.birthDate,
                    request.isRegistered,
                    cancellationToken);

                IEnumerable<PersonDto> personDtos = PersonMapper.ToDtos(persons);

                // Create paged result with actual total count
                PagedResult<PersonDto> pagedResult = CreatePagedResult(
                    personDtos,
                    totalCount,
                    request.page,
                    actualPageSize);

                _logger.LogInformation("Successfully retrieved {Count} persons",
                    personDtos.Count());

                return Result<PagedResult<PersonDto>>.Success(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving persons");
                return Result<PagedResult<PersonDto>>.Failure(
                    "An error occurred while retrieving persons.");
            }
        }
    }
}
