using System.Reflection;
using Domain.Constants;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Routing;

namespace WebApiTestProject.Controllers.Common;

/// <summary>
/// Pins which match endpoints the MatchOfficial role can call. Class- and method-level
/// [Authorize] attributes must all be satisfied, so a role is allowed on an action only when
/// every attribute on the controller and the action lists it.
/// </summary>
public class MatchOfficialAuthorizationTests
{
    /// <summary>
    /// Controllers whose write actions are open to match officials, with the actions that stay admin-only.
    /// </summary>
    public static TheoryData<Type, string[]> LiveMatchControllers => new()
    {
        { typeof(WebAPI.Controllers.Floorball.FloorballMatchEventsController), ["ImportEvents"] },
        { typeof(WebAPI.Controllers.Floorball.Match.FloorballMatchLifecycleController), ["Postpone", "Cancel", "Reactivate", "AssignTeams"] },
        { typeof(WebAPI.Controllers.Floorball.FloorballMatchRosterController), [] },
        { typeof(WebAPI.Controllers.Football.FootballMatchEventsController), ["ImportEvents"] },
        { typeof(WebAPI.Controllers.Football.Match.FootballMatchLifecycleController), ["Postpone", "Cancel", "Reactivate", "AssignTeams"] },
        { typeof(WebAPI.Controllers.Football.Match.FootballMatchLineupController), [] },
        { typeof(WebAPI.Controllers.Hockey.HockeyMatchEventsController), ["ImportEvents"] },
        { typeof(WebAPI.Controllers.Hockey.HockeyMatchLifecycleController), [] },
        { typeof(WebAPI.Controllers.Hockey.HockeyMatchLinesController), [] },
        { typeof(WebAPI.Controllers.Hockey.HockeyMatchOnIceController), [] },
        { typeof(WebAPI.Controllers.Hockey.HockeyMatchRosterController), [] },
        { typeof(WebAPI.Controllers.Common.MatchTimerController), [] },
    };

    /// <summary>
    /// Controllers whose write actions stay admin-only.
    /// </summary>
    public static TheoryData<Type> AdminOnlyControllers => new()
    {
        typeof(WebAPI.Controllers.Floorball.FloorballMatchesController),
        typeof(WebAPI.Controllers.Floorball.Match.FloorballMatchOfficialsController),
        typeof(WebAPI.Controllers.Floorball.Match.FloorballMatchScorekeepersController),
        typeof(WebAPI.Controllers.Football.FootballMatchesController),
        typeof(WebAPI.Controllers.Football.Match.FootballMatchOfficialsController),
        typeof(WebAPI.Controllers.Football.Match.FootballMatchScorekeepersController),
        typeof(WebAPI.Controllers.Hockey.HockeyMatchesController),
        typeof(WebAPI.Controllers.Hockey.HockeyMatchOfficialsController),
        typeof(WebAPI.Controllers.Hockey.HockeyMatchScorekeepersController),
    };

    /// <summary>
    /// Single actions outside the match controllers that the live match desk needs: match-day
    /// loan players and goalkeepers, and the hockey stats recalculation run after a match finishes.
    /// </summary>
    public static TheoryData<Type, string> MatchDayActionsOutsideMatchControllers => new()
    {
        { typeof(WebAPI.Controllers.Floorball.FloorballTeamController), "EnsureLoanGoalkeeper" },
        { typeof(WebAPI.Controllers.Floorball.FloorballTeamController), "EnsureLoanPlayers" },
        { typeof(WebAPI.Controllers.Football.FootballTeamController), "EnsureLoanGoalkeeper" },
        { typeof(WebAPI.Controllers.Football.FootballTeamController), "EnsureLoanPlayers" },
        { typeof(WebAPI.Controllers.Hockey.HockeyTeamController), "EnsureLoanGoalkeeper" },
        { typeof(WebAPI.Controllers.Hockey.HockeyTeamController), "EnsureLoanPlayers" },
        { typeof(WebAPI.Controllers.Hockey.HockeyStatisticsController), "RecalculateMatch" },
    };

    [Theory]
    [MemberData(nameof(MatchDayActionsOutsideMatchControllers))]
    public void MatchDayAction_OutsideMatchControllers_AllowsMatchOfficialAndAdminOnly(Type controller, string actionName)
    {
        MethodInfo action = WriteActions(controller).Single(a => a.Name == actionName);

        IsAllowed(controller, action, AuthRoles.MatchOfficial).Should().BeTrue();
        IsAllowed(controller, action, AuthRoles.SystemAdmin).Should().BeTrue();
        IsAllowed(controller, action, AuthRoles.ClubAdmin).Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(LiveMatchControllers))]
    public void WriteActions_LiveMatchController_AllowMatchOfficialExceptAdminOnlyActions(Type controller, string[] adminOnlyActions)
    {
        List<MethodInfo> actions = WriteActions(controller);
        actions.Should().NotBeEmpty();
        adminOnlyActions.Should().BeSubsetOf(actions.Select(a => a.Name));

        foreach (MethodInfo action in actions)
        {
            bool expectOfficial = !adminOnlyActions.Contains(action.Name);
            IsAllowed(controller, action, AuthRoles.MatchOfficial).Should().Be(expectOfficial,
                $"{controller.Name}.{action.Name} should {(expectOfficial ? "" : "not ")}be open to match officials");
            IsAllowed(controller, action, AuthRoles.SystemAdmin).Should().BeTrue($"{controller.Name}.{action.Name}");
            IsAllowed(controller, action, AuthRoles.ClubAdmin).Should().BeFalse($"{controller.Name}.{action.Name}");
        }
    }

    [Theory]
    [MemberData(nameof(AdminOnlyControllers))]
    public void WriteActions_AdminOnlyController_DenyMatchOfficial(Type controller)
    {
        List<MethodInfo> actions = WriteActions(controller);
        actions.Should().NotBeEmpty();

        foreach (MethodInfo action in actions)
        {
            IsAllowed(controller, action, AuthRoles.MatchOfficial).Should().BeFalse($"{controller.Name}.{action.Name}");
            IsAllowed(controller, action, AuthRoles.SystemAdmin).Should().BeTrue($"{controller.Name}.{action.Name}");
        }
    }

    private static List<MethodInfo> WriteActions(Type controller) =>
        controller
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Where(m => m.GetCustomAttributes<HttpMethodAttribute>()
                .Any(a => a.HttpMethods.Any(verb => verb != "GET")))
            .ToList();

    private static bool IsAllowed(Type controller, MethodInfo action, string role)
    {
        List<AuthorizeAttribute> attributes = controller.GetCustomAttributes<AuthorizeAttribute>(true)
            .Concat(action.GetCustomAttributes<AuthorizeAttribute>(true))
            .ToList();

        // Write actions must never be anonymous.
        attributes.Should().NotBeEmpty($"{controller.Name}.{action.Name} has no [Authorize]");

        return attributes.All(a => string.IsNullOrEmpty(a.Roles)
            || a.Roles.Split(',', StringSplitOptions.TrimEntries).Contains(role));
    }
}
