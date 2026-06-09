using WebAPI.Services;

namespace WebApiTestProject.Services;

public class MatchEventRateLimiterTests
{
    [Fact]
    public void IsRateLimited_FirstCallForKey_ReturnsFalse()
    {
        MatchEventRateLimiter limiter = new MatchEventRateLimiter();

        bool limited = limiter.IsRateLimited("match-1:goal:player-1", TimeSpan.FromMilliseconds(50));

        limited.Should().BeFalse("the first event for a key is always allowed");
    }

    [Fact]
    public void IsRateLimited_SameKeyWithinWindow_ReturnsTrue()
    {
        MatchEventRateLimiter limiter = new MatchEventRateLimiter();
        const string key = "match-1:goal:player-1";
        TimeSpan window = TimeSpan.FromSeconds(10);

        bool first = limiter.IsRateLimited(key, window);
        bool second = limiter.IsRateLimited(key, window);

        first.Should().BeFalse();
        second.Should().BeTrue("a repeat call within the window should be rejected");
    }

    [Fact]
    public void IsRateLimited_SameKeyAfterWindowElapses_ReturnsFalse()
    {
        MatchEventRateLimiter limiter = new MatchEventRateLimiter();
        const string key = "match-1:save:player-1";

        bool first = limiter.IsRateLimited(key, TimeSpan.FromMilliseconds(20));
        Thread.Sleep(40);
        bool second = limiter.IsRateLimited(key, TimeSpan.FromMilliseconds(20));

        first.Should().BeFalse();
        second.Should().BeFalse("once the window has elapsed the same key is allowed again");
    }

    [Fact]
    public void IsRateLimited_DifferentKeys_AreIndependent()
    {
        MatchEventRateLimiter limiter = new MatchEventRateLimiter();
        TimeSpan window = TimeSpan.FromSeconds(10);

        bool a = limiter.IsRateLimited("match-1:goal:player-1", window);
        bool b = limiter.IsRateLimited("match-1:goal:player-2", window);
        bool c = limiter.IsRateLimited("match-2:goal:player-1", window);

        a.Should().BeFalse();
        b.Should().BeFalse("a different player's events must not be limited by player-1's");
        c.Should().BeFalse("a different match's events must not be limited by match-1's");
    }

    [Fact]
    public void IsRateLimited_NullOrEmptyKey_ThrowsArgumentException()
    {
        MatchEventRateLimiter limiter = new MatchEventRateLimiter();

        Action callWithNull = () => limiter.IsRateLimited(null!, TimeSpan.FromSeconds(1));
        Action callWithEmpty = () => limiter.IsRateLimited(string.Empty, TimeSpan.FromSeconds(1));

        callWithNull.Should().Throw<ArgumentException>();
        callWithEmpty.Should().Throw<ArgumentException>();
    }
}
