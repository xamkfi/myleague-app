using Domain.Enums.Common;
using JoomleagueImporter.Import;
using JoomleagueImporter.Models;

namespace JoomleagueImporter.UnitTests;

public class ImportSeriesResolverTests
{
    [Theory]
    [InlineData("SALIBANDY PMT 2026 | MIEHET", "Salibandy PMT", TeamCategory.Adult, true)]
    [InlineData("SALIBANDY PMT 2026 | NAISET", "Salibandy PMT", TeamCategory.Women, true)]
    [InlineData("2025-2026 | SALIBANDY LIIGA", "Salibandy Liiga", TeamCategory.Adult, false)]
    [InlineData("2025 | SALIBANDY DIVARI", "Salibandy Divari", TeamCategory.Adult, false)]
    [InlineData("SALIBANDY DIVARI PLAYOFF", "Salibandy Playoff", TeamCategory.Adult, true)]
    [InlineData("2025-2026 | JALKAPALLO TALVISARJA", "Jalkapallo Talvisarja", TeamCategory.Adult, false)]
    [InlineData("2025 | MAHL JALKAPALLO RAUTALIIGA", "Jalkapallo Rautaliiga", TeamCategory.Adult, false)]
    [InlineData("2025-2026 | JÄÄKIEKKO LIIGA", "Jääkiekko Liiga", TeamCategory.Adult, false)]
    [InlineData("JÄÄKIEKKO +40", "Jääkiekko +40", TeamCategory.Adult, false)]
    [InlineData("SALIBANDY JUNIORIT", "Salibandy Sarja", TeamCategory.Youth, false)]
    public void Resolve_ProjectName_MapsBandAndAudience(
        string projectName,
        string divisionName,
        TeamCategory category,
        bool isTournament)
    {
        string sport = divisionName.Split(' ')[0];
        ImportSeriesProfile profile = ImportSeriesResolver.Resolve(projectName, sport);

        profile.DivisionName.Should().Be(divisionName);
        profile.Category.Should().Be(category);
        profile.IsTournament.Should().Be(isTournament);
    }

    [Fact]
    public void HomeForTeam_PrefersLatestLeagueOverLaterCup()
    {
        FloorballImportSet set = new();
        set.Projects.Add(Project(1, "2024 | SALIBANDY LIIGA", new DateTime(2024, 9, 1), teamId: 7));
        set.Projects.Add(Project(2, "SALIBANDY PMT 2025 | MIEHET", new DateTime(2025, 4, 1), teamId: 7));

        ImportSeriesProfile home = ImportSeriesResolver.HomeForTeam(set, 7, "Salibandy");

        home.DivisionName.Should().Be("Salibandy Liiga");
        home.IsTournament.Should().BeFalse();
    }

    [Fact]
    public void HomeForTeam_UsesTournamentWhenTeamHasNoLeague()
    {
        FloorballImportSet set = new();
        set.Projects.Add(Project(3, "SALIBANDY PMT 2026 | NAISET", new DateTime(2026, 4, 23), teamId: 9));

        ImportSeriesProfile home = ImportSeriesResolver.HomeForTeam(set, 9, "Salibandy");

        home.DivisionName.Should().Be("Salibandy PMT");
        home.Category.Should().Be(TeamCategory.Women);
        home.IsTournament.Should().BeTrue();
    }

    private static ProjectImport Project(int projectId, string name, DateTime start, int teamId)
    {
        ProjectImport project = new()
        {
            Project = new OldProject { Id = projectId, Name = name, StartDate = start },
        };
        project.Teams[projectId] = new ProjectTeamImport
        {
            ProjectTeam = new OldProjectTeam { Id = projectId, ProjectId = projectId, TeamId = teamId },
            Team = new OldTeam { Id = teamId, Name = "Xamk" },
        };
        return project;
    }
}
