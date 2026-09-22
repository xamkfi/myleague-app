using System;
using Application.Features.Common.Organization.Users.DTOs;
using Application.Features.Common.Organization.Persons.DTOs;
using Application.Features.Common.Organization.Clubs.DTOs;
using Application.Features.Common.Organization.Divisions.DTOs;
using Application.Features.Common.Content.News.DTOs;
using Application.Features.Common.CrossCutting.Search.DTOs;
using Application.Features.Common.CrossCutting.MatchTimer.DTOs;
using Application.Features.Common.Shared.DTOs;

using Domain.Entities.Football.Officials;

namespace Application.Features.Football.Referees.DTOs
{
    /// <summary>
    /// Data Transfer Object for FootballReferee entity
    /// </summary>
    /// <param name="Id">The unique identifier of the referee</param>
    /// <param name="PersonId">The ID of the person this referee profile belongs to</param>
    /// <param name="Person">The person information for this referee</param>
    /// <param name="IsActive">Whether the referee is currently active</param>
    /// <param name="LicenseIssueDate">The date when the referee's license was issued</param>
    /// <param name="LicenseExpiryDate">The date when the referee's license expires</param>
    /// <param name="MatchesOfficiated">The number of football matches officiated by this referee</param>
    public record FootballRefereeDto(
        Guid Id,
        Guid PersonId,
        PersonDto Person,
        bool IsActive,
        DateTime? LicenseIssueDate,
        DateTime? LicenseExpiryDate,
        int MatchesOfficiated);
}
