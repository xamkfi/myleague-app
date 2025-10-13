using System;
using Application.Commands.Users;
using Application.Common;
using Application.DTOs.Common;
using Application.Mappings.Common;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Handlers.Users
{
    /// <summary>
    /// Handler for updating an existing user
    /// </summary>
    public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, Result<UserDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateUserHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the UpdateUserHandler class
        /// </summary>
        /// <param name="userRepository">The user repository</param>
        /// <param name="unitOfWork">The unit of work</param>
        /// <param name="logger">The logger</param>
        public UpdateUserHandler(IUserRepository userRepository, IUnitOfWork unitOfWork, ILogger<UpdateUserHandler> logger)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Handles the UpdateUserCommand request
        /// </summary>
        /// <param name="request">The update user command</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result containing the updated user DTO</returns>
        public async Task<Result<UserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Get the existing user
                User? user = await _userRepository.GetByIdAsync(request.Id);
                if (user == null)
                {
                    _logger.LogInformation("Attempt to update non-existent user with ID: {UserId}", request.Id);
                    return Result<UserDto>.Failure($"User with ID '{request.Id}' does not exist.");
                }

                // Check if the new username already exists (for another user)
                if (user.Username != request.Username && await _userRepository.ExistsByUsernameAsync(request.Username))
                {
                    _logger.LogInformation("Attempt to update user with existing username: {Username}", request.Username);
                    return Result<UserDto>.Failure($"A user with the username '{request.Username}' already exists.");
                }

                // Update the user
                UserMapper.UpdateFromCommand(user, request);

                _logger.LogInformation("Updating user: {Username} (ID: {UserId})", user.Username, user.Id);
                await _userRepository.UpdateAsync(user);

                // Save changes explicitly to trigger domain events
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Load the updated user with person data for the response
                User? updatedUser = await _userRepository.GetByIdAsync(user.Id);
                if (updatedUser == null)
                {
                    _logger.LogError("Failed to retrieve updated user with ID: {UserId}", user.Id);
                    return Result<UserDto>.Failure("Failed to retrieve the updated user.");
                }

                UserDto userDto = UserMapper.ToDto(updatedUser);
                _logger.LogInformation("Successfully updated user with ID: {UserId}", user.Id);

                return Result<UserDto>.Success(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating user: {UserId}", request.Id);
                return Result<UserDto>.Failure("An error occurred while updating the user.");
            }
        }
    }
} 