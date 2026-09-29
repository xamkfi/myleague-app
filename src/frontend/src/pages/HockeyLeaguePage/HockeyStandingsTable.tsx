import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import StatAbbr from '../../components/StatAbbr/StatAbbr';
import TeamLogoMark from '../../components/TeamLogoMark/TeamLogoMark';
import type { HockeyTeamCompetitionStatisticsDto } from '../../types/hockey/hockeyTypes';
import { uniqueHockeyStandingsByTeamId } from '../../utils/hockeyLookups';
import { getTeamSlug } from '../../utils/slugUtils';
import { getTeamPath } from '../../utils/sportRoutes';
import '../../components/TournamentGroupStandingsTable/TournamentGroupStandingsTable.scss';
import '../../components/LeagueStanding/LeagueStanding.scss';

interface HockeyStandingsTableProps {
  standings: HockeyTeamCompetitionStatisticsDto[];
  teamNames: Map<string, string>;
  teamLogos?: Map<string, string | null>;
  teamMarks?: Map<string, string | null>;
  competitionId?: string | null;
  previewLimit?: number;
  teamsAdvancing?: number;
}

function HockeyStandingsTable({
  standings,
  teamNames,
  teamLogos,
  teamMarks,
  competitionId,
  previewLimit,
  teamsAdvancing = 0,
}: HockeyStandingsTableProps) {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const namedTeams = [...teamNames.entries()].map(([id, name]) => ({ id, name }));
  const uniqueStandings = uniqueHockeyStandingsByTeamId(standings);
  const rows = previewLimit ? uniqueStandings.slice(0, previewLimit) : uniqueStandings;

  return (
    <div className="table-wrapper standing-container">
    <table className="standing-table standing-table--wide">
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
        {rows.map((row, index) => {
          const name = teamNames.get(row.teamId) || row.teamName || row.teamId.slice(0, 8);
          const isQualifying = teamsAdvancing > 0 && index < teamsAdvancing;
          return (
            <tr
              key={row.teamId}
              className={`clickable-row${isQualifying ? ' qualifying-row' : ''}`}
              title={isQualifying ? t('seasonStandings.qualifyingHint') : undefined}
              onClick={() => navigate(getTeamPath('hockey', getTeamSlug({ id: row.teamId, name }, namedTeams), competitionId))}
            >
              <td className="rank-col">
                {isQualifying && <span className="qualifying-marker" aria-hidden="true" />}
                {row.standingRank || index + 1}
              </td>
              <td className="team-col">
                <div className="team-info">
                  <TeamLogoMark
                    logo={teamLogos?.get(row.teamId)}
                    name={name}
                    mark={teamMarks?.get(row.teamId)}
                    imageClassName="logo-image"
                    fallbackClassName="logo-empty"
                  />
                  <span className="team-name">{name}</span>
                </div>
              </td>
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
    {teamsAdvancing > 0 && rows.length > 0 && (
      <div className="qualifying-legend">
        <span className="qualifying-legend__swatch" aria-hidden="true" />
        <span>{t('seasonStandings.qualifyingLegend', { count: teamsAdvancing })}</span>
      </div>
    )}
    </div>
  );
}

export default HockeyStandingsTable;
