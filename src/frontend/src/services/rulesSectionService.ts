import { API_URL } from "../constants/config";
import { authFetch } from "../api/utils/authFetch";
import type { ApiResponse } from "../types/common/apiResponseType";
import type {
    AddRulesSectionRuleRequest,
    RulesSection,
    RulesSectionCreateRequest,
    RulesSectionUpdateRequest,
    UpdateRulesSectionRuleRequest,
} from "../types/admin/ruleTypes";

export class RulesSectionService {
    private getBaseUrl(): string {
        if (import.meta.env.DEV) {
            return "http://localhost:8080/api/RulesSection";
        }

        return `${API_URL}/RulesSection`;
    }

    async getAllSections(): Promise<RulesSection[]> {
        const response = await fetch(`${this.getBaseUrl()}/`, {
            method: "GET",
            headers: { "Content-Type": "application/json" },
            credentials: "include",
        });

        const result: ApiResponse<RulesSection[]> = await response.json();

        if (!response.ok || !result.success || !result.data) {
            throw new Error(
                result.message ||
                    result.errors?.filter(Boolean).join(", ") ||
                    "Failed to load rules sections",
            );
        }

        return result.data;
    }

    async getSectionById(id: string): Promise<RulesSection> {
        const response = await fetch(`${this.getBaseUrl()}/${id}`, {
            method: "GET",
            headers: { "Content-Type": "application/json" },
            credentials: "include",
        });

        const result: ApiResponse<RulesSection> = await response.json();

        if (!response.ok || !result.success || !result.data) {
            throw new Error(
                result.message ||
                    result.errors?.filter(Boolean).join(", ") ||
                    "Failed to load rules section",
            );
        }

        return result.data;
    }

    async createSection(
        data: RulesSectionCreateRequest,
    ): Promise<RulesSection> {
        const response = await authFetch(`${this.getBaseUrl()}/`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            credentials: "include",
            body: JSON.stringify(data),
        });

        const result: ApiResponse<RulesSection> = await response.json();

        if (!response.ok || !result.success || !result.data) {
            throw new Error(
                result.message ||
                    result.errors?.filter(Boolean).join(", ") ||
                    "Failed to create rules section",
            );
        }

        return result.data;
    }

    async updateSection(
        id: string,
        data: RulesSectionUpdateRequest,
    ): Promise<RulesSection> {
        const response = await authFetch(`${this.getBaseUrl()}/${id}`, {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            credentials: "include",
            body: JSON.stringify(data),
        });

        const result: ApiResponse<RulesSection> = await response.json();

        if (!response.ok || !result.success || !result.data) {
            throw new Error(
                result.message ||
                    result.errors?.filter(Boolean).join(", ") ||
                    "Failed to update rules section",
            );
        }

        return result.data;
    }

    async deleteSection(id: string): Promise<void> {
        const response = await authFetch(`${this.getBaseUrl()}/${id}`, {
            method: "DELETE",
            headers: { "Content-Type": "application/json" },
            credentials: "include",
        });

        const result: ApiResponse<boolean> = await response.json();

        if (!response.ok || !result.success) {
            throw new Error(
                result.message ||
                    result.errors?.filter(Boolean).join(", ") ||
                    "Failed to delete rules section",
            );
        }
    }

    async addRule(
        sectionId: string,
        data: AddRulesSectionRuleRequest,
    ): Promise<RulesSection> {
        const response = await authFetch(
            `${this.getBaseUrl()}/${sectionId}/rules`,
            {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                credentials: "include",
                body: JSON.stringify(data),
            },
        );

        const result: ApiResponse<RulesSection> = await response.json();

        if (!response.ok || !result.success || !result.data) {
            throw new Error(
                result.message ||
                    result.errors?.filter(Boolean).join(", ") ||
                    "Failed to add rule",
            );
        }

        return result.data;
    }

    async updateRule(
        sectionId: string,
        ruleId: string,
        data: UpdateRulesSectionRuleRequest,
    ): Promise<RulesSection> {
        const response = await authFetch(
            `${this.getBaseUrl()}/${sectionId}/rules/${ruleId}`,
            {
                method: "PUT",
                headers: { "Content-Type": "application/json" },
                credentials: "include",
                body: JSON.stringify(data),
            },
        );

        const result: ApiResponse<RulesSection> = await response.json();

        if (!response.ok || !result.success || !result.data) {
            throw new Error(
                result.message ||
                    result.errors?.filter(Boolean).join(", ") ||
                    "Failed to update rule",
            );
        }

        return result.data;
    }

    async deleteRule(
        sectionId: string,
        ruleId: string,
    ): Promise<RulesSection> {
        const response = await authFetch(
            `${this.getBaseUrl()}/${sectionId}/rules/${ruleId}`,
            {
                method: "DELETE",
                headers: { "Content-Type": "application/json" },
                credentials: "include",
            },
        );

        const result: ApiResponse<RulesSection> = await response.json();

        if (!response.ok || !result.success || !result.data) {
            throw new Error(
                result.message ||
                    result.errors?.filter(Boolean).join(", ") ||
                    "Failed to delete rule",
            );
        }

        return result.data;
    }
}

export const rulesSectionService = new RulesSectionService();
