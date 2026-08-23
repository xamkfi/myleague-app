using Application.Common;
using Application.Features.Hockey.Teams.Commands;
using Application.Features.Hockey.Teams.DTOs;
using Application.Features.Hockey.Teams.Handlers;
using Domain.Entities.Common;
using Domain.Entities.Hockey.Teams;
using Domain.Enums.Common;
using Domain.Enums.Hockey.Teams;
using Domain.Repositories.Common;
using Domain.Repositories.Hockey;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Hockey;

public class HockeyTeamHandlerTests
{
    private readonly Mock<IHockeyTeamRepository> _teamRepo = new();
    private readonly Mock<IHockeyPlayerRepository> _playerRepo = new();
    private readonly Mock<IClubRepository> _clubRepo = new();
    private readonly Mock<IPersonRepository> _personRepo = new();
    private readonly Mock<IHockeyUnitOfWork> _unitOfWork = new();

    private static Club CreateClub() => new("Test HC");

    private static HockeyTeam CreateTeam(Club? club = null)
    {
        Club owningClub = club ?? CreateClub();
        return new HockeyTeam("Wolves", owningClub, TeamCategory.Adult, shortName: "WOL");
    }

    [Fact]
    public async Task Create_ClubNotFound_ReturnsNotFound()
    {
        Guid clubId = Guid.NewGuid();
        _clubRepo.Setup(r => r.GetByIdAsync(clubId)).ReturnsAsync((Club?)null);

        CreateHockeyTeamHandler handler = new(
            _teamRepo.Object,
            _clubRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<CreateHockeyTeamHandler>>());

        Result<HockeyTeamDto> result = await handler.Handle(
            new CreateHockeyTeamCommand("Wolves", clubId, TeamCategory.Adult),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Create_ValidClub_AddsAndSaves()
    {
        Club club = CreateClub();
        _clubRepo.Setup(r => r.GetByIdAsync(club.Id)).ReturnsAsync(club);

        CreateHockeyTeamHandler handler = new(
            _teamRepo.Object,
            _clubRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<CreateHockeyTeamHandler>>());

        Result<HockeyTeamDto> result = await handler.Handle(
            new CreateHockeyTeamCommand("Wolves", club.Id, TeamCategory.Adult, ShortName: "WOL"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Name.Should().Be("Wolves");
        result.Data.ShortName.Should().Be("WOL");
        result.Data.Roster.Should().BeEmpty();
        _teamRepo.Verify(r => r.AddAsync(It.IsAny<HockeyTeam>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_ExistingTeam_UpdatesName()
    {
        HockeyTeam team = CreateTeam();
        _teamRepo.Setup(r => r.GetByIdAsync(team.Id)).ReturnsAsync(team);

        UpdateHockeyTeamHandler handler = new(
            _teamRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<UpdateHockeyTeamHandler>>());

        Result<HockeyTeamDto> result = await handler.Handle(
            new UpdateHockeyTeamCommand(team.Id, "Updated Wolves", "UW", TeamCategory.Adult, null, "Arena", "Blue", "White"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Name.Should().Be("Updated Wolves");
        result.Data.ShortName.Should().Be("UW");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddPlayer_ValidPlayer_AddsToRoster()
    {
        HockeyTeam team = CreateTeam();
        HockeyPlayer player = new(Guid.NewGuid(), HockeyPosition.Center);
        _teamRepo.Setup(r => r.GetByIdAsync(team.Id)).ReturnsAsync(team);
        _playerRepo.Setup(r => r.GetByIdAsync(player.Id)).ReturnsAsync(player);

        AddPlayerToHockeyTeamHandler handler = new(
            _teamRepo.Object,
            _playerRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<AddPlayerToHockeyTeamHandler>>());

        Result<HockeyTeamDto> result = await handler.Handle(
            new AddPlayerToHockeyTeamCommand(team.Id, player.Id, HockeyPosition.Center, JerseyNumber: 13),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Roster.Should().ContainSingle(p => p.PlayerId == player.Id && p.JerseyNumber == 13);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddPlayer_PlayerNotFound_ReturnsNotFound()
    {
        HockeyTeam team = CreateTeam();
        Guid missingPlayerId = Guid.NewGuid();
        _teamRepo.Setup(r => r.GetByIdAsync(team.Id)).ReturnsAsync(team);
        _playerRepo.Setup(r => r.GetByIdAsync(missingPlayerId)).ReturnsAsync((HockeyPlayer?)null);

        AddPlayerToHockeyTeamHandler handler = new(
            _teamRepo.Object,
            _playerRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<AddPlayerToHockeyTeamHandler>>());

        Result<HockeyTeamDto> result = await handler.Handle(
            new AddPlayerToHockeyTeamCommand(team.Id, missingPlayerId, HockeyPosition.Center),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddLine_ValidRequest_AddsLine()
    {
        HockeyTeam team = CreateTeam();
        _teamRepo.Setup(r => r.GetByIdAsync(team.Id)).ReturnsAsync(team);

        AddHockeyLineHandler handler = new(
            _teamRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<AddHockeyLineHandler>>());

        Result<HockeyTeamDto> result = await handler.Handle(
            new AddHockeyLineCommand(team.Id, "PP1", 1, HockeyLineType.PowerPlayUnit),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Lines.Should().ContainSingle(l => l.Name == "PP1" && l.IsActive);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddStaff_ValidPerson_AddsStaff()
    {
        HockeyTeam team = CreateTeam();
        Person person = new("Head", "Coach");
        _teamRepo.Setup(r => r.GetByIdAsync(team.Id)).ReturnsAsync(team);
        _personRepo.Setup(r => r.GetByIdAsync(person.Id)).ReturnsAsync(person);

        AddHockeyTeamStaffHandler handler = new(
            _teamRepo.Object,
            _personRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<AddHockeyTeamStaffHandler>>());

        Result<HockeyTeamDto> result = await handler.Handle(
            new AddHockeyTeamStaffCommand(team.Id, person.Id, HockeyTeamStaffRole.HeadCoach),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.StaffMembers.Should().ContainSingle(s => s.PersonId == person.Id && s.IsActive);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateJerseyNumber_ExistingPlayer_UpdatesOnlyJersey()
    {
        HockeyTeam team = CreateTeam();
        HockeyPlayer player = new(Guid.NewGuid(), HockeyPosition.Center);
        team.AddPlayer(player, HockeyPosition.Center, jerseyNumber: 13);
        _teamRepo.Setup(r => r.GetByIdAsync(team.Id)).ReturnsAsync(team);

        UpdateHockeyTeamPlayerJerseyNumberHandler handler = new(
            _teamRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<UpdateHockeyTeamPlayerJerseyNumberHandler>>());

        Result<HockeyTeamPlayerDto> result = await handler.Handle(
            new UpdateHockeyTeamPlayerJerseyNumberCommand(team.Id, player.Id, 99),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.JerseyNumber.Should().Be(99);
        result.Data.Position.Should().Be(HockeyPosition.Center.ToString());
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateJerseyNumber_PlayerNotOnRoster_ReturnsFailure()
    {
        HockeyTeam team = CreateTeam();
        _teamRepo.Setup(r => r.GetByIdAsync(team.Id)).ReturnsAsync(team);

        UpdateHockeyTeamPlayerJerseyNumberHandler handler = new(
            _teamRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<UpdateHockeyTeamPlayerJerseyNumberHandler>>());

        Result<HockeyTeamPlayerDto> result = await handler.Handle(
            new UpdateHockeyTeamPlayerJerseyNumberCommand(team.Id, Guid.NewGuid(), 7),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not in the team roster");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
