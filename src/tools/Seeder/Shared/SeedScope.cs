namespace Seeder;

[Flags]
public enum SeedScope
{
    None              = 0,
    Persons           = 1 << 0,
    Clubs             = 1 << 1,
    Divisions         = 1 << 2,
    PlayersReferees   = 1 << 3,
    Teams             = 1 << 4,
    Seasons           = 1 << 5,
    SeasonMatches     = 1 << 6,
    Tournaments       = 1 << 7,
    All = Persons | Clubs | Divisions | PlayersReferees | Teams | Seasons | SeasonMatches | Tournaments
}

public static class SeedScopeResolver
{
    // Iterative fixed-point: each pass adds the prerequisites of any flag currently in the set.
    // Iterating until stable keeps the rule list flat and lets us add new transitive dependencies
    // without manually computing closures.
    public static SeedScope Resolve(SeedScope requested)
    {
        SeedScope current = requested;
        SeedScope previous;
        do
        {
            previous = current;

            if (current.HasFlag(SeedScope.PlayersReferees))
            {
                current |= SeedScope.Persons;
            }
            if (current.HasFlag(SeedScope.Teams))
            {
                current |= SeedScope.Persons | SeedScope.Clubs | SeedScope.Divisions | SeedScope.PlayersReferees;
            }
            if (current.HasFlag(SeedScope.Seasons))
            {
                current |= SeedScope.Persons | SeedScope.Clubs | SeedScope.Divisions | SeedScope.PlayersReferees | SeedScope.Teams;
            }
            if (current.HasFlag(SeedScope.SeasonMatches))
            {
                current |= SeedScope.Persons | SeedScope.Clubs | SeedScope.Divisions | SeedScope.PlayersReferees | SeedScope.Teams | SeedScope.Seasons;
            }
            if (current.HasFlag(SeedScope.Tournaments))
            {
                // Tournaments is a single inseparable phase: tournament records + their group-stage matches
                // are always seeded together by FloorballTournamentMatchesSeeder, so we don't expose a
                // standalone "tournament matches" scope.
                current |= SeedScope.Persons | SeedScope.Clubs | SeedScope.Divisions | SeedScope.PlayersReferees | SeedScope.Teams;
            }
        } while (current != previous);

        return current;
    }

    public static string Explain(SeedScope effective, SeedScope requested)
    {
        SeedScope auto = effective & ~requested;
        string selected = FormatScope(requested);
        if (auto == SeedScope.None)
        {
            return $"Selected: {selected}.";
        }
        string autoIncluded = FormatScope(auto);
        return $"Selected: {selected}. Auto-included: {autoIncluded}.";
    }

    public static bool TryParseToken(string token, out SeedScope scope)
    {
        scope = SeedScope.None;
        string trimmed = token.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return false;
        }
        if (string.Equals(trimmed, "all", StringComparison.OrdinalIgnoreCase))
        {
            scope = SeedScope.All;
            return true;
        }
        // Reject pure-numeric strings — Enum.TryParse silently accepts them as raw bit values,
        // which is not what users mean when they pass --scope tokens.
        if (int.TryParse(trimmed, out _))
        {
            return false;
        }
        if (Enum.TryParse(trimmed, ignoreCase: true, out SeedScope parsed) &&
            parsed != SeedScope.None)
        {
            scope = parsed;
            return true;
        }
        return false;
    }

    private static string FormatScope(SeedScope scope)
    {
        if (scope == SeedScope.None)
        {
            return "(none)";
        }
        SeedScope[] order = new[]
        {
            SeedScope.Persons,
            SeedScope.Clubs,
            SeedScope.Divisions,
            SeedScope.PlayersReferees,
            SeedScope.Teams,
            SeedScope.Seasons,
            SeedScope.SeasonMatches,
            SeedScope.Tournaments
        };
        List<string> parts = new List<string>();
        foreach (SeedScope flag in order)
        {
            if (scope.HasFlag(flag))
            {
                parts.Add(flag.ToString());
            }
        }
        return string.Join(", ", parts);
    }
}
