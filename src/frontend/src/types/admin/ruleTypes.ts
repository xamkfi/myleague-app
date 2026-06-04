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

export interface ApiResponse<T> {
    data: T | null;
    success: boolean;
    message: string;
    errors: string[];
}

export interface RuleItem {
    id: string;
    html: string;
    text: string;
    category: string;
}

export interface PageRuleUpdateRequest {
    ruleHtml: string;
}
