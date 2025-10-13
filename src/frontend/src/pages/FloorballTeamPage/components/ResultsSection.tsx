import { useTranslation } from 'react-i18next';
import './ResultsSection.scss';
import type { FloorballMatchDto, FloorballTeam } from "../../../types/floorball/floorballTypes"
import { floorballSeasonService, type FloorballSeasonDto } from '../../../api/floorball/floorballSeasonService';
import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { createTeamSlug } from '../../../utils/slugUtils';
import { useFloorballTeamsData } from '../../../hooks/useTeamsData'
import MatchRow from '../../../components/MatchRow';
import React from 'react';

interface ResultsSectionProps {
   matchesLoading: boolean,
   matchesError: string | null,
   matches: FloorballMatchDto[] | null
   team: FloorballTeam | null
   currentPage: number
   totalPages: number
   handlePageChange: (page: number) => void
}

export default function ResultsSection({
   matchesLoading,
   matchesError,
   matches,
   team,
   currentPage,
   totalPages,
   handlePageChange
}: ResultsSectionProps) {
   const [seasons, setSeasons] = useState<FloorballSeasonDto[] | null>(null);
   const { teams, refetch } = useFloorballTeamsData();
   const { t } = useTranslation();
   const navigate = useNavigate()

   const fetchSeasons = async () => {
      try {
         const seasonsResponse = await floorballSeasonService.getAll();
         setSeasons(seasonsResponse.data);
      } catch {
         setSeasons([]); // fallback
      }
   }

   // fetch seasons once
   useEffect(() => {
      refetch();
      fetchSeasons();
   }, [refetch]);

   const checkIfTeamWon = (match: FloorballMatchDto) => {
      return (match.homeTeamId === team?.id && match.homeScore > match.awayScore) ||
         (match.awayTeamId === team?.id && match.awayScore > match.homeScore)
   }

   const navigateToTeamPage = (teamId: string): void => {
      const teamToSearch: FloorballTeam | undefined = teams?.find(t => t.id == teamId)
      console.log(teamToSearch, teamToSearch)
      if (teamToSearch != undefined) {
         const teamSlug = createTeamSlug(teamToSearch, teams)
         navigate(`/team/${teamSlug}`)
      }
   }

   // Custom status component for win/loss badges
   const MatchStatusComponent = ({ match }: { match: FloorballMatchDto }) => {
      if (match.status === 'Completed') {
         const isWin = checkIfTeamWon(match);
         return (
            <span className={`result-badge ${isWin ? 'win' : 'loss'}`}>
               {isWin ? 'W' : 'L'}
            </span>
         );
      }
      return <span>?</span>;
   };

   return (
      <div className="results-section">
         <div className="results-container">
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
                     <div className="results-header">
                        {t('teamUserPage.resultsTitle')}
                     </div>
                     {seasons && matches && seasons.map((season) => {
                        const seasonMatches = matches.filter(m => m.seasonId === season.id);
                        if (seasonMatches.length === 0) return null;
                        return (

                           <React.Fragment key={season.id}>
                              <div
                                 className="results-season-header"
                                 onClick={() => navigate(`/league/${season.id}`)}
                                 role="button"
                                 tabIndex={0}
                                 onKeyDown={(e) => { if (e.key === 'Enter' || e.key === ' ') navigate(`/league/${season.id}`); }}
                              >
                                 <span>{season.name}</span>
                              </div>
                              {seasonMatches.map(match => (
                                 <MatchRow
                                    key={match.id}
                                    id={match.id}
                                    scheduledDateTime={match.scheduledDateTime}
                                    homeTeamName={match.homeTeamName}
                                    awayTeamName={match.awayTeamName}
                                    homeScore={match.homeScore}
                                    awayScore={match.awayScore}
                                    periodCount={3}
                                    periodScores={match.periodScores}
                                    statusComponent={<MatchStatusComponent match={match} />}
                                    onClick={() => navigateToTeamPage(match.homeTeamId)}
                                 />
                              ))}
                           </React.Fragment>
                        );
                     })}
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
      </div>
   )
}