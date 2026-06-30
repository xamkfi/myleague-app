using Domain.Entities.Common;
using Domain.Repositories.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MyLeague.Infrastructure.Persistence.Seeding;

/// <summary>
/// Seeds default MAHL info page content entries
/// </summary>
public static class InfoPageContentSeeder
{
    private static readonly (string Slug, string Title, string ContentHtml)[] DefaultPages =
    [
        ("mahl-summary", "Summary", "<p>MAHL summary content.</p>"),
        ("mahl-finance", "Seuran talous", "<p>Seuran talous content.</p>"),
        ("mahl-partners", "Kumppanuudet", "<p>Kumppanuudet content.</p>"),
        ("mahl-responsibility", "Vastuullisuus", "<p>Vastuullisuus content.</p>"),
    ];

    /// <summary>
    /// Seeds default info page content if not already present
    /// </summary>
    /// <param name="serviceProvider">The service provider</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public static async Task SeedAsync(
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        IInfoPageContentRepository repository =
            scope.ServiceProvider.GetRequiredService<IInfoPageContentRepository>();
        IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        ILogger logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("InfoPageContentSeeder");

        foreach ((string slug, string title, string contentHtml) in DefaultPages)
        {
            bool exists = await repository.ExistsBySlugAsync(slug, cancellationToken);

            if (exists)
            {
                continue;
            }

            await repository.AddAsync(
                new InfoPageContent(Guid.NewGuid(), slug, title, contentHtml, "system"),
                cancellationToken);

            logger.LogInformation("Seeded info page content slug {Slug}", slug);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
