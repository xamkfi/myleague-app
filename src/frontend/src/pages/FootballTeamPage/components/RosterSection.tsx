import { useEffect, useState } from "react"
import { useTranslation } from "react-i18next"
import './RosterSection.scss'
import type { FootballTeam, FootballTeamPlayer } from "../../../types/football/footballTypes"
import type { FootballPlayerSeasonStatisticsDto } from "../../../api/football/footballStatistics"
import { useNavigate } from "react-router-dom"

interface RosterSectionProps {
  team: FootballTeam;
  playerStatistics?: FootballPlayerSeasonStatisticsDto[] | null;
}

export default function RosterSection({ team, playerStatistics }: RosterSectionProps) {
  const [roster, setRoster] = useState<FootballTeamPlayer[]>([])
  const navigate = useNavigate()
  const { t } = useTranslation()

  const navigateToPlayerPage = (playerId: string) => {
    navigate(`/football/player/${playerId}`)
  }

  useEffect(() => {
    if (team.roster.length > 0) {
      setRoster(team.roster)
    }
  }, [team.roster])

  const statsLookup = new Map(
    (playerStatistics ?? []).map(ps => [ps.playerId, ps])
  );

  const positionOrder = ['Goalkeeper', 'Defender', 'Midfielder', 'Forward']
  const playerPositions = [...new Set(roster.map(p => p.position))]
    .sort((a, b) => positionOrder.indexOf(a) - positionOrder.indexOf(b))

  return (
    <div className="roster-section">

      {/* Playing positions */}
      {playerPositions.map((pos, key) => (
        <div className="roster-container" key={key}>
          <div className="roster-position-header">
            {t(`roster.positions.${pos}`)}
          </div>

          <div className="roster-position-container">
            <div className="table-wrapper">
              <div className="table stats-header ">
                <div className="roster-jersey" title={t('roster.tooltips.jerseyNumber')}>{t('roster.jerseyNumber')}</div>
                <div className="roster-player-name">{t('roster.name')}</div>
                <div className="roster-age" title={t('roster.tooltips.age')}>{t('roster.age')}</div>
                <div className="roster-games-played" title={t('roster.tooltips.matchesPlayed')}>{t('roster.matchesPlayed')}</div>
                <div className="roster-goals" title={t('roster.tooltips.goals')}>{t('roster.goals')}</div>
                <div className="roster-assists" title={t('roster.tooltips.assists')}>{t('roster.assists')}</div>
              </div>
              {roster
                .filter(player => player.position === pos)
                .map((player) => {
                  const isCreator = player.playerName === 'Tuomas Reijonen';
                  const seasonStats = statsLookup.get(player.playerId);
                  return (
                  <div
                    className={`table roster-player${isCreator ? ' roster-player--creator' : ''}`}
                    onClick={() => navigateToPlayerPage(player.playerId)}
                    key={player.playerId}
                  >
                    
                    <div className="roster-jersey row">
                      {player.jerseyNumber}
                    </div>
                    
                    <div className="roster-player-name">
                      {isCreator ? (
                        <><span className="creator-badge" title="System Creator">💪</span> {player.playerName} <span className="creator-badge" title="System Creator">💪</span></>
                      ) : player.playerName}
                    </div>
                    
                    <div className="roster-age">
                      {player.age && player.age !== 99 ? player.age : "-"}
                    </div>
                    
                    <div className="roster-games-played">
                      {seasonStats?.gamesPlayed ?? player.gamesPlayed}
                    </div>
                    
                    <div className="roster-goals">
                      {seasonStats?.goals ?? player.goals ?? "-"}
                    </div>
                    
                    <div className="roster-assists">
                      {seasonStats?.assists ?? player.assists ?? "-"}
                    </div>
                    
                  </div>
                  );
                })}
            </div>
          </div>
        </div>
      ))}
    </div>
  )
}