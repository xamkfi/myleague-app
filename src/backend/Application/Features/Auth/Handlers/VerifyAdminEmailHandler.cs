using Application.Common;
using Application.Configuration;
using Application.Features.Auth.Commands;
using Application.Interfaces.Auth;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Features.Auth.Handlers;

/// <summary>
/// Handler for verifying a new admin's email address.
/// Validates the token, activates the user account, and sends a welcome email.
/// </summary>
public class VerifyAdminEmailHandler : IRequestHandler<VerifyAdminEmailCommand, Result<bool>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly FrontendConfiguration _frontendConfig;
    private readonly ILogger<VerifyAdminEmailHandler> _logger;

    public VerifyAdminEmailHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IOptions<FrontendConfiguration> frontendConfig,
        ILogger<VerifyAdminEmailHandler> logger)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _frontendConfig = frontendConfig.Value;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(VerifyAdminEmailCommand request, CancellationToken cancellationToken)
    {
        try
        {
            User? user = await _userRepository.GetByEmailVerificationTokenAsync(request.Token);

            if (user == null)
            {
                _logger.LogInformation("Email verification attempted with unknown token");
                return Result<bool>.Failure("This verification link is invalid or has already been used.");
            }

            if (user.EmailVerificationTokenExpiresAt < DateTime.UtcNow)
            {
                _logger.LogInformation("Email verification attempted with expired token for user: {UserId}", user.Id);
                return Result<bool>.Failure("This verification link has expired. Please ask an administrator to resend the invitation.");
            }

            string firstName = user.Person?.FirstName ?? "Admin";

            user.VerifyEmail();
            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Email verified for user: {UserId} ({Email})", user.Id, user.Email);

            string loginUrl = $"{_frontendConfig.BaseUrl}/admin/login";
            await _emailService.SendAdminVerificationSuccessAsync(
                user.Email,
                firstName,
                loginUrl,
                cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while verifying admin email with token");
            return Result<bool>.Failure("An error occurred while verifying the email address.");
        }
    }
}
