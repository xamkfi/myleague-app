export interface InfoPageContentResponse {
    id: string;
    pageSlug: string;
    title: string;
    contentHtml: string;
    lastModifiedBy: string | null;
    updatedAt: string;
}

export interface InfoPageContentUpdateRequest {
    title: string;
    contentHtml: string;
}

export interface InfoPageListItem {
    slug: string;
    path: string;
    labelKey: string;
    defaultLabel: string;
    defaultTitle: string;
    title: string;
    contentHtml: string;
    lastModifiedBy: string | null;
    updatedAt: string | null;
    existsInDatabase: boolean;
}
