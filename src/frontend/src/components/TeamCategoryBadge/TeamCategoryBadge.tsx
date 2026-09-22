import { useTranslation } from 'react-i18next';
import { TEAM_CATEGORY_META } from './teamCategoryMeta';
import './TeamCategoryBadge.scss';

interface TeamCategoryBadgeProps {
  category?: string | null;
  /** When true, articles without a group show as visible to every audience. */
  showAll?: boolean;
}

/**
 * Small colored pill showing an item's audience/age-group category using the same
 * primary colors as the public site themes. Renders nothing for unknown/missing values.
 */
function TeamCategoryBadge({ category, showAll = false }: TeamCategoryBadgeProps) {
  const { t } = useTranslation();
  const meta = category ? TEAM_CATEGORY_META[category] : undefined;

  if (!meta) {
    if (!showAll) {
      return null;
    }

    return (
      <span className="team-category-badge team-category-badge--all">
        {t('audience.all')}
      </span>
    );
  }

  return (
    <span className={`team-category-badge team-category-badge--${meta.modifier}`}>
      {t(meta.i18nKey)}
    </span>
  );
}

export default TeamCategoryBadge;
