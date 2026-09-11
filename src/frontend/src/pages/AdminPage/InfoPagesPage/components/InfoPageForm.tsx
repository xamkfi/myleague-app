import { useRef } from "react";
import { Link } from "react-router-dom";
import Button from "../../../../components/Button/Button";
import RichTextEditor from "../../../../components/RichTextEditor/RichTextEditor";
import { useTranslation } from "react-i18next";
import backArrow from "../../../../assets/adminIcons/backArrow.svg";
import type { InfoPageListItem } from "../../../../types/admin/infoPageContentTypes";
import "./InfoPageForm.scss";

interface InfoPageFormProps {
    page: InfoPageListItem;
    title: string;
    contentHtml: string;
    isSaving: boolean;
    onBack: () => void;
    onTitleChange: (value: string) => void;
    onContentChange: (value: string) => void;
    onSave: () => void;
}

export default function InfoPageForm({
    page,
    title,
    contentHtml,
    isSaving,
    onBack,
    onTitleChange,
    onContentChange,
    onSave,
}: Readonly<InfoPageFormProps>) {
    const { t } = useTranslation();
    const initialFormRef = useRef({ title, contentHtml });

    const hasChanges =
        title !== initialFormRef.current.title ||
        contentHtml !== initialFormRef.current.contentHtml;

    const hasContent =
        title.trim().length > 0 &&
        contentHtml.replace(/<p><br><\/p>/g, "").trim().length > 0;

    const handleAttemptClose = (action: () => void): void => {
        if (hasChanges) {
            const confirmed = window.confirm(
                t(
                    "admin.siteContent.infoPages.confirmDiscardChanges",
                    "Haluatko hylätä tallentamattomat muutokset?",
                ),
            );

            if (!confirmed) {
                return;
            }
        }

        action();
    };

    return (
        <div className="info-page-form">
            <div className="info-page-form__header">
                <div className="info-page-form__header-left">
                    <button
                        type="button"
                        className="info-page-form__back-button"
                        onClick={() => handleAttemptClose(onBack)}
                        aria-label={t("common.back", "Takaisin")}
                    >
                        <img src={backArrow} alt="" />
                    </button>

                    <div>
                        <span className="info-page-form__mode-badge">
                            {t(
                                "admin.siteContent.infoPages.editMode",
                                "Muokkaustila",
                            )}
                        </span>
                        <h3>
                            {t(page.labelKey, page.defaultLabel)}
                        </h3>
                    </div>
                </div>

                <div className="info-page-form__header-actions">
                    <Link
                        to={page.path}
                        className="info-page-form__preview-link"
                        target="_blank"
                        rel="noreferrer"
                    >
                        {t(
                            "admin.siteContent.infoPages.previewPublic",
                            "Avaa julkinen sivu",
                        )}
                    </Link>

                    <Button
                        rounded="pill"
                        onClick={onSave}
                        disabled={isSaving || !hasContent || !hasChanges}
                    >
                        {t("admin.siteContent.infoPages.save", "Tallenna")}
                    </Button>
                </div>
            </div>

            <div className="info-page-form__body">
                <div className="info-page-form__card">
                    <div className="info-page-form__field">
                        <label
                            className="info-page-form__label"
                            htmlFor="info-page-title"
                        >
                            {t(
                                "admin.siteContent.infoPages.titleField",
                                "Sivun otsikko",
                            )}
                        </label>
                        <input
                            id="info-page-title"
                            type="text"
                            className="info-page-form__title-input"
                            value={title}
                            onChange={(event) =>
                                onTitleChange(event.target.value)
                            }
                        />
                    </div>

                    <div className="info-page-form__field">
                        <label className="info-page-form__label">
                            {t(
                                "admin.siteContent.infoPages.contentField",
                                "Sivun sisältö",
                            )}
                        </label>
                        <RichTextEditor
                            value={contentHtml}
                            onChange={onContentChange}
                            showMatchInsert={false}
                        />
                    </div>
                </div>
            </div>
        </div>
    );
}
