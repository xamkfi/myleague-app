using Application.Common;
using Application.DTOs.Common;
using Application.Features.Common.RulesSection.Commands;
using Application.Features.Common.RulesSection.Queries;
using Domain.Enums.Common;
using Domain.Repositories.Common;
using Moq;
using RulesSectionEntity = Domain.Entities.Common.RulesSection;

namespace ApplicationTestProject.Handlers.Common;

public class RulesSectionHandlerTests
{
    private readonly Mock<IRulesSectionRepository> _repo = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    [Fact]
    public async Task Create_GlobalTab_AddsAndSaves()
    {
        CreateRulesSectionCommandHandler handler = new(_repo.Object, _uow.Object);

        Result<RulesSectionDto> result = await handler.Handle(
            new CreateRulesSectionCommand("Yleiset", 1, RulesSectionType.Global, null, "admin"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.SectionType.Should().Be(RulesSectionType.Global);
        _repo.Verify(r => r.AddAsync(It.IsAny<RulesSectionEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_SportWithoutParent_ReturnsFailure()
    {
        CreateRulesSectionCommandHandler handler = new(_repo.Object, _uow.Object);

        Result<RulesSectionDto> result = await handler.Handle(
            new CreateRulesSectionCommand("Floorball", 1, RulesSectionType.Sport, null, null),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("SportGroup");
        _repo.Verify(r => r.AddAsync(It.IsAny<RulesSectionEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Create_DuplicateSportGroup_ReturnsFailure()
    {
        CreateRulesSectionCommandHandler handler = new(_repo.Object, _uow.Object);
        _repo.Setup(r => r.ExistsBySectionTypeAsync(RulesSectionType.SportGroup, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        Result<RulesSectionDto> result = await handler.Handle(
            new CreateRulesSectionCommand("Lajikohtaiset", 2, RulesSectionType.SportGroup, null, null),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("already exists");
    }

    [Fact]
    public async Task GetById_WhenMissing_ReturnsNotFound()
    {
        GetRulesSectionByIdQueryHandler handler = new(_repo.Object);
        Guid id = Guid.NewGuid();
        _repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RulesSectionEntity?)null);

        Result<RulesSectionDto> result = await handler.Handle(
            new GetRulesSectionByIdQuery(id),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("RulesSection");
    }

    [Fact]
    public async Task AddRule_AppendsHtmlAndSaves()
    {
        AddRulesSectionRuleCommandHandler handler = new(_repo.Object, _uow.Object);
        RulesSectionEntity section = new(Guid.NewGuid(), "Fees", 1, RulesSectionType.Fee);
        _repo.Setup(r => r.GetByIdAsync(section.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(section);

        string ruleHtml = """<div class="rules-item" data-rule-id="rule-1"><p>Pay fee</p></div>""";

        Result<RulesSectionDto> result = await handler.Handle(
            new AddRulesSectionRuleCommand(section.Id, ruleHtml, "admin"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        section.ContentHtml.Should().Contain("data-rule-id=\"rule-1\"");
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateRule_WhenRuleMissing_ReturnsNotFound()
    {
        UpdateRulesSectionRuleCommandHandler handler = new(_repo.Object, _uow.Object);
        RulesSectionEntity section = new(Guid.NewGuid(), "Fees", 1, RulesSectionType.Fee);
        _repo.Setup(r => r.GetByIdAsync(section.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(section);

        Result<RulesSectionDto> result = await handler.Handle(
            new UpdateRulesSectionRuleCommand(
                section.Id,
                "missing",
                """<div class="rules-item" data-rule-id="missing">x</div>""",
                null),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("RulesSectionRule");
    }

    [Fact]
    public async Task Delete_WhenHasChildren_ReturnsFailure()
    {
        DeleteRulesSectionCommandHandler handler = new(_repo.Object, _uow.Object);
        RulesSectionEntity section = new(Guid.NewGuid(), "Group", 1, RulesSectionType.SportGroup);
        _repo.Setup(r => r.GetByIdAsync(section.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(section);
        _repo.Setup(r => r.HasChildSectionsAsync(section.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        Result<bool> result = await handler.Handle(
            new DeleteRulesSectionCommand(section.Id),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("child");
        _repo.Verify(r => r.RemoveAsync(It.IsAny<RulesSectionEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Delete_WhenEmpty_RemovesAndSaves()
    {
        DeleteRulesSectionCommandHandler handler = new(_repo.Object, _uow.Object);
        RulesSectionEntity section = new(Guid.NewGuid(), "Fees", 1, RulesSectionType.Fee);
        _repo.Setup(r => r.GetByIdAsync(section.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(section);
        _repo.Setup(r => r.HasChildSectionsAsync(section.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        Result<bool> result = await handler.Handle(
            new DeleteRulesSectionCommand(section.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repo.Verify(r => r.RemoveAsync(section, It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
