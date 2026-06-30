import { useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import ActionsDropdown from "../../../../components/ActionsDropdown/ActionsDropdown";
import type { RuleItem, RulesSection } from "../../../../types/admin/ruleTypes";
import { getSectionLabel } from "../../../../utils/rulesSectionUtils";
import "./RulesListItem.scss";

interface RuleListItemProps {
    rule: RuleItem;
    sections: RulesSection[];
    isSaving: boolean;
    showActions?: boolean;
    wordLimit?: number;
    onEdit: (rule: RuleItem) => void;
    onDelete: (rule: RuleItem) => void;
}

const normalizeRuleText = (text: string | undefined): string => {
    return text?.replace(/\s+/g, " ").trim() || "-";
};

const limitWords = (text: string, wordLimit: number): string => {
    const words = text.split(" ");

    if (words.length <= wordLimit) {
        return text;
    }

    return `${words.slice(0, wordLimit).join(" ")}…`;
};

export default function RulesListItem({
    rule,
    sections,
    isSaving,
    showActions = true,
    wordLimit = 10,
    onEdit,
    onDelete,
}: Readonly<RuleListItemProps>) {
    const { t } = useTranslation();
    const [isExpanded, setIsExpanded] = useState(false);

    const fullText = useMemo(() => normalizeRuleText(rule.text), [rule.text]);
    const words = fullText === "-" ? [] : fullText.split(" ");
    const canExpand = words.length > wordLimit;
    const visibleText = isExpanded ? fullText : limitWords(fullText, wordLimit);

    const sectionLabel = useMemo(() => {
        const section = sections.find(
            (candidate) => candidate.id === rule.sectionId,
        );

        return section
            ? getSectionLabel(section, sections)
            : t("rules.admin.unknownSection", "Tuntematon osio");
    }, [rule.sectionId, sections, t]);

    return (
        <tr>
            <td>
                <div className="admin-table__name rules-list-item__title">
                    {visibleText}
                </div>

                {canExpand && (
                    <button
                        type="button"
                        className="rules-list-item__expand-button"
                        onClick={() => setIsExpanded((prev) => !prev)}
                    >
                        {isExpanded
                            ? t("rules.admin.showLess", "Näytä vähemmän")
                            : t("rules.admin.showMore", "Näytä lisää")}
                    </button>
                )}
            </td>

            <td className="rules-list-item__meta-col">
                <span className="admin-tag admin-tag--blue">{sectionLabel}</span>
            </td>

            <td className="rules-list-item__meta-col">
                <span className="admin-tag admin-tag--blue">
                    {String(rule.order).padStart(2, "0")}
                </span>
            </td>

            <td className="admin-table__actions-col">
                {showActions && (
                    <ActionsDropdown
                        ariaLabel={t(
                            "rules.admin.actionsMenu",
                            "Säännön toiminnot",
                        )}
                        actions={[
                            {
                                label: t("rules.admin.editRule", "Muokkaa"),
                                onClick: () => onEdit(rule),
                                disabled: isSaving,
                            },
                            {
                                label: t("common.delete", "Poista"),
                                onClick: () => onDelete(rule),
                                variant: "danger",
                                disabled: isSaving,
                            },
                        ]}
                    />
                )}
            </td>
        </tr>
    );
}
