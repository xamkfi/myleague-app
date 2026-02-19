using System;
using Application.Commands.Users;
using Application.Common;
using Domain.Repositories.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Handlers.Users
{
    /// <summary>
    /// Handler for deleting a user
    /// </summary>
    public class DeleteUserHandler : IRequestHandler<DeleteUserCommand, Result<bool>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteUserHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the DeleteUserHandler class
        /// </summary>
        /// <param name="userRepository">The user repository</param>
        /// <param name="unitOfWork">The unit of work</param>
        /// <param name="logger">The logger</param>
        public DeleteUserHandler(IUserRepository userRepository, IUnitOfWork unitOfWork, ILogger<DeleteUserHandler> logger)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Handles the DeleteUserCommand request
        /// </summary>
        /// <param name="request">The delete user command</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result containing true if deletion was successful</returns>
        public async Task<Result<bool>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Check if user exists
                if (!await _userRepository.ExistsAsync(request.Id))
                {
                    _logger.LogInformation("Attempt to delete non-existent user with ID: {UserId}", request.Id);
                    return Result<bool>.Failure($"User with ID '{request.Id}' does not exist.");
                }

                _logger.LogInformation("Deleting user with ID: {UserId}", request.Id);
                await _userRepository.DeleteAsync(request.Id);

                // Save changes explicitly to trigger domain events
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Successfully deleted user with ID: {UserId}", request.Id);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting user: {UserId}", request.Id);
                return Result<bool>.Failure("An error occurred while deleting the user.");
            }
        }
    }
} 