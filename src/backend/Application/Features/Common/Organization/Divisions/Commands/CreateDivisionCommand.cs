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
using Domain.Enums.Common;

namespace Application.Features.Common.Organization.Divisions.Commands;

/// <summary>
/// Command for creating a new division
/// </summary>
public record CreateDivisionCommand(
    string Name,
    string Description,
    int Level,
    SportsCategory SportType) : IRequest<Result<DivisionDto>>;
