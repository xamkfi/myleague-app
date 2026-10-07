import { useTranslation } from 'react-i18next';
import { AUDIENCE_REGISTRY } from '../../audience/audienceRegistry';
import type { TeamCategory } from '../../types/floorball/floorballTypes';
import './TeamCategoryPicker.scss';

interface TeamCategoryPickerProps {
  /** Selected category: 'Adult' | 'Youth' | 'Women'. */
  value: TeamCategory | string;
  onChange: (category: TeamCategory) => void;
  /** Explains what the choice affects. */
  hint?: string;
  disabled?: boolean;
  /** Keeps radio names unique when two pickers are on one page. */
  name?: string;
}

/** Large radio cards for choosing which audience group (adult, youth, women) an item belongs to. */
function TeamCategoryPicker({ value, onChange, hint, disabled = false, name = 'team-category' }: TeamCategoryPickerProps) {
  const { t } = useTranslation();

  return (
    <fieldset className="team-category-picker" disabled={disabled}>
      <legend className="team-category-picker__legend">
        {t('seasonGroup.label')} <span aria-hidden="true">*</span>
      </legend>
      {hint && <p className="team-category-picker__hint">{hint}</p>}
      <div className="team-category-picker__options">
        {AUDIENCE_REGISTRY.map((entry) => {
          const checked: boolean = value === entry.teamCategory;
          return (
            <label
              key={entry.id}
              className={`team-category-picker__option team-category-picker__option--${entry.themeId}${checked ? ' is-selected' : ''}`}
            >
              <input
                type="radio"
                name={name}
                value={entry.teamCategory}
                checked={checked}
                onChange={() => onChange(entry.teamCategory)}
              />
              <span className="team-category-picker__dot" aria-hidden="true" />
              <span className="team-category-picker__text">
                <span className="team-category-picker__title">{t(entry.i18nKey)}</span>
                <span className="team-category-picker__description">{t(`audience.descriptions.${entry.id}`)}</span>
              </span>
            </label>
          );
        })}
      </div>
    </fieldset>
  );
}

export default TeamCategoryPicker;
