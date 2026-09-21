import type { ApiResponse } from '../../types/common/apiResponseType';
import { API_URL } from '../../constants/config';
import { parseErrorResponse } from '../utils/ParseErrorResponse';

export type PersonSportKind = 'floorball' | 'football' | 'hockey';

export interface PersonSportPlayer {
  sport: PersonSportKind;
  playerId: string;
}

export interface PersonPlayerLicence {
  sport: PersonSportKind;
  teamId: string;
  teamName: string;
  competitionId: string | null;
  competitionName: string | null;
  isActive: boolean;
}

export interface PersonPlayerSports {
  personId: string;
  fullName: string;
  birthDate: string | null;
  sports: PersonSportPlayer[];
  licences: PersonPlayerLicence[];
}

export const personPlayerSportsService = {
  getById: async (id: string): Promise<PersonPlayerSports> => {
    const response = await fetch(`${API_URL}/persons/${id}/player-sports`);
    if (!response.ok) {
      throw new Error(await parseErrorResponse(response, 'Failed to fetch player sports'));
    }

    const apiResponse: ApiResponse<PersonPlayerSports> = await response.json();
    if (!apiResponse.success || !apiResponse.data) {
      throw new Error(await parseErrorResponse(apiResponse, 'Failed to fetch player sports'));
    }

    return apiResponse.data;
  },
};
