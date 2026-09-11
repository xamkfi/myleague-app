import { API_URL } from "../constants/config";
import { authFetch } from "../api/utils/authFetch";
import { MAHL_INFO_PAGES } from "../constants/mahlInfoPages";
import type { ApiResponse } from "../types/common/apiResponseType";
import type {
    InfoPageContentResponse,
    InfoPageContentUpdateRequest,
    InfoPageListItem,
} from "../types/admin/infoPageContentTypes";

export class InfoPageContentService {
    private getBaseUrl(): string {
        return `${API_URL}/InfoPageContent`;
    }

    async getAllInfoPages(): Promise<InfoPageContentResponse[]> {
        const response = await authFetch(this.getBaseUrl(), {
            method: "GET",
            headers: { "Content-Type": "application/json" },
            credentials: "include",
        });

        const result: ApiResponse<InfoPageContentResponse[]> =
            await response.json();

        if (!response.ok || !result.success || !result.data) {
            throw new Error(
                result.message ||
                    result.errors?.filter(Boolean).join(", ") ||
                    "Failed to get info pages",
            );
        }

        return result.data;
    }

    async getPageContent(slug: string): Promise<InfoPageContentResponse> {
        const response = await fetch(`${this.getBaseUrl()}/${slug}`, {
            method: "GET",
            headers: { "Content-Type": "application/json" },
            credentials: "include",
        });

        const result: ApiResponse<InfoPageContentResponse> =
            await response.json();

        if (!response.ok || !result.success || !result.data) {
            throw new Error(
                result.message ||
                    result.errors?.filter(Boolean).join(", ") ||
                    `Failed to get info page content for slug '${slug}'`,
            );
        }

        return result.data;
    }

    async updatePageContent(
        slug: string,
        data: InfoPageContentUpdateRequest,
    ): Promise<InfoPageContentResponse> {
        const response = await authFetch(`${this.getBaseUrl()}/${slug}`, {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            credentials: "include",
            body: JSON.stringify(data),
        });

        const result: ApiResponse<InfoPageContentResponse> =
            await response.json();

        if (!response.ok || !result.success || !result.data) {
            throw new Error(
                result.message ||
                    result.errors?.filter(Boolean).join(", ") ||
                    `Failed to update info page content for slug '${slug}'`,
            );
        }

        return result.data;
    }

    buildListItems(
        pagesFromApi: InfoPageContentResponse[],
    ): InfoPageListItem[] {
        const apiBySlug = new Map(
            pagesFromApi.map((page) => [page.pageSlug, page]),
        );

        return MAHL_INFO_PAGES.map((definition) => {
            const apiPage = apiBySlug.get(definition.slug);

            return {
                slug: definition.slug,
                path: definition.path,
                labelKey: definition.labelKey,
                defaultLabel: definition.defaultLabel,
                defaultTitle: definition.defaultTitle,
                title: apiPage?.title ?? definition.defaultTitle,
                contentHtml: apiPage?.contentHtml ?? "",
                lastModifiedBy: apiPage?.lastModifiedBy ?? null,
                updatedAt: apiPage?.updatedAt ?? null,
                existsInDatabase: Boolean(apiPage),
            };
        });
    }
}

export const infoPageContentService = new InfoPageContentService();
