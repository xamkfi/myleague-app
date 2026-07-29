using Domain.Entities.Hockey.Competitions;
using Domain.Entities.Hockey.Matches;
using Domain.Entities.Hockey.Teams;
using Domain.ValueObjects.Hockey.Matches;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence.Configurations.Hockey;

namespace MyLeague.Infrastructure.Persistence.Contexts;

/// <summary>
/// Database context for hockey-specific entities.
/// </summary>
public class HockeyDbContext : DbContext
{
    public HockeyDbContext(DbContextOptions<HockeyDbContext> options) : base(options) { }

    public DbSet<HockeyTeam> HockeyTeams => Set<HockeyTeam>();
    public DbSet<HockeyPlayer> HockeyPlayers => Set<HockeyPlayer>();
    public DbSet<HockeyTeamPlayer> HockeyTeamPlayers => Set<HockeyTeamPlayer>();
    public DbSet<HockeyLine> HockeyLines => Set<HockeyLine>();
    public DbSet<HockeyLinePlayer> HockeyLinePlayers => Set<HockeyLinePlayer>();
    public DbSet<HockeyTeamStaff> HockeyTeamStaff => Set<HockeyTeamStaff>();
    public DbSet<HockeyOfficial> HockeyOfficials => Set<HockeyOfficial>();

    public DbSet<HockeyCompetition> HockeyCompetitions => Set<HockeyCompetition>();
    public DbSet<HockeySeason> HockeySeasons => Set<HockeySeason>();
    public DbSet<HockeyTournament> HockeyTournaments => Set<HockeyTournament>();
    public DbSet<HockeyCompetitionTeam> HockeyCompetitionTeams => Set<HockeyCompetitionTeam>();
    public DbSet<HockeyCompetitionDivision> HockeyCompetitionDivisions => Set<HockeyCompetitionDivision>();
    public DbSet<HockeyCompetitionDivisionTeam> HockeyCompetitionDivisionTeams => Set<HockeyCompetitionDivisionTeam>();
    public DbSet<HockeyTournamentGroup> HockeyTournamentGroups => Set<HockeyTournamentGroup>();
    public DbSet<HockeyTournamentGroupTeam> HockeyTournamentGroupTeams => Set<HockeyTournamentGroupTeam>();
    public DbSet<HockeyPlayoffSeries> HockeyPlayoffSeries => Set<HockeyPlayoffSeries>();
    public DbSet<HockeyMatch> HockeyMatches => Set<HockeyMatch>();
    public DbSet<HockeyMatchTeam> HockeyMatchTeams => Set<HockeyMatchTeam>();
    public DbSet<HockeyMatchOfficial> HockeyMatchOfficials => Set<HockeyMatchOfficial>();
    public DbSet<HockeyPeriodScore> HockeyPeriodScores => Set<HockeyPeriodScore>();
    public DbSet<HockeyMatchPlayerSelection> HockeyMatchPlayerSelections => Set<HockeyMatchPlayerSelection>();
    public DbSet<HockeyMatchActivePlayer> HockeyMatchActivePlayers => Set<HockeyMatchActivePlayer>();
    public DbSet<HockeyMatchLine> HockeyMatchLines => Set<HockeyMatchLine>();
    public DbSet<HockeyMatchLinePlayer> HockeyMatchLinePlayers => Set<HockeyMatchLinePlayer>();
    public DbSet<HockeyOnIceState> HockeyOnIceStates => Set<HockeyOnIceState>();
    public DbSet<HockeyOnIcePlayer> HockeyOnIcePlayers => Set<HockeyOnIcePlayer>();
    public DbSet<HockeyOnIceChange> HockeyOnIceChanges => Set<HockeyOnIceChange>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("hockey");
        modelBuilder.Ignore<HockeyPlayoffScheduleSlot>();

        modelBuilder.ApplyConfiguration(new HockeyTeamConfiguration());
        modelBuilder.ApplyConfiguration(new HockeyPlayerConfiguration());
        modelBuilder.ApplyConfiguration(new HockeyTeamPlayerConfiguration());
        modelBuilder.ApplyConfiguration(new HockeyLineConfiguration());
        modelBuilder.ApplyConfiguration(new HockeyLinePlayerConfiguration());
        modelBuilder.ApplyConfiguration(new HockeyTeamStaffConfiguration());
        modelBuilder.ApplyConfiguration(new HockeyOfficialConfiguration());

        modelBuilder.ApplyConfiguration(new HockeyCompetitionConfiguration());
        modelBuilder.ApplyConfiguration(new HockeySeasonConfiguration());
        modelBuilder.ApplyConfiguration(new HockeyTournamentConfiguration());
        modelBuilder.ApplyConfiguration(new HockeyCompetitionTeamConfiguration());
        modelBuilder.ApplyConfiguration(new HockeyCompetitionDivisionConfiguration());
        modelBuilder.ApplyConfiguration(new HockeyCompetitionDivisionTeamConfiguration());
        modelBuilder.ApplyConfiguration(new HockeyTournamentGroupConfiguration());
        modelBuilder.ApplyConfiguration(new HockeyTournamentGroupTeamConfiguration());
        modelBuilder.ApplyConfiguration(new HockeyPlayoffSeriesConfiguration());
        modelBuilder.ApplyConfiguration(new HockeyMatchConfiguration());
        modelBuilder.ApplyConfiguration(new HockeyMatchTeamConfiguration());
        modelBuilder.ApplyConfiguration(new HockeyMatchOfficialConfiguration());
        modelBuilder.ApplyConfiguration(new HockeyPeriodScoreConfiguration());
        modelBuilder.ApplyConfiguration(new HockeyMatchPlayerSelectionConfiguration());
        modelBuilder.ApplyConfiguration(new HockeyMatchActivePlayerConfiguration());
        modelBuilder.ApplyConfiguration(new HockeyMatchLineConfiguration());
        modelBuilder.ApplyConfiguration(new HockeyMatchLinePlayerConfiguration());
        modelBuilder.ApplyConfiguration(new HockeyOnIceStateConfiguration());
        modelBuilder.ApplyConfiguration(new HockeyOnIcePlayerConfiguration());
        modelBuilder.ApplyConfiguration(new HockeyOnIceChangeConfiguration());
    }
}
