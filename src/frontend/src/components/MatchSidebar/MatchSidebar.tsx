import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { floorballMatchService } from '../../api/floorball/floorballMatchService';
import type { FloorballMatchDto } from '../../types/floorball/floorballTypes';
import { FloorballMatchStatus } from '../../types/floorball/floorballTypes';
import SidebarMatchCard from './SidebarMatchCard';
import './MatchSidebar.scss';

const MAX_MATCHES_PER_SECTION = 5;

function MatchSidebar() {
  const { t } = useTranslation();
  const [matches, setMatches] = useState<FloorballMatchDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchMatches = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);
      
      const response = await floorballMatchService.getAll({
        pageSize: 20,
        sortOrder: 'asc'
      });

      if (response.success && response.data) {
        setMatches(response.data);
      } else {
        setMatches([]);
      }
    } catch (err) {
      console.error('Failed to fetch matches:', err);
      setError(t('sidebar.error', 'Otteluiden lataus epäonnistui'));
    } finally {
      setIsLoading(false);
    }
  }, [t]);

  useEffect(() => {
    fetchMatches();
  }, [fetchMatches]);

  // Filter matches by status
  const liveMatches = matches
    .filter(m => m.status === FloorballMatchStatus.InProgress)
    .slice(0, MAX_MATCHES_PER_SECTION);

  const upcomingMatches = matches
    .filter(m => m.status === FloorballMatchStatus.Scheduled)
    .sort((a, b) => new Date(a.scheduledDateTime).getTime() - new Date(b.scheduledDateTime).getTime())
    .slice(0, MAX_MATCHES_PER_SECTION);

  const completedMatches = matches
    .filter(m => m.status === FloorballMatchStatus.Completed)
    .sort((a, b) => new Date(b.scheduledDateTime).getTime() - new Date(a.scheduledDateTime).getTime())
    .slice(0, MAX_MATCHES_PER_SECTION);

  if (isLoading) {
    return (
      <div className="match-sidebar">
        <div className="match-sidebar__loading">
          <div className="loading-spinner" />
          <span>{t('sidebar.loading', 'Ladataan...')}</span>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="match-sidebar">
        <div className="match-sidebar__error">
          <p>{error}</p>
          <button onClick={fetchMatches} className="match-sidebar__retry-btn">
            {t('common.retry', 'Yritä uudelleen')}
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="match-sidebar">
      {/* Live Matches Section */}
      <div className="match-sidebar__section">
        <h3 className="match-sidebar__section-title match-sidebar__section-title--live">
          <span className="live-indicator" />
          {t('sidebar.liveMatches', 'Käynnissä')}
        </h3>
        {liveMatches.length > 0 ? (
          <div className="match-sidebar__list">
            {liveMatches.map(match => (
              <SidebarMatchCard key={match.id} match={match} />
            ))}
          </div>
        ) : (
          <p className="match-sidebar__empty">
            {t('sidebar.noLiveMatches', 'Ei käynnissä olevia otteluita')}
          </p>
        )}
      </div>

      {/* Upcoming Matches Section */}
      <div className="match-sidebar__section">
        <h3 className="match-sidebar__section-title">
          {t('sidebar.upcomingMatches', 'Tulevat')}
        </h3>
        {upcomingMatches.length > 0 ? (
          <div className="match-sidebar__list">
            {upcomingMatches.map(match => (
              <SidebarMatchCard key={match.id} match={match} />
            ))}
          </div>
        ) : (
          <p className="match-sidebar__empty">
            {t('sidebar.noUpcomingMatches', 'Ei tulevia otteluita')}
          </p>
        )}
      </div>

      {/* Completed Matches Section */}
      <div className="match-sidebar__section">
        <h3 className="match-sidebar__section-title">
          {t('sidebar.completedMatches', 'Päättyneet')}
        </h3>
        {completedMatches.length > 0 ? (
          <div className="match-sidebar__list">
            {completedMatches.map(match => (
              <SidebarMatchCard key={match.id} match={match} />
            ))}
          </div>
        ) : (
          <p className="match-sidebar__empty">
            {t('sidebar.noCompletedMatches', 'Ei päättyneitä otteluita')}
          </p>
        )}
      </div>
    </div>
  );
}

export default MatchSidebar;
