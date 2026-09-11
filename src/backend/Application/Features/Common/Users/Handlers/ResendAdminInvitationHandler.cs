using Application.Common;
using Application.Configuration;
using Application.Features.Common.Users.Commands;
using Application.Interfaces.Auth;
using Domain.Entities.Common;
using Domain.Enums.Common;
using Domain.Repositories.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace Application.Features.Common.Users.Handlers;

/// <summary>
/// Handler for resending an admin invitation email.
/// Generates a fresh verification token and sends a new invitation to the user.
/// </summary>
public class ResendAdminInvitationHandler : IRequestHandler<ResendAdminInvitationCommand, Result<bool>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly FrontendConfiguration _frontendConfig;
    private readonly ILogger<ResendAdminInvitationHandler> _logger;

    public ResendAdminInvitationHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IOptions<FrontendConfiguration> frontendConfig,
        ILogger<ResendAdminInvitationHandler> logger)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _frontendConfig = frontendConfig.Value;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(ResendAdminInvitationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            User? user = await _userRepository.GetByIdAsync(request.UserId);

            if (user == null)
            {
                return Result<bool>.Failure($"User with ID '{request.UserId}' was not found.");
            }

            if (user.IsEmailVerified)
            {
                return Result<bool>.Failure("This user has already verified their email address.");
            }

            string firstName = user.Person?.FirstName ?? "Admin";

            string token = GenerateVerificationToken();
            user.SetEmailVerificationToken(token, DateTime.UtcNow.AddHours(48));
            user.IsActive = false;

            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            string verifyPath = user.Role == UserRole.ClubAdmin
                ? "/club-admin/verify-email"
                : "/admin/verify-email";
            string verificationUrl = $"{_frontendConfig.BaseUrl}{verifyPath}?token={Uri.EscapeDataString(token)}";
            await _emailService.SendAdminInvitationAsync(
                user.Email,
                firstName,
                verificationUrl,
                cancellationToken);

            _logger.LogInformation("Admin invitation resent to {Email}", user.Email);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while resending invitation for user: {UserId}", request.UserId);
            return Result<bool>.Failure("An error occurred while resending the invitation.");
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
