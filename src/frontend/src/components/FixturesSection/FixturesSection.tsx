import { useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import LoadingSpinner from '../LoadingSpinner/LoadingSpinner';
import MatchRow from '../MatchRow';
import FootballMatchRow from '../../pages/FootballLeaguePage/components/FootballMatchRow';
import { FloorballMatchStatus } from '../../types/floorball/floorballTypes';
import { getMatchPath, type SportKind } from '../../utils/sportRoutes';
import '../../pages/LeaguePage/components/FixturesSection.scss';

function toRowStatus(status: string): FloorballMatchStatus {
  if (status === 'InProgress') {
    return FloorballMatchStatus.InProgress;
  }
  if (status === 'Completed') {
    return FloorballMatchStatus.Completed;
  }
  return FloorballMatchStatus.Scheduled;
}

type ScheduleFilter = 'all' | 'upcoming' | 'past' | 'next';

export interface FixtureMatch {
  id: string;
  scheduledDateTime: string;
  status: string;
  homeTeamName?: string | null;
  awayTeamName?: string | null;
  homeTeamLogo?: string | null;
  awayTeamLogo?: string | null;
  homeScore: number;
  awayScore: number;
  periodScores?: Record<number, { homeScore: number; awayScore: number }>;
}

interface FixturesSectionProps {
  sport: SportKind;
  matchesLoading: boolean;
  matchesError: string | null;
  matches: FixtureMatch[] | null;
  currentPage: number;
  totalPages: number;
  handlePageChange: (page: number) => void;
}

function isCompleted(status: string): boolean {
  return status === 'Completed';
}

function isLive(status: string): boolean {
  return status === 'InProgress';
}

function isUpcoming(status: string): boolean {
  return status === 'Scheduled' || status === 'InProgress';
}

export default function FixturesSection({
  sport,
  matchesLoading,
  matchesError,
  matches,
  currentPage,
  totalPages,
  handlePageChange,
}: FixturesSectionProps) {
  const { t } = useTranslation();
  const [filter, setFilter] = useState<ScheduleFilter>('all');

  const filteredMatches = useMemo(() => {
    if (!matches) {
      return [];
    }

    switch (filter) {
      case 'upcoming':
        return matches.filter((match) => !isCompleted(match.status));
      case 'past':
        return matches.filter((match) => isCompleted(match.status));
      case 'next': {
        const nextMatch = matches.find((match) => isUpcoming(match.status));
        return nextMatch ? [nextMatch] : [];
      }
      default:
        return matches;
    }
  }, [matches, filter]);

  const filters: { key: ScheduleFilter; label: string }[] = [
    { key: 'all', label: t('fixtures.filterAll') },
    { key: 'upcoming', label: t('fixtures.filterUpcoming') },
    { key: 'past', label: t('fixtures.filterPast') },
    { key: 'next', label: t('fixtures.filterNext') },
  ];

  if (matchesLoading) {
    return (
      <div className="schedule-section">
        <div className="schedule-section__loading">
          <LoadingSpinner size="sm" text={t('matches.loading')} />
        </div>
      </div>
    );
  }

  if (matchesError) {
    return (
      <div className="schedule-section">
        <div className="schedule-section__error">
          <p>{matchesError}</p>
          <button type="button" onClick={() => handlePageChange(1)} className="schedule-section__retry">
            {t('common.retry')}
          </button>
        </div>
      </div>
    );
  }

  const Row = sport === 'football' ? FootballMatchRow : MatchRow;

  return (
    <div className="schedule-section">
      <div className="schedule-section__header">
        <h2 className="schedule-section__title">{t('fixtures.title')}</h2>
        <div className="schedule-section__filters">
          {filters.map((item) => (
            <button
              key={item.key}
              type="button"
              className={`schedule-filter ${filter === item.key ? 'schedule-filter--active' : ''}`}
              onClick={() => setFilter(item.key)}
            >
              {item.label}
            </button>
          ))}
        </div>
      </div>

      {filteredMatches.length === 0 ? (
        <div className="schedule-section__empty">
          <p>{t('fixtures.noMatches')}</p>
        </div>
      ) : (
        <>
          <div className="schedule-list">
            {filteredMatches.map((match) => (
              <Row
                key={match.id}
                id={match.id}
                scheduledDateTime={match.scheduledDateTime}
                homeTeamName={match.homeTeamName ?? t('common.tbd')}
                awayTeamName={match.awayTeamName ?? t('common.tbd')}
                homeTeamLogo={match.homeTeamLogo || undefined}
                awayTeamLogo={match.awayTeamLogo || undefined}
                homeScore={match.homeScore}
                awayScore={match.awayScore}
                periodScores={match.periodScores}
                status={toRowStatus(match.status)}
                href={getMatchPath(sport, match.id)}
                className={isLive(match.status) ? 'schedule-match--live' : undefined}
              />
            ))}
          </div>

          {totalPages > 1 && (
            <div className="schedule-section__pagination">
              <button
                type="button"
                onClick={() => handlePageChange(currentPage - 1)}
                disabled={currentPage === 1}
                className="schedule-section__page-btn"
              >
                {t('common.pagination.previous')}
              </button>
              <span className="schedule-section__page-info">
                {t('common.pagination.pageOf', { current: currentPage, total: totalPages })}
              </span>
              <button
                type="button"
                onClick={() => handlePageChange(currentPage + 1)}
                disabled={currentPage === totalPages}
                className="schedule-section__page-btn"
              >
                {t('common.pagination.next')}
              </button>
            </div>
          )}
        </>
      )}
    </div>
  );
}
