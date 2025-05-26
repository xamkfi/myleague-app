using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Commands.Person;
using Application.Common;
using Application.DTOs.Common;
using Application.Handlers.Clubs;
using Application.Mappings.Common;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Handlers.Person
{
    /// <summary>
    /// Handler for updating an existing person
    /// </summary>
    public class UpdatePersonCommandHandler : IRequestHandler<UpdatePersonCommand, Result<PersonDto>>
    {
        private readonly IPersonRepository _personRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateClubHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the UpdatePersonHandler class
        /// </summary>
        /// <param name="personRepository"></param>
        /// <param name="unitOfWork"></param>
        /// <param name="logger"></param>
        public UpdatePersonCommandHandler(IPersonRepository personRepository, IUnitOfWork unitOfWork, ILogger<UpdateClubHandler> logger)
        {
            _personRepository = personRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Handles the UpdatePersonCommand request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Result<PersonDto>> Handle(UpdatePersonCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Find the existing club
                Person existingPerson = await _personRepository.GetByIdAsync(request.Id);
                if(existingPerson == null)
                {
                    _logger.LogWarning("Attempt to update non-existent person with ID: {PersonId}", request.Id);
                    return Result<PersonDto>.NotFound("Person", request.Id);
                }

                //Update the person
                PersonMapper.UpdateFromCommand(existingPerson, request);

                _logger.LogInformation("Updating person: {PersonId}", existingPerson.Id);
                await _personRepository.UpdateAsync(existingPerson);

                // Save changes explicitly to trigger domain events
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                PersonDto personDto = PersonMapper.ToDto(existingPerson);
                _logger.LogInformation("Successfully updated person with ID: {PersonId}", existingPerson.Id);

                return Result<PersonDto>.Success(personDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating person: {PersonId}", request.Id);
                return Result<PersonDto>.Failure("An error occurred while updating the person.");
            }
        }
    }
}
