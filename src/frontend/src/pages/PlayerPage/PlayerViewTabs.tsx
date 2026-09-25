import { lazy, Suspense, useState, type ReactNode } from 'react';
import { useTranslation } from 'react-i18next';
import LoadingSpinner from '../../components/LoadingSpinner/LoadingSpinner';
import UnderlineTabs from '../../components/UnderlineTabs/UnderlineTabs';
import {
  countDistinctCompetitions,
  type GoalieSeasonInput,
  type SkaterSeasonInput,
} from './playerSeasonSeries';

const PlayerDevelopmentCharts = lazy(() => import('./PlayerDevelopmentCharts'));

type PlayerViewTab = 'numbers' | 'development';

interface PlayerViewTabsProps {
  numbers: ReactNode;
  skaterSeasons: SkaterSeasonInput[];
  goalieSeasons?: GoalieSeasonInput[];
}

export function PlayerViewTabs({
  numbers,
  skaterSeasons,
  goalieSeasons = [],
}: PlayerViewTabsProps) {
  const { t } = useTranslation();
  const [tab, setTab] = useState<PlayerViewTab>('numbers');
  const seasonCount = Math.max(
    countDistinctCompetitions(skaterSeasons),
    countDistinctCompetitions(goalieSeasons),
  );

  const selectTab = (id: string): void => {
    if (id === 'numbers' || id === 'development') {
      setTab(id);
    }
  };

  return (
    <div className="player-view">
      <UnderlineTabs
        tabs={[
          { id: 'numbers', label: t('playerPage.charts.numbers') },
          { id: 'development', label: t('playerPage.charts.development') },
        ]}
        activeId={tab}
        onChange={selectTab}
        ariaLabel={t('playerPage.charts.tabsLabel')}
      />

      {tab === 'numbers' && (
        <div role="tabpanel" id="tabpanel-numbers" aria-labelledby="tab-numbers">
          {numbers}
        </div>
      )}

      {tab === 'development' && (
        <div className="player-container" role="tabpanel" id="tabpanel-development" aria-labelledby="tab-development">
          {seasonCount < 2 ? (
            <p className="no-data-message">{t('playerPage.charts.needMoreSeasons')}</p>
          ) : (
            <Suspense fallback={<LoadingSpinner text={t('playerPage.charts.loading')} />}>
              <PlayerDevelopmentCharts
                skaterSeasons={skaterSeasons}
                goalieSeasons={goalieSeasons}
              />
            </Suspense>
          )}
        </div>
      )}
    </div>
  );
}
