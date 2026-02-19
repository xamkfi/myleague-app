using System;
using Application.Common;
using Application.Features.Common.Users.DTOs;
using Application.Features.Common.Persons.DTOs;
using Application.Features.Common.Clubs.DTOs;
using Application.Features.Common.Divisions.DTOs;
using Application.Features.Common.News.DTOs;
using Application.Features.Common.Search.DTOs;
using Application.Features.Common.MatchTimer.DTOs;
using Application.Features.Common.Shared.DTOs;
using Application.Features.Common.Users.Mappings;
using Application.Features.Common.Persons.Mappings;
using Application.Features.Common.Clubs.Mappings;
using Application.Features.Common.Divisions.Mappings;
using Application.Features.Common.News.Mappings;
using Application.Features.Common.Persons.Queries;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Common.Persons.Handlers
{
    /// <summary>
    /// Handler for retrieving person by email
    /// </summary>
    public class GetPersonByEmailHandler : IRequestHandler<GetPersonByEmailQuery, Result<PersonDto>>
    {
        private readonly IPersonRepository _personRepository;
        private readonly ILogger<GetPersonByEmailHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the GetPersonByEmailHandler class
        /// </summary>
        /// <param name="PersonRepository">The Person repository</param>
        /// <param name="logger">The logger</param>
        public GetPersonByEmailHandler(IPersonRepository PersonRepository, ILogger<GetPersonByEmailHandler> logger)
        {
            _personRepository = PersonRepository;
            _logger = logger;
        }

        /// <summary>
        /// Handles for GetPersonByEmailQuery request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Result<PersonDto>> Handle(GetPersonByEmailQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Retrieving person with email: {Email}", request.email);

                Person? person = await _personRepository.GetByEmailAsync(request.email);
                if(person == null)
                {
                    _logger.LogWarning("Person with email {Email} not found", request.email);
                    return Result<PersonDto>.NotFound("Email", request.email);
                }

                PersonDto personDto = PersonMapper.ToDto(person);

                _logger.LogInformation("Successfully retrieved person with email: {email}", request.email);
                return Result<PersonDto>.Success(personDto);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving person by email: {email}", request.email);
                return Result<PersonDto>.Failure("An error occurred while retrieving the person by email.");
            }
        }
    }
}
