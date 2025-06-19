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
    /// Handler for creating a new person
    /// </summary>
    public class CreatePersonHandler : IRequestHandler<CreatePersonCommand, Result<PersonDto>>
    {
        private readonly IPersonRepository _personRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreatePersonHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the CreatePersonHandler class
        /// </summary>
        /// <param name="personRepository">The person repository</param>
        /// <param name="unitOfWork"></param>
        /// <param name="logger"></param>
        public CreatePersonHandler(IPersonRepository personRepository, IUnitOfWork unitOfWork, ILogger<CreatePersonHandler> logger)
        {
            _personRepository = personRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Handles the CreatePersonCommand request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Result<PersonDto>> Handle(CreatePersonCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Check if a person with the same name already exists
                if (await _personRepository.ExistsByFullNameAsync(request.FirstName, request.LastName))
                {
                    _logger.LogInformation("Attempt to create person with existing name: {FirstName} {LastName}", request.FirstName, request.LastName);
                    return Result<PersonDto>.Failure($"A person with the name '{request.FirstName} {request.LastName}' already exists.");
                }

                //Create the Person entity
                Person person = PersonMapper.ToEntity(request);

                _logger.LogInformation("Creating new person: {Person}", person.FullName);
                await _personRepository.AddAsync(person);

                //Save changes explicitly to trigger domain events
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                PersonDto personDto = PersonMapper.ToDto(person);
                _logger.LogInformation("Successfully created person with ID: {PersonId}", person.Id);

                return Result<PersonDto>.Success(personDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating person: {FirstName} {LastName}", request.FirstName, request.LastName);
                return Result<PersonDto>.Failure("An error occurred while creating the person.");
            }
        }

    }
}
