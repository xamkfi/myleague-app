using Domain.Entities.Common;
using Domain.Enums.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace MyLeague.Infrastructure.Persistence.Configurations.Common
{
    /// <summary>
    /// Entity Framework configuration for the News entity.
    /// </summary>
    public class NewsConfiguration : IEntityTypeConfiguration<News>
    {
        /// <summary>
        /// Configures the entity mapping for News.
        /// </summary>
        /// <param name="builder">The entity type builder.</param>
        public void Configure(EntityTypeBuilder<News> builder)
        {
            // Table configuration
            builder.ToTable("News", "common");
            
            // Primary key
            builder.HasKey(n => n.Id);
            
            // Id property
            builder.Property(n => n.Id)
                .IsRequired()
                .ValueGeneratedNever();

            // Title property - required, max 200 characters
            builder.Property(n => n.Title)
                .IsRequired()
                .HasMaxLength(200);

            // ContentHtml property - required, unlimited length
            builder.Property(n => n.ContentHtml)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            // Summary property - optional, max 500 characters
            builder.Property(n => n.Summary)
                .HasMaxLength(500);

            // Author property - optional, max 100 characters
            builder.Property(n => n.Author)
                .HasMaxLength(100);

            // CreatedAt property - required, UTC datetime
            builder.Property(n => n.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime2");

            // UpdatedAt property - optional, UTC datetime
            builder.Property(n => n.UpdatedAt)
                .HasColumnType("datetime2");

            // Category property - enum stored as string
            builder.Property(n => n.Category)
                .HasConversion(
                    v => v.HasValue ? v.Value.ToString() : null,
                    v => v != null ? Enum.Parse<NewsCategory>(v) : null)
                .HasMaxLength(50);

            // SportCategory property - enum stored as string
            builder.Property(n => n.SportCategory)
                .HasConversion(
                    v => v.HasValue ? v.Value.ToString() : null,
                    v => v != null ? Enum.Parse<SportsCategory>(v) : null)
                .HasMaxLength(50);

            // IsArchived property - required boolean with default false
            builder.Property(n => n.IsArchived)
                .IsRequired()
                .HasDefaultValue(false);

            // Tags property - JSON serialized string collection
            builder.Property(n => n.Tags)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>())
                .HasColumnType("nvarchar(max)");

            // ImageUrls property - JSON serialized URI collection
            builder.Property(n => n.ImageUrls)
                .HasConversion(
                    v => JsonSerializer.Serialize(v.Select(uri => uri.ToString()), (JsonSerializerOptions?)null),
                    v => (JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()).Select(s => new Uri(s)).ToList())
                .HasColumnType("nvarchar(max)");

            // Performance indexes
            
            // Index for retrieving recent news (most common query)
            builder.HasIndex(n => n.CreatedAt)
                .HasDatabaseName("IX_News_CreatedAt")
                .IsDescending();

            // Index for filtering by category
            builder.HasIndex(n => n.Category)
                .HasDatabaseName("IX_News_Category");

            // Index for filtering by sport category
            builder.HasIndex(n => n.SportCategory)
                .HasDatabaseName("IX_News_SportCategory");

            // Index for filtering by author
            builder.HasIndex(n => n.Author)
                .HasDatabaseName("IX_News_Author");

            // Index for filtering archived/active news
            builder.HasIndex(n => n.IsArchived)
                .HasDatabaseName("IX_News_IsArchived");

            // Composite index for efficient archived + date queries
            builder.HasIndex(n => new { n.IsArchived, n.CreatedAt })
                .HasDatabaseName("IX_News_IsArchived_CreatedAt")
                .IsDescending(false, true); // IsArchived ASC, CreatedAt DESC
        }
    }
} 