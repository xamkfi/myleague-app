import { useTranslation } from 'react-i18next';
import { PlayerLink } from '../SportLinks';
import type { SportKind } from '../../utils/sportRoutes';
import '../../pages/FloorballTeamPage/components/RosterSection.scss';

export interface RosterPlayerRow {
  playerId: string;
  playerName: string;
  position: string;
  jerseyNumber?: number | null;
  age?: number;
  gamesPlayed?: number;
  goals?: number;
  assists?: number;
  nameSuffix?: string;
}

export interface RosterStatRow {
  playerId: string;
  gamesPlayed: number;
  goals: number;
  assists: number;
}

interface RosterSectionProps {
  sport: SportKind;
  players: RosterPlayerRow[];
  playerStatistics?: RosterStatRow[] | null;
  positionOrder: string[];
  positionLabel?: (position: string) => string;
}

export default function RosterSection({
  sport,
  players,
  playerStatistics,
  positionOrder,
  positionLabel,
}: RosterSectionProps) {
  const { t } = useTranslation();
  const statsLookup = new Map((playerStatistics ?? []).map((row) => [row.playerId, row]));
  const playerPositions = [...new Set(players.map((player) => player.position))].sort(
    (a, b) => positionOrder.indexOf(a) - positionOrder.indexOf(b),
  );

  return (
    <div className="roster-section">
      {playerPositions.map((pos) => (
        <div className="roster-container" key={pos}>
          <div className="roster-position-header">
            {positionLabel?.(pos) ?? t(`roster.positions.${pos}`, pos)}
          </div>
          <div className="roster-position-container">
            <div className="table-wrapper">
              <div className="table stats-header">
                <div className="roster-jersey" title={t('roster.tooltips.jerseyNumber')}>{t('roster.jerseyNumber')}</div>
                <div className="roster-player-name">{t('roster.name')}</div>
                <div className="roster-age" title={t('roster.tooltips.age')}>{t('roster.age')}</div>
                <div className="roster-games-played" title={t('roster.tooltips.matchesPlayed')}>{t('roster.matchesPlayed')}</div>
                <div className="roster-goals" title={t('roster.tooltips.goals')}>{t('roster.goals')}</div>
                <div className="roster-assists" title={t('roster.tooltips.assists')}>{t('roster.assists')}</div>
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
                      <div className="roster-jersey row">{player.jerseyNumber ?? '—'}</div>
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
                      <div className="roster-age">
                        {player.age && player.age !== 99 ? player.age : '—'}
                      </div>
                      <div className="roster-games-played">
                        {seasonStats?.gamesPlayed ?? player.gamesPlayed ?? '—'}
                      </div>
                      <div className="roster-goals">
                        {seasonStats?.goals ?? player.goals ?? '—'}
                      </div>
                      <div className="roster-assists">
                        {seasonStats?.assists ?? player.assists ?? '—'}
                      </div>
                    </div>
                  );
                })}
            </div>
          </div>
        </div>
      ))}
    </div>
  );
}
