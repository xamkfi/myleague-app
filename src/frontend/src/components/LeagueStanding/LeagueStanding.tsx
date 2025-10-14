import './LeagueStanding.scss';
import type { 
  FloorballPlayerSeasonStatisticsDto,
  FloorballSeasonStatisticsSummaryDto,
  FloorballTeamSeasonStatisticsDto
} from '../../api/floorball/floorballStatistics';
import { useState } from 'react';
import { FloorballGameResult } from '../../api/floorball/floorballStatistics';

interface LeagueStandingProps {
  seasonSummary?: FloorballSeasonStatisticsSummaryDto | null;
  loading?: boolean;
  error?: string | null;
}

export default function LeagueStanding({ seasonSummary, loading, error }: LeagueStandingProps) {
  const [activeView, setActiveView] = useState<'standings' | 'scorers' | 'assists'>('standings');
  // Show loading state
  if (loading) {
    return (
      <div className="standing-container">
        <div className="loading-state">
          <h3>Loading standings...</h3>
        </div>
      </div>
    );
  }

  // Show error state
  if (error) {
    return (
      <div className="standing-container">
        <div className="error-state">
          <h3>Error loading standings</h3>
          <p>{error}</p>
        </div>
      </div>
    );
  }


  // Dropdown icon component
  const DropdownIcon = () => (
    <svg className="dropdown-icon" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
      <path d="M6 9L12 15L18 9" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"/>
    </svg>
  );

  // Render standings table
  const renderStandingsTable = () => {
    const data: FloorballTeamSeasonStatisticsDto[] = seasonSummary?.teamStandings || [];
    
    return (
      <table className="standing-table">
        <tbody>
          {data.map((team, index) => {
            const form = Array.isArray(team.lastFiveForm)
              ? team.lastFiveForm
              : [];
            const rank = index + 1;
            
            return (
              <tr key={team.id} className="table-row">
                <td className="rank-col">{rank}</td>
                <td className="team-col">
                  <div className="team-info">
                    
                      {team.teamLogo && team.teamLogo.trim() !== '' ? (
                        <img 
                          className="logo-image" 
                          src={team.teamLogo} 
                          alt={team.teamName}
                          onError={(e) => {
                            // Hide image if it fails to load - show empty container
                            const target = e.target as HTMLImageElement;
                            target.style.display = 'none';
                          }}
                        />
                      ) : (
                        <div className="logo-empty"></div>
                      )}
                    
                    <span className="team-name">{team.teamName}</span>
                  </div>
                </td>
                <td className="spacer-col"></td>
                <td className="stats-col">{team.gamesPlayed}</td>
                <td className="stats-col">{team.wins}</td>
                <td className="stats-col">{team.ties}</td>
                <td className="stats-col">{team.losses}</td>
                <td className="goals-col">{team.goalsFor}:{team.goalsAgainst}</td>
                <td className="stats-col">{team.goalDifference}</td>
                <td className="points-col">{team.points}</td>
                <td className="form-col">
                  <div className="form-indicators">
                    {form.map((result: FloorballGameResult, formIndex: number) => {    
                      return (
                        <div 
                          key={formIndex} 
                          className={`form-box form-${result.toString()}`}
                          title={result} // Add tooltip showing the full result
                        >
                          {result.charAt(0)}
                        </div>
                      );
                    })}
                  </div>
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>
    );
  };

  // Render top scorers table
  const renderTopScorersTable = () => {
    const scorers = seasonSummary?.topScorers || [];
    
    return (
      <table className="standing-table">
        <tbody>
          {scorers.map((player: FloorballPlayerSeasonStatisticsDto, index: number) => {
            const rank = index + 1;
            
            return (
              <tr key={player.id} className="table-row">
                <td className="rank-col">{rank}</td>
                <td className="team-col">
                  <div className="team-info">
                    <span className="team-name">{player.playerName}</span>
                  </div>
                </td>
                <td className="spacer-col">
                  <div className="team-info">
                    <span className="team-name">{player.teamName}</span>
                  </div>
                </td>
                <td className="stats-col"></td>
                <td className="stats-col">{player.gamesPlayed}</td>
                <td className="stats-col">{player.goals}</td>
                <td className="stats-col">{player.assists}</td>
                <td className="points-col">{player.points}</td>
                <td className="stats-col">{player.shotsOnGoal}</td>
                <td className="stats-col">{player.shotPercentage.toFixed(1)}%</td>
              </tr>
            );
          })}
        </tbody>
      </table>
    );
  };

  // Render top assists table
  const renderTopAssistsTable = () => {
    const assists = seasonSummary?.topAssists || [];
    
    return (
      <table className="standing-table">
        <tbody>
          {assists.map((player: FloorballPlayerSeasonStatisticsDto, index: number) => {
            const rank = index + 1;
            
            return (
              <tr key={player.id} className="table-row">
                <td className="rank-col">{rank}</td>
                <td className="team-col">
                  <div className="team-info">
                    <span className="team-name">{player.playerName}</span>
                  </div>
                </td>
                <td className="spacer-col">
                  <div className="team-info">
                    <span className="team-name">{player.teamName}</span>
                  </div>
                </td>
                <td className="stats-col"></td>
                <td className="stats-col">{player.gamesPlayed}</td>
                <td className="stats-col">{player.assists}</td>
                <td className="stats-col">{player.goals}</td>
                <td className="points-col">{player.points}</td>
                <td className="stats-col">{player.shotsOnGoal}</td>
                <td className="stats-col">{player.shotPercentage.toFixed(1)}%</td>
              </tr>
            );
          })}
        </tbody>
      </table>
    );
  };

  // Get headers based on active view
  const getHeaders = () => {
    switch (activeView) {
      case 'standings':
        return (
          <div className="table-headers">
            <span className="header-rank">#</span>
            <span className="header-team">TEAM</span>
            <span className="header-spacer"></span>
            <span className="header-item">MP</span>
            <span className="header-item">W</span>
            <span className="header-item">D</span>
            <span className="header-item">L</span>
            <span className="header-item">G</span>
            <span className="header-item">GD</span>
            <span className="header-item">PTS</span>
            <span className="header-item">FORM</span>
          </div>
        );
      case 'scorers':
        return (
          <div className="table-headers">
            <span className="header-rank">#</span>
            <span className="header-team">PLAYER</span>
            <span className="header-spacer">TEAM</span>
            <span className="header-item"></span>
            <span className="header-item">GP</span>
            <span className="header-item">G</span>
            <span className="header-item">A</span>
            <span className="header-item">PTS</span>
            <span className="header-item">SOG</span>
            <span className="header-item">S%</span>
          </div>
        );
      case 'assists':
        return (
          <div className="table-headers">
            <span className="header-rank">#</span>
            <span className="header-team">PLAYER</span>
            <span className="header-spacer">TEAM</span>
            <span className="header-item"></span>
            <span className="header-item">GP</span>
            <span className="header-item">A</span>
            <span className="header-item">G</span>
            <span className="header-item">PTS</span>
            <span className="header-item">SOG</span>
            <span className="header-item">S%</span>
          </div>
        );
      default:
        return null;
    }
  };

  // Render content based on active view
  const renderContent = () => {
    switch (activeView) {
      case 'standings':
        return renderStandingsTable();
      case 'scorers':
        return renderTopScorersTable();
      case 'assists':
        return renderTopAssistsTable();
      default:
        return renderStandingsTable();
    }
  };

  return (
    <div className="standing-container">
      {/* Header with dropdown and view buttons */}
      <div className="standing-header">
        <div className="header-top-row">
          <div className="league-selector">
            <span className="league-title">
              {seasonSummary?.seasonName || "2025 RAUTALIIGA"}
            </span>
            <DropdownIcon />
          </div>
          
          {/* View buttons */}
          <div className="view-buttons">
            <button 
              className={`view-button ${activeView === 'standings' ? 'active' : ''}`}
              onClick={() => setActiveView('standings')}
            >
              Standings
            </button>
            <button 
              className={`view-button ${activeView === 'scorers' ? 'active' : ''}`}
              onClick={() => setActiveView('scorers')}
            >
              Top Scorers
            </button>
            <button 
              className={`view-button ${activeView === 'assists' ? 'active' : ''}`}
              onClick={() => setActiveView('assists')}
            >
              Top Assists
            </button>
          </div>
        </div>
        
        {/* Dynamic headers based on active view */}
        {getHeaders()}
      </div>

      {/* Dynamic content based on active view */}
      {renderContent()}
    </div>
  );
}


