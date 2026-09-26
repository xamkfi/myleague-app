import { personApi } from '../admin/personApi';
import type {
  SeasonImportCallbacks,
  SeasonImportCreatedRecord,
  SeasonImportPlayerAdapters,
  SeasonImportSummary,
} from '../../types/common/seasonImportTypes';
import { FootballPosition } from '../../types/football/footballTypes';
import type { RosterAssignment, RosterTeamOption } from '../common/rosterExcelParser';
import { importRosterAssignments, revertRosterRecords, snapshotFromRosterRows } from '../common/rosterImportRunner';
import { footballPlayerService } from './footballPlayerService';
import { footballSeasonService } from './footballSeasonService';
import { footballTeamService } from './footballTeamService';

const FOOTBALL_POSITIONS = new Set<string>(Object.values(FootballPosition));

export async function loadFootballRosterSeasons(): Promise<RosterTeamOption[]> {
  const response = await footballSeasonService.getAll(true);
  return (response.data ?? [])
    .map((season) => ({ id: season.id, name: season.name }))
    .sort((left, right) => left.name.localeCompare(right.name, 'fi'));
}

export async function loadFootballRosterSeasonTeams(seasonId: string): Promise<RosterTeamOption[]> {
  const response = await footballSeasonService.getById(seasonId, true);
  const unique = new Map<string, string>();
  for (const team of response.data.teams ?? []) {
    if (team.id && team.name) unique.set(team.id, team.name);
  }
  return [...unique.entries()]
    .map(([id, name]) => ({ id, name }))
    .sort((left, right) => left.name.localeCompare(right.name, 'fi'));
}

export async function importFootballRosters(
  competitionId: string,
  assignments: readonly RosterAssignment[],
  callbacks: SeasonImportCallbacks,
): Promise<SeasonImportSummary> {
  return importRosterAssignments(assignments, createFootballRosterAdapters(competitionId), callbacks);
}

export async function revertFootballRosters(
  competitionId: string,
  records: SeasonImportCreatedRecord[],
  callbacks: Pick<SeasonImportCallbacks, 'onStep' | 'onError'>,
): Promise<{ deleted: number; failed: number }> {
  return revertRosterRecords(records, (record) => deleteFootballRosterRecord(record, competitionId), callbacks);
}

function createFootballRosterAdapters(competitionId: string): SeasonImportPlayerAdapters {
  return {
    defaultPosition: FootballPosition.Forward,
    normalizePosition: (position) =>
      position && FOOTBALL_POSITIONS.has(position) ? position : FootballPosition.Forward,
    loadRoster: async (teamId) => {
      try {
        const team = await footballTeamService.getById(teamId, competitionId);
        return snapshotFromRosterRows(
          (team.roster ?? []).map((player) => ({
            playerId: player.playerId,
            playerName: player.playerName,
            jerseyNumber: player.jerseyNumber,
          })),
        );
      } catch {
        return snapshotFromRosterRows([]);
      }
    },
    ensureSportPlayer: async (personId, personFullName) => {
      const lookup = async (): Promise<{ id: string; created: false } | null> => {
        try {
          const list = await footballPlayerService.getAll({ searchTerm: personFullName, pageSize: 50 });
          const match = (list.data ?? []).find((player) => player.personId === personId);
          if (match) return { id: match.id, created: false };
        } catch {
          // Search failure is treated as not found.
        }
        return null;
      };
      const existing = await lookup();
      if (existing) return existing;
      try {
        const created = await footballPlayerService.create({ personId });
        return { id: created.id, created: true };
      } catch (err) {
        const retry = await lookup();
        if (retry) return retry;
        throw err;
      }
    },
    addPlayerToTeam: async (teamId, playerId, position, jerseyNumber, requestedJerseyNumber) => {
      await footballTeamService.addPlayerToTeam(
        teamId,
        playerId,
        position as FootballPosition,
        jerseyNumber,
        requestedJerseyNumber,
        competitionId,
      );
    },
  };
}

async function deleteFootballRosterRecord(
  record: SeasonImportCreatedRecord,
  competitionId: string,
): Promise<void> {
  switch (record.kind) {
    case 'team-player':
      await footballTeamService.removePlayerFromTeam(record.teamId, record.playerId, competitionId);
      return;
    case 'player':
      await footballPlayerService.delete(record.id);
      return;
    case 'person':
      await personApi.delete(record.id);
      return;
    case 'match':
    case 'season':
    case 'team':
    case 'division':
    case 'club':
      return;
  }
}
