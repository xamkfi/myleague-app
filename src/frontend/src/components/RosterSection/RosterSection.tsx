import { useTranslation } from 'react-i18next';
import { PlayerLink } from '../SportLinks';
import type { SportKind } from '../../utils/sportRoutes';
import { LicenceLegend, LicenceStatusDot } from '../LicenceStatus/LicenceStatus';
import '../../pages/floorball/FloorballTeamPage/components/RosterSection.scss';

/** Statistics a roster column can show. */
export type RosterStatColumn =
  | 'goals'
  | 'assists'
  | 'savePercentage'
  | 'goalsAgainstAverage'
  | 'yellowCards'
  | 'cleanSheets';

/** The two columns after games played. */
export type RosterStatColumns = readonly [RosterStatColumn, RosterStatColumn];

const DECIMAL_COLUMNS: ReadonlySet<RosterStatColumn> = new Set<RosterStatColumn>(['savePercentage', 'goalsAgainstAverage']);
const DEFAULT_FIELD_COLUMNS: RosterStatColumns = ['goals', 'assists'];
const DEFAULT_GOALIE_COLUMNS: RosterStatColumns = ['savePercentage', 'goalsAgainstAverage'];

export interface RosterPlayerRow {
  playerId: string;
  playerName: string;
  position: string;
  jerseyNumber?: number | null;
  gamesPlayed?: number;
  goals?: number;
  assists?: number;
  yellowCards?: number;
  nameSuffix?: string;
  /** Licence paid for this team. Leave undefined to hide the indicator. */
  licenceActive?: boolean;
}

export interface RosterStatRow {
  playerId: string;
  gamesPlayed: number;
  goals: number;
  assists: number;
  savePercentage?: number;
  goalsAgainstAverage?: number;
  yellowCards?: number;
  cleanSheets?: number;
}

interface RosterSectionProps {
  sport: SportKind;
  players: RosterPlayerRow[];
  playerStatistics?: RosterStatRow[] | null;
  positionOrder: string[];
  /** Positions whose group shows the goalie columns. */
  goaliePositions?: string[];
  fieldColumns?: RosterStatColumns;
  goalieColumns?: RosterStatColumns;
  positionLabel?: (position: string) => string;
}

function formatStat(column: RosterStatColumn, stats: RosterStatRow | undefined, player: RosterPlayerRow): string {
  const fallback: Partial<Record<RosterStatColumn, number>> = {
    goals: player.goals,
    assists: player.assists,
    yellowCards: player.yellowCards,
  };
  const value: number | undefined = stats?.[column] ?? fallback[column];
  if (value === undefined) return '—';
  return DECIMAL_COLUMNS.has(column) ? value.toFixed(2) : String(value);
}

export default function RosterSection({
  sport,
  players,
  playerStatistics,
  positionOrder,
  goaliePositions = [],
  fieldColumns = DEFAULT_FIELD_COLUMNS,
  goalieColumns = DEFAULT_GOALIE_COLUMNS,
  positionLabel,
}: RosterSectionProps) {
  const { t } = useTranslation();
  const statsLookup = new Map((playerStatistics ?? []).map((row) => [row.playerId, row]));
  const playerPositions = [...new Set(players.map((player) => player.position))].sort(
    (a, b) => positionOrder.indexOf(a) - positionOrder.indexOf(b),
  );

  const showLicence: boolean = players.some((player) => player.licenceActive !== undefined);

  return (
    <div className="roster-section">
      {showLicence && <LicenceLegend />}
      {playerPositions.map((pos) => {
        const [firstColumn, secondColumn]: RosterStatColumns = goaliePositions.includes(pos) ? goalieColumns : fieldColumns;
        return (
          <div className="roster-container" key={pos}>
            <div className="roster-position-header">
              {positionLabel?.(pos) ?? t(`roster.positions.${pos}`, pos)}
            </div>
            <div className="roster-position-container">
              <div className="table-wrapper">
                <div className="table stats-header">
                  <div className="roster-jersey" title={t('roster.tooltips.jerseyNumber')}>{t('roster.jerseyNumber')}</div>
                  <div className="roster-player-name">{t('roster.name')}</div>
                  <div className="roster-games-played" title={t('roster.tooltips.matchesPlayed')}>{t('roster.matchesPlayed')}</div>
                  <div className="roster-goals" title={t(`roster.tooltips.${firstColumn}`)}>{t(`roster.${firstColumn}`)}</div>
                  <div className="roster-assists" title={t(`roster.tooltips.${secondColumn}`)}>{t(`roster.${secondColumn}`)}</div>
                </div>
                {players
                  .filter((player) => player.position === pos)
                  .map((player) => {
                    const isCreator = player.playerName === 'Tuomas Reijonen';
                    const seasonStats = statsLookup.get(player.playerId);
                    return (
                      <div
                        className={`table roster-player${isCreator ? ' roster-player--creator' : ''}`}
                        key={player.playerId}
                      >
                        <div className="roster-jersey row licence-anchor">
                          {player.jerseyNumber ?? '—'}
                          {player.licenceActive !== undefined && <LicenceStatusDot active={player.licenceActive} />}
                        </div>
                        <div className="roster-player-name">
                          <PlayerLink sport={sport} playerId={player.playerId}>
                            {isCreator ? (
                              <>
                                <span className="creator-badge" title="System Creator">💪</span>
                                {` ${player.playerName} `}
                                <span className="creator-badge" title="System Creator">💪</span>
                              </>
                            ) : (
                              `${player.playerName}${player.nameSuffix ?? ''}`
                            )}
                          </PlayerLink>
                        </div>
                        <div className="roster-games-played">
                          {seasonStats?.gamesPlayed ?? player.gamesPlayed ?? '—'}
                        </div>
                        <div className="roster-goals">{formatStat(firstColumn, seasonStats, player)}</div>
                        <div className="roster-assists">{formatStat(secondColumn, seasonStats, player)}</div>
                      </div>
                    );
                  })}
              </div>
            </div>
          </div>
        );
      })}
    </div>
  );
}
