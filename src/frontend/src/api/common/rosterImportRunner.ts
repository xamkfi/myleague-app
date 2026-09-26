import type {
  SeasonImportCallbacks,
  SeasonImportCreatedRecord,
  SeasonImportPlayerAdapters,
  SeasonImportSummary,
  SeasonImportTeamPlayer,
  TeamRosterSnapshot,
} from '../../types/common/seasonImportTypes';
import {
  buildPlayerNameKey,
  createImportRuntime,
  emptyRosterSnapshot,
  emptySeasonImportSummary,
  importTeamPlayers,
  revertCreatedRecords,
} from './seasonImportShared';
import { splitPersonName, type RosterAssignment } from './rosterExcelParser';

export function snapshotFromRosterRows(
  rows: readonly {
    playerId: string;
    playerName?: string | null;
    jerseyNumber?: number | null;
    personId?: string | null;
  }[],
): TeamRosterSnapshot {
  const snapshot = emptyRosterSnapshot();
  for (const row of rows) {
    if (row.playerId) snapshot.playerIds.add(row.playerId);
    if (row.personId) snapshot.personIds.add(row.personId);
    if (typeof row.jerseyNumber === 'number' && row.jerseyNumber > 0) {
      snapshot.jerseyNumbers.add(row.jerseyNumber);
    }
    if (row.playerName) {
      const split = splitPersonName(row.playerName);
      if (split) {
        const key = buildPlayerNameKey(split.firstName, split.lastName);
        if (key.length > 0) snapshot.nameKeys.add(key);
      }
    }
  }
  return snapshot;
}

export async function importRosterAssignments(
  assignments: readonly RosterAssignment[],
  adapters: SeasonImportPlayerAdapters,
  callbacks: SeasonImportCallbacks,
): Promise<SeasonImportSummary> {
  const summary = emptySeasonImportSummary();
  const { checkAbort } = createImportRuntime(summary, callbacks);
  const playersByTeam = assignments.map((assignment) => ({
    teamId: assignment.teamId,
    teamName: assignment.teamName,
    players: assignment.block.players
      .filter((player) => !player.invalidName)
      .map((player): SeasonImportTeamPlayer => ({
        firstName: player.firstName,
        lastName: player.lastName,
        position: player.isGoalkeeper ? 'Goalkeeper' : undefined,
        jerseyNumber: player.jerseyNumber,
      })),
  }));
  const total = playersByTeam.reduce((sum, team) => sum + team.players.length, 0);
  let index = 0;
  for (const team of playersByTeam) {
    if (checkAbort()) return summary;
    index = await importTeamPlayers(
      team.teamName,
      team.teamId,
      team.players,
      adapters,
      summary,
      callbacks,
      index,
      total,
      checkAbort,
    );
    if (summary.aborted) return summary;
  }
  if (!summary.fatal && !summary.aborted) {
    callbacks.onStep({
      phase: 'done',
      index: 1,
      total: 1,
      label: 'Import finished successfully',
      status: 'info',
    });
  }
  return summary;
}

export async function revertRosterRecords(
  records: SeasonImportCreatedRecord[],
  deleteRecord: (record: SeasonImportCreatedRecord) => Promise<void>,
  callbacks: Pick<SeasonImportCallbacks, 'onStep' | 'onError'>,
): Promise<{ deleted: number; failed: number }> {
  return revertCreatedRecords(records, deleteRecord, callbacks);
}
