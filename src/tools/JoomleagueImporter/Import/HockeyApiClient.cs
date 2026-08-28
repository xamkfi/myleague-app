using System.Net.Http.Json;
using Application.Features.Hockey.Competitions.DTOs;
using Application.Features.Hockey.Matches.DTOs;
using Application.Features.Hockey.Officials.DTOs;
using Application.Features.Hockey.Players.DTOs;
using Application.Features.Hockey.Seasons.DTOs;
using Application.Features.Hockey.Teams.DTOs;
using Domain.Enums.Common;
using Domain.Enums.Hockey.Competitions;
using Domain.Enums.Hockey.Matches;
using Domain.Enums.Hockey.Teams;

namespace JoomleagueImporter.Import;

/// <summary>
/// Hockey WebAPI routes used by the JoomLeague importer.
/// </summary>
public class HockeyApiClient : ImportApiClient
{
    public HockeyApiClient(string baseUrl) : base(baseUrl)
    {
    }

    public async Task<HockeyPlayerDto?> CreatePlayerAsync(
        Guid personId,
        HockeyPosition primaryPosition,
        HockeyCatches? catches = null)
    {
        HttpResponseMessage resp = await Http.PostAsJsonAsync("api/HockeyPlayer", new
        {
            personId,
            primaryPosition = primaryPosition.ToString(),
            shoots = HockeyShoots.Unknown.ToString(),
            catches = catches?.ToString(),
        });
        return await ReadDataOrNull<HockeyPlayerDto>(resp, $"Create hockey player for person {personId}");
    }

    public async Task<List<HockeyTeamDto>> GetTeamsAsync() =>
        await GetUnpaginatedListAsync<HockeyTeamDto>("api/HockeyTeam");

    public async Task<HockeyTeamDto?> GetTeamByIdAsync(Guid teamId)
    {
        HttpResponseMessage resp = await Http.GetAsync($"api/HockeyTeam/{teamId}");
        return await ReadDataOrNull<HockeyTeamDto>(resp, $"Get hockey team {teamId}");
    }

    public async Task<HockeyTeamDto?> CreateTeamAsync(
        string name,
        string shortName,
        Guid clubId,
        Guid? divisionId,
        TeamCategory teamCategory)
    {
        HttpResponseMessage resp = await Http.PostAsJsonAsync("api/HockeyTeam", new
        {
            name,
            clubId,
            teamCategory = teamCategory.ToString(),
            divisionId,
            homeArena = "MAHL Arena",
            shortName,
        });
        return await ReadDataOrNull<HockeyTeamDto>(resp, $"Create hockey team '{name}'");
    }

    public async Task<bool> AddPlayerToTeamAsync(
        Guid teamId,
        Guid playerId,
        HockeyPosition position,
        int? jerseyNumber)
    {
        object request = new
        {
            playerId,
            position = position.ToString(),
            jerseyNumber,
            rosterStatus = HockeyRosterStatus.Active.ToString(),
        };
        HttpResponseMessage resp = await Http.PostAsJsonAsync($"api/HockeyTeam/{teamId}/players", request);
        if (resp.IsSuccessStatusCode)
            return true;

        string body = await resp.Content.ReadAsStringAsync();
        if (body.Contains("already", StringComparison.OrdinalIgnoreCase))
            return true;

        Console.WriteLine($"  WARN: Add hockey player to team failed: {Truncate(body)}");
        return false;
    }

    public async Task<bool> UpdateTeamPlayerAsync(
        Guid teamId,
        Guid playerId,
        HockeyPosition position,
        int jerseyNumber,
        HockeyRosterStatus rosterStatus,
        HockeyCaptainRole captainRole)
    {
        HttpResponseMessage resp = await Http.PutAsJsonAsync($"api/HockeyTeam/{teamId}/players/{playerId}", new
        {
            position = position.ToString(),
            jerseyNumber,
            rosterStatus = rosterStatus.ToString(),
            captainRole = captainRole.ToString(),
        });
        return await OkOrWarn(resp, $"Update hockey team player {playerId} jersey {jerseyNumber}");
    }

    public async Task<List<HockeyOfficialDto>> GetOfficialsAsync() =>
        await GetUnpaginatedListAsync<HockeyOfficialDto>("api/HockeyOfficial");

    public async Task<HockeyOfficialDto?> CreateOfficialAsync(Guid personId)
    {
        HttpResponseMessage resp = await Http.PostAsJsonAsync("api/HockeyOfficial", new
        {
            personId,
            officialRole = HockeyOfficialRole.Referee.ToString(),
            licenseIssueDate = UtcDate("2020-01-01"),
            licenseExpiryDate = UtcDate("2035-12-31"),
        });
        return await ReadDataOrNull<HockeyOfficialDto>(resp, $"Create hockey official for person {personId}");
    }

    public async Task<List<HockeySeasonDto>> GetSeasonsAsync() =>
        await GetUnpaginatedListAsync<HockeySeasonDto>("api/HockeySeason");

    public async Task<HockeySeasonDto?> GetSeasonByIdAsync(Guid seasonId)
    {
        HttpResponseMessage resp = await Http.GetAsync($"api/HockeySeason/{seasonId}");
        return await ReadDataOrNull<HockeySeasonDto>(resp, $"Get hockey season {seasonId}");
    }

    public async Task<HockeySeasonDto?> CreateSeasonAsync(string name, DateTime startDate, DateTime endDate)
    {
        HttpResponseMessage resp = await Http.PostAsJsonAsync("api/HockeySeason", new
        {
            name,
            startDate = UtcDateTime(startDate),
            endDate = UtcDateTime(endDate),
        });
        return await ReadDataOrNull<HockeySeasonDto>(resp, $"Create hockey season '{name}'");
    }

    public async Task<bool> AddDivisionToSeasonAsync(Guid seasonId, Guid divisionId, string name)
    {
        HttpResponseMessage resp = await Http.PostAsJsonAsync($"api/HockeySeason/{seasonId}/divisions", new
        {
            divisionId,
            name,
            sortOrder = 1,
        });
        return await OkOrAlready(resp, "AddDivisionToHockeySeason");
    }

    public async Task<HockeyCompetitionTeamDto?> AddTeamToSeasonAsync(Guid seasonId, Guid teamId)
    {
        HttpResponseMessage resp = await Http.PostAsJsonAsync($"api/HockeySeason/{seasonId}/teams", new { teamId });
        if (resp.IsSuccessStatusCode)
            return await ReadDataOrNull<HockeyCompetitionTeamDto>(resp, "AddTeamToHockeySeason");

        string body = await resp.Content.ReadAsStringAsync();
        if (body.Contains("already", StringComparison.OrdinalIgnoreCase))
            return null;
        Console.WriteLine($"  WARN: AddTeamToHockeySeason failed: {Truncate(body)}");
        return null;
    }

    public async Task<bool> AddTeamToSeasonDivisionAsync(
        Guid seasonId,
        Guid competitionDivisionId,
        Guid competitionTeamId)
    {
        HttpResponseMessage resp = await Http.PostAsJsonAsync(
            $"api/HockeySeason/{seasonId}/divisions/{competitionDivisionId}/teams",
            new { competitionTeamId });
        return await OkOrAlready(resp, "AddTeamToHockeySeasonDivision");
    }

    public async Task<bool> ApplyHobbyRulesAsync(
        Guid competitionId,
        int regularPeriodCount,
        int regularPeriodLengthMinutes,
        int minDressedPlayers)
    {
        object request = new
        {
            name = "MAHL hobby",
            ruleBookSource = HockeyRuleBookSource.Custom.ToString(),
            matchRules = new
            {
                regularPeriodCount,
                regularPeriodLengthMinutes,
                overtimeLengthMinutes = 5,
                stopClock = true,
                overtimeEnabled = true,
                shootoutEnabled = true,
                offsideEnabled = false,
                delayedOffsideEnabled = false,
                icingRule = HockeyIcingRule.Hybrid.ToString(),
                penaltyShotEnabled = true,
                goaliePullAllowed = true,
            },
            standingRules = new
            {
                regulationWinPoints = 3,
                overtimeWinPoints = 2,
                shootoutWinPoints = 2,
                overtimeLossPoints = 1,
                shootoutLossPoints = 1,
                tiePoints = 1,
            },
            rosterRules = new
            {
                maxDressedPlayers = 20,
                maxDressedGoalies = 2,
                minDressedPlayers,
                requiresGoalie = true,
                maxCaptains = 1,
                maxAlternateCaptains = 2,
                canGoalieBeCaptain = false,
                allowGuestPlayers = true,
                lineManagementEnabled = false,
            },
        };
        HttpResponseMessage resp = await Http.PutAsJsonAsync($"api/HockeyCompetition/{competitionId}/rules", request);
        return await OkOrWarn(resp, "ApplyHockeyHobbyRules");
    }

    public async Task<bool> PublishSeasonAsync(Guid seasonId)
    {
        HttpResponseMessage resp = await Http.PostAsync($"api/HockeySeason/{seasonId}/publish", null);
        return await OkOrAlready(resp, "PublishHockeySeason");
    }

    public async Task<bool> OpenSeasonRegistrationAsync(Guid seasonId)
    {
        HttpResponseMessage resp = await Http.PostAsync($"api/HockeySeason/{seasonId}/open-registration", null);
        return await OkOrAlready(resp, "OpenHockeySeasonRegistration");
    }

    public async Task<bool> ActivateSeasonAsync(Guid seasonId)
    {
        HttpResponseMessage resp = await Http.PostAsync($"api/HockeySeason/{seasonId}/activate", null);
        return await OkOrAlready(resp, "ActivateHockeySeason");
    }

    public async Task<HockeyMatchDto?> CreateMatchAsync(
        Guid competitionId,
        Guid? competitionDivisionId,
        DateTime scheduledStartTime,
        string? venue)
    {
        HttpResponseMessage resp = await Http.PostAsJsonAsync("api/HockeyMatch", new
        {
            scheduledStartTime = UtcDateTime(scheduledStartTime),
            matchType = HockeyMatchType.League.ToString(),
            competitionId,
            competitionDivisionId,
            venue,
        });
        return await ReadDataOrNull<HockeyMatchDto>(resp, "Create hockey match");
    }

    public async Task<HockeyMatchDto?> SetMatchTeamsAsync(Guid matchId, Guid homeTeamId, Guid awayTeamId)
    {
        HttpResponseMessage resp = await Http.PutAsJsonAsync($"api/HockeyMatch/{matchId}/teams", new
        {
            homeTeamId,
            awayTeamId,
        });
        return await ReadDataOrNull<HockeyMatchDto>(resp, "Set hockey match teams");
    }

    public async Task<HockeyMatchDto?> GetMatchByIdAsync(Guid matchId)
    {
        HttpResponseMessage resp = await Http.GetAsync($"api/HockeyMatch/{matchId}");
        return await ReadDataOrNull<HockeyMatchDto>(resp, $"Get hockey match {matchId}");
    }

    public async Task<HockeyMatchDto?> ConfirmMatchRosterAsync(
        Guid matchId,
        Guid matchTeamId,
        IReadOnlyList<Guid> teamPlayerIds)
    {
        HttpResponseMessage resp = await Http.PostAsJsonAsync($"api/HockeyMatch/{matchId}/roster/confirm", new
        {
            matchTeamId,
            teamPlayerIds,
            source = "Manual",
        });
        return await ReadDataOrNull<HockeyMatchDto>(resp, "Confirm hockey match roster");
    }

    public async Task<bool> AddMatchOfficialAsync(Guid matchId, Guid officialId)
    {
        HttpResponseMessage resp = await Http.PostAsJsonAsync($"api/HockeyMatch/{matchId}/officials", new
        {
            officialId,
            role = HockeyOfficialRole.Referee.ToString(),
            isMainOfficial = true,
        });
        return await OkOrAlready(resp, "AddHockeyMatchOfficial");
    }

    public async Task<bool> StartMatchAsync(Guid matchId, DateTime? actualStartTime)
    {
        object request = actualStartTime.HasValue
            ? new { actualStartTime = UtcDateTime(actualStartTime.Value) }
            : new { };
        HttpResponseMessage resp = await Http.PostAsJsonAsync($"api/HockeyMatch/{matchId}/start", request);
        return await OkOrWarn(resp, "StartHockeyMatch");
    }

    public async Task<bool> RecordPeriodAsync(Guid matchId, int periodNumber, int timeInSeconds, HockeyPeriodAction action)
    {
        HttpResponseMessage resp = await Http.PostAsJsonAsync($"api/HockeyMatch/{matchId}/events/periods", new
        {
            periodNumber,
            timeInSeconds,
            action = action.ToString(),
        });
        return resp.IsSuccessStatusCode;
    }

    public Task<HockeyMatchEventsImportDto?> ImportEventsAsync(
        Guid matchId,
        IReadOnlyList<object> events) =>
        PostDataOrNullAsync<HockeyMatchEventsImportDto>(
            $"api/HockeyMatch/{matchId}/events/import",
            new { events },
            "ImportHockeyMatchEvents");

    public Task<bool> RecordGoalAsync(
        Guid matchId,
        Guid scoringMatchTeamId,
        Guid scorerActivePlayerId,
        Guid? primaryAssistActivePlayerId,
        Guid? secondaryAssistActivePlayerId,
        Guid? goalieActivePlayerId,
        int periodNumber,
        int timeInSeconds,
        HockeyGoalStrength goalStrength) =>
        PostWithRetryAsync($"api/HockeyMatch/{matchId}/events/goals", new
        {
            scoringMatchTeamId,
            scorerActivePlayerId,
            periodNumber,
            timeInSeconds,
            goalStrength = goalStrength.ToString(),
            primaryAssistActivePlayerId,
            secondaryAssistActivePlayerId,
            goalieActivePlayerId,
        }, "RecordHockeyGoal");

    public Task<bool> RecordPenaltyAsync(
        Guid matchId,
        Guid penaltyMatchTeamId,
        Guid? penalizedActivePlayerId,
        int periodNumber,
        int timeInSeconds,
        HockeyPenaltySeverity severity,
        int penaltyMinutes) =>
        PostWithRetryAsync($"api/HockeyMatch/{matchId}/events/penalties", new
        {
            penaltyMatchTeamId,
            periodNumber,
            timeInSeconds,
            severity = severity.ToString(),
            offence = HockeyPenaltyOffence.UnsportsmanlikeConduct.ToString(),
            penaltyMinutes,
            penalizedActivePlayerId,
            isBenchPenalty = penalizedActivePlayerId == null,
        }, "RecordHockeyPenalty");

    public async Task<bool> FinishMatchAsync(Guid matchId, DateTime? actualEndTime, HockeyMatchResultType resultType)
    {
        HttpResponseMessage resp = await Http.PostAsJsonAsync($"api/HockeyMatch/{matchId}/finish", new
        {
            actualEndTime = actualEndTime.HasValue ? UtcDateTime(actualEndTime.Value) : null,
            resultType = resultType.ToString(),
        });
        return await OkOrWarn(resp, "FinishHockeyMatch");
    }

    public async Task<bool> SetMatchStatusAsync(Guid matchId, HockeyMatchStatus status)
    {
        HttpResponseMessage resp = await Http.PatchAsJsonAsync($"api/HockeyMatch/{matchId}/status", new
        {
            status = status.ToString(),
        });
        return await OkOrWarn(resp, "SetHockeyMatchStatus");
    }

    public async Task<bool> DeleteGoalEventAsync(Guid matchId, Guid eventId)
    {
        HttpResponseMessage resp = await Http.DeleteAsync($"api/HockeyMatch/{matchId}/events/goals/{eventId}");
        return await OkOrWarn(resp, "DeleteHockeyGoalEvent");
    }

    public async Task<bool> DeletePenaltyEventAsync(Guid matchId, Guid eventId)
    {
        HttpResponseMessage resp = await Http.DeleteAsync($"api/HockeyMatch/{matchId}/events/penalties/{eventId}");
        return await OkOrWarn(resp, "DeleteHockeyPenaltyEvent");
    }

    public async Task RecalculateMatchAsync(Guid matchId)
    {
        HttpResponseMessage resp = await Http.PostAsync($"api/HockeyStatistics/matches/{matchId}/recalculate", null);
        await OkOrWarn(resp, "RecalculateHockeyMatch");
    }

    public async Task RecalculateCompetitionAsync(Guid competitionId)
    {
        HttpResponseMessage resp = await Http.PostAsJsonAsync(
            $"api/HockeyStatistics/competitions/{competitionId}/recalculate", new { });
        await OkOrWarn(resp, "RecalculateHockeyCompetition");
    }
}
