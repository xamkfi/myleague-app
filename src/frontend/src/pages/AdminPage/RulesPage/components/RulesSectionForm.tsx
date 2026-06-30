import { useTranslation } from "react-i18next";
import Button from "../../../../components/Button/Button";
import backArrow from "../../../../assets/adminIcons/backArrow.svg";
import type {
    RulesSection,
    RulesSectionType,
} from "../../../../types/admin/ruleTypes";
import {
    findSportGroupSection,
    MAIN_TAB_SECTION_TYPES,
} from "../../../../utils/rulesSectionUtils";
import "./RulesSectionForm.scss";

interface RulesSectionFormState {
    id: string | null;
    title: string;
    sortOrder: number;
    sectionType: RulesSectionType;
    parentSectionId: string | null;
}

type SectionCreateMode = "main" | "sport";

interface RulesSectionFormProps {
    sections: RulesSection[];
    formState: RulesSectionFormState;
    createMode?: SectionCreateMode;
    isSaving: boolean;
    onBack: () => void;
    onChange: (updates: Partial<RulesSectionFormState>) => void;
    onSave: () => void;
}

export default function RulesSectionForm({
    sections,
    formState,
    createMode,
    isSaving,
    onBack,
    onChange,
    onSave,
}: Readonly<RulesSectionFormProps>) {
    const { t } = useTranslation();
    const sportGroup = findSportGroupSection(sections);
    const isEditMode = Boolean(formState.id);
    const isSportSection = formState.sectionType === "Sport";

    const availableSectionTypes = ((): RulesSectionType[] => {
        if (isEditMode) {
            return formState.sectionType === "Sport"
                ? ["Sport"]
                : MAIN_TAB_SECTION_TYPES.filter(
                      (type) => type !== "SportGroup" || !sportGroup || formState.sectionType === "SportGroup",
                  );
        }

        if (createMode === "sport") {
            return ["Sport"];
        }

        return MAIN_TAB_SECTION_TYPES.filter(
            (type) => type !== "SportGroup" || !sportGroup,
        );
    })();

    return (
        <div className="rules-section-form">
            <div className="rules-section-form__header">
                <button
                    type="button"
                    className="rules-section-form__back"
                    onClick={onBack}
                    aria-label={t("common.back", "Takaisin")}
                >
                    <img src={backArrow} alt="" />
                </button>

                <h3>
                    {isEditMode
                        ? t("rules.admin.editSection", "Muokkaa sääntöosiota")
                        : createMode === "sport"
                          ? t("rules.admin.addSportSection", "Lisää lajikohtainen laji")
                          : t("rules.admin.addMainSection", "Lisää päävälilehti")}
                </h3>
            </div>

            <div className="rules-section-form__card">
                <label className="rules-section-form__label">
                    {t("rules.admin.sectionTitle", "Otsikko")}
                    <input
                        type="text"
                        value={formState.title}
                        onChange={(event) =>
                            onChange({ title: event.target.value })
                        }
                    />
                </label>

                <label className="rules-section-form__label">
                    {t("rules.admin.sortOrder", "Järjestysnumero")}
                    <input
                        type="number"
                        min={1}
                        value={formState.sortOrder}
                        onChange={(event) =>
                            onChange({
                                sortOrder: Number.parseInt(
                                    event.target.value,
                                    10,
                                ) || 1,
                            })
                        }
                    />
                </label>

                <label className="rules-section-form__label">
                    {t("rules.admin.sectionType", "Osion tyyppi")}
                    <select
                        value={formState.sectionType}
                        onChange={(event) => {
                            const nextType = event.target
                                .value as RulesSectionType;

                            onChange({
                                sectionType: nextType,
                                parentSectionId:
                                    nextType === "Sport"
                                        ? sportGroup?.id ?? null
                                        : null,
                            });
                        }}
                        disabled={
                            isEditMode ||
                            createMode === "sport" ||
                            availableSectionTypes.length <= 1
                        }
                    >
                        {availableSectionTypes.map((option) => (
                            <option key={option} value={option}>
                                {t(
                                    `rules.admin.sectionTypes.${option}`,
                                    option,
                                )}
                            </option>
                        ))}
                    </select>
                </label>

                {isSportSection && sportGroup && (
                    <p className="rules-section-form__hint">
                        {t(
                            "rules.admin.sportSectionHint",
                            "Laji lisätään osioon {{group}} / [otsikko].",
                            { group: sportGroup.title },
                        )}
                    </p>
                )}

                {createMode === "main" && (
                    <p className="rules-section-form__hint">
                        {t(
                            "rules.admin.mainSectionHint",
                            "Päävälilehdet: Yleissäännöt, Vahvistukset, Maksut ja Lajikohtaiset säännöt. Lajit (esim. Jalkapallo) lisätään erikseen.",
                        )}
                    </p>
                )}
            </div>

            <div className="rules-section-form__footer">
                <Button rounded="pill" onClick={onSave} disabled={isSaving}>
                    {isEditMode
                        ? t("common.save", "Tallenna")
                        : t("rules.admin.createSection", "Luo osio")}
                </Button>
            </div>
        </div>
    );
}

export type { RulesSectionFormState, SectionCreateMode };
