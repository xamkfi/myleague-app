using Domain.Entities.Floorball;
using Domain.Entities.Football.Teams;
using Domain.Entities.Hockey.Teams;
using Domain.Enums.Common;

namespace Application.Features.Common.Shared;

/// <summary>
/// Shared enroll-time roster copy for all three sports.
/// </summary>
public static class RosterEnrollment
{
    public static int Apply(FloorballTeam team, Guid competitionId, RosterEnrollmentMode mode)
    {
        if (mode != RosterEnrollmentMode.CopyLatest)
            return 0;

        Guid? source = team.FindLatestRosterCompetitionId(competitionId);
        return team.CopyRosterToCompetition(source, competitionId);
    }

    public static int Apply(FootballTeam team, Guid competitionId, RosterEnrollmentMode mode)
    {
        if (mode != RosterEnrollmentMode.CopyLatest)
            return 0;

        Guid? source = team.FindLatestRosterCompetitionId(competitionId);
        return team.CopyRosterToCompetition(source, competitionId);
    }

    public static int Apply(HockeyTeam team, Guid competitionId, RosterEnrollmentMode mode)
    {
        if (mode != RosterEnrollmentMode.CopyLatest)
            return 0;

        Guid? source = team.FindLatestRosterCompetitionId(competitionId);
        return team.CopyRosterToCompetition(source, competitionId);
    }
}
