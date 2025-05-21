import React from 'react';
import { useTranslation } from 'react-i18next';
import './MatchSidebar.css';

interface TeamProps {
  name: string;
  logo?: string;
}

interface MatchProps {
  date: string;
  homeTeam: TeamProps;
  awayTeam: TeamProps;
}

interface StandingsRowProps {
  position: number;
  team: string;
  points: number;
}

interface StandingsProps {
  rows: StandingsRowProps[];
}

interface MatchSidebarProps {
  match: MatchProps;
  standings: StandingsProps;
  teamStats: Array<{teamName: string, playerName: string, value: number}>;
}

const MatchSidebar: React.FC<MatchSidebarProps> = ({ match, standings, teamStats }) => {
  const { t } = useTranslation();
  
  return (
    <div className="match-sidebar">
      <div className="next-match">
        <h2 className="sidebar-title">{t('sidebar.nextMatch')}</h2>
        <p className="match-date">{match.date}</p>
        
        <div className="teams-container">
          <div className="team-logo">
            {match.homeTeam.logo ? (
              <img src={match.homeTeam.logo} alt={match.homeTeam.name} />
            ) : (
              <div className="placeholder-logo"></div>
            )}
          </div>
          
          <span className="vs-text">vs</span>
          
          <div className="team-logo">
            {match.awayTeam.logo ? (
              <img src={match.awayTeam.logo} alt={match.awayTeam.name} />
            ) : (
              <div className="placeholder-logo"></div>
            )}
          </div>
        </div>
      </div>

      <div className="standings-section">
        <h3 className="standings-title">{t('sidebar.standings')}</h3>
        <table className="standings-table">
          <tbody>
            {standings.rows.map((row) => (
              <tr key={row.position} className="standings-row">
                <td className="position">{row.position}</td>
                <td className="team-indicator">
                  <div className="team-circle"></div>
                </td>
                <td className="team-name">{row.team}</td>
                <td className="team-points">{row.points} PTS</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <div className="team-stats">
        <h3 className="stats-title">{t('sidebar.stats')}</h3>
        <table className="stats-table">
          <tbody>
            {teamStats.map((stat, index) => (
              <tr key={index} className="stat-row">
                <td className="team-indicator">
                  <div className="team-circle"></div>
                </td>
                <td className="player-name">{stat.teamName}</td>
                <td className="stat-value">{stat.value}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default MatchSidebar; 