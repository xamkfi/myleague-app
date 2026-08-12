using Application.Common;
using Application.Features.Hockey.Officials.Commands;
using Application.Features.Hockey.Officials.DTOs;
using Application.Features.Hockey.Officials.Handlers;
using Application.Features.Hockey.Officials.Queries;
using Domain.Entities.Common;
using Domain.Entities.Hockey.Teams;
using Domain.Enums.Hockey.Teams;
using Domain.Repositories.Common;
using Domain.Repositories.Hockey;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Hockey;

public class HockeyOfficialHandlerTests
{
    private readonly Mock<IHockeyOfficialRepository> _officialRepo = new();
    private readonly Mock<IPersonRepository> _personRepo = new();
    private readonly Mock<IHockeyUnitOfWork> _unitOfWork = new();

    [Fact]
    public async Task Create_PersonNotFound_ReturnsNotFound()
    {
        Guid personId = Guid.NewGuid();
        _personRepo.Setup(r => r.GetByIdAsync(personId)).ReturnsAsync((Person?)null);

        CreateHockeyOfficialHandler handler = new(
            _officialRepo.Object,
            _personRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<CreateHockeyOfficialHandler>>());

        Result<HockeyOfficialDto> result = await handler.Handle(
            new CreateHockeyOfficialCommand(personId, HockeyOfficialRole.Referee),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task Create_PersonAlreadyOfficial_ReturnsFailure()
    {
        Person person = new("Test", "Official");
        _personRepo.Setup(r => r.GetByIdAsync(person.Id)).ReturnsAsync(person);
        _officialRepo.Setup(r => r.GetByPersonIdAsync(person.Id))
            .ReturnsAsync(new HockeyOfficial(person.Id, HockeyOfficialRole.Linesperson));

        CreateHockeyOfficialHandler handler = new(
            _officialRepo.Object,
            _personRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<CreateHockeyOfficialHandler>>());

        Result<HockeyOfficialDto> result = await handler.Handle(
            new CreateHockeyOfficialCommand(person.Id, HockeyOfficialRole.Referee),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("already");
        _officialRepo.Verify(r => r.AddAsync(It.IsAny<HockeyOfficial>()), Times.Never);
    }

    [Fact]
    public async Task Create_ValidPerson_AddsAndSaves()
    {
        Person person = new("Test", "Official");
        _personRepo.Setup(r => r.GetByIdAsync(person.Id)).ReturnsAsync(person);
        _officialRepo.Setup(r => r.GetByPersonIdAsync(person.Id)).ReturnsAsync((HockeyOfficial?)null);

        CreateHockeyOfficialHandler handler = new(
            _officialRepo.Object,
            _personRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<CreateHockeyOfficialHandler>>());

        Result<HockeyOfficialDto> result = await handler.Handle(
            new CreateHockeyOfficialCommand(
                person.Id,
                HockeyOfficialRole.Referee,
                OfficialNumber: "42",
                LicenseIssueDate: new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                LicenseExpiryDate: new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.PersonId.Should().Be(person.Id);
        result.Data.OfficialRole.Should().Be(HockeyOfficialRole.Referee.ToString());
        result.Data.OfficialNumber.Should().Be("42");
        result.Data.IsActive.Should().BeTrue();
        _officialRepo.Verify(r => r.AddAsync(It.IsAny<HockeyOfficial>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_NotFound_ReturnsNotFound()
    {
        Guid officialId = Guid.NewGuid();
        _officialRepo.Setup(r => r.GetByIdAsync(officialId)).ReturnsAsync((HockeyOfficial?)null);

        UpdateHockeyOfficialHandler handler = new(
            _officialRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<UpdateHockeyOfficialHandler>>());

        Result<HockeyOfficialDto> result = await handler.Handle(
            new UpdateHockeyOfficialCommand(
                officialId,
                HockeyOfficialRole.Linesperson,
                null,
                null,
                null,
                IsActive: false),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task Update_Valid_UpdatesAndSaves()
    {
        HockeyOfficial official = new(Guid.NewGuid(), HockeyOfficialRole.Referee, "10");
        _officialRepo.Setup(r => r.GetByIdAsync(official.Id)).ReturnsAsync(official);

        UpdateHockeyOfficialHandler handler = new(
            _officialRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<UpdateHockeyOfficialHandler>>());

        Result<HockeyOfficialDto> result = await handler.Handle(
            new UpdateHockeyOfficialCommand(
                official.Id,
                HockeyOfficialRole.Linesperson,
                "99",
                new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2028, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsActive: false),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.OfficialRole.Should().Be(HockeyOfficialRole.Linesperson.ToString());
        result.Data.OfficialNumber.Should().Be("99");
        result.Data.IsActive.Should().BeFalse();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetById_Found_ReturnsDto()
    {
        HockeyOfficial official = new(Guid.NewGuid(), HockeyOfficialRole.Referee);
        _officialRepo.Setup(r => r.GetByIdAsync(official.Id)).ReturnsAsync(official);

        GetHockeyOfficialByIdHandler handler = new(
            _officialRepo.Object,
            Mock.Of<ILogger<GetHockeyOfficialByIdHandler>>());

        Result<HockeyOfficialDto> result = await handler.Handle(
            new GetHockeyOfficialByIdQuery(official.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Id.Should().Be(official.Id);
    }

    [Fact]
    public async Task GetAll_ReturnsMappedList()
    {
        HockeyOfficial a = new(Guid.NewGuid(), HockeyOfficialRole.Referee);
        HockeyOfficial b = new(Guid.NewGuid(), HockeyOfficialRole.Linesperson);
        _officialRepo.Setup(r => r.GetAllAsync(true)).ReturnsAsync(new List<HockeyOfficial> { a, b });

        GetHockeyOfficialsHandler handler = new(
            _officialRepo.Object,
            Mock.Of<ILogger<GetHockeyOfficialsHandler>>());

        Result<IReadOnlyList<HockeyOfficialDto>> result = await handler.Handle(
            new GetHockeyOfficialsQuery(IsActive: true),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Should().HaveCount(2);
    }
}
