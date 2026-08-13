import { useTranslation } from 'react-i18next';
import { TEAM_CATEGORY_META } from '../TeamCategoryBadge/teamCategoryMeta';
import './TeamCategoryFilter.scss';

const ALL_CATEGORIES = Object.keys(TEAM_CATEGORY_META);

interface TeamCategoryFilterProps {
  /** Currently selected category values (backend enum names, e.g. 'Adult'). */
  selected: string[];
  onChange: (categories: string[]) => void;
}

/**
 * Multi-select toggle chips for filtering admin lists by audience/age-group
 * category. Zero selected chips means "show all".
 */
function TeamCategoryFilter({ selected, onChange }: TeamCategoryFilterProps) {
  const { t } = useTranslation();

  const toggle = (category: string) => {
    if (selected.includes(category)) {
      onChange(selected.filter((value) => value !== category));
    } else {
      onChange([...selected, category]);
    }
  };

  return (
    <div className="team-category-filter" role="group" aria-labelledby="team-category-filter-label">
      <div className="team-category-filter__heading">
        <span id="team-category-filter-label" className="team-category-filter__label">
          {t('audience.filterLabel')}
        </span>
        <span className="team-category-filter__hint">{t('audience.filterHint')}</span>
      </div>
      <div className="team-category-filter__chips">
      {ALL_CATEGORIES.map((category) => {
        const meta = TEAM_CATEGORY_META[category];
        const isActive = selected.includes(category);

        return (
          <button
            key={category}
            type="button"
            className={`team-category-filter__chip team-category-filter__chip--${meta.modifier}${isActive ? ' active' : ''}`}
            onClick={() => toggle(category)}
            aria-pressed={isActive}
          >
            <span className="team-category-filter__dot" aria-hidden="true" />
            {t(meta.i18nKey)}
          </button>
        );
      })}
      </div>
    </div>
  );
}

export default TeamCategoryFilter;
