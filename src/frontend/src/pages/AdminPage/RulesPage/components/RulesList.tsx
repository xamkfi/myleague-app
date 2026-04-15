import { useTranslation } from "react-i18next";
import type { RuleItem } from "../../../../types/admin/ruleTypes";
import RuleListItem from "./RulesListItem";
import "./RulesList.scss";

interface RulesListProps {
    title: string;
    rules: RuleItem[];
    emptyMessage: string;
    isSaving: boolean;
    showCancel?: boolean;
    isLoading: boolean;
    showActions?: boolean;
    onEditRule?: (rule: RuleItem) => void;
    onDeleteRule?: (rule: RuleItem) => void;
}

export default function RulesList({
    title,
    rules,
    emptyMessage,
    isSaving,
    showCancel,
    isLoading,
    showActions = true,
    onEditRule,
    onDeleteRule,
}: Readonly<RulesListProps>) {
    const { t } = useTranslation();

    const renderContent = () => {
        if (isLoading) {
            return <p>{t("rules.loading")}</p>;
        }

        if (rules.length === 0) {
            return (
                <div className="rules-management-page__preview-box--empty">
                    {emptyMessage}
                </div>
            );
        }

        return rules.map((rule) => (
            <RuleListItem
                key={rule.id}
                rule={rule}
                isSaving={isSaving}
                showCancel={showCancel}
                showActions={showActions}
                onEdit={onEditRule ?? (() => {})}
                onDelete={onDeleteRule ?? (() => {})}
            />
        ));
    };

    return (
        <div className="rules-management-page__saved-card">
            <div className="rules-management-page__section-header">
                <label>{title}</label>
            </div>

            <div className="rules-management-page__scroll-area">
                {renderContent()}
            </div>
        </div>
    );
}
