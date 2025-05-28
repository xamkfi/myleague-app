using Application.Common;
using Application.DTOs.Common;
using Application.Handlers.Clubs;
using Application.Mappings.Common;
using Application.Queries.Persons;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Handlers.Persons
{
    /// <summary>
    /// Handler for retrieving a person by search name
    /// </summary>
    public class SearchPersonByNameHandler : IRequestHandler<SearchPersonByNameQuery, Result<IEnumerable<PersonDto>>>
    {
        private readonly IPersonRepository _personRepository;
        private readonly ILogger<GetClubByIdHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the SearchPersonByNameHandler class
        /// </summary>
        /// <param name="clubRepository">The club repository</param>
        /// <param name="logger">The logger</param>
        public SearchPersonByNameHandler(IPersonRepository personRepository, ILogger<GetClubByIdHandler> logger)
        {
            _personRepository = personRepository;
            _logger = logger;
        }

        /// <summary>
        /// Handles the SearchPersonByNameQuery request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Result<IEnumerable<PersonDto>>> Handle(SearchPersonByNameQuery request, CancellationToken cancellationToken)
        {
            try
            {
                IEnumerable<Person> persons = await _personRepository.SearchByNameAsync(request.name);
                if(!persons.Any())
                {
                    _logger.LogWarning("Person with name {search} not found", request.name);
                    return Result<IEnumerable<PersonDto>>.NotFound("Person", request.name);
                }

                IEnumerable<PersonDto> personDtos = PersonMapper.ToDtos(persons);

                return Result<IEnumerable<PersonDto>>.Success(personDtos);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching person by: {name}", request.name);
                return Result<IEnumerable<PersonDto>>.Failure("An error occurred while retrieving the person.");
            }
        }

    }
}
