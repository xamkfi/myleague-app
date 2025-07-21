import { useEffect, useState } from "react"
import { useTranslation } from "react-i18next"
import './SummarySection.scss'
import type { FloorballMatchDto, FloorballTeam } from "../../../types/floorball/floorballTypes"
import { floorballSeasonService, type FloorballSeasonDto } from "../../../api/floorball/floorballSeasonService"
import { formatMatchDateTime } from "../../../utils/dateUtils"
import ResultUnknown from "../../../components/MatchResultIcons/ResultUnknown"

interface SummarySectionProps {
   team: FloorballTeam
   matches: FloorballMatchDto[]
   
}

export default function SummarySection({ team, matches }: SummarySectionProps) {
   const { t } = useTranslation();
   const [seasons, setSeasons] = useState<FloorballSeasonDto[] | null>(null);


   const fetchSeasons = async () => {
      try {
         const seasonsResponse = await floorballSeasonService.getAll();
         setSeasons(seasonsResponse.data);
      } catch {
         setSeasons([]); // fallback
      }
   }

   const periodAmnt = [1,2,3]


   useEffect(() => {
      fetchSeasons();
   }, []);

   return (
      <div className="summary-container">

         {/* Playing positions */}
         {seasons?.map((season, key) => (
            <div key={key}>
               <div className="summary-season-header-today">
                     {t('teamUserPage.todaysMatches')}
               </div>
               <div className="summary-season-header">
                  {season.name}
               </div>

               <div className="summary-season-container">
                  
                  {matches
                     .filter(match => match.seasonId === season.id)
                     .map((match) => {
                        const [formattedDate, formattedTime] = formatMatchDateTime(match.scheduledDateTime);
                        return (
                           <div
                              className="summary-table summary-match"
                              //onClick={() => navigateToMatchPage(match.id)}
                           >

                              {/* Date */}
                              <div className="summary-date">
                                 <div className="summary-date-day">{formattedDate}</div>
                                 <div className="summary-date-time">{formattedTime}</div>
                              </div>

                              {/* Teams */}
                              <div className="summary-teams-container">
                           
                                 <div className="summary-home-team">
                                    <img src="http://www.mahl.fi/media/com_joomleague/clubs/small/myry21_1683621904.jpg" alt="?" />
                                    {match.homeTeamName}
                                 </div>

                                 <div className="summary-away-team">
                                    <img src="http://www.mahl.fi/media/com_joomleague/clubs/small/knp_21_1715843664.jpg" alt="?" />
                                    {match.awayTeamName}
                                 </div>
                              </div>

                              {/* Total score */}
                              <div className="summary-total-score-container">
                                 <div className="summary-home-total-score">
                                    -
                                 </div>

                                 <div className="summary-away-total-score">
                                    -
                                 </div>
                              </div>

                              {/* Period score */}
                              <div className="summary-period-score-container">
                                 {periodAmnt.map((period, key) => (
                                    <div key={key}>
                                       <div className="summary-home-period-score">
                                          {match.homeScore !== 0 ? match.homeScore : ''}
                                       </div>
                                       <div className="summary-away-period-score">
                                          {match.awayScore !== 0 ? match.awayScore : ''}
                                       </div>
                                    </div>
                                 ))}

                              </div>

                              {/* Match status */}
                              <div className="summary-match-status seperator-line">
                                 <ResultUnknown />
                              </div>
                           </div>
                        );
                     })}
               </div>
            </div>
         ))}

      </div>
   )
}