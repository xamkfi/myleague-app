using System;
using Application.Queries.Users;
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
    /// Handler for getting a user by username
    /// </summary>
    public class GetUserByUsernameHandler : IRequestHandler<GetUserByUsernameQuery, Result<UserDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<GetUserByUsernameHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the GetUserByUsernameHandler class
        /// </summary>
        /// <param name="userRepository">The user repository</param>
        /// <param name="logger">The logger</param>
        public GetUserByUsernameHandler(IUserRepository userRepository, ILogger<GetUserByUsernameHandler> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        /// <summary>
        /// Handles the GetUserByUsernameQuery request
        /// </summary>
        /// <param name="request">The get user by username query</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result containing the user DTO</returns>
        public async Task<Result<UserDto>> Handle(GetUserByUsernameQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Retrieving user with username: {Username}", request.Username);

                User? user = await _userRepository.GetByUsernameAsync(request.Username);
                if (user == null)
                {
                    _logger.LogInformation("User with username {Username} not found", request.Username);
                    return Result<UserDto>.Failure($"User with username '{request.Username}' not found.");
                }

                UserDto userDto = UserMapper.ToDto(user);
                _logger.LogInformation("Successfully retrieved user with username: {Username}", request.Username);

                return Result<UserDto>.Success(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving user with username: {Username}", request.Username);
                return Result<UserDto>.Failure("An error occurred while retrieving the user.");
            }
        }
    }
} 