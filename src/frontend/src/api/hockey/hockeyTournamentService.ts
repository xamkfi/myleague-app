import type {
  CreateHockeyTournamentRequest,
  HockeyTournamentDto,
} from '../../types/hockey/hockeyTypes';
import { hockeyRequest, jsonBody, withTeamCategory } from './hockeyApi';

const action = (
  tournamentId: string,
  verb: string,
  fallback: string,
): Promise<HockeyTournamentDto> =>
  hockeyRequest<HockeyTournamentDto>(`/HockeyTournament/${tournamentId}/${verb}`, fallback, {
    method: 'POST',
  });

export const hockeyTournamentService = {
  getAll: (teamCategory?: string): Promise<HockeyTournamentDto[]> =>
    hockeyRequest<HockeyTournamentDto[]>(
      withTeamCategory('/HockeyTournament', teamCategory),
      'Failed to fetch hockey tournaments',
    ),

  getActive: (teamCategory?: string): Promise<HockeyTournamentDto[]> =>
    hockeyRequest<HockeyTournamentDto[]>(
      withTeamCategory('/HockeyTournament/active', teamCategory),
      'Failed to fetch active hockey tournaments',
    ),

  getById: (id: string): Promise<HockeyTournamentDto> =>
    hockeyRequest<HockeyTournamentDto>(`/HockeyTournament/${id}`, 'Failed to fetch hockey tournament'),

  create: (data: CreateHockeyTournamentRequest): Promise<HockeyTournamentDto> =>
    hockeyRequest<HockeyTournamentDto>('/HockeyTournament', 'Failed to create hockey tournament', {
      method: 'POST',
      ...jsonBody(data),
    }),

  update: (
    id: string,
    data: {
      name: string;
      startDate: string;
      endDate: string;
      venue?: string;
      contentHtml?: string;
      teamCategory: string;
    },
  ): Promise<HockeyTournamentDto> =>
    hockeyRequest<HockeyTournamentDto>(`/HockeyTournament/${id}`, 'Failed to update hockey tournament', {
      method: 'PUT',
      ...jsonBody(data),
    }),

  publish: (id: string): Promise<HockeyTournamentDto> =>
    action(id, 'publish', 'Failed to publish tournament'),
  openRegistration: (id: string): Promise<HockeyTournamentDto> =>
    action(id, 'open-registration', 'Failed to open registration'),
  activate: (id: string): Promise<HockeyTournamentDto> =>
    action(id, 'activate', 'Failed to activate tournament'),
  deactivate: (id: string): Promise<HockeyTournamentDto> =>
    action(id, 'deactivate', 'Failed to deactivate tournament'),
  complete: (id: string): Promise<HockeyTournamentDto> =>
    action(id, 'complete', 'Failed to complete tournament'),
  cancel: (id: string): Promise<HockeyTournamentDto> =>
    action(id, 'cancel', 'Failed to cancel tournament'),
  startGroupStage: (id: string): Promise<HockeyTournamentDto> =>
    action(id, 'start-group-stage', 'Failed to start group stage'),
  startPlayoffStage: (id: string): Promise<HockeyTournamentDto> =>
    action(id, 'start-playoff-stage', 'Failed to start playoffs'),

  addTeam: (tournamentId: string, teamId: string, seed?: number): Promise<unknown> =>
    hockeyRequest(`/HockeyTournament/${tournamentId}/teams`, 'Failed to add team to tournament', {
      method: 'POST',
      ...jsonBody({ teamId, seed }),
    }),

  removeTeam: (tournamentId: string, teamId: string): Promise<HockeyTournamentDto> =>
    hockeyRequest<HockeyTournamentDto>(
      `/HockeyTournament/${tournamentId}/teams/${teamId}`,
      'Failed to remove team from tournament',
      { method: 'DELETE' },
    ),

  createGroup: (tournamentId: string, name: string): Promise<HockeyTournamentDto> =>
    hockeyRequest<HockeyTournamentDto>(
      `/HockeyTournament/${tournamentId}/groups`,
      'Failed to create group',
      { method: 'POST', ...jsonBody({ name }) },
    ),

  deleteGroup: (tournamentId: string, groupId: string): Promise<HockeyTournamentDto> =>
    hockeyRequest<HockeyTournamentDto>(
      `/HockeyTournament/${tournamentId}/groups/${groupId}`,
      'Failed to delete group',
      { method: 'DELETE' },
    ),

  addTeamToGroup: (
    tournamentId: string,
    groupId: string,
    competitionTeamId: string,
    seed?: number,
  ): Promise<HockeyTournamentDto> =>
    hockeyRequest<HockeyTournamentDto>(
      `/HockeyTournament/${tournamentId}/groups/${groupId}/teams`,
      'Failed to add team to group',
      { method: 'POST', ...jsonBody({ competitionTeamId, seed }) },
    ),

  removeTeamFromGroup: (
    tournamentId: string,
    groupId: string,
    competitionTeamId: string,
  ): Promise<HockeyTournamentDto> =>
    hockeyRequest<HockeyTournamentDto>(
      `/HockeyTournament/${tournamentId}/groups/${groupId}/teams/${competitionTeamId}`,
      'Failed to remove team from group',
      { method: 'DELETE' },
    ),
};
