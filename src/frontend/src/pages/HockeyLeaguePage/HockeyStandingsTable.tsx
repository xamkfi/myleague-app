import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import type { HockeyTeamCompetitionStatisticsDto } from '../../types/hockey/hockeyTypes';
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
  const rows = previewLimit ? standings.slice(0, previewLimit) : standings;

  return (
    <table className="standing-table">
      <thead>
        <tr className="header-row">
          <th className="rank-col">#</th>
          <th className="team-col">{t('hockeyPage.team', 'TEAM')}</th>
          <th className="stats-col">{t('hockeyPage.colGp', 'GP')}</th>
          <th className="stats-col">{t('hockeyPage.colW', 'W')}</th>
          <th className="stats-col">{t('hockeyPage.colOtw', 'OTW')}</th>
          <th className="stats-col">{t('hockeyPage.colOtl', 'OTL')}</th>
          <th className="stats-col">{t('hockeyPage.colL', 'L')}</th>
          <th className="goals-col">{t('hockeyPage.colGf', 'GF')}</th>
          <th className="stats-col">{t('hockeyPage.colGaAbbr', 'GA')}</th>
          <th className="stats-col">{t('hockeyPage.colGd', 'GD')}</th>
          <th className="points-col">{t('hockeyPage.pointsShort', 'PTS')}</th>
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
