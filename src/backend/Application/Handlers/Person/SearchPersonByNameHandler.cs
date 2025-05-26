
using Application.Common;
using Application.DTOs.Common;
using Application.Handlers.Clubs;
using Application.Mappings.Common;
using Application.Queries.Person;
using Domain.Repositories.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Handlers.Person
{
    public class SearchPersonByNameHandler : IRequestHandler<SearchPersonByNameQuery, Result<PersonDto>>
    {
        private readonly IClubRepository _clubRepository;
        private readonly ILogger<GetClubByIdHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the SearchPersonByNameHandler class
        /// </summary>
        /// <param name="clubRepository">The club repository</param>
        /// <param name="logger">The logger</param>
        public SearchPersonByNameHandler(IClubRepository clubRepository, ILogger<GetClubByIdHandler> logger)
        {
            _clubRepository = clubRepository;
            _logger = logger;
        }

        /// <summary>
        /// Handles the SearchPersonByNameQuery request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Result<PersonDto>> Handle(SearchPersonByNameQuery request, CancellationToken cancellationToken)
        {
            try
            {
                Person person = await _clubRepository.SearchByNameAsync(request.name);
                if(person == null)
                {
                    _logger.LogWarning("Person with name {search} not found", request.name);
                    return Result<PersonDto>.NotFound("Person", request.name);
                }

                PersonDto personDto = PersonMapper.ToDto(person);

                return Result<PersonDto>.Success(personDto);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching person by: {name}", request.name);
                return Result<PersonDto>.Failure("An error occurred while retrieving the person.");
            }
        }

    }
}
