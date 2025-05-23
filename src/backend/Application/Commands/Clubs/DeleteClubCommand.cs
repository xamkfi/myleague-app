using System;
using MediatR;

namespace Application.Commands.Clubs;

/// <summary>
/// Command for deleting a club
/// </summary>
public record DeleteClubCommand(Guid ClubId) : IRequest; 