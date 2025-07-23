import { useEffect, useState } from "react"
import { useTranslation } from "react-i18next"
import './RosterSection.scss'
import type { FloorballTeam, FloorballTeamPlayer } from "../../../types/floorball/floorballTypes"
import { useNavigate } from "react-router-dom"

interface RosterSectionProps {
  team: FloorballTeam
}

export default function RosterSection({ team }: RosterSectionProps) {
  const [roster, setRoster] = useState<FloorballTeamPlayer[]>([])
  const navigate = useNavigate()
  const { t } = useTranslation()

  const navigateToPlayerPage = (playerId: string) => {
    navigate(`/pelaaja/${playerId}`)
  }

  useEffect(() => {
    if (team.roster.length > 0) {
      setRoster(team.roster)
    }
  }, [team.roster])

  // Get unique positions and sort them in the correct order
  const positionOrder = ['Goalkeeper', 'Defender', 'Center', 'Forward']
  const playerPositions = [...new Set(roster.map(p => p.position))]
    .sort((a, b) => positionOrder.indexOf(a) - positionOrder.indexOf(b))

  return (
    <div className="roster-section">

      {/* Playing positions */}
      {playerPositions.map((pos, key) => (
        <div className="roster-container">
          <div key={key}>
            <div className="roster-position-header">
              {t(`roster.positions.${pos}`)}
            </div>

            <div className="roster-position-container">
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
                .map((player) =>
                  <div
                    className="table roster-player"
                    onClick={() => navigateToPlayerPage(player.playerId)}
                  >

                    <div className="roster-jersey row">
                      {player.jerseyNumber}
                    </div>

                    <div className="roster-player-name">
                      {player.playerName}
                    </div>

                    <div className="roster-age">
                      {player.age ?? 99}
                    </div>

                    <div className="roster-games-played">
                      {player.gamesPlayed}
                    </div>

                    <div className="roster-goals">
                      {player.goals ?? "-"}
                    </div>

                    <div className="roster-assists">
                      {player.assists ?? "-"}
                    </div>

                  </div>
                )}
            </div>
          </div>
        </div>
      ))}
    </div>
  )
}