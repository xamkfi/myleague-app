using Application.Features.Hockey.Matches.DTOs;
using Application.Features.Hockey.Matches.Mappings;
using Domain.Entities.Hockey.Matches.Events;
using Domain.Enums.Hockey.Matches;
using FluentAssertions;
using System;
using Xunit;

namespace ApplicationTestProject.Mappings;

public class HockeyMatchMapperTests
{
    [Fact]
    public void ToEventDto_Goal_MapsAssistsAndStrength()
    {
        Guid primaryAssist = Guid.NewGuid();
        Guid secondaryAssist = Guid.NewGuid();
        HockeyGoal goal = new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            periodNumber: 2,
            gameTime: TimeSpan.FromSeconds(125),
            HockeyGoalStrength.PowerPlayOneMan,
            primaryAssistActivePlayerId: primaryAssist,
            secondaryAssistActivePlayerId: secondaryAssist);

        HockeyMatchEventDto dto = HockeyMatchMapper.ToEventDto(goal);

        dto.PrimaryAssistActivePlayerId.Should().Be(primaryAssist);
        dto.SecondaryAssistActivePlayerId.Should().Be(secondaryAssist);
        dto.GoalStrength.Should().Be(nameof(HockeyGoalStrength.PowerPlayOneMan));
        dto.PenaltyOffence.Should().BeNull();
        dto.PenaltyMinutes.Should().BeNull();
    }

    [Fact]
    public void ToEventDto_Penalty_MapsOffenceSeverityAndMinutes()
    {
        HockeyPenalty penalty = new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            periodNumber: 1,
            gameTime: TimeSpan.FromSeconds(30),
            HockeyPenaltySeverity.Minor,
            HockeyPenaltyOffence.Hooking,
            penaltyMinutes: 2);

        HockeyMatchEventDto dto = HockeyMatchMapper.ToEventDto(penalty);

        dto.PenaltyOffence.Should().Be(nameof(HockeyPenaltyOffence.Hooking));
        dto.PenaltySeverity.Should().Be(nameof(HockeyPenaltySeverity.Minor));
        dto.PenaltyMinutes.Should().Be(2);
        dto.PrimaryAssistActivePlayerId.Should().BeNull();
        dto.GoalStrength.Should().BeNull();
    }
}
