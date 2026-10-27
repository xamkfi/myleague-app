import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import StatAbbr from '../../../components/StatAbbr/StatAbbr';
import { MatchPlayerStatsTable, MatchTeamStatsCard, type MatchPlayerStatRow } from '../../../components/match';
import type { HockeyMatchStatisticsDto } from '../../../types/hockey/hockeyTypes';
import { formatHockeyFaceoffPercentage } from '../../../utils/hockeyLookups';
import { getPlayerPath } from '../../../utils/sportRoutes';
import '../../floorball/FloorballMatchPage/components/MatchStats.scss';

interface HockeyMatchPlayerStatsProps {
  stats: HockeyMatchStatisticsDto;
  homeName: string;
  awayName: string;
  homeTeamId: string | null;
  awayTeamId: string | null;
  playerNames: Map<string, string>;
  jerseyByPlayerId: Map<string, number>;
}

/**
 * Per-team skater points and goalie lines for one hockey match. Like the floorball
 * match page, only skaters who recorded a point or a penalty are listed.
 */
function HockeyMatchPlayerStats({
  stats,
  homeName,
  awayName,
  homeTeamId,
  awayTeamId,
  playerNames,
  jerseyByPlayerId,
}: HockeyMatchPlayerStatsProps) {
  const { t } = useTranslation();
  const sides: { side: 'home' | 'away'; teamId: string | null; teamName: string }[] = [
    { side: 'home', teamId: homeTeamId, teamName: homeName },
    { side: 'away', teamId: awayTeamId, teamName: awayName },
  ];

  return (
    <>
      {sides.map(({ side, teamId, teamName }) => {
        const teamPlayers: MatchPlayerStatRow[] = stats.players
          .filter((row) => row.teamId === teamId && (row.points > 0 || row.penaltyMinutes > 0))
          .map((row) => ({
            playerId: row.playerId,
            name: playerNames.get(row.playerId) ?? row.playerId.slice(0, 8),
            jerseyNumber: jerseyByPlayerId.get(row.playerId),
            goals: row.goals,
            assists: row.assists,
            points: row.points,
            penaltyMinutes: row.penaltyMinutes,
            faceoffPercentage: formatHockeyFaceoffPercentage(row.faceoffWins ?? 0, row.faceoffAttempts ?? 0),
          }));
        if (teamPlayers.length === 0) return null;
        return (
          <MatchPlayerStatsTable
            key={`players-${side}`}
            teamName={teamName}
            side={side}
            sport="hockey"
            rows={teamPlayers}
            showFaceoffs
          />
        );
      })}
      {sides.map(({ side, teamId, teamName }) => {
        const teamGoalies = stats.goalies.filter((row) => row.teamId === teamId);
        if (teamGoalies.length === 0) return null;
        return (
          <MatchTeamStatsCard key={`goalies-${side}`} teamName={teamName} side={side}>
            <table className="standing-table">
              <thead>
                <tr className="header-row">
                  <th className="stats-col">#</th>
                  <th className="team-col">{t('hockeyPage.goalie', 'Goalie')}</th>
                  <th className="stats-col"><StatAbbr abbr={t('hockeyPage.colSa', 'SA')} title={t('hockeyPage.colSaTitle', 'Shots against')} /></th>
                  <th className="stats-col"><StatAbbr abbr={t('hockeyPage.colSv', 'SV')} title={t('hockeyPage.colSvTitle', 'Saves')} /></th>
                  <th className="stats-col"><StatAbbr abbr={t('hockeyPage.colSvPct', 'SV%')} title={t('hockeyPage.colSvPctTitle', 'Save percentage')} /></th>
                  <th className="stats-col"><StatAbbr abbr={t('hockeyPage.colGa', 'GA')} title={t('hockeyPage.colGaTitle', 'Goals against')} /></th>
                </tr>
              </thead>
              <tbody>
                {teamGoalies.map((row) => (
                  <tr key={row.playerId}>
                    <td className="stats-col">{jerseyByPlayerId.get(row.playerId) ?? '—'}</td>
                    <td className="team-col">
                      <Link to={getPlayerPath('hockey', row.playerId)}>
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
          </MatchTeamStatsCard>
        );
      })}
    </>
  );
}

export default HockeyMatchPlayerStats;
