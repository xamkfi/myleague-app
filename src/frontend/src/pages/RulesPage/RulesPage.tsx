import { useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import PageTemplate from "../../components/PageTemplate/PageTemplate";
import type { PageContentResponse } from "../../types/admin/ruleTypes";
import { pageContentService } from "../../services/pageContentService";
import { parseRulesFromHtml } from "../../utils/helpers";
import "./RulesPage.scss";

const RULES_SLUG = "saannot";

const mahlLinks = [
    {
        label: "Summary",
        path: "/mahl",
    },
    {
        label: "Seuran talous",
        path: "/mahl/seuran-talous",
    },
    {
        label: "Kumppanuudet",
        path: "/mahl/kumppanuudet",
    },
    {
        label: "Vastuullisuus",
        path: "/mahl/vastuullisuus",
    },
    {
        label: "Säännöt",
        path: "/saannot",
        active: true,
    },
];

const categoryTranslationKeys: Record<string, string> = {
    all: "rules.admin.allCategories",
    general: "rules.admin.categories.general",
    fees: "rules.admin.categories.fees",
    validation: "rules.admin.categories.validation",
    calculation: "rules.admin.categories.calculation",
};

export default function RulesPage() {
    const { t } = useTranslation();

    const [pageContent, setPageContent] = useState<PageContentResponse | null>(
        null,
    );
    const [isLoading, setIsLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);
    const [filterCategory, setFilterCategory] = useState<string>("all");

    const translate = (key: string, defaultValue: string): string => {
        return t(key, { defaultValue });
    };

    useEffect(() => {
        let isMounted = true;

        const loadPageContent = async (): Promise<void> => {
            try {
                setIsLoading(true);
                setError(null);

                const response =
                    await pageContentService.getPageContent(RULES_SLUG);

                if (!isMounted) {
                    return;
                }

                setPageContent(response);
            } catch (err) {
                if (!isMounted) {
                    return;
                }

                const message =
                    err instanceof Error
                        ? err.message
                        : translate("rules.loadFailed", "Sääntöjen lataus epäonnistui.");

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
        return parseRulesFromHtml(pageContent?.contentHtml ?? "");
    }, [pageContent?.contentHtml]);

    const availableCategories = useMemo(() => {
        const categories = new Set<string>();

        rules.forEach((rule) => {
            if (rule.category) {
                categories.add(rule.category);
            }
        });

        return ["all", ...Array.from(categories)];
    }, [rules]);

    const filteredRules = useMemo(() => {
        return rules.filter((rule) => {
            return filterCategory === "all" || rule.category === filterCategory;
        });
    }, [rules, filterCategory]);

    const getCategoryLabel = (category: string): string => {
        return t(categoryTranslationKeys[category] ?? category, {
            defaultValue: category,
        });
    };

    return (
        <PageTemplate
            title={
                pageContent?.title ||
                translate("rules.title", "Säännöt")
            }
        >
            <main className="rules-page">
                <section className="rules-page__hero">
                    <div className="rules-page__hero-overlay">
                        <div className="rules-page__hero-content">
                            <p className="rules-page__eyebrow">MAHL</p>

                            <h1 className="rules-page__title">
                                {translate("rules.publicTitle", "MAHL Säännöt")}
                            </h1>

                            <p className="rules-page__intro">
                                {translate(
                                    "rules.publicIntro",
                                    "Tältä sivulta löydät MAHL-toimintaan liittyvät yleissäännöt, maksut, tarkistukset ja laskentaperiaatteet.",
                                )}
                            </p>
                        </div>
                    </div>
                </section>

                <nav
                    className="rules-page__mahl-nav"
                    aria-label={translate(
                        "rules.mahlNavigation",
                        "MAHL navigaatio",
                    )}
                >
                    {mahlLinks.map((link) => (
                        <Link
                            key={link.label}
                            to={link.path}
                            className={
                                link.active
                                    ? "rules-page__mahl-nav-link rules-page__mahl-nav-link--active"
                                    : "rules-page__mahl-nav-link"
                            }
                        >
                            {link.label}
                        </Link>
                    ))}
                </nav>

                <section className="rules-page__content-section">
                    <div className="rules-page__section-header">
                        <div>
                            <p className="rules-page__section-kicker">
                                {translate("rules.sectionKicker", "Säännöstö")}
                            </p>

                            <h2>
                                {pageContent?.title ||
                                    translate(
                                        "rules.generalRules",
                                        "MAHL yleissääntöjä",
                                    )}
                            </h2>
                        </div>

                        {!isLoading && (
                            <span className="rules-page__rule-count">
                                {filteredRules.length}{" "}
                                {translate("rules.ruleCountLabel", "sääntöä")}
                            </span>
                        )}
                    </div>

                    {availableCategories.length > 2 && (
                        <div
                            className="rules-page__category-tabs"
                            aria-label={translate(
                                "rules.categoryFilter",
                                "Suodata sääntöjä kategorian mukaan",
                            )}
                        >
                            {availableCategories.map((category) => (
                                <button
                                    key={category}
                                    type="button"
                                    className={
                                        filterCategory === category
                                            ? "rules-page__category-tab rules-page__category-tab--active"
                                            : "rules-page__category-tab"
                                    }
                                    onClick={() => setFilterCategory(category)}
                                >
                                    {getCategoryLabel(category)}
                                </button>
                            ))}
                        </div>
                    )}

                    {error && !error.includes("not found") && (
                        <div className="rules-page__alert rules-page__alert--error">
                            {error}
                        </div>
                    )}

                    {isLoading ? (
                        <div className="rules-page__loading">
                            {translate("rules.loading", "Ladataan sääntöjä...")}
                        </div>
                    ) : filteredRules.length === 0 ? (
                        <div className="rules-page__empty">
                            {translate(
                                "rules.noRules",
                                "Sääntöjä ei ole vielä lisätty.",
                            )}
                        </div>
                    ) : (
                        <div className="rules-page__rules-list">
                            {filteredRules.map((rule, index) => (
                                <article
                                    key={rule.id}
                                    className="rules-page__rule-card"
                                >
                                    <div className="rules-page__rule-number">
                                        {String(index + 1).padStart(2, "0")}.
                                    </div>

                                    <div className="rules-page__rule-content">
                                        {rule.category && (
                                            <span className="rules-page__rule-category">
                                                {getCategoryLabel(rule.category)}
                                            </span>
                                        )}

                                        <div
                                            className="rules-page__rule-html"
                                            dangerouslySetInnerHTML={{
                                                __html: rule.html,
                                            }}
                                        />
                                    </div>
                                </article>
                            ))}
                        </div>
                    )}
                </section>
            </main>
        </PageTemplate>
    );
}
