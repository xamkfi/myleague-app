using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.Persistence.Repositories.Common;
using Domain.Entities.Common;
using Domain.Enums.Common;
using InfrastructureIntegrationTestProject.Common;

namespace Tests.Backend.Infrastructure.IntegrationTests.InfrastructureIntegrationTestProject.Repositories
{
    public class NewsArticleRepositoryTests : BaseIntegrationTest
    {
        private NewsArticleRepository CreateRepository()
        {
            ILogger<NewsArticleRepository> logger = _serviceProvider.GetRequiredService<ILogger<NewsArticleRepository>>();
            return new NewsArticleRepository(_dbContext, logger);
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsNewsArticle()
        {
            // Arrange
            NewsArticleRepository repository = CreateRepository();
            Guid newsId = Guid.NewGuid();
            NewsArticle newsArticle = new NewsArticle(newsId, "Test News", "<p>Test content</p>", "Test Author");
            await repository.CreateNews(newsArticle);
            await _dbContext.SaveChangesAsync();

            // Act
            NewsArticle? result = await repository.GetByIdAsync(newsId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(newsId, result.Id);
            Assert.Equal("Test News", result.Title);
            Assert.Equal("<p>Test content</p>", result.ContentHtml);
            Assert.Equal("Test Author", result.Author);
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            NewsArticleRepository repository = CreateRepository();
            Guid newsId = Guid.NewGuid();

            // Act
            NewsArticle? result = await repository.GetByIdAsync(newsId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByAuthorAsync_WithValidAuthor_ReturnsNewsArticles()
        {
            // Arrange
            NewsArticleRepository repository = CreateRepository();
            string author = "Test Author";
            NewsArticle newsArticle1 = new NewsArticle(Guid.NewGuid(), "News 1", "<p>Content 1</p>", author);
            NewsArticle newsArticle2 = new NewsArticle(Guid.NewGuid(), "News 2", "<p>Content 2</p>", author);
            NewsArticle newsArticle3 = new NewsArticle(Guid.NewGuid(), "News 3", "<p>Content 3</p>", "Other Author");

            await repository.CreateNews(newsArticle1);
            await repository.CreateNews(newsArticle2);
            await repository.CreateNews(newsArticle3);
            await _dbContext.SaveChangesAsync();

            // Act
            IEnumerable<NewsArticle> result = await repository.GetByAuthorAsync(author);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, n => Assert.Equal(author, n.Author));
        }

        [Fact]
        public async Task GetByCategoryAsync_WithValidCategory_ReturnsNewsArticles()
        {
            // Arrange
            NewsArticleRepository repository = CreateRepository();
            NewsArticle newsArticle1 = new NewsArticle(Guid.NewGuid(), "News 1", "<p>Content 1</p>", "Author 1");
            NewsArticle newsArticle2 = new NewsArticle(Guid.NewGuid(), "News 2", "<p>Content 2</p>", "Author 2");
            NewsArticle newsArticle3 = new NewsArticle(Guid.NewGuid(), "News 3", "<p>Content 3</p>", "Author 3");

            newsArticle1.SetCategory(NewsCategory.General);
            newsArticle2.SetCategory(NewsCategory.General);
            newsArticle3.SetCategory(NewsCategory.MatchReports);

            await repository.CreateNews(newsArticle1);
            await repository.CreateNews(newsArticle2);
            await repository.CreateNews(newsArticle3);
            await _dbContext.SaveChangesAsync();

            // Act
            IEnumerable<NewsArticle> result = await repository.GetByCategoryAsync("General");

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, n => Assert.Equal(NewsCategory.General, n.Category));
        }

        [Fact(Skip = "GetByTagAsync uses PostgreSQL-specific JsonContains function not supported by InMemory database")]
        public async Task GetByTagAsync_WithValidTag_ReturnsNewsArticles()
        {
            // This test is skipped because it requires PostgreSQL-specific JSON functions
            // that are not supported by the InMemory database provider
        }

        [Fact]
        public async Task CreateNews_WithValidNewsArticle_SavesSuccessfully()
        {
            // Arrange
            NewsArticleRepository repository = CreateRepository();
            Guid newsId = Guid.NewGuid();
            NewsArticle newsArticle = new NewsArticle(newsId, "New Article", "<p>New content</p>", "New Author");

            // Act
            await repository.CreateNews(newsArticle);
            await _dbContext.SaveChangesAsync();

            // Assert
            NewsArticle? savedNews = await repository.GetByIdAsync(newsId);
            Assert.NotNull(savedNews);
            Assert.Equal("New Article", savedNews.Title);
        }

        [Fact]
        public async Task UpdateNews_WithModifiedNewsArticle_UpdatesSuccessfully()
        {
            // Arrange
            NewsArticleRepository repository = CreateRepository();
            Guid newsId = Guid.NewGuid();
            NewsArticle newsArticle = new NewsArticle(newsId, "Original Title", "<p>Original content</p>", "Original Author");

            await repository.CreateNews(newsArticle);
            await _dbContext.SaveChangesAsync();

            // Modify the news article
            newsArticle.UpdateContent("Updated Title", "<p>Updated content</p>", "Updated summary");
            newsArticle.SetCategory(NewsCategory.General);
            newsArticle.AddTag("updated");

            // Act
            await repository.UpdateNews(newsArticle);
            await _dbContext.SaveChangesAsync();

            // Assert
            NewsArticle? updatedNews = await repository.GetByIdAsync(newsId);
            Assert.NotNull(updatedNews);
            Assert.Equal("Updated Title", updatedNews.Title);
            Assert.Equal("<p>Updated content</p>", updatedNews.ContentHtml);
            Assert.Equal(NewsCategory.General, updatedNews.Category);
            Assert.Contains("updated", updatedNews.Tags);
        }

        [Fact]
        public async Task ExistsAsync_WithExistingId_ReturnsTrue()
        {
            // Arrange
            NewsArticleRepository repository = CreateRepository();
            Guid newsId = Guid.NewGuid();
            NewsArticle newsArticle = new NewsArticle(newsId, "Existing Article", "<p>Existing content</p>", "Existing Author");

            await repository.CreateNews(newsArticle);
            await _dbContext.SaveChangesAsync();

            // Act
            bool exists = await repository.ExistsAsync(newsId);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsAsync_WithNonExistingId_ReturnsFalse()
        {
            // Arrange
            NewsArticleRepository repository = CreateRepository();
            Guid nonExistingId = Guid.NewGuid();

            // Act
            bool exists = await repository.ExistsAsync(nonExistingId);

            // Assert
            Assert.False(exists);
        }

        [Fact(Skip = "SearchAsync uses PostgreSQL-specific ILike function not supported by InMemory database")]
        public async Task SearchAsync_WithSearchTerm_ReturnsMatchingNewsArticles()
        {
            // This test is skipped because it requires PostgreSQL-specific ILike function
            // that is not supported by the InMemory database provider
        }
    }
} 