using Domain.Entities.Hockey.Competitions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Hockey;

public class HockeySeasonContentBlockConfiguration : IEntityTypeConfiguration<HockeySeasonContentBlock>
{
    public void Configure(EntityTypeBuilder<HockeySeasonContentBlock> builder)
    {
        builder.ToTable("HockeySeasonContentBlocks", "hockey");

        builder.HasKey(block => block.Id);

        builder.Property(block => block.SeasonId)
            .IsRequired();

        builder.Property(block => block.Title)
            .IsRequired()
            .HasMaxLength(HockeySeasonContentBlock.TitleMaxLength);

        builder.Property(block => block.ContentHtml)
            .IsRequired()
            .HasMaxLength(HockeySeasonContentBlock.ContentHtmlMaxLength);

        builder.Property(block => block.SortOrder)
            .IsRequired();

        builder.HasIndex(block => new { block.SeasonId, block.SortOrder })
            .HasDatabaseName("IX_HockeySeasonContentBlocks_Season_SortOrder");
    }
}
