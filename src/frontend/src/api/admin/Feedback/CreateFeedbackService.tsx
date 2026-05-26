import { API_URL } from "../../../constants/config";
import type { ApiResponse } from "../../../types/common/apiResponseType";
import type { FeedbackEntity } from "../../../types/feedback/feedbackTypes";
import { authFetch } from "../../utils/authFetch";


export const createFeedbackService = {
    create: async(
        data: Omit<FeedbackEntity, 'id'>
    ): Promise<FeedbackEntity> => {
        const response = await authFetch(`${API_URL}/Feedback`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(data),
        });
        if(!response.ok){
            throw new Error("Failed to create feedback");
        }

        const apiResponse: ApiResponse<FeedbackEntity> = await response.json();
        if(!apiResponse.success){
            throw new Error(apiResponse.errors?.join(',') || "Failed to create feedback");
        }
        return apiResponse.data;
    },
};