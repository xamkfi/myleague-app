using Application.Features.Floorball.Matches.Commands;
using Application.Features.Floorball.Matches.Validators;
using FluentValidation.TestHelper;

namespace ApplicationTestProject.Validators.Commands.FloorballMatches;

/// <summary>
/// Validator tests for <see cref="AssignMatchTeamsCommand"/>. The validator's job is to enforce
/// shape constraints (non-empty match id, non-empty team ids when provided, different home/away
/// when both are provided). Match-existence and status validation live in the handler.
/// </summary>
public class AssignMatchTeamsCommandValidatorTests
{
    private readonly AssignMatchTeamsCommandValidator _validator = new();

    [Fact]
    public void Validate_BothTeamsNull_PassesSoCallerCanClearSlots()
    {
        // Clearing both slots back to TBD is a legitimate use-case (admin reverts an incorrect
        // jury override); the validator must allow it.
        AssignMatchTeamsCommand command = new AssignMatchTeamsCommand(Guid.NewGuid(), null, null);

        TestValidationResult<AssignMatchTeamsCommand> result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_OnlyHomeProvided_PassesSoCallerCanFillSlots()
    {
        AssignMatchTeamsCommand command = new AssignMatchTeamsCommand(Guid.NewGuid(), Guid.NewGuid(), null);

        TestValidationResult<AssignMatchTeamsCommand> result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_DifferentHomeAndAway_Passes()
    {
        AssignMatchTeamsCommand command = new AssignMatchTeamsCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        TestValidationResult<AssignMatchTeamsCommand> result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyMatchId_Fails()
    {
        AssignMatchTeamsCommand command = new AssignMatchTeamsCommand(Guid.Empty, null, null);

        TestValidationResult<AssignMatchTeamsCommand> result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.MatchId);
    }

    [Fact]
    public void Validate_EmptyHomeTeamId_FailsWhenProvided()
    {
        // Distinguish "not provided" (null) from "provided but empty Guid" — the latter is a
        // common admin-tooling bug that the validator must catch.
        AssignMatchTeamsCommand command = new AssignMatchTeamsCommand(Guid.NewGuid(), Guid.Empty, null);

        TestValidationResult<AssignMatchTeamsCommand> result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("HomeTeamId.Value");
    }

    [Fact]
    public void Validate_SameHomeAndAway_Fails()
    {
        Guid teamId = Guid.NewGuid();
        AssignMatchTeamsCommand command = new AssignMatchTeamsCommand(Guid.NewGuid(), teamId, teamId);

        TestValidationResult<AssignMatchTeamsCommand> result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.AwayTeamId);
    }
}
