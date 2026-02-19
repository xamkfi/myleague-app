using System;
using Application.DTOs.Common;

namespace Application.DTOs.Floorball
{
    /// <summary>
    /// Data Transfer Object for FloorballReferee entity
    /// </summary>
    /// <param name="Id">The unique identifier of the referee</param>
    /// <param name="PersonId">The ID of the person this referee profile belongs to</param>
    /// <param name="Person">The person information for this referee</param>
    /// <param name="IsActive">Whether the referee is currently active</param>
    /// <param name="LicenseIssueDate">The date when the referee's license was issued</param>
    /// <param name="LicenseExpiryDate">The date when the referee's license expires</param>
    /// <param name="MatchesOfficiated">The number of floorball matches officiated by this referee</param>
    public record FloorballRefereeDto(
        Guid Id,
        Guid PersonId,
        PersonDto Person,
        bool IsActive,
        DateTime? LicenseIssueDate,
        DateTime? LicenseExpiryDate,
        int MatchesOfficiated);
}
