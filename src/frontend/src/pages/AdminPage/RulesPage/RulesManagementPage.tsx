import { useEffect, useMemo, useState } from "react";
import PageTemplate from "../../../components/PageTemplate/AdminPageTemplate";
import Button from "../../../components/Button/Button";
import AddIcon from "../../../assets/basicIcons/add.svg";
import "./RulesManagementPage.scss";
import type {
    PageContentResponse,
    PageContentUpdateRequest,
    RuleItem,
} from "../../../types/admin/ruleTypes";
import { pageContentService } from "../../../services/pageContentService";
import { useTranslation } from "react-i18next";
import RuleForm from "./components/RulesForm";
import RulesList from "./components/RulesList";
import CategorySelect from "./components/CategorySelect";
import { createRuleBlock, parseRulesFromHtml } from "../../../utils/helpers";

type RuleFormState = Pick<RuleItem, "html" | "category"> & {
    id: string | null;
};

const RULES_SLUG = "saannot";

const emptyRuleFormState: RuleFormState = {
    id: null,
    html: "",
    category: "general",
};

export default function RulesManagementPage() {
    const { t } = useTranslation();

    const [isCreateLayerOpen, setIsCreateLayerOpen] = useState<boolean>(false);
    const [filterCategory, setFilterCategory] = useState<string>("all");
    const [savedContent, setSavedContent] =
        useState<PageContentResponse | null>(null);
    const [isLoading, setIsLoading] = useState<boolean>(true);
    const [isSaving, setIsSaving] = useState<boolean>(false);
    const [successMessage, setSuccessMessage] = useState<string | null>(null);
    const [errorMessage, setErrorMessage] = useState<string | null>(null);
    const [ruleFormState, setRuleFormState] =
        useState<RuleFormState>(emptyRuleFormState);

    const pageTitle = savedContent?.title?.trim() || t("rules.defaultTitle");

    useEffect(() => {
        let isMounted = true;

        const loadRulesContent = async (): Promise<void> => {
            try {
                setIsLoading(true);
                setErrorMessage(null);

                const response =
                    await pageContentService.getPageContent(RULES_SLUG);

                if (!isMounted) {
                    return;
                }

                setSavedContent(response);
            } catch (error) {
                if (!isMounted) {
                    return;
                }

                const message =
                    error instanceof Error
                        ? error.message
                        : t("rules.admin.loadFailed");

                if (message.includes("not found")) {
                    setSavedContent(null);
                } else {
                    setErrorMessage(message);
                }
            } finally {
                if (isMounted) {
                    setIsLoading(false);
                }
            }
        };

        loadRulesContent();

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

    const publishedRules = useMemo(() => {
        return parseRulesFromHtml(savedContent?.contentHtml || "").reverse();
    }, [savedContent]);

    const filteredPublishedRules = useMemo(() => {
        return publishedRules.filter((rule) => {
            return filterCategory === "all" || rule.category === filterCategory;
        });
    }, [publishedRules, filterCategory]);

    const handleOpenCreateLayer = (): void => {
        setRuleFormState(emptyRuleFormState);
        setErrorMessage(null);
        setSuccessMessage(null);
        setIsCreateLayerOpen(true);
    };

    const handleCloseCreateLayer = (): void => {
        setRuleFormState(emptyRuleFormState);
        setErrorMessage(null);
        setIsCreateLayerOpen(false);
    };

    const handleStartEditRule = (rule: RuleItem): void => {
        setErrorMessage(null);
        setSuccessMessage(null);

        setRuleFormState({
            id: rule.id,
            html: rule.html,
            category: rule.category,
        });

        setIsCreateLayerOpen(true);
    };

    const handleSaveRule = async (): Promise<void> => {
        const updatedRuleBlock = createRuleBlock(
            ruleFormState.html,
            ruleFormState.category,
            ruleFormState.id ?? undefined,
        );

        if (!updatedRuleBlock) {
            setErrorMessage(t("rules.admin.typeRuleToAdd"));
            return;
        }

        const confirmMessage = ruleFormState.id
            ? t(
                  "rules.admin.confirmUpdateRule",
                  "Haluatko varmasti päivittää tämän säännön?",
              )
            : t(
                  "rules.admin.confirmPublishSingleRule",
                  "Haluatko varmasti julkaista tämän säännön?",
              );

        const confirmed = window.confirm(confirmMessage);
        if (!confirmed) {
            return;
        }

        try {
            setIsSaving(true);
            setSuccessMessage(null);
            setErrorMessage(null);

            const wrapper = document.createElement("div");
            wrapper.innerHTML = savedContent?.contentHtml || "";

            if (ruleFormState.id) {
                const target = wrapper.querySelector(
                    `.rules-item[data-rule-id="${ruleFormState.id}"]`,
                );

                const newWrapper = document.createElement("div");
                newWrapper.innerHTML = updatedRuleBlock;
                const newRuleElement = newWrapper.firstElementChild;

                if (target && newRuleElement) {
                    target.replaceWith(newRuleElement);
                } else if (newRuleElement) {
                    wrapper.appendChild(newRuleElement);
                }
            } else {
                const newWrapper = document.createElement("div");
                newWrapper.innerHTML = updatedRuleBlock;
                const newRuleElement = newWrapper.firstElementChild;

                if (newRuleElement) {
                    wrapper.appendChild(newRuleElement);
                }
            }

            const request: PageContentUpdateRequest = {
                title: pageTitle,
                contentHtml: wrapper.innerHTML.trim(),
            };

            const response = await pageContentService.updatePageContent(
                RULES_SLUG,
                request,
            );

            setSavedContent(response);
            setRuleFormState(emptyRuleFormState);
            setIsCreateLayerOpen(false);
            setSuccessMessage(
                ruleFormState.id
                    ? t(
                          "rules.admin.ruleUpdatedSuccessfully",
                          "Sääntö päivitettiin onnistuneesti.",
                      )
                    : t(
                          "rules.admin.rulePublishedSuccessfully",
                          "Sääntö julkaistiin onnistuneesti.",
                      ),
            );
        } catch (error) {
            const message =
                error instanceof Error
                    ? error.message
                    : t("rules.admin.saveFailed");
            setErrorMessage(message);
        } finally {
            setIsSaving(false);
        }
    };

    const handleDeleteRule = async (rule: RuleItem): Promise<void> => {
        const confirmed = window.confirm(
            t(
                "rules.admin.confirmDeleteRule",
                `Haluatko varmasti poistaa säännön: "${rule.text}"?`,
            ),
        );

        if (!confirmed) return;

        try {
            setIsSaving(true);
            setErrorMessage(null);
            setSuccessMessage(null);

            const response = await pageContentService.deletePageRule(
                RULES_SLUG,
                rule.id,
            );

            setSavedContent(response);
            setSuccessMessage(t("rules.admin.ruleDeletedSuccessfully"));
        } catch (error) {
            const message =
                error instanceof Error
                    ? error.message
                    : t("rules.admin.deleteRuleFailed");

            setErrorMessage(message);
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

                {!isCreateLayerOpen && (
                    <>
                        <div className="rules-management-page__topbar">
                            <h2 className="rules-management-page__page-title">
                                {t("rules.admin.subtitle")}
                            </h2>

                            <div className="rules-management-page__topbar-actions">
                                <Button
                                    iconLeft={AddIcon}
                                    rounded="pill"
                                    onClick={handleOpenCreateLayer}
                                    disabled={isSaving}
                                >
                                    {t("rules.admin.addRule")}
                                </Button>
                            </div>
                        </div>

                        <div className="rules-management-page__meta">
                            <p>
                                <strong>{t("rules.admin.lastUpdate")}: </strong>
                                {savedContent?.updatedAt
                                    ? new Date(
                                          savedContent.updatedAt,
                                      ).toLocaleString()
                                    : t("rules.admin.noRulesAddedYet")}
                            </p>
                            <p>
                                <strong>{t("rules.admin.editor")}: </strong>
                                {savedContent?.lastModifiedBy ??
                                    t("rules.admin.unknown")}
                            </p>
                        </div>

                        <div className="rules-management-page__filter-card">
                            <div>
                                <label className="rules-management-page__filter-label">
                                    {t("rules.admin.category", "Kategoria")}
                                </label>

                                <CategorySelect
                                    value={filterCategory}
                                    onChange={setFilterCategory}
                                    includeAll
                                />
                            </div>
                        </div>

                        <div className="rules-management-page__content">
                            <div className="rules-management-page__column">
                                <RulesList
                                    title={t("rules.admin.publishedRules")}
                                    rules={filteredPublishedRules}
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
                        </div>
                    </>
                )}

                {isCreateLayerOpen && (
                    <RuleForm
                        isEditMode={Boolean(ruleFormState.id)}
                        category={ruleFormState.category}
                        contentHtml={ruleFormState.html}
                        isSaving={isSaving}
                        saveLabel={
                            ruleFormState.id
                                ? t("rules.admin.updateRule", "Päivitä sääntö")
                                : t("rules.admin.publishRule", "Julkaise sääntö")
                        }
                        onBack={handleCloseCreateLayer}
                        onCategoryChange={(value) =>
                            setRuleFormState((prev) => ({
                                ...prev,
                                category: value,
                            }))
                        }
                        onContentChange={(value) =>
                            setRuleFormState((prev) => ({
                                ...prev,
                                html: value,
                            }))
                        }
                        onCancel={handleCloseCreateLayer}
                        onSave={handleSaveRule}
                    />
                )}
            </div>
        </PageTemplate>
    );
}
