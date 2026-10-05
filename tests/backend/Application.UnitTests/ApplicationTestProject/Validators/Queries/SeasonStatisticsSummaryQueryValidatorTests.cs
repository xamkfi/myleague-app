using Application.Features.Floorball.Statistics.Queries;
using Application.Features.Floorball.Statistics.Validators;
using Application.Features.Football.Statistics.Queries;
using Application.Features.Football.Statistics.Validators;
using FluentValidation.TestHelper;

namespace ApplicationTestProject.Validators.Queries;

public class SeasonStatisticsSummaryQueryValidatorTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public void Floorball_TopNInRange_ShouldNotHaveValidationErrors(int topN)
    {
        GetFloorballSeasonStatisticsSummaryQueryValidator validator = new();
        TestValidationResult<GetFloorballSeasonStatisticsSummaryQuery> result =
            validator.TestValidate(new GetFloorballSeasonStatisticsSummaryQuery(Guid.NewGuid(), topN));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Floorball_TopNOutOfRange_ShouldHaveValidationError(int topN)
    {
        GetFloorballSeasonStatisticsSummaryQueryValidator validator = new();
        TestValidationResult<GetFloorballSeasonStatisticsSummaryQuery> result =
            validator.TestValidate(new GetFloorballSeasonStatisticsSummaryQuery(Guid.NewGuid(), topN));

        result.ShouldHaveValidationErrorFor(query => query.TopN);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Football_TopNOutOfRange_ShouldHaveValidationError(int topN)
    {
        GetFootballSeasonStatisticsSummaryQueryValidator validator = new();
        TestValidationResult<GetFootballSeasonStatisticsSummaryQuery> result =
            validator.TestValidate(new GetFootballSeasonStatisticsSummaryQuery(Guid.NewGuid(), topN));

        result.ShouldHaveValidationErrorFor(query => query.TopN);
    }

    [Fact]
    public void Football_EmptyCompetitionId_ShouldHaveValidationError()
    {
        GetFootballSeasonStatisticsSummaryQueryValidator validator = new();
        TestValidationResult<GetFootballSeasonStatisticsSummaryQuery> result =
            validator.TestValidate(new GetFootballSeasonStatisticsSummaryQuery(Guid.Empty));

        result.ShouldHaveValidationErrorFor(query => query.CompetitionId);
    }
}
