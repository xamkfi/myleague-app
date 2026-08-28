using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Floorball;

public class FloorballSeasonContentBlockConfiguration : IEntityTypeConfiguration<FloorballSeasonContentBlock>
{
    public void Configure(EntityTypeBuilder<FloorballSeasonContentBlock> builder)
    {
        builder.ToTable("FloorballSeasonContentBlocks", "floorball");

        builder.HasKey(block => block.Id);

        builder.Property(block => block.SeasonId)
            .IsRequired();

        builder.Property(block => block.Title)
            .IsRequired()
            .HasMaxLength(FloorballSeasonContentBlock.TitleMaxLength);

        builder.Property(block => block.ContentHtml)
            .IsRequired()
            .HasMaxLength(FloorballSeasonContentBlock.ContentHtmlMaxLength);

        builder.Property(block => block.SortOrder)
            .IsRequired();

        builder.HasIndex(block => new { block.SeasonId, block.SortOrder })
            .HasDatabaseName("IX_FloorballSeasonContentBlocks_Season_SortOrder");
    }
}
