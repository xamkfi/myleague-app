using Domain.Entities.Football.Competitions;
using Domain.Entities.Football.Matches;
using Domain.Entities.Football.Statistics;
using Domain.Entities.Football.Teams;
using Domain.ValueObjects.Football;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence.Configurations.Football;
using MyLeague.Infrastructure.Persistence.Extensions;

namespace MyLeague.Infrastructure.Persistence.Contexts;

/// <summary>
/// Database context for football-specific entities.
/// </summary>
public class FootballDbContext : DbContext
{
    private bool _isDispatchingEvents;

    public FootballDbContext(DbContextOptions<FootballDbContext> options) : base(options)
    {
    }

    public DbSet<FootballPlayer> FootballPlayers { get; set; }
    public DbSet<FootballTeam> FootballTeams { get; set; }
    public DbSet<FootballTeamPlayer> FootballTeamPlayers { get; set; }
    public DbSet<FootballMatch> FootballMatches { get; set; }
    public DbSet<FootballCompetition> FootballCompetitions { get; set; }
    public DbSet<FootballSeason> FootballSeasons { get; set; }
    public DbSet<FootballTournament> FootballTournaments { get; set; }
    public DbSet<FootballTournamentGroup> FootballTournamentGroups { get; set; }
    public DbSet<FootballTournamentGroupTeam> FootballTournamentGroupTeams { get; set; }
    public DbSet<FootballReferee> FootballReferees { get; set; }
    public DbSet<FootballPeriodScore> FootballPeriodScores { get; set; }
    public DbSet<FootballMatchLineupPlayer> FootballMatchLineupPlayers { get; set; }
    public DbSet<FootballMatchEvent> FootballMatchEvents { get; set; }
    public DbSet<FootballGoal> FootballGoals { get; set; }
    public DbSet<FootballCard> FootballCards { get; set; }
    public DbSet<FootballSubstitution> FootballSubstitutions { get; set; }
    public DbSet<FootballTeamManager> FootballTeamManagers { get; set; }
    public DbSet<FootballTeamSeasonStatistics> FootballTeamSeasonStatistics { get; set; }
    public DbSet<FootballPlayerSeasonStatistics> FootballPlayerSeasonStatistics { get; set; }
    public DbSet<FootballMatchTeamStatistics> FootballMatchTeamStatistics { get; set; }
    public DbSet<FootballStatisticsCache> FootballStatisticsCache { get; set; }
    public DbSet<FootballCompetitionDivision> FootballCompetitionDivisions { get; set; }
    public DbSet<FootballCompetitionDivisionTeam> FootballCompetitionDivisionTeams { get; set; }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (_isDispatchingEvents)
            return await base.SaveChangesAsync(cancellationToken);

        return await this.SaveChangesWithEventsAsync(cancellationToken);
    }

    internal async Task<int> SaveChangesWithoutEventsAsync(CancellationToken cancellationToken = default)
    {
        _isDispatchingEvents = true;
        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            _isDispatchingEvents = false;
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("football");
        modelBuilder.Ignore<FootballPlayoffScheduleSlot>();
        modelBuilder.Ignore<FootballScore>();
        modelBuilder.Ignore<FootballLineupSelection>();

        modelBuilder.ApplyConfiguration(new FootballPlayerConfiguration());
        modelBuilder.ApplyConfiguration(new FootballTeamConfiguration());
        modelBuilder.ApplyConfiguration(new FootballTeamPlayerConfiguration());
        modelBuilder.ApplyConfiguration(new FootballMatchConfiguration());
        modelBuilder.ApplyConfiguration(new FootballCompetitionConfiguration());
        modelBuilder.ApplyConfiguration(new FootballTournamentConfiguration());
        modelBuilder.ApplyConfiguration(new FootballRefereeConfiguration());
        modelBuilder.ApplyConfiguration(new FootballPeriodScoreConfiguration());
        modelBuilder.ApplyConfiguration(new FootballMatchLineupPlayerConfiguration());
        modelBuilder.ApplyConfiguration(new FootballMatchEventConfiguration());
        modelBuilder.ApplyConfiguration(new FootballTeamManagerConfiguration());
        modelBuilder.ApplyConfiguration(new FootballGoalConfiguration());
        modelBuilder.ApplyConfiguration(new FootballCardConfiguration());
        modelBuilder.ApplyConfiguration(new FootballSubstitutionConfiguration());
        modelBuilder.ApplyConfiguration(new FootballTeamSeasonStatisticsConfiguration());
        modelBuilder.ApplyConfiguration(new FootballPlayerSeasonStatisticsConfiguration());
        modelBuilder.ApplyConfiguration(new FootballMatchTeamStatisticsConfiguration());
        modelBuilder.ApplyConfiguration(new FootballStatisticsCacheConfiguration());
        modelBuilder.ApplyConfiguration(new FootballCompetitionDivisionConfiguration());
        modelBuilder.ApplyConfiguration(new FootballCompetitionDivisionTeamConfiguration());
        modelBuilder.ApplyConfiguration(new FootballTournamentGroupConfiguration());
        modelBuilder.ApplyConfiguration(new FootballTournamentGroupTeamConfiguration());
    }
}
