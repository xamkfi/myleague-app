using Application.Common;
using Application.Features.Floorball.Matches.Commands;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Matches.Handlers;
using Domain.Entities.Common;
using Domain.Entities.Floorball;
using Domain.Enums.Common;
using Domain.Enums.Floorball;
using Domain.Repositories.Floorball;
using Domain.ValueObjects.Floorball;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.FloorballMatches;

/// <summary>
/// Behavioural tests for <see cref="AssignMatchTeamsHandler"/>. Cover:
///   1. Filling a placeholder slot succeeds and persists.
///   2. Same-team-on-both-slots bubbles up as a Failure (domain rule).
///   3. Wrong-status matches are rejected.
///   4. NotFound for unknown match / unknown team IDs.
///   5. Playoff propagation forwards the *home* team to the next match's projected slot, but
///      stops as soon as the downstream match is no longer Scheduled/Postponed.
/// </summary>
public class AssignMatchTeamsHandlerTests
{
    private readonly Mock<IFloorballMatchRepository> _matchRepo = new();
    private readonly Mock<IFloorballTeamRepository> _teamRepo = new();
    private readonly Mock<IFloorballUnitOfWork> _unitOfWork = new();
    private readonly Mock<ILogger<AssignMatchTeamsHandler>> _logger = new();

    private readonly AssignMatchTeamsHandler _handler;

    public AssignMatchTeamsHandlerTests()
    {
        _handler = new AssignMatchTeamsHandler(
            _matchRepo.Object,
            _teamRepo.Object,
            _unitOfWork.Object,
            _logger.Object);
    }

    private static FloorballTournament CreateTournament()
    {
        return new FloorballTournament(
            "Test Tournament",
            new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 9, 7, 0, 0, 0, DateTimeKind.Utc));
    }

    private static FloorballTeam CreateTeam(string name)
    {
        Club club = new Club(name + " HC");
        return new FloorballTeam(
            name,
            divisionId: null,
            club,
            homeArena: "Test Arena",
            primaryJerseyColor: "Blue",
            teamCategory: TeamCategory.Adult);
    }

    private static FloorballMatch CreatePlayoffMatch(
        FloorballTournament competition,
        FloorballTeam? home,
        FloorballTeam? away,
        Guid? id = null)
    {
        return FloorballMatch.CreatePlayoffMatch(
            id ?? Guid.NewGuid(),
            competition,
            home,
            away,
            new DateTime(2026, 9, 6, 18, 0, 0, DateTimeKind.Utc),
            "Arena",
            new FloorballMatchRules(3, 20, true, 5, true));
    }

    [Fact]
    public async Task Handle_FillsPlaceholderSlots_AndPersists()
    {
        // Arrange: a scheduled placeholder match has no teams set yet, the admin selects both.
        FloorballTournament tournament = CreateTournament();
        FloorballMatch match = CreatePlayoffMatch(tournament, home: null, away: null);
        FloorballTeam home = CreateTeam("Wolves");
        FloorballTeam away = CreateTeam("Bears");

        _matchRepo.Setup(r => r.GetByIdAsync(match.Id)).ReturnsAsync(match);
        _teamRepo.Setup(r => r.GetByIdAsync((Guid?)home.Id)).ReturnsAsync(home);
        _teamRepo.Setup(r => r.GetByIdAsync((Guid?)away.Id)).ReturnsAsync(away);

        AssignMatchTeamsCommand command = new AssignMatchTeamsCommand(match.Id, home.Id, away.Id);

        // Act
        Result<FloorballMatchDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        match.HomeTeamId.Should().Be(home.Id);
        match.AwayTeamId.Should().Be(away.Id);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_BothSlotsSameTeam_ReturnsFailure()
    {
        // Arrange: same team in both slots is a domain invariant violation. The handler must
        // catch the ArgumentException and turn it into a Result.Failure so the controller can
        // serialise it as 400 BadRequest instead of leaking the exception as 500.
        FloorballTournament tournament = CreateTournament();
        FloorballMatch match = CreatePlayoffMatch(tournament, home: null, away: null);
        FloorballTeam team = CreateTeam("Wolves");

        _matchRepo.Setup(r => r.GetByIdAsync(match.Id)).ReturnsAsync(match);
        _teamRepo.Setup(r => r.GetByIdAsync((Guid?)team.Id)).ReturnsAsync(team);

        AssignMatchTeamsCommand command = new AssignMatchTeamsCommand(match.Id, team.Id, team.Id);

        // Act
        Result<FloorballMatchDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("same");
        // Even though the first AssignTeam succeeded in-memory, the unit of work must NOT have
        // been committed — partial saves would be confusing to operators.
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WrongStatus_ReturnsFailure()
    {
        // Arrange: a Completed match should never have its teams swapped via this command
        // (use the reopen flow first).
        FloorballTournament tournament = CreateTournament();
        FloorballTeam home = CreateTeam("Wolves");
        FloorballTeam away = CreateTeam("Bears");
        FloorballMatch match = CreatePlayoffMatch(tournament, home, away);
        // Move through the realistic state machine to land in Completed. Cancel() is the
        // safest transition that doesn't require officials/rosters.
        match.Cancel();

        _matchRepo.Setup(r => r.GetByIdAsync(match.Id)).ReturnsAsync(match);

        AssignMatchTeamsCommand command = new AssignMatchTeamsCommand(match.Id, away.Id, home.Id);

        // Act
        Result<FloorballMatchDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("ajastetuille");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UnknownMatch_ReturnsNotFound()
    {
        Guid missingId = Guid.NewGuid();
        _matchRepo.Setup(r => r.GetByIdAsync(missingId)).ReturnsAsync((FloorballMatch?)null);

        AssignMatchTeamsCommand command = new AssignMatchTeamsCommand(missingId, null, null);

        Result<FloorballMatchDto> result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain(missingId.ToString());
    }

    [Fact]
    public async Task Handle_UnknownTeamId_ReturnsNotFoundWithoutMutatingMatch()
    {
        FloorballTournament tournament = CreateTournament();
        FloorballMatch match = CreatePlayoffMatch(tournament, home: null, away: null);
        Guid missingTeamId = Guid.NewGuid();

        _matchRepo.Setup(r => r.GetByIdAsync(match.Id)).ReturnsAsync(match);
        _teamRepo.Setup(r => r.GetByIdAsync((Guid?)missingTeamId)).ReturnsAsync((FloorballTeam?)null);

        AssignMatchTeamsCommand command = new AssignMatchTeamsCommand(match.Id, missingTeamId, null);

        Result<FloorballMatchDto> result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        match.HomeTeamId.Should().BeNull();
        match.AwayTeamId.Should().BeNull();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_PlayoffPropagation_UpdatesScheduledDownstreamSlot()
    {
        // Arrange: a QF feeds into a SF. The QF's home team changes from null → Wolves; the SF
        // is still Scheduled with a TBD home slot, so propagation should fill that slot in.
        FloorballTournament tournament = CreateTournament();
        FloorballMatch semiFinal = CreatePlayoffMatch(tournament, home: null, away: null);
        FloorballMatch quarterFinal = CreatePlayoffMatch(tournament, home: null, away: null);
        // QF winner projects into SF's Home slot.
        quarterFinal.SetPlayoffInfo(
            FloorballPlayoffRound.QuarterFinal,
            matchOrder: 0,
            nextMatchId: semiFinal.Id,
            nextMatchSlot: FloorballPlayoffSlot.Home);

        FloorballTeam newHome = CreateTeam("Wolves");

        _matchRepo.Setup(r => r.GetByIdAsync(quarterFinal.Id)).ReturnsAsync(quarterFinal);
        _matchRepo.Setup(r => r.GetByIdAsync(semiFinal.Id)).ReturnsAsync(semiFinal);
        _teamRepo.Setup(r => r.GetByIdAsync((Guid?)newHome.Id)).ReturnsAsync(newHome);

        AssignMatchTeamsCommand command = new AssignMatchTeamsCommand(quarterFinal.Id, newHome.Id, null);

        // Act
        Result<FloorballMatchDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        quarterFinal.HomeTeamId.Should().Be(newHome.Id);
        // The downstream SF should now show the new team in its Home slot — this is the core
        // "jury override flows forward" promise the AssignTeams flow makes.
        semiFinal.HomeTeamId.Should().Be(newHome.Id);
    }

    [Fact]
    public async Task Handle_PlayoffPropagation_DoesNotOverwriteStartedDownstreamMatch()
    {
        // Arrange: same QF→SF setup, but the SF has already started (status InProgress means a
        // real participant was already in the slot and the operator made it live). Propagation
        // must skip the SF — overwriting an in-progress match would destroy operator state.
        FloorballTournament tournament = CreateTournament();
        FloorballTeam existingSemiHome = CreateTeam("Bears");
        FloorballTeam existingSemiAway = CreateTeam("Lynxes");
        FloorballMatch semiFinal = CreatePlayoffMatch(tournament, existingSemiHome, existingSemiAway);
        // Move SF to a "no longer schedule-only" status. We use Postpone() then would need
        // InProgress, but starting requires officials + goalies which are not part of this
        // unit test's surface. Cancel() is the simplest reachable non-Scheduled status that
        // exercises the same propagation guard.
        semiFinal.Cancel();

        FloorballMatch quarterFinal = CreatePlayoffMatch(tournament, home: null, away: null);
        quarterFinal.SetPlayoffInfo(
            FloorballPlayoffRound.QuarterFinal,
            matchOrder: 0,
            nextMatchId: semiFinal.Id,
            nextMatchSlot: FloorballPlayoffSlot.Home);

        FloorballTeam newHome = CreateTeam("Wolves");

        _matchRepo.Setup(r => r.GetByIdAsync(quarterFinal.Id)).ReturnsAsync(quarterFinal);
        _matchRepo.Setup(r => r.GetByIdAsync(semiFinal.Id)).ReturnsAsync(semiFinal);
        _teamRepo.Setup(r => r.GetByIdAsync((Guid?)newHome.Id)).ReturnsAsync(newHome);

        AssignMatchTeamsCommand command = new AssignMatchTeamsCommand(quarterFinal.Id, newHome.Id, null);

        // Act
        Result<FloorballMatchDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        quarterFinal.HomeTeamId.Should().Be(newHome.Id);
        // SF home slot must be untouched — propagation should have refused to overwrite a
        // match that is no longer Scheduled/Postponed.
        semiFinal.HomeTeamId.Should().Be(existingSemiHome.Id);
    }
}
