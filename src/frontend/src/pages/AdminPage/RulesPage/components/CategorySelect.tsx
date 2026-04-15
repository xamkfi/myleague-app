import { useTranslation } from "react-i18next";
import "./CategorySelect.scss";

interface CategorySelectProps {
    value: string;
    onChange: (value: string) => void;
    includeAll?: boolean;
}

export default function CategorySelect({
    value,
    onChange,
    includeAll = false,
}: Readonly<CategorySelectProps>) {
    const { t } = useTranslation();

    return (
        <div className="rules-management-page__filter">
            <select
                value={value}
                onChange={(e) => onChange(e.target.value)}
                className="rules-management-page__filter-select"
            >
                {includeAll && (
                    <option value="all">
                        {t("rules.admin.allCategories")}
                    </option>
                )}
                <option value="general">
                    {t("rules.admin.categories.general")}
                </option>
                <option value="fees">{t("rules.admin.categories.fees")}</option>
                <option value="validation">
                    {t("rules.admin.categories.validation")}
                </option>
                <option value="calculation">
                    {t("rules.admin.categories.calculation")}
                </option>
            </select>
        </div>
    );
}
