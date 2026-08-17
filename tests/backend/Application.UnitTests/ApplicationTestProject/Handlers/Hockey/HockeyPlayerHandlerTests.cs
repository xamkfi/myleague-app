using Application.Common;
using Application.Features.Hockey.Players.Commands;
using Application.Features.Hockey.Players.DTOs;
using Application.Features.Hockey.Players.Handlers;
using Domain.Entities.Common;
using Domain.Entities.Hockey.Teams;
using Domain.Enums.Hockey.Teams;
using Domain.Repositories.Common;
using Domain.Repositories.Hockey;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Hockey;

public class HockeyPlayerHandlerTests
{
    private readonly Mock<IHockeyPlayerRepository> _playerRepo = new();
    private readonly Mock<IPersonRepository> _personRepo = new();
    private readonly Mock<IHockeyUnitOfWork> _unitOfWork = new();

    [Fact]
    public async Task Create_PersonNotFound_ReturnsNotFound()
    {
        Guid personId = Guid.NewGuid();
        _personRepo.Setup(r => r.GetByIdAsync(personId)).ReturnsAsync((Person?)null);

        CreateHockeyPlayerHandler handler = new(
            _playerRepo.Object,
            _personRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<CreateHockeyPlayerHandler>>());

        Result<HockeyPlayerDto> result = await handler.Handle(
            new CreateHockeyPlayerCommand(personId, HockeyPosition.Center),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task Create_ValidPerson_AddsAndSaves()
    {
        Person person = new("Test", "Player");
        _personRepo.Setup(r => r.GetByIdAsync(person.Id)).ReturnsAsync(person);

        CreateHockeyPlayerHandler handler = new(
            _playerRepo.Object,
            _personRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<CreateHockeyPlayerHandler>>());

        Result<HockeyPlayerDto> result = await handler.Handle(
            new CreateHockeyPlayerCommand(person.Id, HockeyPosition.Center, HockeyShoots.Left),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.PersonId.Should().Be(person.Id);
        result.Data.PrimaryPosition.Should().Be(HockeyPosition.Center.ToString());
        _playerRepo.Verify(r => r.AddAsync(It.IsAny<HockeyPlayer>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
