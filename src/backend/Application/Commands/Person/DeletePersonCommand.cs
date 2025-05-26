using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using MediatR;

namespace Application.Commands.Person;

/// <summary>
/// Command for deleting a Person
/// </summary>
public record DeletePersonCommand(Guid Id) : IRequest<Result>;


