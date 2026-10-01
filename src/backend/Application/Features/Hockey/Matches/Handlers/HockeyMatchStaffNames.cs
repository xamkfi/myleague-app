using Application.Features.Common.Shared;
using Domain.Entities.Common;
using Domain.Entities.Hockey.Matches;
using Domain.Entities.Hockey.Officials;
using Domain.Repositories.Common;
using Domain.Repositories.Hockey;

namespace Application.Features.Hockey.Matches.Handlers;

/// <summary>
/// Resolves display names for hockey match officials (official ID → person) and scorekeepers (person ID).
/// </summary>
internal static class HockeyMatchStaffNames
{
    public static async Task<(Dictionary<Guid, string> OfficialNames, Dictionary<Guid, string> ScorekeeperNames)> LoadAsync(
        HockeyMatch match,
        IHockeyOfficialRepository officialRepository,
        IPersonRepository personRepository)
    {
        Dictionary<Guid, Guid> officialPersonIds = new();
        foreach (Guid officialId in match.Officials.Select(o => o.OfficialId).Distinct())
        {
            HockeyOfficial? official = await officialRepository.GetByIdAsync(officialId);
            if (official is not null)
                officialPersonIds[officialId] = official.PersonId;
        }

        Dictionary<Guid, Person> persons = await MatchPersonLookup.LoadAsync(
            personRepository,
            officialPersonIds.Values.Concat(match.Scorekeepers.Select(s => s.PersonId)));

        Dictionary<Guid, string> officialNames = officialPersonIds.ToDictionary(
            pair => pair.Key,
            pair => MatchPersonLookup.NameOf(pair.Value, persons));
        Dictionary<Guid, string> scorekeeperNames = match.Scorekeepers.ToDictionary(
            s => s.PersonId,
            s => MatchPersonLookup.NameOf(s.PersonId, persons));

        return (officialNames, scorekeeperNames);
    }
}
