using Domain.Entities.Common;
using Domain.Enums.Common;
using InfrastructureIntegrationTestProject.Common;
using MyLeague.Infrastructure.Persistence.Repositories.Common;

namespace InfrastructureIntegrationTestProject.Repositories.Common;

public class SiteContentRepositoryIntegrationTests : BaseIntegrationTest
{
    [Fact]
    public async Task InfoPageContent_RoundTripsBySlug()
    {
        InfoPageContentRepository repo = new(_dbContext);
        InfoPageContent page = new(Guid.NewGuid(), "contact", "Contact", "<p>Email us</p>", "admin");

        await repo.AddAsync(page);
        await _dbContext.SaveChangesAsync();

        InfoPageContent? loaded = await repo.GetBySlugAsync("contact");

        loaded.Should().NotBeNull();
        loaded!.Title.Should().Be("Contact");
        loaded.ContentHtml.Should().Be("<p>Email us</p>");
    }

    [Fact]
    public async Task RulesSection_RoundTripsById()
    {
        RulesSectionRepository repo = new(_dbContext);
        RulesSection section = new(Guid.NewGuid(), "Yleiset", 1, RulesSectionType.Global, contentHtml: "");

        await repo.AddAsync(section);
        await _dbContext.SaveChangesAsync();

        RulesSection? loaded = await repo.GetByIdAsync(section.Id);

        loaded.Should().NotBeNull();
        loaded!.Title.Should().Be("Yleiset");
        loaded.SectionType.Should().Be(RulesSectionType.Global);
    }

    [Fact]
    public async Task Division_RoundTrips()
    {
        DivisionRepository repo = new(_dbContext);
        Division division = new("Championship", "Top level", 1, SportsCategory.Floorball);

        await repo.AddAsync(division);
        await _dbContext.SaveChangesAsync();

        Division? loaded = await repo.GetByIdAsync(division.Id);

        loaded.Should().NotBeNull();
        loaded!.Name.Should().Be("Championship");
        loaded.SportType.Should().Be(SportsCategory.Floorball);
    }
}
