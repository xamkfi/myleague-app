import { useTranslation } from 'react-i18next';
import StatAbbr from '../../components/StatAbbr/StatAbbr';
import { PlayerLink, TeamLink } from '../../components/SportLinks';
import type {
  HockeyGoalieCompetitionStatisticsDto,
  HockeyPlayerCompetitionStatisticsDto,
} from '../../types/hockey/hockeyTypes';
import { formatHockeyFaceoffPercentage } from '../../utils/hockeyLookups';

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
  const namedTeams = [...teamNames.entries()].map(([id, name]) => ({ id, name }));

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
              <th className="stats-col"><StatAbbr abbr={t('hockeyPage.colGp', 'GP')} title={t('hockeyPage.colGpTitle', 'Games played')} /></th>
              <th className="stats-col"><StatAbbr abbr={t('hockeyPage.colG', 'G')} title={t('hockeyPage.colGTitle', 'Goals')} /></th>
              <th className="stats-col"><StatAbbr abbr={t('hockeyPage.colA', 'A')} title={t('hockeyPage.colATitle', 'Assists')} /></th>
              <th className="points-col"><StatAbbr abbr={t('hockeyPage.colP', 'P')} title={t('hockeyPage.colPTitle', 'Points')} /></th>
              <th className="stats-col"><StatAbbr abbr={t('hockeyPage.colPim', 'PIM')} title={t('hockeyPage.colPimTitle', 'Penalty minutes')} /></th>
              <th className="stats-col"><StatAbbr abbr={t('hockeyPage.colFo', 'FO%')} title={t('hockeyPage.colFoTitle', 'Faceoff win percentage')} /></th>
              <th className="stats-col"><StatAbbr abbr={t('hockeyPage.colPlusMinus', '+/-')} title={t('hockeyPage.colPlusMinusTitle', 'Plus-minus')} /></th>
            </tr>
          </thead>
          <tbody>
            {scorers.map((row, index) => (
              <tr key={row.id}>
                <td className="rank-col">{index + 1}</td>
                <td className="team-col">
                  <PlayerLink sport="hockey" playerId={row.playerId}>
                    {playerNames.get(row.playerId) ?? row.playerId.slice(0, 8)}
                  </PlayerLink>
                </td>
                <td className="team-col">
                  <TeamLink
                    sport="hockey"
                    teamId={row.teamId}
                    teamName={teamNames.get(row.teamId) ?? ''}
                    teams={namedTeams}
                  />
                </td>
                <td className="stats-col">{row.gamesPlayed}</td>
                <td className="stats-col">{row.goals}</td>
                <td className="stats-col">{row.assists}</td>
                <td className="points-col">{row.points}</td>
                <td className="stats-col">{row.penaltyMinutes}</td>
                <td className="stats-col">{formatHockeyFaceoffPercentage(row.faceoffWins ?? 0, row.faceoffAttempts ?? 0)}</td>
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
                <th className="stats-col"><StatAbbr abbr={t('hockeyPage.colGp', 'GP')} title={t('hockeyPage.colGpTitle', 'Games played')} /></th>
                <th className="stats-col"><StatAbbr abbr={t('hockeyPage.colW', 'W')} title={t('hockeyPage.colWTitle', 'Wins')} /></th>
                <th className="stats-col"><StatAbbr abbr={t('hockeyPage.colSvPct', 'SV%')} title={t('hockeyPage.colSvPctTitle', 'Save percentage')} /></th>
                <th className="stats-col"><StatAbbr abbr={t('hockeyPage.colGaa', 'GAA')} title={t('hockeyPage.colGaaTitle', 'Goals against average')} /></th>
                <th className="stats-col"><StatAbbr abbr={t('hockeyPage.colSo', 'SO')} title={t('hockeyPage.colSoTitle', 'Shutouts')} /></th>
              </tr>
            </thead>
            <tbody>
              {rankedGoalies.map((row, index) => (
                <tr key={row.id}>
                  <td className="rank-col">{index + 1}</td>
                  <td className="team-col">
                    <PlayerLink sport="hockey" playerId={row.playerId}>
                      {playerNames.get(row.playerId) ?? row.playerId.slice(0, 8)}
                    </PlayerLink>
                  </td>
                  <td className="team-col">
                    <TeamLink
                      sport="hockey"
                      teamId={row.teamId}
                      teamName={teamNames.get(row.teamId) ?? ''}
                      teams={namedTeams}
                    />
                  </td>
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
