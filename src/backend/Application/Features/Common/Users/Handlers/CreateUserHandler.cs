using Application.Features.Common.Users.Commands;
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
using Domain.Entities.Common;
using Domain.Repositories.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Common.Users.Handlers;

/// <summary>
/// Handler for creating a new user
/// </summary>
public class CreateUserHandler : IRequestHandler<CreateUserCommand, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateUserHandler> _logger;

    public CreateUserHandler(
        IUserRepository userRepository,
        IPersonRepository personRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateUserHandler> logger)
    {
        _userRepository = userRepository;
        _personRepository = personRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if email already exists
            if (await _userRepository.ExistsByEmailAsync(request.Email))
            {
                _logger.LogInformation("Attempt to create user with existing email: {Email}", request.Email);
                return Result<UserDto>.Failure($"A user with the email '{request.Email}' already exists.");
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

            _logger.LogInformation("Creating new user: {Email} for person: {PersonId}", user.Email, user.PersonId);
            await _userRepository.AddAsync(user);
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
            _logger.LogError(ex, "Error occurred while creating user: {Email}", request.Email);
            return Result<UserDto>.Failure("An error occurred while creating the user.");
        }
    }
}
