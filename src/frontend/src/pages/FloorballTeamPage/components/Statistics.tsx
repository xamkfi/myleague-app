import './Statistics.scss';
import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import LoadingSpinner from '../../../components/LoadingSpinner/LoadingSpinner';
import type { FloorballTeamSeasonStatisticsDto, FloorballPlayerSeasonStatisticsDto } from '../../../api/floorball/floorballStatistics';
import type { FloorballTeamPlayer } from '../../../types/floorball/floorballTypes';

type SortField = 'pts' | 'g' | 'a' | 'gp' | 'pim' | 'playerName';

interface StatisticsProps {
  teamStatistics?: FloorballTeamSeasonStatisticsDto | null;
  playerStatistics?: FloorballPlayerSeasonStatisticsDto[] | null;
  roster?: FloorballTeamPlayer[];
  loading?: boolean;
  error?: string | null;
  seasonName?: string;
}

export default function Statistics({ teamStatistics, playerStatistics, roster = [], loading, error, seasonName }: StatisticsProps) {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [sortField, setSortField] = useState<SortField>('pts');
  const [sortAsc, setSortAsc] = useState(false);

  if (loading) {
    return (
      <div className="statistics-block">
        <div className="statistics-empty-state">
          <LoadingSpinner size="lg" text={t('teamUserPage.stats.loading')} />
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="statistics-block">
        <div className="statistics-empty-state statistics-error">
          <h3>{t('teamUserPage.stats.error')}</h3>
          <p>{error}</p>
        </div>
      </div>
    );
  }

  if (!teamStatistics) {
    return (
      <div className="statistics-block">
        <div className="statistics-empty-state">
          <h3>{t('teamUserPage.stats.noStats')}</h3>
          <p>{t('teamUserPage.stats.noStatsDesc')}</p>
        </div>
      </div>
    );
  }

  const data = teamStatistics;
  const hasGames = data.gamesPlayed > 0;

  const pct = (value: number, total: number): string => {
    if (total === 0) return '0.0%';
    return `${((value / total) * 100).toFixed(1)}%`;
  };

  const perGame = (value: number): string => {
    if (!hasGames) return '0.0';
    return (value / data.gamesPlayed).toFixed(1);
  };

  const formatDiff = (value: number): string => {
    return value > 0 ? `+${value}` : `${value}`;
  };

  const positionAbbrev = (pos: string): string => {
    const map: Record<string, string> = {
      Goalkeeper: 'MV',
      Defender: 'P',
      Center: 'KH',
      Forward: 'H',
    };
    return map[pos] ?? pos.charAt(0).toUpperCase();
  };

  const handleSort = (field: SortField) => {
    if (sortField === field) {
      setSortAsc(prev => !prev);
    } else {
      setSortField(field);
      setSortAsc(field === 'playerName');
    }
  };

  const hasPlayerStats = playerStatistics && playerStatistics.length > 0;

  const sortedPlayerStats = hasPlayerStats
    ? [...playerStatistics].sort((a, b) => {
        const dir = sortAsc ? 1 : -1;
        switch (sortField) {
          case 'pts': {
            const diff = a.points - b.points;
            return diff !== 0 ? diff * dir : (a.goals - b.goals) * dir;
          }
          case 'g': return (a.goals - b.goals) * dir;
          case 'a': return (a.assists - b.assists) * dir;
          case 'gp': return (a.gamesPlayed - b.gamesPlayed) * dir;
          case 'pim': return (a.penaltyMinutes - b.penaltyMinutes) * dir;
          case 'playerName': return a.playerName.localeCompare(b.playerName) * dir;
          default: return 0;
        }
      })
    : null;

  const activePlayers = roster.filter(p => p.isActive);

  const sortedRosterPlayers = [...activePlayers].sort((a, b) => {
    const dir = sortAsc ? 1 : -1;
    switch (sortField) {
      case 'pts': {
        const diff = (a.goals + a.assists) - (b.goals + b.assists);
        return diff !== 0 ? diff * dir : (a.goals - b.goals) * dir;
      }
      case 'g': return (a.goals - b.goals) * dir;
      case 'a': return (a.assists - b.assists) * dir;
      case 'gp': return (a.gamesPlayed - b.gamesPlayed) * dir;
      case 'pim': return (a.penaltyMinutes - b.penaltyMinutes) * dir;
      case 'playerName': return a.playerName.localeCompare(b.playerName) * dir;
      default: return 0;
    }
  });

  return (
    <div className="statistics-container">
      {seasonName && (
        <div className="statistics-season-label">{seasonName}</div>
      )}

      {/* Overall Record */}
      <div className="statistics-block">
        <div className="statistics-block-title">
          {t('teamUserPage.stats.overallRecord')}
        </div>
        <div className="statistics-table-wrap">
          <div className="statistics-grid statistics-grid-header statistics-record-cols">
            <div>{t('teamUserPage.stats.gamesPlayed')}</div>
            <div>{t('teamUserPage.stats.wins')}</div>
            <div>{t('teamUserPage.stats.losses')}</div>
            <div>{t('teamUserPage.stats.ties')}</div>
            <div>{t('teamUserPage.stats.points')}</div>
            <div>PPG</div>
          </div>
          <div className="statistics-grid statistics-record-cols">
            <div className="statistics-cell">{data.gamesPlayed}</div>
            <div className="statistics-cell cell-win">{data.wins}</div>
            <div className="statistics-cell cell-loss">{data.losses}</div>
            <div className="statistics-cell">{data.ties}</div>
            <div className="statistics-cell cell-highlight">{data.points}</div>
            <div className="statistics-cell">{perGame(data.points)}</div>
          </div>
        </div>
      </div>

      {/* Goals & Shots */}
      <div className="statistics-block">
        <div className="statistics-block-title">
          {t('teamUserPage.stats.goals')}
        </div>
        <div className="statistics-table-wrap">
          <div className="statistics-grid statistics-grid-header statistics-goals-cols">
            <div>{t('teamUserPage.stats.goalsFor')}</div>
            <div>{t('teamUserPage.stats.goalsAgainst')}</div>
            <div>+/-</div>
            <div>SF</div>
            <div>SA</div>
            <div>S%</div>
          </div>
          <div className="statistics-grid statistics-goals-cols">
            <div className="statistics-cell">{data.goalsFor}</div>
            <div className="statistics-cell">{data.goalsAgainst}</div>
            <div className={`statistics-cell ${data.goalDifference > 0 ? 'cell-positive' : data.goalDifference < 0 ? 'cell-negative' : ''}`}>
              {formatDiff(data.goalDifference)}
            </div>
            <div className="statistics-cell">{data.shotsFor}</div>
            <div className="statistics-cell">{data.shotsAgainst}</div>
            <div className="statistics-cell">{data.shotPercentage.toFixed(1)}%</div>
          </div>
          <div className="statistics-grid statistics-grid-sub statistics-goals-cols">
            <div>{perGame(data.goalsFor)}/g</div>
            <div>{perGame(data.goalsAgainst)}/g</div>
            <div></div>
            <div>{perGame(data.shotsFor)}/g</div>
            <div>{perGame(data.shotsAgainst)}/g</div>
            <div></div>
          </div>
        </div>
      </div>

      {/* Special Teams */}
      <div className="statistics-block">
        <div className="statistics-block-title">
          {t('teamUserPage.stats.specialTeams')}
        </div>
        <div className="statistics-table-wrap">
          <div className="statistics-grid statistics-grid-header statistics-special-cols">
            <div>PP</div>
            <div>PP Opp</div>
            <div>PP%</div>
            <div>SHG</div>
            <div>PK Opp</div>
            <div>PK%</div>
            <div>PIM</div>
          </div>
          <div className="statistics-grid statistics-special-cols">
            <div className="statistics-cell">{data.powerPlayGoals}</div>
            <div className="statistics-cell">{data.powerPlayOpportunities}</div>
            <div className="statistics-cell">{data.powerPlayPercentage.toFixed(1)}%</div>
            <div className="statistics-cell">{data.shortHandedGoals}</div>
            <div className="statistics-cell">{data.penaltyKillOpportunities}</div>
            <div className="statistics-cell">{data.penaltyKillPercentage.toFixed(1)}%</div>
            <div className="statistics-cell">{data.penaltyMinutes}</div>
          </div>
        </div>
      </div>

      {/* Home vs Away */}
      <div className="statistics-block">
        <div className="statistics-block-title">
          {t('teamUserPage.stats.homeVsAway')}
        </div>
        <div className="statistics-ha-grid">
          <div className="statistics-ha-card">
            <div className="statistics-ha-title">
              {t('teamUserPage.stats.home')}
            </div>
            <div className="statistics-ha-rows">
              <div className="statistics-ha-row">
                <span className="statistics-ha-label">{t('teamUserPage.stats.wins')}</span>
                <span className="statistics-ha-value">{data.homeWins}</span>
              </div>
              <div className="statistics-ha-row">
                <span className="statistics-ha-label">{t('teamUserPage.stats.losses')}</span>
                <span className="statistics-ha-value">{data.homeLosses}</span>
              </div>
              {data.homeWins + data.homeLosses > 0 && (
                <div className="statistics-ha-row">
                  <span className="statistics-ha-label">{t('teamUserPage.stats.winPercentage')}</span>
                  <span className="statistics-ha-value">{pct(data.homeWins, data.homeWins + data.homeLosses)}</span>
                </div>
              )}
            </div>
          </div>
          <div className="statistics-ha-card">
            <div className="statistics-ha-title">
              {t('teamUserPage.stats.away')}
            </div>
            <div className="statistics-ha-rows">
              <div className="statistics-ha-row">
                <span className="statistics-ha-label">{t('teamUserPage.stats.wins')}</span>
                <span className="statistics-ha-value">{data.awayWins}</span>
              </div>
              <div className="statistics-ha-row">
                <span className="statistics-ha-label">{t('teamUserPage.stats.losses')}</span>
                <span className="statistics-ha-value">{data.awayLosses}</span>
              </div>
              {data.awayWins + data.awayLosses > 0 && (
                <div className="statistics-ha-row">
                  <span className="statistics-ha-label">{t('teamUserPage.stats.winPercentage')}</span>
                  <span className="statistics-ha-value">{pct(data.awayWins, data.awayWins + data.awayLosses)}</span>
                </div>
              )}
            </div>
          </div>
        </div>
      </div>

      {/* Player Statistics */}
      <div className="statistics-block">
        <div className="statistics-block-title">
          {t('teamUserPage.stats.playerStats')}
        </div>
        {(sortedPlayerStats && sortedPlayerStats.length > 0) ? (
          <div className="statistics-table-wrap">
            <table className="player-stats-table">
              <thead>
                <tr>
                  <th className="ps-col-rank">#</th>
                  <th
                    className={`ps-col-name sortable ${sortField === 'playerName' ? 'sorted' : ''}`}
                    onClick={() => handleSort('playerName')}
                  >
                    {t('teamUserPage.stats.playerName')}
                    {sortField === 'playerName' && <span className="sort-arrow">{sortAsc ? '▲' : '▼'}</span>}
                  </th>
                  <th
                    className={`ps-col-num sortable ${sortField === 'gp' ? 'sorted' : ''}`}
                    onClick={() => handleSort('gp')}
                  >
                    {t('teamUserPage.stats.gp')}
                    {sortField === 'gp' && <span className="sort-arrow">{sortAsc ? '▲' : '▼'}</span>}
                  </th>
                  <th
                    className={`ps-col-num sortable ${sortField === 'g' ? 'sorted' : ''}`}
                    onClick={() => handleSort('g')}
                  >
                    {t('teamUserPage.stats.g')}
                    {sortField === 'g' && <span className="sort-arrow">{sortAsc ? '▲' : '▼'}</span>}
                  </th>
                  <th
                    className={`ps-col-num sortable ${sortField === 'a' ? 'sorted' : ''}`}
                    onClick={() => handleSort('a')}
                  >
                    {t('teamUserPage.stats.a')}
                    {sortField === 'a' && <span className="sort-arrow">{sortAsc ? '▲' : '▼'}</span>}
                  </th>
                  <th
                    className={`ps-col-num ps-col-pts sortable ${sortField === 'pts' ? 'sorted' : ''}`}
                    onClick={() => handleSort('pts')}
                  >
                    {t('teamUserPage.stats.pts')}
                    {sortField === 'pts' && <span className="sort-arrow">{sortAsc ? '▲' : '▼'}</span>}
                  </th>
                  <th
                    className={`ps-col-num sortable ${sortField === 'pim' ? 'sorted' : ''}`}
                    onClick={() => handleSort('pim')}
                  >
                    {t('teamUserPage.stats.pim')}
                    {sortField === 'pim' && <span className="sort-arrow">{sortAsc ? '▲' : '▼'}</span>}
                  </th>
                </tr>
              </thead>
              <tbody>
                {sortedPlayerStats.map((player, idx) => (
                  <tr
                    key={player.playerId}
                    className="ps-row"
                    onClick={() => navigate(`/floorballplayer/${player.playerId}`)}
                  >
                    <td className="ps-col-rank">{idx + 1}</td>
                    <td className="ps-col-name">{player.playerName}</td>
                    <td className="ps-col-num">{player.gamesPlayed}</td>
                    <td className="ps-col-num">{player.goals}</td>
                    <td className="ps-col-num">{player.assists}</td>
                    <td className="ps-col-num ps-col-pts-value">{player.points}</td>
                    <td className="ps-col-num">{player.penaltyMinutes}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : roster.length > 0 ? (
          <div className="statistics-table-wrap">
            <table className="player-stats-table">
              <thead>
                <tr>
                  <th className="ps-col-rank">#</th>
                  <th
                    className={`ps-col-name sortable ${sortField === 'playerName' ? 'sorted' : ''}`}
                    onClick={() => handleSort('playerName')}
                  >
                    {t('teamUserPage.stats.playerName')}
                    {sortField === 'playerName' && <span className="sort-arrow">{sortAsc ? '▲' : '▼'}</span>}
                  </th>
                  <th className="ps-col-pos">{t('teamUserPage.stats.position')}</th>
                  <th
                    className={`ps-col-num sortable ${sortField === 'gp' ? 'sorted' : ''}`}
                    onClick={() => handleSort('gp')}
                  >
                    {t('teamUserPage.stats.gp')}
                    {sortField === 'gp' && <span className="sort-arrow">{sortAsc ? '▲' : '▼'}</span>}
                  </th>
                  <th
                    className={`ps-col-num sortable ${sortField === 'g' ? 'sorted' : ''}`}
                    onClick={() => handleSort('g')}
                  >
                    {t('teamUserPage.stats.g')}
                    {sortField === 'g' && <span className="sort-arrow">{sortAsc ? '▲' : '▼'}</span>}
                  </th>
                  <th
                    className={`ps-col-num sortable ${sortField === 'a' ? 'sorted' : ''}`}
                    onClick={() => handleSort('a')}
                  >
                    {t('teamUserPage.stats.a')}
                    {sortField === 'a' && <span className="sort-arrow">{sortAsc ? '▲' : '▼'}</span>}
                  </th>
                  <th
                    className={`ps-col-num ps-col-pts sortable ${sortField === 'pts' ? 'sorted' : ''}`}
                    onClick={() => handleSort('pts')}
                  >
                    {t('teamUserPage.stats.pts')}
                    {sortField === 'pts' && <span className="sort-arrow">{sortAsc ? '▲' : '▼'}</span>}
                  </th>
                  <th
                    className={`ps-col-num sortable ${sortField === 'pim' ? 'sorted' : ''}`}
                    onClick={() => handleSort('pim')}
                  >
                    {t('teamUserPage.stats.pim')}
                    {sortField === 'pim' && <span className="sort-arrow">{sortAsc ? '▲' : '▼'}</span>}
                  </th>
                </tr>
              </thead>
              <tbody>
                {sortedRosterPlayers.map((player, idx) => (
                  <tr
                    key={player.playerId}
                    className="ps-row"
                    onClick={() => navigate(`/floorballplayer/${player.playerId}`)}
                  >
                    <td className="ps-col-rank">{idx + 1}</td>
                    <td className="ps-col-name">{player.playerName}</td>
                    <td className="ps-col-pos">{positionAbbrev(player.position)}</td>
                    <td className="ps-col-num">{player.gamesPlayed}</td>
                    <td className="ps-col-num">{player.goals}</td>
                    <td className="ps-col-num">{player.assists}</td>
                    <td className="ps-col-num ps-col-pts-value">{player.goals + player.assists}</td>
                    <td className="ps-col-num">{player.penaltyMinutes}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <p className="ps-no-data">{t('teamUserPage.stats.noRosterData')}</p>
        )}
      </div>
    </div>
  );
}
