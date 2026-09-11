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
    /// Handler for updating person basic info
    /// </summary>
    public class UpdatePersonBasicInfoHandler : IRequestHandler<UpdatePersonBasicInfoCommand, Result<PersonDto>>
    {
        private readonly IPersonRepository _personRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdatePersonBasicInfoHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the UpdatePersonBasicInfoHandler class
        /// </summary>
        /// <param name="personRepository"></param>
        /// <param name="unitOfWork"></param>
        /// <param name="logger"></param>
        public UpdatePersonBasicInfoHandler(IPersonRepository personRepository, IUnitOfWork unitOfWork, ILogger<UpdatePersonBasicInfoHandler> logger)
        {
            _personRepository = personRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Handles the UpdatePersonBasicInfoCommand request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Result<PersonDto>> Handle(UpdatePersonBasicInfoCommand request, CancellationToken cancellationToken)
        {
            try
            {
                //Check if the person exists
                Person? personExists = await _personRepository.GetByIdAsync(request.Id);
                if(personExists == null)
                {
                    return Result<PersonDto>.NotFound("Person", request.Id);
                }

                //Update name and lastname
                personExists.UpdateBasicInfo(request.FirstName, request.LastName);

                //save changes to database
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                //Create dto for returning data
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
