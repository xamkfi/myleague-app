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

namespace Application.Features.Common.Clubs.Queries;

/// <summary>
/// Query for retrieving a club by its ID
/// </summary>
public record GetClubByIdQuery(Guid ClubId) : IRequest<Result<ClubDto>>; 
