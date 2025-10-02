using System;
using Application.Queries.Users;
using Application.Common;
using Application.DTOs.Common;
using Application.Mappings.Common;
using Domain.Repositories.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Handlers.Users
{
    /// <summary>
    /// Handler for getting all users
    /// </summary>
    public class GetAllUsersHandler : IRequestHandler<GetAllUsersQuery, Result<IEnumerable<UserDto>>>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<GetAllUsersHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the GetAllUsersHandler class
        /// </summary>
        /// <param name="userRepository">The user repository</param>
        /// <param name="logger">The logger</param>
        public GetAllUsersHandler(IUserRepository userRepository, ILogger<GetAllUsersHandler> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        /// <summary>
        /// Handles the GetAllUsersQuery request
        /// </summary>
        /// <param name="request">The get all users query</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result containing the collection of user DTOs</returns>
        public async Task<Result<IEnumerable<UserDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Retrieving all users");

                IEnumerable<Domain.Entities.Common.User> users = await _userRepository.GetAllAsync();
                _logger.LogInformation("Retrieved {Count} users from repository", users.Count());

                IEnumerable<UserDto> userDtos = UserMapper.ToDtos(users);
                _logger.LogInformation("Successfully mapped {Count} users to DTOs", userDtos.Count());

                return Result<IEnumerable<UserDto>>.Success(userDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all users. Exception: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}", 
                    ex.GetType().Name, ex.Message, ex.StackTrace);
                return Result<IEnumerable<UserDto>>.Failure($"An error occurred while retrieving users: {ex.Message}");
            }
        }
    }
} 