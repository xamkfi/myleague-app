import { useEffect, useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import './MatchesList.scss';
import MatchRow from '../MatchRow';
import type { FloorballMatchDto, FloorballTeam } from '../../types/floorball/floorballTypes';
import { floorballSeasonService, type FloorballSeasonDto } from '../../api/floorball/floorballSeasonService';
import { useFloorballTeamsData } from '../../hooks/useTeamsData';
import { createTeamSlug } from '../../utils/slugUtils';

export type MatchesListVariant = 'results' | 'fixtures';

interface MatchesListProps {
  variant: MatchesListVariant;
  matchesLoading: boolean;
  matchesError: string | null;
  matches: FloorballMatchDto[] | null;
  currentPage: number;
  totalPages: number;
  handlePageChange: (page: number) => void;
  team?: FloorballTeam | null; // optional, used for win/loss badge in team view
}

export default function MatchesList({
  variant,
  matchesLoading,
  matchesError,
  matches,
  currentPage,
  totalPages,
  handlePageChange,
  team
}: MatchesListProps) {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { teams, refetch } = useFloorballTeamsData();
  const [seasons, setSeasons] = useState<FloorballSeasonDto[] | null>(null);

  useEffect(() => {
    const run = async () => {
      try {
        const seasonsResponse = await floorballSeasonService.getAll();
        setSeasons(seasonsResponse.data || []);
      } catch {
        setSeasons([]);
      }
    };
    refetch();
    run();
  }, [refetch]);

  const navigateToTeamPage = (teamId: string): void => {
    const teamToSearch = teams?.find(t => t.id === teamId);
    if (!teamToSearch) return;
    const teamSlug = createTeamSlug(teamToSearch, teams);
    navigate(`/team/${teamSlug}`);
  };

  const isResults = variant === 'results';
  const sectionClass = isResults ? 'results-section' : 'fixtures-section';
  const sectionHeaderClass = isResults ? 'results-header' : 'fixtures-header';
  const seasonHeaderClass = isResults ? 'results-season-header' : 'fixtures-season-header';
  const sectionTitle = isResults ? t('teamUserPage.resultsTitle') : t('teamUserPage.scheduled');

  const checkIfTeamWon = (match: FloorballMatchDto) => {
    if (!team) return undefined;
    const isWin = (match.homeTeamId === team.id && match.homeScore > match.awayScore) ||
      (match.awayTeamId === team.id && match.awayScore > match.homeScore);
    return isWin;
  };

  const seasonToMatches = useMemo(() => {
    if (!seasons || !matches) return [] as Array<{ season: FloorballSeasonDto, matches: FloorballMatchDto[] }>;
    return seasons.map(season => {
      const filtered = matches.filter(m => {
        const sameSeason = m.seasonId === season.id;
        if (!sameSeason) return false;
        return isResults ? m.status === 'Completed' : m.status !== 'Completed';
      });
      return { season, matches: filtered };
    }).filter(x => x.matches.length > 0);
  }, [seasons, matches, isResults]);

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
            {seasonToMatches.map(({ season, matches: seasonMatches }) => (
              <div key={season.id} className="season-block">
                <div className={seasonHeaderClass}><span>{season.name}</span></div>
                {seasonMatches.map(match => (
                  <MatchRow
                    key={match.id}
                    id={match.id}
                    scheduledDateTime={match.scheduledDateTime}
                    homeTeamName={match.homeTeamName}
                    awayTeamName={match.awayTeamName}
                    homeTeamLogo={match.homeTeamLogo || undefined}
                    awayTeamLogo={match.awayTeamLogo || undefined}
                    homeScore={match.homeScore}
                    awayScore={match.awayScore}
                    periodCount={3}
                    periodScores={match.periodScores}
                    // show W/L badge if team is provided and variant=results
                    statusComponent={isResults && team ? (
                      <span className={`result-badge ${checkIfTeamWon(match) ? 'win' : 'loss'}`}>
                        {checkIfTeamWon(match) ? 'W' : 'L'}
                      </span>
                    ) : undefined}
                    onClick={() => navigateToTeamPage(match.homeTeamId)}
                  />
                ))}
              </div>
            ))}
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



