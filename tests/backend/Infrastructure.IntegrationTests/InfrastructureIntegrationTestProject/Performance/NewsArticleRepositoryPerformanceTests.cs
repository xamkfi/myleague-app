using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.Persistence.Repositories.Common;
using Domain.Entities.Common;
using Domain.Enums.Common;
using InfrastructureIntegrationTestProject.Common;
using System.Diagnostics;

namespace Tests.Backend.Infrastructure.IntegrationTests.InfrastructureIntegrationTestProject.Performance
{
    /// <summary>
    /// Performance tests for NewsArticleRepository to validate query efficiency
    /// </summary>
    public class NewsArticleRepositoryPerformanceTests : BaseIntegrationTest
    {
        private NewsArticleRepository CreateRepository()
        {
            ILogger<NewsArticleRepository> logger = _serviceProvider.GetRequiredService<ILogger<NewsArticleRepository>>();
            return new NewsArticleRepository(_dbContext, logger);
        }

        [Fact]
        public async Task GetByIdAsync_WithLargeDataset_PerformsWithinAcceptableTime()
        {
            // Arrange
            NewsArticleRepository repository = CreateRepository();
            const int articleCount = 100; // Reduced for InMemory database
            List<NewsArticle> articles = new List<NewsArticle>();

            // Create test data
            for (int i = 0; i < articleCount; i++)
            {
                NewsArticle article = new NewsArticle(
                    Guid.NewGuid(), 
                    $"Performance Test Article {i:D4}", 
                    $"<p>Performance test content for article {i}</p>", 
                    $"Author {i % 10}"
                );
                articles.Add(article);
                await repository.CreateNews(article);
            }
            await _dbContext.SaveChangesAsync();

            // Test performance of single ID lookup
            Guid targetId = articles[articleCount / 2].Id;
            
            // Act
            Stopwatch stopwatch = Stopwatch.StartNew();
            NewsArticle? result = await repository.GetByIdAsync(targetId);
            stopwatch.Stop();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(targetId, result.Id);
            
            // Performance assertion - GetById should be very fast
            Assert.True(stopwatch.ElapsedMilliseconds < 100, 
                $"GetByIdAsync took {stopwatch.ElapsedMilliseconds}ms, expected < 100ms");
        }

        [Fact]
        public async Task GetRecentAsync_WithLargeDataset_PerformsEfficientlyWithLimits()
        {
            // Arrange
            NewsArticleRepository repository = CreateRepository();
            const int articleCount = 100;

            // Create test data
            for (int i = 0; i < articleCount; i++)
            {
                NewsArticle article = new NewsArticle(
                    Guid.NewGuid(), 
                    $"Recent Article {i:D3}", 
                    $"<p>Content for recent article {i}</p>", 
                    "Performance Author"
                );
                
                // Archive some articles to test filtering
                if (i % 5 == 0)
                {
                    article.Archive();
                }
                
                await repository.CreateNews(article);
                
                // Small delay to ensure different timestamps
                if (i % 10 == 0)
                {
                    await Task.Delay(1);
                }
            }
            await _dbContext.SaveChangesAsync();

            // Test different page sizes
            int[] pageSizes = { 10, 25, 50 };
            
            foreach (int pageSize in pageSizes)
            {
                // Act
                Stopwatch stopwatch = Stopwatch.StartNew();
                IEnumerable<NewsArticle> result = await repository.GetRecentAsync(pageSize, includeArchived: false);
                stopwatch.Stop();

                // Assert
                List<NewsArticle> resultList = result.ToList();
                Assert.True(resultList.Count <= pageSize);
                Assert.All(resultList, article => Assert.False(article.IsArchived));
                
                // Performance assertion
                Assert.True(stopwatch.ElapsedMilliseconds < 200, 
                    $"GetRecentAsync({pageSize}) took {stopwatch.ElapsedMilliseconds}ms, expected < 200ms");
            }
        }

        [Fact]
        public async Task GetAllAsync_WithPagination_ScalesWithPageSize()
        {
            // Arrange
            NewsArticleRepository repository = CreateRepository();
            const int articleCount = 75;

            // Create test data with different categories
            for (int i = 0; i < articleCount; i++)
            {
                NewsArticle article = new NewsArticle(
                    Guid.NewGuid(), 
                    $"Paginated Article {i:D3}", 
                    $"<p>Content for paginated article {i}</p>", 
                    $"Author {i % 3}"
                );
                
                // Set categories for filtering tests
                if (i % 3 == 0) article.SetCategory(NewsCategory.General);
                else if (i % 3 == 1) article.SetCategory(NewsCategory.MatchReports);
                
                await repository.CreateNews(article);
            }
            await _dbContext.SaveChangesAsync();

            // Test pagination performance
            int[] pageSizes = { 10, 25, 50 };

            foreach (int pageSize in pageSizes)
            {
                // Act
                Stopwatch stopwatch = Stopwatch.StartNew();
                IEnumerable<NewsArticle> result = await repository.GetAllAsync(
                    page: 1, 
                    pageSize: pageSize
                );
                stopwatch.Stop();

                // Assert
                List<NewsArticle> resultList = result.ToList();
                Assert.True(resultList.Count <= pageSize);
                
                Assert.True(stopwatch.ElapsedMilliseconds < 150, 
                    $"GetAllAsync(pageSize: {pageSize}) took {stopwatch.ElapsedMilliseconds}ms, expected < 150ms");
                
                // Test that results are properly ordered (newest first)
                for (int i = 1; i < resultList.Count; i++)
                {
                    Assert.True(resultList[i-1].CreatedAt >= resultList[i].CreatedAt, 
                        "Results should be ordered by CreatedAt descending");
                }
            }
        }

        [Fact]
        public async Task GetByCategoryAsync_PerformsEfficiently()
        {
            // Arrange
            NewsArticleRepository repository = CreateRepository();
            const int articlesPerCategory = 25;
            
            // Create articles distributed across categories
            NewsCategory[] categories = { NewsCategory.General, NewsCategory.MatchReports, NewsCategory.Announcements };
            
            foreach (NewsCategory category in categories)
            {
                for (int i = 0; i < articlesPerCategory; i++)
                {
                    NewsArticle article = new NewsArticle(
                        Guid.NewGuid(), 
                        $"{category} Article {i:D3}", 
                        $"<p>Content for {category} article {i}</p>", 
                        "Category Author"
                    );
                    article.SetCategory(category);
                    await repository.CreateNews(article);
                }
            }
            await _dbContext.SaveChangesAsync();

            // Test category filtering performance
            foreach (NewsCategory category in categories)
            {
                // Act
                Stopwatch stopwatch = Stopwatch.StartNew();
                IEnumerable<NewsArticle> result = await repository.GetByCategoryAsync(category.ToString());
                stopwatch.Stop();

                // Assert
                List<NewsArticle> resultList = result.ToList();
                Assert.Equal(articlesPerCategory, resultList.Count);
                Assert.All(resultList, article => Assert.Equal(category, article.Category));
                
                Assert.True(stopwatch.ElapsedMilliseconds < 100, 
                    $"GetByCategoryAsync({category}) took {stopwatch.ElapsedMilliseconds}ms, expected < 100ms");
            }
        }

        [Fact]
        public async Task GetByAuthorAsync_PerformsEfficiently()
        {
            // Arrange
            NewsArticleRepository repository = CreateRepository();
            const int authorsCount = 10;
            const int articlesPerAuthor = 10;
            
            // Create articles for different authors
            for (int authorIndex = 0; authorIndex < authorsCount; authorIndex++)
            {
                string authorName = $"Author {authorIndex:D2}";
                
                for (int articleIndex = 0; articleIndex < articlesPerAuthor; articleIndex++)
                {
                    NewsArticle article = new NewsArticle(
                        Guid.NewGuid(), 
                        $"Article by {authorName} #{articleIndex:D2}", 
                        $"<p>Content by {authorName} for article {articleIndex}</p>", 
                        authorName
                    );
                    await repository.CreateNews(article);
                }
            }
            await _dbContext.SaveChangesAsync();

            // Test author filtering performance
            string targetAuthor = "Author 05";
            
            // Act
            Stopwatch stopwatch = Stopwatch.StartNew();
            IEnumerable<NewsArticle> result = await repository.GetByAuthorAsync(targetAuthor);
            stopwatch.Stop();

            // Assert
            List<NewsArticle> resultList = result.ToList();
            Assert.Equal(articlesPerAuthor, resultList.Count);
            Assert.All(resultList, article => Assert.Equal(targetAuthor, article.Author));
            
            Assert.True(stopwatch.ElapsedMilliseconds < 100, 
                $"GetByAuthorAsync took {stopwatch.ElapsedMilliseconds}ms, expected < 100ms");
        }

        [Fact]
        public async Task ExistsAsync_PerformsVeryFast()
        {
            // Arrange
            NewsArticleRepository repository = CreateRepository();
            const int articleCount = 50;
            List<Guid> articleIds = new List<Guid>();

            // Create test data
            for (int i = 0; i < articleCount; i++)
            {
                Guid id = Guid.NewGuid();
                NewsArticle article = new NewsArticle(
                    id, 
                    $"Exists Test Article {i:D4}", 
                    $"<p>Content for exists test {i}</p>", 
                    "Exists Author"
                );
                articleIds.Add(id);
                await repository.CreateNews(article);
            }
            await _dbContext.SaveChangesAsync();

            // Test existence checks
            List<(Guid Id, bool ShouldExist, string TestCase)> testCases = new List<(Guid, bool, string)>
            {
                (articleIds[0], true, "First article"),
                (articleIds[articleCount / 2], true, "Middle article"),
                (articleIds[articleCount - 1], true, "Last article"),
                (Guid.NewGuid(), false, "Non-existent article")
            };

            foreach ((Guid id, bool shouldExist, string testCase) in testCases)
            {
                // Act
                Stopwatch stopwatch = Stopwatch.StartNew();
                bool exists = await repository.ExistsAsync(id);
                stopwatch.Stop();

                // Assert
                Assert.Equal(shouldExist, exists);
                
                Assert.True(stopwatch.ElapsedMilliseconds < 50, 
                    $"ExistsAsync ({testCase}) took {stopwatch.ElapsedMilliseconds}ms, expected < 50ms");
            }
        }

        [Fact]
        public async Task ConcurrentOperations_MaintainPerformance()
        {
            // Arrange
            NewsArticleRepository repository = CreateRepository();
            const int concurrentOperations = 10;
            const int articlesPerOperation = 5;

            // Create some base data
            List<NewsArticle> baseArticles = new List<NewsArticle>();
            for (int i = 0; i < 20; i++)
            {
                NewsArticle article = new NewsArticle(
                    Guid.NewGuid(), 
                    $"Concurrent Test Base Article {i:D3}", 
                    $"<p>Base content {i}</p>", 
                    "Concurrent Author"
                );
                baseArticles.Add(article);
                await repository.CreateNews(article);
            }
            await _dbContext.SaveChangesAsync();

            // Prepare concurrent operations
            List<Task> tasks = new List<Task>();
            object lockObject = new object();
            List<long> operationTimes = new List<long>();

            for (int i = 0; i < concurrentOperations; i++)
            {
                int operationIndex = i;
                Task task = Task.Run(async () =>
                {
                    // Create a separate DbContext for this thread to avoid concurrency issues
                    using IServiceScope scope = _serviceProvider.CreateScope();
                    MyLeague.Infrastructure.Persistence.Contexts.CommonDbContext scopedDbContext = scope.ServiceProvider.GetRequiredService<MyLeague.Infrastructure.Persistence.Contexts.CommonDbContext>();
                    ILogger<NewsArticleRepository> logger = scope.ServiceProvider.GetRequiredService<ILogger<NewsArticleRepository>>();
                    NewsArticleRepository concurrentRepository = new NewsArticleRepository(scopedDbContext, logger);
                    
                    Stopwatch operationStopwatch = Stopwatch.StartNew();

                    // Perform mixed operations
                    for (int j = 0; j < articlesPerOperation; j++)
                    {
                        // Read operations
                        NewsArticle? article = await concurrentRepository.GetByIdAsync(baseArticles[j % baseArticles.Count].Id);
                        bool exists = await concurrentRepository.ExistsAsync(baseArticles[(j + 1) % baseArticles.Count].Id);
                        
                        // Create operation
                        NewsArticle newArticle = new NewsArticle(
                            Guid.NewGuid(), 
                            $"Concurrent Article {operationIndex}-{j}", 
                            $"<p>Concurrent content {operationIndex}-{j}</p>", 
                            $"Author {operationIndex}"
                        );
                        await concurrentRepository.CreateNews(newArticle);
                    }
                    
                    await scopedDbContext.SaveChangesAsync();
                    operationStopwatch.Stop();

                    lock (lockObject)
                    {
                        operationTimes.Add(operationStopwatch.ElapsedMilliseconds);
                    }
                });
                tasks.Add(task);
            }

            // Act
            Stopwatch totalStopwatch = Stopwatch.StartNew();
            await Task.WhenAll(tasks);
            totalStopwatch.Stop();

            // Assert
            Assert.Equal(concurrentOperations, operationTimes.Count);
            
            // Performance assertions
            double averageOperationTime = operationTimes.Average();
            long maxOperationTime = operationTimes.Max();
            
            Assert.True(averageOperationTime < 500, 
                $"Average operation time was {averageOperationTime:F2}ms, expected < 500ms");
            Assert.True(maxOperationTime < 1000, 
                $"Max operation time was {maxOperationTime}ms, expected < 1000ms");
            Assert.True(totalStopwatch.ElapsedMilliseconds < 5000, 
                $"Total concurrent test time was {totalStopwatch.ElapsedMilliseconds}ms, expected < 5000ms");
        }

        [Fact]
        public async Task CreateNews_BulkOperations_PerformsReasonablyWell()
        {
            // Arrange
            NewsArticleRepository repository = CreateRepository();
            const int batchSize = 25;
            List<NewsArticle> articles = new List<NewsArticle>();

            // Prepare articles
            for (int i = 0; i < batchSize; i++)
            {
                NewsArticle article = new NewsArticle(
                    Guid.NewGuid(), 
                    $"Bulk Creation Article {i:D3}", 
                    $"<p>Content for bulk creation test article {i}</p>", 
                    $"Bulk Author {i % 5}"
                );
                
                // Add some variety
                if (i % 3 == 0) article.SetCategory(NewsCategory.General);
                if (i % 4 == 0) article.AddTag($"tag-{i}");
                
                articles.Add(article);
            }

            // Act
            Stopwatch stopwatch = Stopwatch.StartNew();
            
            foreach (NewsArticle article in articles)
            {
                await repository.CreateNews(article);
            }
            await _dbContext.SaveChangesAsync();
            
            stopwatch.Stop();

            // Assert
            Assert.Equal(batchSize, articles.Count);
            
            // Performance assertion
            double averageTimePerArticle = (double)stopwatch.ElapsedMilliseconds / batchSize;
            Assert.True(averageTimePerArticle < 20, 
                $"Average time per article creation was {averageTimePerArticle:F2}ms, expected < 20ms");
            Assert.True(stopwatch.ElapsedMilliseconds < 1000, 
                $"Total bulk creation time was {stopwatch.ElapsedMilliseconds}ms, expected < 1000ms");
        }

        [Fact]
        public async Task MemoryUsage_WithLargeResultSets_RemainsReasonable()
        {
            // Arrange
            NewsArticleRepository repository = CreateRepository();
            const int articleCount = 50;

            // Create test data
            for (int i = 0; i < articleCount; i++)
            {
                NewsArticle article = new NewsArticle(
                    Guid.NewGuid(), 
                    $"Memory Test Article {i:D3}", 
                    $"<p>This is a reasonably long content for article {i} to test memory usage. " +
                    $"It contains enough text to make the test meaningful while not being excessive. " +
                    $"Article number {i} was created for memory testing purposes.</p>", 
                    $"Memory Author {i % 5}"
                );
                await repository.CreateNews(article);
            }
            await _dbContext.SaveChangesAsync();

            // Measure memory before operations
            long memoryBefore = GC.GetTotalMemory(true);

            // Act - Perform operations that could cause memory issues
            IEnumerable<NewsArticle> recentArticles = await repository.GetRecentAsync(25);
            IEnumerable<NewsArticle> allArticles = await repository.GetAllAsync(1, 20);
            IEnumerable<NewsArticle> authorArticles = await repository.GetByAuthorAsync("Memory Author 1");

            // Force garbage collection and measure memory
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            long memoryAfter = GC.GetTotalMemory(false);

            // Assert
            Assert.True(recentArticles.Count() <= 25);
            Assert.True(allArticles.Count() <= 20);
            Assert.True(authorArticles.Any());

            // Memory assertion - shouldn't use excessive memory (< 5MB increase)
            long memoryIncrease = memoryAfter - memoryBefore;
            Assert.True(memoryIncrease < 5 * 1024 * 1024, 
                $"Memory usage increased by {memoryIncrease / (1024 * 1024)}MB, expected < 5MB");
        }
    }
} 