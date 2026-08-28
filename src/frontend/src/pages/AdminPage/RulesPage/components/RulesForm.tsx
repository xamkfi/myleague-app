import { useRef, useState } from "react";
import Button from "../../../../components/Button/Button";
import RichTextEditor from "../../../../components/RichTextEditor/RichTextEditor";
import { useTranslation } from "react-i18next";
import SectionSelect from "./SectionSelect";
import RulePreviewModal from "./RulePreviewModal";
import backArrow from "../../../../assets/adminIcons/backArrow.svg";
import type { RuleItem, RulesSection } from "../../../../types/admin/ruleTypes";
import "./RulesForm.scss";

interface RuleFormProps {
    isEditMode: boolean;
    sectionId: string;
    sections: RulesSection[];
    contentHtml: string;
    order: number;
    isSaving: boolean;
    saveLabel?: string;
    onBack: () => void;
    onSectionChange: (value: string) => void;
    onOrderChange: (value: number) => void;
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
    sectionId,
    sections,
    contentHtml,
    order,
    isSaving,
    saveLabel,
    onBack,
    onSectionChange,
    onOrderChange,
    onContentChange,
    onCancel,
    onSave,
}: Readonly<RuleFormProps>) {
    const { t } = useTranslation();
    const [isPreviewOpen, setIsPreviewOpen] = useState(false);

    const initialFormRef = useRef({
        contentHtml,
        sectionId,
        order,
    });

    const hasChanges =
        contentHtml !== initialFormRef.current.contentHtml ||
        sectionId !== initialFormRef.current.sectionId ||
        order !== initialFormRef.current.order;

    const hasContent =
        contentHtml.replace(/<p><br><\/p>/g, "").trim().length > 0;

    const previewRule: RuleItem = {
        id: "preview-rule",
        html: contentHtml,
        text: getPlainTextFromHtml(contentHtml),
        sectionId,
        order,
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
                    <div className="rules-management-page__create-row">
                        <div className="rules-management-page__create-field">
                            <label className="rules-management-page__create-label">
                                {t("rules.admin.section", "Sääntöosio")}
                            </label>

                            <SectionSelect
                                sections={sections}
                                value={sectionId}
                                onChange={onSectionChange}
                            />
                        </div>

                        <div className="rules-management-page__create-field rules-management-page__create-field--order">
                            <label
                                className="rules-management-page__create-label"
                                htmlFor="rule-order-input"
                            >
                                {t("rules.admin.table.order", "Järjestys")}
                            </label>

                            <input
                                id="rule-order-input"
                                type="number"
                                min={1}
                                className="rules-management-page__order-input"
                                value={order}
                                onChange={(event) =>
                                    onOrderChange(
                                        Number.parseInt(
                                            event.target.value,
                                            10,
                                        ) || 1,
                                    )
                                }
                            />
                        </div>
                    </div>

                    <div className="rules-management-page__create-field">
                        <label className="rules-management-page__create-label">
                            {t("rules.admin.rule")}
                        </label>
                        <div className="rules-management-page__quill-wrapper">
                            <RichTextEditor
                                value={contentHtml}
                                onChange={onContentChange}
                                showMatchInsert={false}
                                placeholder={t(
                                    "rules.admin.typeRulePlaceholder",
                                )}
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
