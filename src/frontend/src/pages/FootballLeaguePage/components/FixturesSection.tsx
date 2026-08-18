import { useState, useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import type { FootballMatchDto } from '../../../types/football/footballTypes';
import { FootballMatchStatus } from '../../../types/football/footballTypes';
import LoadingSpinner from '../../../components/LoadingSpinner/LoadingSpinner';
import { formatMatchDateTime } from '../../../utils/helpers';
import './FixturesSection.scss';

type ScheduleFilter = 'all' | 'upcoming' | 'past' | 'next';

interface FixturesSectionProps {
  matchesLoading: boolean;
  matchesError: string | null;
  matches: FootballMatchDto[] | null;
  currentPage: number;
  totalPages: number;
  handlePageChange: (page: number) => void;
}

function getMatchStatusClass(status: FootballMatchStatus): string {
  switch (status) {
    case FootballMatchStatus.Completed: return 'schedule-match--completed';
    case FootballMatchStatus.InProgress: return 'schedule-match--live';
    case FootballMatchStatus.Cancelled: return 'schedule-match--cancelled';
    case FootballMatchStatus.Postponed: return 'schedule-match--postponed';
    default: return 'schedule-match--scheduled';
  }
}

function getStatusLabel(status: FootballMatchStatus, t: (key: string) => string): string {
  switch (status) {
    case FootballMatchStatus.Completed: return t('fixtures.statusCompleted');
    case FootballMatchStatus.InProgress: return t('fixtures.statusLive');
    case FootballMatchStatus.Cancelled: return t('fixtures.statusCancelled');
    case FootballMatchStatus.Postponed: return t('fixtures.statusPostponed');
    default: return '';
  }
}

export default function FixturesSection({
  matchesLoading,
  matchesError,
  matches,
  currentPage,
  totalPages,
  handlePageChange,
}: FixturesSectionProps) {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [filter, setFilter] = useState<ScheduleFilter>('all');

  const filteredMatches = useMemo(() => {
    if (!matches) return [];

    switch (filter) {
      case 'upcoming':
        return matches.filter(m => m.status !== FootballMatchStatus.Completed);
      case 'past':
        return matches.filter(m => m.status === FootballMatchStatus.Completed);
      case 'next': {
        const nextMatch = matches.find(m =>
          m.status === FootballMatchStatus.Scheduled || m.status === FootballMatchStatus.InProgress
        );
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

  const handleMatchClick = (matchId: string) => {
    navigate(`/football/match/${matchId}`);
  };

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
          <button onClick={() => handlePageChange(1)} className="schedule-section__retry">
            {t('common.retry')}
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="schedule-section">
      <div className="schedule-section__header">
        <h2 className="schedule-section__title">{t('fixtures.title')}</h2>
        <div className="schedule-section__filters">
          {filters.map((f) => (
            <button
              key={f.key}
              className={`schedule-filter ${filter === f.key ? 'schedule-filter--active' : ''}`}
              onClick={() => setFilter(f.key)}
            >
              {f.label}
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
            {filteredMatches.map((match) => {
              const [date, time] = formatMatchDateTime(match.scheduledDateTime);
              const isCompleted = match.status === FootballMatchStatus.Completed;
              const isLive = match.status === FootballMatchStatus.InProgress;
              const homeWon = isCompleted && match.homeScore > match.awayScore;
              const awayWon = isCompleted && match.awayScore > match.homeScore;
              const statusLabel = getStatusLabel(match.status, t);

              return (
                <div
                  key={match.id}
                  className={`schedule-match ${getMatchStatusClass(match.status)}`}
                  onClick={() => handleMatchClick(match.id)}
                  role="button"
                  tabIndex={0}
                  onKeyDown={(e) => {
                    if (e.key === 'Enter' || e.key === ' ') {
                      e.preventDefault();
                      handleMatchClick(match.id);
                    }
                  }}
                >
                  <div className="schedule-match__date">
                    <span className="schedule-match__date-day">{date}</span>
                    <span className="schedule-match__date-time">{time}</span>
                  </div>

                  <div className="schedule-match__teams">
                    <div className={`schedule-match__team ${homeWon ? 'schedule-match__team--winner' : ''}`}>
                      {match.homeTeamLogo && (
                        <img src={match.homeTeamLogo} alt="" className="schedule-match__team-logo" />
                      )}
                      <span className="schedule-match__team-name">{match.homeTeamName ?? 'TBD'}</span>
                    </div>
                    <div className={`schedule-match__team ${awayWon ? 'schedule-match__team--winner' : ''}`}>
                      {match.awayTeamLogo && (
                        <img src={match.awayTeamLogo} alt="" className="schedule-match__team-logo" />
                      )}
                      <span className="schedule-match__team-name">{match.awayTeamName ?? 'TBD'}</span>
                    </div>
                  </div>

                  <div className="schedule-match__score">
                    {isCompleted || isLive ? (
                      <>
                        <span className={`schedule-match__score-value ${homeWon ? 'schedule-match__score-value--winner' : ''}`}>
                          {match.homeScore}
                        </span>
                        <span className="schedule-match__score-separator">-</span>
                        <span className={`schedule-match__score-value ${awayWon ? 'schedule-match__score-value--winner' : ''}`}>
                          {match.awayScore}
                        </span>
                      </>
                    ) : (
                      <span className="schedule-match__score-pending">vs</span>
                    )}
                  </div>

                  {statusLabel && (
                    <div className="schedule-match__status">
                      <span className={`schedule-match__status-badge schedule-match__status-badge--${match.status.toLowerCase()}`}>
                        {statusLabel}
                      </span>
                    </div>
                  )}
                </div>
              );
            })}
          </div>

          {totalPages > 1 && (
            <div className="schedule-section__pagination">
              <button
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
