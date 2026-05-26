import { API_URL } from "../../../constants/config";
import type { ApiResponse } from "../../../types/common/apiResponseType";
import type {
    GetFeedbackRequest,
    PaginatedApiResponse,
    FeedbackEntity,
} from "../../../types/feedback/feedbackTypes";
import { authFetch } from "../../utils/authFetch";

export const getFeedbackService = {
    getAll: async (
        params?: GetFeedbackRequest,
    ): Promise<PaginatedApiResponse<FeedbackEntity>> => {
        const searchParams = new URLSearchParams();

        if (params?.page) searchParams.append("page", params.page.toString());
        if (params?.pageSize)
            searchParams.append("pageSize", params.pageSize.toString());

        const url = `${API_URL}/Feedback${searchParams.toString() ? `?${searchParams.toString()}` : ""}`;
        const response = await authFetch(url);
        if (!response.ok) {
            throw new Error("Failed to fetch feedback");
        }

        const apiResponse: PaginatedApiResponse<FeedbackEntity> =
            await response.json();

        if (!apiResponse.success) {
            throw new Error(apiResponse.errors?.join(",") || "Failed to retrieve feedback");
        }

        return apiResponse;
    },

    getById: async (
        id: string
    ) : Promise<FeedbackEntity> => {
        const response = await authFetch(`${API_URL}/Feedback/${id}`);
        if(!response.ok) {
            throw new Error("Failed to fetch feedback");
        }

        const apiResponse: ApiResponse<FeedbackEntity> = await response.json();
        if(!apiResponse.success) {
            throw new Error(apiResponse.errors?.join(',') || 'Failed to fetch feedback');
        }

        return apiResponse.data;
    },
};
