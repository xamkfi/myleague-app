using Application.Features.Common.Organization.Divisions.DTOs;

namespace JoomleagueImporter.Import;

/// <summary>
/// Finds or creates import divisions by name. Safe to call from parallel team imports.
/// </summary>
internal sealed class ImportDivisionCatalog
{
    private readonly ImportApiClient _api;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly Dictionary<string, DivisionDto> _byName = new(StringComparer.OrdinalIgnoreCase);
    private List<DivisionDto>? _loaded;

    public ImportDivisionCatalog(ImportApiClient api)
    {
        _api = api;
    }

    public async Task<DivisionDto> GetOrCreateAsync(ImportSeriesProfile profile, string sportType)
    {
        await _gate.WaitAsync();
        try
        {
            if (_byName.TryGetValue(profile.DivisionName, out DivisionDto? cached))
                return cached;

            _loaded ??= await _api.GetDivisionsAsync();
            DivisionDto? division = _loaded.FirstOrDefault(item =>
                string.Equals(item.Name, profile.DivisionName, StringComparison.OrdinalIgnoreCase));
            if (division == null)
            {
                division = await _api.CreateDivisionAsync(
                    profile.DivisionName,
                    $"JoomLeague-tuonnin sarja ({profile.DivisionName})",
                    profile.Level,
                    sportType);
                if (division == null)
                    throw new InvalidOperationException($"Failed to create division '{profile.DivisionName}'.");
                _loaded.Add(division);
                Console.WriteLine($"  Created division '{division.Name}' ({division.Id})");
            }

            _byName[profile.DivisionName] = division;
            return division;
        }
        finally
        {
            _gate.Release();
        }
    }
}
