import { useRef, useState } from "react";
import ReactQuill from "react-quill";
import "react-quill/dist/quill.snow.css";
import Button from "../../../../components/Button/Button";
import { useTranslation } from "react-i18next";
import CategorySelect from "./CategorySelect";
import RulePreviewModal from "./RulePreviewModal";
import backArrow from "../../../../assets/adminIcons/backArrow.svg";
import type { RuleItem } from "../../../../types/admin/ruleTypes";
import "./RulesForm.scss";

interface RuleFormProps {
    isEditMode: boolean;
    category: string;
    contentHtml: string;
    isSaving: boolean;
    saveLabel?: string;
    onBack: () => void;
    onCategoryChange: (value: string) => void;
    onContentChange: (value: string) => void;
    onCancel: () => void;
    onSave: () => void;
}

const getPlainTextFromHtml = (html: string): string => {
    const wrapper = document.createElement("div");
    wrapper.innerHTML = html;
    return wrapper.textContent?.replace(/\s+/g, " ").trim() || "";
};

export default function RuleForm({
    isEditMode,
    category,
    contentHtml,
    isSaving,
    saveLabel,
    onBack,
    onCategoryChange,
    onContentChange,
    onCancel,
    onSave,
}: Readonly<RuleFormProps>) {
    const { t } = useTranslation();
    const [isPreviewOpen, setIsPreviewOpen] = useState(false);

    const initialFormRef = useRef({
        contentHtml,
        category,
    });

    const hasChanges =
        contentHtml !== initialFormRef.current.contentHtml ||
        category !== initialFormRef.current.category;

    const hasContent =
        contentHtml.replace(/<p><br><\/p>/g, "").trim().length > 0;

    const previewRule: RuleItem = {
        id: "preview-rule",
        html: contentHtml,
        text: getPlainTextFromHtml(contentHtml),
        category,
    };

    const handleAttemptClose = (action: () => void): void => {
        if (hasChanges) {
            const confirmed = window.confirm(
                t("rules.admin.confirmDiscardChanges"),
            );

            if (!confirmed) {
                return;
            }
        }

        action();
    };

    const saveButtonLabel =
        saveLabel ??
        (isEditMode
            ? t("rules.admin.updateRule", "Päivitä sääntö")
            : t("rules.admin.publishRule", "Julkaise sääntö"));

    return (
        <div className="rules-management-page__create-layer">
            <div className="rules-management-page__create-header">
                <div className="rules-management-page__create-header-left">
                    <button
                        type="button"
                        className="rules-management-page__back-button"
                        onClick={() => handleAttemptClose(onBack)}
                        aria-label={t("common.back", "Takaisin")}
                    >
                        <img src={backArrow} alt="" />
                    </button>

                    <div>
                        <span className="rules-management-page__mode-badge">
                            {isEditMode
                                ? t("rules.admin.editMode", "Muokkaustila")
                                : t("rules.admin.createMode", "Luontitila")}
                        </span>
                        <h3>
                            {isEditMode
                                ? t("rules.admin.editRuleTitle")
                                : t("rules.admin.addRule")}
                        </h3>
                    </div>
                </div>

                <div className="rules-management-page__create-header-actions">
                    <button
                        type="button"
                        className="rules-management-page__preview-button"
                        onClick={() => setIsPreviewOpen(true)}
                        disabled={!hasContent}
                    >
                        {t("common.preview", "Esikatselu")}
                    </button>

                    <Button
                        rounded="pill"
                        onClick={onSave}
                        disabled={isSaving || !hasContent || !hasChanges}
                    >
                        {saveButtonLabel}
                    </Button>
                </div>
            </div>

            <div className="rules-management-page__create-body">
                <div className="rules-management-page__create-card">
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
            </div>

            <div className="rules-management-page__create-footer">
                <button
                    type="button"
                    className="rules-management-page__cancel-button"
                    onClick={() => handleAttemptClose(onCancel)}
                    disabled={isSaving}
                >
                    {t("common.cancel")}
                </button>

                <button
                    type="button"
                    className="rules-management-page__preview-button"
                    onClick={() => setIsPreviewOpen(true)}
                    disabled={!hasContent}
                >
                    {t("common.preview", "Esikatselu")}
                </button>

                <Button
                    rounded="pill"
                    onClick={onSave}
                    disabled={isSaving || !hasContent || !hasChanges}
                >
                    {saveButtonLabel}
                </Button>
            </div>

            <RulePreviewModal
                rule={isPreviewOpen ? previewRule : null}
                onClose={() => setIsPreviewOpen(false)}
            />
        </div>
    );
}
