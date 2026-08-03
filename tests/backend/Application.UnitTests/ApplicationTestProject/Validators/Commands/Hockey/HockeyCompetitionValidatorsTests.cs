using Application.Features.Hockey.Competitions.Commands;
using Application.Features.Hockey.Competitions.Validators;
using Application.Features.Hockey.Seasons.Commands;
using Application.Features.Hockey.Seasons.Validators;
using Application.Features.Hockey.Tournaments.Commands;
using Application.Features.Hockey.Tournaments.Validators;
using FluentValidation.TestHelper;

namespace ApplicationTestProject.Validators.Commands.Hockey;

public class CreateHockeySeasonCommandValidatorTests
{
    private readonly CreateHockeySeasonCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_Passes()
    {
        CreateHockeySeasonCommand command = new(
            "Liiga 2026-27",
            new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2027, 4, 30, 0, 0, 0, DateTimeKind.Utc),
            "2026-27");

        TestValidationResult<CreateHockeySeasonCommand> result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyName_Fails()
    {
        CreateHockeySeasonCommand command = new(
            "",
            new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2027, 4, 30, 0, 0, 0, DateTimeKind.Utc));

        TestValidationResult<CreateHockeySeasonCommand> result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_EndBeforeStart_Fails()
    {
        CreateHockeySeasonCommand command = new(
            "Bad Season",
            new DateTime(2027, 4, 30, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc));

        TestValidationResult<CreateHockeySeasonCommand> result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.EndDate);
    }
}

public class CreateHockeyTournamentCommandValidatorTests
{
    private readonly CreateHockeyTournamentCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_Passes()
    {
        CreateHockeyTournamentCommand command = new(
            "Christmas Cup",
            new DateTime(2026, 12, 20, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 12, 28, 0, 0, 0, DateTimeKind.Utc),
            "Nokia Arena");

        TestValidationResult<CreateHockeyTournamentCommand> result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}

public class AddTeamToHockeyCompetitionCommandValidatorTests
{
    private readonly AddTeamToHockeyCompetitionCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_Passes()
    {
        AddTeamToHockeyCompetitionCommand command = new(Guid.NewGuid(), Guid.NewGuid(), 2);

        TestValidationResult<AddTeamToHockeyCompetitionCommand> result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyTeamId_Fails()
    {
        AddTeamToHockeyCompetitionCommand command = new(Guid.NewGuid(), Guid.Empty);

        TestValidationResult<AddTeamToHockeyCompetitionCommand> result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.TeamId);
    }
}
