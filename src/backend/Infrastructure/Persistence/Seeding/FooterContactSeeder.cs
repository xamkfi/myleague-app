using Domain.Entities.Common;
using Domain.Repositories.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MyLeague.Infrastructure.Persistence.Seeding;

/// <summary>
/// Seeds the current public footer contacts when none exist yet.
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

        IReadOnlyList<FooterContact> existing = await repository.GetAllAsync(cancellationToken);

        if (existing.Count > 0)
        {
            return;
        }

        FooterContact[] defaults =
        [
            new FooterContact(
                Guid.NewGuid(),
                "Mikkelin alueen harrasteliigat ry",
                "Savilahdenkatu 12 B 23\n50100 MIKKELI",
                null,
                null,
                null,
                0,
                "system"),
            new FooterContact(
                Guid.NewGuid(),
                "Seuratyöntekijä Pasi (asukasmiehet)",
                null,
                "pasi@mahl.fi",
                "044 209 9919",
                null,
                1,
                "system"),
            new FooterContact(
                Guid.NewGuid(),
                "Seuratyöntekijä Mikko Loukonen",
                null,
                "mikko@mahl.fi",
                "044 209 9919",
                null,
                2,
                "system"),
        ];

        foreach (FooterContact contact in defaults)
        {
            await repository.AddAsync(contact, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded {Count} footer contacts", defaults.Length);
    }
}
