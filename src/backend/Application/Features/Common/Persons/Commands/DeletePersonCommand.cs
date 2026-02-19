using System;
using Application.Common;
using MediatR;

namespace Application.Features.Common.Persons.Commands;

/// <summary>
/// Command for deleting a Person
/// </summary>
public record DeletePersonCommand(Guid Id) : IRequest<Result>;


