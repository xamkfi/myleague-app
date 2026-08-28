import type { ApiResponse } from '../../types/common/apiResponseType';
import { API_URL } from '../../constants/config';

export interface GlobalSearchPerson {
  personId: string;
  firstName: string;
  lastName: string;
  teamId?: string | null;
  teamName?: string | null;
  clubId?: string | null;
  clubName?: string | null;
  sport?: string | null;
}
export interface GlobalSearchTeam {
  teamId: string;
  teamName: string;
  clubId?: string | null;
  clubName?: string | null;
  sport?: string | null;
}
export interface GlobalSearchResult {
  person: GlobalSearchPerson[];
  team: GlobalSearchTeam[];
  clubNames: string[];
}

export const globalSearchService = {
  search: async (term: string): Promise<ApiResponse<GlobalSearchResult>> => {
    const url = `${API_URL}/Search?term=${encodeURIComponent(term)}`;
    const response = await fetch(url);
    const apiResponse: ApiResponse<GlobalSearchResult> = await response.json();
    
    if (!response.ok || !apiResponse.success) {
      throw new Error(apiResponse.message + apiResponse.errors?.join(', '));
    }
    return apiResponse;
  },
}; 