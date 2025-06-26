import { VITE_API_URL } from "../../constants/config";
import type { DivisionType } from "../../types/common/divisionType";
import type { ApiResponse } from "../../types/common/apiResponseType";

export const divisionService = {
   getAll: async (): Promise<ApiResponse<DivisionType[]>> => {
      const response = await fetch(`${VITE_API_URL}/Divisions`);
      const data: ApiResponse<DivisionType[]> = await response.json();
      return data;
   },

   getById: async (id: string): Promise<ApiResponse<DivisionType>> => {
      const response = await fetch(`${VITE_API_URL}/Divisions/${id}`)
      const data: ApiResponse<DivisionType> = await response.json()
      return data;
   }
}