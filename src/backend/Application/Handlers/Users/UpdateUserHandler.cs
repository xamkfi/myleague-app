using Application.Commands.Users;
using Application.Common;
using Application.DTOs.Common;
using Application.Mappings.Common;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Handlers.Users;

/// <summary>
/// Handler for updating an existing user
/// </summary>
public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateUserHandler> _logger;

    public UpdateUserHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateUserHandler> logger)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<UserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            User? user = await _userRepository.GetByIdAsync(request.Id);
            if (user == null)
            {
                _logger.LogInformation("Attempt to update non-existent user with ID: {UserId}", request.Id);
                return Result<UserDto>.Failure($"User with ID '{request.Id}' does not exist.");
            }

            // Check if the new email already exists (for another user)
            if (!string.Equals(user.Email, request.Email, StringComparison.OrdinalIgnoreCase)
                && await _userRepository.ExistsByEmailAsync(request.Email))
            {
                _logger.LogInformation("Attempt to update user with existing email: {Email}", request.Email);
                return Result<UserDto>.Failure($"A user with the email '{request.Email}' already exists.");
            }

            UserMapper.UpdateFromCommand(user, request);

            _logger.LogInformation("Updating user: {Email} (ID: {UserId})", user.Email, user.Id);
            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

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
