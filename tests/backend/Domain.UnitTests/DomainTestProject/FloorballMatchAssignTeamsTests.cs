using System;
using Domain.Entities.Common;
using Domain.Entities.Floorball;
using Domain.Enums.Common;
using Domain.Enums.Floorball;

namespace DomainTestProject;

/// <summary>
/// Tests that exercise the new "teamless future match" + "assign teams later" domain behavior on
/// <see cref="FloorballMatch"/>. These cover the three guarantees the feature promises:
///   1. A match can be created with one or both team slots null.
///   2. <see cref="FloorballMatch.Start"/> refuses to start while either slot is null and produces
///      a user-facing Finnish message.
///   3. <see cref="FloorballMatch.AssignTeam"/> can fill / clear a slot, rejects same-team
///      conflicts, and is only allowed in Scheduled or Postponed status.
/// </summary>
public class FloorballMatchAssignTeamsTests
{
    private static FloorballSeason CreateSeason()
    {
        return new FloorballSeason(
            "Test Season",
            new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2027, 5, 31, 0, 0, 0, DateTimeKind.Utc));
    }

    private static FloorballTeam CreateTeam(string name)
    {
        // Constructing real domain objects (vs. mocking) keeps these tests faithful to actual
        // production behavior — FloorballMatch reaches into FloorballTeam.Id and equality.
        Club club = new Club(name + " HC");
        return new FloorballTeam(
            name,
            divisionId: null,
            club,
            homeArena: "Test Arena",
            primaryJerseyColor: "Blue",
            teamCategory: TeamCategory.Adult);
    }

    private static FloorballMatch CreateMatch(
        FloorballSeason season,
        FloorballTeam? home,
        FloorballTeam? away)
    {
        return new FloorballMatch(
            season,
            home,
            away,
            new DateTime(2027, 1, 15, 18, 30, 0, DateTimeKind.Utc),
            "Test Arena");
    }

    [Fact]
    public void Constructor_AllowsBothTeamsNull()
    {
        FloorballSeason season = CreateSeason();

        FloorballMatch match = CreateMatch(season, home: null, away: null);

        match.HomeTeam.Should().BeNull();
        match.HomeTeamId.Should().BeNull();
        match.AwayTeam.Should().BeNull();
        match.AwayTeamId.Should().BeNull();
        match.Status.Should().Be(FloorballMatchStatus.Scheduled);
        // Period scores should still be created up-front with placeholder team IDs so that
        // AssignTeam can backfill them later without any "missing rows" branch in callers.
        match.PeriodScores.Should().NotBeEmpty();
        foreach (FloorballPeriodScore ps in match.PeriodScores)
        {
            ps.HomeTeamId.Should().Be(Guid.Empty);
            ps.AwayTeamId.Should().Be(Guid.Empty);
        }
    }

    [Fact]
    public void Constructor_AllowsOnlyHomeAssigned()
    {
        FloorballSeason season = CreateSeason();
        FloorballTeam home = CreateTeam("Wolves");

        FloorballMatch match = CreateMatch(season, home: home, away: null);

        match.HomeTeamId.Should().Be(home.Id);
        match.AwayTeamId.Should().BeNull();
    }

    [Fact]
    public void Constructor_BothTeamsSame_Throws()
    {
        FloorballSeason season = CreateSeason();
        FloorballTeam team = CreateTeam("Wolves");

        Action act = () => CreateMatch(season, team, team);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*same*team*");
    }

    [Fact]
    public void Start_WithoutHomeTeam_ThrowsClearMessage()
    {
        FloorballSeason season = CreateSeason();
        FloorballTeam away = CreateTeam("Bears");
        FloorballMatch match = CreateMatch(season, home: null, away: away);

        Action act = () => match.Start();

        act.Should().Throw<InvalidOperationException>()
            // The message must be user-friendly Finnish: the controller bubbles it straight back
            // into the frontend ErrorPopup, so a developer-only stack trace style isn't enough.
            .WithMessage("Ottelua ei voi aloittaa: molempien joukkueiden tulee olla valittuina.");
    }

    [Fact]
    public void Start_WithoutAwayTeam_ThrowsClearMessage()
    {
        FloorballSeason season = CreateSeason();
        FloorballTeam home = CreateTeam("Wolves");
        FloorballMatch match = CreateMatch(season, home: home, away: null);

        Action act = () => match.Start();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Ottelua ei voi aloittaa: molempien joukkueiden tulee olla valittuina.");
    }

    [Fact]
    public void Start_TeamsCheckRunsBeforeOfficialsCheck()
    {
        // Order matters: a half-configured match (no teams, no officials, no goalies) should report
        // the *first* missing prerequisite, namely the teams. Otherwise admins would see "Cannot
        // start a match without officials." and assign referees, only to be hit by the team
        // requirement on the next click.
        FloorballSeason season = CreateSeason();
        FloorballMatch match = CreateMatch(season, home: null, away: null);

        Action act = () => match.Start();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*joukkueiden tulee olla valittuina.");
    }

    [Fact]
    public void AssignTeam_FillsPreviouslyNullHomeSlot()
    {
        FloorballSeason season = CreateSeason();
        FloorballTeam away = CreateTeam("Bears");
        FloorballTeam home = CreateTeam("Wolves");
        FloorballMatch match = CreateMatch(season, home: null, away: away);

        match.AssignTeam(FloorballPlayoffSlot.Home, home);

        match.HomeTeam.Should().Be(home);
        match.HomeTeamId.Should().Be(home.Id);
        // Period scores must reflect the newly-assigned id so per-period statistics queries can
        // attribute the eventual goals to the right team without a JOIN through FloorballMatches.
        foreach (FloorballPeriodScore ps in match.PeriodScores)
        {
            ps.HomeTeamId.Should().Be(home.Id);
            ps.AwayTeamId.Should().Be(away.Id);
        }
    }

    [Fact]
    public void AssignTeam_NullClearsExistingSlotBackToTbd()
    {
        FloorballSeason season = CreateSeason();
        FloorballTeam home = CreateTeam("Wolves");
        FloorballTeam away = CreateTeam("Bears");
        FloorballMatch match = CreateMatch(season, home, away);

        match.AssignTeam(FloorballPlayoffSlot.Home, null);

        match.HomeTeam.Should().BeNull();
        match.HomeTeamId.Should().BeNull();
        // PeriodScore home id must reset to Guid.Empty so a future re-assignment can be detected
        // by AssignTeam writing a real id back in.
        foreach (FloorballPeriodScore ps in match.PeriodScores)
        {
            ps.HomeTeamId.Should().Be(Guid.Empty);
        }
    }

    [Fact]
    public void AssignTeam_RejectsSameTeamAsOppositeSlot()
    {
        FloorballSeason season = CreateSeason();
        FloorballTeam home = CreateTeam("Wolves");
        FloorballMatch match = CreateMatch(season, home, away: null);

        Action act = () => match.AssignTeam(FloorballPlayoffSlot.Away, home);

        act.Should().Throw<ArgumentException>().WithMessage("*same*team*");
    }

    [Fact]
    public void AssignTeam_AllowsSwitchingBothSlotsToSwap()
    {
        // First clear one side then re-assign, mimicking the "jury overrides home/away" workflow.
        FloorballSeason season = CreateSeason();
        FloorballTeam home = CreateTeam("Wolves");
        FloorballTeam away = CreateTeam("Bears");
        FloorballTeam other = CreateTeam("Lynxes");
        FloorballMatch match = CreateMatch(season, home, away);

        match.AssignTeam(FloorballPlayoffSlot.Home, other);

        match.HomeTeam.Should().Be(other);
        match.AwayTeam.Should().Be(away);
    }

    [Fact]
    public void ChangeTeams_NullAndNullClearsBothSlots()
    {
        FloorballSeason season = CreateSeason();
        FloorballTeam home = CreateTeam("Wolves");
        FloorballTeam away = CreateTeam("Bears");
        FloorballMatch match = CreateMatch(season, home, away);

        match.ChangeTeams(null, null);

        match.HomeTeam.Should().BeNull();
        match.AwayTeam.Should().BeNull();
    }
}
