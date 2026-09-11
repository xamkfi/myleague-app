using Domain.Entities.Common;

namespace DomainTestProject.Common;

public class NewsArticleTagTests
{
    [Fact]
    public void AddTag_SameTagOnDifferentArticles_IsAllowed()
    {
        NewsArticle first = CreateArticle();
        NewsArticle second = CreateArticle();

        first.AddTag("kissa");
        second.AddTag("kissa");

        first.Tags.Should().Contain("kissa");
        second.Tags.Should().Contain("kissa");
    }

    [Fact]
    public void AddTag_DuplicateOnSameArticle_IsIgnored()
    {
        NewsArticle article = CreateArticle();

        article.AddTag("kissa");
        article.AddTag("Kissa");
        article.AddTag("#kissa");

        article.Tags.Should().Equal("kissa");
    }

    [Fact]
    public void ReplaceTags_ReplacesEntireSet()
    {
        NewsArticle article = CreateArticle();
        article.AddTag("old");

        article.ReplaceTags(new[] { "kissa", "koira" });

        article.Tags.Should().Equal("kissa", "koira");
    }

    [Fact]
    public void ReplaceTags_EmptyList_ClearsTags()
    {
        NewsArticle article = CreateArticle();
        article.AddTag("kissa");

        article.ReplaceTags(Array.Empty<string>());

        article.Tags.Should().BeEmpty();
    }

    private static NewsArticle CreateArticle()
    {
        return new NewsArticle(Guid.NewGuid(), "Title", null, "<p>content</p>", "Author");
    }
}
