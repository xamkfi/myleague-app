using Application.Features.Hockey.Matches.Commands;
using Application.Features.Hockey.Matches.Validators;
using Domain.Enums.Hockey.Matches;
using FluentValidation.TestHelper;

namespace ApplicationTestProject.Validators.Commands.Hockey;

public class RecordHockeyShotCommandValidatorTests
{
    private readonly RecordHockeyShotCommandValidator _validator = new();

    private static RecordHockeyShotCommand Command(
        HockeyShotResult shotResult = HockeyShotResult.Saved,
        Guid? goalieId = null,
        int count = 1) =>
        new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            PeriodNumber: 1,
            TimeInSeconds: 60,
            shotResult,
            CountsAsShotOnGoal: true,
            GoalieActivePlayerId: goalieId,
            Count: count);

    [Fact]
    public void Validate_BulkSavesWithGoalie_Passes()
    {
        TestValidationResult<RecordHockeyShotCommand> result =
            _validator.TestValidate(Command(goalieId: Guid.NewGuid(), count: 20));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_BulkWithoutSavedResult_Fails()
    {
        TestValidationResult<RecordHockeyShotCommand> result =
            _validator.TestValidate(Command(HockeyShotResult.Missed, Guid.NewGuid(), count: 5));

        result.ShouldHaveValidationErrorFor(x => x.ShotResult);
    }

    [Fact]
    public void Validate_BulkWithoutGoalie_Fails()
    {
        TestValidationResult<RecordHockeyShotCommand> result =
            _validator.TestValidate(Command(count: 5));

        result.ShouldHaveValidationErrorFor(x => x.GoalieActivePlayerId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public void Validate_CountOutOfRange_Fails(int count)
    {
        TestValidationResult<RecordHockeyShotCommand> result =
            _validator.TestValidate(Command(goalieId: Guid.NewGuid(), count: count));

        result.ShouldHaveValidationErrorFor(x => x.Count);
    }
}
