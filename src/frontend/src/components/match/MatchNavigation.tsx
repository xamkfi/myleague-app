import { useTranslation } from 'react-i18next';
import type { MatchTabType, TableTabVariant } from './matchPageTypes';

interface MatchNavigationProps {
  activeTab: MatchTabType;
  onTabChange: (tab: MatchTabType) => void;
  tableVariant?: TableTabVariant;
  showTableTab?: boolean;
  showStatsTab?: boolean;
}

export default function MatchNavigation({
  activeTab,
  onTabChange,
  tableVariant = 'season',
  showTableTab = true,
  showStatsTab = false,
}: MatchNavigationProps) {
  const { t } = useTranslation();

  let tableLabel = t('matchPage.navigation.table');
  if (tableVariant === 'tournamentGroup') {
    tableLabel = t('matchPage.navigation.groupStandings');
  } else if (tableVariant === 'tournamentPlayoff') {
    tableLabel = t('matchPage.navigation.playoffBracket');
  }

  const tabs: { key: MatchTabType; label: string }[] = [
    { key: 'summary', label: t('matchPage.navigation.summary') },
    { key: 'lineups', label: t('matchPage.navigation.lineups') },
  ];
  if (showStatsTab) {
    tabs.push({ key: 'stats', label: t('matchPage.navigation.stats') });
  }
  if (showTableTab) {
    tabs.push({ key: 'table', label: tableLabel });
  }

  return (
    <div className="navigation-tabs" role="tablist" aria-label={t('matchPage.pageTitle')}>
      {tabs.map((tab) => (
        <button
          key={tab.key}
          type="button"
          className={`nav-tab ${activeTab === tab.key ? 'active' : ''}`}
          onClick={() => onTabChange(tab.key)}
          role="tab"
          aria-selected={activeTab === tab.key}
          aria-controls={`tabpanel-${tab.key}`}
          id={`tab-${tab.key}`}
        >
          {tab.label}
        </button>
      ))}
    </div>
  );
}
