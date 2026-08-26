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
