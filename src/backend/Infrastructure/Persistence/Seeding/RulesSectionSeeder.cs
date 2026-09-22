using Domain.Entities.Common;
using Domain.Enums.Common;
using Domain.Repositories.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MyLeague.Infrastructure.Persistence.Seeding;

/// <summary>
/// Seeds public rules sections when the table is empty.
/// </summary>
public static class RulesSectionSeeder
{
    public static async Task SeedAsync(
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        IRulesSectionRepository repository =
            scope.ServiceProvider.GetRequiredService<IRulesSectionRepository>();
        IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        ILogger logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("RulesSectionSeeder");

        IReadOnlyList<RulesSection> existing = await repository.GetAllAsync(cancellationToken);
        if (existing.Count > 0)
        {
            return;
        }

        Guid generalId = Guid.NewGuid();
        Guid sportGroupId = Guid.NewGuid();
        Guid floorballId = Guid.NewGuid();
        Guid footballId = Guid.NewGuid();
        Guid hockeyId = Guid.NewGuid();

        RulesSection[] sections =
        [
            new RulesSection(
                generalId,
                "Yleissäännöt",
                0,
                RulesSectionType.Global,
                null,
                string.Concat(
                    RuleBlock(1, "<p>Kaikkia osallistujia koskevat samat pelisäännöt ja kunnioittava käytös.</p>"),
                    RuleBlock(2, "<p>Ottelut pelataan ilmoitetun aikataulun mukaan. Myöhästymisistä ilmoitetaan vastustajalle ja tuomarille.</p>"),
                    RuleBlock(3, "<p>Häirintään tai sääntörikkomuksiin voi puuttua ottamalla yhteyttä seuran työntekijään.</p>")),
                "system"),
            new RulesSection(
                sportGroupId,
                "Lajisäännöt",
                1,
                RulesSectionType.SportGroup,
                null,
                string.Empty,
                "system"),
            new RulesSection(
                floorballId,
                "Salibandy",
                0,
                RulesSectionType.Sport,
                sportGroupId,
                RuleBlock(1, "<p>Salibandyotteluissa noudatetaan MAHL:n yleissääntöjä ja ottelukohtaisia eräsääntöjä.</p>"),
                "system"),
            new RulesSection(
                footballId,
                "Jalkapallo",
                1,
                RulesSectionType.Sport,
                sportGroupId,
                RuleBlock(1, "<p>Jalkapallo-otteluissa noudatetaan MAHL:n yleissääntöjä sekä kauden peliaika- ja erikoissääntöjä.</p>"),
                "system"),
            new RulesSection(
                hockeyId,
                "Jääkiekko",
                2,
                RulesSectionType.Sport,
                sportGroupId,
                RuleBlock(1, "<p>Jääkiekko-otteluissa noudatetaan MAHL:n yleissääntöjä sekä erä- ja jäähykäytäntöjä.</p>"),
                "system"),
        ];

        foreach (RulesSection section in sections)
        {
            await repository.AddAsync(section, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded {Count} default rules sections", sections.Length);
    }

    private static string RuleBlock(int order, string html)
    {
        return $"""<div class="rules-item" data-rule-id="{Guid.NewGuid()}" data-rule-order="{order}">{html}</div>""";
    }
}
