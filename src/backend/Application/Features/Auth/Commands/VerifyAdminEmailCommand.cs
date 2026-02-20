using Application.Common;
using MediatR;

namespace Application.Features.Auth.Commands;

/// <summary>
/// Command for verifying a new admin's email address using the token from the invitation email.
/// On success the user account is activated and a welcome email is sent.
/// </summary>
public record VerifyAdminEmailCommand(string Token) : IRequest<Result<bool>>;
