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

namespace Application.Features.Common.Organization.Divisions.Queries;

/// <summary>
/// Query for retrieving a division by ID
/// </summary>
public record GetDivisionByIdQuery(Guid Id) : IRequest<Result<DivisionDto>>; 
