import { useTranslation } from 'react-i18next';
import UnderlineTabs from '../UnderlineTabs/UnderlineTabs';
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

  const tabs: { id: MatchTabType; label: string }[] = [
    { id: 'summary', label: t('matchPage.navigation.summary') },
    { id: 'lineups', label: t('matchPage.navigation.lineups') },
  ];
  if (showStatsTab) {
    tabs.push({ id: 'stats', label: t('matchPage.navigation.stats') });
  }
  if (showTableTab) {
    tabs.push({ id: 'table', label: tableLabel });
  }

  const selectTab = (id: string): void => {
    const match = tabs.find((tab) => tab.id === id);
    if (match) {
      onTabChange(match.id);
    }
  };

  return (
    <UnderlineTabs
      tabs={tabs}
      activeId={activeTab}
      onChange={selectTab}
      ariaLabel={t('matchPage.pageTitle')}
    />
  );
}
