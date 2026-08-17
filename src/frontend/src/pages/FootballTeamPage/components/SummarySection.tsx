import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next'
import './SummarySection.scss'
import type { FootballMatchDto, FootballTeam } from "../../../types/football/footballTypes"
import { footballSeasonService, type FootballSeasonDto } from "../../../api/football/footballSeasonService"
import MatchRow from "../../FootballLeaguePage/components/FootballMatchRow"
import { footballMatchService } from "../../../api/football/footballMatchService"
import { useNavigate } from 'react-router-dom'

interface SummarySectionProps {
   team: FootballTeam
   matches: FootballMatchDto[]

}

export default function SummarySection({ team, matches }: SummarySectionProps) {
   const { t } = useTranslation();
   const [seasons, setSeasons] = useState<FootballSeasonDto[] | null>(null);
   const navigate = useNavigate();

   const [todaysMatches, setTodaysMatches] = useState<FootballMatchDto[] | null>(null);

   const fetchTodaysMatches = useCallback(async () => {
      const response = await footballMatchService.getTodaysMatchesByTeam(team.id);
      setTodaysMatches(response.data);
   }, [team.id]);

   const fetchSeasons = useCallback(async () => {
      try {
         const seasonsResponse = await footballSeasonService.getAll();
         setSeasons(seasonsResponse.data);
      } catch {
         setSeasons([]); // fallback
      }
   }, []);

   const handleMatchClick = (matchId: string) => {
      // TODO: Navigate to match page
      console.log('Navigate to match:', matchId);
   }

   useEffect(() => {
      fetchSeasons();
      if (todaysMatches === null) {
         fetchTodaysMatches();
      }
   }, [todaysMatches, fetchTodaysMatches, fetchSeasons]);

   return (
      <div>
         <div className="summary-container">
            {/* Today's Matches Section */}
            {todaysMatches && todaysMatches.length > 0 && (
               <div>
                  <div className="summary-header">
                     {t('teamUserPage.todaysMatches')}
                  </div>
                  
                  {/* Group today's matches by season */}
                  {seasons?.map((season) => {
                     const todaysSeasonMatches = todaysMatches.filter(match => match.competitionId === season.id);
                     
                     // Only render season if it has today's matches
                     if (todaysSeasonMatches.length === 0) return null;
                     
                     return (
                        <div key={season.id}>
                           <div
                              className="summary-season-header"
                              onClick={() => navigate(`/football/league/${season.id}`)}
                              role="button"
                              tabIndex={0}
                              onKeyDown={(e) => { if (e.key === 'Enter' || e.key === ' ') navigate(`/football/league/${season.id}`); }}
                           >
                              {season.name}
                           </div>
                           <div className="summary-season-container">
                             {todaysSeasonMatches.map((match) => (
                                 <MatchRow
                                    key={match.id}
                                    id={match.id}
                                    scheduledDateTime={match.scheduledDateTime}
                                    homeTeamName={match.homeTeamName ?? 'TBD'}
                                    awayTeamName={match.awayTeamName ?? 'TBD'}
                                    homeTeamLogo={match.homeTeamLogo || undefined}
                                    awayTeamLogo={match.awayTeamLogo || undefined}
                                    homeScore={match.homeScore}
                                    awayScore={match.awayScore}
                                    periodCount={2}
                                    periodScores={match.periodScores}
                                    status={match.status}
                                    onClick={() => handleMatchClick(match.id)}
                                 />
                              ))}
                           </div>
                        </div>
                     );
                  })}
               </div>
            )}

            <div className="summary-header">
               {t('teamUserPage.scheduled')}
            </div>

            {/* Seasons */}
            {seasons?.map((season) => {
               const seasonMatches = matches.filter(match => match.competitionId === season.id);
               
               // Filter out matches that are already shown in today's matches
               const todaysMatchIds = todaysMatches?.map(match => match.id) || [];
               const filteredSeasonMatches = seasonMatches.filter(match => !todaysMatchIds.includes(match.id));
               
               // Only render season if it has matches (excluding today's matches)
               if (filteredSeasonMatches.length === 0) return null;
               
               return (
                  <div key={season.id}>
                     <div
                        className="summary-season-header"
                        onClick={() => navigate(`/football/league/${season.id}`)}
                        role="button"
                        tabIndex={0}
                        onKeyDown={(e) => { if (e.key === 'Enter' || e.key === ' ') navigate(`/football/league/${season.id}`); }}
                     >
                        {season.name}
                     </div>

                     <div className="summary-season-container">
                        {filteredSeasonMatches.map((match) => (
                           <MatchRow
                              key={match.id}
                              id={match.id}
                              scheduledDateTime={match.scheduledDateTime}
                              homeTeamName={match.homeTeamName ?? 'TBD'}
                              awayTeamName={match.awayTeamName ?? 'TBD'}
                              homeTeamLogo={match.homeTeamLogo || undefined}
                              awayTeamLogo={match.awayTeamLogo || undefined}
                              homeScore={match.homeScore}
                              awayScore={match.awayScore}
                              periodCount={2}
                              periodScores={match.periodScores}
                              status={match.status}
                              onClick={() => handleMatchClick(match.id)}
                           />
                        ))}
                     </div>
                  </div>
               );
            })}
         </div>
         {/* Latest Matches Section - Only show if there are finished matches */}
         {(() => {
            const finishedMatches = matches.filter(match => match.status === 'Completed');
            if (finishedMatches.length === 0) return null;
            
            return (
               <div className="summary-container">
                  <div className="summary-header">
                     {t('teamUserPage.latestMatches')}
                  </div>
                  <div className="summary-season-container">
                     {finishedMatches
                        .sort((a, b) => new Date(b.scheduledDateTime).getTime() - new Date(a.scheduledDateTime).getTime())
                        .slice(0, 5)
                        .map((match) => (
                           <MatchRow
                              key={match.id}
                              id={match.id}
                              scheduledDateTime={match.scheduledDateTime}
                              homeTeamName={match.homeTeamName ?? 'TBD'}
                              awayTeamName={match.awayTeamName ?? 'TBD'}
                              homeTeamLogo={match.homeTeamLogo || undefined}
                              awayTeamLogo={match.awayTeamLogo || undefined}
                              homeScore={match.homeScore}
                              awayScore={match.awayScore}
                              periodCount={2}
                              periodScores={match.periodScores}
                              status={match.status}
                              onClick={() => handleMatchClick(match.id)}
                           />
                        ))}
                  </div>
               </div>
            );
         })()}
      </div>
   )
}