using Application.Common;
using Application.Features.Auth.DTOs;
using MediatR;

namespace Application.Features.Auth.Commands;

/// <summary>
/// Command to refresh authentication tokens using a valid refresh token
/// </summary>
public record RefreshTokenCommand(string RefreshToken) : IRequest<Result<AuthTokenDto>>;
