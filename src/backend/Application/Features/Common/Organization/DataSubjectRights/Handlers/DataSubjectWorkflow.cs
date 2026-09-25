using Application.Common;
using Application.Features.Common.Organization.DataSubjectRights.DTOs;
using Application.Features.Common.Organization.DataSubjectRights.Mappings;
using Application.Features.Common.Organization.Deletion;
using Domain.Entities.Common;
using Domain.Enums.Common;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;

namespace Application.Features.Common.Organization.DataSubjectRights.Handlers;

/// <summary>
/// Loads a person and account, applies a data-subject change, and returns the updated copy.
/// </summary>
internal static class DataSubjectWorkflow
{
    public static Task<Result<DataSubjectCopyDto>> ReadAsync(
        Guid personId,
        IPersonRepository personRepository,
        IUserRepository userRepository,
        IPersonDeletionGuard deletionGuard,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        return ExecuteAsync(
            personId,
            personRepository,
            userRepository,
            deletionGuard,
            unitOfWork: null,
            logger,
            cancellationToken,
            mutate: null);
    }

    public static Task<Result<DataSubjectCopyDto>> ChangeAsync(
        Guid personId,
        IPersonRepository personRepository,
        IUserRepository userRepository,
        IPersonDeletionGuard deletionGuard,
        IUnitOfWork unitOfWork,
        ILogger logger,
        CancellationToken cancellationToken,
        Func<Person, User?, Task> mutate)
    {
        return ExecuteAsync(
            personId,
            personRepository,
            userRepository,
            deletionGuard,
            unitOfWork,
            logger,
            cancellationToken,
            mutate);
    }

    /// <summary>
    /// Refuses changes that would disable the only system administrator.
    /// </summary>
    public static async Task<string?> LastAdminBlockReasonAsync(User? user, IUserRepository userRepository)
    {
        if (user is null || user.Role != UserRole.SystemAdmin)
        {
            return null;
        }

        int adminCount = await userRepository.CountByRoleAsync(UserRole.SystemAdmin);
        if (adminCount <= 1)
        {
            return DeletionReasons.LastSystemAdmin;
        }

        return null;
    }

    private static async Task<Result<DataSubjectCopyDto>> ExecuteAsync(
        Guid personId,
        IPersonRepository personRepository,
        IUserRepository userRepository,
        IPersonDeletionGuard deletionGuard,
        IUnitOfWork? unitOfWork,
        ILogger logger,
        CancellationToken cancellationToken,
        Func<Person, User?, Task>? mutate)
    {
        try
        {
            Person? person = await personRepository.GetByIdAsync(personId);
            if (person is null)
            {
                return Result<DataSubjectCopyDto>.NotFound("Person", personId);
            }

            User? user = await userRepository.GetByPersonIdAsync(personId);
            if (mutate is not null)
            {
                await mutate(person, user);
                await personRepository.UpdateAsync(person);
                if (user is not null)
                {
                    await userRepository.UpdateAsync(user);
                }

                if (unitOfWork is null)
                {
                    throw new InvalidOperationException("A unit of work is required to save a data-subject change.");
                }

                await unitOfWork.SaveChangesAsync(cancellationToken);
            }

            PersonDeletionEvaluation evaluation = await deletionGuard.EvaluateAsync(personId, cancellationToken);
            return Result<DataSubjectCopyDto>.Success(DataSubjectMapper.ToCopy(person, user, evaluation));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Data-subject request rejected for person {PersonId}", personId);
            return Result<DataSubjectCopyDto>.Failure(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Data-subject request rejected for person {PersonId}", personId);
            return Result<DataSubjectCopyDto>.Failure(ex.Message);
        }
    }
}
