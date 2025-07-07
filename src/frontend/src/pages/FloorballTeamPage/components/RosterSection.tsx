import { useEffect, useState } from "react"
import './RosterSection.scss'
import type { FloorballPlayerDto } from "../../../api/floorball/floorballPlayerService"
import type { FloorballTeam, FloorballTeamPlayer } from "../../../types/floorball/floorballTypes"

interface RosterSectionProps {
  team: FloorballTeam
}

export default function RosterSection({ team }: RosterSectionProps) {
  const [roster, setRoster] = useState<FloorballTeamPlayer[]>([])
  const [playerPositions, setPlayerPositions] = useState<string[]>([])

  const shortenName = (name: string): string => {
    const maxLenght: number = 30
    if(name.length > maxLenght){
      let modifiedName: string = name.slice(0, maxLenght) + "..."
      return modifiedName
    }
    return name
  }

  const filterPlayerPositions = () => {
    const posList = roster.map(p => p.position)
    return [...new Set(posList)]
  }

  useEffect(() => {
    setRoster(team.roster)
  }, [])

  useEffect(() => {
    if (playerPositions.length < 1) {
      setPlayerPositions(filterPlayerPositions())
      console.log(playerPositions)

    }
    console.log(playerPositions)

  }, [playerPositions])

  return (
    <div>
      
       {/* Playing positions */}
         {playerPositions.map((pos, key) => (
          <div>
            
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
                <div className="table roster-player">

                  <div className="roster-jersey">
                    {player.jerseyNumber}
                  </div>

                  <div className="roster-player-name">
                    {player.playerName}
                  </div>                   

                  <div className="roster-games-played">
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


          // {roster
          //   .filter(player => player.position === pos)
          //   .map((player) => (
          //     <tr key={player.playerId}>
          //       <td>{player.jerseyNumber}</td>
          //       <td>{player.playerName}</td>
          //       <td>{player.position}</td>
          //       <td></td>
          //     </tr>
          // ))}


      //  {/* Playing positions */}
      //    {playerPositions.map((pos, key) => (
      //     <div className="roster-container">
      //       <h1 className="roster-position-header">{pos}</h1>
      //       <div className="roster-position-container" key={key}>
              

      //         <div>
      //           <div className="stats-header">
      //             #
      //           </div>

      //           {roster.map((player, key) => {
      //             if (player.position == pos)
      //               return(
      //                 <div>{player.jerseyNumber}</div>
      //               )
      //           })}
      //         </div>
              
      //         {/* Player name */}
      //         <div className="roster-player">

      //           <div className="stats-header">
      //             Player
      //           </div>
      //           {roster.map((player, key) => {
      //             if (player.position == pos)
      //               return(
      //                 <div>{player.playerName}</div>
      //               )
      //           })}
      //         </div>
              
      //         {/* Matches */}
      //         <div>
      //           <div className="stats-header">
      //             Matches
      //           </div>

      //           {roster.map((player, key) => {
      //             if (player.position == pos)
      //               return(
      //                 <div>{player.gamesPlayed}</div>
      //               )
      //           })}
      //         </div>

      //         {/* Goals */}
      //         <div>
      //           <div className="stats-header">
      //             Goals
      //           </div>

      //           {roster.map((player, key) => {
      //             if (player.position == pos)
      //               return(
      //                 <div>{player.goals}</div>
      //               )
      //           })}
      //         </div>

      //         {/* Assists */}
      //         <div>
      //           <div className="stats-header">
      //             Assists
      //           </div>
                
      //           {roster.map((player, key) => {
      //             if (player.position == pos)
      //               return(
      //                 <div>{player.assists}</div>
      //               )
      //           })}
      //         </div>

      //       </div>
      //     </div>
      //   ))}