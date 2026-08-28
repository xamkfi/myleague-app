using Application.Interfaces.Common;

namespace MyLeague.Infrastructure.Services.Common;

/// <summary>
/// Process-local cache of effective site settings. Invalidated when an admin saves.
/// </summary>
public sealed class SiteSettingsCache
{
    private readonly object _sync = new();
    private EffectiveAuthSettings? _value;

    public bool TryGet(out EffectiveAuthSettings settings)
    {
        lock (_sync)
        {
            if (_value is null)
            {
                settings = default!;
                return false;
            }

            settings = _value;
            return true;
        }
    }

    public void Set(EffectiveAuthSettings settings)
    {
        lock (_sync)
        {
            _value = settings;
        }
    }

    public void Invalidate()
    {
        lock (_sync)
        {
            _value = null;
        }
    }
}
