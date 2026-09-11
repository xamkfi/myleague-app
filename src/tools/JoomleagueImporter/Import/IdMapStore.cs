using System.Text.Json;
using System.Text.Json.Serialization;

namespace JoomleagueImporter.Import;

/// <summary>
/// Persistent mapping from old JoomLeague ids to new system Guids. Match-phase writes are
/// batched (every <see cref="SaveBatchSize"/> changes) so resume stays cheap under parallelism.
/// </summary>
public class IdMapStore
{
    public const int SaveBatchSize = 10;

    private readonly string _path;
    private readonly object _sync = new();
    private int _pendingSaves;

    public Dictionary<int, PersonMapping> Persons { get; set; } = [];
    public Dictionary<int, Guid> Clubs { get; set; } = [];
    public Dictionary<int, Guid> Teams { get; set; } = [];
    public Dictionary<int, Guid> Seasons { get; set; } = [];

    /// <summary>Old match id -> new match id, for matches whose import fully finished.</summary>
    public Dictionary<int, Guid> ProcessedMatches { get; set; } = [];

    /// <summary>"Unknown scorer" player per team (old team id -> player id).</summary>
    public Dictionary<int, Guid> UnknownPlayers { get; set; } = [];

    /// <summary>
    /// Extra unique "Tuntematon" players used to pad a football lineup when the real roster
    /// is smaller than PlayersOnField. Keyed by old team id.
    /// </summary>
    public Dictionary<int, List<Guid>> ExtraUnknownPlayers { get; set; } = [];

    public class PersonMapping
    {
        public Guid PersonId { get; set; }
        public Guid PlayerId { get; set; }
    }

    [JsonConstructor]
    public IdMapStore()
    {
        _path = "";
    }

    private IdMapStore(string path)
    {
        _path = path;
    }

    public static IdMapStore LoadOrCreate(string path)
    {
        if (File.Exists(path))
        {
            IdMapStore? loaded = JsonSerializer.Deserialize<IdMapStore>(File.ReadAllText(path));
            if (loaded != null)
            {
                IdMapStore store = new(path)
                {
                    Persons = loaded.Persons,
                    Clubs = loaded.Clubs,
                    Teams = loaded.Teams,
                    Seasons = loaded.Seasons,
                    ProcessedMatches = loaded.ProcessedMatches,
                    UnknownPlayers = loaded.UnknownPlayers,
                    ExtraUnknownPlayers = loaded.ExtraUnknownPlayers ?? [],
                };
                return store;
            }
        }
        return new IdMapStore(path);
    }

    public bool HasTeam(int oldTeamId)
    {
        lock (_sync)
            return Teams.ContainsKey(oldTeamId);
    }

    public bool TryGetTeam(int oldTeamId, out Guid teamId)
    {
        lock (_sync)
            return Teams.TryGetValue(oldTeamId, out teamId);
    }

    public void MapTeam(int oldTeamId, Guid teamId)
    {
        lock (_sync)
            Teams[oldTeamId] = teamId;
        Save(force: false);
    }

    public bool HasClub(int oldClubKey)
    {
        lock (_sync)
            return Clubs.ContainsKey(oldClubKey);
    }

    public bool TryGetClub(int oldClubKey, out Guid clubId)
    {
        lock (_sync)
            return Clubs.TryGetValue(oldClubKey, out clubId);
    }

    public void MapClub(int oldClubKey, Guid clubId)
    {
        lock (_sync)
            Clubs[oldClubKey] = clubId;
        Save(force: false);
    }

    public bool TryGetPerson(int oldPersonId, out PersonMapping? mapping)
    {
        lock (_sync)
            return Persons.TryGetValue(oldPersonId, out mapping);
    }

    public bool HasPerson(int oldPersonId)
    {
        lock (_sync)
            return Persons.ContainsKey(oldPersonId);
    }

    public void MapPerson(int oldPersonId, PersonMapping mapping)
    {
        lock (_sync)
            Persons[oldPersonId] = mapping;
        Save(force: false);
    }

    public bool TryGetSeason(int oldProjectId, out Guid seasonId)
    {
        lock (_sync)
            return Seasons.TryGetValue(oldProjectId, out seasonId);
    }

    public bool HasMappedSeasonId(Guid seasonId)
    {
        lock (_sync)
            return Seasons.ContainsValue(seasonId);
    }

    public List<Guid> GetMappedSeasonIds()
    {
        lock (_sync)
            return Seasons.Values.Distinct().ToList();
    }

    public void MapSeason(int oldProjectId, Guid seasonId)
    {
        lock (_sync)
            Seasons[oldProjectId] = seasonId;
        Save(force: true);
    }

    public void MapMatch(int oldMatchId, Guid newMatchId)
    {
        lock (_sync)
            ProcessedMatches[oldMatchId] = newMatchId;
        Save(force: false);
    }

    public bool TryGetProcessedMatch(int oldMatchId, out Guid newMatchId)
    {
        lock (_sync)
            return ProcessedMatches.TryGetValue(oldMatchId, out newMatchId);
    }

    /// <param name="force">When false, writes only after <see cref="SaveBatchSize"/> dirty updates.</param>
    public void Save(bool force = true)
    {
        if (string.IsNullOrEmpty(_path)) return;
        lock (_sync)
        {
            if (!force)
            {
                _pendingSaves++;
                if (_pendingSaves < SaveBatchSize)
                    return;
            }

            string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_path, json);
            _pendingSaves = 0;
        }
    }
}
