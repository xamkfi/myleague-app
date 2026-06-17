using Application.Interfaces.Common;
using Domain.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MyLeague.Infrastructure.Persistence.Seeding;

public static class InfoPageContentSeeder
{
    private static readonly (string Slug, string Title, string ContentHtml)[] DefaultPages =
    [
        ("mahl-summary", "Summary", "<p>MAHL summary content.</p>"),
        ("mahl-finance", "Seuran talous", "<p>Seuran talous content.</p>"),
        ("mahl-partners", "Kumppanuudet", "<p>Kumppanuudet content.</p>"),
        ("mahl-responsibility", "Vastuullisuus", "<p>Vastuullisuus content.</p>"),
    ];

    public static async Task SeedAsync(
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        ICommonDbContext context = scope.ServiceProvider.GetRequiredService<ICommonDbContext>();
        ILogger logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("InfoPageContentSeeder");

        foreach ((string slug, string title, string contentHtml) in DefaultPages)
        {
            bool exists = await context.InfoPageContents
                .AnyAsync(x => x.PageSlug == slug, cancellationToken);

            if (exists)
            {
                continue;
            }

            context.InfoPageContents.Add(
                new InfoPageContent(Guid.NewGuid(), slug, title, contentHtml, "system"));

            logger.LogInformation("Seeded info page content slug {Slug}", slug);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
