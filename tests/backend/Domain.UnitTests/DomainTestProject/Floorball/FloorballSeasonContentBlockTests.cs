using Domain.Entities.Floorball;

namespace DomainTestProject.Floorball;

public class FloorballSeasonContentBlockTests
{
    [Fact]
    public void Constructor_ValidInput_TrimsTitle()
    {
        FloorballSeasonContentBlock block = new(Guid.NewGuid(), "  Intro  ", " <p>Hi</p> ", 0);

        block.Title.Should().Be("Intro");
        block.ContentHtml.Should().Be("<p>Hi</p>");
        block.SortOrder.Should().Be(0);
    }

    [Fact]
    public void Constructor_EmptyTitle_Throws()
    {
        Action act = () => new FloorballSeasonContentBlock(Guid.NewGuid(), "  ", "<p>x</p>", 0);

        act.Should().Throw<ArgumentException>().WithParameterName("title");
    }

    [Fact]
    public void Constructor_NegativeSortOrder_Throws()
    {
        Action act = () => new FloorballSeasonContentBlock(Guid.NewGuid(), "Title", "<p>x</p>", -1);

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("sortOrder");
    }

    [Fact]
    public void ReplaceContentBlocks_AddsUpdatesAndRemovesInListOrder()
    {
        FloorballSeason season = new(
            "2026 Season",
            new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2027, 5, 1, 0, 0, 0, DateTimeKind.Utc));
        season.ReplaceContentBlocks([(null, "One", "<p>1</p>"), (null, "Two", "<p>2</p>")]);
        Guid keepId = season.ContentBlocks.First(block => block.Title == "One").Id;

        season.ReplaceContentBlocks([(keepId, "One updated", "<p>1b</p>"), (null, "Three", "<p>3</p>")]);

        season.ContentBlocks.Should().HaveCount(2);
        FloorballSeasonContentBlock first = season.ContentBlocks.Single(block => block.SortOrder == 0);
        FloorballSeasonContentBlock second = season.ContentBlocks.Single(block => block.SortOrder == 1);
        first.Id.Should().Be(keepId);
        first.Title.Should().Be("One updated");
        first.ContentHtml.Should().Be("<p>1b</p>");
        second.Title.Should().Be("Three");
        season.ContentBlocks.Should().NotContain(block => block.Title == "Two");
    }
}
