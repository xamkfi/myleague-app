using Domain.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Common;

/// <summary>
/// Entity Framework configuration for the SeasonContentBlock entity
/// </summary>
public class SeasonContentBlockConfiguration : IEntityTypeConfiguration<SeasonContentBlock>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<SeasonContentBlock> builder)
    {
        builder.ToTable("SeasonContentBlocks", "common");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Sport)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.CompetitionId)
            .IsRequired();

        builder.Property(x => x.SeasonYear)
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.ContentHtml)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(x => x.SortOrder)
            .IsRequired();

        builder.Property(x => x.LastModifiedBy)
            .HasMaxLength(256);

        builder.HasIndex(x => new { x.Sport, x.SeasonYear, x.SortOrder });
        builder.HasIndex(x => new { x.CompetitionId, x.SortOrder });
    }
}
