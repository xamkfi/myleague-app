import ExcelJS from 'exceljs';

export interface ParsedRosterPlayer {
  firstName: string;
  lastName: string;
  rawName: string;
  jerseyNumber?: number;
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

export type RosterImportMode = 'single' | 'multiple';

export interface RosterTeamOption {
  id: string;
  name: string;
}

export interface RosterAssignment {
  teamId: string;
  teamName: string;
  block: ParsedRosterTeamBlock;
}

export interface RosterImportPreview {
  assignments: RosterAssignment[];
  missingTeams: RosterTeamOption[];
  extraFileTeams: string[];
  invalidNames: string[];
  unnamedPlayerCount: number;
  duplicateFileTeams: string[];
  canImport: boolean;
}

const GOALIE_SECTIONS = new Set(['mv', 'maalivahti', 'maalivahdit', 'goalkeeper', 'goalie']);
const FIELD_SECTIONS = new Set(['pelaajat', 'kenttapelaajat', 'kenttäpelaajat']);

interface ColumnMap {
  name: number;
  number: number;
  captain: number;
  referee: number;
}

const DEFAULT_COLUMNS: ColumnMap = { name: 0, number: 1, captain: 2, referee: 4 };

export function normalizeTeamName(name: string): string {
  return name.trim().replace(/\s+/g, ' ').toLocaleLowerCase('fi');
}

export function splitPersonName(rawName: string): { firstName: string; lastName: string } | null {
  const parts = rawName.trim().replace(/\s+/g, ' ').split(' ').filter((part) => part.length > 0);
  if (parts.length < 2) return null;
  const lastName = parts[parts.length - 1] ?? '';
  const firstName = parts.slice(0, -1).join(' ');
  if (firstName.length === 0 || lastName.length === 0) return null;
  return { firstName, lastName };
}

export function parseRosterGrid(rows: readonly (readonly string[])[]): ParsedRosterWorkbook {
  const teams: ParsedRosterTeamBlock[] = [];
  let current: ParsedRosterTeamBlock | null = null;
  let inPlayers = false;
  let isGoalkeeper = false;
  let columns: ColumnMap = { ...DEFAULT_COLUMNS };

  const pushCurrent = (): void => {
    if (!current) return;
    if (current.teamName || current.coachName || current.jerseyColor || current.players.length > 0) {
      teams.push(current);
    }
    current = null;
  };

  const ensureCurrent = (): ParsedRosterTeamBlock => {
    if (!current) {
      current = { teamName: null, coachName: null, jerseyColor: null, players: [] };
    }
    return current;
  };

  rows.forEach((row, index) => {
    const cells = row.map((cell) => (cell ?? '').replace(/\s+/g, ' ').trim());
    if (!cells.some((cell) => cell.length > 0)) return;

    const labelIndex = cells.findIndex((cell) => cell.length > 0);
    const label = normalizeLabel(cells[labelIndex] ?? '');
    const rowNumber = index + 1;

    if (label === 'joukkueen nimi') {
      pushCurrent();
      current = {
        teamName: valueAfter(cells, labelIndex),
        coachName: null,
        jerseyColor: null,
        players: [],
      };
      inPlayers = false;
      isGoalkeeper = false;
      columns = { ...DEFAULT_COLUMNS };
      return;
    }

    if (label === 'jojon nimi') {
      ensureCurrent().coachName = valueAfter(cells, labelIndex);
      return;
    }

    if (label === 'pelipaidan väri' || label === 'pelipaidan vari') {
      ensureCurrent().jerseyColor = valueAfter(cells, labelIndex);
      return;
    }

    if (label === 'joukkueen pelaajat') {
      ensureCurrent();
      inPlayers = true;
      return;
    }

    if (isHeaderRow(cells)) {
      ensureCurrent();
      inPlayers = true;
      columns = readColumns(cells);
      return;
    }

    if (!inPlayers) return;

    const name = cells[columns.name] ?? '';
    if (name.length === 0) return;

    const jerseyRaw = cells[columns.number] ?? '';
    const section = sectionKind(name, jerseyRaw);
    if (section === 'goalkeeper') {
      isGoalkeeper = true;
      return;
    }
    if (section === 'field') {
      isGoalkeeper = false;
      return;
    }

    const split = splitPersonName(name);
    const jerseyNumber = parseJerseyNumber(jerseyRaw);
    ensureCurrent().players.push({
      firstName: split?.firstName ?? '',
      lastName: split?.lastName ?? '',
      rawName: name,
      jerseyNumber,
      isGoalkeeper,
      isCaptain: isMark(cells[columns.captain] ?? ''),
      isReferee: isMark(cells[columns.referee] ?? ''),
      rowNumber,
      invalidName: split === null,
    });
  });

  pushCurrent();
  return { teams };
}

export async function parseRosterWorkbook(buffer: ArrayBuffer): Promise<ParsedRosterWorkbook> {
  const rows = await readFirstSheet(buffer);
  return parseRosterGrid(rows);
}

export function matchRosterImport(
  parsed: ParsedRosterWorkbook,
  selectedTeams: readonly RosterTeamOption[],
  mode: RosterImportMode,
): RosterImportPreview {
  const missingTeams: RosterTeamOption[] = [];
  const extraFileTeams: string[] = [];
  const duplicateFileTeams: string[] = [];
  const assignments: RosterAssignment[] = [];
  const unnamedPlayerCount = parsed.teams
    .filter((block) => !block.teamName)
    .reduce((sum, block) => sum + block.players.length, 0);

  if (mode === 'single') {
    const selected = selectedTeams[0];
    if (selected) {
      const named = parsed.teams.filter((block) => block.teamName);
      if (named.length === 0) {
        const players = parsed.teams.flatMap((block) => block.players);
        const coachName = parsed.teams.find((block) => block.coachName)?.coachName ?? null;
        const jerseyColor = parsed.teams.find((block) => block.jerseyColor)?.jerseyColor ?? null;
        if (players.length > 0) {
          assignments.push({
            teamId: selected.id,
            teamName: selected.name,
            block: { teamName: null, coachName, jerseyColor, players },
          });
        }
      } else {
        const matched = named.filter((block) => namesMatch(block.teamName ?? '', selected.name));
        const extras = named
          .filter((block) => !namesMatch(block.teamName ?? '', selected.name))
          .map((block) => block.teamName ?? '')
          .filter((name) => name.length > 0);
        extraFileTeams.push(...extras);
        if (matched.length === 0) {
          missingTeams.push(selected);
        } else if (matched.length > 1) {
          duplicateFileTeams.push(selected.name);
        } else {
          const block = matched[0];
          if (block) {
            assignments.push({ teamId: selected.id, teamName: selected.name, block });
          }
        }
      }
    }
  } else {
    const used = new Set<ParsedRosterTeamBlock>();
    for (const team of selectedTeams) {
      const hits = parsed.teams.filter(
        (block) => block.teamName && namesMatch(block.teamName, team.name),
      );
      if (hits.length === 0) {
        missingTeams.push(team);
      } else if (hits.length > 1) {
        duplicateFileTeams.push(team.name);
      } else {
        const block = hits[0];
        if (block) {
          used.add(block);
          assignments.push({ teamId: team.id, teamName: team.name, block });
        }
      }
    }
    for (const block of parsed.teams) {
      if (!block.teamName || used.has(block)) continue;
      if (duplicateFileTeams.some((name) => namesMatch(name, block.teamName ?? ''))) continue;
      extraFileTeams.push(block.teamName);
    }
  }

  const invalidNames = assignments.flatMap((assignment) =>
    assignment.block.players
      .filter((player) => player.invalidName)
      .map((player) => `${assignment.teamName}: ${player.rawName}`),
  );
  const playerCount = assignments.reduce(
    (sum, assignment) => sum + assignment.block.players.filter((player) => !player.invalidName).length,
    0,
  );
  const singleExtrasBlocked = mode === 'single' && extraFileTeams.length > 0;
  const unnamedBlocked = mode === 'multiple' && unnamedPlayerCount > 0;
  const singleUnnamedWithNamedBlock =
    mode === 'single' && unnamedPlayerCount > 0 && assignments.some((assignment) => assignment.block.teamName);

  const canImport =
    selectedTeams.length > 0 &&
    missingTeams.length === 0 &&
    duplicateFileTeams.length === 0 &&
    invalidNames.length === 0 &&
    !singleExtrasBlocked &&
    !unnamedBlocked &&
    !singleUnnamedWithNamedBlock &&
    playerCount > 0;

  return {
    assignments,
    missingTeams,
    extraFileTeams,
    invalidNames,
    unnamedPlayerCount: mode === 'multiple' || singleUnnamedWithNamedBlock ? unnamedPlayerCount : 0,
    duplicateFileTeams,
    canImport,
  };
}

async function readFirstSheet(buffer: ArrayBuffer): Promise<string[][]> {
  const workbook = new ExcelJS.Workbook();
  await workbook.xlsx.load(buffer);
  const sheet = workbook.worksheets[0];
  if (!sheet) return [];
  const columnCount = Math.max(sheet.columnCount, 8);
  const rows: string[][] = [];
  sheet.eachRow({ includeEmpty: true }, (row, rowNumber) => {
    const line: string[] = [];
    for (let column = 1; column <= columnCount; column += 1) {
      line.push(formatExcelCell(row.getCell(column).value));
    }
    rows[rowNumber - 1] = line;
  });
  const dense: string[][] = [];
  for (let index = 0; index < rows.length; index += 1) {
    dense.push(rows[index] ?? []);
  }
  return dense;
}

function formatExcelCell(value: ExcelJS.CellValue): string {
  if (value == null) return '';
  if (typeof value === 'string') return value.replace(/\s+/g, ' ').trim();
  if (typeof value === 'number') return Number.isInteger(value) ? String(value) : String(value);
  if (typeof value === 'boolean') return value ? 'X' : '';
  if (value instanceof Date) return '';
  if (typeof value === 'object') {
    if ('richText' in value && Array.isArray(value.richText)) {
      return value.richText
        .map((part) => part.text)
        .join('')
        .replace(/\s+/g, ' ')
        .trim();
    }
    if ('text' in value && typeof value.text === 'string') {
      return value.text.replace(/\s+/g, ' ').trim();
    }
    if ('result' in value && value.result != null && typeof value.result !== 'object') {
      return formatExcelCell(value.result);
    }
  }
  return '';
}

function normalizeLabel(value: string): string {
  return value.replace(/[:.]+$/g, '').trim().replace(/\s+/g, ' ').toLocaleLowerCase('fi');
}

function valueAfter(cells: readonly string[], labelIndex: number): string | null {
  for (let index = labelIndex + 1; index < cells.length; index += 1) {
    const value = cells[index] ?? '';
    if (value.length > 0) return value;
  }
  return null;
}

function isHeaderRow(cells: readonly string[]): boolean {
  return cells.some((cell) => normalizeLabel(cell) === 'nimi');
}

function readColumns(cells: readonly string[]): ColumnMap {
  const columns: ColumnMap = { ...DEFAULT_COLUMNS };
  cells.forEach((cell, index) => {
    const label = normalizeLabel(cell);
    if (label === 'nimi') columns.name = index;
    if (label === 'nro' || label === 'numero' || label === 'pelinumero' || label === '#') columns.number = index;
    if (label === 'kapt' || label === 'kapteeni' || label === 'captain') columns.captain = index;
    if (label === 'tuomari' || label === 'referee') columns.referee = index;
  });
  return columns;
}

function sectionKind(name: string, jerseyRaw: string): 'goalkeeper' | 'field' | null {
  if (jerseyRaw.trim().length > 0) return null;
  const label = normalizeLabel(name);
  if (GOALIE_SECTIONS.has(label)) return 'goalkeeper';
  if (FIELD_SECTIONS.has(label)) return 'field';
  return null;
}

function parseJerseyNumber(raw: string): number | undefined {
  const trimmed = raw.trim();
  if (!/^\d+$/.test(trimmed)) return undefined;
  const value = Number.parseInt(trimmed, 10);
  if (!Number.isFinite(value) || value < 1 || value > 99) return undefined;
  return value;
}

function isMark(raw: string): boolean {
  return /^(x|kyllä|kylla|yes|true|1)$/i.test(raw.trim());
}

function namesMatch(left: string, right: string): boolean {
  return normalizeTeamName(left) === normalizeTeamName(right);
}
