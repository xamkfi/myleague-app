using Application.Common;
using MediatR;

namespace Application.Features.Auth.Commands;

/// <summary>
/// Command to request a login code be sent to the specified email.
/// Returns the generated code (for development auto-fill) or null if user not found.
/// </summary>
public record RequestLoginCodeCommand(string Email) : IRequest<Result<string?>>;
