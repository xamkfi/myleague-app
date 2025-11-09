import type { ApiResponse } from "../../types/floorball/floorballTypes";
import type { PaginatedApiResponse } from "../../types/floorball/floorballTypes";

/**
 * Helper function to parse error responses properly
 */
export async function parseErrorResponse<T>(
   response: ApiResponse<T> | PaginatedApiResponse<T>, 
   defaultMessage: string
): Promise<string> {
   try {
      const responseText = JSON.stringify(response);
      console.error('API Error Response (raw):', responseText);
      console.error('API Error Response (parsed):', response);

      return responseText

   } catch (readError) {
      console.error('Error reading response:', readError);
   }

   return `${defaultMessage}`;
} 