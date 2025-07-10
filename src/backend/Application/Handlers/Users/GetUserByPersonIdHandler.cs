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
    /// Handler for getting a user by person ID
    /// </summary>
    public class GetUserByPersonIdHandler : IRequestHandler<GetUserByPersonIdQuery, Result<UserDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<GetUserByPersonIdHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the GetUserByPersonIdHandler class
        /// </summary>
        /// <param name="userRepository">The user repository</param>
        /// <param name="logger">The logger</param>
        public GetUserByPersonIdHandler(IUserRepository userRepository, ILogger<GetUserByPersonIdHandler> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        /// <summary>
        /// Handles the GetUserByPersonIdQuery request
        /// </summary>
        /// <param name="request">The get user by person ID query</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result containing the user DTO</returns>
        public async Task<Result<UserDto>> Handle(GetUserByPersonIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Retrieving user with person ID: {PersonId}", request.PersonId);

                User? user = await _userRepository.GetByPersonIdAsync(request.PersonId);
                if (user == null)
                {
                    _logger.LogInformation("User with person ID {PersonId} not found", request.PersonId);
                    return Result<UserDto>.Failure($"User with person ID '{request.PersonId}' not found.");
                }

                UserDto userDto = UserMapper.ToDto(user);
                _logger.LogInformation("Successfully retrieved user with person ID: {PersonId}", request.PersonId);

                return Result<UserDto>.Success(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving user with person ID: {PersonId}", request.PersonId);
                return Result<UserDto>.Failure("An error occurred while retrieving the user.");
            }
        }
    }
} 