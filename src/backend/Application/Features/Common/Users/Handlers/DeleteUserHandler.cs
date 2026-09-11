using Application.Common;
using Application.Features.Common.Deletion;
using Application.Features.Common.Users.Commands;
using Domain.Entities.Common;
using Domain.Enums.Common;
using Domain.Repositories.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Common.Users.Handlers;

/// <summary>
/// Handler for deleting a user. Blocks self-delete and deleting the last system administrator.
/// </summary>
public class DeleteUserHandler : IRequestHandler<DeleteUserCommand, Result<bool>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteUserHandler> _logger;

    public DeleteUserHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ILogger<DeleteUserHandler> logger)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            User? user = await _userRepository.GetByIdAsync(request.Id);
            if (user == null)
            {
                _logger.LogInformation("Attempt to delete non-existent user with ID: {UserId}", request.Id);
                return Result<bool>.NotFound("User", request.Id);
            }

            if (request.Id == request.RequestedByUserId)
            {
                _logger.LogWarning("User {UserId} attempted to delete their own account", request.Id);
                return Result<bool>.Failure(DeletionReasons.CannotDeleteOwnAccount);
            }

            if (user.Role == UserRole.SystemAdmin)
            {
                int adminCount = await _userRepository.CountByRoleAsync(UserRole.SystemAdmin);
                if (adminCount <= 1)
                {
                    _logger.LogWarning("Blocked delete of the last system administrator {UserId}", request.Id);
                    return Result<bool>.Failure(DeletionReasons.LastSystemAdmin);
                }
            }

            _logger.LogInformation("Deleting user with ID: {UserId}", request.Id);
            await _userRepository.DeleteAsync(request.Id);
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
