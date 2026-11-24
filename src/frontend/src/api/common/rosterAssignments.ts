export interface ParsedRosterPlayer {
  firstName: string;
  lastName: string;
  rawName: string;
  jerseyNumber?: number;
  position?: string;
  isGoalkeeper: boolean;
  isCaptain: boolean;
  isReferee: boolean;
  rowNumber: number;
  invalidName: boolean;
}

export interface ParsedRosterTeamBlock {
  teamName: string | null;
  coachName: string | null;
  jerseyColor: string | null;
  players: ParsedRosterPlayer[];
}

export interface ParsedRosterWorkbook {
  teams: ParsedRosterTeamBlock[];
}

export interface RosterTeamOption {
  id: string;
  name: string;
}

/** A competition whose teams' rosters can be imported. */
export interface RosterCompetitionOption {
  id: string;
  name: string;
  kind: 'season' | 'tournament';
}

export interface RosterAssignment {
  teamId: string;
  teamName: string;
  block: ParsedRosterTeamBlock;
}

export interface RosterImportPreview {
  assignments: RosterAssignment[];
  invalidNames: string[];
  duplicateDestinations: string[];
  playerCount: number;
  parsedPlayerCount: number;
  canImport: boolean;
}

export function normalizeTeamName(name: string): string {
  return name.trim().replace(/\s+/g, ' ').toLocaleLowerCase('fi');
}

/** Competition choices: seasons first, then tournaments, each sorted by name. */
export function toRosterCompetitionOptions(
  seasons: readonly RosterTeamOption[],
  tournaments: readonly RosterTeamOption[],
): RosterCompetitionOption[] {
  const toOptions = (
    competitions: readonly RosterTeamOption[],
    kind: RosterCompetitionOption['kind'],
  ): RosterCompetitionOption[] =>
    competitions
      .map(({ id, name }) => ({ id, name, kind }))
      .sort((left, right) => left.name.localeCompare(right.name, 'fi'));
  return [...toOptions(seasons, 'season'), ...toOptions(tournaments, 'tournament')];
}

/** Team choices without duplicates, sorted by name. */
export function toRosterTeamOptions(teams: readonly RosterTeamOption[]): RosterTeamOption[] {
  const unique = new Map<string, string>();
  for (const { id, name } of teams) {
    if (id && name) unique.set(id, name);
  }
  return [...unique.entries()]
    .map(([id, name]) => ({ id, name }))
    .sort((left, right) => left.name.localeCompare(right.name, 'fi'));
}

export function splitPersonName(rawName: string): { firstName: string; lastName: string } | null {
  const parts = rawName.trim().replace(/\s+/g, ' ').split(' ').filter((part) => part.length > 0);
  if (parts.length < 2) return null;
  const lastName = parts[parts.length - 1] ?? '';
  const firstName = parts.slice(0, -1).join(' ');
  if (firstName.length === 0 || lastName.length === 0) return null;
  return { firstName, lastName };
}

export function suggestRosterDestinations(
  parsed: ParsedRosterWorkbook,
  teams: readonly RosterTeamOption[],
  preferredTeamId?: string | null,
): Record<number, string> {
  const destinations: Record<number, string> = {};
  const used = new Set<string>();
  parsed.teams.forEach((block, index) => {
    const named = block.teamName
      ? teams.find((team) => !used.has(team.id) && namesMatch(team.name, block.teamName ?? ''))
      : undefined;
    if (named) {
      destinations[index] = named.id;
      used.add(named.id);
      return;
    }
    if (
      parsed.teams.length === 1
      && preferredTeamId
      && !used.has(preferredTeamId)
      && teams.some((team) => team.id === preferredTeamId)
    ) {
      destinations[index] = preferredTeamId;
      used.add(preferredTeamId);
    }
  });
  return destinations;
}

export function assignRosterDestinations(
  parsed: ParsedRosterWorkbook,
  teams: readonly RosterTeamOption[],
  destinations: Readonly<Record<number, string>>,
): RosterImportPreview {
  const chosenCounts = new Map<string, number>();
  for (const teamId of Object.values(destinations)) {
    if (teamId.length === 0) continue;
    chosenCounts.set(teamId, (chosenCounts.get(teamId) ?? 0) + 1);
  }
  const duplicateDestinations = teams
    .filter((team) => (chosenCounts.get(team.id) ?? 0) > 1)
    .map((team) => team.name);

  const assignments: RosterAssignment[] = [];
  parsed.teams.forEach((block, index) => {
    const teamId = destinations[index];
    if (!teamId || (chosenCounts.get(teamId) ?? 0) !== 1) return;
    const team = teams.find((candidate) => candidate.id === teamId);
    if (!team) return;
    assignments.push({ teamId: team.id, teamName: team.name, block });
  });

  const invalidNames = assignments.flatMap((assignment) =>
    assignment.block.players
      .filter((player) => player.invalidName)
      .map((player) => `${assignment.teamName}: ${player.rawName}`),
  );
  const playerCount = assignments.reduce(
    (sum, assignment) => sum + assignment.block.players.filter((player) => !player.invalidName).length,
    0,
  );
  const parsedPlayerCount = parsed.teams.reduce((sum, block) => sum + block.players.length, 0);

  return {
    assignments,
    invalidNames,
    duplicateDestinations,
    playerCount,
    parsedPlayerCount,
    canImport:
      assignments.length > 0
      && duplicateDestinations.length === 0
      && invalidNames.length === 0
      && playerCount > 0,
  };
}

function namesMatch(left: string, right: string): boolean {
  return normalizeTeamName(left) === normalizeTeamName(right);
}
