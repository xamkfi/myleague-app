using System;
using MediatR;
using Application.Features.Common.Users.DTOs;
using Application.Features.Common.Persons.DTOs;
using Application.Features.Common.Clubs.DTOs;
using Application.Features.Common.Divisions.DTOs;
using Application.Features.Common.News.DTOs;
using Application.Features.Common.Search.DTOs;
using Application.Features.Common.MatchTimer.DTOs;
using Application.Features.Common.Shared.DTOs;
using Application.Common;

namespace Application.Features.Common.Clubs.Commands;

/// <summary>
/// Command for updating a club's logo
/// </summary>
/// <param name="ClubId">The ID of the club to update</param>
/// <param name="LogoUrl">The new logo URL (optional)</param>
public record UpdateClubLogoCommand(
    Guid ClubId,
    string? LogoUrl) : IRequest<Result<ClubDto>>; 
