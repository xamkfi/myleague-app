using Application.Common;
using MediatR;

namespace Application.Features.Auth.Commands;

/// <summary>
/// Command to revoke a refresh token (logout)
/// </summary>
public record RevokeTokenCommand(string RefreshToken) : IRequest<Result<bool>>;
