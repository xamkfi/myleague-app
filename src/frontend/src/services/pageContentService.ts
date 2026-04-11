import { API_URL } from "../constants/config";
import type {
    ApiResponse,
    PageContentResponse,
    PageContentUpdateRequest,
    PageRuleUpdateRequest,
} from "../types/admin/ruleTypes";

export class PageContentService {
    private getBaseUrl(): string {
        if (import.meta.env.DEV) {
            return "http://localhost:8080/api/PageContent";
        }

        return `${API_URL}/PageContent`;
    }

    async getPageContent(slug: string): Promise<PageContentResponse> {
        const url = `${this.getBaseUrl()}/${slug}`;

        try {
            console.log(`Getting page content from: ${url}`);

            const response = await fetch(url, {
                method: "GET",
                headers: {
                    "Content-Type": "application/json",
                },
                credentials: "include",
            });

            const result: ApiResponse<PageContentResponse> =
                await response.json();

            if (!response.ok || !result.success || !result.data) {
                const errorMessage =
                    result.message ||
                    result.errors?.filter(Boolean).join(", ") ||
                    `Failed to get page content for slug '${slug}'`;

                throw new Error(errorMessage);
            }

            return result.data;
        } catch (error) {
            console.error(
                `Error getting page content for slug '${slug}':`,
                error,
            );
            throw error;
        }
    }

    async updatePageContent(
        slug: string,
        data: PageContentUpdateRequest,
    ): Promise<PageContentResponse> {
        const url = `${this.getBaseUrl()}/${slug}`;

        try {
            console.log(`Updating page content at: ${url}`);

            const response = await fetch(url, {
                method: "PUT",
                headers: {
                    "Content-Type": "application/json",
                },
                credentials: "include",
                body: JSON.stringify(data),
            });

            const result: ApiResponse<PageContentResponse> =
                await response.json();

            if (!response.ok || !result.success || !result.data) {
                const errorMessage =
                    result.message ||
                    result.errors?.filter(Boolean).join(", ") ||
                    `Failed to update page content for slug '${slug}'`;

                throw new Error(errorMessage);
            }

            return result.data;
        } catch (error) {
            console.error(
                `Error updating page content for slug '${slug}':`,
                error,
            );
            throw error;
        }
    }

    async updatePageRule(
        slug: string,
        ruleId: string,
        data: PageRuleUpdateRequest,
    ): Promise<PageContentResponse> {
        const url = `${this.getBaseUrl()}/${slug}/rules/${ruleId}`;

        const response = await fetch(url, {
            method: "PUT",
            headers: {
                "Content-Type": "application/json",
            },
            credentials: "include",
            body: JSON.stringify(data),
        });

        const result: ApiResponse<PageContentResponse> = await response.json();

        if (!response.ok || !result.success || !result.data) {
            const errorMessage =
                result.message ||
                result.errors?.filter(Boolean).join(", ") ||
                "Failed to update rule";

            throw new Error(errorMessage);
        }

        return result.data;
    }

    async deletePageRule(
        slug: string,
        ruleId: string,
    ): Promise<PageContentResponse> {
        const url = `${this.getBaseUrl()}/${slug}/rules/${ruleId}`;

        const response = await fetch(url, {
            method: "DELETE",
            headers: {
                "Content-Type": "application/json",
            },
            credentials: "include",
        });

        const result: ApiResponse<PageContentResponse> = await response.json();

        if (!response.ok || !result.success || !result.data) {
            const errorMessage =
                result.message ||
                result.errors?.filter(Boolean).join(", ") ||
                "Failed to delete rule";

            throw new Error(errorMessage);
        }

        return result.data;
    }
}

export const pageContentService = new PageContentService();
