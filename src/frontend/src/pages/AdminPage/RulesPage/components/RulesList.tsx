import { useTranslation } from "react-i18next";
import type { RuleItem } from "../../../../types/admin/ruleTypes";
import RuleListItem from "./RuleListItem";
import ExpandMore from "../../../../assets/adminIcons/ExpandMore.svg";
import ExpandLess from "../../../../assets/adminIcons/ExpandLess.png";

interface RulesListProps {
    title: string;
    rules: RuleItem[];
    emptyMessage: string;
    isExpanded: boolean;
    isSaving: boolean;
    showCancel?: boolean;
    isLoading: boolean;
    showActions?: boolean;
    showExpand?: boolean;
    onToggleExpanded?: () => void;
    onEditRule?: (rule: RuleItem) => void;
    onDeleteRule?: (rule: RuleItem) => void;
}

export default function RulesList({
    title,
    rules,
    emptyMessage,
    isExpanded,
    isSaving,
    showCancel,
    isLoading,
    showActions = true,
    showExpand = true,
    onToggleExpanded,
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
                <div className="rules-management-page__preview-box">
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
                {showExpand && (
                    <button
                        type="button"
                        className="rules-management-page__collapse-button"
                        onClick={onToggleExpanded}
                        title={
                            isExpanded ? t("common.close") : t("common.open")
                        }
                    >
                        {isExpanded ? (
                            <img src={ExpandLess} alt="expand less" />
                        ) : (
                            <img src={ExpandMore} alt="expand more" />
                        )}
                    </button>
                )}
            </div>

            {isExpanded && (
                <div className="rules-management-page__scroll-area">
                    {renderContent()}
                </div>
            )}
        </div>
    );
}
