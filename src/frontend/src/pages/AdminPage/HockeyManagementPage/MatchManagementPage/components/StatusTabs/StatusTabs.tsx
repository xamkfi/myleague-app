import { useTranslation } from 'react-i18next';
import './StatusTabs.scss';

type MatchTab = 'all' | 'ongoing' | 'scheduled' | 'completed' | 'cancelled';

interface StatusCounts {
  total: number;
  inProgress: number;
  scheduled: number;
  completed: number;
  cancelled: number;
}

interface StatusTabsProps {
  activeTab: MatchTab;
  onTabChange: (tab: MatchTab) => void;
  counts: StatusCounts;
}

function StatusTabs({ activeTab, onTabChange, counts }: StatusTabsProps) {
  const { t } = useTranslation();

  const tabs: { key: MatchTab; label: string; count: number }[] = [
    { key: 'all', label: t('hockey.matches.tabs.all', 'All'), count: counts.total },
    { key: 'ongoing', label: t('hockey.matches.tabs.ongoing', 'Ongoing'), count: counts.inProgress },
    { key: 'scheduled', label: t('hockey.matches.tabs.scheduled', 'Scheduled'), count: counts.scheduled },
    { key: 'completed', label: t('hockey.matches.tabs.completed', 'Completed'), count: counts.completed },
    { key: 'cancelled', label: t('hockey.matches.tabs.cancelled', 'Cancelled'), count: counts.cancelled },
  ];

  return (
    <div className="status-tabs" role="tablist" aria-label={t('hockey.matches.tabs.ariaLabel', 'Filter matches by status')}>
      {tabs.map((tab) => (
        <button
          key={tab.key}
          type="button"
          className={`status-tabs__tab ${activeTab === tab.key ? 'status-tabs__tab--active' : ''}`}
          onClick={() => onTabChange(tab.key)}
          role="tab"
          aria-selected={activeTab === tab.key}
          aria-controls="match-table-panel"
          id={`tab-${tab.key}`}
        >
          <span className="status-tabs__label">{tab.label}</span>
          <span className={`status-tabs__count ${activeTab === tab.key ? 'status-tabs__count--active' : ''}`}>
            {tab.count}
          </span>
        </button>
      ))}
    </div>
  );
}

export default StatusTabs;
export type { MatchTab, StatusCounts };
