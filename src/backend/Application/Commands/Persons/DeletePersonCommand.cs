using System;
using Application.Common;
using MediatR;

namespace Application.Commands.Persons;

/// <summary>
/// Command for deleting a Person
/// </summary>
public record DeletePersonCommand(Guid Id) : IRequest<Result>;


