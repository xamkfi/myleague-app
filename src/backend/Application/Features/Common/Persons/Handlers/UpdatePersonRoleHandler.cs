using System;
using Application.Commands.Persons;
using Application.Common;
using Application.DTOs.Common;
using Application.Mappings.Common;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Handlers.Persons
{
    /// <summary>
    /// Handler for updating person role
    /// </summary>
    public class UpdatePersonRoleHandler : IRequestHandler<UpdatePersonRoleCommand, Result<PersonDto>>
    {
        private readonly IPersonRepository _personRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdatePersonRoleHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the UpdatePersonRoleHandler class
        /// </summary>
        /// <param name="personRepository">The person repository</param>
        /// <param name="unitOfWork">The unit of work</param>
        /// <param name="logger">The logger</param>
        public UpdatePersonRoleHandler(IPersonRepository personRepository, IUnitOfWork unitOfWork, ILogger<UpdatePersonRoleHandler> logger)
        {
            _personRepository = personRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Handles the UpdatePersonRoleCommand request
        /// </summary>
        /// <param name="request">The update person role command</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result containing the updated person DTO</returns>
        public async Task<Result<PersonDto>> Handle(UpdatePersonRoleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Get the existing person
                Person? person = await _personRepository.GetByIdAsync(request.Id);
                if (person == null)
                {
                    _logger.LogInformation("Attempt to update role for non-existent person with ID: {PersonId}", request.Id);
                    return Result<PersonDto>.Failure($"Person with ID '{request.Id}' does not exist.");
                }

                // Update the person's role
                person.UpdateRole(request.Role);

                _logger.LogInformation("Updating role for person: {PersonId} to {Role}", person.Id, request.Role);
                await _personRepository.UpdateAsync(person);

                // Save changes explicitly to trigger domain events
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                PersonDto personDto = PersonMapper.ToDto(person);
                _logger.LogInformation("Successfully updated role for person with ID: {PersonId}", person.Id);

                return Result<PersonDto>.Success(personDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating role for person: {PersonId}", request.Id);
                return Result<PersonDto>.Failure("An error occurred while updating the person's role.");
            }
        }
    }
} 