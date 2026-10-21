import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import StatAbbr from '../../../components/StatAbbr/StatAbbr';
import { MatchPlayerStatsTable, MatchTeamStatsCard, type MatchPlayerStatRow } from '../../../components/match';
import StatRow from '../../floorball/FloorballMatchPage/components/StatRow';
import { getTeamInitials } from '../../floorball/FloorballMatchPage/components/matchUtils';
import type { HockeyMatchStatisticsDto } from '../../../types/hockey/hockeyTypes';
import { formatHockeyFaceoffPercentage } from '../../../utils/hockeyLookups';
import { getPlayerPath } from '../../../utils/sportRoutes';
import '../../floorball/FloorballMatchPage/components/MatchStats.scss';

interface HockeyMatchStatsProps {
  stats: HockeyMatchStatisticsDto;
  homeName: string;
  awayName: string;
  homeTeamId: string | null;
  awayTeamId: string | null;
  playerNames: Map<string, string>;
  jerseyByPlayerId: Map<string, number>;
}

function HockeyMatchStats({
  stats,
  homeName,
  awayName,
  homeTeamId,
  awayTeamId,
  playerNames,
  jerseyByPlayerId,
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
  const sides: { side: 'home' | 'away'; teamId: string | null; teamName: string }[] = [
    { side: 'home', teamId: homeTeamId, teamName: homeName },
    { side: 'away', teamId: awayTeamId, teamName: awayName },
  ];

  return (
    <>
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
      {sides.map(({ side, teamId, teamName }) => {
        const teamPlayers: MatchPlayerStatRow[] = stats.players
          .filter((row) => row.teamId === teamId)
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

export default HockeyMatchStats;
