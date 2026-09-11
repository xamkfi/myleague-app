using Domain.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Common;

public class SiteSettingsConfiguration : IEntityTypeConfiguration<SiteSettings>
{
    public void Configure(EntityTypeBuilder<SiteSettings> builder)
    {
        builder.ToTable("SiteSettings", "common");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AccessTokenExpirationMinutes)
            .IsRequired();

        builder.Property(x => x.RefreshTokenExpirationDays)
            .IsRequired();

        builder.Property(x => x.LoginCodeExpirationMinutes)
            .IsRequired();

        builder.Property(x => x.LoginCodeMaxAttempts)
            .IsRequired();

        builder.Property(x => x.SessionExpiryWarningMinutes)
            .IsRequired();
    }
}
