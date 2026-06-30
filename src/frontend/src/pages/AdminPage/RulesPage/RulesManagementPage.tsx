import { useEffect, useMemo, useState } from "react";
import PageTemplate from "../../../components/PageTemplate/AdminPageTemplate";
import Button from "../../../components/Button/Button";
import AddIcon from "../../../assets/basicIcons/add.svg";
import "./RulesManagementPage.scss";
import type { RuleItem, RulesSection } from "../../../types/admin/ruleTypes";
import { rulesSectionService } from "../../../services/rulesSectionService";
import { useTranslation } from "react-i18next";
import RuleForm from "./components/RulesForm";
import RulesList from "./components/RulesList";
import SectionSelect from "./components/SectionSelect";
import RulesSectionList from "./components/RulesSectionList";
import RulesSectionForm, {
    type RulesSectionFormState,
    type SectionCreateMode,
} from "./components/RulesSectionForm";
import { createRuleBlock, parseRulesFromHtml } from "../../../utils/helpers";
import {
    findGlobalSection,
    findSportGroupSection,
    getChildSections,
    getNextRuleOrder,
    getRuleableSections,
    getTopLevelSections,
    resolveRuleOrderConflict,
    sortRulesByOrder,
} from "../../../utils/rulesSectionUtils";

function formatSectionDeleteError(
    message: string,
    t: (key: string, defaultValue: string) => string,
): string {
    if (message.includes("child sections")) {
        return t(
            "rules.admin.sectionDeleteHasChildren",
            "Osioita ei voi poistaa ennen kuin sen lajiosiot on poistettu.",
        );
    }

    if (message.includes("contains rules")) {
        return t(
            "rules.admin.sectionDeleteHasRules",
            "Osioita ei voi poistaa ennen kuin kaikki säännöt on poistettu.",
        );
    }

    if (message.includes("SportGroup section already exists")) {
        return t(
            "rules.admin.sportGroupAlreadyExists",
            "Lajikohtaiset säännöt -ryhmä on jo olemassa. Lisää uusi laji tyypillä Laji (Sport).",
        );
    }

    return message;
}

type RuleFormState = {
    id: string | null;
    html: string;
    sectionId: string;
    order: number;
};

type AdminPanel = "rules" | "sections";

const emptyRuleFormState = (): RuleFormState => ({
    id: null,
    html: "",
    sectionId: "",
    order: 1,
});

const emptySectionFormState = (): RulesSectionFormState => ({
    id: null,
    title: "",
    sortOrder: 1,
    sectionType: "Global",
    parentSectionId: null,
});

export default function RulesManagementPage() {
    const { t } = useTranslation();

    const [activePanel, setActivePanel] = useState<AdminPanel>("rules");
    const [sections, setSections] = useState<RulesSection[]>([]);
    const [filterSectionId, setFilterSectionId] = useState<string>("all");
    const [isLoading, setIsLoading] = useState<boolean>(true);
    const [isSaving, setIsSaving] = useState<boolean>(false);
    const [successMessage, setSuccessMessage] = useState<string | null>(null);
    const [errorMessage, setErrorMessage] = useState<string | null>(null);
    const [isRuleFormOpen, setIsRuleFormOpen] = useState<boolean>(false);
    const [isSectionFormOpen, setIsSectionFormOpen] = useState<boolean>(false);
    const [ruleFormState, setRuleFormState] =
        useState<RuleFormState>(emptyRuleFormState());
    const [sectionFormState, setSectionFormState] =
        useState<RulesSectionFormState>(emptySectionFormState());
    const [sectionCreateMode, setSectionCreateMode] =
        useState<SectionCreateMode>("main");

    const loadSections = async (): Promise<void> => {
        const loadedSections = await rulesSectionService.getAllSections();
        setSections(loadedSections);

        const ruleableSections = getRuleableSections(loadedSections);
        const defaultSection =
            findGlobalSection(loadedSections) ?? ruleableSections[0];

        setRuleFormState((prev) => ({
            ...prev,
            sectionId: prev.sectionId || defaultSection?.id || "",
        }));
    };

    useEffect(() => {
        let isMounted = true;

        const initialize = async (): Promise<void> => {
            try {
                setIsLoading(true);
                setErrorMessage(null);
                await loadSections();
            } catch (error) {
                if (!isMounted) {
                    return;
                }

                setErrorMessage(
                    error instanceof Error
                        ? error.message
                        : t("rules.admin.loadFailed"),
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
    }, [t]);

    useEffect(() => {
        if (!successMessage) {
            return;
        }

        const timeout = setTimeout(() => {
            setSuccessMessage(null);
        }, 6000);

        return () => clearTimeout(timeout);
    }, [successMessage]);

    const allRules = useMemo(() => {
        return getRuleableSections(sections).flatMap((section) =>
            sortRulesByOrder(
                parseRulesFromHtml(section.contentHtml, section.id),
            ),
        );
    }, [sections]);

    const filteredRules = useMemo(() => {
        if (filterSectionId === "all") {
            return allRules;
        }

        return allRules.filter((rule) => rule.sectionId === filterSectionId);
    }, [allRules, filterSectionId]);

    const handleOpenCreateRule = (): void => {
        let targetSection = "";

        if (filterSectionId !== "all") {
            targetSection = filterSectionId;
        } else {
            const sportGroup = findSportGroupSection(sections);
            const sportSections = sportGroup
                ? getChildSections(sections, sportGroup.id)
                : [];

            targetSection =
                sportSections[0]?.id ||
                findGlobalSection(sections)?.id ||
                getRuleableSections(sections)[0]?.id ||
                "";
        }

        const sectionRules = allRules.filter(
            (rule) => rule.sectionId === targetSection,
        );

        setRuleFormState({
            ...emptyRuleFormState(),
            sectionId: targetSection,
            order: getNextRuleOrder(sectionRules),
        });
        setErrorMessage(null);
        setSuccessMessage(null);
        setIsRuleFormOpen(true);
    };

    const handleStartEditRule = (rule: RuleItem): void => {
        setRuleFormState({
            id: rule.id,
            html: rule.html,
            sectionId: rule.sectionId || "",
            order: rule.order,
        });
        setErrorMessage(null);
        setSuccessMessage(null);
        setIsRuleFormOpen(true);
    };

    const handleSaveRule = async (): Promise<void> => {
        if (!ruleFormState.sectionId) {
            setErrorMessage(t("rules.admin.selectSectionFirst"));
            return;
        }

        const normalizedOrder = Math.max(1, ruleFormState.order);
        const sectionRules = allRules.filter(
            (rule) => rule.sectionId === ruleFormState.sectionId,
        );
        const previousOrder = ruleFormState.id
            ? allRules.find((rule) => rule.id === ruleFormState.id)?.order ??
              null
            : null;

        const updatedRuleBlock = createRuleBlock(
            ruleFormState.html,
            ruleFormState.id ?? undefined,
            normalizedOrder,
        );

        if (!updatedRuleBlock) {
            setErrorMessage(t("rules.admin.typeRuleToAdd"));
            return;
        }

        const confirmed = window.confirm(
            ruleFormState.id
                ? t("rules.admin.confirmUpdateRule")
                : t("rules.admin.confirmPublishSingleRule"),
        );

        if (!confirmed) {
            return;
        }

        try {
            setIsSaving(true);
            setErrorMessage(null);
            setSuccessMessage(null);

            const orderUpdates = resolveRuleOrderConflict(
                sectionRules,
                ruleFormState.id,
                normalizedOrder,
                previousOrder,
            );

            for (const orderUpdate of orderUpdates) {
                const conflictingRule = sectionRules.find(
                    (rule) => rule.id === orderUpdate.ruleId,
                );

                if (!conflictingRule) {
                    continue;
                }

                const swappedRuleBlock = createRuleBlock(
                    conflictingRule.html,
                    conflictingRule.id,
                    orderUpdate.newOrder,
                );

                if (!swappedRuleBlock) {
                    continue;
                }

                await rulesSectionService.updateRule(
                    ruleFormState.sectionId,
                    conflictingRule.id,
                    { ruleHtml: swappedRuleBlock },
                );
            }

            if (ruleFormState.id) {
                await rulesSectionService.updateRule(
                    ruleFormState.sectionId,
                    ruleFormState.id,
                    { ruleHtml: updatedRuleBlock },
                );
            } else {
                await rulesSectionService.addRule(ruleFormState.sectionId, {
                    ruleHtml: updatedRuleBlock,
                });
            }

            await loadSections();
            setRuleFormState(emptyRuleFormState());
            setIsRuleFormOpen(false);
            setSuccessMessage(
                ruleFormState.id
                    ? t("rules.admin.ruleUpdatedSuccessfully")
                    : t("rules.admin.rulePublishedSuccessfully"),
            );
        } catch (error) {
            setErrorMessage(
                error instanceof Error
                    ? error.message
                    : t("rules.admin.saveFailed"),
            );
        } finally {
            setIsSaving(false);
        }
    };

    const handleDeleteRule = async (rule: RuleItem): Promise<void> => {
        const confirmed = window.confirm(
            t("rules.admin.confirmDeleteRule", {
                ruleName: rule.text,
            }),
        );

        if (!confirmed) {
            return;
        }

        try {
            setIsSaving(true);
            setErrorMessage(null);
            setSuccessMessage(null);

            await rulesSectionService.deleteRule(rule.sectionId, rule.id);
            await loadSections();
            setSuccessMessage(t("rules.admin.ruleDeletedSuccessfully"));
        } catch (error) {
            setErrorMessage(
                error instanceof Error
                    ? error.message
                    : t("rules.admin.deleteRuleFailed"),
            );
        } finally {
            setIsSaving(false);
        }
    };

    const handleOpenCreateSection = (): void => {
        const mainTabs = getTopLevelSections(sections);

        setSectionCreateMode("main");
        setSectionFormState({
            ...emptySectionFormState(),
            sectionType: "Global",
            parentSectionId: null,
            sortOrder: mainTabs.length + 1,
        });
        setIsSectionFormOpen(true);
    };

    const handleOpenCreateSportSection = (): void => {
        const sportGroup = findSportGroupSection(sections);

        if (!sportGroup) {
            setErrorMessage(
                t(
                    "rules.admin.createSportGroupFirst",
                    "Luo ensin päävälilehti Lajikohtaiset säännöt, ennen kuin lisäät lajeja.",
                ),
            );
            return;
        }

        setSectionCreateMode("sport");
        setSectionFormState({
            ...emptySectionFormState(),
            sectionType: "Sport",
            parentSectionId: sportGroup.id,
            sortOrder:
                getChildSections(sections, sportGroup.id).length + 1,
        });
        setErrorMessage(null);
        setIsSectionFormOpen(true);
    };

    const handleStartEditSection = (section: RulesSection): void => {
        setSectionFormState({
            id: section.id,
            title: section.title,
            sortOrder: section.sortOrder,
            sectionType: section.sectionType,
            parentSectionId: section.parentSectionId,
        });
        setIsSectionFormOpen(true);
    };

    const handleSaveSection = async (): Promise<void> => {
        if (!sectionFormState.title.trim()) {
            setErrorMessage(t("rules.admin.sectionTitleRequired"));
            return;
        }

        if (
            sectionFormState.sectionType === "Sport" &&
            !sectionFormState.parentSectionId
        ) {
            setErrorMessage(
                t(
                    "rules.admin.createSportGroupFirst",
                    "Luo ensin päävälilehti Lajikohtaiset säännöt, ennen kuin lisäät lajeja.",
                ),
            );
            return;
        }

        try {
            setIsSaving(true);
            setErrorMessage(null);

            if (sectionFormState.id) {
                await rulesSectionService.updateSection(sectionFormState.id, {
                    title: sectionFormState.title.trim(),
                    sortOrder: sectionFormState.sortOrder,
                    sectionType: sectionFormState.sectionType,
                    parentSectionId: sectionFormState.parentSectionId,
                });
            } else {
                await rulesSectionService.createSection({
                    title: sectionFormState.title.trim(),
                    sortOrder: sectionFormState.sortOrder,
                    sectionType: sectionFormState.sectionType,
                    parentSectionId: sectionFormState.parentSectionId,
                });
            }

            await loadSections();
            setSectionFormState(emptySectionFormState());
            setIsSectionFormOpen(false);
            setSuccessMessage(t("rules.admin.sectionSavedSuccessfully"));
        } catch (error) {
            const rawMessage =
                error instanceof Error
                    ? error.message
                    : t("rules.admin.sectionSaveFailed");

            setErrorMessage(formatSectionDeleteError(rawMessage, t));
        } finally {
            setIsSaving(false);
        }
    };

    const handleDeleteSection = async (section: RulesSection): Promise<void> => {
        const confirmed = window.confirm(
            t("rules.admin.confirmDeleteSection", { title: section.title }),
        );

        if (!confirmed) {
            return;
        }

        try {
            setIsSaving(true);
            setErrorMessage(null);
            await rulesSectionService.deleteSection(section.id);
            await loadSections();
            setSuccessMessage(t("rules.admin.sectionDeletedSuccessfully"));
        } catch (error) {
            const rawMessage =
                error instanceof Error
                    ? error.message
                    : t("rules.admin.sectionDeleteFailed");

            setErrorMessage(formatSectionDeleteError(rawMessage, t));
        } finally {
            setIsSaving(false);
        }
    };

    return (
        <PageTemplate title={t("rules.admin.pageTitle")}>
            <div className="rules-management-page">
                <div className="rules-management-page__alerts-overlay">
                    {successMessage && (
                        <div className="rules-management-page__alert rules-management-page__alert--success">
                            {successMessage}
                        </div>
                    )}

                    {errorMessage && (
                        <div className="rules-management-page__alert rules-management-page__alert--error">
                            {errorMessage}
                        </div>
                    )}
                </div>

                {!isRuleFormOpen && !isSectionFormOpen && (
                    <>
                        <div className="rules-management-page__topbar">
                            <h2 className="rules-management-page__page-title">
                                {t("rules.admin.subtitle")}
                            </h2>

                            <div className="rules-management-page__topbar-actions">
                                <Button
                                    rounded="pill"
                                    variant="secondary"
                                    onClick={() =>
                                        setActivePanel(
                                            activePanel === "rules"
                                                ? "sections"
                                                : "rules",
                                        )
                                    }
                                >
                                    {activePanel === "rules"
                                        ? t(
                                              "rules.admin.manageSections",
                                              "Hallitse osioita",
                                          )
                                        : t(
                                              "rules.admin.manageRules",
                                              "Hallitse sääntöjä",
                                          )}
                                </Button>

                                {activePanel === "rules" ? (
                                    <Button
                                        iconLeft={AddIcon}
                                        rounded="pill"
                                        onClick={handleOpenCreateRule}
                                        disabled={isSaving}
                                    >
                                        {t("rules.admin.addRule")}
                                    </Button>
                                ) : (
                                    <>
                                        <Button
                                            rounded="pill"
                                            variant="secondary"
                                            onClick={handleOpenCreateSection}
                                            disabled={isSaving}
                                        >
                                            {t(
                                                "rules.admin.addMainSection",
                                                "Lisää päävälilehti",
                                            )}
                                        </Button>
                                        <Button
                                            iconLeft={AddIcon}
                                            rounded="pill"
                                            onClick={handleOpenCreateSportSection}
                                            disabled={isSaving}
                                        >
                                            {t(
                                                "rules.admin.addSportSection",
                                                "Lisää laji",
                                            )}
                                        </Button>
                                    </>
                                )}
                            </div>
                        </div>

                        {activePanel === "rules" ? (
                            <>
                                <div className="rules-management-page__filter-card">
                                    <div>
                                        <label className="rules-management-page__filter-label">
                                            {t("rules.admin.section", "Osio")}
                                        </label>

                                        <SectionSelect
                                            sections={sections}
                                            value={filterSectionId}
                                            onChange={setFilterSectionId}
                                            includeAll
                                        />
                                    </div>
                                </div>

                                <div className="rules-management-page__content">
                                    <RulesList
                                        title={t("rules.admin.publishedRules")}
                                        rules={filteredRules}
                                        sections={sections}
                                        wordLimit={10}
                                        emptyMessage={t(
                                            "rules.admin.noPublishedRules",
                                        )}
                                        onEditRule={handleStartEditRule}
                                        onDeleteRule={handleDeleteRule}
                                        isSaving={isSaving}
                                        isLoading={isLoading}
                                    />
                                </div>
                            </>
                        ) : (
                            <RulesSectionList
                                sections={sections}
                                isSaving={isSaving}
                                onEdit={handleStartEditSection}
                                onDelete={handleDeleteSection}
                            />
                        )}
                    </>
                )}

                {isRuleFormOpen && (
                    <RuleForm
                        isEditMode={Boolean(ruleFormState.id)}
                        sectionId={ruleFormState.sectionId}
                        sections={sections}
                        contentHtml={ruleFormState.html}
                        order={ruleFormState.order}
                        isSaving={isSaving}
                        onBack={() => {
                            setRuleFormState(emptyRuleFormState());
                            setIsRuleFormOpen(false);
                        }}
                        onSectionChange={(value) => {
                            const sectionRules = allRules.filter(
                                (rule) => rule.sectionId === value,
                            );

                            setRuleFormState((prev) => ({
                                ...prev,
                                sectionId: value,
                                order: prev.id
                                    ? prev.order
                                    : getNextRuleOrder(sectionRules),
                            }));
                        }}
                        onOrderChange={(value) =>
                            setRuleFormState((prev) => ({
                                ...prev,
                                order: Math.max(1, value),
                            }))
                        }
                        onContentChange={(value) =>
                            setRuleFormState((prev) => ({
                                ...prev,
                                html: value,
                            }))
                        }
                        onCancel={() => {
                            setRuleFormState(emptyRuleFormState());
                            setIsRuleFormOpen(false);
                        }}
                        onSave={handleSaveRule}
                    />
                )}

                {isSectionFormOpen && (
                    <RulesSectionForm
                        sections={sections}
                        formState={sectionFormState}
                        createMode={
                            sectionFormState.id ? undefined : sectionCreateMode
                        }
                        isSaving={isSaving}
                        onBack={() => {
                            setSectionFormState(emptySectionFormState());
                            setIsSectionFormOpen(false);
                        }}
                        onChange={(updates) =>
                            setSectionFormState((prev) => ({
                                ...prev,
                                ...updates,
                            }))
                        }
                        onSave={handleSaveSection}
                    />
                )}
            </div>
        </PageTemplate>
    );
}
