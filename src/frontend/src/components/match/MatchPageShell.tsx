import type { ReactNode } from 'react';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../PageTemplate/PageTemplate';
import LoadingSpinner from '../LoadingSpinner/LoadingSpinner';
import MatchBreadcrumb from './MatchBreadcrumb';
import MatchNavigation from './MatchNavigation';
import MatchScoreHeader from './MatchScoreHeader';
import type { MatchScoreHeaderProps, MatchTabType, TableTabVariant } from './matchPageTypes';
import './MatchPageShell.scss';

interface MatchPageShellProps {
  isLoading: boolean;
  error: string | null;
  competitionName?: string;
  competitionPath?: string;
  header?: MatchScoreHeaderProps;
  activeTab: MatchTabType;
  onTabChange: (tab: MatchTabType) => void;
  tableVariant: TableTabVariant;
  children: ReactNode;
}

export default function MatchPageShell({
  isLoading,
  error,
  competitionName,
  competitionPath,
  header,
  activeTab,
  onTabChange,
  tableVariant,
  children,
}: MatchPageShellProps) {
  const { t } = useTranslation();
  const pageTitle = t('matchPage.pageTitle');

  if (isLoading) {
    return (
      <div className="match-page-wrapper">
        <PageTemplate title={pageTitle}>
          <div className="match-page">
            <div className="match-page-shell__state">
              <LoadingSpinner text={t('matchPage.loading')} />
            </div>
          </div>
        </PageTemplate>
      </div>
    );
  }

  if (error || !header) {
    return (
      <div className="match-page-wrapper">
        <PageTemplate title={pageTitle}>
          <div className="match-page">
            <div className="match-page-shell__state match-page-shell__error">
              {error ?? t('matchPage.notFound')}
            </div>
          </div>
        </PageTemplate>
      </div>
    );
  }

  return (
    <div className="match-page-wrapper">
      <PageTemplate title={pageTitle}>
        <div className="match-page">
          {competitionName && competitionPath && (
            <MatchBreadcrumb
              competitionName={competitionName}
              competitionPath={competitionPath}
            />
          )}
          <MatchScoreHeader {...header} />
          <MatchNavigation
            activeTab={activeTab}
            onTabChange={onTabChange}
            tableVariant={tableVariant}
          />
          {children}
        </div>
      </PageTemplate>
    </div>
  );
}
