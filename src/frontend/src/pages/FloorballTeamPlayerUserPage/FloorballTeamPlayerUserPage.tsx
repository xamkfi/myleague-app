import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { FloorballPosition, FloorballMatchStatus } from "../../types/floorball/floorballTypes";
import { floorballPlayerService } from "../../api/floorball/floorballPlayerService";
import { floorballTeamService } from "../../api/floorball/floorballTeamService";
import { floorballMatchService } from "../../api/floorball/floorballMatchService";
import PageTemplate from "../../components/PageTemplate/PageTemplate";
import './FloorballTeamPlayerUserPage.scss';

interface FloorballMatch {
  id: string;
  seasonId: string;
  homeTeamId: string;
  homeTeamName: string;
  awayTeamId: string;
  awayTeamName: string;
  scheduledDateTime: string;
  venue?: string;
  status: 'scheduled' | 'in_progress' | 'completed' | 'cancelled';
  homeScore: number;
  awayScore: number;
  wentToOvertime: boolean;
  wentToShootout: boolean;
  periodScores: Record<string, { homeScore: number; awayScore: number }>;
  officials: string[];
  playerStats?: {
    goals: number;
    assists: number;
    penaltyMinutes: number;
    playedMinutes: number;
  };
}

interface PlayerStats {
  gamesPlayed: number;
  goals: number;
  assists: number;
  points: number;
  penaltyMinutes: number;
}

interface TeamCareerStats {
  teamId: string;
  teamName: string;
  stats: PlayerStats;
}

interface PlayerWithMatches {
  id: string;
  playerName: string;
  position: FloorballPosition;
  jerseyNumber?: number;
  teamName: string;
  teamId: string;
  isActive: boolean;
  careerStats: TeamCareerStats[];
  recentMatches: FloorballMatch[];
}

// Enhanced API function to fetch comprehensive player data
const fetchPlayerData = async (playerId: string): Promise<PlayerWithMatches | null> => {
  try {
    // Fetch basic player data
    const playerData = await floorballPlayerService.getById(playerId);
    
    // Fetch all teams to find which team(s) this player belongs to
    const teamsResponse = await floorballTeamService.getAll({ pageSize: 100 });
    const allTeams = teamsResponse.data || [];
    
    // Find teams where this player is in the roster
    const playerTeams = allTeams.filter(team => 
      team.roster.some(rosterPlayer => rosterPlayer.playerId === playerId)
    );
    
    // Build career stats from roster data
    const careerStats = playerTeams.map(team => {
      const playerInTeam = team.roster.find(rosterPlayer => rosterPlayer.playerId === playerId);
      return {
        teamId: team.id,
        teamName: team.name,
        stats: {
          gamesPlayed: playerInTeam?.gamesPlayed || 0,
          goals: playerInTeam?.goals || 0,
          assists: playerInTeam?.assists || 0,
          points: (playerInTeam?.goals || 0) + (playerInTeam?.assists || 0),
          penaltyMinutes: playerInTeam?.penaltyMinutes || 0,
        }
      };
    });
    
    // Get current/primary team (first active team or first team if none active)
    const currentTeam = playerTeams.find(team => 
      team.roster.find(rosterPlayer => 
        rosterPlayer.playerId === playerId && rosterPlayer.isActive
      )
    ) || playerTeams[0];
    
    const playerInCurrentTeam = currentTeam?.roster.find(rosterPlayer => 
      rosterPlayer.playerId === playerId
    );
    
    // Fetch recent matches for the current team
    let recentMatches: FloorballMatch[] = [];
    if (currentTeam) {
      try {
        const matchesResponse = await floorballMatchService.getAll({
          teamId: currentTeam.id,
          pageSize: 10 // Get recent matches
        });
        
        // Transform match data to our expected format
        recentMatches = (matchesResponse.data || []).map(match => ({
          id: match.id,
          seasonId: match.seasonId,
          homeTeamId: match.homeTeamId,
          homeTeamName: match.homeTeamName,
          awayTeamId: match.awayTeamId,
          awayTeamName: match.awayTeamName,
          scheduledDateTime: match.scheduledDateTime,
          venue: match.venue,
          status: match.status === FloorballMatchStatus.Completed ? 'completed' : 
                 match.status === FloorballMatchStatus.InProgress ? 'in_progress' : 
                 match.status === FloorballMatchStatus.Cancelled ? 'cancelled' : 
                 match.status === FloorballMatchStatus.Postponed ? 'cancelled' : 'scheduled',
          homeScore: match.homeScore,
          awayScore: match.awayScore,
          wentToOvertime: match.wentToOvertime,
          wentToShootout: match.wentToShootout,
          periodScores: match.periodScores,
          officials: match.officials,
          // Note: Individual player stats per match are not available in the current API
          playerStats: undefined
        }));
      } catch (error) {
        console.warn('Could not fetch matches:', error);
      }
    }
    
    const transformedData: PlayerWithMatches = {
      id: playerData.id,
      playerName: playerData.person.fullName,
      position: playerData.position,
      jerseyNumber: playerInCurrentTeam?.jerseyNumber,
      teamName: currentTeam?.name || 'Ei joukkuetta',
      teamId: currentTeam?.id || 'no-team',
      isActive: playerInCurrentTeam?.isActive || false,
      careerStats: careerStats.length > 0 ? careerStats : [{
        teamId: 'no-team',
        teamName: 'Ei joukkuetta',
        stats: {
          gamesPlayed: 0,
          goals: playerData.careerGoals,
          assists: playerData.careerAssists,
          points: playerData.careerGoals + playerData.careerAssists,
          penaltyMinutes: 0,
        }
      }],
      recentMatches
    };
    
    return transformedData;
  } catch (error) {
    console.error('Error fetching player data:', error);
    throw error;
  }
};

// Helper function to calculate total career stats
const calculateTotalStats = (careerStats: TeamCareerStats[]): PlayerStats => {
  return careerStats.reduce(
    (total, teamStats) => ({
      gamesPlayed: total.gamesPlayed + teamStats.stats.gamesPlayed,
      goals: total.goals + teamStats.stats.goals,
      assists: total.assists + teamStats.stats.assists,
      points: total.points + teamStats.stats.points,
      penaltyMinutes: total.penaltyMinutes + teamStats.stats.penaltyMinutes,
    }),
    { gamesPlayed: 0, goals: 0, assists: 0, points: 0, penaltyMinutes: 0 }
  );
};

const FloorballTeamPlayerUserPage = () => {
  const { id } = useParams<{ id: string }>();
  const [player, setPlayer] = useState<PlayerWithMatches | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const loadPlayerData = async () => {
      if (!id) return;
      
      try {
        setLoading(true);
        setError(null);
        
        const playerData = await fetchPlayerData(id);
        
        setPlayer(playerData);
      } catch (err) {
        setError(err instanceof Error ? err.message : 'An error occurred');
      } finally {
        setLoading(false);
      }
    };

    loadPlayerData();
  }, [id]);

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('fi-FI', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
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

  const getMatchResult = (match: FloorballMatch, playerTeamId: string) => {
    const isHome = match.homeTeamId === playerTeamId;
    const playerScore = isHome ? match.homeScore : match.awayScore;
    const opponentScore = isHome ? match.awayScore : match.homeScore;
    
    if (playerScore > opponentScore) return 'voitto';
    if (playerScore < opponentScore) return 'tappio';
    return 'tasapeli';
  };

  if (loading) return <PageTemplate title="Pelaaja"><div>Ladataan...</div></PageTemplate>;
  if (error) return <PageTemplate title="Pelaaja"><div>Virhe: {error}</div></PageTemplate>;
  if (!player) return <PageTemplate title="Pelaaja"><div>Pelaajaa ei löytynyt</div></PageTemplate>;

  const totalStats = calculateTotalStats(player.careerStats);

  return (
    <PageTemplate title={player.playerName}>
      <div className="floorball-player-container">
        <div className="floorball-player-header">
          <div className="floorball-player-avatar"></div>
          <div className="floorball-player-info">
            <div className="floorball-player-name">{player.playerName}</div>
            <div className="floorball-player-subtitle">
              {player.jerseyNumber ? `#${player.jerseyNumber} • ` : ''}{getPositionText(player.position)}
            </div>
            <div className="floorball-player-subtitle">
              {player.teamName !== 'Ei joukkuetta' ? player.teamName : 'Joukkuetieto ei saatavilla'}
            </div>
            <div className="floorball-player-subtitle">
              {player.isActive ? 'Aktiivinen pelaaja' : 'Ei aktiivinen'}
            </div>
          </div>
        </div>

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

        <div className="matches-section">
          <h3>Viimeisimmät ottelut</h3>
          {player.recentMatches.length === 0 ? (
            <div className="no-matches">
              <p>Ei otteluita saatavilla.</p>
              <p>Pelaajan joukkueelle ei ole vielä luotu otteluita tai pelaaja ei kuulu mihinkään joukkueeseen.</p>
            </div>
          ) : (
            <div className="matches-list">
              {player.recentMatches.map(match => {
                const isHome = match.homeTeamId === player.teamId;
                const opponentName = isHome ? match.awayTeamName : match.homeTeamName;
                const result = getMatchResult(match, player.teamId);
                
                return (
                  <div key={match.id} className={`match-card ${result}`}>
                    <div className="match-header">
                      <div className="match-teams">
                        <div className="match-opponent">{opponentName}</div>
                        <div className="match-location">{isHome ? 'Kotona' : 'Vieraissa'}</div>
                      </div>
                      <div className={`match-result ${result}`}>
                        {match.homeScore} - {match.awayScore}
                      </div>
                    </div>
                    
                    <div className="match-info">
                      <div className="match-date">{formatDate(match.scheduledDateTime)}</div>
                      <div className="match-venue">{match.venue}</div>
                      <div className={`match-status ${result}`}>{result.toUpperCase()}</div>
                    </div>

                    {match.playerStats && (
                      <div className="match-player-stats">
                        <div className="player-stat">
                          <span className="stat-label">Maalit:</span>
                          <span className="stat-value">{match.playerStats.goals}</span>
                        </div>
                        <div className="player-stat">
                          <span className="stat-label">Syötöt:</span>
                          <span className="stat-value">{match.playerStats.assists}</span>
                        </div>
                        <div className="player-stat">
                          <span className="stat-label">Jäähyt:</span>
                          <span className="stat-value">{match.playerStats.penaltyMinutes} min</span>
                        </div>
                        <div className="player-stat">
                          <span className="stat-label">Peliminuutit:</span>
                          <span className="stat-value">{match.playerStats.playedMinutes} min</span>
                        </div>
                      </div>
                    )}
                  </div>
                );
              })}
            </div>
          )}
        </div>
      </div>
    </PageTemplate>
  );
};

export default FloorballTeamPlayerUserPage;
