import { personApi } from '../api/admin/personApi';
import { clubService } from '../api/common/clubService';
import { hockeyPlayerService } from '../api/hockey/hockeyPlayerService';
import { hockeyTeamService } from '../api/hockey/hockeyTeamService';
import type { HockeyMatchDto, HockeyMatchEventDto, HockeyTeamDto } from '../types/hockey/hockeyTypes';

export async function loadPersonNameMap(personIds: string[]): Promise<Map<string, string>> {
  const unique = [...new Set(personIds.filter(Boolean))];
  const entries = await Promise.all(
    unique.map(async (personId) => {
      try {
        const person = await personApi.getById(personId);
        return [personId, person.fullName || `${person.firstName} ${person.lastName}`.trim()] as const;
      } catch {
        return [personId, personId.slice(0, 8)] as const;
      }
    }),
  );
  return new Map(entries);
}

export async function loadClubNameMap(): Promise<Map<string, string>> {
  const clubs = await clubService.getAll();
  return new Map(clubs.map((club) => [club.id, club.name]));
}

export async function loadTeamNameMap(teams?: HockeyTeamDto[]): Promise<Map<string, string>> {
  const list = teams ?? (await hockeyTeamService.getAll());
  return new Map(list.map((team) => [team.id, team.name]));
}

export function formatHockeyDateTime(iso: string): string {
  const date = new Date(iso);
  if (Number.isNaN(date.getTime())) {
    return iso;
  }
  return date.toLocaleString();
}

export function formatHockeyDate(iso: string): string {
  const date = new Date(iso);
  if (Number.isNaN(date.getTime())) {
    return iso;
  }
  return date.toLocaleDateString();
}

export async function loadHockeyRosterNameMaps(teams: HockeyTeamDto[]): Promise<{
  byPlayerId: Map<string, string>;
  byTeamPlayerId: Map<string, string>;
}> {
  const playerIds = [...new Set(teams.flatMap((team) => team.roster.map((row) => row.playerId)))];
  const profiles = await Promise.all(
    playerIds.map(async (playerId) => {
      try {
        return await hockeyPlayerService.getById(playerId);
      } catch {
        return null;
      }
    }),
  );
  const valid = profiles.filter((player) => player !== null);
  const people = await loadPersonNameMap(valid.map((player) => player.personId));
  const byPlayerId = new Map<string, string>();
  for (const player of valid) {
    byPlayerId.set(player.id, people.get(player.personId) ?? player.id.slice(0, 8));
  }
  const byTeamPlayerId = new Map<string, string>();
  for (const team of teams) {
    for (const row of team.roster) {
      byTeamPlayerId.set(row.id, byPlayerId.get(row.playerId) ?? row.playerId.slice(0, 8));
    }
  }
  return { byPlayerId, byTeamPlayerId };
}

export function hockeyStatusTranslationKey(status: string): string {
  if (!status) {
    return 'hockey.matches.status.scheduled';
  }
  const camel = `${status.charAt(0).toLowerCase()}${status.slice(1)}`;
  return `hockey.matches.status.${camel}`;
}

export function formatHockeyClock(totalSeconds: number): string {
  const safe = Math.max(0, totalSeconds);
  const minutes = Math.floor(safe / 60);
  const seconds = safe % 60;
  return `${minutes}:${seconds.toString().padStart(2, '0')}`;
}

export function hockeyEventPlayerLabel(
  match: HockeyMatchDto,
  event: HockeyMatchEventDto,
  playerNames: Map<string, string>
): string {
  if (!event.matchActivePlayerId) {
    return event.description?.trim() ?? '';
  }

  for (const side of match.matchTeams) {
    const player = side.activePlayers.find((row) => row.id === event.matchActivePlayerId);
    if (!player) {
      continue;
    }
    const name = playerNames.get(player.teamPlayerId) ?? '';
    return name ? `#${player.jerseyNumber} ${name}` : `#${player.jerseyNumber}`;
  }

  return event.description?.trim() ?? '';
}

export function toDateTimeLocalValue(iso: string): string {
  const date = new Date(iso);
  if (Number.isNaN(date.getTime())) {
    return '';
  }
  const pad = (value: number): string => String(value).padStart(2, '0');
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
}

export function splitHockeyDateTime(iso: string): { date: string; hours: string; minutes: string } {
  const local = toDateTimeLocalValue(iso);
  const [date, time] = local.split('T');
  const [hours, minutes] = (time ?? '00:00').split(':');
  return { date: date ?? '', hours: hours ?? '', minutes: minutes ?? '' };
}

export function joinHockeyDateTime(date: string, hours: string, minutes: string): string {
  const paddedHours = hours.padStart(2, '0');
  const paddedMinutes = minutes.padStart(2, '0');
  return new Date(`${date}T${paddedHours}:${paddedMinutes}`).toISOString();
}
