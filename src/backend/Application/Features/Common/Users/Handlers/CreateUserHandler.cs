using Application.Features.Common.Users.Commands;
using Application.Common;
using Application.Configuration;
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
using Application.Interfaces.Auth;
using Domain.Entities.Common;
using Domain.Entities.Floorball;
using Domain.Entities.Football.Teams;
using Domain.Enums.Common;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace Application.Features.Common.Users.Handlers;

/// <summary>
/// Handler for creating a new user
/// </summary>
public class CreateUserHandler : IRequestHandler<CreateUserCommand, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly FrontendConfiguration _frontendConfig;
    private readonly IFloorballTeamManagerRepository _floorballTeamManagerRepository;
    private readonly IFootballTeamManagerRepository _footballTeamManagerRepository;
    private readonly IFloorballUnitOfWork _floorballUnitOfWork;
    private readonly IFootballUnitOfWork _footballUnitOfWork;
    private readonly ILogger<CreateUserHandler> _logger;

    public CreateUserHandler(
        IUserRepository userRepository,
        IPersonRepository personRepository,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IOptions<FrontendConfiguration> frontendConfig,
        IFloorballTeamManagerRepository floorballTeamManagerRepository,
        IFootballTeamManagerRepository footballTeamManagerRepository,
        IFloorballUnitOfWork floorballUnitOfWork,
        IFootballUnitOfWork footballUnitOfWork,
        ILogger<CreateUserHandler> logger)
    {
        _userRepository = userRepository;
        _personRepository = personRepository;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _frontendConfig = frontendConfig.Value;
        _floorballTeamManagerRepository = floorballTeamManagerRepository;
        _footballTeamManagerRepository = footballTeamManagerRepository;
        _floorballUnitOfWork = floorballUnitOfWork;
        _footballUnitOfWork = footballUnitOfWork;
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
            Person? person = await _personRepository.GetByIdAsync(request.PersonId);
            if (person == null)
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

            // Create the User entity; all admin users start inactive pending email verification
            User user = UserMapper.ToEntity(request);
            user.IsActive = false;

            string token = GenerateVerificationToken();
            user.SetEmailVerificationToken(token, DateTime.UtcNow.AddHours(48));

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

            // Team leaders get manager links to the teams they were invited for
            if (request.Role == UserRole.TeamLeader && request.TeamAssignments is { Count: > 0 })
            {
                await CreateTeamManagerLinksAsync(request.PersonId, request.TeamAssignments, cancellationToken);
            }

            // Send invitation email with verification link. Team leaders verify through their
            // own area so they land on the team leader login afterwards.
            string verifyPath = request.Role == UserRole.TeamLeader
                ? "/team-leader/verify-email"
                : "/admin/verify-email";
            string verificationUrl = $"{_frontendConfig.BaseUrl}{verifyPath}?token={Uri.EscapeDataString(token)}";
            await _emailService.SendAdminInvitationAsync(
                createdUser.Email,
                person.FirstName,
                verificationUrl,
                cancellationToken);

            _logger.LogInformation("Admin invitation sent to {Email} with verification URL", createdUser.Email);

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

    /// <summary>
    /// Creates (or reactivates) the team manager link rows that grant the invited team leader
    /// access to the requested teams.
    /// </summary>
    private async Task CreateTeamManagerLinksAsync(
        Guid personId,
        IReadOnlyList<TeamAssignmentDto> assignments,
        CancellationToken cancellationToken)
    {
        bool floorballTouched = false;
        bool footballTouched = false;

        foreach (TeamAssignmentDto assignment in assignments)
        {
            if (string.Equals(assignment.Sport, "floorball", StringComparison.OrdinalIgnoreCase))
            {
                FloorballTeamManager? existing = await _floorballTeamManagerRepository.GetByPersonAndTeamAsync(personId, assignment.TeamId);
                if (existing == null)
                {
                    await _floorballTeamManagerRepository.AddAsync(new FloorballTeamManager(personId, assignment.TeamId));
                    floorballTouched = true;
                }
                else if (!existing.IsActive)
                {
                    existing.UpdateActiveStatus(true);
                    await _floorballTeamManagerRepository.UpdateAsync(existing);
                    floorballTouched = true;
                }
            }
            else if (string.Equals(assignment.Sport, "football", StringComparison.OrdinalIgnoreCase))
            {
                FootballTeamManager? existing = await _footballTeamManagerRepository.GetByPersonAndTeamAsync(personId, assignment.TeamId);
                if (existing == null)
                {
                    await _footballTeamManagerRepository.AddAsync(new FootballTeamManager(personId, assignment.TeamId));
                    footballTouched = true;
                }
                else if (!existing.IsActive)
                {
                    existing.UpdateActiveStatus(true);
                    await _footballTeamManagerRepository.UpdateAsync(existing);
                    footballTouched = true;
                }
            }
            else
            {
                _logger.LogWarning("Unknown sport '{Sport}' in team assignment, skipping", assignment.Sport);
            }
        }

        if (floorballTouched)
        {
            await _floorballUnitOfWork.SaveChangesAsync(cancellationToken);
        }

        if (footballTouched)
        {
            await _footballUnitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    private static string GenerateVerificationToken()
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }
}
