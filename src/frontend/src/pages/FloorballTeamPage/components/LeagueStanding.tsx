import './LeagueStanding.scss';
import type { FloorballTeamSeasonStatisticsDto } from '../../../api/floorball/floorballStatistics';

interface LeagueStandingProps {
  standings?: FloorballTeamSeasonStatisticsDto[] | null;
  loading?: boolean;
  error?: string | null;
  seasonName?: string;
}

export default function LeagueStanding({ standings, loading, error, seasonName }: LeagueStandingProps) {
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

  // Use real data or fallback to empty array
  const data = standings || [];

  // Generate recent form data (last 6 games) - 1 unknown + 5 wins as shown in image
  const generateForm = () => {
    const form = ['?']; // First game is unknown
    // Add 5 wins
    for (let i = 0; i < 5; i++) {
      form.push('W');
    }
    return form;
  };

  // Dropdown icon component
  const DropdownIcon = () => (
    <svg className="dropdown-icon" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
      <path d="M6 9L12 15L18 9" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"/>
    </svg>
  );

  return (
    <div className="standing-container">
      {/* Header with dropdown and column headers */}
      <div className="standing-header">
        <div className="league-selector">
          <span className="league-title">
            {seasonName || (data.length > 0 ? data[0].seasonName : "2025 RAUTALIIGA")}
          </span>
          <DropdownIcon />
        </div>
        
        <div className="table-headers">
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
      </div>

      {/* Table */}
      <table className="standing-table">
        <tbody>
          {data.map((team, index) => {
            const form = generateForm();
            const rank = index + 1;
            
            return (
              <tr key={team.id} className="table-row">
                <td className="rank-col">{rank}</td>
                <td className="team-col">
                  <div className="team-info">
                    <div className="team-logo">
                      <span className="logo-text">
                        {team.teamName.split(' ').map(word => word[0]).join('').substring(0, 2).toUpperCase()}
                      </span>
                    </div>
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
                    {form.map((result, formIndex) => (
                      <span 
                        key={formIndex} 
                        className={`form-indicator form-${result.toLowerCase() === 'w' ? 'w' : result.toLowerCase() === 'd' ? 'd' : result.toLowerCase() === 'l' ? 'l' : 'unknown'}`}
                      >
                        {result}
                      </span>
                    ))}
                  </div>
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
}