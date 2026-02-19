using Application.Common;
using Application.DTOs.Common;
using Application.Mappings.Common;
using Application.Queries.Persons;
using Application.Handlers.Common;
using Application.Services.Common;
using Domain.Common;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Handlers.Persons
{
    /// <summary>
    /// Handler for retrieving a person by search name
    /// </summary>
    public class SearchPersonByNameHandler : BasePagedQueryHandler<SearchPersonByNameQuery, PersonDto>,
        IRequestHandler<SearchPersonByNameQuery, Result<PagedResult<PersonDto>>>
    {
        private readonly IPersonRepository _personRepository;

        /// <summary>
        /// Initializes a new instance of the SearchPersonByNameHandler class
        /// </summary>
        /// <param name="personRepository">The person repository</param>
        /// <param name="logger">The logger</param>
        /// <param name="paginationService">The pagination service</param>
        public SearchPersonByNameHandler(
            IPersonRepository personRepository,
            ILogger<SearchPersonByNameHandler> logger,
            IPaginationService paginationService) : base(paginationService, logger)
        {
            _personRepository = personRepository;
        }

        /// <summary>
        /// Handles the SearchPersonByNameQuery request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Result<PagedResult<PersonDto>>> Handle(SearchPersonByNameQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Searching persons by name: {Name} - Page: {Page}, PageSize: {PageSize}",
                    request.name, request.page, request.pageSize);

                // Validate pagination parameters
                Result<PaginationValidationResult> validationResult = ValidatePaginationParameters(
                    request.page,
                    request.pageSize,
                    SearchPersonByNameQuery.ResourceKey);

                if (validationResult.IsFailure)
                {
                    return Result<PagedResult<PersonDto>>.Failure(validationResult.Error!);
                }

                int actualPageSize = validationResult.Data!.ActualPageSize;

                // Get total count and paginated results
                PagedResult<Person> searchResult = await _personRepository.SearchByNameAsync(
                    request.name,
                    request.page,
                    actualPageSize,
                    cancellationToken);

                if (!searchResult.Items.Any())
                {
                    _logger.LogWarning("Person with name {search} not found", request.name);
                    // Return empty paged result instead of NotFound
                    PagedResult<PersonDto> emptyResult = CreatePagedResult(
                        Enumerable.Empty<PersonDto>(),
                        0,
                        request.page,
                        actualPageSize);
                    return Result<PagedResult<PersonDto>>.Success(emptyResult);
                }

                IEnumerable<PersonDto> personDtos = PersonMapper.ToDtos(searchResult.Items);

                // Create paged result
                PagedResult<PersonDto> pagedResult = CreatePagedResult(
                    personDtos,
                    searchResult.TotalCount,
                    request.page,
                    actualPageSize);

                _logger.LogInformation("Successfully retrieved {Count} persons matching '{Name}'",
                    personDtos.Count(), request.name);

                return Result<PagedResult<PersonDto>>.Success(pagedResult);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching person by: {name}", request.name);
                return Result<PagedResult<PersonDto>>.Failure("An error occurred while retrieving the person.");
            }
        }

    }
}
