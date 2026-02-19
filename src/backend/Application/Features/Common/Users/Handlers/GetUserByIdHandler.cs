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
    /// Handler for getting a user by ID
    /// </summary>
    public class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<GetUserByIdHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the GetUserByIdHandler class
        /// </summary>
        /// <param name="userRepository">The user repository</param>
        /// <param name="logger">The logger</param>
        public GetUserByIdHandler(IUserRepository userRepository, ILogger<GetUserByIdHandler> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        /// <summary>
        /// Handles the GetUserByIdQuery request
        /// </summary>
        /// <param name="request">The get user by ID query</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result containing the user DTO</returns>
        public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Retrieving user with ID: {UserId}", request.Id);

                User? user = await _userRepository.GetByIdAsync(request.Id);
                if (user == null)
                {
                    _logger.LogInformation("User with ID {UserId} not found", request.Id);
                    return Result<UserDto>.Failure($"User with ID '{request.Id}' not found.");
                }

                UserDto userDto = UserMapper.ToDto(user);
                _logger.LogInformation("Successfully retrieved user with ID: {UserId}", request.Id);

                return Result<UserDto>.Success(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving user with ID: {UserId}", request.Id);
                return Result<UserDto>.Failure("An error occurred while retrieving the user.");
            }
        }
    }
} 