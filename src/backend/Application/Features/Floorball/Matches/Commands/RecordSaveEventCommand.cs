using MediatR;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Floorball.Players.DTOs;
using Application.Features.Floorball.Referees.DTOs;
using Application.Features.Floorball.TeamManagers.DTOs;
using Application.Features.Floorball.Statistics.DTOs;
using System;
using Application.Common;

namespace Application.Features.Floorball.Matches.Commands
{
    /// <summary>
    /// Command to record a save event in an event-sourced floorball match
    /// </summary>
    public class RecordSaveEventCommand : IRequest<Result<FloorballSaveEventDto>>
    {
        public Guid MatchId { get; }
        public Guid TeamId { get; }
        public Guid GoalieId { get; }
        public int PeriodNumber { get; }
        public int TimeInSeconds { get; }
        public bool WasInOvertime { get; }
        public bool WasInShootout { get; }
        // Optionally, shooter info, etc.

        public RecordSaveEventCommand(Guid matchId, Guid teamId, Guid goalieId, int periodNumber, int timeInSeconds, bool wasInOvertime, bool wasInShootout)
        {
            MatchId = matchId;
            TeamId = teamId;
            GoalieId = goalieId;
            PeriodNumber = periodNumber;
            TimeInSeconds = timeInSeconds;
            WasInOvertime = wasInOvertime;
            WasInShootout = wasInShootout;
        }
    }
}
