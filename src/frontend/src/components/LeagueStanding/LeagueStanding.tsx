import './LeagueStanding.scss';
import type {
  FloorballPlayerSeasonStatisticsDto,
  FloorballGoalieSeasonStatisticsDto,
  FloorballSeasonStatisticsSummaryDto,
  FloorballTeamSeasonStatisticsDto,
} from '../../api/floorball/floorballStatistics';
import type {
  FootballPlayerSeasonStatisticsDto,
  FootballSeasonStatisticsSummaryDto,
  FootballTeamSeasonStatisticsDto,
} from '../../api/football/footballStatistics';
import type { ReactNode } from 'react';
import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import { useFloorballTeamsData, useFootballTeamsData } from '../../hooks/useTeamsData';
import { createTeamSlug } from '../../utils/slugUtils';
import { getPlayerPath, getTeamPath, type SportKind } from '../../utils/sportRoutes';
import { TeamLink, PlayerLink } from '../SportLinks';

type StandingView = 'standings' | 'scorers' | 'assists' | 'goalies';
type LeagueSeasonSummary = FloorballSeasonStatisticsSummaryDto | FootballSeasonStatisticsSummaryDto;
type LeagueTeamStanding = FloorballTeamSeasonStatisticsDto | FootballTeamSeasonStatisticsDto;
type LeaguePlayerStat = FloorballPlayerSeasonStatisticsDto | FootballPlayerSeasonStatisticsDto;

interface LeagueStandingProps {
  sport?: SportKind;
  seasonSummary?: LeagueSeasonSummary | null;
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

function getDraws(team: LeagueTeamStanding): number {
  if ('draws' in team) {
    return team.draws;
  }
  return team.ties;
}

function getGoalies(summary: LeagueSeasonSummary | null | undefined): FloorballGoalieSeasonStatisticsDto[] {
  if (summary && 'topGoalies' in summary) {
    return summary.topGoalies ?? [];
  }
  return [];
}

export default function LeagueStanding({
  sport = 'floorball',
  seasonSummary,
  loading,
  error,
  standingsOverride,
  titleOverride,
}: LeagueStandingProps) {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const floorballTeams = useFloorballTeamsData();
  const footballTeams = useFootballTeamsData();
  const { teams, refetch } = sport === 'football' ? footballTeams : floorballTeams;
  const showGoalies = sport !== 'football';
  const [activeView, setActiveView] = useState<StandingView>('standings');

  useEffect(() => {
    refetch();
  }, [refetch]);

  useEffect(() => {
    if (!showGoalies && activeView === 'goalies') {
      setActiveView('standings');
    }
  }, [showGoalies, activeView]);

  const navigateToTeam = (teamId: string) => {
    const team = teams?.find((item) => item.id === teamId);
    if (team) {
      const slug = createTeamSlug(team, teams);
      navigate(getTeamPath(sport, slug));
    }
  };

  const navigateToPlayer = (playerId: string) => {
    navigate(getPlayerPath(sport, playerId));
  };

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

  const renderHeaderRow = (view: StandingView) => {
    if (view === 'standings') {
      return (
        <thead>
          <tr className="header-row">
            <th className="rank-col">#</th>
            <th className="team-col">{t('leaguePage.standings.team')}</th>
            <th className="spacer-col"></th>
            <th className="stats-col" title={t('leaguePage.standings.colMpTitle')}>MP</th>
            <th className="stats-col" title={t('leaguePage.standings.colWTitle')}>W</th>
            <th className="stats-col" title={t('leaguePage.standings.colDTitle')}>D</th>
            <th className="stats-col" title={t('leaguePage.standings.colLTitle')}>L</th>
            <th className="goals-col" title={t('leaguePage.standings.colGTitle')}>G</th>
            <th className="stats-col" title={t('leaguePage.standings.colGdTitle')}>GD</th>
            <th className="points-col" title={t('leaguePage.standings.colPtsTitle')}>PTS</th>
            <th className="form-col" title={t('leaguePage.standings.colFormTitle')}>FORM</th>
          </tr>
        </thead>
      );
    }

    if (view === 'scorers') {
      return (
        <thead>
          <tr className="header-row">
            <th className="rank-col">#</th>
            <th className="team-col">{t('leaguePage.standings.player')}</th>
            <th className="spacer-col">{t('leaguePage.standings.team')}</th>
            <th className="stats-col"></th>
            <th className="stats-col" title={t('leaguePage.standings.colGoalsTitle')}>G</th>
            <th className="stats-col" title={t('leaguePage.standings.colAssistsTitle')}>A</th>
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
            <th className="stats-col" title={t('leaguePage.standings.colGpTitle')}>GP</th>
            <th className="stats-col" title={t('leaguePage.standings.colWTitle')}>W</th>
            <th className="stats-col" title={t('leaguePage.standings.colLTitle')}>L</th>
            <th className="stats-col" title={t('leaguePage.standings.colGaTitle')}>GA</th>
            <th className="stats-col" title={t('leaguePage.standings.colSvTitle')}>SV%</th>
            <th className="stats-col" title={t('leaguePage.standings.colSoTitle')}>SO</th>
          </tr>
        </thead>
      );
    }

    return (
      <thead>
        <tr className="header-row">
          <th className="rank-col">#</th>
          <th className="team-col">{t('leaguePage.standings.player')}</th>
          <th className="spacer-col">{t('leaguePage.standings.team')}</th>
          <th className="stats-col"></th>
          <th className="stats-col" title={t('leaguePage.standings.colAssistsTitle')}>A</th>
          <th className="stats-col" title={t('leaguePage.standings.colGoalsTitle')}>G</th>
        </tr>
      </thead>
    );
  };

  const renderStandingsTable = () => {
    const data: LeagueTeamStanding[] = seasonSummary?.teamStandings || [];

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
            const form = Array.isArray(team.lastFiveForm) ? team.lastFiveForm : [];
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
                    <TeamLink
                      sport={sport}
                      teamId={team.teamId}
                      teamName={team.teamName}
                      teams={teams}
                      className="team-name"
                    />
                  </div>
                </td>
                <td className="spacer-col"></td>
                <td className="stats-col">{team.gamesPlayed}</td>
                <td className="stats-col">{team.wins}</td>
                <td className="stats-col">{getDraws(team)}</td>
                <td className="stats-col">{team.losses}</td>
                <td className="goals-col">{team.goalsFor}:{team.goalsAgainst}</td>
                <td className="stats-col">{team.goalDifference}</td>
                <td className="points-col">{team.points}</td>
                <td className="form-col">
                  <div className="form-indicators">
                    {form.map((result, formIndex) => (
                      <div
                        key={formIndex}
                        className={`form-box form-${result.toString()}`}
                        title={result}
                      >
                        {result.charAt(0)}
                      </div>
                    ))}
                  </div>
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>
    );
  };

  const renderTopScorersTable = () => {
    const scorers: LeaguePlayerStat[] = seasonSummary?.topScorers || [];

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
          {scorers.map((player, index) => {
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
                    <PlayerLink sport={sport} playerId={player.playerId} className="team-name">
                      {player.playerName}
                    </PlayerLink>
                  </div>
                </td>
                <td className="spacer-col">
                  <div className="team-info">
                    <TeamLink
                      sport={sport}
                      teamId={player.teamId}
                      teamName={player.teamName}
                      teams={teams}
                      className="team-name"
                    />
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

  const renderTopAssistsTable = () => {
    const assists: LeaguePlayerStat[] = seasonSummary?.topAssists || [];

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
          {assists.map((player, index) => {
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
                    <PlayerLink sport={sport} playerId={player.playerId} className="team-name">
                      {player.playerName}
                    </PlayerLink>
                  </div>
                </td>
                <td className="spacer-col">
                  <div className="team-info">
                    <TeamLink
                      sport={sport}
                      teamId={player.teamId}
                      teamName={player.teamName}
                      teams={teams}
                      className="team-name"
                    />
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

  const renderGoaliesTable = () => {
    const goalies = getGoalies(seasonSummary);

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
          {goalies.map((goalie, index) => {
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
                    <PlayerLink sport={sport} playerId={goalie.playerId} className="team-name">
                      {goalie.playerName}
                    </PlayerLink>
                  </div>
                </td>
                <td className="spacer-col">
                  <div className="team-info">
                    <TeamLink
                      sport={sport}
                      teamId={goalie.teamId}
                      teamName={goalie.teamName}
                      teams={teams}
                      className="team-name"
                    />
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

  const renderContent = () => {
    switch (activeView) {
      case 'standings':
        return standingsOverride ?? renderStandingsTable();
      case 'scorers':
        return renderTopScorersTable();
      case 'assists':
        return renderTopAssistsTable();
      case 'goalies':
        return showGoalies ? renderGoaliesTable() : standingsOverride ?? renderStandingsTable();
      default:
        return standingsOverride ?? renderStandingsTable();
    }
  };

  return (
    <div className="standing-container">
      <div className="standing-header">
        <div className="header-top-row">
          <div className="league-selector">
            <span className="league-title">
              {titleOverride ?? seasonSummary?.seasonName ?? ''}
            </span>
          </div>

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
            {showGoalies && (
              <button
                className={`view-button ${activeView === 'goalies' ? 'active' : ''}`}
                onClick={() => setActiveView('goalies')}
              >
                {t('leaguePage.standings.topGoalies')}
              </button>
            )}
          </div>
        </div>
      </div>

      <div className="table-wrapper">
        {renderContent()}
      </div>
    </div>
  );
}
