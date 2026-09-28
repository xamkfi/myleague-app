import { useEffect, useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import PageTemplate from "../../components/PageTemplate/PageTemplate";
import MahlInfoLayout from "../../components/MahlInfoLayout/MahlInfoLayout";
import SportIcon from "../../components/SportIcon/SportIcon";
import UnderlineTabs from "../../components/UnderlineTabs/UnderlineTabs";
import type { RulesSection } from "../../types/admin/ruleTypes";
import { rulesSectionService } from "../../services/rulesSectionService";
import { parseRulesFromHtml } from "../../utils/helpers";
import {
    formatRuleNumber,
    getChildSections,
    getTopLevelSections,
    sortRulesByOrder,
} from "../../utils/rulesSectionUtils";
import "./RulesPage.scss";

function sportFromSectionTitle(title: string): string {
    const normalized = title
        .toLowerCase()
        .normalize("NFD")
        .replace(/[\u0300-\u036f]/g, "");

    if (normalized.includes("salibandy") || normalized.includes("floorball")) {
        return "floorball";
    }

    if (
        normalized.includes("jalkapallo") ||
        normalized.includes("football") ||
        normalized.includes("soccer")
    ) {
        return "football";
    }

    if (normalized.includes("jaakiekko") || normalized.includes("hockey")) {
        return "hockey";
    }

    return title;
}

export default function RulesPage() {
    const { t } = useTranslation();

    const [sections, setSections] = useState<RulesSection[]>([]);
    const [isLoading, setIsLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);
    const [activeSectionId, setActiveSectionId] = useState<string>("");
    const [activeSportSectionId, setActiveSportSectionId] = useState<string>("");

    useEffect(() => {
        let isMounted = true;

        const loadSections = async (): Promise<void> => {
            try {
                setIsLoading(true);
                setError(null);

                const loadedSections = await rulesSectionService.getAllSections();

                if (!isMounted) {
                    return;
                }

                setSections(loadedSections);

                const topLevel = getTopLevelSections(loadedSections);
                const firstSection = topLevel[0];

                if (firstSection) {
                    setActiveSectionId(firstSection.id);

                    if (firstSection.sectionType === "SportGroup") {
                        const sports = getChildSections(
                            loadedSections,
                            firstSection.id,
                        );
                        setActiveSportSectionId(sports[0]?.id ?? "");
                    }
                }
            } catch (err) {
                if (!isMounted) {
                    return;
                }

                setError(
                    err instanceof Error
                        ? err.message
                        : t("rules.loadFailed", "Sääntöjen lataus epäonnistui."),
                );
            } finally {
                if (isMounted) {
                    setIsLoading(false);
                }
            }
        };

        loadSections();

        return () => {
            isMounted = false;
        };
    }, [t]);

    const topLevelSections = useMemo(
        () => getTopLevelSections(sections),
        [sections],
    );

    const activeTopSection = useMemo(() => {
        return topLevelSections.find(
            (section) => section.id === activeSectionId,
        );
    }, [activeSectionId, topLevelSections]);

    const sportSections = useMemo(() => {
        if (activeTopSection?.sectionType !== "SportGroup") {
            return [];
        }

        return getChildSections(sections, activeTopSection.id);
    }, [activeTopSection, sections]);

    useEffect(() => {
        if (
            activeTopSection?.sectionType === "SportGroup" &&
            sportSections.length > 0 &&
            !sportSections.some((section) => section.id === activeSportSectionId)
        ) {
            setActiveSportSectionId(sportSections[0].id);
        }
    }, [activeTopSection, sportSections, activeSportSectionId]);

    const contentSection = useMemo(() => {
        if (activeTopSection?.sectionType === "SportGroup") {
            return sportSections.find(
                (section) => section.id === activeSportSectionId,
            );
        }

        return activeTopSection;
    }, [activeTopSection, activeSportSectionId, sportSections]);

    const displayedRules = useMemo(() => {
        if (!contentSection) {
            return [];
        }

        return sortRulesByOrder(
            parseRulesFromHtml(contentSection.contentHtml, contentSection.id),
        );
    }, [contentSection]);

    const sectionHeading =
        contentSection?.title ||
        t("rules.generalRules", "MAHL yleissääntöjä");

    return (
        <PageTemplate title={t("rules.title", "Säännöt")} fullBleed>
            <MahlInfoLayout
                pageTitle={t("nav.mahl", "MAHL")}
                intro={t(
                    "rules.publicIntro",
                    "Tältä sivulta löydät MAHL-toimintaan liittyvät yleissäännöt, lajikohtaiset säännöt sekä muut sääntökokonaisuudet.",
                )}
            >
                <section className="rules-page__content-section">
                    <div className="rules-page__section-header">
                        <div>
                            <p className="rules-page__section-kicker">
                                {t("rules.sectionKicker", "Säännöstö")}
                            </p>

                            <h2>{sectionHeading}</h2>
                        </div>

                        {!isLoading && (
                            <span className="rules-page__rule-count">
                                {displayedRules.length}{" "}
                                {t("rules.ruleCountLabel", "sääntöä")}
                            </span>
                        )}
                    </div>

                    {topLevelSections.length > 1 && (
                        <div
                            className="rules-page__category-tabs"
                            aria-label={t(
                                "rules.sectionTabs",
                                "Sääntöosion valinta",
                            )}
                        >
                            {topLevelSections.map((section) => (
                                <button
                                    key={section.id}
                                    type="button"
                                    className={
                                        activeSectionId === section.id
                                            ? "rules-page__category-tab rules-page__category-tab--active"
                                            : "rules-page__category-tab"
                                    }
                                    onClick={() => setActiveSectionId(section.id)}
                                >
                                    {section.title}
                                </button>
                            ))}
                        </div>
                    )}

                    {activeTopSection?.sectionType === "SportGroup" &&
                        sportSections.length > 0 && (
                            <UnderlineTabs
                                tabs={sportSections.map((section) => ({
                                    id: section.id,
                                    label: section.title,
                                    icon: (
                                        <SportIcon
                                            sport={sportFromSectionTitle(
                                                section.title,
                                            )}
                                            size="sm"
                                            decorative
                                        />
                                    ),
                                }))}
                                activeId={activeSportSectionId}
                                onChange={setActiveSportSectionId}
                                ariaLabel={t(
                                    "rules.selectSport",
                                    "Valitse laji",
                                )}
                            />
                        )}

                    {error && (
                        <div className="rules-page__alert rules-page__alert--error">
                            {error}
                        </div>
                    )}

                    <div
                        role={
                            activeTopSection?.sectionType === "SportGroup"
                                ? "tabpanel"
                                : undefined
                        }
                        id={
                            activeTopSection?.sectionType === "SportGroup" &&
                            activeSportSectionId
                                ? `tabpanel-${activeSportSectionId}`
                                : undefined
                        }
                        aria-labelledby={
                            activeTopSection?.sectionType === "SportGroup" &&
                            activeSportSectionId
                                ? `tab-${activeSportSectionId}`
                                : undefined
                        }
                    >
                        {isLoading ? (
                            <div className="rules-page__loading">
                                {t("rules.loading", "Ladataan sääntöjä...")}
                            </div>
                        ) : displayedRules.length === 0 ? (
                            <div className="rules-page__empty">
                                {t(
                                    "rules.noRules",
                                    "Sääntöjä ei ole vielä lisätty.",
                                )}
                            </div>
                        ) : (
                            <div className="rules-page__rules-list">
                                {displayedRules.map((rule) => (
                                    <article
                                        key={rule.id}
                                        className="rules-page__rule-card"
                                    >
                                        <div className="rules-page__rule-number">
                                            {formatRuleNumber(rule.order)}
                                        </div>

                                        <div className="rules-page__rule-content">
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
                    </div>
                </section>
            </MahlInfoLayout>
        </PageTemplate>
    );
}
