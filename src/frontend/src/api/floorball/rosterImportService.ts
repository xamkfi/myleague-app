import { personApi } from '../admin/personApi';
import type {
  SeasonImportCallbacks,
  SeasonImportCreatedRecord,
  SeasonImportPlayerAdapters,
  SeasonImportSummary,
} from '../../types/common/seasonImportTypes';
import { FloorballPosition } from '../../types/floorball/floorballTypes';
import {
  toRosterCompetitionOptions,
  toRosterTeamOptions,
  type RosterAssignment,
  type RosterCompetitionOption,
  type RosterTeamOption,
} from '../common/rosterAssignments';
import { importRosterAssignments, revertRosterRecords, snapshotFromRosterRows } from '../common/rosterImportRunner';
import { floorballPlayerService } from './floorballPlayerService';
import { floorballSeasonService } from './floorballSeasonService';
import { floorballTeamService } from './floorballTeamService';
import { floorballTournamentService } from './floorballTournamentService';

const FLOORBALL_POSITIONS = new Set<string>(Object.values(FloorballPosition));

export async function loadFloorballRosterCompetitions(): Promise<RosterCompetitionOption[]> {
  const [seasons, tournaments] = await Promise.all([
    floorballSeasonService.getAll(true),
    floorballTournamentService.getAll(undefined, true),
  ]);
  return toRosterCompetitionOptions(seasons.data ?? [], tournaments.data ?? []);
}

export async function loadFloorballRosterTeams(competition: RosterCompetitionOption): Promise<RosterTeamOption[]> {
  if (competition.kind === 'tournament') {
    const tournament = await floorballTournamentService.getById(competition.id, true);
    return toRosterTeamOptions(
      (tournament.data.groups ?? [])
        .flatMap((group) => group.teams ?? [])
        .map((team) => ({ id: team.teamId, name: team.teamName })),
    );
  }
  const season = await floorballSeasonService.getById(competition.id, true);
  return toRosterTeamOptions(season.data.teams ?? []);
}

export async function importFloorballRosters(
  competitionId: string,
  assignments: readonly RosterAssignment[],
  callbacks: SeasonImportCallbacks,
): Promise<SeasonImportSummary> {
  return importRosterAssignments(assignments, createFloorballRosterAdapters(competitionId), callbacks);
}

export async function revertFloorballRosters(
  competitionId: string,
  records: SeasonImportCreatedRecord[],
  callbacks: Pick<SeasonImportCallbacks, 'onStep' | 'onError'>,
): Promise<{ deleted: number; failed: number }> {
  return revertRosterRecords(records, (record) => deleteFloorballRosterRecord(record, competitionId), callbacks);
}

function createFloorballRosterAdapters(competitionId: string): SeasonImportPlayerAdapters {
  return {
    defaultPosition: FloorballPosition.Forward,
    normalizePosition: (position) =>
      position && FLOORBALL_POSITIONS.has(position) ? position : FloorballPosition.Forward,
    loadRoster: async (teamId) => {
      try {
        const team = await floorballTeamService.getById(teamId, competitionId);
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
          const list = await floorballPlayerService.getAll({ searchTerm: personFullName, pageSize: 50 });
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
        const created = await floorballPlayerService.create({ personId });
        return { id: created.id, created: true };
      } catch (err) {
        const retry = await lookup();
        if (retry) return retry;
        throw err;
      }
    },
    addPlayerToTeam: async (teamId, playerId, position, jerseyNumber, requestedJerseyNumber) => {
      await floorballTeamService.addPlayerToTeam(
        teamId,
        playerId,
        position as FloorballPosition,
        jerseyNumber,
        requestedJerseyNumber,
        competitionId,
      );
    },
  };
}

async function deleteFloorballRosterRecord(
  record: SeasonImportCreatedRecord,
  competitionId: string,
): Promise<void> {
  switch (record.kind) {
    case 'team-player':
      await floorballTeamService.removePlayerFromTeam(record.teamId, record.playerId, competitionId);
      return;
    case 'player':
      await floorballPlayerService.delete(record.id);
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
