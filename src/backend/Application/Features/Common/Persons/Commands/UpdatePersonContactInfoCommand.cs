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
using Domain.ValueObjects.Common;
using MediatR;

namespace Application.Features.Common.Persons.Commands;

/// <summary>
/// Command for updating an existing ContactInfo
/// </summary>
/// <param name="Id"></param>
/// <param name="contactInfo"></param>
public record UpdatePersonContactInfoCommand(Guid Id, ContactInfo contactInfo) : IRequest<Result<ContactInfoDto>>;
