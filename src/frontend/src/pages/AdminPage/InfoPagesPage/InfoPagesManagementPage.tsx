import { useEffect, useMemo, useState } from "react";
import PageTemplate from "../../../components/PageTemplate/AdminPageTemplate";
import Button from "../../../components/Button/Button";
import { useTranslation } from "react-i18next";
import { infoPageContentService } from "../../../services/infoPageContentService";
import {
    CHILDREN_INFO_PAGES,
    MAHL_INFO_PAGES,
    type MahlInfoPageDefinition,
} from "../../../constants/mahlInfoPages";
import type { InfoPageListItem } from "../../../types/admin/infoPageContentTypes";
import InfoPageForm from "./components/InfoPageForm";
import "./InfoPagesManagementPage.scss";

function formatUpdatedAt(
    value: string | null,
    locale: string,
): string {
    if (!value) {
        return "—";
    }

    return new Intl.DateTimeFormat(locale, {
        dateStyle: "medium",
        timeStyle: "short",
    }).format(new Date(value));
}

export type InfoPagesVariant = "mahl" | "children";

interface InfoPagesVariantConfig {
    definitions: MahlInfoPageDefinition[];
    pageTitleKey: string;
    pageTitleFallback: string;
    subtitleKey: string;
    subtitleFallback: string;
    descriptionKey: string;
    descriptionFallback: string;
}

const VARIANT_CONFIG: Record<InfoPagesVariant, InfoPagesVariantConfig> = {
    mahl: {
        definitions: MAHL_INFO_PAGES,
        pageTitleKey: "admin.siteContent.infoPages.pageTitle",
        pageTitleFallback: "MAHL-infosivut",
        subtitleKey: "admin.siteContent.infoPages.subtitle",
        subtitleFallback: "Hallitse MAHL-osion staattisia infosivuja.",
        descriptionKey: "admin.siteContent.infoPages.description",
        descriptionFallback:
            "Valitse sivu listasta tai pudotusvalikosta ja muokkaa sisältöä RichText-editorilla. Kuvat ja sponsorilogot lisätään suoraan sisältöön.",
    },
    children: {
        definitions: CHILDREN_INFO_PAGES,
        pageTitleKey: "admin.siteContent.childrenPages.pageTitle",
        pageTitleFallback: "Lapset-sivut",
        subtitleKey: "admin.siteContent.childrenPages.subtitle",
        subtitleFallback: "Hallitse Lapset-osion infosivuja.",
        descriptionKey: "admin.siteContent.childrenPages.description",
        descriptionFallback:
            "Valitse sivu listasta tai pudotusvalikosta ja muokkaa sisältöä RichText-editorilla. Kuvat lisätään suoraan sisältöön.",
    },
};

interface InfoPagesManagementPageProps {
    variant?: InfoPagesVariant;
}

export default function InfoPagesManagementPage({
    variant = "mahl",
}: InfoPagesManagementPageProps) {
    const { t, i18n } = useTranslation();
    const config = VARIANT_CONFIG[variant];

    const [pages, setPages] = useState<InfoPageListItem[]>([]);
    const [selectedSlug, setSelectedSlug] = useState<string>("");
    const [editTitle, setEditTitle] = useState<string>("");
    const [editContentHtml, setEditContentHtml] = useState<string>("");
    const [isLoading, setIsLoading] = useState<boolean>(true);
    const [isSaving, setIsSaving] = useState<boolean>(false);
    const [isFormOpen, setIsFormOpen] = useState<boolean>(false);
    const [successMessage, setSuccessMessage] = useState<string | null>(null);
    const [errorMessage, setErrorMessage] = useState<string | null>(null);

    const selectedPage = useMemo(() => {
        return pages.find((page) => page.slug === selectedSlug) ?? null;
    }, [pages, selectedSlug]);

    const loadPages = async (): Promise<void> => {
        const apiPages = await infoPageContentService.getAllInfoPages();
        setPages(
            infoPageContentService.buildListItems(
                apiPages,
                config.definitions,
            ),
        );
    };

    useEffect(() => {
        let isMounted = true;

        const initialize = async (): Promise<void> => {
            try {
                setIsLoading(true);
                setErrorMessage(null);
                await loadPages();
            } catch (error) {
                if (!isMounted) {
                    return;
                }

                setErrorMessage(
                    error instanceof Error
                        ? error.message
                        : t(
                              "admin.siteContent.infoPages.loadFailed",
                              "Infosivujen lataus epäonnistui.",
                          ),
                );
            } finally {
                if (isMounted) {
                    setIsLoading(false);
                }
            }
        };

        initialize();

        return () => {
            isMounted = false;
        };
    }, [t, variant]);

    useEffect(() => {
        if (!successMessage) {
            return;
        }

        const timeout = setTimeout(() => {
            setSuccessMessage(null);
        }, 6000);

        return () => clearTimeout(timeout);
    }, [successMessage]);

    const handleOpenEdit = (page: InfoPageListItem): void => {
        setSelectedSlug(page.slug);
        setEditTitle(page.title);
        setEditContentHtml(
            page.contentHtml ||
                `<p>${t("admin.siteContent.infoPages.defaultContent", "Lisää sisältö tähän.")}</p>`,
        );
        setErrorMessage(null);
        setSuccessMessage(null);
        setIsFormOpen(true);
    };

    const handleDropdownChange = (slug: string): void => {
        if (!slug) {
            return;
        }

        const page = pages.find((item) => item.slug === slug);

        if (page) {
            handleOpenEdit(page);
        }
    };

    const handleCloseForm = (): void => {
        setIsFormOpen(false);
        setSelectedSlug("");
    };

    const handleSave = async (): Promise<void> => {
        if (!selectedPage) {
            return;
        }

        try {
            setIsSaving(true);
            setErrorMessage(null);

            await infoPageContentService.updatePageContent(selectedPage.slug, {
                title: editTitle.trim(),
                contentHtml: editContentHtml,
            });

            await loadPages();
            setSuccessMessage(
                t(
                    "admin.siteContent.infoPages.saveSuccess",
                    "Sivu tallennettu onnistuneesti.",
                ),
            );
            setIsFormOpen(false);
            setSelectedSlug("");
        } catch (error) {
            setErrorMessage(
                error instanceof Error
                    ? error.message
                    : t(
                          "admin.siteContent.infoPages.saveFailed",
                          "Sivun tallennus epäonnistui.",
                      ),
            );
        } finally {
            setIsSaving(false);
        }
    };

    return (
        <PageTemplate
            title={t(config.pageTitleKey, config.pageTitleFallback)}
        >
            <div className="info-pages-management-page">
                <div className="info-pages-management-page__alerts-overlay">
                    {successMessage && (
                        <div className="info-pages-management-page__alert info-pages-management-page__alert--success">
                            {successMessage}
                        </div>
                    )}

                    {errorMessage && (
                        <div className="info-pages-management-page__alert info-pages-management-page__alert--error">
                            {errorMessage}
                        </div>
                    )}
                </div>

                {!isFormOpen && (
                    <>
                        <div className="info-pages-management-page__topbar">
                            <div>
                                <h2 className="info-pages-management-page__page-title">
                                    {t(
                                        config.subtitleKey,
                                        config.subtitleFallback,
                                    )}
                                </h2>
                                <p className="info-pages-management-page__description">
                                    {t(
                                        config.descriptionKey,
                                        config.descriptionFallback,
                                    )}
                                </p>
                            </div>
                        </div>

                        <div className="info-pages-management-page__filter-card">
                            <label
                                className="info-pages-management-page__filter-label"
                                htmlFor="info-page-select"
                            >
                                {t(
                                    "admin.siteContent.infoPages.selectPage",
                                    "Valitse muokattava sivu",
                                )}
                            </label>
                            <select
                                id="info-page-select"
                                className="info-pages-management-page__select"
                                value={selectedSlug}
                                onChange={(event) =>
                                    handleDropdownChange(event.target.value)
                                }
                                disabled={isLoading || isSaving}
                            >
                                <option value="">
                                    {t(
                                        "admin.siteContent.infoPages.selectPlaceholder",
                                        "Valitse sivu...",
                                    )}
                                </option>
                                {pages.map((page) => (
                                    <option key={page.slug} value={page.slug}>
                                        {t(page.labelKey, page.defaultLabel)}
                                    </option>
                                ))}
                            </select>
                        </div>

                        <div className="info-pages-management-page__table-wrapper">
                            <table className="info-pages-management-page__table">
                                <thead>
                                    <tr>
                                        <th>
                                            {t(
                                                "admin.siteContent.infoPages.table.page",
                                                "Sivu",
                                            )}
                                        </th>
                                        <th>
                                            {t(
                                                "admin.siteContent.infoPages.table.title",
                                                "Otsikko",
                                            )}
                                        </th>
                                        <th>
                                            {t(
                                                "admin.siteContent.infoPages.table.updated",
                                                "Viimeksi muokattu",
                                            )}
                                        </th>
                                        <th>
                                            {t(
                                                "admin.siteContent.infoPages.table.actions",
                                                "Toiminnot",
                                            )}
                                        </th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {pages.map((page) => (
                                        <tr key={page.slug}>
                                            <td>
                                                {t(
                                                    page.labelKey,
                                                    page.defaultLabel,
                                                )}
                                            </td>
                                            <td>{page.title}</td>
                                            <td>
                                                {formatUpdatedAt(
                                                    page.updatedAt,
                                                    i18n.language,
                                                )}
                                            </td>
                                            <td>
                                                <Button
                                                    rounded="pill"
                                                    variant="secondary"
                                                    onClick={() =>
                                                        handleOpenEdit(page)
                                                    }
                                                    disabled={isSaving}
                                                >
                                                    {t(
                                                        "admin.siteContent.infoPages.edit",
                                                        "Muokkaa",
                                                    )}
                                                </Button>
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>

                            {!isLoading && pages.length === 0 && (
                                <p className="info-pages-management-page__empty">
                                    {t(
                                        "admin.siteContent.infoPages.noPages",
                                        "Infosivuja ei löytynyt.",
                                    )}
                                </p>
                            )}
                        </div>
                    </>
                )}

                {isFormOpen && selectedPage && (
                    <InfoPageForm
                        page={selectedPage}
                        title={editTitle}
                        contentHtml={editContentHtml}
                        isSaving={isSaving}
                        onBack={handleCloseForm}
                        onTitleChange={setEditTitle}
                        onContentChange={setEditContentHtml}
                        onSave={handleSave}
                    />
                )}
            </div>
        </PageTemplate>
    );
}
