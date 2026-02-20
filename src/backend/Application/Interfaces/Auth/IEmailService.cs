namespace Application.Interfaces.Auth;

/// <summary>
/// Abstraction for sending emails.
/// In development, logs to console. In production, uses Azure Communication Services.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends a login code to the specified email address
    /// </summary>
    /// <param name="email">The recipient email address</param>
    /// <param name="code">The login code to send</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendLoginCodeAsync(string email, string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an admin invitation email with a verification link
    /// </summary>
    /// <param name="email">The recipient email address</param>
    /// <param name="firstName">The admin's first name for personalisation</param>
    /// <param name="verificationUrl">The full URL the admin must click to verify their email</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendAdminInvitationAsync(string email, string firstName, string verificationUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a welcome email after successful email verification with login instructions
    /// </summary>
    /// <param name="email">The recipient email address</param>
    /// <param name="firstName">The admin's first name for personalisation</param>
    /// <param name="loginUrl">The URL for the admin login page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendAdminVerificationSuccessAsync(string email, string firstName, string loginUrl, CancellationToken cancellationToken = default);
}
