import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { floorballMatchService } from '../../api/floorball/floorballMatchService';
import type { FloorballMatchDto } from '../../types/floorball/floorballTypes';
import { FloorballMatchStatus } from '../../types/floorball/floorballTypes';
import MatchPanelCard from './MatchPanelCard';
import LoadingSpinner from '../LoadingSpinner/LoadingSpinner';
import './MatchesPanel.scss';

const MAX_PER_SECTION = 5;

interface MatchSection {
  key: string;
  titleKey: string;
  titleFallback: string;
  emptyKey: string;
  emptyFallback: string;
  isLive: boolean;
  matches: FloorballMatchDto[];
}

function MatchesPanel() {
  const { t } = useTranslation();
  const [matches, setMatches] = useState<FloorballMatchDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchMatches = async () => {
    try {
      setIsLoading(true);
      setError(null);

      const response = await floorballMatchService.getAll({
        pageSize: 20,
        sortOrder: 'asc',
      });

      if (response.success && response.data) {
        setMatches(response.data);
      } else {
        setMatches([]);
      }
    } catch (err) {
      console.error('MatchesPanel: fetch failed', err);
      setError(t('sidebar.error', 'Otteluiden lataus epäonnistui'));
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchMatches();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // Build sections from fetched data
  const sections: MatchSection[] = [
    {
      key: 'live',
      titleKey: 'sidebar.liveMatches',
      titleFallback: 'Käynnissä',
      emptyKey: 'sidebar.noLiveMatches',
      emptyFallback: 'Ei käynnissä olevia otteluita',
      isLive: true,
      matches: matches
        .filter((m) => m.status === FloorballMatchStatus.InProgress)
        .slice(0, MAX_PER_SECTION),
    },
    {
      key: 'upcoming',
      titleKey: 'sidebar.upcomingMatches',
      titleFallback: 'Tulevat',
      emptyKey: 'sidebar.noUpcomingMatches',
      emptyFallback: 'Ei tulevia otteluita',
      isLive: false,
      matches: matches
        .filter((m) => m.status === FloorballMatchStatus.Scheduled)
        .sort(
          (a, b) =>
            new Date(a.scheduledDateTime).getTime() -
            new Date(b.scheduledDateTime).getTime(),
        )
        .slice(0, MAX_PER_SECTION),
    },
    {
      key: 'completed',
      titleKey: 'sidebar.completedMatches',
      titleFallback: 'Päättyneet',
      emptyKey: 'sidebar.noCompletedMatches',
      emptyFallback: 'Ei päättyneitä otteluita',
      isLive: false,
      matches: matches
        .filter((m) => m.status === FloorballMatchStatus.Completed)
        .sort(
          (a, b) =>
            new Date(b.scheduledDateTime).getTime() -
            new Date(a.scheduledDateTime).getTime(),
        )
        .slice(0, MAX_PER_SECTION),
    },
  ];

  // --- Loading ---
  if (isLoading) {
    return (
      <div className="matches-panel">
        <div className="matches-panel__state">
          <LoadingSpinner size="sm" text={t('sidebar.loading', 'Ladataan...')} />
        </div>
      </div>
    );
  }

  // --- Error ---
  if (error) {
    return (
      <div className="matches-panel">
        <div className="matches-panel__state">
          <p>{error}</p>
          <button
            type="button"
            className="matches-panel__retry-btn"
            onClick={fetchMatches}
          >
            {t('common.retry', 'Yritä uudelleen')}
          </button>
        </div>
      </div>
    );
  }

  // Link to season schedule: use first completed match's seasonId, then any match, otherwise sports/floorball
  const completedSection = sections.find((s) => s.key === 'completed');
  const firstCompletedMatch = completedSection?.matches[0];
  const anyMatch = matches[0];
  const seasonId = firstCompletedMatch?.seasonId ?? anyMatch?.seasonId;
  const fixturesPath = seasonId
    ? `/league/${seasonId}?tab=fixtures`
    : '/sports/floorball';

  // --- Content ---
  return (
    <div className="matches-panel">
      {sections.map((section) => (
        <div key={section.key} className="matches-panel__section">
          <div
            className={`matches-panel__section-header${
              section.isLive ? ' matches-panel__section-header--live' : ''
            }`}
          >
            {section.isLive && <span className="pulse-dot" />}
            <h3 className="matches-panel__section-title">
              {t(section.titleKey, section.titleFallback)}
            </h3>
            {section.matches.length > 0 && (
              <span className="matches-panel__section-count">
                ({section.matches.length})
              </span>
            )}
          </div>

          {section.matches.length > 0 ? (
            section.matches.map((match) => (
              <MatchPanelCard key={match.id} match={match} />
            ))
          ) : (
            <p className="matches-panel__empty-text">
              {t(section.emptyKey, section.emptyFallback)}
            </p>
          )}
        </div>
      ))}

      <div className="matches-panel__all-link-wrap">
        <Link to={fixturesPath} className="matches-panel__all-link">
          {t('sidebar.allMatches', 'Kaikki ottelut')}
        </Link>
      </div>
    </div>
  );
}

export default MatchesPanel;
