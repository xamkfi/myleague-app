using System;
using Application.Commands.Users;
using Application.Common;
using Application.Mappings.Common;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Handlers.Users
{
    /// <summary>
    /// Handler for updating user password
    /// </summary>
    public class UpdateUserPasswordHandler : IRequestHandler<UpdateUserPasswordCommand, Result<bool>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateUserPasswordHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the UpdateUserPasswordHandler class
        /// </summary>
        /// <param name="userRepository">The user repository</param>
        /// <param name="unitOfWork">The unit of work</param>
        /// <param name="logger">The logger</param>
        public UpdateUserPasswordHandler(IUserRepository userRepository, IUnitOfWork unitOfWork, ILogger<UpdateUserPasswordHandler> logger)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Handles the UpdateUserPasswordCommand request
        /// </summary>
        /// <param name="request">The update password command</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result containing true if password update was successful</returns>
        public async Task<Result<bool>> Handle(UpdateUserPasswordCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Get the existing user
                User? user = await _userRepository.GetByIdAsync(request.Id);
                if (user == null)
                {
                    _logger.LogInformation("Attempt to update password for non-existent user with ID: {UserId}", request.Id);
                    return Result<bool>.Failure($"User with ID '{request.Id}' does not exist.");
                }

                // Verify current password
                if (!UserMapper.VerifyPassword(request.CurrentPassword, user.PasswordHash))
                {
                    _logger.LogInformation("Attempt to update password with invalid current password for user: {UserId}", request.Id);
                    return Result<bool>.Failure("Current password is incorrect.");
                }

                // Update password
                var updateCommand = new UpdateUserCommand(user.Id, user.Username, request.NewPassword);
                UserMapper.UpdateFromCommand(user, updateCommand);

                _logger.LogInformation("Updating password for user: {UserId}", user.Id);
                await _userRepository.UpdateAsync(user);

                // Save changes explicitly to trigger domain events
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Successfully updated password for user with ID: {UserId}", user.Id);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating password for user: {UserId}", request.Id);
                return Result<bool>.Failure("An error occurred while updating the password.");
            }
        }
    }
} 