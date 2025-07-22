import { useEffect, useState } from "react";
import { floorballTeamService } from "../../../api/floorball/floorballTeamService";
import type { FloorballMatchDto, FloorballTeamPlayer } from "../../../types/floorball/floorballTypes";
import './MatchLineups.scss';

export default function MatchLineups({match}: {match: FloorballMatchDto}) {

    const [homeRoster, setHomeRoster] = useState<FloorballTeamPlayer[]>([]);
    const [awayRoster, setAwayRoster] = useState<FloorballTeamPlayer[]>([]);
    useEffect(() => {
        async function fetchLineups() {
            const response = await floorballTeamService.getById(match.homeTeamId);
            const response2 = await floorballTeamService.getById(match.awayTeamId);

            console.log(response);
            console.log(response2);
            setHomeRoster(response.roster);
            setAwayRoster(response2.roster);
        }
        fetchLineups();
    }, [match.homeTeamId, match.awayTeamId]);

    const sortedHomeRoster = [...homeRoster].sort((a, b) => (a.jerseyNumber || 0) - (b.jerseyNumber || 0));
    const sortedAwayRoster = [...awayRoster].sort((a, b) => (a.jerseyNumber || 0) - (b.jerseyNumber || 0));

  return (
    <div className="match-lineups">
      <div className="lineups-header">
        <h2>Match Lineups</h2>
      </div>
      
      <div className="teams-container">
        {/* Home Team */}
        <div className="team-side home-team">
          <div className="team-header">
            <h3 className="team-name">{match.homeTeamName}</h3>
            <span className="team-label">HOME</span>
          </div>
          <div className="players-list">
            {sortedHomeRoster.map((player) => (
              <div key={player.playerId} className="player-row">
                <span className="player-position">{player.position}</span>
                <span className="player-name">{player.playerName}</span>
                <span className="player-number">{player.jerseyNumber}</span>
              </div>
            ))}
          </div>
        </div>

        {/* Away Team */}
        <div className="team-side away-team">
          <div className="team-header">
            <h3 className="team-name">{match.awayTeamName}</h3>
            <span className="team-label">AWAY</span>
          </div>
          <div className="players-list">
            {sortedAwayRoster.map((player) => (
              <div key={player.playerId} className="player-row">
                <span className="player-position">{player.position}</span>
                <span className="player-name">{player.playerName}</span>
                <span className="player-number">{player.jerseyNumber}</span>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}