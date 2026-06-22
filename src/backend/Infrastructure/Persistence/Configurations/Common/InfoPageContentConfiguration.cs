using Domain.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Common;

/// <summary>
/// Entity Framework configuration for the InfoPageContent entity
/// </summary>
public class InfoPageContentConfiguration : IEntityTypeConfiguration<InfoPageContent>
{
    /// <summary>
    /// Configures the entity mapping for InfoPageContent
    /// </summary>
    /// <param name="builder">The entity type builder</param>
    public void Configure(EntityTypeBuilder<InfoPageContent> builder)
    {
        builder.ToTable("InfoPageContents", "common");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PageSlug)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(x => x.PageSlug)
            .IsUnique();

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.ContentHtml)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(x => x.LastModifiedBy)
            .HasMaxLength(256);
    }
}
