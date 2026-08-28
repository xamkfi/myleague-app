using Domain.Entities.Hockey.Competitions;
using Domain.Entities.Hockey.Matches;
using Domain.Entities.Hockey.Matches.Events;
using Domain.Entities.Hockey.Statistics;
using Domain.Entities.Hockey.Teams;
using Domain.Enums.Hockey.Competitions;
using Domain.Enums.Hockey.Matches;
using Domain.Enums.Hockey.Statistics;
using Domain.Enums.Hockey.Teams;
using Domain.Services.Hockey;
using Domain.ValueObjects.Hockey.Rules;

namespace DomainTestProject.Hockey;

/// <summary>
/// Focused domain behavior tests for the hockey model (no database / EF).
/// </summary>
public class HockeyDomainBehaviorTests
{
    [Fact]
    public void HockeySeason_And_HockeyTournament_Inherit_HockeyCompetition()
    {
        HockeySeason season = HockeyTestHelpers.CreateSeason();
        HockeyTournament tournament = HockeyTestHelpers.CreateTournament();

        season.Should().BeAssignableTo<HockeyCompetition>();
        tournament.Should().BeAssignableTo<HockeyCompetition>();
        season.CompetitionType.Should().Be(HockeyCompetitionType.Season);
        tournament.CompetitionType.Should().Be(HockeyCompetitionType.Tournament);
        season.Status.Should().Be(HockeyCompetitionStatus.Draft);
        tournament.Status.Should().Be(HockeyCompetitionStatus.Draft);
    }

    [Fact]
    public void CannotModifyCompletedCompetition()
    {
        HockeySeason season = HockeyTestHelpers.CreateSeason();
        HockeyTestHelpers.ActivateCompetition(season);
        season.Complete();

        Action act = () => season.AddTeam(Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Completed*");
    }

    [Fact]
    public void CannotAddTwoHomeTeamsToSameMatch()
    {
        HockeyTeam home = HockeyTestHelpers.CreateTeam("Home");
        HockeyTeam other = HockeyTestHelpers.CreateTeam("Other");
        HockeyMatch match = HockeyTestHelpers.CreateStandaloneMatch();
        match.AssignMatchTeam(home.Id, HockeyTeamSlot.Home);

        Action act = () => match.AssignMatchTeam(other.Id, HockeyTeamSlot.Home);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Home*");
    }

    [Fact]
    public void HomeTeamId_And_AwayTeamId_Come_From_MatchTeams()
    {
        HockeyTeam home = HockeyTestHelpers.CreateTeam("Wolves");
        HockeyTeam away = HockeyTestHelpers.CreateTeam("Bears");
        (HockeyMatch match, HockeyMatchTeam homeSide, HockeyMatchTeam awaySide) =
            HockeyTestHelpers.CreateMatchWithSides(home, away);

        match.HomeTeamId.Should().Be(home.Id);
        match.AwayTeamId.Should().Be(away.Id);
        match.HomeMatchTeam.Should().BeSameAs(homeSide);
        match.AwayMatchTeam.Should().BeSameAs(awaySide);
        homeSide.TeamSlot.Should().Be(HockeyTeamSlot.Home);
        awaySide.TeamSlot.Should().Be(HockeyTeamSlot.Away);
    }

    [Fact]
    public void CompetitionMatch_Requires_CompetitionTeam_From_SameCompetition()
    {
        HockeySeason season = HockeyTestHelpers.CreateSeason();
        HockeyTeam home = HockeyTestHelpers.CreateTeam("Wolves");
        HockeyTeam away = HockeyTestHelpers.CreateTeam("Bears");
        HockeyCompetitionTeam homeCt = season.AddTeam(home.Id);
        HockeyCompetitionTeam awayCt = season.AddTeam(away.Id);
        HockeyMatch match = HockeyTestHelpers.CreateCompetitionMatch(season);

        Action withoutCompetitionTeam = () =>
            match.AssignMatchTeam(home.Id, HockeyTeamSlot.Home);

        withoutCompetitionTeam.Should().Throw<ArgumentNullException>();

        match.AssignMatchTeam(home.Id, HockeyTeamSlot.Home, homeCt);
        match.AssignMatchTeam(away.Id, HockeyTeamSlot.Away, awayCt);

        match.HomeMatchTeam!.CompetitionTeamId.Should().Be(homeCt.Id);
        match.AwayMatchTeam!.CompetitionTeamId.Should().Be(awayCt.Id);
    }

    [Fact]
    public void CannotAddSamePlayerTwiceToActiveRoster()
    {
        HockeyTeam team = HockeyTestHelpers.CreateTeam();
        HockeyPlayer player = HockeyTestHelpers.CreatePlayer();
        team.AddPlayer(player, HockeyPosition.Center, jerseyNumber: 10);

        Action act = () => team.AddPlayer(player, HockeyPosition.LeftWing, jerseyNumber: 11);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already on this roster*");
    }

    [Fact]
    public void AddingGoalIncrementsScoringTeamGoals()
    {
        HockeyTeam home = HockeyTestHelpers.CreateTeam("Wolves");
        HockeyTeam away = HockeyTestHelpers.CreateTeam("Bears");
        (HockeyMatch match, HockeyMatchTeam homeSide, _) =
            HockeyTestHelpers.CreateMatchWithSides(home, away);

        HockeyTeamPlayer scorerTp = HockeyTestHelpers.AddRosterPlayer(home, jerseyNumber: 19);
        HockeyMatchActivePlayer scorer = HockeyTestHelpers.DressPlayer(homeSide, scorerTp, 19);

        HockeyGoal goal = new(
            match.Id,
            homeSide.Id,
            scorer.Id,
            periodNumber: 1,
            gameTime: TimeSpan.FromMinutes(5),
            HockeyGoalStrength.EvenStrength);

        match.AddEvent(goal);

        homeSide.Goals.Should().Be(1);
        match.HomeScore.Should().Be(1);
        match.AwayScore.Should().Be(0);
    }

    [Fact]
    public void ShotCanBeLinkedToGoal()
    {
        HockeyTeam home = HockeyTestHelpers.CreateTeam("Wolves");
        HockeyTeam away = HockeyTestHelpers.CreateTeam("Bears");
        (HockeyMatch match, HockeyMatchTeam homeSide, _) =
            HockeyTestHelpers.CreateMatchWithSides(home, away);

        HockeyTeamPlayer scorerTp = HockeyTestHelpers.AddRosterPlayer(home, jerseyNumber: 19);
        HockeyMatchActivePlayer scorer = HockeyTestHelpers.DressPlayer(homeSide, scorerTp, 19);

        HockeyShot shot = new(
            match.Id,
            homeSide.Id,
            periodNumber: 1,
            gameTime: TimeSpan.FromMinutes(4),
            HockeyShotResult.Goal,
            countsAsShotOnGoal: true,
            shooterActivePlayerId: scorer.Id);
        match.AddEvent(shot);

        HockeyGoal goal = new(
            match.Id,
            homeSide.Id,
            scorer.Id,
            periodNumber: 1,
            gameTime: TimeSpan.FromMinutes(4),
            HockeyGoalStrength.EvenStrength);
        goal.LinkRelatedShot(shot);
        match.AddEvent(goal);

        goal.RelatedShotId.Should().Be(shot.Id);
    }

    [Fact]
    public void FailedCoachChallengeCreatesBenchMinorWhenRulesRequirePenalty()
    {
        HockeyTeam home = HockeyTestHelpers.CreateTeam("Wolves");
        HockeyTeam away = HockeyTestHelpers.CreateTeam("Bears");
        (HockeyMatch match, HockeyMatchTeam homeSide, _) =
            HockeyTestHelpers.CreateMatchWithSides(home, away);

        HockeyVideoReview review = new(
            match.Id,
            periodNumber: 2,
            gameTime: TimeSpan.FromMinutes(10),
            HockeyVideoReviewType.OffsideBeforeGoal,
            HockeyReviewDecision.Goal,
            HockeyReviewDecision.NoGoal,
            isCoachChallenge: true,
            wasSuccessful: false,
            requestedByMatchTeamId: homeSide.Id);
        match.AddEvent(review);

        HockeyCoachChallengeRules rules = HockeyTestHelpers.FailedChallengeBenchMinorRules();

        HockeyCoachChallengeResult result = HockeyCoachChallengeService.HandleFailedChallenge(
            match,
            review,
            rules,
            homeSide.Id);

        result.Validation.IsValid.Should().BeTrue();
        result.ResultingPenalty.Should().NotBeNull();
        result.ResultingPenalty!.IsBenchPenalty.Should().BeTrue();
        result.ResultingPenalty.Severity.Should().Be(HockeyPenaltySeverity.BenchMinor);
        result.ResultingPenalty.PenaltyMinutes.Should().Be(2);
        review.ResultingPenaltyId.Should().Be(result.ResultingPenalty.Id);
        match.Events.OfType<HockeyPenalty>().Should().ContainSingle();
    }

    [Fact]
    public void TournamentStatisticsUseCompetitionId_And_GroupScopeUsesTournamentGroupId()
    {
        HockeyTournament tournament = HockeyTestHelpers.CreateTournament();
        HockeyTournamentGroup group = tournament.AddGroup("A");
        HockeyTeam team = HockeyTestHelpers.CreateTeam("Wolves");

        HockeyTeamCompetitionStatistics tournamentStats = new(
            team.Id,
            tournament.Id,
            HockeyStatisticsScope.Competition);
        tournamentStats.UpdateRecord(
            gamesPlayed: 2,
            regulationWins: 1,
            overtimeWins: 1,
            shootoutWins: 0,
            regulationLosses: 0,
            overtimeLosses: 0,
            shootoutLosses: 0,
            ties: 0,
            homeWins: 1,
            homeLosses: 0,
            awayWins: 1,
            awayLosses: 0);
        tournamentStats.RecalculateStandingsMetrics(HockeyStandingRules.Default());

        tournamentStats.CompetitionId.Should().Be(tournament.Id);
        tournamentStats.Scope.Should().Be(HockeyStatisticsScope.Competition);
        // Default rules: regulation win 3 + OT win 2
        tournamentStats.Points.Should().Be(5);
        tournamentStats.Wins.Should().Be(2);

        HockeyTeamCompetitionStatistics groupStats = new(
            team.Id,
            tournament.Id,
            HockeyStatisticsScope.TournamentGroup,
            tournamentGroupId: group.Id);

        groupStats.CompetitionId.Should().Be(tournament.Id);
        groupStats.Scope.Should().Be(HockeyStatisticsScope.TournamentGroup);
        groupStats.TournamentGroupId.Should().Be(group.Id);
    }

    [Fact]
    public void HockeyTournament_SetChampion_WhenNotCompleted_Throws()
    {
        HockeyTournament tournament = HockeyTestHelpers.CreateTournament();

        Action act = () => tournament.SetChampion(Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*completed tournament*");
    }

    [Fact]
    public void HockeyTournament_SetChampion_WhenCompleted_SetsChampion()
    {
        HockeyTournament tournament = HockeyTestHelpers.CreateTournament();
        tournament.Publish();
        tournament.Activate();
        tournament.Complete();
        Guid championId = Guid.NewGuid();

        tournament.SetChampion(championId);

        tournament.ChampionCompetitionTeamId.Should().Be(championId);
    }
}
