import ReactQuill from "react-quill";
import "react-quill/dist/quill.snow.css";
import Button from "../../../../components/Button/Button";
import { useTranslation } from "react-i18next";
import CategorySelect from "./CategorySelect";
import backArrow from "../../../../assets/adminIcons/backArrow.svg";
import "../RulesManagementPage.scss";

interface RuleFormProps {
    isEditMode: boolean;
    category: string;
    contentHtml: string;
    isSaving: boolean;
    onBack: () => void;
    onCategoryChange: (value: string) => void;
    onContentChange: (value: string) => void;
    onCancel: () => void;
    onSave: () => void;
}

export default function RuleForm({
    isEditMode,
    category,
    contentHtml,
    isSaving,
    onBack,
    onCategoryChange,
    onContentChange,
    onCancel,
    onSave,
}: Readonly<RuleFormProps>) {
    const { t } = useTranslation();

    const hasContent =
        contentHtml.replace(/<p><br><\/p>/g, "").trim().length > 0;

    return (
        <div className="rules-management-page__create-layer">
            <div className="rules-management-page__create-header">
                <div className="rules-management-page__create-header-left">
                    <button
                        type="button"
                        className="rules-management-page__back-button"
                        onClick={onBack}
                    >
                        <div>
                            <img src={backArrow} alt="back arrow" />
                        </div>
                    </button>

                    <h3>
                        {isEditMode
                            ? t("rules.admin.editRuleTitle")
                            : t("rules.admin.addRule")}
                    </h3>
                </div>
            </div>

            <div className="rules-management-page__create-body">
                <div className="rules-management-page__create-field">
                    <label className="rules-management-page__create-label">
                        {t("rules.admin.category")}
                    </label>

                    <CategorySelect
                        value={category}
                        onChange={onCategoryChange}
                    />
                </div>

                <div className="rules-management-page__create-field">
                    <label className="rules-management-page__create-label">
                        {t("rules.admin.rule")}
                    </label>
                    <div className="rules-management-page__quill-wrapper">
                        <ReactQuill
                            theme="snow"
                            value={contentHtml}
                            onChange={onContentChange}
                            placeholder={t("rules.admin.typeRulePlaceholder")}
                        />
                    </div>
                </div>
            </div>

            <div className="rules-management-page__create-footer">
                <button
                    type="button"
                    className="rules-management-page__cancel-button"
                    onClick={onCancel}
                    disabled={isSaving}
                >
                    {t("common.cancel")}
                </button>

                <Button
                    rounded="pill"
                    onClick={onSave}
                    disabled={isSaving || !hasContent}
                >
                    {t("common.save")}
                </Button>
            </div>
        </div>
    );
}
