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

namespace Application.Features.Common.Organization.Persons.Queries;

/// <summary>
/// Query for retrieving a person by its email
/// </summary>
public record GetPersonByEmailQuery(string email) : IRequest<Result<PersonDto>>;
