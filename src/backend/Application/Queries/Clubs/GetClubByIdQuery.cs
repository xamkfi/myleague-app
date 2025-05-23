using System;
using MediatR;
using Application.DTOs.Common;

namespace Application.Queries.Clubs;

/// <summary>
/// Query for retrieving a club by its ID
/// </summary>
public record GetClubByIdQuery(Guid ClubId) : IRequest<ClubDto>; 