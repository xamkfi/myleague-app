using System;
using MediatR;
using Application.Common;

namespace Application.Features.Common.Clubs.Commands;

/// <summary>
/// Command for deleting a club
/// </summary>
public record DeleteClubCommand(Guid ClubId) : IRequest<Result>; 
