import { useMemo } from "react";
import { useTranslation } from "react-i18next";
import type { RulesSection } from "../../../../types/admin/ruleTypes";
import {
    findSportGroupSection,
    getChildSections,
    getRuleableSections,
    getSectionLabel,
} from "../../../../utils/rulesSectionUtils";
import "./SectionSelect.scss";

interface SectionSelectProps {
    sections: RulesSection[];
    value: string;
    onChange: (value: string) => void;
    includeAll?: boolean;
}

export default function SectionSelect({
    sections,
    value,
    onChange,
    includeAll = false,
}: Readonly<SectionSelectProps>) {
    const { t } = useTranslation();

    const { topLevelSections, sportSections } = useMemo(() => {
        const ruleableSections = getRuleableSections(sections);
        const sportGroup = findSportGroupSection(sections);

        return {
            topLevelSections: ruleableSections.filter(
                (section) => section.sectionType !== "Sport",
            ),
            sportSections: sportGroup
                ? getChildSections(sections, sportGroup.id)
                : ruleableSections.filter(
                      (section) => section.sectionType === "Sport",
                  ),
        };
    }, [sections]);

    return (
        <div className="section-select">
            <select
                value={value}
                onChange={(event) => onChange(event.target.value)}
                className="section-select__input"
            >
                {includeAll && (
                    <option value="all">
                        {t("rules.admin.allSections", "Kaikki osiot")}
                    </option>
                )}

                {topLevelSections.length > 0 && (
                    <optgroup
                        label={t(
                            "rules.admin.topLevelSections",
                            "Päävälilehdet",
                        )}
                    >
                        {topLevelSections.map((section) => (
                            <option key={section.id} value={section.id}>
                                {getSectionLabel(section, sections)}
                            </option>
                        ))}
                    </optgroup>
                )}

                {sportSections.length > 0 && (
                    <optgroup
                        label={t(
                            "rules.admin.sportSections",
                            "Lajikohtaiset säännöt",
                        )}
                    >
                        {sportSections.map((section) => (
                            <option key={section.id} value={section.id}>
                                {getSectionLabel(section, sections)}
                            </option>
                        ))}
                    </optgroup>
                )}
            </select>
        </div>
    );
}
