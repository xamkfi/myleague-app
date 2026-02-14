using Application.Queries.Users;
using Application.Common;
using Application.DTOs.Common;
using Application.Mappings.Common;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Handlers.Users;

/// <summary>
/// Handler for getting a user by email
/// </summary>
public class GetUserByEmailHandler : IRequestHandler<GetUserByEmailQuery, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GetUserByEmailHandler> _logger;

    public GetUserByEmailHandler(IUserRepository userRepository, ILogger<GetUserByEmailHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<Result<UserDto>> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving user with email: {Email}", request.Email);

            User? user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogInformation("User with email {Email} not found", request.Email);
                return Result<UserDto>.Failure($"User with email '{request.Email}' not found.");
            }

            UserDto userDto = UserMapper.ToDto(user);
            _logger.LogInformation("Successfully retrieved user with email: {Email}", request.Email);

            return Result<UserDto>.Success(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving user with email: {Email}", request.Email);
            return Result<UserDto>.Failure("An error occurred while retrieving the user.");
        }
    }
}
