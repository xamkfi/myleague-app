import type { 
  ApiResponse,
  PaginatedApiResponse,
  FloorballMatchDto,
  CreateFloorballMatchRequest,
  UpdateFloorballMatchRequest,
  GetFloorballMatchesRequest
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

export const floorballMatchService = {
  /**
   * Get all floorball matches with pagination and filtering
   */
  getAll: async (params?: GetFloorballMatchesRequest): Promise<PaginatedApiResponse<FloorballMatchDto>> => {
    try {
      const searchParams = new URLSearchParams();
      
      if (params?.page) searchParams.append('page', params.page.toString());
      if (params?.pageSize) searchParams.append('pageSize', params.pageSize.toString());
      if (params?.seasonId) searchParams.append('seasonId', params.seasonId);
      if (params?.teamId) searchParams.append('teamId', params.teamId);
      if (params?.startDate) searchParams.append('startDate', params.startDate);
      if (params?.endDate) searchParams.append('endDate', params.endDate);
      if (params?.sortOrder) searchParams.append('sortOrder', params.sortOrder);

      const url = `${API_URL}/FloorballMatch?${searchParams.toString()}`;
      console.log('Fetching matches from URL:', url);
      
      const response = await fetch(url);
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to fetch floorball matches');
        throw new Error(errorMessage);
      }
      
      const apiResponse: PaginatedApiResponse<FloorballMatchDto> = await response.json();
      console.log('API Response:', apiResponse);
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to fetch floorball matches');
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in floorballMatchService.getAll:', error);
      throw error;
    }
  },

  /**
   * Get matches by season ID
   */
  getBySeason: async (seasonId: string): Promise<ApiResponse<FloorballMatchDto[]>> => {
    try {
      const url = `${API_URL}/FloorballMatch/by-seasonId/${seasonId}`;
      console.log('Fetching matches by season from URL:', url);
      
      const response = await fetch(url);
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to fetch matches by season');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FloorballMatchDto[]> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to fetch matches by season');
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in floorballMatchService.getBySeason:', error);
      throw error;
    }
  },

  /**
   * Get matches by team ID
   */
  getByTeam: async (teamId: string): Promise<ApiResponse<FloorballMatchDto[]>> => {
    try {
      const url = `${API_URL}/FloorballMatch/by-team/${teamId}`;
      console.log('Fetching matches by team from URL:', url);
      
      const response = await fetch(url);
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to fetch matches by team');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FloorballMatchDto[]> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to fetch matches by team');
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in floorballMatchService.getByTeam:', error);
      throw error;
    }
  },

  /**
   * Get today's matches by team ID
   */
  getTodaysMatchesByTeam: async (teamId: string): Promise<ApiResponse<FloorballMatchDto[]>> => {
    try {
      const url = `${API_URL}/FloorballMatch/by-team/${teamId}/today`;
      console.log('Fetching today\'s matches by team from URL:', url);
      
      const response = await fetch(url);
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to fetch today\'s matches by team');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FloorballMatchDto[]> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to fetch today\'s matches by team');
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in floorballMatchService.getTodaysMatchesByTeam:', error);
      throw error;
    }
  },

  /**
   * Get a floorball match by ID
   */
  getById: async (id: string): Promise<ApiResponse<FloorballMatchDto>> => {
    try {
      const url = `${API_URL}/FloorballMatch/by-id/${id}`;
      console.log('Fetching match from URL:', url);
      
      const response = await fetch(url);
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to fetch floorball match');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FloorballMatchDto> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to fetch floorball match');
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in floorballMatchService.getById:', error);
      throw error;
    }
  },

  /**
   * Create a new floorball match
   */
  create: async (data: CreateFloorballMatchRequest): Promise<ApiResponse<FloorballMatchDto>> => {
    try {
      console.log('Creating match:', data);
      
      // Convert empty refereeId to null for backend compatibility
      const requestData = {
        ...data,
        refereeId: data.refereeId && data.refereeId.trim() !== '' ? data.refereeId : null
      };
      
      const response = await fetch(`${API_URL}/FloorballMatch`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(requestData),
      });
      
      console.log('Create response status:', response.status);
      console.log('Create response ok:', response.ok);
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to create floorball match');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FloorballMatchDto> = await response.json();
      console.log('Create API Response:', apiResponse);
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to create floorball match');
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in floorballMatchService.create:', error);
      throw error;
    }
  },

  /**
   * Update a floorball match
   */
  update: async (data: UpdateFloorballMatchRequest): Promise<ApiResponse<FloorballMatchDto>> => {
    try {
      console.log('Updating match:', data);
      
      const response = await fetch(`${API_URL}/FloorballMatch`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(data),
      });
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to update floorball match');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FloorballMatchDto> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to update floorball match');
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in floorballMatchService.update:', error);
      throw error;
    }
  },

  /**
   * Start a floorball match
   */
  start: async (id: string): Promise<ApiResponse<FloorballMatchDto>> => {
    try {
      console.log('Starting match with ID:', id);
      
      const response = await fetch(`${API_URL}/FloorballMatch/start-match/${id}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
      });
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to start floorball match');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FloorballMatchDto> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to start floorball match');
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in floorballMatchService.start:', error);
      throw error;
    }
  },

  /**
   * Complete a floorball match
   */
  complete: async (id: string): Promise<ApiResponse<FloorballMatchDto>> => {
    try {
      console.log('Completing match with ID:', id);
      
      const response = await fetch(`${API_URL}/FloorballMatch/complete-match/${id}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
      });
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to complete floorball match');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FloorballMatchDto> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to complete floorball match');
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in floorballMatchService.complete:', error);
      throw error;
    }
  }
}; 