using Application.Features.Common.Deletion;
using Domain.Entities.Common;
using Domain.Entities.Floorball;
using Domain.Enums.Floorball;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using Domain.Repositories.Football;
using Domain.Repositories.Hockey;
using Domain.ValueObjects.Floorball;
using Moq;

namespace ApplicationTestProject.Handlers.Deletion;

public class PersonDeletionGuardTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IClubManagerRepository> _clubManagerRepository = new();
    private readonly Mock<IFloorballPlayerRepository> _floorballPlayerRepository = new();
    private readonly Mock<IFootballPlayerRepository> _footballPlayerRepository = new();
    private readonly Mock<IHockeyPlayerRepository> _hockeyPlayerRepository = new();
    private readonly Mock<IFloorballRefereeRepository> _floorballRefereeRepository = new();
    private readonly Mock<IFootballRefereeRepository> _footballRefereeRepository = new();
    private readonly Mock<IHockeyOfficialRepository> _hockeyOfficialRepository = new();
    private readonly Mock<IFloorballTeamManagerRepository> _floorballTeamManagerRepository = new();
    private readonly Mock<IFootballTeamManagerRepository> _footballTeamManagerRepository = new();
    private readonly PersonDeletionGuard _guard;

    public PersonDeletionGuardTests()
    {
        _userRepository.Setup(x => x.ExistsByPersonIdAsync(It.IsAny<Guid>())).ReturnsAsync(false);
        _clubManagerRepository.Setup(x => x.GetAllByPersonIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(Enumerable.Empty<ClubManager>());
        _floorballTeamManagerRepository.Setup(x => x.GetAllByPersonIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(Enumerable.Empty<Domain.Entities.Floorball.FloorballTeamManager>());
        _footballTeamManagerRepository.Setup(x => x.GetAllByPersonIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(Enumerable.Empty<Domain.Entities.Football.Teams.FootballTeamManager>());

        _guard = new PersonDeletionGuard(
            _userRepository.Object,
            _clubManagerRepository.Object,
            _floorballPlayerRepository.Object,
            _footballPlayerRepository.Object,
            _hockeyPlayerRepository.Object,
            _floorballRefereeRepository.Object,
            _footballRefereeRepository.Object,
            _hockeyOfficialRepository.Object,
            _floorballTeamManagerRepository.Object,
            _footballTeamManagerRepository.Object);
    }

    [Fact]
    public async Task EvaluateAsync_PersonHasUserAccount_Blocks()
    {
        Guid personId = Guid.NewGuid();
        _userRepository.Setup(x => x.ExistsByPersonIdAsync(personId)).ReturnsAsync(true);

        PersonDeletionEvaluation evaluation = await _guard.EvaluateAsync(personId, CancellationToken.None);

        evaluation.IsBlocked.Should().BeTrue();
        evaluation.BlockReason.Should().Be(DeletionReasons.PersonHasUserAccount);
    }

    [Fact]
    public async Task EvaluateAsync_PersonIsClubManager_Blocks()
    {
        Guid personId = Guid.NewGuid();
        _clubManagerRepository.Setup(x => x.GetAllByPersonIdAsync(personId))
            .ReturnsAsync(new[] { new ClubManager(personId, Guid.NewGuid()) });

        PersonDeletionEvaluation evaluation = await _guard.EvaluateAsync(personId, CancellationToken.None);

        evaluation.IsBlocked.Should().BeTrue();
        evaluation.BlockReason.Should().Be(DeletionReasons.PersonIsClubManager);
    }

    [Fact]
    public async Task EvaluateAsync_PlayerHasHistory_Blocks()
    {
        Guid personId = Guid.NewGuid();
        FloorballPlayer player = new FloorballPlayer(personId, new Position(FloorballPosition.Forward));
        _floorballPlayerRepository.Setup(x => x.GetByPersonIdAsync(personId)).ReturnsAsync(player);
        _floorballPlayerRepository
            .Setup(x => x.HasCompetitionHistoryAsync(player.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        PersonDeletionEvaluation evaluation = await _guard.EvaluateAsync(personId, CancellationToken.None);

        evaluation.IsBlocked.Should().BeTrue();
        evaluation.BlockReason.Should().Be(DeletionReasons.PersonHasMatchRecords);
    }

    [Fact]
    public async Task EvaluateAsync_RefereeAssignedToMatch_Blocks()
    {
        Guid personId = Guid.NewGuid();
        FloorballReferee referee = new FloorballReferee(
            personId,
            DateTime.UtcNow.AddYears(-1),
            DateTime.UtcNow.AddYears(1));
        _floorballRefereeRepository.Setup(x => x.GetByPersonIdAsync(personId)).ReturnsAsync(referee);
        _floorballRefereeRepository
            .Setup(x => x.IsAssignedToAnyMatchAsync(referee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        PersonDeletionEvaluation evaluation = await _guard.EvaluateAsync(personId, CancellationToken.None);

        evaluation.IsBlocked.Should().BeTrue();
        evaluation.BlockReason.Should().Be(DeletionReasons.PersonIsAssignedOfficial);
    }

    [Fact]
    public async Task EvaluateAsync_UnusedPerson_Allows()
    {
        Guid personId = Guid.NewGuid();

        PersonDeletionEvaluation evaluation = await _guard.EvaluateAsync(personId, CancellationToken.None);

        evaluation.IsBlocked.Should().BeFalse();
        evaluation.UnusedFloorballPlayerId.Should().BeNull();
        evaluation.UnusedFootballPlayerId.Should().BeNull();
        evaluation.UnusedHockeyPlayerId.Should().BeNull();
    }

    [Fact]
    public async Task EvaluateAsync_UnusedPlayerProfile_AllowsCascade()
    {
        Guid personId = Guid.NewGuid();
        FloorballPlayer player = new FloorballPlayer(personId, new Position(FloorballPosition.Forward));
        _floorballPlayerRepository.Setup(x => x.GetByPersonIdAsync(personId)).ReturnsAsync(player);
        _floorballPlayerRepository
            .Setup(x => x.HasCompetitionHistoryAsync(player.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        PersonDeletionEvaluation evaluation = await _guard.EvaluateAsync(personId, CancellationToken.None);

        evaluation.IsBlocked.Should().BeFalse();
        evaluation.UnusedFloorballPlayerId.Should().Be(player.Id);
    }
}
