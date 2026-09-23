import { useTranslation } from 'react-i18next';
import UnderlineTabs from '../UnderlineTabs/UnderlineTabs';

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
    <UnderlineTabs
      tabs={tabs}
      activeId={currentTab}
      onChange={(id) => onTabChange?.(id)}
      ariaLabel={t('teamUserPage.summary')}
    />
  );
}
