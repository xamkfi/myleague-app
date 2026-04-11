import { useEffect, useMemo, useState } from "react";
import "./RulesPage.scss";
import type { PageContentResponse } from "../../types/admin/ruleTypes";
import { pageContentService } from "../../services/pageContentService";
import { useTranslation } from "react-i18next";
import { parseRulesFromHtml } from "../../utils/helpers";
import RulesList from "../AdminPage/RulesPage/components/RulesList";

export default function RulesPage() {
    const { t } = useTranslation();

    const [pageContent, setPageContent] = useState<PageContentResponse | null>(
        null,
    );
    const [isLoading, setIsLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        let isMounted = true;

        const loadPageContent = async (): Promise<void> => {
            try {
                setIsLoading(true);
                setError(null);

                const response =
                    await pageContentService.getPageContent("saannot");

                if (!isMounted) {
                    return;
                }

                setPageContent(response);
            } catch (err) {
                if (!isMounted) {
                    return;
                }

                const message =
                    err instanceof Error ? err.message : t("rules.loadFailed");
                setError(message);
                setPageContent(null);
            } finally {
                if (isMounted) {
                    setIsLoading(false);
                }
            }
        };

        loadPageContent();

        return () => {
            isMounted = false;
        };
    }, []);

    const rules = useMemo(() => {
        return parseRulesFromHtml(pageContent?.contentHtml ?? "").reverse();
    }, [pageContent?.contentHtml]);

    return (
        <div className="rules-page container">
            <h1>{pageContent?.title || t("Rules")}</h1>

            {error && !error.includes("not found") && (
                <div className="rules-page__alert--error">{error}</div>
            )}

            <div className="rules-management-page__content">
                <RulesList
                    title=""
                    rules={rules}
                    emptyMessage={t("No rules have been added.")}
                    isExpanded={true}
                    isSaving={false}
                    isLoading={isLoading}
                    showActions={false}
                    showExpand={false}
                />
            </div>
        </div>
    );
}
