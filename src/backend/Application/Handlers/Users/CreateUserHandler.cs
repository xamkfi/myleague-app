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
    /// Handler for creating a new user
    /// </summary>
    public class CreateUserHandler : IRequestHandler<CreateUserCommand, Result<UserDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateUserHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the CreateUserHandler class
        /// </summary>
        /// <param name="userRepository">The user repository</param>
        /// <param name="personRepository">The person repository</param>
        /// <param name="unitOfWork">The unit of work</param>
        /// <param name="logger">The logger</param>
        public CreateUserHandler(IUserRepository userRepository, IPersonRepository personRepository, IUnitOfWork unitOfWork, ILogger<CreateUserHandler> logger)
        {
            _userRepository = userRepository;
            _personRepository = personRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Handles the CreateUserCommand request
        /// </summary>
        /// <param name="request">The create user command</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result containing the created user DTO</returns>
        public async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Check if username already exists
                if (await _userRepository.ExistsByUsernameAsync(request.Username))
                {
                    _logger.LogInformation("Attempt to create user with existing username: {Username}", request.Username);
                    return Result<UserDto>.Failure($"A user with the username '{request.Username}' already exists.");
                }

                // Check if person exists
                if (!await _personRepository.ExistsAsync(request.PersonId))
                {
                    _logger.LogInformation("Attempt to create user with non-existent person ID: {PersonId}", request.PersonId);
                    return Result<UserDto>.Failure($"Person with ID '{request.PersonId}' does not exist.");
                }

                // Check if person already has a user account
                if (await _userRepository.ExistsByPersonIdAsync(request.PersonId))
                {
                    _logger.LogInformation("Attempt to create user for person who already has an account: {PersonId}", request.PersonId);
                    return Result<UserDto>.Failure($"Person with ID '{request.PersonId}' already has a user account.");
                }

                // Create the User entity
                User user = UserMapper.ToEntity(request);

                _logger.LogInformation("Creating new user: {Username} for person: {PersonId}", user.Username, user.PersonId);
                await _userRepository.AddAsync(user);

                // Save changes explicitly to trigger domain events
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Load the user with person data for the response
                User? createdUser = await _userRepository.GetByIdAsync(user.Id);
                if (createdUser == null)
                {
                    _logger.LogError("Failed to retrieve created user with ID: {UserId}", user.Id);
                    return Result<UserDto>.Failure("Failed to retrieve the created user.");
                }

                UserDto userDto = UserMapper.ToDto(createdUser);
                _logger.LogInformation("Successfully created user with ID: {UserId}", user.Id);

                return Result<UserDto>.Success(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating user: {Username}", request.Username);
                return Result<UserDto>.Failure("An error occurred while creating the user.");
            }
        }
    }
} 