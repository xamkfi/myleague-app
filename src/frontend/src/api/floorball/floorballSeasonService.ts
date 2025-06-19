import type { 
  ApiResponse,
  FloorballTeam,
  FloorballDivision
} from '../../types/floorball/floorballTypes';

const API_URL = import.meta.env.VITE_API_URL || '/api';

/**
 * Helper function to parse error responses properly
 */
const parseErrorResponse = async (response: Response, defaultMessage: string): Promise<string> => {
  try {
    const responseText = await response.text();
    console.error('API Error Response (raw):', responseText);
    
    if (responseText) {
      try {
        const errorResponse = JSON.parse(responseText);
        console.error('API Error Response (parsed):', errorResponse);
        
        if (errorResponse.errors && Array.isArray(errorResponse.errors)) {
          return errorResponse.errors.join(', ');
        } else if (errorResponse.message) {
          return errorResponse.message;
        } else {
          return responseText;
        }
      } catch {
        // If JSON parsing fails, use the raw text
        return responseText;
      }
    }
  } catch (readError) {
    console.error('Error reading response:', readError);
  }
  
  return `HTTP ${response.status}: ${defaultMessage}`;
};

export interface FloorballSeasonDto {
  id: string;
  name: string;
  division: FloorballDivision;
  startDate: string;
  endDate: string;
  isActive: boolean;
  isCompleted: boolean;
  teams: FloorballTeam[];
  matches: unknown[];
}

export interface CreateFloorballSeasonRequest {
  name: string;
  startDate: string;
  endDate: string;
  division: FloorballDivision;
}

export interface UpdateFloorballSeasonRequest {
  name: string;
  startDate: string;
  endDate: string;
  division: FloorballDivision;
}

export const floorballSeasonService = {
  /**
   * Get all floorball seasons
   */
  getAll: async (): Promise<ApiResponse<FloorballSeasonDto[]>> => {
    try {
      const url = `${API_URL}/FloorballSeason`;
      console.log('Fetching seasons from URL:', url);
      
      const response = await fetch(url);
      
      console.log('Response status:', response.status);
      console.log('Response ok:', response.ok);
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to fetch floorball seasons');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FloorballSeasonDto[]> = await response.json();
      console.log('API Response:', apiResponse);
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to fetch floorball seasons');
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in floorballSeasonService.getAll:', error);
      throw error;
    }
  },

  /**
   * Get active floorball seasons
   */
  getActive: async (): Promise<ApiResponse<FloorballSeasonDto[]>> => {
    try {
      const url = `${API_URL}/FloorballSeason/active`;
      console.log('Fetching active seasons from URL:', url);
      
      const response = await fetch(url);
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to fetch active floorball seasons');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FloorballSeasonDto[]> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to fetch active floorball seasons');
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in floorballSeasonService.getActive:', error);
      throw error;
    }
  },

  /**
   * Get a floorball season by ID
   */
  getById: async (id: string): Promise<ApiResponse<FloorballSeasonDto>> => {
    try {
      const url = `${API_URL}/FloorballSeason/${id}`;
      console.log('Fetching season from URL:', url);
      
      const response = await fetch(url);
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to fetch floorball season');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FloorballSeasonDto> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to fetch floorball season');
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in floorballSeasonService.getById:', error);
      throw error;
    }
  },

  /**
   * Create a new floorball season
   */
  create: async (data: CreateFloorballSeasonRequest): Promise<ApiResponse<FloorballSeasonDto>> => {
    try {
      console.log('Creating season:', data);
      
      const response = await fetch(`${API_URL}/FloorballSeason`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(data),
      });
      
      console.log('Create response status:', response.status);
      console.log('Create response ok:', response.ok);
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to create floorball season');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FloorballSeasonDto> = await response.json();
      console.log('Create API Response:', apiResponse);
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to create floorball season');
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in floorballSeasonService.create:', error);
      throw error;
    }
  },

  /**
   * Update a floorball season
   */
  update: async (id: string, data: UpdateFloorballSeasonRequest): Promise<ApiResponse<FloorballSeasonDto>> => {
    try {
      console.log('Updating season with ID:', id);
      console.log('Update data:', data);
      
      const response = await fetch(`${API_URL}/FloorballSeason/${id}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(data),
      });
      
      console.log('Update response status:', response.status);
      console.log('Update response ok:', response.ok);
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to update floorball season');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FloorballSeasonDto> = await response.json();
      console.log('Update API Response:', apiResponse);
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to update floorball season');
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in floorballSeasonService.update:', error);
      throw error;
    }
  },

  /**
   * Delete a floorball season
   */
  delete: async (id: string): Promise<void> => {
    try {
      console.log('Deleting season with ID:', id);
      
      const response = await fetch(`${API_URL}/FloorballSeason/${id}`, {
        method: 'DELETE',
      });
      
      console.log('Delete response status:', response.status);
      console.log('Delete response ok:', response.ok);
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to delete floorball season');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<void> = await response.json();
      console.log('Delete API Response:', apiResponse);
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to delete floorball season');
      }
    } catch (error) {
      console.error('Error in floorballSeasonService.delete:', error);
      throw error;
    }
  },

  /**
   * Activate a floorball season
   */
  activate: async (id: string): Promise<ApiResponse<FloorballSeasonDto>> => {
    try {
      console.log('Activating season with ID:', id);
      
      const response = await fetch(`${API_URL}/FloorballSeason/${id}/activate`, {
        method: 'PUT',
      });
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to activate floorball season');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FloorballSeasonDto> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to activate floorball season');
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in floorballSeasonService.activate:', error);
      throw error;
    }
  },

  /**
   * Deactivate a floorball season
   */
  deactivate: async (id: string): Promise<ApiResponse<FloorballSeasonDto>> => {
    try {
      console.log('Deactivating season with ID:', id);
      
      const response = await fetch(`${API_URL}/FloorballSeason/${id}/deactivate`, {
        method: 'PUT',
      });
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to deactivate floorball season');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FloorballSeasonDto> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to deactivate floorball season');
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in floorballSeasonService.deactivate:', error);
      throw error;
    }
  },

  /**
   * Complete a floorball season
   */
  complete: async (id: string): Promise<ApiResponse<FloorballSeasonDto>> => {
    try {
      console.log('Completing season with ID:', id);
      
      const response = await fetch(`${API_URL}/FloorballSeason/${id}/complete`, {
        method: 'PUT',
      });
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to complete floorball season');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FloorballSeasonDto> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to complete floorball season');
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in floorballSeasonService.complete:', error);
      throw error;
    }
  },

  /**
   * Add a team to a floorball season
   */
  addTeamToSeason: async (seasonId: string, teamId: string): Promise<ApiResponse<FloorballSeasonDto>> => {
    try {
      console.log('Adding team to season:', { seasonId, teamId });
      
      const response = await fetch(`${API_URL}/FloorballSeason/${seasonId}/teams/${teamId}`, {
        method: 'POST',
      });
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to add team to season');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FloorballSeasonDto> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to add team to season');
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in floorballSeasonService.addTeamToSeason:', error);
      throw error;
    }
  },

  /**
   * Remove a team from a floorball season
   */
  removeTeamFromSeason: async (seasonId: string, teamId: string): Promise<ApiResponse<FloorballSeasonDto>> => {
    try {
      console.log('Removing team from season:', { seasonId, teamId });
      
      const response = await fetch(`${API_URL}/FloorballSeason/${seasonId}/teams/${teamId}`, {
        method: 'DELETE',
      });
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to remove team from season');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FloorballSeasonDto> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to remove team from season');
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in floorballSeasonService.removeTeamFromSeason:', error);
      throw error;
    }
  }
}; 