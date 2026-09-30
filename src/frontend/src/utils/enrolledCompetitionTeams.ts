import { useEffect, useState } from 'react';
import { floorballSeasonService } from '../api/floorball/floorballSeasonService';
import { floorballTournamentService } from '../api/floorball/floorballTournamentService';
import { footballSeasonService } from '../api/football/footballSeasonService';
import { footballTournamentService } from '../api/football/footballTournamentService';
import { hockeySeasonService } from '../api/hockey/hockeySeasonService';
import { hockeyTeamService } from '../api/hockey/hockeyTeamService';
import { hockeyTournamentService } from '../api/hockey/hockeyTournamentService';

export interface TeamOption {
  id: string;
  name: string;
  [key: string]: unknown;
}

export type CompetitionKind = 'season' | 'tournament';

export interface EnrolledTeamCatalog {
  competitionId: string | null;
  teams: TeamOption[] | null;
  failed: boolean;
}

const IDLE_CATALOG: EnrolledTeamCatalog = { competitionId: null, teams: null, failed: false };

export function competitionKindFromMatch(match: {
  competitionType?: string | null;
  tournamentGroupId?: string | null;
  tournamentStage?: string | null;
}): CompetitionKind {
  if (match.competitionType === 'Tournament') {
    return 'tournament';
  }
  if (match.competitionType === 'Season') {
    return 'season';
  }
  if (match.tournamentGroupId) {
    return 'tournament';
  }
  if (match.tournamentStage && match.tournamentStage !== 'None') {
    return 'tournament';
  }
  return 'season';
}

export function hockeyCompetitionKind(matchType: string): CompetitionKind {
  return matchType.startsWith('Tournament') ? 'tournament' : 'season';
}

export function uniqueTeamOptions(teams: Array<{ id: string; name: string }>): TeamOption[] {
  const seen = new Set<string>();
  const result: TeamOption[] = [];
  for (const team of teams) {
    const name = team.name.trim();
    if (!team.id || !name || seen.has(team.id)) {
      continue;
    }
    seen.add(team.id);
    result.push({ id: team.id, name });
  }
  result.sort((left, right) => left.name.localeCompare(right.name, 'fi'));
  return result;
}

export function toTeamSearchResult(
  teams: readonly TeamOption[],
  query: string,
  page: number,
  pinned: readonly TeamOption[] = [],
): { data: TeamOption[]; pagination: { hasNextPage: boolean; totalCount: number } } {
  if (page > 1) {
    return { data: [], pagination: { hasNextPage: false, totalCount: 0 } };
  }
  const needle = query.trim().toLowerCase();
  const matches = (team: TeamOption): boolean => !needle || team.name.toLowerCase().includes(needle);
  const visiblePinned = pinned.filter(matches);
  const seen = new Set(visiblePinned.map((team) => team.id));
  const data = [...visiblePinned, ...teams.filter((team) => matches(team) && !seen.has(team.id))];
  return { data, pagination: { hasNextPage: false, totalCount: data.length } };
}

export async function loadFloorballEnrolledTeams(
  competitionId: string,
  kind: CompetitionKind,
): Promise<TeamOption[]> {
  if (kind === 'tournament') {
    const response = await floorballTournamentService.getById(competitionId, true);
    const groups = response.data?.groups ?? [];
    return uniqueTeamOptions(
      groups.flatMap((group) => group.teams.map((team) => ({ id: team.teamId, name: team.teamName }))),
    );
  }
  const response = await floorballSeasonService.getById(competitionId, true);
  return uniqueTeamOptions((response.data?.teams ?? []).map((team) => ({ id: team.id, name: team.name })));
}

export async function loadFootballEnrolledTeams(
  competitionId: string,
  kind: CompetitionKind,
): Promise<TeamOption[]> {
  if (kind === 'tournament') {
    const response = await footballTournamentService.getById(competitionId, true);
    const groups = response.data?.groups ?? [];
    return uniqueTeamOptions(
      groups.flatMap((group) => group.teams.map((team) => ({ id: team.teamId, name: team.teamName }))),
    );
  }
  const response = await footballSeasonService.getById(competitionId, true);
  return uniqueTeamOptions((response.data?.teams ?? []).map((team) => ({ id: team.id, name: team.name })));
}

export async function loadHockeyEnrolledTeams(
  competitionId: string,
  kind: CompetitionKind,
): Promise<TeamOption[]> {
  const [catalog, memberships] = await Promise.all([
    hockeyTeamService.getAll(),
    kind === 'tournament'
      ? hockeyTournamentService.getById(competitionId, true).then((tournament) => tournament.teams)
      : hockeySeasonService.getById(competitionId, true).then((season) => season.teams),
  ]);
  const names = new Map(catalog.map((team) => [team.id, team.name]));
  return uniqueTeamOptions(
    memberships
      .filter((membership) => membership.isActive)
      .map((membership) => ({ id: membership.teamId, name: names.get(membership.teamId) ?? '' })),
  );
}

export function useEnrolledTeams(
  competitionId: string | null | undefined,
  kind: CompetitionKind,
  sport: 'floorball' | 'football' | 'hockey',
): EnrolledTeamCatalog {
  const [catalog, setCatalog] = useState<EnrolledTeamCatalog>(IDLE_CATALOG);

  useEffect(() => {
    if (!competitionId) {
      setCatalog({ competitionId: null, teams: [], failed: false });
      return;
    }

    let cancelled = false;
    setCatalog({ competitionId, teams: null, failed: false });

    const load = (): Promise<TeamOption[]> => {
      if (sport === 'floorball') {
        return loadFloorballEnrolledTeams(competitionId, kind);
      }
      if (sport === 'football') {
        return loadFootballEnrolledTeams(competitionId, kind);
      }
      return loadHockeyEnrolledTeams(competitionId, kind);
    };

    void load()
      .then((teams) => {
        if (!cancelled) {
          setCatalog({ competitionId, teams, failed: false });
        }
      })
      .catch(() => {
        if (!cancelled) {
          setCatalog({ competitionId, teams: [], failed: true });
        }
      });

    return () => {
      cancelled = true;
    };
  }, [competitionId, kind, sport]);

  return catalog;
}
