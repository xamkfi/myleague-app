import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import StatAbbr from '../StatAbbr/StatAbbr';
import { getPlayerPath } from '../../utils/sportRoutes';
import MatchTeamStatsCard from './MatchTeamStatsCard';

export interface MatchPlayerStatRow {
  playerId: string;
  name: string;
  jerseyNumber?: number;
  goals: number;
  assists: number;
  points: number;
  penaltyMinutes: number;
  /** Already formatted faceoff win percentage; shown only when `showFaceoffs` is set. */
  faceoffPercentage?: string;
}

interface MatchPlayerStatsTableProps {
  teamName: string;
  side: 'home' | 'away';
  sport: 'floorball' | 'hockey';
  rows: MatchPlayerStatRow[];
  showFaceoffs?: boolean;
}

/** One team's skater points for a single match, sorted by points then goals. */
export default function MatchPlayerStatsTable({
  teamName,
  side,
  sport,
  rows,
  showFaceoffs = false,
}: MatchPlayerStatsTableProps) {
  const { t } = useTranslation();
  const sorted: MatchPlayerStatRow[] = [...rows].sort((a, b) => b.points - a.points || b.goals - a.goals);

  return (
    <MatchTeamStatsCard teamName={teamName} side={side}>
      <table className="standing-table">
        <thead>
          <tr className="header-row">
            <th className="stats-col">#</th>
            <th className="team-col">{t('hockeyPage.player', 'Player')}</th>
            <th className="stats-col"><StatAbbr abbr={t('hockeyPage.colG', 'G')} title={t('hockeyPage.colGTitle', 'Goals')} /></th>
            <th className="stats-col"><StatAbbr abbr={t('hockeyPage.colA', 'A')} title={t('hockeyPage.colATitle', 'Assists')} /></th>
            <th className="points-col"><StatAbbr abbr={t('hockeyPage.colP', 'P')} title={t('hockeyPage.colPTitle', 'Points')} /></th>
            <th className="stats-col"><StatAbbr abbr={t('hockeyPage.colPim', 'PIM')} title={t('hockeyPage.colPimTitle', 'Penalty minutes')} /></th>
            {showFaceoffs && (
              <th className="stats-col"><StatAbbr abbr={t('hockeyPage.colFo', 'FO%')} title={t('hockeyPage.colFoTitle', 'Faceoff win percentage')} /></th>
            )}
          </tr>
        </thead>
        <tbody>
          {sorted.map((row) => (
            <tr key={row.playerId}>
              <td className="stats-col">{row.jerseyNumber ?? '—'}</td>
              <td className="team-col">
                <Link to={getPlayerPath(sport, row.playerId)}>{row.name}</Link>
              </td>
              <td className="stats-col">{row.goals}</td>
              <td className="stats-col">{row.assists}</td>
              <td className="points-col">{row.points}</td>
              <td className="stats-col">{row.penaltyMinutes}</td>
              {showFaceoffs && <td className="stats-col">{row.faceoffPercentage ?? '—'}</td>}
            </tr>
          ))}
        </tbody>
      </table>
    </MatchTeamStatsCard>
  );
}
