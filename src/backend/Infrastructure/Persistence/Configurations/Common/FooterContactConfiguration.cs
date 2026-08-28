using Domain.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Common;

public class FooterContactConfiguration : IEntityTypeConfiguration<FooterContact>
{
    public void Configure(EntityTypeBuilder<FooterContact> builder)
    {
        builder.ToTable("FooterContacts", "common");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Details)
            .HasMaxLength(500);

        builder.Property(x => x.Email)
            .HasMaxLength(200);

        builder.Property(x => x.Phone)
            .HasMaxLength(50);

        builder.Property(x => x.Url)
            .HasMaxLength(500);

        builder.Property(x => x.SortOrder)
            .IsRequired();

        builder.Property(x => x.Section)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.LastModifiedBy)
            .HasMaxLength(256);

        builder.HasIndex(x => new { x.Section, x.SortOrder });
    }
}
