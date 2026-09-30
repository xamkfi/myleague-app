import type { ParsedRosterPlayer, ParsedRosterWorkbook } from './rosterExcelParser';

export const ROSTER_IMPORT_SCHEMA = 'myleague-roster-import/v1';

const GOALIE_POSITIONS = new Set(['Goalie', 'Goalkeeper']);

export function parseRosterJson(
  text: string,
  allowedPositions: readonly string[],
): { ok: true; workbook: ParsedRosterWorkbook } | { ok: false; errors: string[] } {
  let raw: unknown;
  try {
    raw = JSON.parse(text);
  } catch (err) {
    const message = err instanceof Error ? err.message : String(err);
    return { ok: false, errors: [`JSON does not parse: ${message}`] };
  }

  if (!raw || typeof raw !== 'object' || Array.isArray(raw)) {
    return { ok: false, errors: ['Uploaded file is not a JSON object.'] };
  }

  const body = raw as { $schema?: unknown; teams?: unknown };
  const errors: string[] = [];
  if (body.$schema !== undefined && body.$schema !== ROSTER_IMPORT_SCHEMA) {
    errors.push(`$schema must be "${ROSTER_IMPORT_SCHEMA}".`);
  }
  if (!Array.isArray(body.teams)) {
    errors.push('"teams" must be an array.');
    return { ok: false, errors };
  }
  if (body.teams.length === 0) {
    errors.push('At least one team is required.');
  }

  const allowed = new Set(allowedPositions);
  const seenNames = new Set<string>();
  const teams: ParsedRosterWorkbook['teams'] = [];

  body.teams.forEach((entry, teamIndex) => {
    if (!entry || typeof entry !== 'object' || Array.isArray(entry)) {
      errors.push(`teams[${teamIndex}] must be an object.`);
      return;
    }
    const team = entry as { name?: unknown; players?: unknown };
    const name = typeof team.name === 'string' ? team.name.trim() : '';
    if (name.length === 0) {
      errors.push(`teams[${teamIndex}].name is required.`);
    } else {
      const key = name.toLocaleLowerCase('fi');
      if (seenNames.has(key)) errors.push(`Team "${name}" is listed more than once.`);
      seenNames.add(key);
    }
    if (!Array.isArray(team.players)) {
      errors.push(`Team "${name || teamIndex}" is missing a players array.`);
      return;
    }
    if (team.players.length === 0) {
      errors.push(`Team "${name || teamIndex}" has no players.`);
    }

    const players: ParsedRosterPlayer[] = [];
    team.players.forEach((playerEntry, playerIndex) => {
      const label = `teams[${teamIndex}].players[${playerIndex}]`;
      if (!playerEntry || typeof playerEntry !== 'object' || Array.isArray(playerEntry)) {
        errors.push(`${label} must be an object.`);
        return;
      }
      const player = playerEntry as {
        firstName?: unknown;
        lastName?: unknown;
        jerseyNumber?: unknown;
        position?: unknown;
      };
      const firstName = typeof player.firstName === 'string' ? player.firstName.trim() : '';
      const lastName = typeof player.lastName === 'string' ? player.lastName.trim() : '';
      if (firstName.length === 0 || lastName.length === 0) {
        errors.push(`${label} needs both firstName and lastName.`);
        return;
      }
      let position: string | undefined;
      if (player.position !== undefined && player.position !== null && player.position !== '') {
        if (typeof player.position !== 'string' || !allowed.has(player.position)) {
          errors.push(
            `${firstName} ${lastName}: position must be one of ${allowedPositions.join(', ')}.`,
          );
          return;
        }
        position = player.position;
      }
      let jerseyNumber: number | undefined;
      if (player.jerseyNumber !== undefined && player.jerseyNumber !== null && player.jerseyNumber !== '') {
        if (typeof player.jerseyNumber !== 'number' || !Number.isInteger(player.jerseyNumber)) {
          errors.push(`${firstName} ${lastName}: jerseyNumber must be an integer from 1 to 99.`);
          return;
        }
        if (player.jerseyNumber < 1 || player.jerseyNumber > 99) {
          errors.push(`${firstName} ${lastName}: jerseyNumber must be from 1 to 99. Omit 0.`);
          return;
        }
        jerseyNumber = player.jerseyNumber;
      }
      players.push({
        firstName,
        lastName,
        rawName: `${firstName} ${lastName}`,
        jerseyNumber,
        position,
        isGoalkeeper: position !== undefined && GOALIE_POSITIONS.has(position),
        isCaptain: false,
        isReferee: false,
        rowNumber: playerIndex + 1,
        invalidName: false,
      });
    });

    if (name.length > 0) {
      teams.push({ teamName: name, coachName: null, jerseyColor: null, players });
    }
  });

  if (errors.length > 0) return { ok: false, errors };
  return { ok: true, workbook: { teams } };
}
