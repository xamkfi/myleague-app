import { useTranslation } from 'react-i18next';

type TabType = 'summary' | 'stats' | 'lineups' | 'table';

/**
 * Variant for the "table" tab label so we can adapt it to tournament context. Group-stage
 * matches show "Group standings" and playoff matches show "Playoff bracket"; season matches
 * keep the default "Standings" label.
 */
type TableTabVariant = 'season' | 'tournamentGroup' | 'tournamentPlayoff';

interface MatchNavigationProps {
  activeTab: TabType;
  onTabChange: (tab: TabType) => void;
  /** Controls the label of the "table" tab. Defaults to the season behaviour. */
  tableVariant?: TableTabVariant;
}

export default function MatchNavigation({ activeTab, onTabChange, tableVariant = 'season' }: MatchNavigationProps) {
  const { t } = useTranslation();

  const tableLabel: string = (() => {
    switch (tableVariant) {
      case 'tournamentGroup':
        return t('matchPage.navigation.groupStandings', 'Lohkon taulukko');
      case 'tournamentPlayoff':
        return t('matchPage.navigation.playoffBracket', 'Pudotuspelikaavio');
      case 'season':
      default:
        return t('matchPage.navigation.table');
    }
  })();

  const tabs = [
    { key: 'summary' as TabType, label: t('matchPage.navigation.summary') },
    { key: 'stats' as TabType, label: t('matchPage.navigation.stats') },
    { key: 'lineups' as TabType, label: t('matchPage.navigation.lineups') },
    { key: 'table' as TabType, label: tableLabel }
  ];

  return (
    <div className="navigation-tabs" role="tablist" aria-label="Match navigation tabs">
      {tabs.map((tab) => (
        <button 
          key={tab.key}
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

export type { TabType }; 