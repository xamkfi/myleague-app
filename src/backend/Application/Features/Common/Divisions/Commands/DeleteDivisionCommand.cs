using System;
using MediatR;
using Application.Common;

namespace Application.Features.Common.Divisions.Commands;

/// <summary>
/// Command for deleting a division
/// </summary>
public record DeleteDivisionCommand(Guid Id) : IRequest<Result<bool>>; 
