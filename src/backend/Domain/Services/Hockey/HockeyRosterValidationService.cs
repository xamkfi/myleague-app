using Domain.Entities.Hockey.Matches;
using Domain.Entities.Hockey.Teams;
using Domain.Enums.Hockey.Teams;
using Domain.ValueObjects.Hockey.Rules;

namespace Domain.Services.Hockey;

/// <summary>
/// Validates team and match-day rosters against <see cref="HockeyRosterRules"/>.
/// </summary>
public static class HockeyRosterValidationService
{
    public static HockeyDomainValidationResult ValidateTeamRoster(
        HockeyTeam team,
        HockeyRosterRules rules,
        Guid? competitionId = null)
    {
        ArgumentNullException.ThrowIfNull(team);
        ArgumentNullException.ThrowIfNull(rules);

        List<HockeyTeamPlayer> players = team.Roster
            .Where(p => p.IsActive)
            .Where(p => competitionId is null || p.CompetitionId == competitionId || p.CompetitionId is null)
            .ToList();

        List<string> errors = new();

        if (!rules.AllowGuestPlayers
            && players.Any(p => p.RosterStatus is HockeyRosterStatus.Guest or HockeyRosterStatus.Loaned))
        {
            errors.Add("Guest or loaned players are not allowed by roster rules.");
        }

        int captains = players.Count(p => p.CaptainRole == HockeyCaptainRole.Captain);
        if (captains > rules.MaxCaptains)
            errors.Add($"Roster exceeds max captains ({rules.MaxCaptains}).");

        int alternates = players.Count(p => p.CaptainRole == HockeyCaptainRole.AlternateCaptain);
        if (alternates > rules.MaxAlternateCaptains)
            errors.Add($"Roster exceeds max alternate captains ({rules.MaxAlternateCaptains}).");

        if (!rules.CanGoalieBeCaptain
            && players.Any(p =>
                p.Position == HockeyPosition.Goalie
                && p.CaptainRole is HockeyCaptainRole.Captain or HockeyCaptainRole.AlternateCaptain))
        {
            errors.Add("Goalies cannot be captains under current roster rules.");
        }

        IEnumerable<IGrouping<int, HockeyTeamPlayer>> duplicateJerseys = players
            .Where(p => p.JerseyNumber is not null)
            .GroupBy(p => p.JerseyNumber!.Value)
            .Where(g => g.Count() > 1);
        foreach (IGrouping<int, HockeyTeamPlayer> group in duplicateJerseys)
            errors.Add($"Jersey number {group.Key} is assigned to more than one active player.");

        int goalies = players.Count(p => p.Position == HockeyPosition.Goalie);
        if (rules.RequiresGoalie && goalies < 1)
            errors.Add("Roster requires at least one goalie.");
        if (goalies > rules.MaxDressedGoalies)
            errors.Add($"Roster exceeds max goalies ({rules.MaxDressedGoalies}).");

        return errors.Count == 0
            ? HockeyDomainValidationResult.Ok()
            : HockeyDomainValidationResult.Fail(errors);
    }

    public static HockeyDomainValidationResult ValidateMatchSelection(
        HockeyMatchPlayerSelection selection,
        HockeyRosterRules rules)
    {
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(rules);

        List<HockeyMatchActivePlayer> dressed = selection.ActivePlayers.Where(p => p.IsActive).ToList();
        List<string> errors = new();

        if (dressed.Count < rules.MinDressedPlayers)
            errors.Add($"Match roster has fewer than the minimum dressed players ({rules.MinDressedPlayers}).");
        if (dressed.Count > rules.MaxDressedPlayers)
            errors.Add($"Match roster exceeds max dressed players ({rules.MaxDressedPlayers}).");

        int goalies = dressed.Count(p => p.IsGoalie || p.IsEmergencyGoalie || p.Position == HockeyPosition.Goalie);
        if (rules.RequiresGoalie && goalies < 1)
            errors.Add("Match roster requires at least one goalie.");
        if (goalies > rules.MaxDressedGoalies)
            errors.Add($"Match roster exceeds max dressed goalies ({rules.MaxDressedGoalies}).");

        int captains = dressed.Count(p => p.CaptainRole == HockeyCaptainRole.Captain);
        if (captains > rules.MaxCaptains)
            errors.Add($"Match roster exceeds max captains ({rules.MaxCaptains}).");

        int alternates = dressed.Count(p => p.CaptainRole == HockeyCaptainRole.AlternateCaptain);
        if (alternates > rules.MaxAlternateCaptains)
            errors.Add($"Match roster exceeds max alternate captains ({rules.MaxAlternateCaptains}).");

        if (!rules.CanGoalieBeCaptain
            && dressed.Any(p =>
                (p.IsGoalie || p.Position == HockeyPosition.Goalie)
                && p.CaptainRole is HockeyCaptainRole.Captain or HockeyCaptainRole.AlternateCaptain))
        {
            errors.Add("Goalies cannot be captains under current roster rules.");
        }

        IEnumerable<IGrouping<int, HockeyMatchActivePlayer>> duplicateJerseys = dressed
            .GroupBy(p => p.JerseyNumber)
            .Where(g => g.Count() > 1);
        foreach (IGrouping<int, HockeyMatchActivePlayer> group in duplicateJerseys)
            errors.Add($"Jersey number {group.Key} is used more than once in the match roster.");

        if (!rules.AllowGuestPlayers && selection.MatchTeam?.Team is HockeyTeam team)
        {
            foreach (HockeyMatchActivePlayer active in dressed)
            {
                HockeyTeamPlayer? teamPlayer = active.TeamPlayer
                    ?? team.Roster.FirstOrDefault(p => p.Id == active.TeamPlayerId);
                if (teamPlayer is not null
                    && teamPlayer.RosterStatus is HockeyRosterStatus.Guest or HockeyRosterStatus.Loaned)
                {
                    errors.Add("Guest or loaned players are not allowed by roster rules.");
                    break;
                }
            }
        }

        return errors.Count == 0
            ? HockeyDomainValidationResult.Ok()
            : HockeyDomainValidationResult.Fail(errors);
    }
}
