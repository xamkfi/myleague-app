using Application.Common;
using Application.DTOs.Common;
using Application.Mappings.Common;
using Application.Queries.Persons;
using Domain.Repositories.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using Domain.Entities.Common;

namespace Application.Handlers.Persons
{
    /// <summary>
    /// Handler for retrieving a Person by its ID
    /// </summary>
    public class GetPersonByIdHandler : IRequestHandler<GetPersonByIdQuery, Result<PersonDto>>
    {
        private readonly IPersonRepository _personRepository;
        private readonly ILogger<GetPersonByIdHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the GetPersonByIdHandler class
        /// </summary>
        /// <param name="PersonRepository">The Person repository</param>
        /// <param name="logger">The logger</param>
        public GetPersonByIdHandler(IPersonRepository PersonRepository, ILogger<GetPersonByIdHandler> logger)
        {
            _personRepository = PersonRepository;
            _logger = logger;
        }

        /// <summary>
        /// Handles the GetPersonByIdQuery request
        /// </summary>
        /// <param name="request">The query containing the Person ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The Person as a DTO wrapped in a Result, or a not found result</returns>
        public async Task<Result<PersonDto>> Handle(GetPersonByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Retrieving Person with ID: {PersonId}", request.PersonId);

                Person? person = await _personRepository.GetByIdAsync(request.PersonId);
                if (person == null)
                {
                    _logger.LogWarning("Person with ID {PersonId} not found", request.PersonId);
                    return Result<PersonDto>.NotFound("Person", request.PersonId);
                }

                PersonDto personDto = PersonMapper.ToDto(person);
                _logger.LogInformation("Successfully retrieved Person: {PersonId}", person.Id);

                return Result<PersonDto>.Success(personDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving Person: {PersonId}", request.PersonId);
                return Result<PersonDto>.Failure("An error occurred while retrieving the Person.");
            }
        }
    }
}
