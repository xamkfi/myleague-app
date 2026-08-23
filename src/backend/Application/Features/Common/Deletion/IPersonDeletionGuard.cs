namespace Application.Features.Common.Deletion;

/// <summary>
/// Cross-context checks for hard-deleting a person.
/// </summary>
public interface IPersonDeletionGuard
{
    /// <summary>
    /// Evaluates whether the person can be deleted and which unused sport profiles to remove first.
    /// </summary>
    Task<PersonDeletionEvaluation> EvaluateAsync(Guid personId, CancellationToken cancellationToken);
}
