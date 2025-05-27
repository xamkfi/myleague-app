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
using Application.Validators.Commands.Person;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Handlers.Persons
{
    /// <summary>
    /// Handler for updating a person address
    /// </summary>
    public class UpdatePersonAddressHandler : IRequestHandler<UpdatePersonAddressCommand, Result<AddressDto>>
    {
        private readonly IPersonRepository _personRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateClubHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the UpdatePersonAddressCommandHandler class
        /// </summary>
        /// <param name="personRepository"></param>
        /// <param name="unitOfWork"></param>
        /// <param name="logger"></param>
        public UpdatePersonAddressHandler(IPersonRepository personRepository, IUnitOfWork unitOfWork, ILogger<UpdateClubHandler> logger)
        {
            _personRepository = personRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<AddressDto>> Handle(UpdatePersonAddressCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Find the existing person
                Person? existingPerson = await _personRepository.GetByIdAsync(request.Id);
                if (existingPerson == null)
                {
                    _logger.LogWarning("Attempt to update non-existent person with ID: {PersonId}", request.Id);
                    return Result<AddressDto>.NotFound("Person", request.Id);
                }

                //Update the Address
                existingPerson.UpdateAddress(request.address);

                // Save changes explicitly to trigger domain events
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                //Create dto return
                AddressDto? addressDto = PersonMapper.ToAddressDto(request.address);

                return Result<AddressDto>.Success(addressDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating person address: {PersonId}", request.Id);
                return Result<AddressDto>.Failure("An error occurred while updating the person address.");
            }
        }
    }
}
