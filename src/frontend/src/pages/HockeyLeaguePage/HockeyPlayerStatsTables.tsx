import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import type {
  HockeyGoalieCompetitionStatisticsDto,
  HockeyPlayerCompetitionStatisticsDto,
} from '../../types/hockey/hockeyTypes';

interface HockeyPlayerStatsTablesProps {
  players: HockeyPlayerCompetitionStatisticsDto[];
  goalies: HockeyGoalieCompetitionStatisticsDto[];
  playerNames: Map<string, string>;
  teamNames: Map<string, string>;
}

function HockeyPlayerStatsTables({
  players,
  goalies,
  playerNames,
  teamNames,
}: HockeyPlayerStatsTablesProps) {
  const { t } = useTranslation();
  const scorers = [...players].sort((a, b) => b.points - a.points || b.goals - a.goals);
  const rankedGoalies = [...goalies].sort((a, b) => b.savePercentage - a.savePercentage);

  return (
    <>
      <div className="standing-container">
        <div className="standing-header">
          <div className="header-top-row">
            <span className="league-title">{t('hockeyPage.playerStats', 'Player Statistics')}</span>
          </div>
        </div>
        <table className="standing-table">
          <thead>
            <tr className="header-row">
              <th className="rank-col">#</th>
              <th className="team-col">{t('hockeyPage.player', 'Player')}</th>
              <th className="team-col">{t('hockeyPage.team', 'TEAM')}</th>
              <th className="stats-col">{t('hockeyPage.colGp', 'GP')}</th>
              <th className="stats-col">{t('hockeyPage.colG', 'G')}</th>
              <th className="stats-col">{t('hockeyPage.colA', 'A')}</th>
              <th className="points-col">{t('hockeyPage.colP', 'P')}</th>
              <th className="stats-col">{t('hockeyPage.colPim', 'PIM')}</th>
              <th className="stats-col">{t('hockeyPage.colPlusMinus', '+/-')}</th>
            </tr>
          </thead>
          <tbody>
            {scorers.map((row, index) => (
              <tr key={row.id}>
                <td className="rank-col">{index + 1}</td>
                <td className="team-col">
                  <Link to={`/hockeyplayer/${row.playerId}`}>
                    {playerNames.get(row.playerId) ?? row.playerId.slice(0, 8)}
                  </Link>
                </td>
                <td className="team-col">{teamNames.get(row.teamId) ?? ''}</td>
                <td className="stats-col">{row.gamesPlayed}</td>
                <td className="stats-col">{row.goals}</td>
                <td className="stats-col">{row.assists}</td>
                <td className="points-col">{row.points}</td>
                <td className="stats-col">{row.penaltyMinutes}</td>
                <td className="stats-col">{row.plusMinusRating}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      {rankedGoalies.length > 0 && (
        <div className="standing-container">
          <div className="standing-header">
            <div className="header-top-row">
              <span className="league-title">{t('hockeyPage.goalieStats', 'Goalie statistics')}</span>
            </div>
          </div>
          <table className="standing-table">
            <thead>
              <tr className="header-row">
                <th className="rank-col">#</th>
                <th className="team-col">{t('hockeyPage.goalie', 'Goalie')}</th>
                <th className="team-col">{t('hockeyPage.team', 'TEAM')}</th>
                <th className="stats-col">{t('hockeyPage.colGp', 'GP')}</th>
                <th className="stats-col">{t('hockeyPage.colW', 'W')}</th>
                <th className="stats-col">{t('hockeyPage.colSvPct', 'SV%')}</th>
                <th className="stats-col">{t('hockeyPage.colGaa', 'GAA')}</th>
                <th className="stats-col">{t('hockeyPage.colSo', 'SO')}</th>
              </tr>
            </thead>
            <tbody>
              {rankedGoalies.map((row, index) => (
                <tr key={row.id}>
                  <td className="rank-col">{index + 1}</td>
                  <td className="team-col">
                    <Link to={`/hockeyplayer/${row.playerId}`}>
                      {playerNames.get(row.playerId) ?? row.playerId.slice(0, 8)}
                    </Link>
                  </td>
                  <td className="team-col">{teamNames.get(row.teamId) ?? ''}</td>
                  <td className="stats-col">{row.gamesPlayed}</td>
                  <td className="stats-col">{row.wins}</td>
                  <td className="stats-col">{row.savePercentage.toFixed(1)}</td>
                  <td className="stats-col">{row.goalsAgainstAverage.toFixed(2)}</td>
                  <td className="stats-col">{row.shutouts}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </>
  );
}

export default HockeyPlayerStatsTables;
