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

function selectUpcomingMatches(
   matches: FootballMatchDto[],
   todaysMatches: FootballMatchDto[] | null,
): FootballMatchDto[] {
   const now = Date.now();
   const todaysMatchIds = new Set(todaysMatches?.map((match) => match.id) ?? []);
   return matches.filter((match) => {
      if (todaysMatchIds.has(match.id) || match.status === 'Completed' || match.status === 'Cancelled') {
         return false;
      }
      if (match.status === 'InProgress') {
         return true;
      }
      return new Date(match.scheduledDateTime).getTime() >= now;
   });
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

   useEffect(() => {
      fetchSeasons();
      if (todaysMatches === null) {
         fetchTodaysMatches();
      }
   }, [todaysMatches, fetchTodaysMatches, fetchSeasons]);

   const hasTodaysMatches = (todaysMatches?.length ?? 0) > 0;
   const upcomingMatches = selectUpcomingMatches(matches, todaysMatches);
   const finishedMatches = matches
      .filter((match) => match.status === 'Completed')
      .sort((left, right) => new Date(right.scheduledDateTime).getTime() - new Date(left.scheduledDateTime).getTime())
      .slice(0, 5);

   return (
      <div>
         {(hasTodaysMatches || upcomingMatches.length > 0) && (
         <div className="summary-container">
            {hasTodaysMatches && todaysMatches && (
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
                                 />
                              ))}
                           </div>
                        </div>
                     );
                  })}
               </div>
            )}

            {upcomingMatches.length > 0 && (
               <>
            <div className="summary-header">
               {t('teamUserPage.scheduled')}
            </div>

            {seasons?.map((season) => {
               const filteredSeasonMatches = upcomingMatches.filter(match => match.competitionId === season.id);
               
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
                           />
                        ))}
                     </div>
                  </div>
               );
            })}
               </>
            )}
         </div>
         )}
         {finishedMatches.length > 0 && (
               <div className="summary-container">
                  <div className="summary-header">
                     {t('teamUserPage.latestMatches')}
                  </div>
                  <div className="summary-season-container">
                     {finishedMatches.map((match) => (
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
                           />
                        ))}
                  </div>
               </div>
         )}
      </div>
   )
}