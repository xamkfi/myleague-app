import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import StatAbbr from '../../components/StatAbbr/StatAbbr';
import type { HockeyTeamCompetitionStatisticsDto } from '../../types/hockey/hockeyTypes';
import { uniqueHockeyStandingsByTeamId } from '../../utils/hockeyLookups';
import { getTeamSlug } from '../../utils/slugUtils';

interface HockeyStandingsTableProps {
  standings: HockeyTeamCompetitionStatisticsDto[];
  teamNames: Map<string, string>;
  previewLimit?: number;
}

function HockeyStandingsTable({ standings, teamNames, previewLimit }: HockeyStandingsTableProps) {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const namedTeams = [...teamNames.entries()].map(([id, name]) => ({ id, name }));
  const uniqueStandings = uniqueHockeyStandingsByTeamId(standings);
  const rows = previewLimit ? uniqueStandings.slice(0, previewLimit) : uniqueStandings;

  return (
    <table className="standing-table">
      <thead>
        <tr className="header-row">
          <th className="rank-col">#</th>
          <th className="team-col">{t('hockeyPage.team', 'TEAM')}</th>
          <th className="stats-col"><StatAbbr abbr={t('hockeyPage.colGp', 'GP')} title={t('hockeyPage.colGpTitle', 'Games played')} /></th>
          <th className="stats-col"><StatAbbr abbr={t('hockeyPage.colW', 'W')} title={t('hockeyPage.colWTitle', 'Wins')} /></th>
          <th className="stats-col"><StatAbbr abbr={t('hockeyPage.colOtw', 'OTW')} title={t('hockeyPage.colOtwTitle', 'Overtime wins')} /></th>
          <th className="stats-col"><StatAbbr abbr={t('hockeyPage.colOtl', 'OTL')} title={t('hockeyPage.colOtlTitle', 'Overtime losses')} /></th>
          <th className="stats-col"><StatAbbr abbr={t('hockeyPage.colL', 'L')} title={t('hockeyPage.colLTitle', 'Losses')} /></th>
          <th className="goals-col"><StatAbbr abbr={t('hockeyPage.colGf', 'GF')} title={t('hockeyPage.colGfTitle', 'Goals for')} /></th>
          <th className="stats-col"><StatAbbr abbr={t('hockeyPage.colGaAbbr', 'GA')} title={t('hockeyPage.colGaTitle', 'Goals against')} /></th>
          <th className="stats-col"><StatAbbr abbr={t('hockeyPage.colGd', 'GD')} title={t('hockeyPage.colGdTitle', 'Goal difference')} /></th>
          <th className="points-col"><StatAbbr abbr={t('hockeyPage.pointsShort', 'PTS')} title={t('hockeyPage.pointsShortTitle', 'Points')} /></th>
        </tr>
      </thead>
      <tbody>
        {rows.map((row) => {
          const name = teamNames.get(row.teamId) ?? row.teamId.slice(0, 8);
          return (
            <tr
              key={row.teamId}
              className="clickable-row"
              onClick={() => navigate(`/hockey/team/${getTeamSlug({ id: row.teamId, name }, namedTeams)}`)}
            >
              <td className="rank-col">{row.standingRank}</td>
              <td className="team-col">{name}</td>
              <td className="stats-col">{row.gamesPlayed}</td>
              <td className="stats-col">{row.regulationWins}</td>
              <td className="stats-col">{row.overtimeWins + row.shootoutWins}</td>
              <td className="stats-col">{row.overtimeLosses + row.shootoutLosses}</td>
              <td className="stats-col">{row.regulationLosses}</td>
              <td className="goals-col">{row.goalsFor}</td>
              <td className="stats-col">{row.goalsAgainst}</td>
              <td className="stats-col">{row.goalDifference}</td>
              <td className="points-col">{row.points}</td>
            </tr>
          );
        })}
      </tbody>
    </table>
  );
}

export default HockeyStandingsTable;
