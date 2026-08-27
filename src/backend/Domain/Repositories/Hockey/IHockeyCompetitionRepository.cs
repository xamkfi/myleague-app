using Domain.Entities.Hockey.Competitions;

namespace Domain.Repositories.Hockey;

/// <summary>
/// Repository for hockey competitions (seasons and tournaments).
/// </summary>
public interface IHockeyCompetitionRepository
{
    Task AddAsync(HockeyCompetition competition);

    Task<HockeyCompetition?> GetByIdAsync(Guid id);

    Task<HockeySeason?> GetSeasonByIdAsync(Guid id);

    Task<HockeyTournament?> GetTournamentByIdAsync(Guid id);

    Task<IReadOnlyList<HockeySeason>> GetAllSeasonsAsync();

    Task<IReadOnlyList<HockeyTournament>> GetAllTournamentsAsync();

    Task<HockeySeason?> GetSeasonWithContentBlocksAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<HockeySeason?> GetFeaturedSeasonWithContentBlocksAsync(
        CancellationToken cancellationToken = default);
}
