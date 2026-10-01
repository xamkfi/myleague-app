using Domain.Entities.Common;
using Domain.Repositories.Common;

namespace Application.Features.Common.Shared;

/// <summary>
/// Resolves person names for match referees and scorekeepers. Persons live in the common
/// context, so sport handlers load them by ID instead of through navigations.
/// </summary>
public static class MatchPersonLookup
{
    /// <summary>
    /// Loads the given persons into a dictionary keyed by person ID.
    /// </summary>
    public static async Task<Dictionary<Guid, Person>> LoadAsync(IPersonRepository personRepository, IEnumerable<Guid> personIds)
    {
        List<Guid> ids = personIds.Where(id => id != Guid.Empty).Distinct().ToList();
        if (ids.Count == 0)
            return new Dictionary<Guid, Person>();

        IEnumerable<Person> persons = await personRepository.GetByIdsAsync(ids);
        return persons.ToDictionary(p => p.Id, p => p);
    }

    /// <summary>
    /// Returns the person's full name, or an empty string when the person is not in the lookup.
    /// </summary>
    public static string NameOf(Guid personId, IReadOnlyDictionary<Guid, Person>? lookup)
    {
        if (lookup is null || !lookup.TryGetValue(personId, out Person? person))
            return string.Empty;

        return person.FullName.Trim();
    }
}
