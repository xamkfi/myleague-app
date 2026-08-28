using Domain.Entities.Hockey.Matches;
using Domain.Entities.Hockey.Teams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLeague.Infrastructure.Persistence.Configurations;

namespace MyLeague.Infrastructure.Persistence.Configurations.Hockey;

public class HockeyMatchOfficialConfiguration : BaseEntityConfiguration<HockeyMatchOfficial>
{
    protected override void ConfigureEntity(EntityTypeBuilder<HockeyMatchOfficial> builder)
    {
        builder.ToTable("HockeyMatchOfficials");

        builder.Property(o => o.MatchId).IsRequired();
        builder.Property(o => o.OfficialId).IsRequired();
        builder.Property(o => o.Role).IsRequired().HasConversion<string>();
        builder.Property(o => o.IsMainOfficial).IsRequired();

        builder.Ignore(o => o.Official);

        builder.HasOne<HockeyOfficial>()
            .WithMany()
            .HasForeignKey(o => o.OfficialId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(o => new { o.MatchId, o.OfficialId })
            .IsUnique()
            .HasDatabaseName("IX_HockeyMatchOfficials_Match_Official");
    }
}
