using Application.Features.Hockey.Competitions.Commands;
using Application.Features.Hockey.Competitions.Validators;
using Application.Features.Hockey.Seasons.Commands;
using Application.Features.Hockey.Seasons.Validators;
using Application.Features.Hockey.Tournaments.Commands;
using Application.Features.Hockey.Tournaments.Validators;
using Domain.Enums.Common;
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

public class CreateHockeyTournamentGroupCommandValidatorTests
{
    private readonly CreateHockeyTournamentGroupCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_Passes()
    {
        CreateHockeyTournamentGroupCommand command = new(Guid.NewGuid(), "A-lohko");

        TestValidationResult<CreateHockeyTournamentGroupCommand> result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyName_Fails()
    {
        CreateHockeyTournamentGroupCommand command = new(Guid.NewGuid(), "");

        TestValidationResult<CreateHockeyTournamentGroupCommand> result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_EmptyTournamentId_Fails()
    {
        CreateHockeyTournamentGroupCommand command = new(Guid.Empty, "A-lohko");

        TestValidationResult<CreateHockeyTournamentGroupCommand> result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.TournamentId);
    }
}

public class AddTeamToHockeyTournamentGroupCommandValidatorTests
{
    private readonly AddTeamToHockeyTournamentGroupCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_Passes()
    {
        AddTeamToHockeyTournamentGroupCommand command = new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1);

        TestValidationResult<AddTeamToHockeyTournamentGroupCommand> result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyCompetitionTeamId_Fails()
    {
        AddTeamToHockeyTournamentGroupCommand command = new(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty);

        TestValidationResult<AddTeamToHockeyTournamentGroupCommand> result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.CompetitionTeamId);
    }
}

public class UpdateHockeyTournamentCommandValidatorTests
{
    private readonly UpdateHockeyTournamentCommandValidator _validator = new();

    [Fact]
    public void Validate_EndBeforeStart_Fails()
    {
        UpdateHockeyTournamentCommand command = new(
            Guid.NewGuid(),
            "Cup",
            new DateTime(2026, 12, 10, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 12, 1, 0, 0, 0, DateTimeKind.Utc),
            null,
            null,
            TeamCategory.Adult);

        TestValidationResult<UpdateHockeyTournamentCommand> result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.EndDate);
    }
}

public class CreateHockeyPlayoffSeriesCommandValidatorTests
{
    private readonly CreateHockeyPlayoffSeriesCommandValidator _validator = new();

    [Fact]
    public void Validate_BestOfZero_Fails()
    {
        CreateHockeyPlayoffSeriesCommand command = new(
            Guid.NewGuid(),
            Domain.Enums.Hockey.Competitions.HockeyPlayoffRound.Final,
            0,
            0);

        TestValidationResult<CreateHockeyPlayoffSeriesCommand> result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.BestOf);
    }
}
