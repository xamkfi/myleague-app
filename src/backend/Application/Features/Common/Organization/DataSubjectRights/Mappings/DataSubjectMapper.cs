using Application.Features.Common.Organization.DataSubjectRights.DTOs;
using Application.Features.Common.Organization.Deletion;
using Application.Features.Common.Organization.Persons.Mappings;
using Domain.Entities.Common;

namespace Application.Features.Common.Organization.DataSubjectRights.Mappings;

/// <summary>
/// Maps a person and optional account into a data-subject copy.
/// </summary>
public static class DataSubjectMapper
{
    /// <summary>
    /// Login address stored after erasure. It does not identify the person.
    /// </summary>
    public static string ErasedAccountEmail(Guid personId) => $"removed-{personId:N}@data-subject.invalid";

    /// <summary>
    /// Builds the copy returned for access, rectification, restriction, objection, and erasure.
    /// </summary>
    public static DataSubjectCopyDto ToCopy(Person person, User? user, PersonDeletionEvaluation evaluation)
    {
        DataSubjectAccountDto? account = user is null
            ? null
            : new DataSubjectAccountDto(
                user.Id,
                user.Email,
                user.Role.ToString(),
                user.IsActive,
                user.LastLoginAt);

        DataSubjectPortableDto portable = new DataSubjectPortableDto(
            person.FirstName,
            person.LastName,
            person.BirthDate,
            PersonMapper.ToAddressDto(person.Address),
            PersonMapper.ToContactInfoDto(person.ContactInfo),
            user?.Email);

        return new DataSubjectCopyDto(
            person.Id,
            true,
            person.FirstName,
            person.LastName,
            person.FullName,
            person.BirthDate,
            person.role.ToString(),
            person.IsRegistered,
            PersonMapper.ToAddressDto(person.Address),
            PersonMapper.ToContactInfoDto(person.ContactInfo),
            person.IsProcessingRestricted,
            person.HasObjectedToLegitimateInterest,
            person.AnonymizedAt,
            evaluation.BlockReason,
            account,
            portable);
    }
}
