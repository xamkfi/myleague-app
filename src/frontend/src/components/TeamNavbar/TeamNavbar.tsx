import { useTranslation } from 'react-i18next';
import './TeamNavbar.scss';

interface TeamNavbarProps {
  currentTab: string;
  onTabChange?: (activeTab: string) => void;
}

export default function TeamNavbar({ currentTab, onTabChange }: TeamNavbarProps) {
  const { t } = useTranslation();

  const tabs = [
    { id: 'summary', label: t('teamUserPage.summary') },
    { id: 'results', label: t('teamUserPage.results') },
    { id: 'roster', label: t('teamUserPage.roster') },
    { id: 'stats', label: t('teamUserPage.stats.Stats') },
    { id: 'standings', label: t('teamUserPage.standings') },
  ];

  return (
    <div className="team-navigation-tabs" role="tablist" aria-label={t('teamUserPage.summary')}>
      {tabs.map((tab) => (
        <button
          key={tab.id}
          type="button"
          className={`team-nav-tab ${currentTab === tab.id ? 'active' : ''}`}
          onClick={() => onTabChange?.(tab.id)}
          role="tab"
          aria-selected={currentTab === tab.id}
          aria-controls={`tabpanel-${tab.id}`}
          id={`tab-${tab.id}`}
        >
          {tab.label}
        </button>
      ))}
    </div>
  );
}
