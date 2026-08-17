import { useEffect, useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import './FootballMatchesList.scss';
import MatchRow from './FootballMatchRow';
import type { FootballMatchDto, FootballTeam } from '../../../types/football/footballTypes';
import { footballSeasonService, type FootballSeasonDto } from '../../../api/football/footballSeasonService';
import { useFootballTeamsData } from '../../../hooks/useTeamsData';
import { createTeamSlug } from '../../../utils/slugUtils';

export type MatchesListVariant = 'results' | 'fixtures';

/**
 * Controls how matches are grouped under headers.
 * - 'season' (default) groups matches under their season name and triggers a season fetch.
 * - 'none' renders all matches as a single flat list and skips the season fetch entirely
 *   (used by tournament views where the parent already provides the competition context).
 */
export type MatchesListGroupingMode = 'season' | 'none';

interface MatchesListProps {
  variant: MatchesListVariant;
  matchesLoading: boolean;
  matchesError: string | null;
  matches: FootballMatchDto[] | null;
  currentPage: number;
  totalPages: number;
  handlePageChange: (page: number) => void;
  team?: FootballTeam | null; // optional, used for win/loss badge in team view
  groupingMode?: MatchesListGroupingMode;
}

export default function MatchesList({
  variant,
  matchesLoading,
  matchesError,
  matches,
  currentPage,
  totalPages,
  handlePageChange,
  team,
  groupingMode = 'season'
}: MatchesListProps) {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { teams, refetch } = useFootballTeamsData();
  const [seasons, setSeasons] = useState<FootballSeasonDto[] | null>(null);

  useEffect(() => {
    if (groupingMode !== 'season') {
      return;
    }
    const run = async () => {
      try {
        const seasonsResponse = await footballSeasonService.getAll();
        setSeasons(seasonsResponse.data || []);
      } catch {
        setSeasons([]);
      }
    };
    refetch();
    run();
  }, [refetch, groupingMode]);

  const navigateToTeamPage = (teamId: string): void => {
    const teamToSearch = teams?.find(t => t.id === teamId);
    if (!teamToSearch) return;
    const teamSlug = createTeamSlug(teamToSearch, teams);
    navigate(`/football/team/${teamSlug}`);
  };

  const isResults = variant === 'results';
  const sectionClass = isResults ? 'results-section' : 'fixtures-section';
  const sectionHeaderClass = isResults ? 'results-header' : 'fixtures-header';
  const seasonHeaderClass = isResults ? 'results-season-header' : 'fixtures-season-header';
  const sectionTitle = isResults ? t('teamUserPage.resultsTitle') : t('teamUserPage.scheduled');

  const checkIfTeamWon = (match: FootballMatchDto) => {
    if (!team) return undefined;
    const isWin = (match.homeTeamId === team.id && match.homeScore > match.awayScore) ||
      (match.awayTeamId === team.id && match.awayScore > match.homeScore);
    return isWin;
  };

  const seasonToMatches = useMemo(() => {
    if (groupingMode !== 'season') return [] as Array<{ season: FootballSeasonDto, matches: FootballMatchDto[] }>;
    if (!seasons || !matches) return [] as Array<{ season: FootballSeasonDto, matches: FootballMatchDto[] }>;
    return seasons.map(season => {
      const filtered = matches.filter(m => {
        const sameSeason = m.competitionId === season.id;
        if (!sameSeason) return false;
        return isResults ? m.status === 'Completed' : m.status !== 'Completed';
      });
      return { season, matches: filtered };
    }).filter(x => x.matches.length > 0);
  }, [seasons, matches, isResults, groupingMode]);

  const flatMatches = useMemo(() => {
    if (groupingMode === 'season') return [] as FootballMatchDto[];
    if (!matches) return [] as FootballMatchDto[];
    // When the parent already pre-filtered the matches via API parameters
    // (e.g. competitionId / tournamentGroupId / status), trust that filter and
    // render the list as-is. Avoids hiding completed matches in a tournament
    // group view that intentionally requests both completed and upcoming games.
    return matches;
  }, [matches, groupingMode]);

  const renderMatchRow = (match: FootballMatchDto) => (
    <MatchRow
      key={match.id}
      id={match.id}
      scheduledDateTime={match.scheduledDateTime}
      homeTeamName={match.homeTeamName ?? t('match.tbd', 'TBD')}
      awayTeamName={match.awayTeamName ?? t('match.tbd', 'TBD')}
      homeTeamLogo={match.homeTeamLogo || undefined}
      awayTeamLogo={match.awayTeamLogo || undefined}
      homeScore={match.homeScore}
      awayScore={match.awayScore}
      periodCount={2}
      periodScores={match.periodScores}
      status={match.status}
      statusComponent={isResults && team ? (
        <span className={`result-badge ${checkIfTeamWon(match) ? 'win' : 'loss'}`}>
          {checkIfTeamWon(match) ? 'W' : 'L'}
        </span>
      ) : undefined}
      // When the home team is not yet assigned (placeholder fixture), the row should still be
      // clickable but should not navigate anywhere — fall back to a no-op via empty string.
      onClick={() => match.homeTeamId && navigateToTeamPage(match.homeTeamId)}
    />
  );

  return (
    <div className={sectionClass}>
      {matchesLoading ? (
        <div className="loading-state">{t('matches.loading')}</div>
      ) : matchesError ? (
        <div className="error-state">
          <p>{matchesError}</p>
          <button onClick={() => handlePageChange(1)} className="retry-button">
            {t('common.retry')}
          </button>
        </div>
      ) : matches && matches.length > 0 ? (
        <>
          <div className="matches-grid">
            <div className={sectionHeaderClass}>{sectionTitle}</div>
            {groupingMode === 'season' ? (
              seasonToMatches.map(({ season, matches: seasonMatches }) => (
                <div key={season.id} className="season-block">
                  <div className={seasonHeaderClass}><span>{season.name}</span></div>
                  {seasonMatches.map(renderMatchRow)}
                </div>
              ))
            ) : (
              <div className="season-block">
                {flatMatches.map(renderMatchRow)}
              </div>
            )}
          </div>

          {totalPages > 1 && (
            <div className="pagination">
              <button
                onClick={() => handlePageChange(currentPage - 1)}
                disabled={currentPage === 1}
                className="pagination-btn"
              >
                {t('common.pagination.previous')}
              </button>
              <span className="page-info">
                {t('common.pagination.pageOf', { current: currentPage, total: totalPages })}
              </span>
              <button
                onClick={() => handlePageChange(currentPage + 1)}
                disabled={currentPage === totalPages}
                className="pagination-btn"
              >
                {t('common.pagination.next')}
              </button>
            </div>
          )}
        </>
      ) : (
        <div className="no-matches">
          <p>{t('matches.noMatches')}</p>
        </div>
      )}
    </div>
  );
}



