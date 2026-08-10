using System.Text.Json;
using System.Text.Json.Serialization;

namespace JoomleagueImporter.Import;

/// <summary>
/// Persistent mapping from old JoomLeague ids to new system Guids. Saved to disk after every
/// update so an interrupted import can be resumed without duplicating already imported data.
/// </summary>
public class IdMapStore
{
    private readonly string _path;

    public Dictionary<int, PersonMapping> Persons { get; set; } = [];
    public Dictionary<int, Guid> Clubs { get; set; } = [];
    public Dictionary<int, Guid> Teams { get; set; } = [];
    public Dictionary<int, Guid> Seasons { get; set; } = [];

    /// <summary>Old match id -> new match id, for matches whose import fully finished.</summary>
    public Dictionary<int, Guid> ProcessedMatches { get; set; } = [];

    /// <summary>"Unknown scorer" player per team (old team id -> player id).</summary>
    public Dictionary<int, Guid> UnknownPlayers { get; set; } = [];

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
                };
                return store;
            }
        }
        return new IdMapStore(path);
    }

    public void Save()
    {
        if (string.IsNullOrEmpty(_path)) return;
        string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_path, json);
    }
}
