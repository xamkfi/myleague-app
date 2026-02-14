using Application.Common;
using MediatR;

namespace Application.Commands.Auth;

/// <summary>
/// Command to request a login code be sent to the specified email
/// </summary>
public record RequestLoginCodeCommand(string Email) : IRequest<Result<bool>>;
