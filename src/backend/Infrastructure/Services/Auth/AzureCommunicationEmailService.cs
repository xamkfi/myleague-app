using Application.Configuration;
using Application.Interfaces.Auth;
using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MyLeague.Infrastructure.Services.Auth;

/// <summary>
/// Production email service using Azure Communication Services Email to send login codes.
/// </summary>
public class AzureCommunicationEmailService : IEmailService
{
    private readonly EmailClient _emailClient;
    private readonly AzureCommunicationServicesConfiguration _config;
    private readonly ILogger<AzureCommunicationEmailService> _logger;

    public AzureCommunicationEmailService(
        IOptions<AzureCommunicationServicesConfiguration> config,
        ILogger<AzureCommunicationEmailService> logger)
    {
        _config = config.Value;
        _logger = logger;
        _emailClient = new EmailClient(_config.ConnectionString);
    }

    public async Task SendLoginCodeAsync(string email, string code, CancellationToken cancellationToken = default)
    {
        try
        {
            EmailSendOperation emailSendOperation = await _emailClient.SendAsync(
                WaitUntil.Completed,
                senderAddress: _config.SenderAddress,
                recipientAddress: email,
                subject: "Your MyLeague Login Code",
                htmlContent: BuildHtmlContent(code),
                plainTextContent: $"Your login code is: {code}. This code expires in 10 minutes.",
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Login code email sent to {Email}. Operation ID: {OperationId}, Status: {Status}",
                email, emailSendOperation.Id, emailSendOperation.Value.Status);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Failed to send login code email to {Email}. Error code: {ErrorCode}", email, ex.ErrorCode);
            throw;
        }
    }

    private static string BuildHtmlContent(string code)
    {
        return $"""
            <html>
            <body style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;">
                <h2 style="color: #333;">MyLeague Login</h2>
                <p>Your login code is:</p>
                <div style="background-color: #f0f0f0; padding: 20px; text-align: center; border-radius: 8px; margin: 20px 0;">
                    <span style="font-size: 32px; font-weight: bold; letter-spacing: 8px; color: #333;">{code}</span>
                </div>
                <p style="color: #666; font-size: 14px;">This code expires in 10 minutes. If you did not request this code, you can safely ignore this email.</p>
            </body>
            </html>
            """;
    }
}
