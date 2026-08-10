import { useTranslation } from "react-i18next";
import type { RulesSection, RulesSectionType } from "../../../../types/admin/ruleTypes";
import { parseRulesFromHtml } from "../../../../utils/helpers";
import {
    findSportGroupSection,
    getChildSections,
    getTopLevelSections,
} from "../../../../utils/rulesSectionUtils";
import "./RulesSectionList.scss";

interface RulesSectionListProps {
    sections: RulesSection[];
    isSaving: boolean;
    onEdit: (section: RulesSection) => void;
    onDelete: (section: RulesSection) => void;
}

export default function RulesSectionList({
    sections,
    isSaving,
    onEdit,
    onDelete,
}: Readonly<RulesSectionListProps>) {
    const { t } = useTranslation();
    const topLevelSections = getTopLevelSections(sections);
    const sportGroup = findSportGroupSection(sections);
    const sportSections = sportGroup
        ? getChildSections(sections, sportGroup.id)
        : [];

    const renderTypeLabel = (sectionType: RulesSectionType): string => {
        return t(`rules.admin.sectionTypes.${sectionType}`, sectionType);
    };

    const getRuleCount = (section: RulesSection): number => {
        return parseRulesFromHtml(section.contentHtml, section.id).length;
    };

    return (
        <div className="rules-section-list">
            <h3>{t("rules.admin.sectionsTitle", "Sääntöosiot")}</h3>

            <div className="rules-section-list__group">
                <h4>{t("rules.admin.topLevelSections", "Päävälilehdet")}</h4>

                <ul>
                    {topLevelSections.map((section) => (
                        <li key={section.id} className="rules-section-list__item">
                            <div>
                                <strong>{section.title}</strong>
                                <span>
                                    {renderTypeLabel(section.sectionType)} ·{" "}
                                    {t("rules.admin.sortOrder", "Järjestys")}{" "}
                                    {section.sortOrder}
                                </span>
                            </div>

                            <div className="rules-section-list__actions">
                                <button
                                    type="button"
                                    onClick={() => onEdit(section)}
                                    disabled={isSaving}
                                >
                                    {t("common.edit", "Muokkaa")}
                                </button>
                                <button
                                    type="button"
                                    className="rules-section-list__delete"
                                    onClick={() => onDelete(section)}
                                    disabled={
                                        isSaving ||
                                        section.sectionType === "SportGroup"
                                    }
                                    title={
                                        section.sectionType === "SportGroup"
                                            ? t(
                                                  "rules.admin.sportGroupDeleteDisabled",
                                                  "Lajikohtaiset säännöt -ryhmää ei voi poistaa.",
                                              )
                                            : undefined
                                    }
                                >
                                    {t("common.delete", "Poista")}
                                </button>
                            </div>
                        </li>
                    ))}
                </ul>
            </div>

            {sportGroup && (
                <div className="rules-section-list__group">
                    <h4>
                        {t(
                            "rules.admin.sportSections",
                            "Lajikohtaiset säännöt",
                        )}
                    </h4>

                    <ul>
                        {sportSections.map((section) => (
                            <li
                                key={section.id}
                                className="rules-section-list__item"
                            >
                                <div>
                                    <strong>{section.title}</strong>
                                    <span>
                                        {t("rules.admin.sortOrder", "Järjestys")}{" "}
                                        {section.sortOrder}
                                        {" · "}
                                        {getRuleCount(section)}{" "}
                                        {t("rules.ruleCountLabel", "sääntöä")}
                                    </span>
                                </div>

                                <div className="rules-section-list__actions">
                                    <button
                                        type="button"
                                        onClick={() => onEdit(section)}
                                        disabled={isSaving}
                                    >
                                        {t("common.edit", "Muokkaa")}
                                    </button>
                                    <button
                                        type="button"
                                        className="rules-section-list__delete"
                                        onClick={() => onDelete(section)}
                                        disabled={isSaving}
                                        title={
                                            getRuleCount(section) > 0
                                                ? t(
                                                      "rules.admin.sectionDeleteHasRules",
                                                      "Poista ensin kaikki säännöt tästä osiosta.",
                                                  )
                                                : undefined
                                        }
                                    >
                                        {t("common.delete", "Poista")}
                                    </button>
                                </div>
                            </li>
                        ))}
                    </ul>
                </div>
            )}
        </div>
    );
}
