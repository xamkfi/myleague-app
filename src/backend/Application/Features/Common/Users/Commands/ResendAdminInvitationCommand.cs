using Application.Common;
using MediatR;

namespace Application.Features.Common.Users.Commands;

/// <summary>
/// Command for resending the admin invitation email to a user who has not yet verified their email.
/// Generates a new verification token and sends a fresh invitation email.
/// </summary>
public record ResendAdminInvitationCommand(Guid UserId) : IRequest<Result<bool>>;
