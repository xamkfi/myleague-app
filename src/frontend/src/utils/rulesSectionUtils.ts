import type { RuleItem, RulesSection, RulesSectionType } from "../types/admin/ruleTypes";

/** The four public top-level tabs: Yleissäännöt, Vahvistukset, Maksut, Lajikohtaiset säännöt */
export const MAIN_TAB_SECTION_TYPES: RulesSectionType[] = [
    "Global",
    "Validation",
    "Fee",
    "SportGroup",
];

export function isMainTabSection(section: RulesSection): boolean {
    return (
        !section.parentSectionId &&
        MAIN_TAB_SECTION_TYPES.includes(section.sectionType)
    );
}

export function getTopLevelSections(sections: RulesSection[]): RulesSection[] {
    return sections.filter(isMainTabSection).sort((a, b) => a.sortOrder - b.sortOrder);
}

export function getChildSections(
    sections: RulesSection[],
    parentId: string,
): RulesSection[] {
    return sections
        .filter((section) => section.parentSectionId === parentId)
        .sort((a, b) => a.sortOrder - b.sortOrder);
}

export function getRuleableSections(sections: RulesSection[]): RulesSection[] {
    return sections
        .filter((section) => section.sectionType !== "SportGroup")
        .sort((a, b) => {
            if (a.parentSectionId && !b.parentSectionId) {
                return 1;
            }

            if (!a.parentSectionId && b.parentSectionId) {
                return -1;
            }

            return a.sortOrder - b.sortOrder;
        });
}

export function getSectionLabel(
    section: RulesSection,
    sections: RulesSection[],
): string {
    if (!section.parentSectionId) {
        return section.title;
    }

    const parent = sections.find(
        (candidate) => candidate.id === section.parentSectionId,
    );

    return parent ? `${parent.title} / ${section.title}` : section.title;
}

export function sortRulesByOrder(rules: RuleItem[]): RuleItem[] {
    return [...rules].sort((a, b) => a.order - b.order);
}

export function getNextRuleOrder(rules: RuleItem[]): number {
    if (rules.length === 0) {
        return 1;
    }

    return Math.max(...rules.map((rule) => rule.order)) + 1;
}

export interface RuleOrderUpdate {
    ruleId: string;
    newOrder: number;
}

export function resolveRuleOrderConflict(
    rules: RuleItem[],
    targetRuleId: string | null,
    newOrder: number,
    previousOrder: number | null,
): RuleOrderUpdate[] {
    const normalizedOrder = Math.max(1, newOrder);
    const conflicting = rules.find(
        (rule) => rule.id !== targetRuleId && rule.order === normalizedOrder,
    );

    if (!conflicting) {
        return [];
    }

    if (previousOrder != null) {
        return [{ ruleId: conflicting.id, newOrder: previousOrder }];
    }

    const maxOrder = Math.max(...rules.map((rule) => rule.order), 0);

    return [{ ruleId: conflicting.id, newOrder: maxOrder + 1 }];
}

export function formatRuleNumber(order: number): string {
    return `${String(order).padStart(2, "0")}.`;
}

export function findSportGroupSection(
    sections: RulesSection[],
): RulesSection | undefined {
    return sections.find((section) => section.sectionType === "SportGroup");
}

export function findGlobalSection(
    sections: RulesSection[],
): RulesSection | undefined {
    return sections.find((section) => section.sectionType === "Global");
}
