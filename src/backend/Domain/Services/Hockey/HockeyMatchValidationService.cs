using Domain.Entities.Hockey.Competitions;
using Domain.Entities.Hockey.Matches;
using Domain.Entities.Hockey.Matches.Events;
using Domain.Enums.Hockey.Matches;

namespace Domain.Services.Hockey;

/// <summary>
/// Cross-aggregate validation for match structure, active players and event references.
/// </summary>
public class HockeyMatchValidationService
{
    private readonly HockeyCompetitionValidationService _competitionValidation = new();

    public HockeyDomainValidationResult ValidateHomeAway(HockeyMatch match)
    {
        ArgumentNullException.ThrowIfNull(match);
        List<string> errors = new();

        HockeyMatchTeam? home = match.HomeMatchTeam;
        HockeyMatchTeam? away = match.AwayMatchTeam;

        if (home is null)
            errors.Add("Home team is not assigned.");
        if (away is null)
            errors.Add("Away team is not assigned.");
        if (home is not null && away is not null && home.TeamId == away.TeamId)
            errors.Add("Home and away teams must be different.");

        int homeCount = match.MatchTeams.Count(t => t.TeamSlot == HockeyTeamSlot.Home);
        int awayCount = match.MatchTeams.Count(t => t.TeamSlot == HockeyTeamSlot.Away);
        if (homeCount > 1)
            errors.Add("More than one home team is assigned.");
        if (awayCount > 1)
            errors.Add("More than one away team is assigned.");

        return errors.Count == 0
            ? HockeyDomainValidationResult.Ok()
            : HockeyDomainValidationResult.Fail(errors);
    }

    public HockeyDomainValidationResult ValidateMatchContext(
        HockeyMatch match,
        HockeyCompetition? competition = null)
    {
        ArgumentNullException.ThrowIfNull(match);
        List<string> errors = new();

        HockeyDomainValidationResult homeAway = ValidateHomeAway(match);
        if (!homeAway.IsValid)
            errors.AddRange(homeAway.Errors);

        if (match.CompetitionId is null)
        {
            if (match.CompetitionDivisionId is not null
                || match.TournamentGroupId is not null
                || match.PlayoffSeriesId is not null)
            {
                errors.Add("Standalone matches cannot reference division, tournament group or playoff series.");
            }

            if (competition is not null)
                errors.Add("Competition was provided but the match has no competition id.");
        }
        else if (competition is not null)
        {
            HockeyDomainValidationResult context = _competitionValidation
                .ValidateMatchCompetitionContext(competition, match);
            if (!context.IsValid)
                errors.AddRange(context.Errors);
        }

        return errors.Count == 0
            ? HockeyDomainValidationResult.Ok()
            : HockeyDomainValidationResult.Fail(errors);
    }

    public HockeyDomainValidationResult ValidateActivePlayers(HockeyMatch match)
    {
        ArgumentNullException.ThrowIfNull(match);
        List<string> errors = new();

        foreach (HockeyMatchTeam matchTeam in match.MatchTeams)
        {
            HockeyDomainValidationResult side = ValidateActivePlayers(matchTeam);
            if (!side.IsValid)
                errors.AddRange(side.Errors);
        }

        return errors.Count == 0
            ? HockeyDomainValidationResult.Ok()
            : HockeyDomainValidationResult.Fail(errors);
    }

    public HockeyDomainValidationResult ValidateActivePlayers(HockeyMatchTeam matchTeam)
    {
        ArgumentNullException.ThrowIfNull(matchTeam);
        List<string> errors = new();

        HockeyMatchPlayerSelection? selection = matchTeam.PlayerSelection;
        if (selection is null)
            return HockeyDomainValidationResult.Ok();

        List<HockeyMatchActivePlayer> active = selection.ActivePlayers.Where(p => p.IsActive).ToList();
        IEnumerable<IGrouping<int, HockeyMatchActivePlayer>> duplicateJerseys = active
            .GroupBy(p => p.JerseyNumber)
            .Where(g => g.Count() > 1);
        foreach (IGrouping<int, HockeyMatchActivePlayer> group in duplicateJerseys)
            errors.Add($"Match team {matchTeam.Id} has duplicate jersey number {group.Key}.");

        foreach (HockeyMatchActivePlayer player in active)
        {
            if (player.MatchPlayerSelectionId != selection.Id)
                errors.Add($"Active player {player.Id} does not belong to the match team selection.");
        }

        return errors.Count == 0
            ? HockeyDomainValidationResult.Ok()
            : HockeyDomainValidationResult.Fail(errors);
    }

    public HockeyDomainValidationResult ValidateEventPlayerReferences(HockeyMatch match)
    {
        ArgumentNullException.ThrowIfNull(match);
        List<string> errors = new();

        HashSet<Guid> matchTeamIds = match.MatchTeams.Select(t => t.Id).ToHashSet();
        HashSet<Guid> activePlayerIds = match.MatchTeams
            .SelectMany(t => t.PlayerSelection?.ActivePlayers ?? Enumerable.Empty<HockeyMatchActivePlayer>())
            .Where(p => p.IsActive)
            .Select(p => p.Id)
            .ToHashSet();

        foreach (HockeyMatchEvent matchEvent in match.Events)
        {
            if (matchEvent.MatchId != match.Id)
                errors.Add($"Event {matchEvent.Id} does not belong to this match.");

            if (matchEvent.MatchTeamId is Guid eventTeamId && !matchTeamIds.Contains(eventTeamId))
                errors.Add($"Event {matchEvent.Id} references an unknown match team.");

            if (matchEvent.MatchActivePlayerId is Guid basePlayerId && !activePlayerIds.Contains(basePlayerId))
                errors.Add($"Event {matchEvent.Id} references an unknown active player.");

            foreach (Guid playerId in CollectPlayerIds(matchEvent))
            {
                if (!activePlayerIds.Contains(playerId))
                    errors.Add($"Event {matchEvent.Id} references active player {playerId} who is not dressed.");
            }

            foreach (Guid teamId in CollectTeamIds(matchEvent))
            {
                if (!matchTeamIds.Contains(teamId))
                    errors.Add($"Event {matchEvent.Id} references match team {teamId} that is not in this match.");
            }
        }

        return errors.Count == 0
            ? HockeyDomainValidationResult.Ok()
            : HockeyDomainValidationResult.Fail(errors);
    }

    private static IEnumerable<Guid> CollectPlayerIds(HockeyMatchEvent matchEvent)
    {
        switch (matchEvent)
        {
            case HockeyGoal goal:
                yield return goal.ScorerActivePlayerId;
                if (goal.PrimaryAssistActivePlayerId is Guid a1) yield return a1;
                if (goal.SecondaryAssistActivePlayerId is Guid a2) yield return a2;
                if (goal.GoalieActivePlayerId is Guid g) yield return g;
                break;
            case HockeyPenalty penalty:
                if (penalty.PenalizedActivePlayerId is Guid p) yield return p;
                if (penalty.ServedByActivePlayerId is Guid s) yield return s;
                break;
            case HockeyShot shot:
                if (shot.ShooterActivePlayerId is Guid sh) yield return sh;
                if (shot.GoalieActivePlayerId is Guid sg) yield return sg;
                break;
            case HockeyFaceoff faceoff:
                if (faceoff.WinningActivePlayerId is Guid fw) yield return fw;
                if (faceoff.LosingActivePlayerId is Guid fl) yield return fl;
                break;
            case HockeyStoppage stoppage:
                if (stoppage.ResponsibleActivePlayerId is Guid r) yield return r;
                break;
            case HockeyGoalieChange change:
                if (change.OutgoingGoalieActivePlayerId is Guid go) yield return go;
                if (change.IncomingGoalieActivePlayerId is Guid gi) yield return gi;
                break;
            case HockeyShootoutAttempt attempt:
                yield return attempt.ShooterActivePlayerId;
                yield return attempt.GoalieActivePlayerId;
                break;
        }
    }

    private static IEnumerable<Guid> CollectTeamIds(HockeyMatchEvent matchEvent)
    {
        switch (matchEvent)
        {
            case HockeyGoal goal:
                yield return goal.ScoringMatchTeamId;
                break;
            case HockeyPenalty penalty:
                yield return penalty.PenaltyMatchTeamId;
                break;
            case HockeyShot shot:
                yield return shot.ShootingMatchTeamId;
                break;
            case HockeyFaceoff faceoff:
                yield return faceoff.WinningMatchTeamId;
                yield return faceoff.LosingMatchTeamId;
                break;
            case HockeyStoppage stoppage:
                if (stoppage.ResponsibleMatchTeamId is Guid st) yield return st;
                break;
            case HockeyVideoReview review:
                if (review.RequestedByMatchTeamId is Guid rt) yield return rt;
                break;
            case HockeyShootoutAttempt attempt when attempt.MatchTeamId is Guid soTeam:
                yield return soTeam;
                break;
        }
    }
}
