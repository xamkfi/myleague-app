using Domain.Entities.Football.Competitions;
using Domain.Entities.Football.Statistics;
using Domain.Entities.Football.Teams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Football;

public class FootballTeamSeasonStatisticsConfiguration : IEntityTypeConfiguration<FootballTeamSeasonStatistics>
{
    public void Configure(EntityTypeBuilder<FootballTeamSeasonStatistics> builder)
    {
        builder.ToTable("FootballTeamSeasonStatistics");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.TeamId).IsRequired();
        builder.Property(s => s.CompetitionId).IsRequired();
        builder.HasOne(s => s.Team).WithMany().HasForeignKey(s => s.TeamId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(s => s.Competition).WithMany().HasForeignKey(s => s.CompetitionId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(s => s.GamesPlayed).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.Wins).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.Losses).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.Draws).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.Points).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.GoalsFor).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.GoalsAgainst).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.GoalDifference).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.HomeWins).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.AwayWins).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.HomeLosses).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.AwayLosses).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.CleanSheets).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.YellowCards).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.RedCards).IsRequired().HasDefaultValue(0);
        builder.HasIndex(s => new { s.TeamId, s.CompetitionId }).IsUnique().HasDatabaseName("IX_FootballTeamSeasonStatistics_Team_Competition");
    }
}

public class FootballPlayerSeasonStatisticsConfiguration : IEntityTypeConfiguration<FootballPlayerSeasonStatistics>
{
    public void Configure(EntityTypeBuilder<FootballPlayerSeasonStatistics> builder)
    {
        builder.ToTable("FootballPlayerSeasonStatistics");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.PlayerId).IsRequired();
        builder.Property(s => s.TeamId).IsRequired();
        builder.Property(s => s.CompetitionId).IsRequired();
        builder.HasOne(s => s.Player).WithMany().HasForeignKey(s => s.PlayerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(s => s.Team).WithMany().HasForeignKey(s => s.TeamId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(s => s.Competition).WithMany().HasForeignKey(s => s.CompetitionId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(s => s.GamesPlayed).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.Goals).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.Assists).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.Points).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.YellowCards).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.RedCards).IsRequired().HasDefaultValue(0);
        builder.HasIndex(s => new { s.PlayerId, s.TeamId, s.CompetitionId }).IsUnique().HasDatabaseName("IX_FootballPlayerSeasonStatistics_Player_Team_Competition");
        builder.HasIndex(s => new { s.CompetitionId, s.Goals }).HasDatabaseName("IX_FootballPlayerSeasonStatistics_Competition_Goals");
    }
}

public class FootballMatchTeamStatisticsConfiguration : IEntityTypeConfiguration<FootballMatchTeamStatistics>
{
    public void Configure(EntityTypeBuilder<FootballMatchTeamStatistics> builder)
    {
        builder.ToTable("FootballMatchTeamStatistics");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.MatchId).IsRequired();
        builder.Property(s => s.TeamId).IsRequired();
        builder.HasOne(s => s.Match).WithMany().HasForeignKey(s => s.MatchId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(s => s.Team).WithMany().HasForeignKey(s => s.TeamId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(s => s.Goals).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.YellowCards).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.RedCards).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.Substitutions).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.CleanSheet).IsRequired().HasDefaultValue(false);
        builder.HasIndex(s => new { s.MatchId, s.TeamId }).IsUnique().HasDatabaseName("IX_FootballMatchTeamStatistics_Match_Team");
    }
}

public class FootballStatisticsCacheConfiguration : IEntityTypeConfiguration<FootballStatisticsCache>
{
    public void Configure(EntityTypeBuilder<FootballStatisticsCache> builder)
    {
        builder.ToTable("FootballStatisticsCache");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.CacheKey).IsRequired().HasMaxLength(255);
        builder.Ignore(s => s.Competition);
        builder.Property(s => s.JsonData).IsRequired().HasColumnType("text");
        builder.Property(s => s.LastUpdated).IsRequired();
        builder.Property(s => s.ExpiresAt).IsRequired();
        builder.HasIndex(s => s.CacheKey).IsUnique().HasDatabaseName("IX_FootballStatisticsCache_CacheKey");
    }
}

public class FootballCompetitionDivisionConfiguration : IEntityTypeConfiguration<FootballCompetitionDivision>
{
    public void Configure(EntityTypeBuilder<FootballCompetitionDivision> builder)
    {
        builder.ToTable("FootballSeasonDivisions", "football");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CompetitionId).IsRequired();
        builder.Property(x => x.DivisionId).IsRequired();
        builder.HasOne(x => x.Competition).WithMany().HasForeignKey(x => x.CompetitionId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.CompetitionId, x.DivisionId }).IsUnique().HasDatabaseName("IX_FootballSeasonDivisions_Season_Division");
    }
}

public class FootballCompetitionDivisionTeamConfiguration : IEntityTypeConfiguration<FootballCompetitionDivisionTeam>
{
    public void Configure(EntityTypeBuilder<FootballCompetitionDivisionTeam> builder)
    {
        builder.ToTable("FootballSeasonDivisionTeams", "football");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CompetitionDivisionId).IsRequired();
        builder.Property(x => x.TeamId).IsRequired();
        builder.Property(x => x.CompetitionId).IsRequired();
        builder.HasOne(x => x.CompetitionDivision).WithMany(sd => sd.Teams).HasForeignKey(x => x.CompetitionDivisionId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Team).WithMany().HasForeignKey(x => x.TeamId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.CompetitionDivisionId, x.TeamId }).IsUnique().HasDatabaseName("IX_FootballSeasonDivisionTeams_SeasonDivision_Team");
        builder.HasIndex(x => new { x.CompetitionId, x.TeamId }).IsUnique().HasDatabaseName("IX_FootballSeasonDivisionTeams_Season_Team");
    }
}

public class FootballTournamentGroupConfiguration : IEntityTypeConfiguration<FootballTournamentGroup>
{
    public void Configure(EntityTypeBuilder<FootballTournamentGroup> builder)
    {
        builder.ToTable("FootballTournamentGroups", "football");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Order).IsRequired();
        builder.Property(x => x.TournamentId).IsRequired();
        builder.HasIndex(x => new { x.TournamentId, x.Order }).HasDatabaseName("IX_FootballTournamentGroups_Tournament_Order");
    }
}

public class FootballTournamentGroupTeamConfiguration : IEntityTypeConfiguration<FootballTournamentGroupTeam>
{
    public void Configure(EntityTypeBuilder<FootballTournamentGroupTeam> builder)
    {
        builder.ToTable("FootballTournamentGroupTeams", "football");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TournamentGroupId).IsRequired();
        builder.Property(x => x.TeamId).IsRequired();
        builder.HasOne(x => x.TournamentGroup).WithMany(g => g.Teams).HasForeignKey(x => x.TournamentGroupId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Team).WithMany().HasForeignKey(x => x.TeamId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.TournamentGroupId, x.TeamId }).IsUnique().HasDatabaseName("IX_FootballTournamentGroupTeams_Group_Team");
    }
}
