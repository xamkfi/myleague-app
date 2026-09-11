using Application.Interfaces.Auth;
using Microsoft.Extensions.Logging;

namespace MyLeague.Infrastructure.Services.Auth;

/// <summary>
/// Development email service that logs login codes to the console instead of sending actual emails.
/// </summary>
public class ConsoleLoginCodeEmailService : IEmailService
{
    private readonly ILogger<ConsoleLoginCodeEmailService> _logger;

    public ConsoleLoginCodeEmailService(ILogger<ConsoleLoginCodeEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendLoginCodeAsync(string email, string code, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "========================================\n" +
            "  LOGIN CODE for {Email}\n" +
            "  Code: {Code}\n" +
            "========================================",
            email, code);

        return Task.CompletedTask;
    }

    public Task SendAdminInvitationAsync(string email, string firstName, string verificationUrl, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "========================================\n" +
            "  ADMIN INVITATION for {Email}\n" +
            "  Name: {FirstName}\n" +
            "  Verification URL: {VerificationUrl}\n" +
            "========================================",
            email, firstName, verificationUrl);

        return Task.CompletedTask;
    }

    public Task SendAdminVerificationSuccessAsync(string email, string firstName, string loginUrl, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "========================================\n" +
            "  ADMIN VERIFIED for {Email}\n" +
            "  Name: {FirstName}\n" +
            "  Login URL: {LoginUrl}\n" +
            "========================================",
            email, firstName, loginUrl);

        return Task.CompletedTask;
    }
}
