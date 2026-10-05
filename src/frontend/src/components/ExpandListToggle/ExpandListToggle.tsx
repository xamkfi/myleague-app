import type { ReactElement } from 'react';
import { useTranslation } from 'react-i18next';
import type { ExpandableList } from '../../hooks/useExpandableList';
import './ExpandListToggle.scss';

interface ExpandListToggleProps<T> {
  list: ExpandableList<T>;
  totalCount: number;
}

function ExpandListToggle<T>({ list, totalCount }: ExpandListToggleProps<T>): ReactElement | null {
  const { t } = useTranslation();
  if (!list.canExpand) {
    return null;
  }
  return (
    <button
      type="button"
      className="expand-list-toggle"
      onClick={list.toggle}
      aria-expanded={list.isExpanded}
    >
      {list.isExpanded
        ? t('leaguePage.stats.showLess', 'Show less')
        : t('leaguePage.stats.showMore', 'Show more ({{count}})', { count: totalCount })}
    </button>
  );
}

export default ExpandListToggle;
