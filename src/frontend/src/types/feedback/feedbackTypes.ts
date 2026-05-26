export interface GetFeedbackRequest {
    page?: number;
    pageSize?: number;
}

export interface PaginatedApiResponse<T> {
    success: boolean;
    data: T[];
    pagination: {
        currentPage: number;
        pageSize: number;
        totalCount: number;
        totalPages: number;
        hasNextPage: boolean;
        hasPreviousPage: boolean;
        startItem: number;
        endItem: number;
    }
    message: string;
    errors: string[];
}

export interface FeedbackEntity {
    id: string;
    feedbackBody: string;
    email?: string | null;
    createdAt: string;
}