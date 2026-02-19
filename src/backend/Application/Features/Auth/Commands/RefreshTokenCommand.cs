using Application.Common;
using Application.DTOs.Auth;
using MediatR;

namespace Application.Commands.Auth;

/// <summary>
/// Command to refresh authentication tokens using a valid refresh token
/// </summary>
public record RefreshTokenCommand(string RefreshToken) : IRequest<Result<AuthTokenDto>>;
