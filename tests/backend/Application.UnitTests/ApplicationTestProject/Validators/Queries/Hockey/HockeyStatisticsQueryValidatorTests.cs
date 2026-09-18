using Application.Features.Hockey.Statistics.Queries;
using Application.Features.Hockey.Statistics.Validators;
using FluentValidation.TestHelper;

namespace ApplicationTestProject.Validators.Queries.Hockey;

public class HockeyStatisticsQueryValidatorTests
{
    [Fact]
    public void PlayerStats_PlayerIdWithoutTeamId_ShouldNotHaveValidationErrors()
    {
        GetHockeyPlayerCompetitionStatisticsQueryValidator validator = new();
        TestValidationResult<GetHockeyPlayerCompetitionStatisticsQuery> result =
            validator.TestValidate(new GetHockeyPlayerCompetitionStatisticsQuery(
                Guid.NewGuid(),
                PlayerId: Guid.NewGuid()));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void PlayerStats_TeamIdWithoutPlayerId_ShouldHaveValidationError()
    {
        GetHockeyPlayerCompetitionStatisticsQueryValidator validator = new();
        TestValidationResult<GetHockeyPlayerCompetitionStatisticsQuery> result =
            validator.TestValidate(new GetHockeyPlayerCompetitionStatisticsQuery(
                Guid.NewGuid(),
                TeamId: Guid.NewGuid()));

        result.ShouldHaveValidationErrorFor(query => query.PlayerId);
    }

    [Fact]
    public void GoalieStats_PlayerIdWithoutTeamId_ShouldNotHaveValidationErrors()
    {
        GetHockeyGoalieCompetitionStatisticsQueryValidator validator = new();
        TestValidationResult<GetHockeyGoalieCompetitionStatisticsQuery> result =
            validator.TestValidate(new GetHockeyGoalieCompetitionStatisticsQuery(
                Guid.NewGuid(),
                PlayerId: Guid.NewGuid()));

        result.ShouldNotHaveAnyValidationErrors();
    }
}
