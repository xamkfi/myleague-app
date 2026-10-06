using Domain.Constants;
using Domain.Entities.Common;
using Domain.Entities.Floorball.Teams;
using Domain.Entities.Football.Teams;
using Domain.Entities.Hockey.Teams;
using Domain.Enums.Floorball;
using Domain.Enums.Football;
using Domain.Enums.Hockey.Teams;
using Domain.ValueObjects.Floorball;
using Domain.ValueObjects.Football;

namespace DomainTestProject;

public class LoanPlayerTests
{
    [Fact]
    public void MarkAsLoanPlayer_FloorballPlayer_SetsFlagAndSequence()
    {
        FloorballPlayer player = new(Guid.NewGuid(), new Position(FloorballPosition.Forward));

        player.IsLoanPlayer.Should().BeFalse();
        player.LoanPlayerNumber.Should().Be(0);
        player.MarkAsLoanPlayer(2);
        player.IsLoanPlayer.Should().BeTrue();
        player.LoanPlayerNumber.Should().Be(2);
    }

    [Fact]
    public void MarkAsLoanPlayer_FootballPlayer_SetsFlagAndSequence()
    {
        FootballPlayer player = new(Guid.NewGuid(), new FootballPositionPreference(FootballPosition.Midfielder));

        player.MarkAsLoanPlayer(1);
        player.IsLoanPlayer.Should().BeTrue();
        player.LoanPlayerNumber.Should().Be(1);
    }

    [Fact]
    public void MarkAsLoanPlayer_HockeyPlayer_SetsFlagAndSequence()
    {
        HockeyPlayer player = new(Guid.NewGuid(), HockeyPosition.Center);

        player.MarkAsLoanPlayer(3);
        player.IsLoanPlayer.Should().BeTrue();
        player.LoanPlayerNumber.Should().Be(3);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void MarkAsLoanPlayer_SequenceBelowOne_Throws(int sequence)
    {
        FloorballPlayer player = new(Guid.NewGuid(), new Position(FloorballPosition.Forward));

        Action act = () => player.MarkAsLoanPlayer(sequence);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void DisplayName_IncludesSequence()
    {
        LoanPlayerNames.DisplayName(1).Should().Be("Lainapelaaja #1");
        LoanPlayerNames.LastName(2).Should().Be("#2");
    }

    [Fact]
    public void Select_SkipsUsedNumbersAndPrefersExisting()
    {
        LoanJerseyNumbers.Select(new[] { 1, 2 }, preferred: null).Should().Be(3);
        LoanJerseyNumbers.Select(new[] { 1, 4 }, preferred: 4).Should().Be(2);
        LoanJerseyNumbers.Select(new[] { 2 }, preferred: 8).Should().Be(8);
    }

    [Fact]
    public void Select_WhenEveryNumberIsUsed_ReturnsNull()
    {
        int[] used = Enumerable.Range(LoanJerseyNumbers.Minimum, LoanJerseyNumbers.Maximum).ToArray();
        LoanJerseyNumbers.Select(used, preferred: null).Should().BeNull();
        LoanJerseyNumbers.FreeCount(used).Should().Be(0);
    }

    [Fact]
    public void Resolve_KeepsCurrentCompetitionNumber()
    {
        Guid playerId = Guid.NewGuid();
        Guid competitionId = Guid.NewGuid();
        List<LoanJerseyNumbers.RosterJersey> roster = new()
        {
            new(playerId, null, 4),
            new(playerId, competitionId, 9),
            new(Guid.NewGuid(), competitionId, 1),
        };

        LoanJerseyNumbers.Resolve(roster, playerId, competitionId).Should().Be(9);
    }

    [Fact]
    public void Resolve_ReusesOwnNumberWhenFree_OtherwiseLowestFree()
    {
        Guid playerId = Guid.NewGuid();
        Guid competitionId = Guid.NewGuid();
        List<LoanJerseyNumbers.RosterJersey> reusable = new()
        {
            new(playerId, null, 7),
            new(Guid.NewGuid(), competitionId, 1),
        };
        LoanJerseyNumbers.Resolve(reusable, playerId, competitionId).Should().Be(7);

        List<LoanJerseyNumbers.RosterJersey> takenElsewhere = new()
        {
            new(playerId, null, 7),
            new(Guid.NewGuid(), Guid.NewGuid(), 7),
            new(Guid.NewGuid(), competitionId, 1),
        };
        LoanJerseyNumbers.Resolve(takenElsewhere, playerId, competitionId).Should().Be(2);
    }
}
