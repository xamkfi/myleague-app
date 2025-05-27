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
    /// Handler for updating person basic info
    /// </summary>
    public class UpdatePersonBasicInfoHandler : IRequestHandler<UpdatePersonBasicInfoCommand, Result<PersonDto>>
    {
        private readonly IPersonRepository _personRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateClubHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the UpdatePersonBasicInfoHandler class
        /// </summary>
        /// <param name="personRepository"></param>
        /// <param name="unitOfWork"></param>
        /// <param name="logger"></param>
        public UpdatePersonBasicInfoHandler(IPersonRepository personRepository, IUnitOfWork unitOfWork, ILogger<UpdateClubHandler> logger)
        {
            _personRepository = personRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<PersonDto>> Handle(UpdatePersonBasicInfoCommand request, CancellationToken cancellationToken)
        {
            try
            {
                Person? personExists = await _personRepository.GetByIdAsync(request.Id);
                if(personExists == null)
                {
                    return Result<PersonDto>.NotFound("Person", request.Id);
                }

                personExists.UpdateBasicInfo(request.FirstName, request.LastName);

                PersonDto personDto = PersonMapper.ToDto(personExists);

                return Result<PersonDto>.Success(personDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating person basic info: {PersonId}", request.Id);
                return Result<PersonDto>.Failure("An error occurred while updating the firstname and lastname.");
            }
        }
    }
}
