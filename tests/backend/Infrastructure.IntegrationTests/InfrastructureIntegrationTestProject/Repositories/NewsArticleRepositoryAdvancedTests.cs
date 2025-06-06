using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.Persistence.Repositories.Common;
using Domain.Entities.Common;
using Domain.Enums.Common;
using InfrastructureIntegrationTestProject.Common;

namespace Tests.Backend.Infrastructure.IntegrationTests.InfrastructureIntegrationTestProject.Repositories
{
    public class NewsArticleRepositoryAdvancedTests : BaseIntegrationTest
    {
        private NewsArticleRepository CreateRepository()
        {
            ILogger<NewsArticleRepository> logger = _serviceProvider.GetRequiredService<ILogger<NewsArticleRepository>>();
            return new NewsArticleRepository(_dbContext, logger);
        }

        [Fact(Skip = "GetAllAsync with author filter uses PostgreSQL-specific ILike function not supported by InMemory database")]
        public async Task GetAllAsync_WithMultipleFilters_ReturnsCorrectResults()
        {
            // Arrange
            NewsArticleRepository repository = CreateRepository();
            string author = "Test Author";
            
            NewsArticle newsArticle1 = new NewsArticle(Guid.NewGuid(), "General Article by Test Author", "<p>General content</p>", author);
            NewsArticle newsArticle2 = new NewsArticle(Guid.NewGuid(), "Match Report by Test Author", "<p>Match content</p>", author);
            NewsArticle newsArticle3 = new NewsArticle(Guid.NewGuid(), "General Article by Other Author", "<p>General content</p>", "Other Author");

            newsArticle1.SetCategory(NewsCategory.General);
            newsArticle2.SetCategory(NewsCategory.MatchReports);
            newsArticle3.SetCategory(NewsCategory.General);

            await repository.CreateNews(newsArticle1);
            await repository.CreateNews(newsArticle2);
            await repository.CreateNews(newsArticle3);
            await _dbContext.SaveChangesAsync();

            // Act
            IEnumerable<NewsArticle> result = await repository.GetAllAsync(page: 1, pageSize: 10, category: "General", author: author);

            // Assert
            Assert.Single(result);
            Assert.Equal(author, result.First().Author);
            Assert.Equal(NewsCategory.General, result.First().Category);
        }

        [Fact]
        public async Task GetBySportCategoryAsync_WithValidSportCategory_ReturnsCorrectResults()
        {
            // Arrange
            NewsArticleRepository repository = CreateRepository();
            NewsArticle newsArticle1 = new NewsArticle(Guid.NewGuid(), "Football News", "<p>Football content</p>", "Sports Reporter");
            NewsArticle newsArticle2 = new NewsArticle(Guid.NewGuid(), "Hockey News", "<p>Hockey content</p>", "Sports Reporter");
            NewsArticle newsArticle3 = new NewsArticle(Guid.NewGuid(), "General News", "<p>General content</p>", "News Reporter");

            newsArticle1.SetSportCategory(SportsCategory.Football);
            newsArticle2.SetSportCategory(SportsCategory.Icehockey);

            await repository.CreateNews(newsArticle1);
            await repository.CreateNews(newsArticle2);
            await repository.CreateNews(newsArticle3);
            await _dbContext.SaveChangesAsync();

            // Act
            IEnumerable<NewsArticle> result = await repository.GetBySportCategoryAsync("Football");

            // Assert
            Assert.Single(result);
            Assert.Equal(SportsCategory.Football, result.First().SportCategory);
        }

        [Fact]
        public async Task ConcurrentAccess_MultipleRepositories_HandlesCorrectly()
        {
            // Arrange
            NewsArticleRepository repository1 = CreateRepository();
            NewsArticleRepository repository2 = CreateRepository();
            
            NewsArticle newsArticle1 = new NewsArticle(Guid.NewGuid(), "Concurrent News 1", "<p>Content 1</p>", "Author 1");
            NewsArticle newsArticle2 = new NewsArticle(Guid.NewGuid(), "Concurrent News 2", "<p>Content 2</p>", "Author 2");

            // Act
            Task task1 = repository1.CreateNews(newsArticle1);
            Task task2 = repository2.CreateNews(newsArticle2);
            
            await Task.WhenAll(task1, task2);
            await _dbContext.SaveChangesAsync();

            // Assert
            NewsArticle? result1 = await repository1.GetByIdAsync(newsArticle1.Id);
            NewsArticle? result2 = await repository2.GetByIdAsync(newsArticle2.Id);
            
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.Equal("Concurrent News 1", result1.Title);
            Assert.Equal("Concurrent News 2", result2.Title);
        }

        [Fact]
        public async Task GetAllAsync_WithPaginationAndOrdering_ReturnsCorrectOrder()
        {
            // Arrange
            NewsArticleRepository repository = CreateRepository();
            DateTime baseTime = DateTime.UtcNow.AddDays(-10);
            
            List<NewsArticle> articles = new List<NewsArticle>
            {
                new NewsArticle(Guid.NewGuid(), "Oldest Article", "<p>Old content</p>", "Author"),
                new NewsArticle(Guid.NewGuid(), "Middle Article", "<p>Middle content</p>", "Author"),
                new NewsArticle(Guid.NewGuid(), "Newest Article", "<p>New content</p>", "Author")
            };

            foreach (NewsArticle article in articles)
            {
                await repository.CreateNews(article);
                await Task.Delay(10); // Ensure different timestamps
            }
            await _dbContext.SaveChangesAsync();

            // Act
            IEnumerable<NewsArticle> firstPage = await repository.GetAllAsync(page: 1, pageSize: 2);
            IEnumerable<NewsArticle> secondPage = await repository.GetAllAsync(page: 2, pageSize: 2);

            // Assert
            Assert.Equal(2, firstPage.Count());
            Assert.Single(secondPage);
            
            // Should be ordered by creation date descending (newest first)
            List<NewsArticle> firstPageList = firstPage.ToList();
            Assert.Equal("Newest Article", firstPageList[0].Title);
            Assert.Equal("Middle Article", firstPageList[1].Title);
            Assert.Equal("Oldest Article", secondPage.First().Title);
        }

        [Fact(Skip = "SearchAsync uses PostgreSQL-specific ILike function not supported by InMemory database")]
        public async Task SearchAsync_WithSpecialCharacters_HandlesCorrectly()
        {
            // Arrange
            NewsArticleRepository repository = CreateRepository();
            NewsArticle newsArticle1 = new NewsArticle(Guid.NewGuid(), "Football & Basketball Tournament", "<p>Sports content with & character</p>", "Sports Reporter");
            NewsArticle newsArticle2 = new NewsArticle(Guid.NewGuid(), "100% Match Results", "<p>Complete match results</p>", "Results Reporter");
            NewsArticle newsArticle3 = new NewsArticle(Guid.NewGuid(), "Regular News", "<p>Regular content</p>", "News Reporter");

            await repository.CreateNews(newsArticle1);
            await repository.CreateNews(newsArticle2);
            await repository.CreateNews(newsArticle3);
            await _dbContext.SaveChangesAsync();

            // Act
            IEnumerable<NewsArticle> result = await repository.SearchAsync("&");

            // Assert
            Assert.Single(result);
            Assert.Contains("&", result.First().Title);
        }

        [Fact]
        public async Task TagOperations_AddAndRemoveTags_WorksCorrectly()
        {
            // Arrange
            NewsArticleRepository repository = CreateRepository();
            NewsArticle newsArticle = new NewsArticle(Guid.NewGuid(), "Tagged Article", "<p>Content with tags</p>", "Author");

            // Add multiple tags
            newsArticle.AddTag("sports");
            newsArticle.AddTag("football");
            newsArticle.AddTag("tournament");
            
            await repository.CreateNews(newsArticle);
            await _dbContext.SaveChangesAsync();

            // Act & Assert - Check initial tags
            NewsArticle? result = await repository.GetByIdAsync(newsArticle.Id);
            Assert.NotNull(result);
            Assert.Equal(3, result.Tags.Count);
            Assert.Contains("sports", result.Tags);
            Assert.Contains("football", result.Tags);
            Assert.Contains("tournament", result.Tags);

            // Remove a tag
            result.RemoveTag("football");
            await repository.UpdateNews(result);
            await _dbContext.SaveChangesAsync();

            // Assert after removal
            NewsArticle? updatedResult = await repository.GetByIdAsync(newsArticle.Id);
            Assert.NotNull(updatedResult);
            Assert.Equal(2, updatedResult.Tags.Count);
            Assert.Contains("sports", updatedResult.Tags);
            Assert.Contains("tournament", updatedResult.Tags);
            Assert.DoesNotContain("football", updatedResult.Tags);
        }

        [Fact]
        public async Task ImageOperations_SetAndRetrieve_WorksCorrectly()
        {
            // Arrange
            NewsArticleRepository repository = CreateRepository();
            NewsArticle newsArticle = new NewsArticle(Guid.NewGuid(), "Article with Images", "<p>Content with images</p>", "Author");
            
            Uri imageUri1 = new Uri("https://example.com/image1.jpg");
            Uri imageUri2 = new Uri("https://example.com/image2.jpg");
            
            newsArticle.SetImage(imageUri1);
            newsArticle.SetImage(imageUri2);
            
            await repository.CreateNews(newsArticle);
            await _dbContext.SaveChangesAsync();

            // Act
            NewsArticle? result = await repository.GetByIdAsync(newsArticle.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.ImageUrls.Count);
            Assert.Contains(imageUri1, result.ImageUrls);
            Assert.Contains(imageUri2, result.ImageUrls);
        }

        [Fact]
        public async Task ArchiveOperations_ArchiveAndRestore_WorksCorrectly()
        {
            // Arrange
            NewsArticleRepository repository = CreateRepository();
            NewsArticle newsArticle = new NewsArticle(Guid.NewGuid(), "Article to Archive", "<p>Content to be archived</p>", "Author");
            
            await repository.CreateNews(newsArticle);
            await _dbContext.SaveChangesAsync();

            // Act - Archive
            newsArticle.Archive();
            await repository.UpdateNews(newsArticle);
            await _dbContext.SaveChangesAsync();

            // Assert - Check archived
            NewsArticle? archivedResult = await repository.GetByIdAsync(newsArticle.Id);
            Assert.NotNull(archivedResult);
            Assert.True(archivedResult.IsArchived);

            // Act - Restore
            archivedResult.Restore();
            await repository.UpdateNews(archivedResult);
            await _dbContext.SaveChangesAsync();

            // Assert - Check restored
            NewsArticle? restoredResult = await repository.GetByIdAsync(newsArticle.Id);
            Assert.NotNull(restoredResult);
            Assert.False(restoredResult.IsArchived);
        }

        [Fact]
        public async Task GetRecentAsync_WithLargeDataset_PerformsEfficiently()
        {
            // Arrange
            NewsArticleRepository repository = CreateRepository();
            List<NewsArticle> articles = new List<NewsArticle>();
            
            // Create a larger dataset
            for (int i = 1; i <= 50; i++)
            {
                NewsArticle article = new NewsArticle(Guid.NewGuid(), $"Article {i:D2}", $"<p>Content for article {i}</p>", $"Author {i % 5}");
                if (i % 10 == 0)
                {
                    article.Archive(); // Archive every 10th article
                }
                articles.Add(article);
            }

            foreach (NewsArticle article in articles)
            {
                await repository.CreateNews(article);
            }
            await _dbContext.SaveChangesAsync();

            // Act
            IEnumerable<NewsArticle> recent = await repository.GetRecentAsync(10, includeArchived: false);
            IEnumerable<NewsArticle> recentWithArchived = await repository.GetRecentAsync(10, includeArchived: true);

            // Assert
            Assert.Equal(10, recent.Count());
            Assert.Equal(10, recentWithArchived.Count());
            Assert.All(recent, article => Assert.False(article.IsArchived));
        }

        [Fact(Skip = "GetAllTagsAsync uses PostgreSQL-specific JSON operations not supported by InMemory database")]
        public async Task GetAllTagsAsync_WithDuplicateTags_ReturnsUniqueTagsOnly()
        {
            // Arrange
            NewsArticleRepository repository = CreateRepository();
            
            NewsArticle article1 = new NewsArticle(Guid.NewGuid(), "Article 1", "<p>Content 1</p>", "Author 1");
            NewsArticle article2 = new NewsArticle(Guid.NewGuid(), "Article 2", "<p>Content 2</p>", "Author 2");
            NewsArticle article3 = new NewsArticle(Guid.NewGuid(), "Article 3", "<p>Content 3</p>", "Author 3");

            // Add overlapping tags
            article1.AddTag("sports");
            article1.AddTag("football");
            article2.AddTag("sports");
            article2.AddTag("basketball");
            article3.AddTag("news");
            article3.AddTag("football");

            await repository.CreateNews(article1);
            await repository.CreateNews(article2);
            await repository.CreateNews(article3);
            await _dbContext.SaveChangesAsync();

            // Act
            IEnumerable<string> allTags = await repository.GetAllTagsAsync();

            // Assert
            List<string> tagList = allTags.ToList();
            Assert.Equal(4, tagList.Count); // sports, football, basketball, news
            Assert.Contains("sports", tagList);
            Assert.Contains("football", tagList);
            Assert.Contains("basketball", tagList);
            Assert.Contains("news", tagList);
        }
    }
} 