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

    Task<HockeySeason?> GetSeasonByNameAsync(string name);

    Task<HockeyTournament?> GetTournamentByIdAsync(Guid id);

    Task<IReadOnlyList<HockeySeason>> GetAllSeasonsAsync();

    Task<IReadOnlyList<HockeyTournament>> GetAllTournamentsAsync();

    Task<HockeySeason?> GetSeasonWithContentBlocksAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<HockeySeason?> GetFeaturedSeasonWithContentBlocksAsync(
        CancellationToken cancellationToken = default);

    void MarkNewContentBlocksAdded(HockeySeason season, IReadOnlyCollection<Guid> existingBlockIds);

    /// <summary>
    /// Returns true when a hockey season with the given id exists.
    /// </summary>
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a hockey season for deletion and removes division-team rows that would
    /// otherwise block the competition-team cascade.
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
