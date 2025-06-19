import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import type { Player, TeamsData, Goalkeeper, FieldPlayer } from "../../types/playerTypes";
import teamsData from "./testdata.json";
import PageTemplate from "../../components/PageTemplate/PageTemplate";
import './PlayerPage.scss';

const PlayerPage = () => {
  const { id } = useParams<{ id: string }>();
  const [player, setPlayer] = useState<Player | null>(null);

  //fetch(`/api/player/${playerId}`).then(...)

  useEffect(() => {
    if (!id) return;
    
    const data = teamsData as unknown as TeamsData;
    const team = data.Teams.find(t =>
      [...t.Players.Goalkeepers, ...t.Players.Fieldplayers].some(p => p.Id === id)
    );
    if (!team) return;

    const allPlayers = [...team.Players.Goalkeepers, ...team.Players.Fieldplayers];
    const found = allPlayers.find(p => p.Id === id);

    if (found) {
      setPlayer({ ...found, teamName: team.Name });
    }
  }, [id]);

  if (!player) return <PageTemplate title="Pelaaja"><div>Ladataan...</div></PageTemplate>;

  const isGoalkeeper = (p: Player): p is Goalkeeper & { teamName?: string } =>
    "SavePercentage" in p;

  return (
    <PageTemplate title={player.Name}>
      <div className="player-container">
        <div className="player-header">
          <div className="player-avatar"></div>
          <div className="player-info">
            <div className="player-name">{player.Name}</div>
            <div className="player-subtitle">Joukkue: {player.teamName}</div>
            <div className="player-subtitle">Ikä: {player.Age}</div>
          </div>
        </div>

        <div className="stats-grid">
          <div className="stats-box">
            <div className="stats-label">Ottelut</div>
            <div className="stats-value">{player.MatchesPlayed}</div>
          </div>

          {isGoalkeeper(player) ? (
            <>
              <div className="stats-box">
                <div className="stats-label">Torjuntaprosentti</div>
                <div className="stats-value">{player.SavePercentage}%</div>
              </div>
              <div className="stats-box">
                <div className="stats-label">Päästetyt maalit / ottelu</div>
                <div className="stats-value">{player.GoalsAgainstAverage}</div>
              </div>
              <div className="stats-box">
                <div className="stats-label">Nollapelit</div>
                <div className="stats-value">{player.ShutOuts}</div>
              </div>
            </>
          ) : (
            <>
              <div className="stats-box">
                <div className="stats-label">Maalit</div>
                <div className="stats-value">{(player as FieldPlayer).GoalsScored}</div>
              </div>
              <div className="stats-box">
                <div className="stats-label">Syötöt</div>
                <div className="stats-value">{(player as FieldPlayer).Assists}</div>
              </div>
              <div className="stats-box">
                <div className="stats-label">Pisteet</div>
                <div className="stats-value">{(player as FieldPlayer).Points}</div>
              </div>
            </>
          )}
        </div>

        <div className="match-history">
          <h3>Otteluhistoria</h3>
          {/* Placeholder: voit lisätä backendistä tai mock-datasta myöhemmin */}
          <div className="match-item">
            <span>12.05.25 FC Ankkalinna 3 - 2 Vastustaja</span>
            <span className="win">W</span>
          </div>
          <div className="match-item">
            <span>10.05.25 FC Ankkalinna 1 - 4 Vastustaja</span>
            <span className="loss">L</span>
          </div>
        </div>
      </div>
    </PageTemplate>
  );
};

export default PlayerPage;
