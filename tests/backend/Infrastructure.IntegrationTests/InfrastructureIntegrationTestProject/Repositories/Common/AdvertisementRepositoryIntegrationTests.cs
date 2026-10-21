using Domain.Entities.Common;
using Domain.Enums.Common;
using InfrastructureIntegrationTestProject.Common;
using MyLeague.Infrastructure.Persistence.Repositories.Common;

namespace InfrastructureIntegrationTestProject.Repositories.Common;

public class AdvertisementRepositoryIntegrationTests : BaseIntegrationTest
{
    private static readonly DateTime Now = new(2026, 10, 16, 12, 0, 0, DateTimeKind.Utc);

    private static Advertisement Ad(
        string name,
        AdSlot slot = AdSlot.LeftTop,
        bool isActive = true,
        int sortOrder = 0,
        DateTime? startsAt = null,
        DateTime? endsAt = null)
    {
        return new Advertisement(
            Guid.NewGuid(),
            name,
            new Uri($"https://cdn.example.com/{name}.png"),
            null,
            null,
            slot,
            isActive,
            sortOrder,
            startsAt,
            endsAt);
    }

    [Fact]
    public async Task GetVisibleAsync_ReturnsOnlyActiveAdsInsideTheirSchedule()
    {
        AdvertisementRepository repo = new(_dbContext);
        await repo.AddAsync(Ad("always"));
        await repo.AddAsync(Ad("inactive", isActive: false));
        await repo.AddAsync(Ad("expired", endsAt: Now.AddDays(-1)));
        await repo.AddAsync(Ad("future", startsAt: Now.AddDays(1)));
        await repo.AddAsync(Ad("running", startsAt: Now.AddDays(-1), endsAt: Now.AddDays(1)));
        await _dbContext.SaveChangesAsync();

        IReadOnlyList<Advertisement> visible = await repo.GetVisibleAsync(Now);

        visible.Select(x => x.Name).Should().BeEquivalentTo("always", "running");
    }

    [Fact]
    public async Task GetVisibleAsync_OrdersBySortOrderWithinSlot()
    {
        AdvertisementRepository repo = new(_dbContext);
        await repo.AddAsync(Ad("second", AdSlot.RightTop, sortOrder: 2));
        await repo.AddAsync(Ad("first", AdSlot.RightTop, sortOrder: 1));
        await _dbContext.SaveChangesAsync();

        IReadOnlyList<Advertisement> visible = await repo.GetVisibleAsync(Now);

        visible.Select(x => x.Name).Should().ContainInOrder("first", "second");
    }

    [Fact]
    public async Task GetAllAsync_IncludesInactiveAds()
    {
        AdvertisementRepository repo = new(_dbContext);
        await repo.AddAsync(Ad("active"));
        await repo.AddAsync(Ad("inactive", isActive: false));
        await _dbContext.SaveChangesAsync();

        IReadOnlyList<Advertisement> all = await repo.GetAllAsync();

        all.Should().HaveCount(2);
    }
}
