using Domain.Entities.Common;
using Domain.Enums.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Common;

/// <summary>
/// Entity Framework configuration for the RulesSection entity
/// </summary>
public class RulesSectionConfiguration : IEntityTypeConfiguration<RulesSection>
{
    /// <summary>
    /// Configures the entity mapping for RulesSection
    /// </summary>
    /// <param name="builder">The entity type builder</param>
    public void Configure(EntityTypeBuilder<RulesSection> builder)
    {
        builder.ToTable("RulesSections", "common");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.SortOrder)
            .IsRequired();

        builder.Property(x => x.SectionType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.ContentHtml)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(x => x.LastModifiedBy)
            .HasMaxLength(256);

        builder.HasOne(x => x.ParentSection)
            .WithMany(x => x.ChildSections)
            .HasForeignKey(x => x.ParentSectionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ParentSectionId);
        builder.HasIndex(x => x.SortOrder);
    }
}
