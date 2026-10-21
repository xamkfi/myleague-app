using Application.Features.Common.Content.Advertisements.Commands;
using Application.Features.Common.Content.Advertisements.Validators;
using Domain.Enums.Common;
using FluentValidation.Results;

namespace ApplicationTestProject.Validators.Commands.Advertisements;

public class CreateAdvertisementCommandValidatorTests
{
    private readonly CreateAdvertisementCommandValidator _validator = new();

    private static CreateAdvertisementCommand Valid() => new(
        "Sponsor", "https://cdn.example.com/ad.png", "https://sponsor.fi", "Logo",
        AdSlot.BottomCenter, true, 0, null, null);

    [Fact]
    public void Valid_Passes()
    {
        _validator.Validate(Valid()).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not a url")]
    [InlineData("ftp://cdn.example.com/ad.png")]
    public void InvalidImageUrl_Fails(string imageUrl)
    {
        ValidationResult result = _validator.Validate(Valid() with { ImageUrl = imageUrl });

        result.Errors.Should().Contain(e => e.PropertyName == "ImageUrl");
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("/relative")]
    public void InvalidLinkUrl_Fails(string linkUrl)
    {
        ValidationResult result = _validator.Validate(Valid() with { LinkUrl = linkUrl });

        result.Errors.Should().Contain(e => e.PropertyName == "LinkUrl");
    }

    [Fact]
    public void EmptyLinkUrl_Passes()
    {
        _validator.Validate(Valid() with { LinkUrl = null }).IsValid.Should().BeTrue();
    }

    [Fact]
    public void EndNotAfterStart_Fails()
    {
        DateTime start = new(2026, 10, 16, 0, 0, 0, DateTimeKind.Utc);

        ValidationResult result = _validator.Validate(Valid() with { StartsAt = start, EndsAt = start });

        result.Errors.Should().Contain(e => e.PropertyName == "EndsAt");
    }

    [Fact]
    public void UndefinedSlot_Fails()
    {
        ValidationResult result = _validator.Validate(Valid() with { Slot = (AdSlot)42 });

        result.Errors.Should().Contain(e => e.PropertyName == "Slot");
    }

    [Fact]
    public void NegativeSortOrder_Fails()
    {
        ValidationResult result = _validator.Validate(Valid() with { SortOrder = -1 });

        result.Errors.Should().Contain(e => e.PropertyName == "SortOrder");
    }
}
