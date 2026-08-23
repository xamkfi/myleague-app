import { API_URL } from '../../constants/config';
import { authFetch } from '../utils/authFetch';
import { parseErrorResponse } from '../utils/ParseErrorResponse';
import type { ApiResponse } from '../../types/common/apiResponseType';
import type { ClubAdminClub, ClubAdminSport } from '../../types/clubAdmin/clubAdminTypes';
import type { FloorballMatchDto, FloorballPosition, FloorballTeamPlayer } from '../../types/floorball/floorballTypes';
import type { FootballMatchDto, FootballPosition, FootballTeamPlayer } from '../../types/football/footballTypes';
import type { HockeyMatchDto, HockeyTeamPlayerDto } from '../../types/hockey/hockeyTypes';

const BASE_URL = `${API_URL}/club-admin`;

async function parseResponse<T>(response: Response, defaultError: string): Promise<T> {
  const apiResponse: ApiResponse<T> = await response.json();
  if (!response.ok || !apiResponse.success) {
    const errorMessage = await parseErrorResponse(apiResponse, defaultError);
    throw new Error(errorMessage || defaultError);
  }
  return apiResponse.data;
}

export const clubAdminService = {
  /** Gets all clubs (with their teams in both sports) the current user manages. */
  getMyClubs: async (): Promise<ClubAdminClub[]> => {
    const response = await authFetch(`${BASE_URL}/my-clubs`);
    return parseResponse<ClubAdminClub[]>(response, 'Failed to load your clubs');
  },

  /** Gets the upcoming (scheduled) floorball matches for a managed team. */
  getFloorballUpcomingMatches: async (teamId: string): Promise<FloorballMatchDto[]> => {
    const response = await authFetch(`${BASE_URL}/floorball/teams/${teamId}/upcoming-matches`);
    return parseResponse<FloorballMatchDto[]>(response, 'Failed to load upcoming matches');
  },

  /** Gets the upcoming (scheduled) football matches for a managed team. */
  getFootballUpcomingMatches: async (teamId: string): Promise<FootballMatchDto[]> => {
    const response = await authFetch(`${BASE_URL}/football/teams/${teamId}/upcoming-matches`);
    return parseResponse<FootballMatchDto[]>(response, 'Failed to load upcoming matches');
  },

  /** Gets the upcoming (scheduled) hockey matches for a managed team. */
  getHockeyUpcomingMatches: async (teamId: string): Promise<HockeyMatchDto[]> => {
    const response = await authFetch(`${BASE_URL}/hockey/teams/${teamId}/upcoming-matches`);
    return parseResponse<HockeyMatchDto[]>(response, 'Failed to load upcoming matches');
  },

  /** Updates a roster player's jersey number on a managed team. */
  updateJerseyNumber: async (
    sport: ClubAdminSport,
    teamId: string,
    playerId: string,
    jerseyNumber: number | null,
  ): Promise<FloorballTeamPlayer | FootballTeamPlayer | HockeyTeamPlayerDto> => {
    const response = await authFetch(
      `${BASE_URL}/${sport}/teams/${teamId}/players/${playerId}/jersey-number`,
      {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ jerseyNumber }),
      },
    );
    return parseResponse<FloorballTeamPlayer | FootballTeamPlayer | HockeyTeamPlayerDto>(
      response,
      'Failed to update jersey number',
    );
  },

  /** Announces the active roster for one team in an upcoming floorball match. */
  announceFloorballRoster: async (
    matchId: string,
    teamId: string,
    payload: { players: { playerId: string; position: FloorballPosition }[]; goalieId: string | null },
  ): Promise<FloorballMatchDto> => {
    const response = await authFetch(
      `${BASE_URL}/floorball/matches/${matchId}/teams/${teamId}/roster`,
      {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload),
      },
    );
    return parseResponse<FloorballMatchDto>(response, 'Failed to announce match roster');
  },

  /** Announces the lineup for one team in an upcoming football match. */
  announceFootballLineup: async (
    matchId: string,
    teamId: string,
    players: { playerId: string; position: FootballPosition; isOnField: boolean }[],
  ): Promise<FootballMatchDto> => {
    const response = await authFetch(
      `${BASE_URL}/football/matches/${matchId}/teams/${teamId}/lineup`,
      {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ players }),
      },
    );
    return parseResponse<FootballMatchDto>(response, 'Failed to announce match lineup');
  },

  /** Announces the match-day roster for one team in an upcoming hockey match. */
  announceHockeyRoster: async (
    matchId: string,
    teamId: string,
    teamPlayerIds: string[],
  ): Promise<HockeyMatchDto> => {
    const response = await authFetch(
      `${BASE_URL}/hockey/matches/${matchId}/teams/${teamId}/roster`,
      {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ teamPlayerIds }),
      },
    );
    return parseResponse<HockeyMatchDto>(response, 'Failed to announce match roster');
  },
};
