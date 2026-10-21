using Domain.Entities.Common;
using Domain.Enums.Common;

namespace DomainTestProject.Common;

public class AdvertisementTests
{
    private static readonly Uri Image = new("https://cdn.example.com/ad.png");
    private static readonly DateTime Now = new(2026, 10, 16, 12, 0, 0, DateTimeKind.Utc);

    private static Advertisement Create(
        bool isActive = true,
        DateTime? startsAt = null,
        DateTime? endsAt = null,
        Uri? linkUrl = null)
    {
        return new Advertisement(
            Guid.NewGuid(),
            " Sponsor ",
            Image,
            linkUrl,
            " Sponsor logo ",
            AdSlot.LeftTop,
            isActive,
            0,
            startsAt,
            endsAt);
    }

    [Fact]
    public void Constructor_ValidInput_SetsTrimmedProperties()
    {
        Advertisement ad = Create(linkUrl: new Uri("https://sponsor.fi"));

        ad.Name.Should().Be("Sponsor");
        ad.AltText.Should().Be("Sponsor logo");
        ad.ImageUrl.Should().Be(Image);
        ad.LinkUrl.Should().Be(new Uri("https://sponsor.fi"));
        ad.Slot.Should().Be(AdSlot.LeftTop);
    }

    [Fact]
    public void Constructor_EmptyName_Throws()
    {
        Action act = () => new Advertisement(Guid.NewGuid(), " ", Image, null, null, AdSlot.LeftTop, true, 0, null, null);

        act.Should().Throw<ArgumentException>().WithParameterName("name");
    }

    [Fact]
    public void Constructor_NonHttpLink_Throws()
    {
        Action act = () => Create(linkUrl: new Uri("javascript:alert(1)"));

        act.Should().Throw<ArgumentException>().WithParameterName("linkUrl");
    }

    [Fact]
    public void Constructor_EndBeforeStart_Throws()
    {
        Action act = () => Create(startsAt: Now, endsAt: Now.AddDays(-1));

        act.Should().Throw<ArgumentException>().WithParameterName("endsAt");
    }

    [Fact]
    public void Constructor_UndefinedSlot_Throws()
    {
        Action act = () => new Advertisement(Guid.NewGuid(), "Ad", Image, null, null, (AdSlot)99, true, 0, null, null);

        act.Should().Throw<ArgumentException>().WithParameterName("slot");
    }

    [Fact]
    public void Constructor_UnspecifiedKindDates_AreTreatedAsUtc()
    {
        Advertisement ad = Create(startsAt: new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Unspecified));

        ad.StartsAt!.Value.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void IsVisibleAt_ActiveWithoutSchedule_IsVisible()
    {
        Create().IsVisibleAt(Now).Should().BeTrue();
    }

    [Fact]
    public void IsVisibleAt_Inactive_IsHidden()
    {
        Create(isActive: false).IsVisibleAt(Now).Should().BeFalse();
    }

    [Fact]
    public void IsVisibleAt_StartIsInclusiveAndEndIsExclusive()
    {
        Advertisement ad = Create(startsAt: Now, endsAt: Now.AddDays(1));

        ad.IsVisibleAt(Now).Should().BeTrue();
        ad.IsVisibleAt(Now.AddSeconds(-1)).Should().BeFalse();
        ad.IsVisibleAt(Now.AddDays(1)).Should().BeFalse();
    }

    [Fact]
    public void Deactivate_ThenActivate_TogglesVisibility()
    {
        Advertisement ad = Create();

        ad.Deactivate();
        ad.IsVisibleAt(Now).Should().BeFalse();

        ad.Activate();
        ad.IsVisibleAt(Now).Should().BeTrue();
    }

    [Fact]
    public void Update_ChangesFieldsAndSetsUpdatedAt()
    {
        Advertisement ad = Create();
        Uri newImage = new("https://cdn.example.com/new.png");

        ad.Update("New", newImage, null, null, AdSlot.BottomCenter, false, 3, null, null);

        ad.Name.Should().Be("New");
        ad.ImageUrl.Should().Be(newImage);
        ad.Slot.Should().Be(AdSlot.BottomCenter);
        ad.IsActive.Should().BeFalse();
        ad.SortOrder.Should().Be(3);
        ad.UpdatedAt.Should().NotBeNull();
    }
}
