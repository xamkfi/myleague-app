import type { MatchPlayerStatRow } from '../../../../components/match';
import {
  FloorballGoalType,
  type FloorballMatchDto,
  type FloorballTeamPlayer,
} from '../../../../types/floorball/floorballTypes';
import { getFloorballGoalTypeInfo } from '../../../../utils/floorballGoalType';

export interface FloorballPlayerMatchStats {
  home: MatchPlayerStatRow[];
  away: MatchPlayerStatRow[];
}

/**
 * Builds per-team player points for one match from its goal and penalty events.
 * Follows the backend season counting: own goals give no points, and bench penalties
 * without a player are skipped. Names fall back to the roster when an event has none.
 */
export function buildFloorballPlayerMatchStats(
  match: FloorballMatchDto,
  homeRoster: FloorballTeamPlayer[],
  awayRoster: FloorballTeamPlayer[],
): FloorballPlayerMatchStats {
  const rosterByPlayerId: Map<string, FloorballTeamPlayer> = new Map<string, FloorballTeamPlayer>();
  for (const player of [...homeRoster, ...awayRoster]) {
    rosterByPlayerId.set(player.playerId, player);
  }

  const rowsByTeam: Map<string, Map<string, MatchPlayerStatRow>> = new Map<string, Map<string, MatchPlayerStatRow>>();

  const rowFor = (teamId: string, playerId: string, eventName: string | undefined): MatchPlayerStatRow => {
    let teamRows: Map<string, MatchPlayerStatRow> | undefined = rowsByTeam.get(teamId);
    if (!teamRows) {
      teamRows = new Map<string, MatchPlayerStatRow>();
      rowsByTeam.set(teamId, teamRows);
    }
    let row: MatchPlayerStatRow | undefined = teamRows.get(playerId);
    if (!row) {
      const rosterPlayer: FloorballTeamPlayer | undefined = rosterByPlayerId.get(playerId);
      const name: string | undefined = eventName && eventName !== 'Unknown Player' ? eventName : rosterPlayer?.playerName;
      row = {
        playerId,
        name: name ?? playerId.slice(0, 8),
        jerseyNumber: rosterPlayer?.jerseyNumber,
        goals: 0,
        assists: 0,
        points: 0,
        penaltyMinutes: 0,
      };
      teamRows.set(playerId, row);
    }
    return row;
  };

  for (const goal of match.goalEvents) {
    if (getFloorballGoalTypeInfo(goal.goalType)?.value === FloorballGoalType.OwnGoal) continue;
    if (goal.playerId) {
      const scorer: MatchPlayerStatRow = rowFor(goal.teamId, goal.playerId, goal.playerName);
      scorer.goals += 1;
      scorer.points += 1;
    }
    if (goal.assisterId) {
      const assister: MatchPlayerStatRow = rowFor(goal.teamId, goal.assisterId, goal.assisterName);
      assister.assists += 1;
      assister.points += 1;
    }
    if (goal.secondaryAssisterId) {
      const secondAssister: MatchPlayerStatRow = rowFor(goal.teamId, goal.secondaryAssisterId, goal.secondaryAssisterName);
      secondAssister.assists += 1;
      secondAssister.points += 1;
    }
  }

  for (const penalty of match.penaltyEvents) {
    if (!penalty.playerId) continue;
    rowFor(penalty.teamId, penalty.playerId, penalty.playerName).penaltyMinutes += penalty.minutes;
  }

  const rowsOf = (teamId: string | null): MatchPlayerStatRow[] =>
    teamId ? [...(rowsByTeam.get(teamId)?.values() ?? [])] : [];

  return { home: rowsOf(match.homeTeamId), away: rowsOf(match.awayTeamId) };
}
