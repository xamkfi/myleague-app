using Application.Common;
using Application.DTOs.Auth;
using MediatR;

namespace Application.Commands.Auth;

/// <summary>
/// Command to verify a login code and obtain authentication tokens
/// </summary>
public record VerifyLoginCodeCommand(string Email, string Code) : IRequest<Result<AuthTokenDto>>;
