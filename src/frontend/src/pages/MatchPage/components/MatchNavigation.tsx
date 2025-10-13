type TabType = 'summary' | 'stats' | 'lineups' | 'table';

interface MatchNavigationProps {
  activeTab: TabType;
  onTabChange: (tab: TabType) => void;
}

export default function MatchNavigation({ activeTab, onTabChange }: MatchNavigationProps) {
  return (
    <div className="navigation-tabs">
      <button 
        className={`nav-tab ${activeTab === 'summary' ? 'active' : ''}`}
        onClick={() => onTabChange('summary')}
      >
        SUMMARY
      </button>
      <button 
        className={`nav-tab ${activeTab === 'stats' ? 'active' : ''}`}
        onClick={() => onTabChange('stats')}
      >
        STATS
      </button>
      <button 
        className={`nav-tab ${activeTab === 'lineups' ? 'active' : ''}`}
        onClick={() => onTabChange('lineups')}
      >
        LINEUPS
      </button>
      <button 
        className={`nav-tab ${activeTab === 'table' ? 'active' : ''}`}
        onClick={() => onTabChange('table')}
      >
        TABLE
      </button>
    </div>
  );
}

export type { TabType }; 