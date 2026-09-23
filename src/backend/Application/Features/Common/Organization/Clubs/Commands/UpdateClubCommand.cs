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
