using Domain.Entities.Hockey.Teams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Hockey;

public class HockeyTeamConfiguration : IEntityTypeConfiguration<HockeyTeam>
{
    public void Configure(EntityTypeBuilder<HockeyTeam> builder)
    {
        builder.ToTable("HockeyTeams");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name).IsRequired().HasMaxLength(100);
        builder.Property(t => t.ShortName).IsRequired().HasMaxLength(4);
        builder.Property(t => t.HomeArena).IsRequired().HasMaxLength(100);
        builder.Property(t => t.PrimaryJerseyColor).IsRequired().HasMaxLength(50);
        builder.Property(t => t.SecondaryJerseyColor).HasMaxLength(50);
        builder.Property(t => t.TeamCategory).IsRequired().HasConversion<string>();
        builder.Property(t => t.IsActive).IsRequired();
        builder.Property(t => t.LogoUrl)
            .HasConversion(v => v != null ? v.ToString() : null, v => v != null ? new Uri(v) : null);

        builder.Ignore(t => t.Club);
        builder.Ignore(t => t.HasActiveMembers);

        builder.Property(t => t.ClubId).IsRequired();

        builder.HasMany(t => t.Roster)
            .WithOne(tp => tp.Team)
            .HasForeignKey(tp => tp.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Lines)
            .WithOne(l => l.Team)
            .HasForeignKey(l => l.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.StaffMembers)
            .WithOne(s => s.Team)
            .HasForeignKey(s => s.TeamId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
