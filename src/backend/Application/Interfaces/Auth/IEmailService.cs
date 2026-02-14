namespace Application.Interfaces.Auth;

/// <summary>
/// Abstraction for sending login code emails.
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
}
