import { memo } from "react";
import "../RulesManagementPage.scss";

interface RulesSearchInputProps {
    value: string;
    onChange: (value: string) => void;
    placeholder: string;
}

const RulesSearchInput = memo(
    ({ value, onChange, placeholder }: RulesSearchInputProps) => {
        const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
            onChange(e.target.value);
        };

        const handleClear = () => {
            onChange("");
        };

        return (
            <div className="rules-management-page__search">
                <input
                    type="text"
                    value={value}
                    onChange={handleChange}
                    placeholder={placeholder}
                    className="rules-management-page__search-input"
                />

                {value && (
                    <button
                        type="button"
                        className="search-clear-button"
                        onClick={handleClear}
                        title="Clear search"
                    >
                        ✕
                    </button>
                )}
            </div>
        );
    },
);

RulesSearchInput.displayName = "RulesSearchInput";

export default RulesSearchInput;
