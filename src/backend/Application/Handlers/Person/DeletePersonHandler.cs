using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Commands.Clubs;
using Application.Commands.Person;
using Application.Common;
using Application.Handlers.Clubs;
using Domain.Repositories.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Handlers.Person
{
    public class DeletePersonHandler : IRequestHandler<DeletePersonCommand, Result>
    {
        private readonly IPersonRepository _personRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteClubHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the DeletePersonHandler class
        /// </summary>
        /// <param name="personRepository"></param>
        /// <param name="unitOfWork"></param>
        /// <param name="logger"></param>
        public DeletePersonHandler(IPersonRepository personRepository, IUnitOfWork unitOfWork, ILogger<DeleteClubHandler> logger)
        {
            _personRepository = personRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Handles the DeletePersonCommand request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Result> Handle(DeletePersonCommand request, CancellationToken cancellationToken)
        {
            try
            {
                //Check if the person exists
                bool personExists = await _personRepository.ExistsAsync(request.Id);
                if (!personExists)
                {
                    _logger.LogWarning("Attempt to delete non-existent person with ID: {PersonId}", request.Id);
                    return Result.NotFound("Person", request.Id);
                }

                _logger.LogInformation("Deleting person with ID: {PersonId}", request.Id);
                await _personRepository.DeleteAsync(request.Id);

                //Save changes explicitly to trigger domain events
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Successfully deleted person with ID: {PersonId}", request.Id);
                return Result.Success();
            }
            catch(Exception ex) 
            {
                _logger.LogError(ex, "Error occurred while deleting club: {PersonId}", request.Id);
                return Result.Failure("An error occurred while deleting the person.");
            }
        }
    }
}
