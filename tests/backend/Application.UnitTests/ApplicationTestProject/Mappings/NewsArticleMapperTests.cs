using Application.Features.Common.News.Commands;
using Application.Features.Common.News.Mappings;
using Domain.Entities.Common;

namespace ApplicationTestProject.Mappings;

public class NewsArticleMapperTests
{
    [Fact]
    public void UpdateFromCommand_WithTags_ReplacesTagsOnArticle()
    {
        NewsArticle article = new(Guid.NewGuid(), "Title", null, "<p>content</p>", "Author");
        article.AddTag("old");

        UpdateNewsArticleCommand command = new(
            article.Id,
            "Title",
            null,
            "<p>content</p>",
            null,
            null,
            "Author",
            null,
            null,
            new[] { "kissa", "koira" });

        NewsArticleMapper.UpdateFromCommand(article, command);

        article.Tags.Should().Equal("kissa", "koira");
    }

    [Fact]
    public void UpdateFromCommand_WithEmptyTags_ClearsTags()
    {
        NewsArticle article = new(Guid.NewGuid(), "Title", null, "<p>content</p>", "Author");
        article.AddTag("kissa");

        UpdateNewsArticleCommand command = new(
            article.Id,
            "Title",
            null,
            "<p>content</p>",
            null,
            null,
            "Author",
            null,
            null,
            Array.Empty<string>());

        NewsArticleMapper.UpdateFromCommand(article, command);

        article.Tags.Should().BeEmpty();
    }

    [Fact]
    public void UpdateFromCommand_WithNullTags_LeavesExistingTags()
    {
        NewsArticle article = new(Guid.NewGuid(), "Title", null, "<p>content</p>", "Author");
        article.AddTag("kissa");

        UpdateNewsArticleCommand command = new(
            article.Id,
            "Title",
            null,
            "<p>content</p>",
            null,
            null,
            "Author");

        NewsArticleMapper.UpdateFromCommand(article, command);

        article.Tags.Should().Equal("kissa");
    }

    [Fact]
    public void ToEntity_AddsTagsFromCreateCommand()
    {
        CreateNewsArticleCommand command = new(
            "Title",
            null,
            "<p>content</p>",
            null,
            null,
            "Author",
            null,
            null,
            new[] { "kissa", "koira" });

        NewsArticle article = NewsArticleMapper.ToEntity(command);

        article.Tags.Should().Equal("kissa", "koira");
    }
}
