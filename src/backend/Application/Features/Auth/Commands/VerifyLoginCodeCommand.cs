using Application.Common;
using Application.Features.Auth.DTOs;
using MediatR;

namespace Application.Features.Auth.Commands;

/// <summary>
/// Command to verify a login code and obtain authentication tokens
/// </summary>
public record VerifyLoginCodeCommand(string Email, string Code) : IRequest<Result<AuthTokenDto>>;
