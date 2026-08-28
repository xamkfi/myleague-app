using Domain.Entities.Common;
using Domain.Enums.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace MyLeague.Infrastructure.Persistence.Configurations.Common
{
    /// <summary>
    /// Entity Framework configuration for the NewsArticle entity.
    /// </summary>
    public class NewsArticleConfiguration : IEntityTypeConfiguration<NewsArticle>
    {
        /// <summary>
        /// Configures the entity mapping for NewsArticle.
        /// </summary>
        /// <param name="builder">The entity type builder.</param>
        public void Configure(EntityTypeBuilder<NewsArticle> builder)
        {
            
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

            // MainImage property - optional, stored as string
            builder.Property(n => n.MainImage)
                .HasConversion(
                    v => v != null ? v.ToString() : null,
                    v => v != null ? new Uri(v) : null);

            // ContentHtml property - required, unlimited length
            builder.Property(n => n.ContentHtml)
                .IsRequired();

            // Summary property - optional, max 500 characters
            builder.Property(n => n.Summary)
                .HasMaxLength(500);

            // Author property - optional, max 100 characters
            builder.Property(n => n.Author)
                .HasMaxLength(100);

            // CreatedAt property - required, UTC datetime
            builder.Property(n => n.CreatedAt)
                .IsRequired();

            // UpdatedAt property - optional, UTC datetime
            builder.Property(n => n.UpdatedAt);

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

            // TeamCategory property - optional audience filter, enum stored as string
            builder.Property(n => n.TeamCategory)
                .HasConversion(
                    v => v.HasValue ? v.Value.ToString() : null,
                    v => v != null ? Enum.Parse<TeamCategory>(v) : null)
                .HasMaxLength(50);

            // IsArchived property - required boolean with default false
            builder.Property(n => n.IsArchived)
                .IsRequired()
                .HasDefaultValue(false);

            // Tags property - JSON serialized string collection
            JsonSerializerOptions tagJsonOptions = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            ValueComparer<IReadOnlyList<string>> tagsComparer = new(
                (left, right) => ReferenceEquals(left, right)
                    || (left != null && right != null && left.SequenceEqual(right)),
                list => list == null ? 0 : list.Aggregate(0, (hash, tag) => HashCode.Combine(hash, tag.GetHashCode(StringComparison.Ordinal))),
                list => list == null ? new List<string>() : list.ToList());

            builder.Property(n => n.Tags)
                .HasField("_tags")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, tagJsonOptions),
                    v => (IReadOnlyList<string>)(JsonSerializer.Deserialize<List<string>>(v, tagJsonOptions) ?? new List<string>()),
                    tagsComparer);

            // ImageUrls property - JSON serialized URI collection
            builder.Property(n => n.ImageUrls)
                .HasConversion(
                    v => JsonSerializer.Serialize(v.Select(uri => uri.ToString()), (JsonSerializerOptions?)null),
                    v => (JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()).Select(s => new Uri(s)).ToList());

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

            // Index for filtering by audience / age-group category
            builder.HasIndex(n => n.TeamCategory)
                .HasDatabaseName("IX_News_TeamCategory");

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
