using Domain.Entities.Common;
using Domain.Enums.Common;
using Domain.Repositories.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MyLeague.Infrastructure.Persistence.Seeding;

/// <summary>
/// Seeds a few published Finnish news articles when the table is empty.
/// </summary>
public static class NewsArticleSeeder
{
    public static async Task SeedAsync(
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        INewsArticleRepository repository =
            scope.ServiceProvider.GetRequiredService<INewsArticleRepository>();
        IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        ILogger logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("NewsArticleSeeder");

        int existingCount = await repository.GetCountAsync(
            includeArchived: true,
            cancellationToken: cancellationToken);
        if (existingCount > 0)
        {
            return;
        }

        NewsArticle welcome = CreateArticle(
            "Tervetuloa MAHL:n sivuille",
            "Mikkelin alueen harrasteliigojen ottelut, taulukot ja uutiset löytyvät nyt yhdestä paikasta.",
            "<p>Tervetuloa Mikkelin alueen harrasteliigojen sivuille. Täältä seuraat salibandyn, jalkapallon ja jääkiekon otteluita, taulukoita ja uutisia.</p><p>Valitse yläpalkista ikäryhmäsi, niin näet sinulle suunnatun sisällön. Ilmoittautumiset ja yhteystiedot löytyvät alatunnisteesta.</p>",
            NewsCategory.Announcements,
            SportsCategory.None,
            ["MAHL"]);

        NewsArticle floorball = CreateArticle(
            "Salibandykausi on käynnissä",
            "Salibandyn otteluohjelma ja taulukko päivittyvät otteluiden edetessä.",
            "<p>Salibandykausi on käynnissä. Otteluohjelman, tulokset ja sarjataulukon löydät salibandy-sivulta ja tapahtumakalenterista.</p><p>Muista tarkistaa pelipaikka ja alkamisaika ennen ottelua. Yleiset pelisäännöt ovat Säännöt-sivulla.</p>",
            NewsCategory.LeagueNews,
            SportsCategory.Floorball,
            ["MAHL", "salibandy"]);

        NewsArticle football = CreateArticle(
            "Jalkapallokausi jatkuu",
            "Jalkapallon ottelut, tulokset ja taulukko ovat saatavilla jalkapallo-sivulla.",
            "<p>Jalkapallokausi jatkuu. Seuraa otteluita jalkapallo-sivulla ja tapahtumakalenterissa.</p><p>Joukkueiden kokoonpanot ja pelaajatiedot päivittyvät hallinnan kautta. Kysymykset voi lähettää seuran työntekijöille.</p>",
            NewsCategory.LeagueNews,
            SportsCategory.Football,
            ["MAHL", "jalkapallo"]);

        await repository.CreateNews(welcome);
        await repository.CreateNews(floorball);
        await repository.CreateNews(football);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded {Count} default news articles", 3);
    }

    private static NewsArticle CreateArticle(
        string title,
        string summary,
        string contentHtml,
        NewsCategory category,
        SportsCategory sportCategory,
        IReadOnlyList<string> tags)
    {
        NewsArticle article = new(Guid.NewGuid(), title, null, contentHtml, "MAHL");
        article.UpdateContent(title, contentHtml, summary);
        article.SetCategory(category);
        if (sportCategory != SportsCategory.None)
        {
            article.SetSportCategory(sportCategory);
        }

        foreach (string tag in tags)
        {
            article.AddTag(tag);
        }

        return article;
    }
}
