using JoomleagueImporter.Import;

namespace JoomleagueImporter.UnitTests;

public class ClaimJerseyTests
{
    [Fact]
    public void ClaimJersey_DuplicatePreferred_AssignsNextFreeNumber()
    {
        HashSet<int> claimed = [];

        int first = HistoricalRosterApplicator.ClaimJersey(7, claimed);
        int second = HistoricalRosterApplicator.ClaimJersey(7, claimed);

        first.Should().Be(7);
        second.Should().Be(1);
    }

    [Fact]
    public void ClaimJersey_MissingPreferred_AssignsLowestFreeNumber()
    {
        HashSet<int> claimed = [1];

        int jersey = HistoricalRosterApplicator.ClaimJersey(null, claimed);

        jersey.Should().Be(2);
    }
}
