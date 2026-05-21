using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLeague.Infrastructure.Persistence.Configurations;

namespace MyLeague.Infrastructure.Persistence.Configurations.Floorball
{
    /// <summary>
    /// Entity Framework configuration for the <see cref="FloorballMatchActivePlayer"/> entity.
    /// Stores per-match field-player lineups for both teams. Goalies are tracked on the parent
    /// match (<c>HomeActiveGoalieId</c> / <c>AwayActiveGoalieId</c>).
    /// </summary>
    public class FloorballMatchActivePlayerConfiguration : BaseEntityConfiguration<FloorballMatchActivePlayer>
    {
        /// <inheritdoc />
        protected override void ConfigureEntity(EntityTypeBuilder<FloorballMatchActivePlayer> builder)
        {
            builder.ToTable("FloorballMatchActivePlayers");

            builder.Property(p => p.MatchId)
                .IsRequired()
                .HasComment("ID of the match this lineup entry belongs to");

            builder.Property(p => p.TeamId)
                .IsRequired()
                .HasComment("Team ID (always equals the match's HomeTeamId or AwayTeamId)");

            builder.Property(p => p.PlayerId)
                .IsRequired()
                .HasComment("ID of the player marked as an active field player");

            builder.Property(p => p.Position)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(32)
                .HasComment("Per-match field role: Forward, Center or Defender");

            builder.HasOne<FloorballMatch>()
                .WithMany(m => m.ActivePlayers)
                .HasForeignKey(p => p.MatchId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<FloorballPlayer>()
                .WithMany()
                .HasForeignKey(p => p.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(p => new { p.MatchId, p.TeamId, p.PlayerId })
                .IsUnique()
                .HasDatabaseName("IX_FloorballMatchActivePlayer_Match_Team_Player");

            builder.HasIndex(p => new { p.MatchId, p.TeamId })
                .HasDatabaseName("IX_FloorballMatchActivePlayer_Match_Team");
        }
    }
}
