using System;
using Application.Features.Common.Users.Queries;
using Application.Common;
using Application.Features.Common.Users.DTOs;
using Application.Features.Common.Persons.DTOs;
using Application.Features.Common.Clubs.DTOs;
using Application.Features.Common.Divisions.DTOs;
using Application.Features.Common.News.DTOs;
using Application.Features.Common.Search.DTOs;
using Application.Features.Common.MatchTimer.DTOs;
using Application.Features.Common.Shared.DTOs;
using Application.Features.Common.Users.Mappings;
using Application.Features.Common.Persons.Mappings;
using Application.Features.Common.Clubs.Mappings;
using Application.Features.Common.Divisions.Mappings;
using Application.Features.Common.News.Mappings;
using Domain.Repositories.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Common.Users.Handlers
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
                IEnumerable<UserDto> userDtos = UserMapper.ToDtos(users);

                _logger.LogInformation("Successfully retrieved {Count} users", userDtos.Count());

                return Result<IEnumerable<UserDto>>.Success(userDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all users");
                return Result<IEnumerable<UserDto>>.Failure("An error occurred while retrieving users.");
            }
        }
    }
} 
