using Application.Common;
using MediatR;

namespace Application.Commands.Auth;

/// <summary>
/// Command to revoke a refresh token (logout)
/// </summary>
public record RevokeTokenCommand(string RefreshToken) : IRequest<Result<bool>>;
