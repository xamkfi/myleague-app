using Domain.Entities.Football.Competitions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Football;

public class FootballSeasonContentBlockConfiguration : IEntityTypeConfiguration<FootballSeasonContentBlock>
{
    public void Configure(EntityTypeBuilder<FootballSeasonContentBlock> builder)
    {
        builder.ToTable("FootballSeasonContentBlocks", "football");

        builder.HasKey(block => block.Id);

        builder.Property(block => block.SeasonId)
            .IsRequired();

        builder.Property(block => block.Title)
            .IsRequired()
            .HasMaxLength(FootballSeasonContentBlock.TitleMaxLength);

        builder.Property(block => block.ContentHtml)
            .IsRequired()
            .HasMaxLength(FootballSeasonContentBlock.ContentHtmlMaxLength);

        builder.Property(block => block.SortOrder)
            .IsRequired();

        builder.HasIndex(block => new { block.SeasonId, block.SortOrder })
            .HasDatabaseName("IX_FootballSeasonContentBlocks_Season_SortOrder");
    }
}
