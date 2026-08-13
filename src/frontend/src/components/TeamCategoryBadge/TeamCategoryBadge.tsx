import { useTranslation } from 'react-i18next';
import { TEAM_CATEGORY_META } from './teamCategoryMeta';
import './TeamCategoryBadge.scss';

interface TeamCategoryBadgeProps {
  category?: string | null;
}

/**
 * Small colored pill showing an item's audience/age-group category using the same
 * primary colors as the public site themes. Renders nothing for unknown/missing values.
 */
function TeamCategoryBadge({ category }: TeamCategoryBadgeProps) {
  const { t } = useTranslation();
  const meta = category ? TEAM_CATEGORY_META[category] : undefined;

  if (!meta) {
    return null;
  }

  return (
    <span className={`team-category-badge team-category-badge--${meta.modifier}`}>
      {t(meta.i18nKey)}
    </span>
  );
}

export default TeamCategoryBadge;
