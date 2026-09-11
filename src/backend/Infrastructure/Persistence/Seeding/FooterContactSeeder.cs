using Domain.Entities.Common;
using Domain.Enums.Common;
using Domain.Repositories.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MyLeague.Infrastructure.Persistence.Seeding;

/// <summary>
/// Seeds default public footer entries when a section is still empty.
/// </summary>
public static class FooterContactSeeder
{
    public static async Task SeedAsync(
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        IFooterContactRepository repository =
            scope.ServiceProvider.GetRequiredService<IFooterContactRepository>();
        IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        ILogger logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("FooterContactSeeder");

        int added = 0;
        added += await SeedSectionIfEmptyAsync(
            repository,
            FooterSection.Contact,
            CreateContacts(),
            cancellationToken);
        added += await SeedSectionIfEmptyAsync(
            repository,
            FooterSection.SeasonalSports,
            CreateSeasonalSports(),
            cancellationToken);
        added += await SeedSectionIfEmptyAsync(
            repository,
            FooterSection.OtherActivities,
            CreateOtherActivities(),
            cancellationToken);

        if (added == 0)
        {
            return;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded {Count} footer entries", added);
    }

    private static async Task<int> SeedSectionIfEmptyAsync(
        IFooterContactRepository repository,
        FooterSection section,
        FooterContact[] defaults,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<FooterContact> existing = await repository.GetAllAsync(section, cancellationToken);

        if (existing.Count > 0)
        {
            return 0;
        }

        foreach (FooterContact contact in defaults)
        {
            await repository.AddAsync(contact, cancellationToken);
        }

        return defaults.Length;
    }

    private static FooterContact[] CreateContacts()
    {
        return
        [
            new FooterContact(
                Guid.NewGuid(),
                "Mikkelin alueen harrasteliigat ry",
                "Savilahdenkatu 12 B 23\n50100 MIKKELI",
                null,
                null,
                null,
                0,
                FooterSection.Contact,
                "system"),
            new FooterContact(
                Guid.NewGuid(),
                "Seuratyöntekijä Pasi (asukasmiehet)",
                null,
                "pasi@mahl.fi",
                "044 209 9919",
                null,
                1,
                FooterSection.Contact,
                "system"),
            new FooterContact(
                Guid.NewGuid(),
                "Seuratyöntekijä Mikko Loukonen",
                null,
                "mikko@mahl.fi",
                "044 209 9919",
                null,
                2,
                FooterSection.Contact,
                "system"),
        ];
    }

    private static FooterContact[] CreateSeasonalSports()
    {
        string[] titles =
        [
            "Jalkapallo",
            "Jääkiekko",
            "Salibandy",
            "Salibandyn Manager",
            "Talvijalkapallo",
            "Jääpallo",
            "Puumalaliga",
            "Jääkiekko +40",
        ];

        return titles
            .Select((title, index) => new FooterContact(
                Guid.NewGuid(),
                title,
                null,
                null,
                null,
                null,
                index,
                FooterSection.SeasonalSports,
                "system"))
            .ToArray();
    }

    private static FooterContact[] CreateOtherActivities()
    {
        string[] titles =
        [
            "PMT Turnaukset",
            "Korttelitoiminta",
            "WHL Liikuntaleirit",
            "Turnauspiste",
        ];

        return titles
            .Select((title, index) => new FooterContact(
                Guid.NewGuid(),
                title,
                null,
                null,
                null,
                null,
                index,
                FooterSection.OtherActivities,
                "system"))
            .ToArray();
    }
}
