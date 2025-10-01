using Application.Common;
using Application.DTOs.Common;
using Domain.Common;
using MediatR;

public record GetAllPersonsQuery(
    int page = 1,
    int pageSize = 25,
    string? firstName = "",
    string? lastName = "",
    string? birthDate = "",
    bool? isRegistered = null
) : IRequest<Result<PagedResult<PersonDto>>>
{
    public const string ResourceKey = "persons";
}
