using System;
using MediatR;
using Application.Common;

namespace Application.Commands.Clubs;

/// <summary>
/// Command for deleting a club
/// </summary>
public record DeleteClubCommand(Guid ClubId) : IRequest<Result>; 