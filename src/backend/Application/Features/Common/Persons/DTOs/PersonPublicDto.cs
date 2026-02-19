
namespace Application.Features.Common.Persons.DTOs
{
    /// <summary>
    /// Data Transfer Object for public Person entity excluding sensitive data
    /// </summary>
    public record PersonPublicDto(
        Guid? Id,
        string FirstName,
        string LastName,
        DateTime? BirthDate,
        string FullName,
        bool? IsRegistered);
}
