using System;
using System.Collections.Generic;

namespace Application.Common;

/// <summary>
/// Helpers for surfacing diagnostic information from exceptions through the
/// <see cref="Result{T}"/>/<see cref="Result"/> contract instead of swallowing it.
/// </summary>
public static class ExceptionExtensions
{
    /// <summary>
    /// Flattens an exception (including its inner-exception chain and any
    /// <see cref="AggregateException"/> children) into a list of human-readable
    /// strings of the form "<c>ExceptionType: Message</c>".
    /// </summary>
    /// <remarks>
    /// Intended for use in command/query handlers where the catch-all branch
    /// would otherwise return a generic message and discard the actual cause.
    /// The returned strings can be placed in <c>Result.Failure(message, errors)</c>
    /// so the API client (e.g. the seeder) sees the real reason for the failure.
    /// </remarks>
    public static IReadOnlyList<string> Flatten(this Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        List<string> messages = new List<string>();
        AppendException(exception, messages);
        return messages;
    }

    private static void AppendException(Exception exception, List<string> messages)
    {
        Exception? current = exception;
        while (current != null)
        {
            messages.Add($"{current.GetType().Name}: {current.Message}");

            if (current is AggregateException aggregate)
            {
                foreach (Exception inner in aggregate.InnerExceptions)
                {
                    AppendException(inner, messages);
                }
                return;
            }

            current = current.InnerException;
        }
    }
}
