import { floorballSeasonService } from '../../../api/floorball/floorballSeasonService';
import { floorballTeamService } from '../../../api/floorball/floorballTeamService';
import { floorballTournamentService } from '../../../api/floorball/floorballTournamentService';
import { footballSeasonService } from '../../../api/football/footballSeasonService';
import { footballTeamService } from '../../../api/football/footballTeamService';
import { footballTournamentService } from '../../../api/football/footballTournamentService';
import { hockeySeasonService } from '../../../api/hockey/hockeySeasonService';
import { hockeyTeamService } from '../../../api/hockey/hockeyTeamService';
import { hockeyTournamentService } from '../../../api/hockey/hockeyTournamentService';

export type TeamSport = 'floorball' | 'football' | 'hockey';
export type ClubTeamCompetitionKind = 'season' | 'tournament';

export interface ClubTeamCompetition {
  id: string;
  name: string;
  kind: ClubTeamCompetitionKind;
  startDate: string;
}

interface SeasonEnrollment {
  id: string;
  name: string;
  startDate: string;
  seasonDivisions?: Array<{ teamIds?: string[] }>;
  teams?: Array<{ id: string }>;
}

interface TournamentEnrollment {
  id: string;
  name: string;
  startDate: string;
  groups?: Array<{ teams?: Array<{ teamId: string }> }>;
}

function isEnrolledInSeason(season: SeasonEnrollment, teamId: string): boolean {
  return (
    season.seasonDivisions?.some((division) => division.teamIds?.includes(teamId)) === true
    || season.teams?.some((team) => team.id === teamId) === true
  );
}

function isEnrolledInTournament(tournament: TournamentEnrollment, teamId: string): boolean {
  return tournament.groups?.some((group) => group.teams?.some((team) => team.teamId === teamId)) === true;
}

function sortCompetitions(items: ClubTeamCompetition[]): ClubTeamCompetition[] {
  return [...items].sort((left, right) => {
    const leftTime = left.startDate ? new Date(left.startDate).getTime() : 0;
    const rightTime = right.startDate ? new Date(right.startDate).getTime() : 0;
    return rightTime - leftTime;
  });
}

function toSeasonRows(seasons: SeasonEnrollment[], teamId: string): ClubTeamCompetition[] {
  return seasons
    .filter((season) => isEnrolledInSeason(season, teamId))
    .map((season) => ({
      id: season.id,
      name: season.name,
      kind: 'season' as const,
      startDate: season.startDate,
    }));
}

function toTournamentRows(tournaments: TournamentEnrollment[], teamId: string): ClubTeamCompetition[] {
  return tournaments
    .filter((tournament) => isEnrolledInTournament(tournament, teamId))
    .map((tournament) => ({
      id: tournament.id,
      name: tournament.name,
      kind: 'tournament' as const,
      startDate: tournament.startDate,
    }));
}

export async function loadClubTeamCompetitions(
  sport: TeamSport,
  teamId: string,
): Promise<ClubTeamCompetition[]> {
  if (sport === 'floorball') {
    const [seasonsResponse, tournamentsResponse] = await Promise.all([
      floorballSeasonService.getAll(),
      floorballTournamentService.getAll(),
    ]);
    return sortCompetitions([
      ...toSeasonRows(seasonsResponse.data ?? [], teamId),
      ...toTournamentRows(tournamentsResponse.data ?? [], teamId),
    ]);
  }

  if (sport === 'football') {
    const [seasonsResponse, tournamentsResponse] = await Promise.all([
      footballSeasonService.getAll(),
      footballTournamentService.getAll(),
    ]);
    return sortCompetitions([
      ...toSeasonRows(seasonsResponse.data ?? [], teamId),
      ...toTournamentRows(tournamentsResponse.data ?? [], teamId),
    ]);
  }

  const [seasons, tournaments] = await Promise.all([
    hockeySeasonService.getAll(),
    hockeyTournamentService.getAll(),
  ]);

  const seasonRows = seasons
    .filter((season) => season.teams?.some((team) => team.teamId === teamId))
    .map((season) => ({
      id: season.id,
      name: season.name,
      kind: 'season' as const,
      startDate: season.startDate,
    }));

  const tournamentRows = tournaments
    .filter((tournament) =>
      tournament.teams?.some((team) => team.teamId === teamId)
      || tournament.groups?.some((group) =>
        group.teams.some((row) =>
          tournament.teams.some((team) => team.id === row.competitionTeamId && team.teamId === teamId),
        ),
      ),
    )
    .map((tournament) => ({
      id: tournament.id,
      name: tournament.name,
      kind: 'tournament' as const,
      startDate: tournament.startDate,
    }));

  return sortCompetitions([...seasonRows, ...tournamentRows]);
}

export async function loadCompetitionRosterCount(
  sport: TeamSport,
  teamId: string,
  competitionId: string,
): Promise<number> {
  if (sport === 'football') {
    const team = await footballTeamService.getById(teamId, competitionId);
    return Array.isArray(team.roster) ? team.roster.length : 0;
  }

  if (sport === 'hockey') {
    const team = await hockeyTeamService.getById(teamId, competitionId);
    return Array.isArray(team.roster) ? team.roster.length : 0;
  }

  const team = await floorballTeamService.getById(teamId, competitionId);
  return Array.isArray(team.roster) ? team.roster.length : 0;
}

export function getTeamEditPath(sport: TeamSport, teamId: string): string {
  return `/admin/${sport}/teams/${teamId}/edit`;
}

export function getTeamRosterPath(sport: TeamSport, teamId: string, competitionId: string): string {
  return `/admin/${sport}/teams/${teamId}/roster?competitionId=${encodeURIComponent(competitionId)}`;
}
