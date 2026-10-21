using Application.Features.Common.Content.Advertisements.Commands;
using Application.Features.Common.Content.Advertisements.DTOs;
using Domain.Entities.Common;

namespace Application.Features.Common.Content.Advertisements.Mappings;

internal static class AdvertisementMapper
{
    public static Uri? ParseUrl(string? url, string paramName)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        if (!Uri.TryCreate(url.Trim(), UriKind.Absolute, out Uri? uri))
        {
            throw new ArgumentException("Url must be an http or https address", paramName);
        }

        return uri;
    }

    public static Advertisement ToEntity(CreateAdvertisementCommand command)
    {
        return new Advertisement(
            Guid.NewGuid(),
            command.Name,
            ParseUrl(command.ImageUrl, nameof(command.ImageUrl))
                ?? throw new ArgumentException("Advertisement image is required", nameof(command.ImageUrl)),
            ParseUrl(command.LinkUrl, nameof(command.LinkUrl)),
            command.AltText,
            command.Slot,
            command.IsActive,
            command.SortOrder,
            command.StartsAt,
            command.EndsAt);
    }

    public static void UpdateFromCommand(Advertisement entity, UpdateAdvertisementCommand command)
    {
        entity.Update(
            command.Name,
            ParseUrl(command.ImageUrl, nameof(command.ImageUrl))
                ?? throw new ArgumentException("Advertisement image is required", nameof(command.ImageUrl)),
            ParseUrl(command.LinkUrl, nameof(command.LinkUrl)),
            command.AltText,
            command.Slot,
            command.IsActive,
            command.SortOrder,
            command.StartsAt,
            command.EndsAt);
    }

    public static AdvertisementDto ToDto(Advertisement entity)
    {
        return new AdvertisementDto
        {
            Id = entity.Id,
            Name = entity.Name,
            ImageUrl = entity.ImageUrl.ToString(),
            LinkUrl = entity.LinkUrl?.ToString(),
            AltText = entity.AltText,
            Slot = entity.Slot,
            IsActive = entity.IsActive,
            SortOrder = entity.SortOrder,
            StartsAt = entity.StartsAt,
            EndsAt = entity.EndsAt,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt ?? entity.CreatedAt,
        };
    }

    public static IReadOnlyList<AdvertisementDto> ToDtos(IEnumerable<Advertisement> entities)
    {
        return entities.Select(ToDto).ToList();
    }

    public static PublicAdvertisementDto ToPublicDto(Advertisement entity)
    {
        return new PublicAdvertisementDto
        {
            Id = entity.Id,
            Name = entity.Name,
            ImageUrl = entity.ImageUrl.ToString(),
            LinkUrl = entity.LinkUrl?.ToString(),
            AltText = entity.AltText,
            Slot = entity.Slot,
            SortOrder = entity.SortOrder,
        };
    }

    public static IReadOnlyList<PublicAdvertisementDto> ToPublicDtos(IEnumerable<Advertisement> entities)
    {
        return entities.Select(ToPublicDto).ToList();
    }
}
