// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Commands.Persons;
using Application.Common;
using Application.DTOs.Common;
using Application.Handlers.Clubs;
using Application.Mappings.Common;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Handlers.Persons
{
    /// <summary>
    /// Handler for updating person contactinfo
    /// </summary>
    public class UpdatePersonContactInfoHandler : IRequestHandler<UpdatePersonContactInfoCommand, Result<ContactInfoDto>>
    {
        private readonly IPersonRepository _personRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateClubHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the UpdatePersonContactInfoHandler class
        /// </summary>
        /// <param name="personRepository"></param>
        /// <param name="unitOfWork"></param>
        /// <param name="logger"></param>
        public UpdatePersonContactInfoHandler(IPersonRepository personRepository, IUnitOfWork unitOfWork, ILogger<UpdateClubHandler> logger)
        {
            _personRepository = personRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// 
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

                return Result<ContactInfoDto>.Success(contactInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating person address: {PersonId}", request.Id);
                return Result<ContactInfoDto>.Failure("An error occurred while updating the person address.");
            }
        }
    }
}
