using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MyLeague.Infrastructure.Persistence.UnitOfWork;

/// <summary>
/// Maps PostgreSQL unique-constraint races (two requests insert the same
/// season-stat row) to a domain exception handlers already catch.
/// </summary>
internal static class UniqueConstraint
{
    internal const string ConcurrentUniqueMessage =
        "A concurrent request already created this unique record. Retry the operation.";

    public static bool IsViolation(DbUpdateException exception)
    {
        for (Exception? current = exception; current != null; current = current.InnerException)
        {
            if (current is PostgresException postgres
                && postgres.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                return true;
            }

            if (current.Message.Contains("23505", StringComparison.Ordinal)
                || current.Message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public static InvalidOperationException ToConcurrentException(DbUpdateException exception)
    {
        string detail = exception.InnerException?.Message ?? exception.Message;
        return new InvalidOperationException($"{ConcurrentUniqueMessage} {detail}", exception);
    }

    public static async Task<int> SaveChangesAsync(DbContext context, CancellationToken cancellationToken)
    {
        try
        {
            return await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsViolation(ex))
        {
            throw ToConcurrentException(ex);
        }
    }
}
