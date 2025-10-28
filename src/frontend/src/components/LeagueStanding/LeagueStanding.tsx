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


  // Render table header row based on active view
  const renderHeaderRow = (view: 'standings' | 'scorers' | 'assists') => {
    if (view === 'standings') {
      return (
        <thead>
          <tr className="header-row">
            <th className="rank-col">#</th>
            <th className="team-col">TEAM</th>
            <th className="spacer-col"></th>
            <th className="stats-col">MP</th>
            <th className="stats-col">W</th>
            <th className="stats-col">D</th>
            <th className="stats-col">L</th>
            <th className="goals-col">G</th>
            <th className="stats-col">GD</th>
            <th className="points-col">PTS</th>
            <th className="form-col">FORM</th>
          </tr>
        </thead>
      );
    }

    if (view === 'scorers') {
      return (
        <thead>
          <tr className="header-row">
            <th className="rank-col">#</th>
            <th className="team-col">PLAYER</th>
            <th className="spacer-col">TEAM</th>
            <th className="stats-col"></th>
            <th className="stats-col">G</th>
            <th className="stats-col">A</th>
          </tr>
        </thead>
      );
    }

    // assists
    return (
      <thead>
        <tr className="header-row">
          <th className="rank-col">#</th>
          <th className="team-col">PLAYER</th>
          <th className="spacer-col">TEAM</th>
          <th className="stats-col"></th>
          <th className="stats-col">A</th>
          <th className="stats-col">G</th>
        </tr>
      </thead>
    );
  };

  // Render standings table
  const renderStandingsTable = () => {
    const data: FloorballTeamSeasonStatisticsDto[] = seasonSummary?.teamStandings || [];
    
    return (
      <table className="standing-table">
        <colgroup>
          <col className="rank-col" />
          <col className="team-col" />
          <col className="spacer-col" />
          <col className="stats-col" />
          <col className="stats-col" />
          <col className="stats-col" />
          <col className="stats-col" />
          <col className="goals-col" />
          <col className="stats-col" />
          <col className="points-col" />
          <col className="form-col" />
        </colgroup>
        {renderHeaderRow('standings')}
        <tbody>
          {data.map((team, index) => {
            const form = Array.isArray(team.lastFiveForm)
              ? team.lastFiveForm
              : [];
            const rank = index + 1;
            
            return (
              <tr key={team.id}>
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
        <colgroup>
          <col className="rank-col" />
          <col className="team-col" />
          <col className="spacer-col" />
          <col className="stats-col" />
          <col className="stats-col" />
          <col className="stats-col" />
        </colgroup>
        {renderHeaderRow('scorers')}
        <tbody>
          {scorers.map((player: FloorballPlayerSeasonStatisticsDto, index: number) => {
            const rank = index + 1;
            
            return (
              <tr key={player.id}>
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
                
                <td className="stats-col">{player.goals}</td>
                <td className="stats-col">{player.assists}</td>
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
        <colgroup>
          <col className="rank-col" />
          <col className="team-col" />
          <col className="spacer-col" />
          <col className="stats-col" />
          <col className="stats-col" />
          <col className="stats-col" />
        </colgroup>
        {renderHeaderRow('assists')}
        <tbody>
          {assists.map((player: FloorballPlayerSeasonStatisticsDto, index: number) => {
            const rank = index + 1;
            
            return (
              <tr key={player.id}>
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
                
                <td className="stats-col">{player.assists}</td>
                <td className="stats-col">{player.goals}</td>
              </tr>
            );
          })}
        </tbody>
      </table>
    );
  };

  // The headers are now rendered inside the tables with <thead>

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
              {seasonSummary?.seasonName || ""}
            </span>
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
        
        {/* Headers now live inside each table's thead for alignment */}
      </div>

      {/* Dynamic content based on active view */}
      {renderContent()}
    </div>
  );
}


