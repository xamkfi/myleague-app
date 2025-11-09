import type { ApiResponse } from "../../types/floorball/floorballTypes";
import type { PaginatedApiResponse } from "../../types/floorball/floorballTypes";

/**
 * Helper function to parse error responses properly
 */
export const parseErrorResponse = async (
   response: ApiResponse<any> | PaginatedApiResponse<any>, 
   defaultMessage: string): Promise<string> => {
      
   try {
      const responseText = JSON.stringify(response);
      console.error('API Error Response (raw):', responseText);
      console.error('API Error Response (parsed):', response);

      return responseText

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

   return `${defaultMessage}`;
};