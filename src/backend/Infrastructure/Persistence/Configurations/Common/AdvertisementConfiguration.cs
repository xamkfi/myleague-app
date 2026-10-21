using Domain.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Common;

public class AdvertisementConfiguration : IEntityTypeConfiguration<Advertisement>
{
    public void Configure(EntityTypeBuilder<Advertisement> builder)
    {
        builder.ToTable("Advertisements", "common");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(Advertisement.NameMaxLength);

        builder.Property(x => x.ImageUrl)
            .IsRequired()
            .HasConversion(
                uri => uri.ToString(),
                value => new Uri(value, UriKind.Absolute))
            .HasMaxLength(Advertisement.UrlMaxLength);

        builder.Property(x => x.LinkUrl)
            .HasConversion(
                uri => uri != null ? uri.ToString() : null,
                value => value != null ? new Uri(value, UriKind.Absolute) : null)
            .HasMaxLength(Advertisement.UrlMaxLength);

        builder.Property(x => x.AltText)
            .HasMaxLength(Advertisement.AltTextMaxLength);

        builder.Property(x => x.Slot)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.SortOrder)
            .IsRequired();

        builder.HasIndex(x => new { x.Slot, x.IsActive })
            .HasDatabaseName("IX_Advertisements_Slot_IsActive");
    }
}
