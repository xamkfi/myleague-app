import { useTranslation } from "react-i18next";
import type { RuleItem, RulesSection } from "../../../../types/admin/ruleTypes";
import RuleListItem from "./RulesListItem";
import "../../../../styles/AdminTable.scss";
import "./RulesList.scss";

interface RulesListProps {
    title: string;
    rules: RuleItem[];
    sections: RulesSection[];
    emptyMessage: string;
    isSaving: boolean;
    isLoading: boolean;
    wordLimit?: number;
    showActions?: boolean;
    onEditRule?: (rule: RuleItem) => void;
    onDeleteRule?: (rule: RuleItem) => void;
}

export default function RulesList({
    title,
    rules,
    sections,
    emptyMessage,
    isSaving,
    isLoading,
    wordLimit = 10,
    showActions = true,
    onEditRule,
    onDeleteRule,
}: Readonly<RulesListProps>) {
    const { t } = useTranslation();

    if (isLoading) {
        return (
            <div className="admin-table__empty">
                {t("rules.loading", "Ladataan sääntöjä...")}
            </div>
        );
    }

    return (
        <div className="rules-list">
            <div className="rules-list__header">
                <h3>{title}</h3>
            </div>

            <div className="admin-table__wrapper">
                <table className="admin-table">
                    <thead>
                        <tr>
                            <th>{t("rules.admin.table.rule", "Sääntö")}</th>
                            <th>{t("rules.admin.table.section", "Osio")}</th>
                            <th>{t("rules.admin.table.order", "Järjestys")}</th>
                            <th className="admin-table__actions-col">
                                {t("rules.admin.table.actions", "Toiminnot")}
                            </th>
                        </tr>
                    </thead>

                    <tbody>
                        {rules.map((rule) => (
                            <RuleListItem
                                key={`${rule.sectionId}-${rule.id}`}
                                rule={rule}
                                sections={sections}
                                isSaving={isSaving}
                                showActions={showActions}
                                wordLimit={wordLimit}
                                onEdit={onEditRule ?? (() => {})}
                                onDelete={onDeleteRule ?? (() => {})}
                            />
                        ))}
                    </tbody>
                </table>
            </div>

            {rules.length === 0 && (
                <div className="admin-table__empty">{emptyMessage}</div>
            )}
        </div>
    );
}
