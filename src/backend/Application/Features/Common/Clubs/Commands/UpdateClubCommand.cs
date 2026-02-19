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
/// Command for updating an existing club
/// </summary>
public record UpdateClubCommand(
    Guid ClubId,
    string Name,
    string? City,
    string? Country,
    DateTime? FoundingDate,
    string? WebsiteUrl = "",
    string? LogoUrl = "",
    string? ContactEmail = "") : IRequest<Result<ClubDto>>; 
