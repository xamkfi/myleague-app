using Domain.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Common;

/// <summary>
/// EF Core configuration for <see cref="SiteSetting"/>.
/// </summary>
public class SiteSettingConfiguration : IEntityTypeConfiguration<SiteSetting>
{
    /// <summary>
    /// Configures the site settings table mapping.
    /// </summary>
    public void Configure(EntityTypeBuilder<SiteSetting> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired()
            .ValueGeneratedNever();

        builder.Property(x => x.Key)
            .IsRequired()
            .HasMaxLength(150);

        builder.HasIndex(x => x.Key)
            .IsUnique();

        builder.Property(x => x.ValueJson)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(x => x.LastModifiedBy)
            .HasMaxLength(100);
    }
}
