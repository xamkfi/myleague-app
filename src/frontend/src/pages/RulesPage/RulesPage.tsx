import { useEffect, useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import PageTemplate from "../../components/PageTemplate/PageTemplate";
import type { PageContentResponse } from "../../types/admin/ruleTypes";
import { pageContentService } from "../../services/pageContentService";
import { parseRulesFromHtml } from "../../utils/helpers";
import "./RulesPage.scss";
import CategorySelect from "../AdminPage/RulesPage/components/CategorySelect";
import RulesList from "../AdminPage/RulesPage/components/RulesList";

export default function RulesPage() {
    const { t } = useTranslation();

    const [pageContent, setPageContent] = useState<PageContentResponse | null>(
        null,
    );
    const [isLoading, setIsLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);
    const [filterCategory, setFilterCategory] = useState<string>("all");

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
    }, [t]);

    const rules = useMemo(() => {
        return parseRulesFromHtml(pageContent?.contentHtml ?? "").reverse();
    }, [pageContent?.contentHtml]);

    const filteredRules = useMemo(() => {
        return rules.filter((rule) => {
            return filterCategory === "all" || rule.category === filterCategory;
        });
    }, [rules, filterCategory]);

    return (
        <PageTemplate title={pageContent?.title || t("Rules")}>
            <div className="rules-page__hero">
                <div className="rules-page__hero-content">
                    <h1 className="rules-page__title">
                        {pageContent?.title || t("Rules")}
                    </h1>
                </div>
            </div>

            <div className="rules-page__content-section">
                <div className="rules-page__filter-wrapper">
                    <CategorySelect
                        value={filterCategory}
                        onChange={setFilterCategory}
                        includeAll
                    />
                </div>

                {error && !error.includes("not found") && (
                    <div className="rules-page__alert rules-page__alert--error">
                        {error}
                    </div>
                )}

                <div className="rules-page__list-wrapper">
                    <RulesList
                        title=""
                        rules={filteredRules}
                        emptyMessage={t("No rules have been added.")}
                        isSaving={false}
                        isLoading={isLoading}
                        showActions={false}
                    />
                </div>
            </div>
        </PageTemplate>
    );
}
