using System.Security.Cryptography;
using Application.Commands.Auth;
using Application.Common;
using Application.Configuration;
using Application.Interfaces.Auth;
using Domain.Repositories.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Handlers.Auth;

/// <summary>
/// Handler for requesting a login code. Generates a cryptographically random code,
/// stores it on the user, and sends it via email.
/// </summary>
public class RequestLoginCodeHandler : IRequestHandler<RequestLoginCodeCommand, Result<string?>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly LoginCodeConfiguration _loginCodeConfig;
    private readonly ILogger<RequestLoginCodeHandler> _logger;

    public RequestLoginCodeHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IOptions<LoginCodeConfiguration> loginCodeConfig,
        ILogger<RequestLoginCodeHandler> logger)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _loginCodeConfig = loginCodeConfig.Value;
        _logger = logger;
    }

    public async Task<Result<string?>> Handle(RequestLoginCodeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            Domain.Entities.Common.User? user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                // Don't reveal whether the email exists -- return success regardless
                _logger.LogInformation("Login code requested for non-existent email: {Email}", request.Email);
                return Result<string?>.Success(null);
            }

            if (!user.IsActive)
            {
                _logger.LogInformation("Login code requested for deactivated account: {Email}", request.Email);
                return Result<string?>.Success(null);
            }

            // Generate cryptographically secure code
            string code = GenerateSecureCode(_loginCodeConfig.CodeLength);
            DateTime expiresAt = DateTime.UtcNow.AddMinutes(_loginCodeConfig.ExpirationMinutes);

            user.SetLoginCode(code, expiresAt);
            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Send the code via email
            await _emailService.SendLoginCodeAsync(request.Email, code, cancellationToken);

            _logger.LogInformation("Login code sent to {Email}, expires at {ExpiresAt}", request.Email, expiresAt);
            return Result<string?>.Success(code);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending login code to {Email}", request.Email);
            return Result<string?>.Failure("An error occurred while sending the login code.");
        }
    }

    /// <summary>
    /// Generates a cryptographically secure numeric code of the specified length
    /// </summary>
    private static string GenerateSecureCode(int length)
    {
        int maxValue = (int)Math.Pow(10, length);
        int code = RandomNumberGenerator.GetInt32(0, maxValue);
        return code.ToString().PadLeft(length, '0');
    }
}
