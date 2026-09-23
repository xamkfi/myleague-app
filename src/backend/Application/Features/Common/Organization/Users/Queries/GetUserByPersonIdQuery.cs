using System;
using Application.Common;
using Application.Features.Common.Organization.Users.DTOs;
using Application.Features.Common.Organization.Persons.DTOs;
using Application.Features.Common.Organization.Clubs.DTOs;
using Application.Features.Common.Organization.Divisions.DTOs;
using Application.Features.Common.Content.News.DTOs;
using Application.Features.Common.CrossCutting.Search.DTOs;
using Application.Features.Common.CrossCutting.MatchTimer.DTOs;
using Application.Features.Common.Shared.DTOs;
using MediatR;

namespace Application.Features.Common.Organization.Users.Queries;

/// <summary>
/// Query for getting a user by person ID
/// </summary>
public record GetUserByPersonIdQuery(Guid PersonId) : IRequest<Result<UserDto>>; 
