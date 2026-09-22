using Application.Features.Common.Organization.Users.Queries;
using Application.Common;
using Application.Features.Common.Organization.Users.DTOs;
using Application.Features.Common.Organization.Persons.DTOs;
using Application.Features.Common.Organization.Clubs.DTOs;
using Application.Features.Common.Organization.Divisions.DTOs;
using Application.Features.Common.Content.News.DTOs;
using Application.Features.Common.CrossCutting.Search.DTOs;
using Application.Features.Common.CrossCutting.MatchTimer.DTOs;
using Application.Features.Common.Shared.DTOs;
using Application.Features.Common.Organization.Users.Mappings;
using Application.Features.Common.Organization.Persons.Mappings;
using Application.Features.Common.Organization.Clubs.Mappings;
using Application.Features.Common.Organization.Divisions.Mappings;
using Application.Features.Common.Content.News.Mappings;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Common.Organization.Users.Handlers;

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
