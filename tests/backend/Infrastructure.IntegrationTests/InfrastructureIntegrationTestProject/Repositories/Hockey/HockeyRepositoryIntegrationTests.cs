using Domain.Entities.Hockey.Matches;
using Domain.Enums.Hockey.Matches;
using InfrastructureIntegrationTestProject.Common;

namespace InfrastructureIntegrationTestProject.Repositories.Hockey;

public class HockeyMatchRepositoryTests : HockeyIntegrationTestBase
{
    [Fact]
    public async Task AddScorekeeper_OnLoadedMatch_PersistsAfterReload()
    {
        HockeyMatch match = await SeedMatchAsync();
        Guid personId = Guid.NewGuid();

        HockeyMatch tracked = (await MatchRepository.GetByIdAsync(match.Id))!;
        tracked.AddScorekeeper(personId);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        HockeyMatch? reloaded = await MatchRepository.GetByIdAsync(match.Id);

        reloaded!.Scorekeepers.Should().ContainSingle(s => s.PersonId == personId);
    }

    [Fact]
    public async Task RemoveScorekeeper_OnLoadedMatch_PersistsAfterReload()
    {
        HockeyMatch match = await SeedMatchAsync();
        Guid personId = Guid.NewGuid();
        HockeyMatch withScorekeeper = (await MatchRepository.GetByIdAsync(match.Id))!;
        withScorekeeper.AddScorekeeper(personId);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        HockeyMatch tracked = (await MatchRepository.GetByIdAsync(match.Id))!;
        tracked.RemoveScorekeeper(personId);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        HockeyMatch? reloaded = await MatchRepository.GetByIdAsync(match.Id);

        reloaded!.Scorekeepers.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveChangesAsync_OnModifiedMatch_SetsUpdatedAt()
    {
        HockeyMatch match = await SeedMatchAsync();
        match.UpdatedAt.Should().BeNull();

        HockeyMatch tracked = (await MatchRepository.GetByIdAsync(match.Id))!;
        tracked.UpdateVenue("Hakametsä");
        DateTime before = DateTime.UtcNow;
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        HockeyMatch? reloaded = await MatchRepository.GetByIdAsync(match.Id);

        reloaded!.UpdatedAt.Should().NotBeNull();
        reloaded.UpdatedAt!.Value.Should().BeOnOrAfter(before);
    }

    private async Task<HockeyMatch> SeedMatchAsync()
    {
        HockeyMatch match = new(
            new DateTime(2026, 10, 1, 18, 0, 0, DateTimeKind.Utc),
            HockeyMatchType.Friendly);

        await MatchRepository.AddAsync(match);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();
        return match;
    }
}
