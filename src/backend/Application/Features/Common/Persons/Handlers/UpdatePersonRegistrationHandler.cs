using System;
using Application.Features.Common.Persons.Commands;
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
using Domain.Entities.Common;
using Domain.Repositories.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Common.Persons.Handlers
{
    /// <summary>
    /// Handler for updating person registration status
    /// </summary>
    public class UpdatePersonRegistrationHandler : IRequestHandler<UpdatePersonRegistrationCommand, Result<PersonDto>>
    {
        private readonly IPersonRepository _personRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdatePersonRegistrationHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the UpdatePersonRegistrationHandler class
        /// </summary>
        /// <param name="personRepository"></param>
        /// <param name="unitOfWork"></param>
        /// <param name="logger"></param>
        public UpdatePersonRegistrationHandler(IPersonRepository personRepository, IUnitOfWork unitOfWork, ILogger<UpdatePersonRegistrationHandler> logger)
        {
            _personRepository = personRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Handles the UpdatePersonRegistrationCommand request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Result<PersonDto>> Handle(UpdatePersonRegistrationCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Updating person registration status with ID: {PersonId} to {IsRegistered}", request.Id, request.IsRegistered);

                // Find the existing person
                Person? existingPerson = await _personRepository.GetByIdAsync(request.Id);
                if (existingPerson == null)
                {
                    _logger.LogWarning("Attempt to update registration status for non-existent person with ID: {PersonId}", request.Id);
                    return Result<PersonDto>.NotFound("Person", request.Id);
                }

                // Update the registration status
                existingPerson.UpdateIsRegistered(request.IsRegistered);

                // Save changes explicitly to trigger domain events
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Map to DTO and return
                PersonDto personDto = PersonMapper.ToDto(existingPerson);
                
                _logger.LogInformation("Successfully updated registration status for person with ID: {PersonId} to {IsRegistered}", existingPerson.Id, request.IsRegistered);
                return Result<PersonDto>.Success(personDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating person registration status: {PersonId}", request.Id);
                return Result<PersonDto>.Failure("An error occurred while updating the person's registration status.");
            }
        }
    }
} 
