import { lazy, Suspense, useState, type ReactNode } from 'react';
import { useTranslation } from 'react-i18next';
import LoadingSpinner from '../../components/LoadingSpinner/LoadingSpinner';
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

  return (
    <div className="player-view">
      <div className="player-view-tabs" role="tablist" aria-label={t('playerPage.charts.tabsLabel')}>
        <div className="player-view-tabs__group">
          <button
            type="button"
            role="tab"
            aria-selected={tab === 'numbers'}
            className={`tab-button${tab === 'numbers' ? ' active' : ''}`}
            onClick={() => setTab('numbers')}
          >
            {t('playerPage.charts.numbers')}
          </button>
          <button
            type="button"
            role="tab"
            aria-selected={tab === 'development'}
            className={`tab-button${tab === 'development' ? ' active' : ''}`}
            onClick={() => setTab('development')}
          >
            {t('playerPage.charts.development')}
          </button>
        </div>
      </div>

      {tab === 'numbers' && <div role="tabpanel">{numbers}</div>}

      {tab === 'development' && (
        <div className="player-container" role="tabpanel">
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
