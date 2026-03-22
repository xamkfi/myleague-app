import { API_URL } from '../../constants/config';
import { authFetch } from '../utils/authFetch';

export interface PageContentResponse {
    id: string;
    pageSlug: string;
    title: string;
    contentHtml: string;
    lastModifiedBy: string | null;
    updatedAt: string;
}
export interface PageContentUpdateRequest {
    title: string;
    contentHtml: string;
}

interface ApiResponse<T> {
    success: boolean;
    data: T;
    message: string;
    errors: string[];
}

// GET /api/page-content/:slug
export async function getPageContent(slug: string): Promise<PageContentResponse> {
    try {
        const response = await authFetch(`${API_URL}/page-content/${slug}`, {
            method: "GET"
        });

        if (!response.ok) {
            const errorText = await response.text();
            console.log("Fetch page content error:", errorText);
            throw new Error("Failed to fetch page content.");
        }

        const data: ApiResponse<PageContentResponse> = await response.json();
        return data.data;

    } catch (error) {
        console.error("Fetch page content error:", error);
        throw error;
    }
}

// PUT /api/page-content/:slug (admin)
export async function updatePageContent(slug: string, data: PageContentUpdateRequest): Promise<PageContentResponse> {
    try {
        const response = await authFetch(`${API_URL}/page-content/${slug}`, {
            method: "PUT",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(data)
        });

        if (!response.ok) {
            const errorText = await response.text();
            console.log("Update page content error:", errorText);
            throw new Error("Failed to update page content.");
        }

        const dataResponse: ApiResponse<PageContentResponse> = await response.json();
        return dataResponse.data;

    } catch (error) {
        console.error("Update page content error:", error);
        throw error;
    }
}
