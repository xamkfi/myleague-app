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
/// Command for updating person basic info
/// </summary>
public record UpdatePersonBasicInfoCommand(Guid Id, string FirstName, string LastName) : IRequest<Result<PersonDto>>;

