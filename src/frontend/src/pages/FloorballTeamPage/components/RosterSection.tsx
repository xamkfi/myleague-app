import { useEffect, useState, useCallback } from "react"
import './RosterSection.scss'
import type { FloorballTeam, FloorballTeamPlayer } from "../../../types/floorball/floorballTypes"
import { useNavigate } from "react-router-dom"

interface RosterSectionProps {
  team: FloorballTeam
}

export default function RosterSection({ team }: RosterSectionProps) {
  const [roster, setRoster] = useState<FloorballTeamPlayer[]>([])
  const [playerPositions, setPlayerPositions] = useState<string[]>([])
  const navigate = useNavigate()

  const navigateToPlayerPage = (playerId: string) => {
    navigate(`/pelaaja/${playerId}`)
  }

  const filterPlayerPositions = useCallback(() => {
    const posList = roster.map(p => p.position)
    return [...new Set(posList)]
  }, [roster])

  useEffect(() => {
    setRoster(team.roster)
  }, [team])

  useEffect(() => {
    if (playerPositions.length < 1) {
      setPlayerPositions(filterPlayerPositions())
    }
  }, [playerPositions, filterPlayerPositions])

  return (
    <div>
      
       {/* Playing positions */}
         {playerPositions.map((pos, key) => (
          <div key={key}>
            <div className="roster-position-header">
              {pos}
            </div>
            
            <div className="roster-position-container">
              <div className="table stats-header ">
                <div className="roster-jersey" title="Jersey number">#</div>
                <div className="roster-player-name">Name</div>
                <div className="roster-games-played" title="Matches played">MP</div>
                <div className="roster-goals" title="Goals">G</div>
                <div className="roster-assists" title="Assists">A</div>
              </div>
              {roster
                .filter(player => player.position === pos)
                .map((player) => 
                <div 
                  className="table roster-player" 
                  onClick={() => navigateToPlayerPage(player.playerId)}
                >

                  <div className="roster-jersey">
                    {player.jerseyNumber}
                  </div>

                  <div className="roster-player-name">
                    {player.playerName}
                  </div>                   

                  <div className="roster-games-played seperator-line">
                    {player.gamesPlayed}
                  </div>

                  <div className="roster-goals">
                    {player.goals}
                  </div>

                  <div className="roster-assists">
                    {player.assists}
                  </div>

                </div>
              )}
            </div>
          </div>
        ))}

    </div>
  )
}