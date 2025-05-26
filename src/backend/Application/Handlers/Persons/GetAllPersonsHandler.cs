using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.DTOs.Common;
using Application.Mappings.Common;
using Application.Queries.Clubs;
using Application.Queries.Persons;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Handlers.Persons
{
    public class GetAllPersonsHandler : IRequestHandler<GetAllPersonsQuery, Result<IEnumerable<PersonDto>>>
    {
        private readonly IPersonRepository _personRepository;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the GetAllPersonsHandler class
        /// </summary>
        /// <param name="personRepository"></param>
        /// <param name="logger"></param>
        public GetAllPersonsHandler(IPersonRepository personRepository, ILogger logger)
        {
            _personRepository = personRepository;
            _logger = logger;
        }

        /// <summary>
        /// Handles the GetAllPersonQuery request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Result<IEnumerable<PersonDto>>> Handle(GetAllPersonsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Retrieving all Persons");

                IEnumerable<Person> persons = await _personRepository.GetAllAsync();
                IEnumerable<PersonDto> personDtos = PersonMapper.ToDtos(persons);

                _logger.LogInformation("Successfully retrieved {PersonCount} persons", personDtos.Count());

                return Result<IEnumerable<PersonDto>>.Success(personDtos);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all persons");
                return Result<IEnumerable<PersonDto>>.Failure("An error occurred while retrieving persons.");
            }
        }
    }
}
