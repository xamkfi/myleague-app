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
                htmlContent: BuildLoginCodeHtml(code),
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

    public async Task SendAdminInvitationAsync(string email, string firstName, string verificationUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            EmailSendOperation emailSendOperation = await _emailClient.SendAsync(
                WaitUntil.Completed,
                senderAddress: _config.SenderAddress,
                recipientAddress: email,
                subject: "You have been invited as a MyLeague admin",
                htmlContent: BuildAdminInvitationHtml(firstName, verificationUrl),
                plainTextContent: $"Hi {firstName},\n\nYou have been invited as an administrator on MyLeague.\n\nPlease verify your email address by visiting the following link:\n{verificationUrl}\n\nThis link expires in 48 hours.\n\nIf you did not expect this invitation, you can safely ignore this email.",
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Admin invitation email sent to {Email}. Operation ID: {OperationId}, Status: {Status}",
                email, emailSendOperation.Id, emailSendOperation.Value.Status);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Failed to send admin invitation email to {Email}. Error code: {ErrorCode}", email, ex.ErrorCode);
            throw;
        }
    }

    public async Task SendAdminVerificationSuccessAsync(string email, string firstName, string loginUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            EmailSendOperation emailSendOperation = await _emailClient.SendAsync(
                WaitUntil.Completed,
                senderAddress: _config.SenderAddress,
                recipientAddress: email,
                subject: "Welcome to MyLeague – Your account is ready",
                htmlContent: BuildAdminVerificationSuccessHtml(firstName, loginUrl),
                plainTextContent: $"Hi {firstName},\n\nYour email has been verified and your MyLeague admin account is now active.\n\nHow to log in:\n1. Go to {loginUrl}\n2. Enter your email address: {email}\n3. You will receive a 6-digit login code by email\n4. Enter the code to access the admin panel\n\nWelcome aboard!",
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Admin verification success email sent to {Email}. Operation ID: {OperationId}, Status: {Status}",
                email, emailSendOperation.Id, emailSendOperation.Value.Status);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Failed to send admin verification success email to {Email}. Error code: {ErrorCode}", email, ex.ErrorCode);
            throw;
        }
    }

    private static string BuildLoginCodeHtml(string code)
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

    private static string BuildAdminInvitationHtml(string firstName, string verificationUrl)
    {
        return $"""
            <html>
            <body style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f9f9f9;">
                <div style="background-color: #ffffff; border-radius: 8px; padding: 40px; box-shadow: 0 2px 8px rgba(0,0,0,0.08);">
                    <h2 style="color: #1a1a2e; margin-top: 0;">You have been invited to MyLeague</h2>
                    <p style="color: #444; font-size: 16px;">Hi {firstName},</p>
                    <p style="color: #444; font-size: 16px;">
                        You have been invited to become an administrator on <strong>MyLeague</strong>.
                        Please verify your email address to activate your account.
                    </p>
                    <div style="text-align: center; margin: 32px 0;">
                        <a href="{verificationUrl}"
                           style="background-color: #4f46e5; color: #ffffff; text-decoration: none;
                                  padding: 14px 28px; border-radius: 6px; font-size: 16px; font-weight: bold;
                                  display: inline-block;">
                            Verify my email
                        </a>
                    </div>
                    <p style="color: #888; font-size: 13px;">
                        This link expires in <strong>48 hours</strong>.
                        If the button does not work, copy and paste the following link into your browser:
                    </p>
                    <p style="word-break: break-all; color: #4f46e5; font-size: 13px;">{verificationUrl}</p>
                    <hr style="border: none; border-top: 1px solid #eee; margin: 24px 0;" />
                    <p style="color: #bbb; font-size: 12px;">
                        If you did not expect this invitation, you can safely ignore this email.
                    </p>
                </div>
            </body>
            </html>
            """;
    }

    private static string BuildAdminVerificationSuccessHtml(string firstName, string loginUrl)
    {
        return $"""
            <html>
            <body style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f9f9f9;">
                <div style="background-color: #ffffff; border-radius: 8px; padding: 40px; box-shadow: 0 2px 8px rgba(0,0,0,0.08);">
                    <h2 style="color: #1a1a2e; margin-top: 0;">Welcome to MyLeague!</h2>
                    <p style="color: #444; font-size: 16px;">Hi {firstName},</p>
                    <p style="color: #444; font-size: 16px;">
                        Your email has been verified and your <strong>MyLeague admin account is now active</strong>.
                    </p>
                    <h3 style="color: #1a1a2e; margin-top: 28px;">How to log in</h3>
                    <ol style="color: #444; font-size: 15px; line-height: 1.8;">
                        <li>Go to the admin login page using the button below.</li>
                        <li>Enter your email address.</li>
                        <li>You will receive a <strong>6-digit login code</strong> in a new email.</li>
                        <li>Enter the code on the login page to access the admin panel.</li>
                    </ol>
                    <div style="text-align: center; margin: 32px 0;">
                        <a href="{loginUrl}"
                           style="background-color: #4f46e5; color: #ffffff; text-decoration: none;
                                  padding: 14px 28px; border-radius: 6px; font-size: 16px; font-weight: bold;
                                  display: inline-block;">
                            Go to admin login
                        </a>
                    </div>
                    <p style="color: #888; font-size: 13px;">
                        Or copy and paste this URL into your browser:
                        <span style="word-break: break-all; color: #4f46e5;">{loginUrl}</span>
                    </p>
                    <hr style="border: none; border-top: 1px solid #eee; margin: 24px 0;" />
                    <p style="color: #bbb; font-size: 12px;">MyLeague – Sports league management platform.</p>
                </div>
            </body>
            </html>
            """;
    }
}
