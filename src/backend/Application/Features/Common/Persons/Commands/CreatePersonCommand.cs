using System;
using Application.Common;
using Application.Features.Common.Users.DTOs;
using Application.Features.Common.Persons.DTOs;
using Application.Features.Common.Clubs.DTOs;
using Application.Features.Common.Divisions.DTOs;
using Application.Features.Common.News.DTOs;
using Application.Features.Common.Search.DTOs;
using Application.Features.Common.MatchTimer.DTOs;
using Application.Features.Common.Shared.DTOs;
using MediatR;

namespace Application.Features.Common.Persons.Commands;

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


