import DOMPurify from "dompurify";
import { useTranslation } from "react-i18next";
import EditIcon from "../../../../assets/adminIcons/EditIcon.svg";
import DeleteIcon from "../../../../assets/adminIcons/DeleteIcon.svg";
import CancelIcon from "../../../../assets/adminIcons/CancelIcon.svg";
import type { RuleItem } from "../../../../types/admin/ruleTypes";
import "./RulesListItem.scss";

interface RuleListItemProps {
    rule: RuleItem;
    isSaving: boolean;
    showCancel?: boolean;
    showActions?: boolean;
    onEdit: (rule: RuleItem) => void;
    onDelete: (rule: RuleItem) => void;
}

export default function RulesListItem({
    rule,
    isSaving,
    showCancel,
    showActions = true,
    onEdit,
    onDelete,
}: Readonly<RuleListItemProps>) {
    const { t } = useTranslation();

    return (
        <div className="rules-management-page__rule-row">
            <div className="rules-management-page__rule-display-row">
                <div className="rules-management-page__rule-display-row-left">
                    <div
                        className="rules-management-page__preview-box--empty rules-management-page__preview-box--inline"
                        dangerouslySetInnerHTML={{
                            __html: DOMPurify.sanitize(rule.html),
                        }}
                    />
                </div>

                <div className="rules-management-page__rule-category-action-buttons">
                    <span className="rules-management-page__rule-category">
                        <strong>{t("rules.admin.category")}: </strong>
                        {rule.category}
                    </span>

                    {showActions && (
                        <div className="rules-management-page__rule-actions">
                            <button
                                type="button"
                                className="rules-management-page__icon-button"
                                onClick={() => onEdit(rule)}
                                disabled={isSaving}
                                title={t("rules.admin.editRule")}
                            >
                                <img
                                    src={EditIcon}
                                    alt={t("rules.admin.editRule")}
                                    width={20}
                                />
                            </button>

                            <button
                                type="button"
                                className={`rules-management-page__icon-button ${
                                    showCancel
                                        ? "rules-management-page__icon-button--secondary"
                                        : "rules-management-page__icon-button--danger"
                                }`}
                                onClick={() => onDelete(rule)}
                                disabled={isSaving}
                                title={
                                    showCancel
                                        ? t("common.cancel")
                                        : t("common.delete")
                                }
                            >
                                <img
                                    src={showCancel ? CancelIcon : DeleteIcon}
                                    alt={
                                        showCancel
                                            ? t("common.cancel")
                                            : t("common.delete")
                                    }
                                    width={20}
                                />
                            </button>
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
}
