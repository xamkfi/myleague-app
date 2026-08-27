using Domain.Entities.Common;
using Domain.Enums.Common;

namespace DomainTestProject.Common;

public class InfoPageContentTests
{
    [Fact]
    public void Constructor_ValidInput_SetsProperties()
    {
        InfoPageContent page = new(
            Guid.NewGuid(),
            " about-us ",
            " About ",
            " <p>Hello</p> ",
            "admin");

        page.PageSlug.Should().Be("about-us");
        page.Title.Should().Be("About");
        page.ContentHtml.Should().Be("<p>Hello</p>");
        page.LastModifiedBy.Should().Be("admin");
    }

    [Fact]
    public void Constructor_EmptySlug_Throws()
    {
        Action act = () => new InfoPageContent(Guid.NewGuid(), "  ", "Title", "<p>x</p>");

        act.Should().Throw<ArgumentException>().WithParameterName("pageSlug");
    }

    [Fact]
    public void UpdateContent_ReplacesTitleAndHtml()
    {
        InfoPageContent page = new(Guid.NewGuid(), "home", "Old", "<p>old</p>");

        page.UpdateContent("New", "<p>new</p>", "editor");

        page.Title.Should().Be("New");
        page.ContentHtml.Should().Be("<p>new</p>");
        page.LastModifiedBy.Should().Be("editor");
    }
}

public class RulesSectionTests
{
    [Fact]
    public void Constructor_ValidInput_SetsProperties()
    {
        RulesSection section = new(
            Guid.NewGuid(),
            " Global ",
            1,
            RulesSectionType.Global);

        section.Title.Should().Be("Global");
        section.SortOrder.Should().Be(1);
        section.SectionType.Should().Be(RulesSectionType.Global);
        section.ContentHtml.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_EmptyTitle_Throws()
    {
        Action act = () => new RulesSection(Guid.NewGuid(), " ", 0, RulesSectionType.Fee);

        act.Should().Throw<ArgumentException>().WithParameterName("title");
    }

    [Fact]
    public void UpdateContentHtml_SetsHtmlAndModifier()
    {
        RulesSection section = new(Guid.NewGuid(), "Fees", 2, RulesSectionType.Fee);

        section.UpdateContentHtml("<div class=\"rules-item\" data-rule-id=\"r1\">Rule</div>", "admin");

        section.ContentHtml.Should().Contain("data-rule-id=\"r1\"");
        section.LastModifiedBy.Should().Be("admin");
    }
}

public class SeasonContentBlockTests
{
    [Fact]
    public void Constructor_ValidInput_SetsProperties()
    {
        Guid competitionId = Guid.NewGuid();
        Domain.Entities.Common.SeasonContentBlock block = new(
            Guid.NewGuid(),
            SportsCategory.Floorball,
            competitionId,
            " 2025-2026 ",
            " Sarjainfo ",
            "<p>Hello</p>",
            1,
            "admin");

        block.Sport.Should().Be(SportsCategory.Floorball);
        block.CompetitionId.Should().Be(competitionId);
        block.SeasonYear.Should().Be("2025-2026");
        block.Title.Should().Be("Sarjainfo");
        block.ContentHtml.Should().Be("<p>Hello</p>");
        block.SortOrder.Should().Be(1);
        block.LastModifiedBy.Should().Be("admin");
    }

    [Fact]
    public void Constructor_EmptyTitle_Throws()
    {
        Action act = () => new Domain.Entities.Common.SeasonContentBlock(
            Guid.NewGuid(),
            SportsCategory.Football,
            Guid.NewGuid(),
            "2026",
            " ",
            "<p>x</p>",
            0);

        act.Should().Throw<ArgumentException>().WithParameterName("title");
    }

    [Fact]
    public void Constructor_NoneSport_Throws()
    {
        Action act = () => new Domain.Entities.Common.SeasonContentBlock(
            Guid.NewGuid(),
            SportsCategory.None,
            Guid.NewGuid(),
            "2026",
            "Title",
            "<p>x</p>",
            0);

        act.Should().Throw<ArgumentException>().WithParameterName("sport");
    }

    [Fact]
    public void UpdateContent_ReplacesTitleHtmlAndSortOrder()
    {
        Domain.Entities.Common.SeasonContentBlock block = new(
            Guid.NewGuid(),
            SportsCategory.Icehockey,
            Guid.NewGuid(),
            "2026-2027",
            "Old",
            "<p>old</p>",
            0);

        block.UpdateContent("New", "<p>new</p>", 3, "editor");

        block.Title.Should().Be("New");
        block.ContentHtml.Should().Be("<p>new</p>");
        block.SortOrder.Should().Be(3);
        block.LastModifiedBy.Should().Be("editor");
    }

    [Fact]
    public void SetSortOrder_UpdatesOrder()
    {
        Domain.Entities.Common.SeasonContentBlock block = new(
            Guid.NewGuid(),
            SportsCategory.Floorball,
            Guid.NewGuid(),
            "2027",
            "Intro",
            "<p>x</p>",
            4);

        block.SetSortOrder(0, "admin");

        block.SortOrder.Should().Be(0);
        block.LastModifiedBy.Should().Be("admin");
    }
}
