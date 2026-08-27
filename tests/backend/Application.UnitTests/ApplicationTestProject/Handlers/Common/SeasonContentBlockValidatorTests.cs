using Application.Features.Common.SeasonContentBlocks.Commands;
using Application.Features.Common.SeasonContentBlocks.Queries;
using Application.Features.Common.SeasonContentBlocks.Validators;
using Domain.Enums.Common;
using FluentValidation.TestHelper;

namespace ApplicationTestProject.Handlers.Common;

public class SeasonContentBlockValidatorTests
{
    [Fact]
    public void Create_ValidCommand_HasNoErrors()
    {
        CreateSeasonContentBlockCommandValidator validator = new();
        CreateSeasonContentBlockCommand command = new(
            SportsCategory.Floorball,
            Guid.NewGuid(),
            "2025-2026",
            "Sarjainfo",
            "<p>Info</p>",
            0,
            "admin");

        TestValidationResult<CreateSeasonContentBlockCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(SportsCategory.None)]
    public void Create_InvalidSport_HasError(SportsCategory sport)
    {
        CreateSeasonContentBlockCommandValidator validator = new();
        CreateSeasonContentBlockCommand command = new(
            sport,
            Guid.NewGuid(),
            "2025-2026",
            "Title",
            "<p>x</p>",
            0,
            null);

        TestValidationResult<CreateSeasonContentBlockCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Sport);
    }

    [Fact]
    public void GetAll_WithoutFilters_HasError()
    {
        GetAllSeasonContentBlocksQueryValidator validator = new();

        TestValidationResult<GetAllSeasonContentBlocksQuery> result = validator.TestValidate(
            new GetAllSeasonContentBlocksQuery(null, null, null));

        result.ShouldHaveValidationErrorFor(x => x);
    }

    [Fact]
    public void GetAll_CompetitionId_HasNoErrors()
    {
        GetAllSeasonContentBlocksQueryValidator validator = new();

        TestValidationResult<GetAllSeasonContentBlocksQuery> result = validator.TestValidate(
            new GetAllSeasonContentBlocksQuery(Guid.NewGuid(), null, null));

        result.ShouldNotHaveAnyValidationErrors();
    }
}
