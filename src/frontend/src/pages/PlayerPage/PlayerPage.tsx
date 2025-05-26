import React, { useEffect, useState } from "react";
import type { Player, TeamsData, Goalkeeper, FieldPlayer } from "../../types/playerTypes";
import teamsData from "./testdata.json";
import styles from "./PlayerPage.module.css";

interface PlayerPageProps {
  playerId: number;
}

const PlayerPage: React.FC<PlayerPageProps> = ({ playerId }) => {
  const [player, setPlayer] = useState<Player | null>(null);

  //fetch(`/api/player/${playerId}`).then(...)

  useEffect(() => {
    const data = teamsData as unknown as TeamsData;
    const team = data.Teams.find(t =>
      [...t.Players.Goalkeepers, ...t.Players.Fieldplayers].some(p => p.Id === playerId)
    );
    if (!team) return;

    const allPlayers = [...team.Players.Goalkeepers, ...team.Players.Fieldplayers];
    const found = allPlayers.find(p => p.Id === playerId);

    if (found) {
      setPlayer({ ...found, teamName: team.Name });
    }
  }, [playerId]);

  if (!player) return <div className={styles.container}>Ladataan...</div>;

  const isGoalkeeper = (p: Player): p is Goalkeeper & { teamName?: string } =>
    "SavePercentage" in p;

  return (
    <div className={styles.container}>
      <div className={styles.header}>
        <div className={styles.avatar}></div>
        <div className={styles.playerInfo}>
          <div className={styles.name}>{player.Name}</div>
          <div className={styles.subtitle}>Joukkue: {player.teamName}</div>
          <div className={styles.subtitle}>Ikä: {player.Age}</div>
        </div>
      </div>

      <div className={styles.statsGrid}>
        <div className={styles.statBox}>
          <div className={styles.label}>Ottelut</div>
          <div className={styles.value}>{player.MatchesPlayed}</div>
        </div>

        {isGoalkeeper(player) ? (
          <>
            <div className={styles.statBox}>
              <div className={styles.label}>Torjuntaprosentti</div>
              <div className={styles.value}>{player.SavePercentage}%</div>
            </div>
            <div className={styles.statBox}>
              <div className={styles.label}>Päästetyt maalit / ottelu</div>
              <div className={styles.value}>{player.GoalsAgainstAverage}</div>
            </div>
            <div className={styles.statBox}>
              <div className={styles.label}>Nollapelit</div>
              <div className={styles.value}>{player.ShutOuts}</div>
            </div>
          </>
        ) : (
          <>
            <div className={styles.statBox}>
              <div className={styles.label}>Maalit</div>
              <div className={styles.value}>{(player as FieldPlayer).GoalsScored}</div>
            </div>
            <div className={styles.statBox}>
              <div className={styles.label}>Syötöt</div>
              <div className={styles.value}>{(player as FieldPlayer).Assists}</div>
            </div>
            <div className={styles.statBox}>
              <div className={styles.label}>Pisteet</div>
              <div className={styles.value}>{(player as FieldPlayer).Points}</div>
            </div>
          </>
        )}
      </div>

      <div className={styles.matchHistory}>
        <h3>Otteluhistoria</h3>
        {/* Placeholder: voit lisätä backendistä tai mock-datasta myöhemmin */}
        <div className={styles.matchItem}>
          <span>12.05.25 FC Ankkalinna 3 - 2 Vastustaja</span>
          <span className={styles.win}>W</span>
        </div>
        <div className={styles.matchItem}>
          <span>10.05.25 FC Ankkalinna 1 - 4 Vastustaja</span>
          <span className={styles.loss}>L</span>
        </div>
      </div>
    </div>
  );
};

export default PlayerPage;
