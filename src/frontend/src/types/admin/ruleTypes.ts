export type RulesSectionType =
    | "Global"
    | "SportGroup"
    | "Sport"
    | "Validation"
    | "Companion"
    | "Fee";

export interface RulesSection {
    id: string;
    title: string;
    sortOrder: number;
    sectionType: RulesSectionType;
    parentSectionId: string | null;
    contentHtml: string;
    lastModifiedBy: string | null;
    updatedAt: string;
}

export interface RulesSectionCreateRequest {
    title: string;
    sortOrder: number;
    sectionType: RulesSectionType;
    parentSectionId?: string | null;
}

export interface RulesSectionUpdateRequest {
    title: string;
    sortOrder: number;
    sectionType: RulesSectionType;
    parentSectionId?: string | null;
}

export interface RuleItem {
    id: string;
    html: string;
    text: string;
    sectionId: string;
    order: number;
}

export interface AddRulesSectionRuleRequest {
    ruleHtml: string;
}

export interface UpdateRulesSectionRuleRequest {
    ruleHtml: string;
}
