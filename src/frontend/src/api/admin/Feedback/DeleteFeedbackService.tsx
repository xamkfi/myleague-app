import { API_URL } from "../../../constants/config"
import type { ApiResponse } from "../../../types/common/apiResponseType";
import { authFetch } from "../../utils/authFetch"

export const DeleteFeedbackService = {
    Delete: async(
        id: string
    ): Promise<void> => {
        const response = await authFetch(`${API_URL}/Feedback/${id}`, {
            method: 'DELETE'
        });
        if(!response.ok){
            throw new Error("Failed to delete feedback")
        }
        
        const apiResponse: ApiResponse<void> = await response.json();
        if(!apiResponse.success){
            throw new Error(apiResponse.errors?.join(',') || "Failed to delete feedbak");
        }
    },
};