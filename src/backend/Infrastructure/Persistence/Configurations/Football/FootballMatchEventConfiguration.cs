using Domain.Entities.Football.Matches;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Football;

public class FootballMatchEventConfiguration : IEntityTypeConfiguration<FootballMatchEvent>
{
    public void Configure(EntityTypeBuilder<FootballMatchEvent> builder)
    {
        builder.ToTable("FootballMatchEvents", t =>
        {
            t.HasCheckConstraint("CK_FootballMatchEvent_PeriodNumber", "\"PeriodNumber\" > 0");
            t.HasCheckConstraint("CK_FootballMatchEvent_TimeInSeconds", "\"TimeInSeconds\" >= 0");
        });

        builder.HasKey(e => e.Id);
        builder.HasDiscriminator<string>("EventType")
            .HasValue<FootballGoal>("Goal")
            .HasValue<FootballCard>("Card")
            .HasValue<FootballSubstitution>("Substitution");

        builder.Property(e => e.MatchId).IsRequired();
        builder.Property(e => e.TeamId).IsRequired();
        builder.Property(e => e.PeriodNumber).IsRequired();
        builder.Property(e => e.TimeInSeconds).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(500).IsRequired(false);
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired(false);
        builder.Ignore(e => e.FormattedTime);

        builder.HasOne<Domain.Entities.Football.Teams.FootballTeam>()
            .WithMany()
            .HasForeignKey(e => e.TeamId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasIndex(e => e.MatchId).HasDatabaseName("IX_FootballMatchEvent_MatchId");
        builder.HasIndex(e => e.TeamId).HasDatabaseName("IX_FootballMatchEvent_TeamId");
        builder.HasIndex(e => new { e.MatchId, e.PeriodNumber, e.TimeInSeconds })
            .HasDatabaseName("IX_FootballMatchEvent_MatchId_Period_Time");
    }
}
