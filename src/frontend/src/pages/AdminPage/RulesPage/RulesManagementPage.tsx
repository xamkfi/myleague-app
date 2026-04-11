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
import RuleForm from "./components/RuleForm";
import RulesList from "./components/RulesList";
import CategorySelect from "./components/CategorySelect";
import { createRuleBlock, parseRulesFromHtml } from "../../../utils/helpers";

const RULES_SLUG = "saannot";

export default function RulesManagementPage() {
    const { t } = useTranslation();

    const [title, setTitle] = useState<string>("Säännöt");
    const [draftRuleHtml, setDraftRuleHtml] = useState<string>("");
    const [draftRuleCategory, setDraftRuleCategory] = useState<string>("");
    const [previewHtml, setPreviewHtml] = useState<string>("");
    const [isCreateLayerOpen, setIsCreateLayerOpen] = useState<boolean>(false);
    const [searchTerm, setSearchTerm] = useState<string>("");
    const [filterCategory, setFilterCategory] = useState<string>("all");
    const [editingRuleId, setEditingRuleId] = useState<string | null>(null);
    const [editingRuleHtml, setEditingRuleHtml] = useState<string>("");
    const [savedContent, setSavedContent] =
        useState<PageContentResponse | null>(null);
    const [isLoading, setIsLoading] = useState<boolean>(true);
    const [isSaving, setIsSaving] = useState<boolean>(false);
    const [successMessage, setSuccessMessage] = useState<string | null>(null);
    const [errorMessage, setErrorMessage] = useState<string | null>(null);
    const [isPreviewExpanded, setIsPreviewExpanded] = useState<boolean>(true);
    const [isPublishedExpanded, setIsPublishedExpanded] =
        useState<boolean>(true);

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

                setTitle(response.title || t("rules.defaultTitle"));
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
                    setTitle(t("rules.defaultTitle"));
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
        setDraftRuleHtml("");
        setDraftRuleCategory("");
        setEditingRuleId(null);
        setEditingRuleHtml("");
        setErrorMessage(null);
        setSuccessMessage(null);
        setIsCreateLayerOpen(true);
    };

    const handleCloseCreateLayer = (): void => {
        setDraftRuleHtml("");
        setDraftRuleCategory("");
        setEditingRuleId(null);
        setEditingRuleHtml("");
        setErrorMessage("");
        setIsCreateLayerOpen(false);
    };

    const handleAddRuleToPreview = (): void => {
        setSuccessMessage(null);
        setErrorMessage(null);

        const newRuleBlock = createRuleBlock(draftRuleHtml, draftRuleCategory);

        if (!newRuleBlock) {
            setErrorMessage(t("rules.admin.typeRuleToAdd"));
            return;
        }

        const mergedPreviewHtml = previewHtml.trim()
            ? `${previewHtml}\n${newRuleBlock}`
            : newRuleBlock;

        setPreviewHtml(mergedPreviewHtml);
        setDraftRuleHtml("");
        setDraftRuleCategory("general");
        setIsCreateLayerOpen(false);
    };

    const previewRules = useMemo(() => {
        return parseRulesFromHtml(previewHtml);
    }, [previewHtml]);

    const publishedRules = useMemo(() => {
        return parseRulesFromHtml(savedContent?.contentHtml || "").reverse();
    }, [savedContent]);

    const filteredPreviewRules = useMemo(() => {
        return previewRules.filter((rule) => {
            const matchesSearch = rule.text
                .toLowerCase()
                .includes(searchTerm.trim().toLowerCase());
            const matchesCategory =
                filterCategory === "all" || rule.category === filterCategory;

            return matchesSearch && matchesCategory;
        });
    }, [previewRules, searchTerm, filterCategory]);

    const filteredPublishedRules = useMemo(() => {
        return publishedRules.filter((rule) => {
            const matchesSearch = rule.text
                .toLowerCase()
                .includes(searchTerm.trim().toLowerCase());
            const matchesCategory =
                filterCategory === "all" || rule.category === filterCategory;

            return matchesSearch && matchesCategory;
        });
    }, [publishedRules, searchTerm, filterCategory]);

    const handlePublishRules = async (): Promise<void> => {
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
                    // Replace old version in published with updated version
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
                    // New rule, append
                    appendHtml += `\n${createRuleBlock(previewRule.html, previewRule.category, previewRule.id)}`;
                }
            }

            const mergedHtml = appendHtml
                ? `${baseHtml}${appendHtml}`
                : baseHtml;

            const request: PageContentUpdateRequest = {
                title: title.trim() || t("rules.defaultTitle"),
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
        setEditingRuleId(rule.id);
        setEditingRuleHtml(rule.html);
        setDraftRuleCategory(rule.category);
        setIsCreateLayerOpen(true);
    };

    const handleUpdateRule = (): void => {
        if (!editingRuleId) {
            return;
        }

        const updatedRuleBlock = createRuleBlock(
            editingRuleHtml,
            draftRuleCategory,
            editingRuleId,
        );

        if (!updatedRuleBlock) {
            setErrorMessage(t("rules.admin.typeRuleToAdd"));
            return;
        }

        const replaceRuleInHtml = (html: string): string => {
            const wrapper = document.createElement("div");
            wrapper.innerHTML = html;

            const target = wrapper.querySelector(
                `.rules-item[data-rule-id="${editingRuleId}"]`,
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
            (rule) => rule.id === editingRuleId,
        );
        const existsInPublished = publishedRules.some(
            (rule) => rule.id === editingRuleId,
        );

        if (existsInPreview) {
            setPreviewHtml(replaceRuleInHtml(previewHtml));
        } else if (existsInPublished) {
            const publishedRule = publishedRules.find(
                (rule) => rule.id === editingRuleId,
            );

            if (publishedRule) {
                const movedToPreviewHtml = previewHtml.trim()
                    ? `${previewHtml}\n${updatedRuleBlock}`
                    : updatedRuleBlock;

                const wrapper = document.createElement("div");
                wrapper.innerHTML = savedContent?.contentHtml || "";

                const target = wrapper.querySelector(
                    `.rules-item[data-rule-id="${editingRuleId}"]`,
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

        setEditingRuleId(null);
        setEditingRuleHtml("");
        setDraftRuleCategory("general");
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
        <div className="rules-management-page-wrapper">
            <PageTemplate title={t("rules.admin.pageTitle")}>
                <div className="rules-management-page">
                    {!isCreateLayerOpen && (
                        <>
                            <div className="rules-management-page__toolbar">
                                <div className="rules-management-page__toolbar-left">
                                    <div className="rules-management-page__search">
                                        <input
                                            type="text"
                                            value={searchTerm}
                                            onChange={(event) =>
                                                setSearchTerm(
                                                    event.target.value,
                                                )
                                            }
                                            placeholder={t(
                                                "rules.admin.searchPlaceholder",
                                            )}
                                            className="rules-management-page__search-input"
                                        />
                                    </div>

                                    <div className="rules-management-page__filter">
                                        <CategorySelect
                                            value={filterCategory}
                                            onChange={setFilterCategory}
                                            includeAll
                                        />
                                    </div>
                                </div>

                                <div className="rules-management-page__toolbar-actions">
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

                            <div className="rules-management-page__meta">
                                <p>
                                    <strong>
                                        {t("rules.admin.lastUpdate")}:{" "}
                                    </strong>
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

                            <div className="rules-management-page__content">
                                <RulesList
                                    title={t("rules.admin.preview")}
                                    rules={filteredPreviewRules}
                                    emptyMessage={t(
                                        "rules.admin.noPreviewRules",
                                    )}
                                    isExpanded={isPreviewExpanded}
                                    onToggleExpanded={() =>
                                        setIsPreviewExpanded((prev) => !prev)
                                    }
                                    onEditRule={handleStartEditRule}
                                    onDeleteRule={handleDeleteRule}
                                    isSaving={isSaving}
                                    showCancel={true}
                                    isLoading={false}
                                />

                                <RulesList
                                    title={t("rules.admin.publishedRules")}
                                    rules={filteredPublishedRules}
                                    emptyMessage={t(
                                        "rules.admin.noPublishedRules",
                                    )}
                                    isExpanded={isPublishedExpanded}
                                    onToggleExpanded={() =>
                                        setIsPublishedExpanded((prev) => !prev)
                                    }
                                    onEditRule={handleStartEditRule}
                                    onDeleteRule={handleDeleteRule}
                                    isSaving={isSaving}
                                    isLoading={isLoading}
                                />
                            </div>
                        </>
                    )}

                    {isCreateLayerOpen && (
                        <RuleForm
                            isEditMode={!!editingRuleId}
                            category={draftRuleCategory}
                            contentHtml={
                                editingRuleId ? editingRuleHtml : draftRuleHtml
                            }
                            isSaving={isSaving}
                            onBack={handleCloseCreateLayer}
                            onCategoryChange={setDraftRuleCategory}
                            onContentChange={
                                editingRuleId
                                    ? setEditingRuleHtml
                                    : setDraftRuleHtml
                            }
                            onCancel={handleCloseCreateLayer}
                            onSave={
                                editingRuleId
                                    ? handleUpdateRule
                                    : handleAddRuleToPreview
                            }
                        />
                    )}
                </div>
            </PageTemplate>
        </div>
    );
}
