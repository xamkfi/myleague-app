import { useTranslation } from 'react-i18next';
import './ResultsSection.scss';
import type { FloorballMatchDto, FloorballTeam } from "../../../types/floorball/floorballTypes"
import { floorballSeasonService, type FloorballSeasonDto } from '../../../api/floorball/floorballSeasonService';
import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { createTeamSlug } from '../../../utils/slugUtils';
import { useFloorballTeamsData } from '../../../hooks/useTeamsData'

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
   const { t, i18n } = useTranslation();
   const navigate = useNavigate()

   // pick locale for date formatting based on current language
   const locale = i18n.language === 'fi' ? 'fi-FI' : 'en-GB';

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

   // ----- RENDER -----
   return (
      <div className="results-section">
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
                  {seasons && matches && seasons.map((season) => {
                     const seasonMatches = matches.filter(m => m.seasonId === season.id);
                     if (seasonMatches.length === 0) return null;
                     return (
                        <>
                           <div key={season.id} className="season-header"><span>{season.name}</span></div>
                           {seasonMatches.map(match => (
                              <div key={match.id} className="match-row">
                                 <div className="match-date">
                                    {new Date(match.scheduledDateTime).toLocaleDateString(locale, {
                                       day: '2-digit',
                                       month: '2-digit',
                                    })} {new Date(match.scheduledDateTime).toLocaleTimeString(locale, {
                                       hour: '2-digit',
                                       minute: '2-digit'
                                    })}
                                 </div>

                                 <div className="teams-section">
                                    <div className="team home-team" onClick={() => navigateToTeamPage(match.homeTeamId)}>
                                       <span className="team-name">{match.homeTeamName}</span>
                                       <span className="team-score">{match.homeScore}</span>
                                    </div>
                                    <div className="team away-team" onClick={() => navigateToTeamPage(match.awayTeamId)}>
                                       <span className="team-name">{match.awayTeamName}</span>
                                       <span className="team-score">{match.awayScore}</span>
                                    </div>
                                 </div>

                                 <div className="match-status">
                                    {match.status === 'Completed' ? (
                                       <span className={`result-badge ${checkIfTeamWon(match) ? 'win' : 'loss'}`}>
                                          {checkIfTeamWon(match) ? 'W' : 'L'}
                                       </span>
                                    ) : (
                                       <span>?</span>
                                    )}
                                 </div>
                              </div>
                           ))}
                        </>
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
   )
}
