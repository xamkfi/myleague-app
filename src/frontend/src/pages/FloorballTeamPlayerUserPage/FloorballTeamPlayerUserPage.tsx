import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { FloorballPosition } from "../../types/floorball/floorballTypes";
import { floorballStatisticsService, type FloorballPlayerSeasonStatisticsDto } from "../../api/floorball/floorballStatistics";
import PageTemplate from "../../components/PageTemplate/PageTemplate";
import './FloorballTeamPlayerUserPage.scss';

interface TeamCareerStats {
  teamId: string;
  teamName: string;
  seasonName: string;
  stats: FloorballPlayerSeasonStatisticsDto;
}

interface MatchPlayerStats {
  id: string;
  matchId: string;
  playerId: string;
  teamId: string;
  matchDate: string;
  opponent: string;
  competition: string;
  minutesPlayed: number;
  goals: number;
  assists: number;
  yellowCards: number;
  redCards: number;
  result: 'W' | 'L' | 'D';
  homeScore: number;
  awayScore: number;
}

interface PlayerWithMatches {
  id: string;
  playerName: string;
  position: FloorballPosition;
  jerseyNumber?: number;
  teamName: string;
  teamId: string;
  isActive: boolean;
  birthDateIso?: string;
  careerStats: TeamCareerStats[];
  recentMatches: MatchPlayerStats[];
}

const FloorballTeamPlayerUserPage = () => {
  const { id } = useParams<{ id: string }>();
  const [player, setPlayer] = useState<PlayerWithMatches | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Helper function to calculate total career stats
  const calculateTotalStats = (careerStats: TeamCareerStats[]): FloorballPlayerSeasonStatisticsDto => {
    return careerStats.reduce(
      (total, teamStats) => ({
        ...total,
        gamesPlayed: total.gamesPlayed + teamStats.stats.gamesPlayed,
        goals: total.goals + teamStats.stats.goals,
        assists: total.assists + teamStats.stats.assists,
        points: total.points + teamStats.stats.points,
        penaltyMinutes: total.penaltyMinutes + teamStats.stats.penaltyMinutes,
        plusMinusRating: total.plusMinusRating + teamStats.stats.plusMinusRating,
        shotsOnGoal: total.shotsOnGoal + teamStats.stats.shotsOnGoal,
        shotPercentage: total.shotPercentage + teamStats.stats.shotPercentage,
        powerPlayGoals: total.powerPlayGoals + teamStats.stats.powerPlayGoals,
        powerPlayAssists: total.powerPlayAssists + teamStats.stats.powerPlayAssists,
        shortHandedGoals: total.shortHandedGoals + teamStats.stats.shortHandedGoals,
        shortHandedAssists: total.shortHandedAssists + teamStats.stats.shortHandedAssists,
        gameWinningGoals: total.gameWinningGoals + teamStats.stats.gameWinningGoals,
        overtimeGoals: total.overtimeGoals + teamStats.stats.overtimeGoals,
        faceoffWins: total.faceoffWins + teamStats.stats.faceoffWins,
        faceoffAttempts: total.faceoffAttempts + teamStats.stats.faceoffAttempts,
        faceoffPercentage: total.faceoffPercentage + teamStats.stats.faceoffPercentage,
      }),
      {
        id: "",
        playerId: "",
        teamId: "",
        seasonId: "",
        playerName: "",
        teamName: "",
        teamLogo: null,
        seasonName: "",
        gamesPlayed: 0,
        goals: 0,
        assists: 0,
        points: 0,
        penaltyMinutes: 0,
        plusMinusRating: 0,
        shotsOnGoal: 0,
        shotPercentage: 0,
        powerPlayGoals: 0,
        powerPlayAssists: 0,
        shortHandedGoals: 0,
        shortHandedAssists: 0,
        gameWinningGoals: 0,
        overtimeGoals: 0,
        faceoffWins: 0,
        faceoffAttempts: 0,
        faceoffPercentage: 0
      }
    );
  };

  useEffect(() => {
    const loadPlayerData = async () => {
      if (!id) return;
      
      try {
        setLoading(true);
        setError(null);
        
        // Fetch full player profile with career statistics
        const profile = await floorballStatisticsService.getPlayerProfile(id);

        // Map career player statistics into UI structure
        const careerStats: TeamCareerStats[] = (profile.seasonStatistics || []).map(s => ({
          teamId: s.teamId,
          teamName: s.teamName,
          seasonName: s.seasonName,
          stats: s
        }));
        
        // For now, use empty array for recent matches since we don't have match-specific player stats API
        const recentMatches: MatchPlayerStats[] = [];
        
        const transformedData: PlayerWithMatches = {
          id: profile.player.id,
          playerName: profile.player.person.fullName,
          position: profile.player.position,
          jerseyNumber: undefined,
          birthDateIso: profile.player.person.birthDate,
          teamName: careerStats[0]?.teamName ?? 'Ei joukkuetta',
          teamId: careerStats[0]?.teamId ?? '',
          isActive: profile.player.isActive,
          careerStats,
          recentMatches
        };
        
        setPlayer(transformedData);
      } catch (err) {
        console.error('Error loading player data:', err);
        setError(err instanceof Error ? err.message : 'An error occurred while loading player data');
      } finally {
        setLoading(false);
      }
    };

    loadPlayerData();
  }, [id]);

  const formatDate = (isoDate?: string) => {
    if (!isoDate) return '';
    const d = new Date(isoDate);
    const dd = String(d.getUTCDate()).padStart(2, '0');
    const mm = String(d.getUTCMonth() + 1).padStart(2, '0');
    const yyyy = d.getUTCFullYear();
    return `${dd}/${mm}/${yyyy}`;
  };

  const calculateAgeFrom = (isoDate?: string) => {
    if (!isoDate) return undefined;
    const birth = new Date(isoDate);
    const today = new Date();
    let age = today.getFullYear() - birth.getFullYear();
    const m = today.getMonth() - birth.getMonth();
    if (m < 0 || (m === 0 && today.getDate() < birth.getDate())) age--;
    return age;
  };

  const getPositionText = (position: FloorballPosition) => {
    switch (position) {
      case FloorballPosition.Goalkeeper:
        return 'Maalivahti';
      case FloorballPosition.Defender:
        return 'Puolustaja';
      case FloorballPosition.Forward:
        return 'Hyökkääjä';
      default:
        return 'Pelaaja';
    }
  };

  if (loading) return <PageTemplate title="Pelaaja"><div>Ladataan...</div></PageTemplate>;
  if (error) return <PageTemplate title="Pelaaja"><div>Virhe: {error}</div></PageTemplate>;
  if (!player) return <PageTemplate title="Pelaaja"><div>Pelaajaa ei löytynyt</div></PageTemplate>;

  const totalStats = calculateTotalStats(player.careerStats);

  return (
    <PageTemplate title={player.playerName}>
      <div>
        {/* Player Header Section */}
        <div className="player-container">
          <div className="player-info-layout">
            <div className="player-info-box">
              <div className="player-avatar-large">
                {player.careerStats[0]?.stats.teamLogo && (
                  <img
                    className="team-logo-img"
                    src={player.careerStats[0].stats.teamLogo}
                    alt={player.teamName}
                  />
                )}
              </div>
              <div className="player-details">
                <div className="player-name">{player.playerName}</div>
                <div className="player-details-row">
                  <span className="player-team">{player.teamName !== 'Ei joukkuetta' ? player.teamName : 'Joukkuetieto ei saatavilla'}</span>
                  <span className="player-position">{getPositionText(player.position)}</span>
                  <span className="player-jersey">{player.jerseyNumber ? `#${player.jerseyNumber}` : ''}</span>
                </div>
              </div>
            </div>
            
            <div className="player-stats-box">
              <div className="stat-item">
                <span className="stat-label">Age:</span>
                <span className="stat-value">{`${calculateAgeFrom(player.birthDateIso) ?? '-'}`}{player.birthDateIso ? ` (${formatDate(player.birthDateIso)})` : ''}</span>
              </div>
              <div className="stat-item">
                <span className="stat-label">Status:</span>
                <span className={`stat-value ${player.isActive ? 'active' : 'inactive'}`}>
                  {player.isActive ? 'active' : 'inactive'}
                </span>
              </div>
              <div className="stat-item">
                <span className="stat-label">Joined:</span>
                <span className="stat-value">07/07/2020</span>
              </div>
            </div>
          </div>
        </div>

        {/* Career Statistics Section */}
        <div className="player-container">
          <div className="career-stats-section">
            <h3>Urastatistiikka</h3>
            
            {/* Show aggregated totals */}
            <div className="stats-grid">
              <div className="stats-box">
                <div className="stats-value">{totalStats.gamesPlayed}</div>
                <div className="stats-label">Ottelut</div>
              </div>
              <div className="stats-box">
                <div className="stats-value">{totalStats.goals}</div>
                <div className="stats-label">Maalit</div>
              </div>
              <div className="stats-box">
                <div className="stats-value">{totalStats.assists}</div>
                <div className="stats-label">Syötöt</div>
              </div>
              <div className="stats-box">
                <div className="stats-value">{totalStats.points}</div>
                <div className="stats-label">Pisteet</div>
              </div>
              <div className="stats-box">
                <div className="stats-value">{totalStats.penaltyMinutes}</div>
                <div className="stats-label">Jäähy min</div>
              </div>
            </div>

            {/* Show team-specific breakdown if player has played for multiple teams */}
            {player.careerStats.length > 1 && (
              <div className="team-stats-breakdown">
                <h4>Joukkuekohtaiset tilastot</h4>
                {player.careerStats.map(teamStats => (
                  <div key={teamStats.teamId} className="team-stats">
                    <h5>{teamStats.teamName !== 'Ei joukkuetta' ? teamStats.teamName : 'Joukkuetieto ei saatavilla'}</h5>
                    <div className="team-stats-grid">
                      <span>{teamStats.stats.gamesPlayed} ottelua</span>
                      <span>{teamStats.stats.goals} maalia</span>
                      <span>{teamStats.stats.assists} syöttöä</span>
                      <span>{teamStats.stats.points} pistettä</span>
                      <span>{teamStats.stats.penaltyMinutes} jäähy min</span>
                    </div>
                  </div>
                ))}
              </div>
            )}
            {/*Tilastot haetaan joukkueiden roster-tiedoista. Ottelukohtaiset pelaajatilastot eivät ole saatavilla nykyisen rajapinnan kautta.*/}
          </div>
        </div>

        {/* Latest Matches Section */}
        <div className="player-container">
          <div className="latest-matches-section">
            <h3>LATEST MATCHES</h3>
            {player.recentMatches.length > 0 ? (
              <div className="matches-table-container">
                <div className="matches-table-header">
                  <div className="header-item">DATE</div>
                  <div className="header-item">COMPETITION</div>
                  <div className="header-item">TEAMS</div>
                  <div className="header-item">MIN</div>
                  <div className="header-item">G</div>
                  <div className="header-item">A</div>
                  <div className="header-item">YC</div>
                  <div className="header-item">RC</div>
                  <div className="header-item">RESULT</div>
                </div>
                <div className="matches-table-body">
                  {player.recentMatches.map(match => (
                    <div key={match.id} className="match-row">
                      <div className="match-cell">{match.matchDate}</div>
                      <div className="match-cell">{match.competition}</div>
                      <div className="match-cell teams-cell">
                        <div className="match-row-home-team">
                          <span className="team-name">FC Alapiha {match.homeScore}</span>
                        </div>
                        <div className="vs-separator">-</div>
                        <div className="match-row-away-team">
                          <span className="team-name">{match.opponent} {match.awayScore}</span>
                        </div>
                      </div>
                      <div className="match-cell">{match.minutesPlayed}</div>
                      <div className="match-cell">{match.goals}</div>
                      <div className="match-cell">{match.assists}</div>
                      <div className="match-cell">{match.yellowCards}</div>
                      <div className="match-cell">{match.redCards}</div>
                      <div className="match-cell">
                        <div className={`result-indicator ${match.result.toLowerCase()}`}>
                          {match.result}
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            ) : (
              <div className="no-matches-message">
                <p>No recent matches available for this player.</p>
              </div>
            )}
          </div>
        </div>

        {/* Career Section */}
        <div className="player-container">
          <div className="career-section">
            <h3>CAREER</h3>
            <div className="career-table-container">
              <div className="career-table-header">
                <div className="header-item">SEASON</div>
                <div className="header-item">TEAM</div>
                <div className="header-item">COMPETITION</div>
                <div className="header-item">GP</div>
                <div className="header-item">G</div>
                <div className="header-item">A</div>
                <div className="header-item">YC</div>
                <div className="header-item">RC</div>
              </div>
              <div className="career-table-body">
                {player.careerStats.map(teamStats => (
                  <div key={teamStats.teamId} className="career-row">
                    <div className="career-cell">{teamStats.seasonName}</div>
                    <div className="career-cell">
                      <div className="match-row-home-team">
                        {teamStats.stats.teamLogo && (
                          <img
                            src={teamStats.stats.teamLogo}
                            alt={`${teamStats.teamName} logo`}
                          />
                        )}
                        <span className="team-name">{teamStats.teamName}</span>
                      </div>
                    </div>
                    <div className="career-cell">Rautaliiga</div>
                    <div className="career-cell">{teamStats.stats.gamesPlayed}</div>
                    <div className="career-cell">{teamStats.stats.goals}</div>
                    <div className="career-cell">{teamStats.stats.assists}</div>
                    <div className="career-cell">{teamStats.stats.penaltyMinutes}</div>
                    <div className="career-cell">0</div>
                  </div>
                ))}
                <div className="career-row total-row">
                  <div className="career-cell">TOTAL</div>
                  <div className="career-cell">-</div>
                  <div className="career-cell">-</div>
                  <div className="career-cell">{totalStats.gamesPlayed}</div>
                  <div className="career-cell">{totalStats.goals}</div>
                  <div className="career-cell">{totalStats.assists}</div>
                  <div className="career-cell">{totalStats.penaltyMinutes}</div>
                  <div className="career-cell">0</div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </PageTemplate>
  );
};

export default FloorballTeamPlayerUserPage;
