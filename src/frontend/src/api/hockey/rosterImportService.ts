import { personApi } from '../admin/personApi';
import type {
  SeasonImportCallbacks,
  SeasonImportCreatedRecord,
  SeasonImportPlayerAdapters,
  SeasonImportSummary,
} from '../../types/common/seasonImportTypes';
import { HOCKEY_POSITIONS, type HockeyPosition } from '../../types/hockey/hockeyTypes';
import type { RosterAssignment, RosterTeamOption } from '../common/rosterExcelParser';
import { importRosterAssignments, revertRosterRecords, snapshotFromRosterRows } from '../common/rosterImportRunner';
import { hockeyPlayerService } from './hockeyPlayerService';
import { hockeySeasonService } from './hockeySeasonService';
import { hockeyTeamService } from './hockeyTeamService';

const HOCKEY_POSITION_SET = new Set<string>(HOCKEY_POSITIONS);

export async function loadHockeyRosterSeasons(): Promise<RosterTeamOption[]> {
  const seasons = await hockeySeasonService.getAll(undefined, true);
  return seasons
    .map((season) => ({ id: season.id, name: season.name }))
    .sort((left, right) => left.name.localeCompare(right.name, 'fi'));
}

export async function loadHockeyRosterSeasonTeams(seasonId: string): Promise<RosterTeamOption[]> {
  const [season, teams] = await Promise.all([
    hockeySeasonService.getById(seasonId, true),
    hockeyTeamService.getAll(),
  ]);
  const names = new Map(teams.map((team) => [team.id, team.name]));
  const unique = new Map<string, string>();
  for (const membership of season.teams) {
    if (!membership.isActive) continue;
    unique.set(membership.teamId, names.get(membership.teamId) ?? membership.teamId);
  }
  return [...unique.entries()]
    .map(([id, name]) => ({ id, name }))
    .sort((left, right) => left.name.localeCompare(right.name, 'fi'));
}

export async function importHockeyRosters(
  competitionId: string,
  assignments: readonly RosterAssignment[],
  callbacks: SeasonImportCallbacks,
): Promise<SeasonImportSummary> {
  return importRosterAssignments(assignments, createHockeyRosterAdapters(competitionId), callbacks);
}

export async function revertHockeyRosters(
  competitionId: string,
  records: SeasonImportCreatedRecord[],
  callbacks: Pick<SeasonImportCallbacks, 'onStep' | 'onError'>,
): Promise<{ deleted: number; failed: number }> {
  return revertRosterRecords(records, (record) => deleteHockeyRosterRecord(record, competitionId), callbacks);
}

function createHockeyRosterAdapters(competitionId: string): SeasonImportPlayerAdapters {
  return {
    defaultPosition: 'Center',
    normalizePosition: (position) => {
      if (position && HOCKEY_POSITION_SET.has(position)) return position;
      if (position === 'Goalkeeper') return 'Goalie';
      if (position === 'Defender') return 'Defenseman';
      if (position === 'Forward') return 'RightWing';
      return 'Center';
    },
    loadRoster: async (teamId) => {
      try {
        const team = await hockeyTeamService.getById(teamId, competitionId);
        const rows = await Promise.all(
          (team.roster ?? []).map(async (player) => {
            try {
              const profile = await hockeyPlayerService.getById(player.playerId);
              return {
                playerId: player.playerId,
                jerseyNumber: player.jerseyNumber,
                personId: profile.personId,
              };
            } catch {
              return { playerId: player.playerId, jerseyNumber: player.jerseyNumber };
            }
          }),
        );
        return snapshotFromRosterRows(rows);
      } catch {
        return snapshotFromRosterRows([]);
      }
    },
    ensureSportPlayer: async (personId, _personFullName, position) => {
      const lookup = async (): Promise<{ id: string; created: false } | null> => {
        try {
          const page = await hockeyPlayerService.getPaged({ page: 1, pageSize: 50, searchTerm: _personFullName });
          const match = (page.data ?? []).find((player) => player.personId === personId);
          if (match) return { id: match.id, created: false };
        } catch {
          // Search failure is treated as not found.
        }
        return null;
      };
      const existing = await lookup();
      if (existing) return existing;
      try {
        const created = await hockeyPlayerService.create({
          personId,
          primaryPosition: position as HockeyPosition,
        });
        return { id: created.id, created: true };
      } catch (err) {
        const retry = await lookup();
        if (retry) return retry;
        throw err;
      }
    },
    addPlayerToTeam: async (teamId, playerId, position, jerseyNumber, requestedJerseyNumber) => {
      await hockeyTeamService.addPlayer(
        teamId,
        playerId,
        position as HockeyPosition,
        jerseyNumber,
        'Active',
        competitionId,
        requestedJerseyNumber,
      );
    },
  };
}

async function deleteHockeyRosterRecord(
  record: SeasonImportCreatedRecord,
  competitionId: string,
): Promise<void> {
  switch (record.kind) {
    case 'team-player':
      await hockeyTeamService.removePlayer(record.teamId, record.playerId, competitionId);
      return;
    case 'player':
      await hockeyPlayerService.delete(record.id);
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
