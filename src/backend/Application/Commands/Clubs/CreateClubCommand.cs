using System;
using MediatR;
using Application.DTOs.Common;
using Application.Common;

namespace Application.Commands.Clubs;

/// <summary>
/// Command for creating a new club
/// </summary>
public record CreateClubCommand(
    string Name,
    string City,
    string Country,
    DateTime FoundingDate,
    string WebsiteUrl = "",
    string LogoUrl = "",
    string ContactEmail = "") : IRequest<Result<ClubDto>>; 