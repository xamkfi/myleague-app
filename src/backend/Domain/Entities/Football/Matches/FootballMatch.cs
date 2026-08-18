using Domain.Entities.Football.Competitions;
using Domain.Entities.Football.Teams;
using Domain.Enums.Football;
using Domain.ValueObjects.Football;

namespace Domain.Entities.Football.Matches;

/// <summary>
/// Football match aggregate: score, events, halves, lineup, officials, and knockout metadata.
/// </summary>
public class FootballMatch : BaseEntity
{
    public FootballCompetition Competition { get; private set; }
    public Guid CompetitionId { get; private set; }
    public FootballTeam? HomeTeam { get; private set; }
    public Guid? HomeTeamId { get; private set; }
    public FootballTeam? AwayTeam { get; private set; }
    public Guid? AwayTeamId { get; private set; }
    public DateTime ScheduledDateTime { get; private set; }
    public string? Venue { get; private set; }
    public FootballMatchStatus Status { get; private set; }
    public int HomeScore { get; private set; }
    public int AwayScore { get; private set; }
    public bool WentToExtraTime { get; private set; }
    public bool WentToPenaltyShootout { get; private set; }
    public FootballMatchRules MatchRules { get; private set; }

    public IReadOnlyCollection<FootballMatchEvent> Events => _events.AsReadOnly();
    private readonly List<FootballMatchEvent> _events = new();

    public IReadOnlyCollection<FootballGoal> GoalEvents =>
        _events.OfType<FootballGoal>().ToList().AsReadOnly();

    public IReadOnlyCollection<FootballCard> CardEvents =>
        _events.OfType<FootballCard>().ToList().AsReadOnly();

    public IReadOnlyCollection<FootballSubstitution> SubstitutionEvents =>
        _events.OfType<FootballSubstitution>().ToList().AsReadOnly();

    public FootballTournamentStage? TournamentStage { get; private set; }
    public Guid? TournamentGroupId { get; private set; }
    public FootballPlayoffRound? PlayoffRound { get; private set; }
    public int? PlayoffMatchOrder { get; private set; }
    public Guid? NextMatchId { get; private set; }
    public FootballPlayoffSlot? NextMatchSlot { get; private set; }

    public IReadOnlyCollection<FootballReferee> Officials => _officials.AsReadOnly();
    private readonly List<FootballReferee> _officials = new();

    public IReadOnlyCollection<FootballPeriodScore> PeriodScores => _periodScores.AsReadOnly();
    private readonly List<FootballPeriodScore> _periodScores = new();

    public IReadOnlyCollection<FootballMatchLineupPlayer> Lineup => _lineup.AsReadOnly();
    private readonly List<FootballMatchLineupPlayer> _lineup = new();

    public IReadOnlyCollection<FootballMatchLineupPlayer> HomeOnFieldPlayers =>
        _lineup.Where(p => p.TeamId == HomeTeamId && p.IsOnField).ToList().AsReadOnly();

    public IReadOnlyCollection<FootballMatchLineupPlayer> AwayOnFieldPlayers =>
        _lineup.Where(p => p.TeamId == AwayTeamId && p.IsOnField).ToList().AsReadOnly();

    private FootballMatch()
    {
        Status = FootballMatchStatus.Scheduled;
        MatchRules = FootballMatchRules.Default();
        Competition = null!;
        Venue = string.Empty;
    }

    public FootballMatch(
        FootballCompetition competition,
        FootballTeam? homeTeam,
        FootballTeam? awayTeam,
        DateTime scheduledDateTime,
        string? venue)
        : this(Guid.NewGuid(), competition, homeTeam, awayTeam, scheduledDateTime, venue)
    {
    }

    public FootballMatch(
        Guid id,
        FootballCompetition competition,
        FootballTeam? homeTeam,
        FootballTeam? awayTeam,
        DateTime scheduledDateTime,
        string? venue)
        : this(id, competition, homeTeam, awayTeam, scheduledDateTime, venue, matchRulesOverride: null)
    {
    }

    private FootballMatch(
        Guid id,
        FootballCompetition competition,
        FootballTeam? homeTeam,
        FootballTeam? awayTeam,
        DateTime scheduledDateTime,
        string? venue,
        FootballMatchRules? matchRulesOverride)
    {
        ArgumentNullException.ThrowIfNull(competition);

        if (homeTeam != null && awayTeam != null && homeTeam == awayTeam)
            throw new ArgumentException("Home team and away team cannot be the same team.");

        Id = id;
        Competition = competition;
        CompetitionId = competition.Id;
        HomeTeam = homeTeam;
        HomeTeamId = homeTeam?.Id;
        AwayTeam = awayTeam;
        AwayTeamId = awayTeam?.Id;
        ScheduledDateTime = scheduledDateTime;
        Venue = venue;
        Status = FootballMatchStatus.Scheduled;
        MatchRules = matchRulesOverride ?? CloneRules(competition.MatchRules);

        Guid homeIdForPeriods = homeTeam?.Id ?? Guid.Empty;
        Guid awayIdForPeriods = awayTeam?.Id ?? Guid.Empty;
        for (int i = 1; i <= MatchRules.NumberOfHalves; i++)
        {
            _periodScores.Add(new FootballPeriodScore(Id, i, homeIdForPeriods, awayIdForPeriods));
        }
    }

    public static FootballMatch CreatePlayoffMatch(
        Guid id,
        FootballCompetition competition,
        FootballTeam? homeTeam,
        FootballTeam? awayTeam,
        DateTime scheduledDateTime,
        string? venue,
        FootballMatchRules playoffMatchRules)
    {
        ArgumentNullException.ThrowIfNull(playoffMatchRules);
        return new FootballMatch(id, competition, homeTeam, awayTeam, scheduledDateTime, venue, playoffMatchRules);
    }

    public void ChangeCompetition(FootballCompetition competition)
    {
        ArgumentNullException.ThrowIfNull(competition);
        Competition = competition;
        CompetitionId = competition.Id;
    }

    public void ChangeTeams(FootballTeam? homeTeam, FootballTeam? awayTeam)
    {
        if (homeTeam != null && awayTeam != null && homeTeam == awayTeam)
            throw new ArgumentException("Home team and away team cannot be the same team.");
        AssignTeam(FootballPlayoffSlot.Home, homeTeam);
        AssignTeam(FootballPlayoffSlot.Away, awayTeam);
    }

    public void ChangeVenue(string venue)
    {
        ArgumentNullException.ThrowIfNull(venue);
        Venue = venue;
    }

    public void Reschedule(DateTime newDateTime, string? newVenue = null)
    {
        if (Status != FootballMatchStatus.Scheduled && Status != FootballMatchStatus.Postponed)
            throw new InvalidOperationException($"Cannot reschedule a match with status {Status}.");

        ScheduledDateTime = newDateTime;
        if (!string.IsNullOrWhiteSpace(newVenue))
            Venue = newVenue;
        Status = FootballMatchStatus.Scheduled;
    }

    public void Postpone()
    {
        if (Status != FootballMatchStatus.Scheduled)
            throw new InvalidOperationException($"Cannot postpone a match with status {Status}.");
        Status = FootballMatchStatus.Postponed;
    }

    public void Start()
    {
        if (Status != FootballMatchStatus.Scheduled)
            throw new InvalidOperationException($"Cannot start a match with status {Status}.");

        if (HomeTeamId is null || AwayTeamId is null)
            throw new InvalidOperationException("Cannot start a match until both teams have been assigned.");

        if (MatchRules.RequireOfficialsToStart && _officials.Count == 0)
            throw new InvalidOperationException("Cannot start a match without officials.");

        EnsureLineupReady(HomeTeamId.Value, "home");
        EnsureLineupReady(AwayTeamId.Value, "away");

        Status = FootballMatchStatus.InProgress;
    }

    public FootballGoal RecordGoal(
        FootballTeam scoringTeam,
        FootballPlayer scoringPlayer,
        FootballPlayer? assistingPlayer,
        int periodNumber,
        int timeInSeconds,
        FootballGoalType? goalType = null,
        string? description = null)
    {
        EnsureInProgress("record a goal");
        ArgumentNullException.ThrowIfNull(scoringTeam);
        ArgumentNullException.ThrowIfNull(scoringPlayer);
        EnsureValidPeriod(periodNumber);

        if (timeInSeconds < 0)
            throw new ArgumentOutOfRangeException(nameof(timeInSeconds), "Time must be non-negative.");

        if (scoringTeam.Id != HomeTeamId && scoringTeam.Id != AwayTeamId)
            throw new ArgumentException("Scoring team is not participating in this match.", nameof(scoringTeam));

        bool isOwnGoal = goalType == FootballGoalType.OwnGoal;
        FootballTeam scorerTeam = isOwnGoal ? OpponentOf(scoringTeam) : scoringTeam;
        ValidatePlayerOnRoster(scorerTeam, scoringPlayer.Id, "Scoring player");

        if (assistingPlayer != null)
        {
            if (isOwnGoal)
                throw new ArgumentException("Own goals cannot have an assist.", nameof(assistingPlayer));
            ValidatePlayerOnRoster(scoringTeam, assistingPlayer.Id, "Assisting player");
        }

        FootballGoalType? mappedType = goalType;
        if (mappedType is null && MatchRules.IsExtraTimePeriod(periodNumber))
            mappedType = FootballGoalType.ExtraTime;
        if (mappedType is null && MatchRules.IsPenaltyShootoutPeriod(periodNumber))
            mappedType = FootballGoalType.PenaltyShootout;

        FootballGoal goalEvent = new(
            Id,
            scoringTeam.Id,
            scoringPlayer.Id,
            assistingPlayer?.Id,
            periodNumber,
            timeInSeconds,
            mappedType,
            description);

        _events.Add(goalEvent);
        AdjustScore(scoringTeam.Id, periodNumber, increment: true);
        return goalEvent;
    }

    public FootballCard RecordCard(
        FootballTeam team,
        FootballPlayer player,
        FootballCardType cardType,
        int periodNumber,
        int timeInSeconds,
        string? description = null)
    {
        EnsureInProgress("record a card");
        ArgumentNullException.ThrowIfNull(team);
        ArgumentNullException.ThrowIfNull(player);
        EnsureValidPeriod(periodNumber);

        if (timeInSeconds < 0)
            throw new ArgumentOutOfRangeException(nameof(timeInSeconds), "Time must be non-negative.");
        if (team.Id != HomeTeamId && team.Id != AwayTeamId)
            throw new ArgumentException("Team is not participating in this match.", nameof(team));

        ValidatePlayerOnRoster(team, player.Id, "Carded player");

        FootballMatchLineupPlayer? lineupPlayer = _lineup.FirstOrDefault(p => p.TeamId == team.Id && p.PlayerId == player.Id);
        if (lineupPlayer is { IsSentOff: true })
            throw new InvalidOperationException("Cannot show a card to a player who has already been sent off.");

        FootballCardType effectiveType = cardType;
        if (cardType == FootballCardType.Yellow && HasUnconvertedYellow(team.Id, player.Id))
            effectiveType = FootballCardType.SecondYellow;

        FootballCard cardEvent = new(
            Id,
            team.Id,
            player.Id,
            effectiveType,
            periodNumber,
            timeInSeconds,
            description);
        _events.Add(cardEvent);

        if (cardEvent.ResultsInSendingOff && lineupPlayer != null)
            lineupPlayer.SendOff();

        return cardEvent;
    }

    public FootballSubstitution RecordSubstitution(
        FootballTeam team,
        FootballPlayer playerOff,
        FootballPlayer playerOn,
        int periodNumber,
        int timeInSeconds,
        string? description = null)
    {
        EnsureInProgress("record a substitution");
        ArgumentNullException.ThrowIfNull(team);
        ArgumentNullException.ThrowIfNull(playerOff);
        ArgumentNullException.ThrowIfNull(playerOn);
        EnsureValidPeriod(periodNumber);

        if (timeInSeconds < 0)
            throw new ArgumentOutOfRangeException(nameof(timeInSeconds), "Time must be non-negative.");
        if (team.Id != HomeTeamId && team.Id != AwayTeamId)
            throw new ArgumentException("Team is not participating in this match.", nameof(team));
        if (MatchRules.IsPenaltyShootoutPeriod(periodNumber))
            throw new InvalidOperationException("Substitutions are not allowed during a penalty shootout.");

        ValidatePlayerOnRoster(team, playerOff.Id, "Player going off");
        ValidatePlayerOnRoster(team, playerOn.Id, "Player coming on");

        FootballMatchLineupPlayer offEntry = GetLineupEntry(team.Id, playerOff.Id, "Player going off");
        FootballMatchLineupPlayer onEntry = GetLineupEntry(team.Id, playerOn.Id, "Player coming on");

        if (offEntry.IsSentOff)
            throw new InvalidOperationException("A sent-off player cannot be substituted.");
        if (!offEntry.IsOnField)
            throw new InvalidOperationException("Player going off is not currently on the field.");
        if (onEntry.IsOnField)
            throw new InvalidOperationException("Player coming on is already on the field.");
        if (onEntry.IsSentOff)
            throw new InvalidOperationException("A sent-off player cannot come on.");

        if (!MatchRules.HasUnlimitedSubstitutions)
        {
            int used = _events.OfType<FootballSubstitution>().Count(s => s.TeamId == team.Id);
            if (used >= MatchRules.MaxSubstitutions)
                throw new InvalidOperationException($"Team has already used all {MatchRules.MaxSubstitutions} substitutions.");
        }

        offEntry.TakeOffField();
        onEntry.PutOnField();

        FootballSubstitution substitution = new(
            Id,
            team.Id,
            playerOff.Id,
            playerOn.Id,
            periodNumber,
            timeInSeconds,
            description);
        _events.Add(substitution);
        return substitution;
    }

    public void AddOfficial(FootballReferee referee)
    {
        ArgumentNullException.ThrowIfNull(referee);
        if (Status == FootballMatchStatus.Completed || Status == FootballMatchStatus.Cancelled)
            throw new InvalidOperationException($"Cannot add officials to a match with status {Status}.");
        if (_officials.Contains(referee))
            return;
        _officials.Add(referee);
    }

    public void RemoveOfficial(Guid refereeId)
    {
        if (Status == FootballMatchStatus.Completed || Status == FootballMatchStatus.Cancelled)
            throw new InvalidOperationException($"Cannot remove officials from a match with status {Status}.");

        FootballReferee? existing = _officials.FirstOrDefault(o => o.Id == refereeId);
        if (existing == null)
            return;

        if (MatchRules.RequireOfficialsToStart && _officials.Count <= 1 && Status == FootballMatchStatus.InProgress)
            throw new InvalidOperationException("Cannot remove the last official from an in-progress match.");

        _officials.Remove(existing);
    }

    public void SetOfficials(IEnumerable<FootballReferee> officials)
    {
        ArgumentNullException.ThrowIfNull(officials);
        if (Status == FootballMatchStatus.Completed || Status == FootballMatchStatus.Cancelled)
            throw new InvalidOperationException($"Cannot update officials when match status is {Status}.");

        List<FootballReferee> refs = officials.Distinct().ToList();
        if (MatchRules.RequireOfficialsToStart && refs.Count == 0)
            throw new InvalidOperationException("Match must have at least one official.");

        _officials.Clear();
        _officials.AddRange(refs);
    }

    public void RecordExtraTime()
    {
        if (!MatchRules.AllowExtraTime)
            throw new InvalidOperationException("Extra time is not allowed by the match rules.");

        WentToExtraTime = true;
        Guid homeId = HomeTeamId ?? Guid.Empty;
        Guid awayId = AwayTeamId ?? Guid.Empty;
        for (int i = 0; i < MatchRules.ExtraTimeHalfCount; i++)
        {
            int period = MatchRules.ExtraTimeStartPeriodNumber + i;
            if (_periodScores.All(ps => ps.PeriodNumber != period))
                _periodScores.Add(new FootballPeriodScore(Id, period, homeId, awayId));
        }
    }

    public void RecordPenaltyShootout()
    {
        if (!MatchRules.AllowPenaltyShootout)
            throw new InvalidOperationException("Penalty shootout is not allowed by the match rules.");

        WentToPenaltyShootout = true;
        int shootoutPeriod = MatchRules.PenaltyShootoutPeriodNumber;
        if (_periodScores.All(ps => ps.PeriodNumber != shootoutPeriod))
        {
            _periodScores.Add(new FootballPeriodScore(
                Id,
                shootoutPeriod,
                HomeTeamId ?? Guid.Empty,
                AwayTeamId ?? Guid.Empty));
        }
    }

    public void Complete()
    {
        if (Status != FootballMatchStatus.InProgress)
            throw new InvalidOperationException($"Cannot complete a match with status {Status}.");

        if (PlayoffRound != null && HomeScore == AwayScore)
            throw new InvalidOperationException("Playoff matches cannot end in a draw. Record extra time or a penalty shootout result first.");

        Status = FootballMatchStatus.Completed;
        foreach (FootballReferee referee in _officials)
            referee.RecordMatchOfficiated();
    }

    public void Cancel()
    {
        if (Status == FootballMatchStatus.Completed)
            throw new InvalidOperationException("Cannot cancel a completed match.");
        Status = FootballMatchStatus.Cancelled;
    }

    public void Reactivate()
    {
        if (Status != FootballMatchStatus.Cancelled)
            throw new InvalidOperationException("Can only reactivate a cancelled match.");
        Status = FootballMatchStatus.Scheduled;
    }

    public void ReopenFromCompleted()
    {
        if (Status != FootballMatchStatus.Completed)
            throw new InvalidOperationException($"Can only reopen a completed match. Current status: {Status}.");
        Status = FootballMatchStatus.InProgress;
    }

    public FootballGoal DeleteGoalEvent(Guid goalEventId)
    {
        if (Status == FootballMatchStatus.Completed)
            throw new InvalidOperationException("Cannot delete goal events from a completed match.");

        FootballGoal? goalEvent = _events.OfType<FootballGoal>().FirstOrDefault(g => g.Id == goalEventId);
        if (goalEvent == null)
            throw new ArgumentException($"Goal event with ID {goalEventId} not found in this match.", nameof(goalEventId));

        _events.Remove(goalEvent);
        AdjustScore(goalEvent.TeamId, goalEvent.PeriodNumber, increment: false);
        return goalEvent;
    }

    public FootballCard DeleteCardEvent(Guid cardEventId)
    {
        if (Status == FootballMatchStatus.Completed)
            throw new InvalidOperationException("Cannot delete card events from a completed match.");

        FootballCard? cardEvent = _events.OfType<FootballCard>().FirstOrDefault(c => c.Id == cardEventId);
        if (cardEvent == null)
            throw new ArgumentException($"Card event with ID {cardEventId} not found in this match.", nameof(cardEventId));

        _events.Remove(cardEvent);

        if (cardEvent.ResultsInSendingOff)
        {
            FootballMatchLineupPlayer? lineupPlayer = _lineup.FirstOrDefault(
                p => p.TeamId == cardEvent.TeamId && p.PlayerId == cardEvent.PlayerId);
            if (lineupPlayer != null && !HasSendingOffRemaining(cardEvent.TeamId, cardEvent.PlayerId))
                lineupPlayer.ClearSendingOff();
        }

        return cardEvent;
    }

    public FootballSubstitution DeleteSubstitutionEvent(Guid substitutionEventId)
    {
        if (Status == FootballMatchStatus.Completed)
            throw new InvalidOperationException("Cannot delete substitution events from a completed match.");

        FootballSubstitution? substitution = _events.OfType<FootballSubstitution>()
            .FirstOrDefault(s => s.Id == substitutionEventId);
        if (substitution == null)
            throw new ArgumentException($"Substitution event with ID {substitutionEventId} not found in this match.", nameof(substitutionEventId));

        FootballMatchLineupPlayer offEntry = GetLineupEntry(substitution.TeamId, substitution.PlayerOffId, "Player who went off");
        FootballMatchLineupPlayer onEntry = GetLineupEntry(substitution.TeamId, substitution.PlayerOnId, "Player who came on");
        if (!onEntry.IsSentOff)
        {
            onEntry.TakeOffField();
            offEntry.PutOnField();
        }

        _events.Remove(substitution);
        return substitution;
    }

    public void EndPeriod(int periodNumber)
    {
        EnsureInProgress("end a period");
        EnsureValidPeriod(periodNumber);

        FootballPeriodScore? periodScore = _periodScores.FirstOrDefault(ps => ps.PeriodNumber == periodNumber);
        if (periodScore == null)
            throw new InvalidOperationException($"Period {periodNumber} has not been started.");
        periodScore.Complete();
    }

    public void SetLineup(Guid teamId, IEnumerable<FootballLineupSelection> selections)
    {
        ArgumentNullException.ThrowIfNull(selections);
        if (Status != FootballMatchStatus.Scheduled && Status != FootballMatchStatus.InProgress)
            throw new InvalidOperationException($"Cannot change lineup when match status is {Status}.");

        FootballTeam team = ResolveParticipatingTeam(teamId);

        List<FootballLineupSelection> distinct = selections
            .GroupBy(s => s.PlayerId)
            .Select(g => g.First())
            .ToList();

        if (distinct.Any(s => s.PlayerId == Guid.Empty))
            throw new ArgumentException("Player IDs cannot contain Guid.Empty.", nameof(selections));

        foreach (FootballLineupSelection selection in distinct)
            ValidatePlayerOnRoster(team, selection.PlayerId, "Lineup player");

        int onFieldCount = distinct.Count(s => s.IsOnField);
        if (onFieldCount > MatchRules.PlayersOnField)
            throw new ArgumentException($"A team cannot have more than {MatchRules.PlayersOnField} players on the field.");

        if (MatchRules.RequireGoalkeeper)
        {
            int goalkeepersOnField = distinct.Count(s => s.IsOnField && s.Position == FootballPosition.Goalkeeper);
            if (onFieldCount == MatchRules.PlayersOnField && goalkeepersOnField != 1)
                throw new ArgumentException("Starting lineup must include exactly one goalkeeper.");
        }

        Dictionary<Guid, FootballMatchLineupPlayer> existingByPlayer = _lineup
            .Where(p => p.TeamId == teamId)
            .ToDictionary(p => p.PlayerId);

        _lineup.RemoveAll(p => p.TeamId == teamId);
        foreach (FootballLineupSelection selection in distinct)
        {
            FootballMatchLineupPlayer entry = new(Id, teamId, selection.PlayerId, selection.Position, selection.IsOnField);
            if (existingByPlayer.TryGetValue(selection.PlayerId, out FootballMatchLineupPlayer? previous) && previous.IsSentOff)
                entry.SendOff();
            _lineup.Add(entry);
        }
    }

    public void SetTournamentInfo(FootballTournamentStage stage, Guid? groupId = null)
    {
        TournamentStage = stage;
        TournamentGroupId = groupId;
    }

    public void SetPlayoffInfo(
        FootballPlayoffRound round,
        int matchOrder,
        Guid? nextMatchId,
        FootballPlayoffSlot? nextMatchSlot)
    {
        if (matchOrder < 0)
            throw new ArgumentOutOfRangeException(nameof(matchOrder), "Playoff match order must be non-negative.");
        if (nextMatchId.HasValue != nextMatchSlot.HasValue)
            throw new ArgumentException("NextMatchId and NextMatchSlot must be provided together.");

        PlayoffRound = round;
        PlayoffMatchOrder = matchOrder;
        NextMatchId = nextMatchId;
        NextMatchSlot = nextMatchSlot;
        TournamentStage = MapRoundToStage(round);
    }

    public void AssignTeam(FootballPlayoffSlot slot, FootballTeam? team)
    {
        if (Status != FootballMatchStatus.Scheduled && Status != FootballMatchStatus.Postponed)
            throw new InvalidOperationException($"Cannot assign a team when match status is {Status}.");

        if (team != null)
        {
            Guid? otherTeamId = slot == FootballPlayoffSlot.Home ? AwayTeamId : HomeTeamId;
            if (otherTeamId.HasValue && otherTeamId.Value == team.Id)
                throw new ArgumentException("Home team and away team cannot be the same team.");
        }

        if (slot == FootballPlayoffSlot.Home)
        {
            HomeTeam = team;
            HomeTeamId = team?.Id;
        }
        else
        {
            AwayTeam = team;
            AwayTeamId = team?.Id;
        }

        Guid stampedId = team?.Id ?? Guid.Empty;
        foreach (FootballPeriodScore ps in _periodScores)
            ps.UpdateTeamId(slot, stampedId);
    }

    public void AssignPlayoffTeam(FootballPlayoffSlot slot, FootballTeam team)
    {
        ArgumentNullException.ThrowIfNull(team);
        AssignTeam(slot, team);
    }

    private static FootballTournamentStage MapRoundToStage(FootballPlayoffRound round) =>
        round switch
        {
            FootballPlayoffRound.QuarterFinal => FootballTournamentStage.Quarterfinal,
            FootballPlayoffRound.SemiFinal => FootballTournamentStage.Semifinal,
            FootballPlayoffRound.ThirdPlaceMatch => FootballTournamentStage.ThirdPlace,
            FootballPlayoffRound.Final => FootballTournamentStage.Final,
            _ => FootballTournamentStage.None
        };

    private void EnsureInProgress(string action)
    {
        if (Status != FootballMatchStatus.InProgress)
            throw new InvalidOperationException($"Cannot {action} when match status is {Status}.");
    }

    private void EnsureValidPeriod(int periodNumber)
    {
        if (periodNumber < 1 || periodNumber > MatchRules.MaxPeriodNumber)
            throw new ArgumentOutOfRangeException(nameof(periodNumber), $"Period number must be between 1 and {MatchRules.MaxPeriodNumber}.");
    }

    private static void ValidatePlayerOnRoster(FootballTeam team, Guid playerId, string roleName)
    {
        if (!team.Roster.Any(tp => tp.PlayerId == playerId))
            throw new ArgumentException($"{roleName} is not on the team's roster.");
    }

    private FootballTeam ResolveParticipatingTeam(Guid teamId)
    {
        if (HomeTeamId.HasValue && teamId == HomeTeamId.Value)
            return HomeTeam!;
        if (AwayTeamId.HasValue && teamId == AwayTeamId.Value)
            return AwayTeam!;
        throw new ArgumentException("Team is not participating in this match.", nameof(teamId));
    }

    private FootballTeam OpponentOf(FootballTeam team)
    {
        if (HomeTeamId.HasValue && team.Id == HomeTeamId.Value)
            return AwayTeam ?? throw new InvalidOperationException("Away team is not assigned.");
        if (AwayTeamId.HasValue && team.Id == AwayTeamId.Value)
            return HomeTeam ?? throw new InvalidOperationException("Home team is not assigned.");
        throw new ArgumentException("Team is not participating in this match.", nameof(team));
    }

    private FootballMatchLineupPlayer GetLineupEntry(Guid teamId, Guid playerId, string roleName)
    {
        FootballMatchLineupPlayer? entry = _lineup.FirstOrDefault(p => p.TeamId == teamId && p.PlayerId == playerId);
        if (entry == null)
            throw new InvalidOperationException($"{roleName} is not in the match squad.");
        return entry;
    }

    private void EnsureLineupReady(Guid teamId, string side)
    {
        List<FootballMatchLineupPlayer> onField = _lineup.Where(p => p.TeamId == teamId && p.IsOnField).ToList();
        if (onField.Count != MatchRules.PlayersOnField)
            throw new InvalidOperationException($"Cannot start: {side} team must have {MatchRules.PlayersOnField} players on the field.");

        if (MatchRules.RequireGoalkeeper && onField.Count(p => p.Position == FootballPosition.Goalkeeper) != 1)
            throw new InvalidOperationException($"Cannot start: {side} team must have exactly one goalkeeper on the field.");
    }

    private bool HasUnconvertedYellow(Guid teamId, Guid playerId)
    {
        List<FootballCard> cards = _events.OfType<FootballCard>()
            .Where(c => c.TeamId == teamId && c.PlayerId == playerId)
            .ToList();
        int yellows = cards.Count(c => c.CardType == FootballCardType.Yellow);
        int secondYellows = cards.Count(c => c.CardType == FootballCardType.SecondYellow);
        return yellows > secondYellows;
    }

    private bool HasSendingOffRemaining(Guid teamId, Guid playerId) =>
        _events.OfType<FootballCard>().Any(c =>
            c.TeamId == teamId && c.PlayerId == playerId && c.ResultsInSendingOff);

    private void AdjustScore(Guid teamId, int periodNumber, bool increment)
    {
        bool isHome = teamId == HomeTeamId;
        if (isHome)
        {
            if (increment) HomeScore++; else HomeScore--;
        }
        else
        {
            if (increment) AwayScore++; else AwayScore--;
        }

        FootballPeriodScore? periodScore = _periodScores.FirstOrDefault(ps => ps.PeriodNumber == periodNumber);
        if (periodScore == null)
            return;

        if (isHome)
        {
            if (increment) periodScore.IncrementHomeScore(); else periodScore.DecrementHomeScore();
        }
        else
        {
            if (increment) periodScore.IncrementAwayScore(); else periodScore.DecrementAwayScore();
        }
    }

    private static FootballMatchRules CloneRules(FootballMatchRules source) =>
        new(
            source.NumberOfHalves,
            source.HalfDurationMinutes,
            source.PlayersOnField,
            source.RequireGoalkeeper,
            source.MaxSubstitutions,
            source.RequireOfficialsToStart,
            source.AllowExtraTime,
            source.ExtraTimeHalfCount == 0 ? 2 : source.ExtraTimeHalfCount,
            source.ExtraTimeHalfDurationMinutes == 0 ? 15 : source.ExtraTimeHalfDurationMinutes,
            source.AllowPenaltyShootout);
}
