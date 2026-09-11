import { useEffect, useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import PageTemplate from "../../components/PageTemplate/PageTemplate";
import MahlInfoLayout from "../../components/MahlInfoLayout/MahlInfoLayout";
import { getMahlInfoPageBySlug } from "../../constants/mahlInfoPages";
import { infoPageContentService } from "../../services/infoPageContentService";
import "./MahlInfoPage.scss";

interface MahlInfoPageProps {
    slug: string;
}

export default function MahlInfoPage({ slug }: MahlInfoPageProps) {
    const { t } = useTranslation();
    const pageDefinition = getMahlInfoPageBySlug(slug);

    const [title, setTitle] = useState<string>("");
    const [contentHtml, setContentHtml] = useState<string>("");
    const [isLoading, setIsLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);

    const defaultTitle = useMemo(() => {
        if (!pageDefinition) {
            return t("nav.mahl", "MAHL");
        }

        return t(pageDefinition.labelKey, pageDefinition.defaultTitle);
    }, [pageDefinition, t]);

    useEffect(() => {
        let isMounted = true;

        const loadContent = async (): Promise<void> => {
            try {
                setIsLoading(true);
                setError(null);

                const page = await infoPageContentService.getPageContent(slug);

                if (!isMounted) {
                    return;
                }

                setTitle(page.title);
                setContentHtml(page.contentHtml);
            } catch (loadError) {
                if (!isMounted) {
                    return;
                }

                setError(
                    loadError instanceof Error
                        ? loadError.message
                        : t("infoPages.loadFailed", "Sivun lataus epäonnistui."),
                );
            } finally {
                if (isMounted) {
                    setIsLoading(false);
                }
            }
        };

        loadContent();

        return () => {
            isMounted = false;
        };
    }, [slug, t]);

    return (
        <PageTemplate title={title || defaultTitle} fullBleed>
            <MahlInfoLayout pageTitle={t("nav.mahl", "MAHL")}>
                <section className="mahl-info-page">
                    {isLoading && (
                        <p className="mahl-info-page__status">
                            {t("common.loading", "Ladataan...")}
                        </p>
                    )}

                    {!isLoading && error && (
                        <p className="mahl-info-page__status mahl-info-page__status--error">
                            {error}
                        </p>
                    )}

                    {!isLoading && !error && (
                        <>
                            <h2 className="mahl-info-page__title">
                                {title || defaultTitle}
                            </h2>

                            {contentHtml.trim().length > 0 ? (
                                <div
                                    className="mahl-info-page__body"
                                    dangerouslySetInnerHTML={{
                                        __html: contentHtml,
                                    }}
                                />
                            ) : (
                                <p className="mahl-info-page__status">
                                    {t(
                                        "infoPages.emptyContent",
                                        "Sivun sisältöä ei ole vielä julkaistu.",
                                    )}
                                </p>
                            )}
                        </>
                    )}
                </section>
            </MahlInfoLayout>
        </PageTemplate>
    );
}
