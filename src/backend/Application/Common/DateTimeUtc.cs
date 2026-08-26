namespace Application.Common;

/// <summary>
/// Normalizes <see cref="DateTime"/> values for PostgreSQL <c>timestamptz</c> columns.
/// </summary>
public static class DateTimeUtc
{
    public static DateTime Normalize(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };

    public static DateTime? Normalize(DateTime? value) =>
        value.HasValue ? Normalize(value.Value) : null;
}
