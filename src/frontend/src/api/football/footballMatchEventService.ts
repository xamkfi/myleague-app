import type {
  ApiResponse,
  FootballMatchDto,
  FootballCardType,
  FootballGoalType,
  LineupPlayerRequest
} from '../../types/football/footballTypes';
import { authFetch } from '../utils/authFetch';
import { parseErrorResponse } from '../utils/ParseErrorResponse';
import { API_URL } from '../../constants/config';

const MATCHES_PATH = 'football-matches';

export interface FootballDomainEventDto {
  eventType: string;
  occurredOn: string;
  data: Record<string, unknown>;
}

export interface RecordGoalEventRequest {
  matchId: string;
  teamId: string;
  playerId: string;
  assisterId?: string;
  periodNumber: number;
  timeInSeconds: number;
  goalType?: FootballGoalType | number;
  description?: string;
}

export interface RecordCardEventRequest {
  matchId: string;
  teamId: string;
  playerId: string;
  cardType: FootballCardType | number | string;
  periodNumber: number;
  timeInSeconds: number;
  description?: string;
}

export interface RecordSubstitutionEventRequest {
  matchId: string;
  teamId: string;
  playerOffId: string;
  playerOnId: string;
  periodNumber: number;
  timeInSeconds: number;
  description?: string;
}

const handleApiResponse = async <T>(response: Response, defaultMessage = 'API request failed'): Promise<ApiResponse<T>> => {
  if (!response.ok) {
    throw new Error(await parseErrorResponse(response, defaultMessage));
  }

  const apiResponse: ApiResponse<T> = await response.json();

  if (!apiResponse.success) {
    throw new Error(await parseErrorResponse(apiResponse, defaultMessage));
  }

  return apiResponse;
};

const normalizeId = (id?: string | null): string | undefined => {
  if (!id) {
    return undefined;
  }
  const trimmed = id.trim();
  return trimmed.length > 0 ? trimmed : undefined;
};

export const footballMatchEventService = {
  getMatchEvents: async (matchId: string): Promise<ApiResponse<FootballDomainEventDto[]>> => {
    const response = await authFetch(`${API_URL}/${MATCHES_PATH}/by-id/${matchId}`, {
      method: 'GET',
      headers: { 'Content-Type': 'application/json' }
    });

    const apiResponse = await handleApiResponse<FootballMatchDto>(response);
    const match = apiResponse.data;
    const occurredOn = match?.scheduledDateTime ?? new Date().toISOString();

    const goalEvents: FootballDomainEventDto[] = (match?.goalEvents ?? []).map((g) => ({
      eventType: 'FootballGoalScoredEvent',
      occurredOn,
      data: {
        matchId,
        eventId: g.id,
        teamId: g.teamId,
        playerId: g.scoringPlayerId,
        periodNumber: g.periodNumber,
        timeInSeconds: g.timeInSeconds,
        assisterId: g.assistingPlayerId,
        goalType: g.goalType ?? null
      }
    }));

    const cardEvents: FootballDomainEventDto[] = (match?.cardEvents ?? []).map((c) => ({
      eventType: 'FootballCardAssignedEvent',
      occurredOn,
      data: {
        matchId,
        eventId: c.id,
        teamId: c.teamId,
        playerId: c.playerId,
        periodNumber: c.periodNumber,
        timeInSeconds: c.timeInSeconds,
        cardType: c.cardType,
        description: c.description
      }
    }));

    const substitutionEvents: FootballDomainEventDto[] = (match?.substitutionEvents ?? []).map((s) => ({
      eventType: 'FootballSubstitutionRecordedEvent',
      occurredOn,
      data: {
        matchId,
        eventId: s.id,
        teamId: s.teamId,
        playerOffId: s.playerOffId,
        playerOnId: s.playerOnId,
        periodNumber: s.periodNumber,
        timeInSeconds: s.timeInSeconds,
        description: s.description
      }
    }));

    return {
      success: true,
      data: [...goalEvents, ...cardEvents, ...substitutionEvents],
      message: 'Match events synthesized from match DTO',
      errors: []
    };
  },

  recordGoal: async (data: RecordGoalEventRequest): Promise<ApiResponse<FootballMatchDto>> => {
    const payload = {
      scoringTeamId: data.teamId,
      scoringPlayerId: data.playerId,
      assistingPlayerId: normalizeId(data.assisterId),
      periodNumber: data.periodNumber,
      timeInSeconds: data.timeInSeconds,
      description: data.description ?? '',
      goalType: data.goalType ?? null
    };

    const response = await authFetch(`${API_URL}/${MATCHES_PATH}/${data.matchId}/events/goal`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    return handleApiResponse<FootballMatchDto>(response);
  },

  recordCard: async (data: RecordCardEventRequest): Promise<ApiResponse<FootballMatchDto>> => {
    const payload = {
      teamId: data.teamId,
      playerId: data.playerId,
      cardType: data.cardType,
      periodNumber: data.periodNumber,
      timeInSeconds: data.timeInSeconds,
      description: data.description ?? ''
    };

    const response = await authFetch(`${API_URL}/${MATCHES_PATH}/${data.matchId}/events/card`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    return handleApiResponse<FootballMatchDto>(response);
  },

  recordSubstitution: async (data: RecordSubstitutionEventRequest): Promise<ApiResponse<FootballMatchDto>> => {
    const payload = {
      teamId: data.teamId,
      playerOffId: data.playerOffId,
      playerOnId: data.playerOnId,
      periodNumber: data.periodNumber,
      timeInSeconds: data.timeInSeconds,
      description: data.description ?? ''
    };

    const response = await authFetch(`${API_URL}/${MATCHES_PATH}/${data.matchId}/events/substitution`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    return handleApiResponse<FootballMatchDto>(response);
  },

  startPeriod: async (matchId: string, periodNumber: number): Promise<ApiResponse<FootballMatchDto>> => {
    if (periodNumber === 1) {
      const response = await authFetch(`${API_URL}/${MATCHES_PATH}/${matchId}/start`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' }
      });
      return handleApiResponse<FootballMatchDto>(response);
    }

    const response = await authFetch(`${API_URL}/${MATCHES_PATH}/${matchId}/events/periods/${periodNumber}/start`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' }
    });
    return handleApiResponse<FootballMatchDto>(response);
  },

  endPeriod: async (matchId: string, periodNumber: number): Promise<ApiResponse<FootballMatchDto>> => {
    const response = await authFetch(`${API_URL}/${MATCHES_PATH}/${matchId}/events/periods/${periodNumber}/end`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' }
    });
    return handleApiResponse<FootballMatchDto>(response);
  },

  recordExtraTime: async (matchId: string): Promise<ApiResponse<FootballMatchDto>> => {
    const response = await authFetch(`${API_URL}/${MATCHES_PATH}/${matchId}/events/extra-time`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' }
    });
    return handleApiResponse<FootballMatchDto>(response);
  },

  recordPenaltyShootout: async (matchId: string): Promise<ApiResponse<FootballMatchDto>> => {
    const response = await authFetch(`${API_URL}/${MATCHES_PATH}/${matchId}/events/penalty-shootout`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' }
    });
    return handleApiResponse<FootballMatchDto>(response);
  },

  cancelMatch: async (matchId: string): Promise<ApiResponse<FootballMatchDto>> => {
    const response = await authFetch(`${API_URL}/${MATCHES_PATH}/${matchId}/cancel`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' }
    });
    return handleApiResponse<FootballMatchDto>(response);
  },

  reactivateMatch: async (matchId: string): Promise<ApiResponse<FootballMatchDto>> => {
    const response = await authFetch(`${API_URL}/${MATCHES_PATH}/${matchId}/reactivate`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' }
    });
    return handleApiResponse<FootballMatchDto>(response);
  },

  setLineup: async (
    matchId: string,
    teamId: string,
    players: LineupPlayerRequest[]
  ): Promise<ApiResponse<FootballMatchDto>> => {
    const response = await authFetch(`${API_URL}/${MATCHES_PATH}/${matchId}/teams/${teamId}/lineup`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ players })
    });
    return handleApiResponse<FootballMatchDto>(response);
  }
};
