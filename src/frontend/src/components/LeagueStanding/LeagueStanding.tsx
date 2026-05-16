import './LeagueStanding.scss';
import type {
  FloorballPlayerSeasonStatisticsDto,
  FloorballGoalieSeasonStatisticsDto,
  FloorballSeasonStatisticsSummaryDto,
  FloorballTeamSeasonStatisticsDto
} from '../../api/floorball/floorballStatistics';
import type { ReactNode } from 'react';
import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import { FloorballGameResult } from '../../api/floorball/floorballStatistics';
import { useFloorballTeamsData } from '../../hooks/useTeamsData';
import { createTeamSlug } from '../../utils/slugUtils';

interface LeagueStandingProps {
  seasonSummary?: FloorballSeasonStatisticsSummaryDto | null;
  loading?: boolean;
  error?: string | null;
  /**
   * Optional override that replaces the contents of the "standings" view. Use cases:
   *  - In a tournament group-stage match, swap the season-wide league table for the
   *    relevant group's standings.
   *  - In a tournament playoff match, render the playoff bracket instead.
   * Other views (top scorers / assists / goalies) are left untouched and continue to
   * render from `seasonSummary`.
   */
  standingsOverride?: ReactNode;
  /**
   * Optional override for the title shown in the header. Defaults to the season name from
   * `seasonSummary`. Useful e.g. to display "Group A" or "Playoff bracket" in match context.
   */
  titleOverride?: string;
}

export default function LeagueStanding({ seasonSummary, loading, error, standingsOverride, titleOverride }: LeagueStandingProps) {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { teams, refetch } = useFloorballTeamsData();
  const [activeView, setActiveView] = useState<'standings' | 'scorers' | 'assists' | 'goalies'>('standings');

  useEffect(() => {
    refetch();
  }, [refetch]);

  const navigateToTeam = (teamId: string) => {
    const team = teams?.find(t => t.id === teamId);
    if (team) {
      const slug = createTeamSlug(team, teams);
      navigate(`/team/${slug}`);
    }
  };

  const navigateToPlayer = (playerId: string) => {
    navigate(`/floorballplayer/${playerId}`);
  };

  // When a `standingsOverride` is provided, the parent owns loading/error UX for the
  // standings view (it's typically a self-contained component like
  // `TournamentGroupStandingsTable` or `TournamentBracket`). We still need to render the
  // surrounding view buttons so users can switch between standings/scorers/assists/goalies.
  // The other views handle their own empty state via `seasonSummary` checks.
  if (loading && !standingsOverride) {
    return (
      <div className="standing-container">
        <div className="loading-state">
          <h3>{t('leaguePage.summary.loading')}</h3>
        </div>
      </div>
    );
  }

  if (error && !standingsOverride) {
    return (
      <div className="standing-container">
        <div className="error-state">
          <h3>{t('leaguePage.summary.error', { error })}</h3>
        </div>
      </div>
    );
  }

  // Render table header row based on active view
  const renderHeaderRow = (view: 'standings' | 'scorers' | 'assists' | 'goalies') => {
    if (view === 'standings') {
      return (
        <thead>
          <tr className="header-row">
            <th className="rank-col">#</th>
            <th className="team-col">TEAM</th>
            <th className="spacer-col"></th>
            <th className="stats-col" title="Pelatut ottelut (Matches Played)">MP</th>
            <th className="stats-col" title="Voitot (Wins)">W</th>
            <th className="stats-col" title="Tasapelit (Draws)">D</th>
            <th className="stats-col" title="Tappiot (Losses)">L</th>
            <th className="goals-col" title="Tehdyt : Päästetyt maalit (Goals)">G</th>
            <th className="stats-col" title="Maaliero (Goal Difference)">GD</th>
            <th className="points-col" title="Pisteet (Points)">PTS</th>
            <th className="form-col" title="Viimeisimmät ottelut (Form)">FORM</th>
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
            <th className="stats-col" title="Maalit (Goals)">G</th>
            <th className="stats-col" title="Syötöt (Assists)">A</th>
          </tr>
        </thead>
      );
    }

    if (view === 'goalies') {
      return (
        <thead>
          <tr className="header-row">
            <th className="rank-col">#</th>
            <th className="team-col">{t('leaguePage.standings.goalieHeaders.player')}</th>
            <th className="spacer-col">{t('leaguePage.standings.goalieHeaders.team')}</th>
            <th className="stats-col" title="Pelatut ottelut (Games Played)">GP</th>
            <th className="stats-col" title="Voitot (Wins)">W</th>
            <th className="stats-col" title="Tappiot (Losses)">L</th>
            <th className="stats-col" title="Päästetyt maalit (Goals Against)">GA</th>
            <th className="stats-col" title="Torjuntaprosentti (Save Percentage)">SV%</th>
            <th className="stats-col" title="Nollapelit (Shutouts)">SO</th>
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
          <th className="stats-col" title="Syötöt (Assists)">A</th>
          <th className="stats-col" title="Maalit (Goals)">G</th>
        </tr>
      </thead>
    );
  };

  // Render standings table
  const renderStandingsTable = () => {
    const data: FloorballTeamSeasonStatisticsDto[] = seasonSummary?.teamStandings || [];
    
    if (!seasonSummary || data.length === 0) {
      return (
        <div className="empty-state">
          <h3>{t('leaguePage.standings.emptyStandings')}</h3>
          <p>{t('leaguePage.standings.emptyStandingsDesc')}</p>
        </div>
      );
    }

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
              <tr
                key={team.id}
                className="clickable-row"
                onClick={() => navigateToTeam(team.teamId)}
              >
                <td className="rank-col">{rank}</td>
                <td className="team-col">
                  <div className="team-info">
                    {team.teamLogo && team.teamLogo.trim() !== '' ? (
                      <img 
                        className="logo-image" 
                        src={team.teamLogo} 
                        alt={team.teamName}
                        onError={(e) => {
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
                          title={result}
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
    
    if (!seasonSummary || scorers.length === 0) {
      return (
        <div className="empty-state">
          <h3>{t('leaguePage.standings.emptyScorers')}</h3>
          <p>{t('leaguePage.standings.emptyScorersDesc')}</p>
        </div>
      );
    }

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
              <tr
                key={player.id}
                className="clickable-row"
                onClick={() => navigateToPlayer(player.playerId)}
              >
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
    
    if (!seasonSummary || assists.length === 0) {
      return (
        <div className="empty-state">
          <h3>{t('leaguePage.standings.emptyAssists')}</h3>
          <p>{t('leaguePage.standings.emptyAssistsDesc')}</p>
        </div>
      );
    }

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
              <tr
                key={player.id}
                className="clickable-row"
                onClick={() => navigateToPlayer(player.playerId)}
              >
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

  // Render goalies table
  const renderGoaliesTable = () => {
    const goalies = seasonSummary?.topGoalies || [];

    if (!seasonSummary || goalies.length === 0) {
      return (
        <div className="empty-state">
          <h3>{t('leaguePage.standings.emptyGoalies')}</h3>
          <p>{t('leaguePage.standings.emptyGoaliesDesc')}</p>
        </div>
      );
    }

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
          <col className="stats-col" />
          <col className="stats-col" />
        </colgroup>
        {renderHeaderRow('goalies')}
        <tbody>
          {goalies.map((goalie: FloorballGoalieSeasonStatisticsDto, index: number) => {
            const rank = index + 1;

            return (
              <tr
                key={goalie.id}
                className="clickable-row"
                onClick={() => navigateToPlayer(goalie.playerId)}
              >
                <td className="rank-col">{rank}</td>
                <td className="team-col">
                  <div className="team-info">
                    <span className="team-name">{goalie.playerName}</span>
                  </div>
                </td>
                <td className="spacer-col">
                  <div className="team-info">
                    <span className="team-name">{goalie.teamName}</span>
                  </div>
                </td>
                <td className="stats-col">{goalie.gamesPlayed}</td>
                <td className="stats-col">{goalie.wins}</td>
                <td className="stats-col">{goalie.losses}</td>
                <td className="stats-col">{goalie.goalsAgainst}</td>
                <td className="points-col">{goalie.savePercentage.toFixed(1)}%</td>
                <td className="stats-col">{goalie.shutouts}</td>
              </tr>
            );
          })}
        </tbody>
      </table>
    );
  };

  // Render content based on active view. If a caller supplied a `standingsOverride` we use
  // it for the standings view only; the override is responsible for its own loading/error
  // states (we don't gate it on the season summary loading state).
  const renderContent = () => {
    switch (activeView) {
      case 'standings':
        return standingsOverride ?? renderStandingsTable();
      case 'scorers':
        return renderTopScorersTable();
      case 'assists':
        return renderTopAssistsTable();
      case 'goalies':
        return renderGoaliesTable();
      default:
        return standingsOverride ?? renderStandingsTable();
    }
  };

  return (
    <div className="standing-container">
      {/* Header with dropdown and view buttons */}
      <div className="standing-header">
        <div className="header-top-row">
          <div className="league-selector">
            <span className="league-title">
              {titleOverride ?? seasonSummary?.seasonName ?? ""}
            </span>
          </div>
          
          {/* View buttons */}
          <div className="view-buttons">
            <button 
              className={`view-button ${activeView === 'standings' ? 'active' : ''}`}
              onClick={() => setActiveView('standings')}
            >
              {t('leaguePage.standings.standings')}
            </button>
            <button 
              className={`view-button ${activeView === 'scorers' ? 'active' : ''}`}
              onClick={() => setActiveView('scorers')}
            >
              {t('leaguePage.standings.topScorers')}
            </button>
            <button 
              className={`view-button ${activeView === 'assists' ? 'active' : ''}`}
              onClick={() => setActiveView('assists')}
            >
              {t('leaguePage.standings.topAssists')}
            </button>
            <button 
              className={`view-button ${activeView === 'goalies' ? 'active' : ''}`}
              onClick={() => setActiveView('goalies')}
            >
              {t('leaguePage.standings.topGoalies')}
            </button>
          </div>
        </div>
      </div>

      {/* Dynamic content based on active view */}
      <div className="table-wrapper">
        {renderContent()}
      </div>
    </div>
  );
}
