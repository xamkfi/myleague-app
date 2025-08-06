using MediatR;
using Application.DTOs.Floorball;
using System;
using Application.Common;

namespace Application.Commands.Floorball.MatchEvent
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
