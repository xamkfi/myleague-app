using System;
using Application.Features.Common.Organization.Persons.Commands;
using Application.Common;
using Application.Features.Common.Organization.Users.DTOs;
using Application.Features.Common.Organization.Persons.DTOs;
using Application.Features.Common.Organization.Clubs.DTOs;
using Application.Features.Common.Organization.Divisions.DTOs;
using Application.Features.Common.Content.News.DTOs;
using Application.Features.Common.CrossCutting.Search.DTOs;
using Application.Features.Common.CrossCutting.MatchTimer.DTOs;
using Application.Features.Common.Shared.DTOs;
using Application.Features.Common.Organization.Users.Mappings;
using Application.Features.Common.Organization.Persons.Mappings;
using Application.Features.Common.Organization.Clubs.Mappings;
using Application.Features.Common.Organization.Divisions.Mappings;
using Application.Features.Common.Content.News.Mappings;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Common.Organization.Persons.Handlers
{
    /// <summary>
    /// Handler for updating person contactinfo
    /// </summary>
    public class UpdatePersonContactInfoHandler : IRequestHandler<UpdatePersonContactInfoCommand, Result<ContactInfoDto>>
    {
        private readonly IPersonRepository _personRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdatePersonContactInfoHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the UpdatePersonContactInfoHandler class
        /// </summary>
        /// <param name="personRepository"></param>
        /// <param name="unitOfWork"></param>
        /// <param name="logger"></param>
        public UpdatePersonContactInfoHandler(IPersonRepository personRepository, IUnitOfWork unitOfWork, ILogger<UpdatePersonContactInfoHandler> logger)
        {
            _personRepository = personRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Handles the UpdatePersonContactInfoCommand request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Result<ContactInfoDto>> Handle(UpdatePersonContactInfoCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Find the existing person
                Person? existingPerson = await _personRepository.GetByIdAsync(request.Id);
                if (existingPerson == null)
                {
                    _logger.LogWarning("Attempt to update non-existent person with ID: {PersonId}", request.Id);
                    return Result<ContactInfoDto>.NotFound("Person", request.Id);
                }

                //Update the contactInfo
                existingPerson.UpdateContactInfo(request.contactInfo);

                // Save changes explicitly to trigger domain events
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                //Create dto return
                ContactInfoDto? contactInfo = PersonMapper.ToContactInfoDto(request.contactInfo);

                return Result<ContactInfoDto>.Success(contactInfo!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating person address: {PersonId}", request.Id);
                return Result<ContactInfoDto>.Failure("An error occurred while updating the person address.");
            }
        }
    }
}
