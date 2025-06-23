import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { FloorballPosition } from "../../types/floorball/floorballTypes";
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

interface PlayerWithMatches {
  id: string;
  playerName: string;
  position: FloorballPosition;
  jerseyNumber?: number;
  teamName: string;
  teamId: string;
  isActive: boolean;
  careerStats: {
    gamesPlayed: number;
    goals: number;
    assists: number;
    points: number;
    penaltyMinutes: number;
  };
  recentMatches: FloorballMatch[];
}

// API function to fetch player's match data
const fetchPlayerMatches = async (playerId: string): Promise<PlayerWithMatches | null> => {
  try {
    const response = await fetch(`/api/FloorballPlayer/${playerId}/matches`);
    
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    
    const apiResponse = await response.json();
    
    // Extract data from the ApiResponse wrapper
    if (apiResponse.success && apiResponse.data) {
      return apiResponse.data;
    } else {
      throw new Error(apiResponse.message || 'Failed to fetch player matches');
    }
  } catch (error) {
    console.error('Error fetching player matches:', error);
    // Return mock data for development
    return {
      id: playerId || '1',
      playerName: 'Matti Meikäläinen',
      position: FloorballPosition.Forward,
      jerseyNumber: 15,
      teamName: 'MAHL Tigers',
      teamId: 'team-1',
      isActive: true,
      careerStats: {
        gamesPlayed: 28,
        goals: 12,
        assists: 8,
        points: 20,
        penaltyMinutes: 14
      },
      recentMatches: [
        {
          id: '1',
          seasonId: 'season-2024',
          homeTeamId: 'team-1',
          homeTeamName: 'MAHL Tigers',
          awayTeamId: 'team-2',
          awayTeamName: 'Helsinki Flyers',
          scheduledDateTime: '2024-03-15T18:30:00Z',
          venue: 'Keskusurheiluhalli',
          status: 'completed',
          homeScore: 4,
          awayScore: 2,
          wentToOvertime: false,
          wentToShootout: false,
          periodScores: {
            '1': { homeScore: 1, awayScore: 1 },
            '2': { homeScore: 2, awayScore: 0 },
            '3': { homeScore: 1, awayScore: 1 }
          },
          officials: ['Tuomari 1', 'Tuomari 2'],
          playerStats: {
            goals: 2,
            assists: 1,
            penaltyMinutes: 2,
            playedMinutes: 55
          }
        },
        {
          id: '2',
          seasonId: 'season-2024',
          homeTeamId: 'team-3',
          homeTeamName: 'Turku Titans',
          awayTeamId: 'team-1',
          awayTeamName: 'MAHL Tigers',
          scheduledDateTime: '2024-03-08T19:00:00Z',
          venue: 'Turkuhalli',
          status: 'completed',
          homeScore: 1,
          awayScore: 3,
          wentToOvertime: false,
          wentToShootout: false,
          periodScores: {
            '1': { homeScore: 0, awayScore: 2 },
            '2': { homeScore: 1, awayScore: 1 },
            '3': { homeScore: 0, awayScore: 0 }
          },
          officials: ['Tuomari 3', 'Tuomari 4'],
          playerStats: {
            goals: 0,
            assists: 2,
            penaltyMinutes: 0,
            playedMinutes: 58
          }
        },
        {
          id: '3',
          seasonId: 'season-2024',
          homeTeamId: 'team-1',
          homeTeamName: 'MAHL Tigers',
          awayTeamId: 'team-4',
          awayTeamName: 'Tampere Thunder',
          scheduledDateTime: '2024-03-01T17:30:00Z',
          venue: 'Keskusurheiluhalli',
          status: 'completed',
          homeScore: 2,
          awayScore: 5,
          wentToOvertime: false,
          wentToShootout: false,
          periodScores: {
            '1': { homeScore: 1, awayScore: 2 },
            '2': { homeScore: 0, awayScore: 2 },
            '3': { homeScore: 1, awayScore: 1 }
          },
          officials: ['Tuomari 5', 'Tuomari 6'],
          playerStats: {
            goals: 1,
            assists: 0,
            penaltyMinutes: 4,
            playedMinutes: 52
          }
        }
      ]
    };
  }
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
        
        const playerData = await fetchPlayerMatches(id);
        
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

  return (
    <PageTemplate title={player.playerName}>
      <div className="floorball-player-container">
        <div className="floorball-player-header">
          <div className="floorball-player-avatar"></div>
          <div className="floorball-player-info">
            <div className="floorball-player-name">{player.playerName}</div>
            <div className="floorball-player-subtitle">#{player.jerseyNumber} • {getPositionText(player.position)}</div>
            <div className="floorball-player-subtitle">{player.teamName}</div>
          </div>
        </div>

        <div className="career-stats-section">
          <h3>Urastatistiikka</h3>
          <div className="stats-grid">
            <div className="stats-box">
              <div className="stats-value">{player.careerStats.gamesPlayed}</div>
              <div className="stats-label">Ottelut</div>
            </div>
            <div className="stats-box">
              <div className="stats-value">{player.careerStats.goals}</div>
              <div className="stats-label">Maalit</div>
            </div>
            <div className="stats-box">
              <div className="stats-value">{player.careerStats.assists}</div>
              <div className="stats-label">Syötöt</div>
            </div>
            <div className="stats-box">
              <div className="stats-value">{player.careerStats.points}</div>
              <div className="stats-label">Pisteet</div>
            </div>
            <div className="stats-box">
              <div className="stats-value">{player.careerStats.penaltyMinutes}</div>
              <div className="stats-label">Jäähy min</div>
            </div>
          </div>
        </div>

        <div className="matches-section">
          <h3>Viimeisimmät ottelut</h3>
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
        </div>
      </div>
    </PageTemplate>
  );
};

export default FloorballTeamPlayerUserPage;
