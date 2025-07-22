using System;
using MediatR;
using Application.DTOs.Common;
using Application.Common;

namespace Application.Commands.Clubs;

/// <summary>
/// Command for updating a club's logo
/// </summary>
/// <param name="ClubId">The ID of the club to update</param>
/// <param name="LogoUrl">The new logo URL (optional)</param>
public record UpdateClubLogoCommand(
    Guid ClubId,
    string? LogoUrl) : IRequest<Result<ClubDto>>; 