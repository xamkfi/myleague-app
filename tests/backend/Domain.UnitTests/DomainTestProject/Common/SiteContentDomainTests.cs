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

public class FooterContactTests
{
    [Fact]
    public void Constructor_ValidInput_SetsProperties()
    {
        FooterContact contact = new(
            Guid.NewGuid(),
            " Office ",
            " Street 1 ",
            " office@mahl.fi ",
            " 044 123 ",
            " https://mahl.fi ",
            2,
            "admin");

        contact.Title.Should().Be("Office");
        contact.Details.Should().Be("Street 1");
        contact.Email.Should().Be("office@mahl.fi");
        contact.Phone.Should().Be("044 123");
        contact.Url.Should().Be("https://mahl.fi");
        contact.SortOrder.Should().Be(2);
    }

    [Fact]
    public void Constructor_EmptyTitle_Throws()
    {
        Action act = () => new FooterContact(Guid.NewGuid(), " ", null, null, null, null, 0);

        act.Should().Throw<ArgumentException>().WithParameterName("title");
    }

    [Fact]
    public void Constructor_InvalidEmail_Throws()
    {
        Action act = () => new FooterContact(Guid.NewGuid(), "Office", null, "not-an-email", null, null, 0);

        act.Should().Throw<ArgumentException>().WithParameterName("email");
    }

    [Fact]
    public void Constructor_InvalidUrl_Throws()
    {
        Action act = () => new FooterContact(Guid.NewGuid(), "Office", null, null, null, "ftp://mahl.fi", 0);

        act.Should().Throw<ArgumentException>().WithParameterName("url");
    }

    [Fact]
    public void Update_ReplacesFields()
    {
        FooterContact contact = new(Guid.NewGuid(), "Old", null, null, null, null, 0);

        contact.Update("New", "Details", "a@b.fi", "123", "https://example.com", 5, "editor");

        contact.Title.Should().Be("New");
        contact.Details.Should().Be("Details");
        contact.Email.Should().Be("a@b.fi");
        contact.SortOrder.Should().Be(5);
        contact.LastModifiedBy.Should().Be("editor");
    }
}
