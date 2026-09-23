using System;
using MediatR;
using Application.Features.Common.Organization.Users.DTOs;
using Application.Features.Common.Organization.Persons.DTOs;
using Application.Features.Common.Organization.Clubs.DTOs;
using Application.Features.Common.Organization.Divisions.DTOs;
using Application.Features.Common.Content.News.DTOs;
using Application.Features.Common.CrossCutting.Search.DTOs;
using Application.Features.Common.CrossCutting.MatchTimer.DTOs;
using Application.Features.Common.Shared.DTOs;
using Application.Common;

namespace Application.Features.Common.Organization.Clubs.Commands;

/// <summary>
/// Command for updating a club's logo
/// </summary>
/// <param name="ClubId">The ID of the club to update</param>
/// <param name="LogoUrl">The new logo URL (optional)</param>
public record UpdateClubLogoCommand(
    Guid ClubId,
    string? LogoUrl) : IRequest<Result<ClubDto>>; 
