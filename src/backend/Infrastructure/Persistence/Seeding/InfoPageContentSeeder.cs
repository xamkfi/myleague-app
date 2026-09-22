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
    private static readonly (string Slug, string OldHtml)[] PlaceholderHtml =
    [
        ("mahl-summary", "<p>MAHL summary content.</p>"),
        ("mahl-finance", "<p>Seuran talous content.</p>"),
        ("mahl-partners", "<p>Kumppanuudet content.</p>"),
        ("mahl-responsibility", "<p>Vastuullisuus content.</p>"),
    ];

    private static readonly (string Slug, string Title, string ContentHtml)[] DefaultPages =
    [
        (
            "mahl-summary",
            "Yhteenveto",
            "<p>Mikkelin alueen harrasteliigat ry (MAHL) järjestää matalan kynnyksen harrasteliigatoimintaa salibandyssa, jalkapallossa ja jääkiekossa. Sivustolta löydät ottelut, taulukot, uutiset ja seuran yhteystiedot.</p><p>Toiminta on tarkoitettu aikuisille, nuorille ja naisille. Valitse yläpalkista oma ikäryhmäsi, niin näet sinulle suunnatut sarjat ja uutiset.</p>"),
        (
            "mahl-finance",
            "Seuran talous",
            "<p>MAHL on voittoa tavoittelematon yhdistys. Toiminta rahoitetaan osallistumismaksuilla, avustuksilla ja kumppanuuksilla. Tavoitteena on pitää harrastamisen hinta kohtuullisena.</p><p>Ajantasaiset maksut ja tuet julkaistaan tällä sivulla ja uutisissa. Lisätietoja saat seuran työntekijöiltä alatunnisteen yhteystiedoista.</p>"),
        (
            "mahl-partners",
            "Kumppanuudet",
            "<p>MAHL tekee yhteistyötä paikallisten seurojen, oppilaitosten ja yritysten kanssa. Kumppanit mahdollistavat vuorot, välineet ja tapahtumat.</p><p>Jos haluat tukea harrasteliigatoimintaa, ota yhteyttä seuran työntekijöihin. Julkaisemme kumppanit tällä sivulla sitä mukaa kun sopimukset vahvistuvat.</p>"),
        (
            "mahl-responsibility",
            "Vastuullisuus",
            "<p>MAHL sitoutuu turvalliseen, yhdenvertaiseen ja päihteettömään harrastusympäristöön. Kaikkia osallistujia koskevat samat säännöt ja kunnioittava käytös.</p><p>Häirintään, syrjintään tai sääntörikkomuksiin voi puuttua ottamalla yhteyttä seuran työntekijään. Yleiset ja lajikohtaiset säännöt löytyvät Säännöt-sivulta.</p>"),
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

        int changed = 0;
        foreach ((string slug, string title, string contentHtml) in DefaultPages)
        {
            InfoPageContent? existing = await repository.GetBySlugAsync(slug, cancellationToken);
            if (existing is null)
            {
                await repository.AddAsync(
                    new InfoPageContent(Guid.NewGuid(), slug, title, contentHtml, "system"),
                    cancellationToken);
                changed++;
                logger.LogInformation("Seeded info page content slug {Slug}", slug);
                continue;
            }

            string? oldHtml = PlaceholderHtml
                .FirstOrDefault(placeholder => placeholder.Slug == slug)
                .OldHtml;
            bool isPlaceholder = !string.IsNullOrWhiteSpace(oldHtml)
                && string.Equals(existing.ContentHtml.Trim(), oldHtml, StringComparison.Ordinal);

            if (!isPlaceholder)
            {
                continue;
            }

            existing.UpdateContent(title, contentHtml, "system");
            await repository.UpdateAsync(existing, cancellationToken);
            changed++;
            logger.LogInformation("Replaced placeholder info page content slug {Slug}", slug);
        }

        if (changed > 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
