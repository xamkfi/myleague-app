import { useTranslation } from 'react-i18next';

type TabType = 'summary' | 'stats' | 'lineups' | 'table';

interface MatchNavigationProps {
  activeTab: TabType;
  onTabChange: (tab: TabType) => void;
}

export default function MatchNavigation({ activeTab, onTabChange }: MatchNavigationProps) {
  const { t } = useTranslation();
  return (
    <div className="navigation-tabs">
      <button 
        className={`nav-tab ${activeTab === 'summary' ? 'active' : ''}`}
        onClick={() => onTabChange('summary')}
      >
        {t('matchPage.navigation.summary')}
      </button>
      <button 
        className={`nav-tab ${activeTab === 'stats' ? 'active' : ''}`}
        onClick={() => onTabChange('stats')}
      >
        {t('matchPage.navigation.stats')}
      </button>
      <button 
        className={`nav-tab ${activeTab === 'lineups' ? 'active' : ''}`}
        onClick={() => onTabChange('lineups')}
      >
        {t('matchPage.navigation.lineups')}
      </button>
      <button 
        className={`nav-tab ${activeTab === 'table' ? 'active' : ''}`}
        onClick={() => onTabChange('table')}
      >
        {t('matchPage.navigation.table')}
      </button>
    </div>
  );
}

export type { TabType }; 