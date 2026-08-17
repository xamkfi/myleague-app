using Domain.Entities.Hockey.Competitions;
using Domain.Entities.Hockey.Matches;
using Domain.Enums.Hockey.Competitions;

namespace Domain.Services.Hockey;

/// <summary>
/// Cross-aggregate validation for competition membership and match competition context.
/// </summary>
public class HockeyCompetitionValidationService
{
    public HockeyDomainValidationResult ValidateCompetitionTeam(
        HockeyCompetition competition,
        Guid competitionTeamId)
    {
        ArgumentNullException.ThrowIfNull(competition);
        List<string> errors = new();

        if (competitionTeamId == Guid.Empty)
        {
            errors.Add("Competition team id cannot be empty.");
            return HockeyDomainValidationResult.Fail(errors);
        }

        HockeyCompetitionTeam? team = competition.Teams.FirstOrDefault(t => t.Id == competitionTeamId);
        if (team is null)
            errors.Add("Competition team does not belong to this competition.");
        else if (!team.IsActive)
            errors.Add("Competition team is not active in this competition.");

        return errors.Count == 0
            ? HockeyDomainValidationResult.Ok()
            : HockeyDomainValidationResult.Fail(errors);
    }

    public HockeyDomainValidationResult ValidateDivisionContext(
        HockeyCompetition competition,
        Guid competitionDivisionId,
        Guid? competitionTeamId = null)
    {
        ArgumentNullException.ThrowIfNull(competition);
        List<string> errors = new();

        if (competitionDivisionId == Guid.Empty)
        {
            errors.Add("Competition division id cannot be empty.");
            return HockeyDomainValidationResult.Fail(errors);
        }

        HockeyCompetitionDivision? division = competition.Divisions
            .FirstOrDefault(d => d.Id == competitionDivisionId);
        if (division is null)
            errors.Add("Division does not belong to this competition.");
        else if (!division.IsActive)
            errors.Add("Division is not active.");
        else if (competitionTeamId is Guid teamId)
        {
            HockeyDomainValidationResult teamResult = ValidateCompetitionTeam(competition, teamId);
            if (!teamResult.IsValid)
                errors.AddRange(teamResult.Errors);
            else if (!division.Teams.Any(t => t.CompetitionTeamId == teamId && t.IsActive))
                errors.Add("Competition team is not assigned to this division.");
        }

        return errors.Count == 0
            ? HockeyDomainValidationResult.Ok()
            : HockeyDomainValidationResult.Fail(errors);
    }

    public HockeyDomainValidationResult ValidateTournamentGroupContext(
        HockeyTournament tournament,
        Guid tournamentGroupId,
        Guid? competitionTeamId = null)
    {
        ArgumentNullException.ThrowIfNull(tournament);
        List<string> errors = new();

        if (tournamentGroupId == Guid.Empty)
        {
            errors.Add("Tournament group id cannot be empty.");
            return HockeyDomainValidationResult.Fail(errors);
        }

        HockeyTournamentGroup? group = tournament.Groups.FirstOrDefault(g => g.Id == tournamentGroupId);
        if (group is null)
            errors.Add("Tournament group does not belong to this tournament.");
        else if (competitionTeamId is Guid teamId)
        {
            HockeyDomainValidationResult teamResult = ValidateCompetitionTeam(tournament, teamId);
            if (!teamResult.IsValid)
                errors.AddRange(teamResult.Errors);
            else if (!group.Teams.Any(t => t.CompetitionTeamId == teamId && t.IsActive))
                errors.Add("Competition team is not assigned to this tournament group.");
        }

        return errors.Count == 0
            ? HockeyDomainValidationResult.Ok()
            : HockeyDomainValidationResult.Fail(errors);
    }

    public HockeyDomainValidationResult ValidatePlayoffSeriesContext(
        HockeyCompetition competition,
        Guid playoffSeriesId,
        Guid? homeCompetitionTeamId = null,
        Guid? awayCompetitionTeamId = null)
    {
        ArgumentNullException.ThrowIfNull(competition);
        List<string> errors = new();

        if (playoffSeriesId == Guid.Empty)
        {
            errors.Add("Playoff series id cannot be empty.");
            return HockeyDomainValidationResult.Fail(errors);
        }

        HockeyPlayoffSeries? series = competition.PlayoffSeries.FirstOrDefault(s => s.Id == playoffSeriesId);
        if (series is null)
        {
            errors.Add("Playoff series does not belong to this competition.");
            return HockeyDomainValidationResult.Fail(errors);
        }

        if (homeCompetitionTeamId is Guid homeId)
        {
            HockeyDomainValidationResult homeResult = ValidateCompetitionTeam(competition, homeId);
            if (!homeResult.IsValid)
                errors.AddRange(homeResult.Errors);
            else if (series.HomeCompetitionTeamId is Guid assignedHome && assignedHome != homeId)
                errors.Add("Home competition team does not match the playoff series home team.");
        }

        if (awayCompetitionTeamId is Guid awayId)
        {
            HockeyDomainValidationResult awayResult = ValidateCompetitionTeam(competition, awayId);
            if (!awayResult.IsValid)
                errors.AddRange(awayResult.Errors);
            else if (series.AwayCompetitionTeamId is Guid assignedAway && assignedAway != awayId)
                errors.Add("Away competition team does not match the playoff series away team.");
        }

        return errors.Count == 0
            ? HockeyDomainValidationResult.Ok()
            : HockeyDomainValidationResult.Fail(errors);
    }

    public HockeyDomainValidationResult ValidateMatchCompetitionContext(
        HockeyCompetition competition,
        HockeyMatch match)
    {
        ArgumentNullException.ThrowIfNull(competition);
        ArgumentNullException.ThrowIfNull(match);
        List<string> errors = new();

        if (match.CompetitionId is null)
        {
            errors.Add("Match is not linked to a competition.");
            return HockeyDomainValidationResult.Fail(errors);
        }

        if (match.CompetitionId != competition.Id)
            errors.Add("Match competition id does not match the provided competition.");

        if (match.CompetitionDivisionId is Guid divisionId)
        {
            HockeyDomainValidationResult divisionResult = ValidateDivisionContext(competition, divisionId);
            if (!divisionResult.IsValid)
                errors.AddRange(divisionResult.Errors);
        }

        if (match.TournamentGroupId is Guid groupId)
        {
            if (competition.CompetitionType != HockeyCompetitionType.Tournament
                || competition is not HockeyTournament tournament)
            {
                errors.Add("Tournament group context requires a tournament competition.");
            }
            else
            {
                HockeyDomainValidationResult groupResult = ValidateTournamentGroupContext(tournament, groupId);
                if (!groupResult.IsValid)
                    errors.AddRange(groupResult.Errors);
            }
        }

        if (match.PlayoffSeriesId is Guid seriesId)
        {
            HockeyDomainValidationResult seriesResult = ValidatePlayoffSeriesContext(competition, seriesId);
            if (!seriesResult.IsValid)
                errors.AddRange(seriesResult.Errors);
        }

        foreach (HockeyMatchTeam matchTeam in match.MatchTeams)
        {
            if (matchTeam.CompetitionTeamId is Guid competitionTeamId)
            {
                HockeyDomainValidationResult teamResult = ValidateCompetitionTeam(competition, competitionTeamId);
                if (!teamResult.IsValid)
                    errors.AddRange(teamResult.Errors);
            }
        }

        return errors.Count == 0
            ? HockeyDomainValidationResult.Ok()
            : HockeyDomainValidationResult.Fail(errors);
    }
}
