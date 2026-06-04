import { useEffect, useMemo, useState } from "react";
import PageTemplate from "../../../components/PageTemplate/AdminPageTemplate";
import Button from "../../../components/Button/Button";
import AddIcon from "../../../assets/basicIcons/add.svg";
import PublishIcon from "../../../assets/adminIcons/PublishIcon.svg";
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
import RulesSearchInput from "./components/RulesSearchInput";
import { useDebouncedValue } from "../../../hooks/useDebouncedValue";

type RuleFormState = Pick<RuleItem, "html" | "category"> & {
    id: string | null;
};

const RULES_SLUG = "saannot";

export default function RulesManagementPage() {
    const { t } = useTranslation();

    const [previewHtml, setPreviewHtml] = useState<string>("");
    const [isCreateLayerOpen, setIsCreateLayerOpen] = useState<boolean>(false);
    const [filterCategory, setFilterCategory] = useState<string>("all");
    const [savedContent, setSavedContent] =
        useState<PageContentResponse | null>(null);
    const [isLoading, setIsLoading] = useState<boolean>(true);
    const [isSaving, setIsSaving] = useState<boolean>(false);
    const [successMessage, setSuccessMessage] = useState<string | null>(null);
    const [errorMessage, setErrorMessage] = useState<string | null>(null);
    const [searchTerm, setSearchTerm] = useState("");
    const emptyRuleFormState: RuleFormState = {
        id: null,
        html: "",
        category: "general",
    };
    const [ruleFormState, setRuleFormState] =
        useState<RuleFormState>(emptyRuleFormState);

    const debouncedSearchTerm = useDebouncedValue(searchTerm, 300);

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
    }, []);

    useEffect(() => {
        if (!successMessage) {
            return;
        }

        const timeout = setTimeout(() => {
            setSuccessMessage(null);
        }, 6000);

        return () => clearTimeout(timeout);
    }, [successMessage]);

    const handleOpenCreateLayer = (): void => {
        setRuleFormState(emptyRuleFormState);
        setErrorMessage(null);
        setSuccessMessage(null);
        setIsCreateLayerOpen(true);
    };

    const handleCloseCreateLayer = (): void => {
        setRuleFormState(emptyRuleFormState);
        setErrorMessage("");
        setIsCreateLayerOpen(false);
    };

    const handleAddRuleToPreview = (): void => {
        setSuccessMessage(null);
        setErrorMessage(null);

        const newRuleBlock = createRuleBlock(
            ruleFormState.html,
            ruleFormState.category,
        );

        if (!newRuleBlock) {
            setErrorMessage(t("rules.admin.typeRuleToAdd"));
            return;
        }

        const mergedPreviewHtml = previewHtml.trim()
            ? `${previewHtml}\n${newRuleBlock}`
            : newRuleBlock;

        setPreviewHtml(mergedPreviewHtml);
        setRuleFormState({
            ...emptyRuleFormState,
            category: "general",
        });
        setIsCreateLayerOpen(false);
    };

    const previewRules = useMemo(() => {
        return parseRulesFromHtml(previewHtml);
    }, [previewHtml]);

    const publishedRules = useMemo(() => {
        return parseRulesFromHtml(savedContent?.contentHtml || "").reverse();
    }, [savedContent]);

    const normalizedSearchTerm =
        debouncedSearchTerm.trim().length >= 2
            ? debouncedSearchTerm.toLowerCase().trim()
            : "";

    const filteredPreviewRules = useMemo(() => {
        return previewRules.filter((rule) => {
            const matchesSearch = rule.text
                .toLowerCase()
                .includes(normalizedSearchTerm);

            const matchesCategory =
                filterCategory === "all" || rule.category === filterCategory;
            return matchesSearch && matchesCategory;
        });
    }, [previewRules, normalizedSearchTerm, filterCategory]);

    const filteredPublishedRules = useMemo(() => {
        return publishedRules.filter((rule) => {
            const matchesSearch = rule.text
                .toLowerCase()
                .includes(normalizedSearchTerm);

            const matchesCategory =
                filterCategory === "all" || rule.category === filterCategory;
            return matchesSearch && matchesCategory;
        });
    }, [publishedRules, normalizedSearchTerm, filterCategory]);

    const handlePublishRules = async (): Promise<void> => {
        const confirmed = window.confirm(t("rules.admin.confirmPublish"));
        if (!confirmed) return;

        try {
            setIsSaving(true);
            setSuccessMessage(null);
            setErrorMessage(null);

            if (!previewHtml.trim()) {
                setErrorMessage(t("rules.admin.noPreviewRules"));
                return;
            }

            let baseHtml = savedContent?.contentHtml || "";
            let appendHtml = "";

            for (const previewRule of previewRules) {
                const existsInPublished = publishedRules.some(
                    (r) => r.id === previewRule.id,
                );

                if (existsInPublished) {
                    const wrapper = document.createElement("div");
                    wrapper.innerHTML = baseHtml;
                    const target = wrapper.querySelector(
                        `.rules-item[data-rule-id="${previewRule.id}"]`,
                    );
                    if (target) {
                        const newWrapper = document.createElement("div");
                        newWrapper.innerHTML = createRuleBlock(
                            previewRule.html,
                            previewRule.category,
                            previewRule.id,
                        );
                        const newEl = newWrapper.firstElementChild;
                        if (newEl) target.replaceWith(newEl);
                    }
                    baseHtml = wrapper.innerHTML.trim();
                } else {
                    appendHtml += `\n${createRuleBlock(previewRule.html, previewRule.category, previewRule.id)}`;
                }
            }

            const mergedHtml = appendHtml
                ? `${baseHtml}${appendHtml}`
                : baseHtml;

            const request: PageContentUpdateRequest = {
                title: pageTitle,
                contentHtml: mergedHtml,
            };

            const response = await pageContentService.updatePageContent(
                RULES_SLUG,
                request,
            );

            setSavedContent(response);
            setPreviewHtml("");
            setSuccessMessage(t("rules.admin.rulesPublishedSuccessfully"));
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

    const handleUpdateRule = (): void => {
        if (!ruleFormState.id) {
            return;
        }

        const updatedRuleBlock = createRuleBlock(
            ruleFormState.html,
            ruleFormState.category,
            ruleFormState.id,
        );

        if (!updatedRuleBlock) {
            setErrorMessage(t("rules.admin.typeRuleToAdd"));
            return;
        }

        const replaceRuleInHtml = (html: string): string => {
            const wrapper = document.createElement("div");
            wrapper.innerHTML = html;

            const target = wrapper.querySelector(
                `.rules-item[data-rule-id="${ruleFormState.id}"]`,
            );

            if (target) {
                const newWrapper = document.createElement("div");
                newWrapper.innerHTML = updatedRuleBlock;
                const newRuleElement = newWrapper.firstElementChild;

                if (newRuleElement) {
                    target.replaceWith(newRuleElement);
                }
            }

            return wrapper.innerHTML.trim();
        };

        const existsInPreview = previewRules.some(
            (rule) => rule.id === ruleFormState.id,
        );
        const existsInPublished = publishedRules.some(
            (rule) => rule.id === ruleFormState.id,
        );

        if (existsInPreview) {
            setPreviewHtml(replaceRuleInHtml(previewHtml));
        } else if (existsInPublished) {
            const publishedRule = publishedRules.find(
                (rule) => rule.id === ruleFormState.id,
            );

            if (publishedRule) {
                const movedToPreviewHtml = previewHtml.trim()
                    ? `${previewHtml}\n${updatedRuleBlock}`
                    : updatedRuleBlock;

                const wrapper = document.createElement("div");
                wrapper.innerHTML = savedContent?.contentHtml || "";

                const target = wrapper.querySelector(
                    `.rules-item[data-rule-id="${ruleFormState.id}"]`,
                );

                if (target) {
                    target.remove();
                }

                setSavedContent((prev) =>
                    prev
                        ? {
                              ...prev,
                              contentHtml: wrapper.innerHTML.trim(),
                          }
                        : prev,
                );

                setPreviewHtml(movedToPreviewHtml);
            }
        }

        setRuleFormState({
            ...emptyRuleFormState,
            category: "general",
        });
        setIsCreateLayerOpen(false);
    };

    const handleDeleteRule = async (rule: RuleItem): Promise<void> => {
        const existsInPreview = previewRules.some(
            (previewRule) => previewRule.id === rule.id,
        );

        if (existsInPreview) {
            const confirmed = window.confirm(
                `Are you sure you want to cancel rule: "${rule.text}"?`,
            );

            if (!confirmed) return;

            const wrapper = document.createElement("div");
            wrapper.innerHTML = previewHtml;
            const target = wrapper.querySelector(
                `.rules-item[data-rule-id="${rule.id}"]`,
            );
            if (target) target.remove();
            setPreviewHtml(wrapper.innerHTML.trim());
            return;
        }

        const confirmed = window.confirm(
            `Are you sure you want to delete rule: "${rule.text}"?`,
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
                                {previewRules.length > 0 && (
                                    <Button
                                        iconLeft={PublishIcon}
                                        rounded="pill"
                                        onClick={handlePublishRules}
                                        disabled={isSaving}
                                    >
                                        {isSaving
                                            ? t("rules.admin.publishing")
                                            : t("rules.admin.publish")}
                                    </Button>
                                )}

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

                        <div className="rules-management-page__toolbar-left">
                            <RulesSearchInput
                                value={searchTerm}
                                onChange={setSearchTerm}
                                placeholder={t("rules.admin.searchPlaceholder")}
                            />

                            <CategorySelect
                                value={filterCategory}
                                onChange={setFilterCategory}
                                includeAll
                            />
                        </div>

                        <div className="rules-management-page__content">
                            <div className="rules-management-page__column">
                                <RulesList
                                    title={t("rules.admin.preview")}
                                    rules={filteredPreviewRules}
                                    emptyMessage={t(
                                        "rules.admin.noPreviewRules",
                                    )}
                                    onEditRule={handleStartEditRule}
                                    onDeleteRule={handleDeleteRule}
                                    isSaving={isSaving}
                                    showCancel={true}
                                    isLoading={false}
                                />
                            </div>

                            <div className="rules-management-page__column">
                                <RulesList
                                    title={t("rules.admin.publishedRules")}
                                    rules={filteredPublishedRules}
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
                        onSave={
                            ruleFormState.id
                                ? handleUpdateRule
                                : handleAddRuleToPreview
                        }
                    />
                )}
            </div>
        </PageTemplate>
    );
}
