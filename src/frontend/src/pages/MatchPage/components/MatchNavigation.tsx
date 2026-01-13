import { useTranslation } from 'react-i18next';

type TabType = 'summary' | 'stats' | 'lineups' | 'table';

interface MatchNavigationProps {
  activeTab: TabType;
  onTabChange: (tab: TabType) => void;
}

export default function MatchNavigation({ activeTab, onTabChange }: MatchNavigationProps) {
  const { t } = useTranslation();
  
  const tabs = [
    { key: 'summary' as TabType, label: t('matchPage.navigation.summary') },
    { key: 'stats' as TabType, label: t('matchPage.navigation.stats') },
    { key: 'lineups' as TabType, label: t('matchPage.navigation.lineups') },
    { key: 'table' as TabType, label: t('matchPage.navigation.table') }
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