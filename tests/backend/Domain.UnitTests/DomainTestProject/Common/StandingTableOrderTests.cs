using Domain.Enums.Common;
using Domain.Services.Common;

namespace DomainTestProject.Common;

public class StandingTableOrderTests
{
    private static readonly Guid AlphaId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid BravoId = Guid.Parse("00000000-0000-0000-0000-000000000002");
    private static readonly Guid CharlieId = Guid.Parse("00000000-0000-0000-0000-000000000003");

    private readonly record struct Row(
        Guid Id,
        string Name,
        int Points,
        int GoalDifference,
        int GoalsFor,
        int GoalsAgainst,
        int PenaltyMinutes = 0);

    private static readonly Row[] Rows =
    [
        new(AlphaId, "Alpha", 6, 2, 5, 3),
        new(BravoId, "Bravo", 6, 2, 5, 1),
        new(CharlieId, "Charlie", 9, 0, 4, 4)
    ];

    [Fact]
    public void Sort_DefaultCriteria_OrdersByPointsThenNameWhenTheRestIsTied()
    {
        List<Row> ordered = StandingTableOrder.Sort(
            Rows,
            StandingSortCriteria.Default,
            ToSnapshot);

        ordered.Select(row => row.Name).Should().Equal("Charlie", "Alpha", "Bravo");
    }

    [Fact]
    public void Sort_HeadToHead_UsesOnlyMatchesAmongTheTiedTeams()
    {
        List<StandingMatchResult> matches =
        [
            new(BravoId, AlphaId, 4, 1, 3, 0)
        ];

        List<Row> ordered = StandingTableOrder.Sort(
            Rows,
            StandingSortCriteria.Default,
            ToSnapshot,
            matches);

        ordered.Select(row => row.Name).Should().Equal("Charlie", "Bravo", "Alpha");
    }

    [Fact]
    public void Sort_HeadToHeadGoalDifference_BreaksASplitSeries()
    {
        List<Row> tied =
        [
            new(AlphaId, "Alpha", 6, 0, 5, 5),
            new(BravoId, "Bravo", 6, 0, 5, 5)
        ];
        List<StandingSortCriterion> criteria =
        [
            StandingSortCriterion.Points,
            StandingSortCriterion.GoalDifference,
            StandingSortCriterion.GoalsFor,
            StandingSortCriterion.HeadToHeadPoints,
            StandingSortCriterion.HeadToHeadGoalDifference
        ];
        List<StandingMatchResult> matches =
        [
            new(AlphaId, BravoId, 3, 1, 3, 0),
            new(BravoId, AlphaId, 4, 0, 3, 0)
        ];

        List<Row> ordered = StandingTableOrder.Sort(tied, criteria, ToSnapshot, matches);

        ordered.Select(row => row.Name).Should().Equal("Bravo", "Alpha");
    }

    [Fact]
    public void Sort_PenaltyMinutes_PrefersTheTeamWithFewer()
    {
        List<Row> rows =
        [
            new(AlphaId, "Alpha", 6, 0, 3, 3, 12),
            new(BravoId, "Bravo", 6, 0, 3, 3, 4)
        ];

        List<Row> ordered = StandingTableOrder.Sort(
            rows,
            [StandingSortCriterion.Points, StandingSortCriterion.PenaltyMinutes],
            ToSnapshot);

        ordered.Select(row => row.Name).Should().Equal("Bravo", "Alpha");
    }

    [Fact]
    public void Sort_Draw_KeepsTheSameOrderOnEveryCall()
    {
        List<Row> rows =
        [
            new(BravoId, "Bravo", 3, 0, 1, 1),
            new(AlphaId, "Alpha", 3, 0, 1, 1)
        ];

        List<Row> first = StandingTableOrder.Sort(rows, [StandingSortCriterion.Draw], ToSnapshot);
        List<Row> second = StandingTableOrder.Sort(rows, [StandingSortCriterion.Draw], ToSnapshot);

        first.Select(row => row.Name).Should().Equal("Alpha", "Bravo");
        second.Select(row => row.Name).Should().Equal(first.Select(row => row.Name));
    }

    private static StandingSortSnapshot ToSnapshot(Row row) =>
        new(row.Id, row.Points, row.GoalDifference, row.GoalsFor, row.GoalsAgainst, row.PenaltyMinutes, row.Name);
}
