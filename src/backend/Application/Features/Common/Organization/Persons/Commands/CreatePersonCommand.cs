using System;
using Application.Common;
using Application.Features.Common.Organization.Users.DTOs;
using Application.Features.Common.Organization.Persons.DTOs;
using Application.Features.Common.Organization.Clubs.DTOs;
using Application.Features.Common.Organization.Divisions.DTOs;
using Application.Features.Common.Content.News.DTOs;
using Application.Features.Common.CrossCutting.Search.DTOs;
using Application.Features.Common.CrossCutting.MatchTimer.DTOs;
using Application.Features.Common.Shared.DTOs;
using MediatR;

namespace Application.Features.Common.Organization.Persons.Commands;

/// <summary>
/// Command for creating a new person
/// </summary>
public record CreatePersonCommand(
    string FirstName,
    string LastName,
    DateTime? BirthDate = null,
    bool IsRegistered = false,
    AddressDto? Address = null,
    ContactInfoDto? ContactInfo = null) : IRequest<Result<PersonDto>>;


