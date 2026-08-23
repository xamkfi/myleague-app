import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import StatRow from '../MatchPage/components/StatRow';
import { getTeamInitials } from '../MatchPage/components/matchUtils';
import type { HockeyMatchStatisticsDto } from '../../types/hockey/hockeyTypes';
import '../MatchPage/components/MatchStats.scss';

interface HockeyMatchStatsProps {
  stats: HockeyMatchStatisticsDto;
  homeName: string;
  awayName: string;
  homeTeamId: string | null;
  awayTeamId: string | null;
  playerNames: Map<string, string>;
}

function HockeyMatchStats({
  stats,
  homeName,
  awayName,
  homeTeamId,
  awayTeamId,
  playerNames,
}: HockeyMatchStatsProps) {
  const { t } = useTranslation();
  const home = stats.teams.find((row) => row.teamId === homeTeamId);
  const away = stats.teams.find((row) => row.teamId === awayTeamId);
  const homeGoals = home?.goalsFor ?? 0;
  const awayGoals = away?.goalsFor ?? 0;
  const homeSog = home?.shotsOnGoal ?? 0;
  const awaySog = away?.shotsOnGoal ?? 0;
  const homeFo = home?.faceoffWins ?? 0;
  const awayFo = away?.faceoffWins ?? 0;
  const homePim = home?.penaltyMinutes ?? 0;
  const awayPim = away?.penaltyMinutes ?? 0;
  const players = [...stats.players].sort((a, b) => b.points - a.points || b.goals - a.goals);
  const goalies = stats.goalies;

  return (
    <div className="summary-content">
      <div className="match-stats">
        <div className="stats-header">
          <div className="team-identity home">
            <div className="team-crest home-team" title={homeName}>
              <span className="team-initials">{getTeamInitials(homeName)}</span>
            </div>
            <span className="team-name">{homeName}</span>
          </div>
          <div className="header-label">{t('matchPage.stats.title')}</div>
          <div className="team-identity away">
            <span className="team-name">{awayName}</span>
            <div className="team-crest away-team" title={awayName}>
              <span className="team-initials">{getTeamInitials(awayName)}</span>
            </div>
          </div>
        </div>
        <div className="stats-content">
          <StatRow
            label={t('hockeyPage.goals', 'Goals')}
            home={homeGoals}
            away={awayGoals}
            homeValue={homeGoals}
            awayValue={awayGoals}
            total={homeGoals + awayGoals}
          />
          <StatRow
            label={t('hockeyPage.shotsOnGoal', 'Shots on goal')}
            home={homeSog}
            away={awaySog}
            homeValue={homeSog}
            awayValue={awaySog}
            total={homeSog + awaySog}
          />
          <StatRow
            label={t('hockeyPage.faceoffs', 'Face-offs')}
            home={homeFo}
            away={awayFo}
            homeValue={homeFo}
            awayValue={awayFo}
            total={homeFo + awayFo}
          />
          <StatRow
            label={t('hockeyPage.penaltyMinutes', 'PIM')}
            home={homePim}
            away={awayPim}
            homeValue={homePim}
            awayValue={awayPim}
            total={homePim + awayPim}
          />
        </div>
      </div>
      {players.length > 0 && (
        <div className="standing-container">
          <table className="standing-table">
            <thead>
              <tr className="header-row">
                <th className="team-col">{t('hockeyPage.player', 'Player')}</th>
                <th className="stats-col">{t('hockeyPage.colG', 'G')}</th>
                <th className="stats-col">{t('hockeyPage.colA', 'A')}</th>
                <th className="points-col">{t('hockeyPage.colP', 'P')}</th>
                <th className="stats-col">{t('hockeyPage.colPim', 'PIM')}</th>
              </tr>
            </thead>
            <tbody>
              {players.map((row) => (
                <tr key={`${row.playerId}-${row.teamId}`}>
                  <td className="team-col">
                    <Link to={`/hockeyplayer/${row.playerId}`}>
                      {playerNames.get(row.playerId) ?? row.playerId.slice(0, 8)}
                    </Link>
                  </td>
                  <td className="stats-col">{row.goals}</td>
                  <td className="stats-col">{row.assists}</td>
                  <td className="points-col">{row.points}</td>
                  <td className="stats-col">{row.penaltyMinutes}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
      {goalies.length > 0 && (
        <div className="standing-container">
          <table className="standing-table">
            <thead>
              <tr className="header-row">
                <th className="team-col">{t('hockeyPage.goalie', 'Goalie')}</th>
                <th className="stats-col">{t('hockeyPage.colSa', 'SA')}</th>
                <th className="stats-col">{t('hockeyPage.colSv', 'SV')}</th>
                <th className="stats-col">{t('hockeyPage.colSvPct', 'SV%')}</th>
                <th className="stats-col">{t('hockeyPage.colGa', 'GA')}</th>
              </tr>
            </thead>
            <tbody>
              {goalies.map((row) => (
                <tr key={`${row.playerId}-${row.teamId}`}>
                  <td className="team-col">
                    <Link to={`/hockeyplayer/${row.playerId}`}>
                      {playerNames.get(row.playerId) ?? row.playerId.slice(0, 8)}
                    </Link>
                  </td>
                  <td className="stats-col">{row.shotsAgainst}</td>
                  <td className="stats-col">{row.saves}</td>
                  <td className="stats-col">{row.savePercentage.toFixed(1)}</td>
                  <td className="stats-col">{row.goalsAgainst}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

export default HockeyMatchStats;
