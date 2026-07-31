namespace Domain.Services.Hockey;

/// <summary>
/// Result of a pure domain validation check (no infrastructure).
/// </summary>
public sealed class HockeyDomainValidationResult
{
    public bool IsValid => Errors.Count == 0;

    public IReadOnlyList<string> Errors { get; }

    private HockeyDomainValidationResult(IReadOnlyList<string> errors)
    {
        Errors = errors;
    }

    public static HockeyDomainValidationResult Ok() =>
        new(Array.Empty<string>());

    public static HockeyDomainValidationResult Fail(params string[] errors) =>
        new((errors ?? Array.Empty<string>()).Where(e => !string.IsNullOrWhiteSpace(e)).ToList());

    public static HockeyDomainValidationResult Fail(IEnumerable<string> errors) =>
        new(errors.Where(e => !string.IsNullOrWhiteSpace(e)).ToList());

    public HockeyDomainValidationResult Merge(HockeyDomainValidationResult other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (IsValid && other.IsValid)
            return Ok();
        return Fail(Errors.Concat(other.Errors));
    }

    public void ThrowIfInvalid()
    {
        if (IsValid)
            return;
        throw new InvalidOperationException(string.Join(" ", Errors));
    }
}
