import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import type { HockeyTeamDto } from '../../../../../types/hockey/hockeyTypes';
import { loadHockeyRosterNameMaps } from '../../../../../utils/hockeyLookups';
import './TeamPlayersRow.scss';

interface TeamPlayersRowProps {
  teamId: string;
  isExpanded: boolean;
  isClosing: boolean;
  team?: HockeyTeamDto;
}

const POSITION_ORDER = ['Goalie', 'Defenseman', 'Center', 'LeftWing', 'RightWing'];

function TeamPlayersRow({ isExpanded, isClosing, team }: TeamPlayersRowProps) {
  const { t } = useTranslation();
  const [playerNames, setPlayerNames] = useState<Map<string, string>>(new Map());
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (!isExpanded || !team) {
      return;
    }
    let cancelled = false;
    const load = async (): Promise<void> => {
      setLoading(true);
      try {
        const names = await loadHockeyRosterNameMaps([team]);
        if (!cancelled) {
          setPlayerNames(names.byPlayerId);
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    };
    void load();
    return () => {
      cancelled = true;
    };
  }, [isExpanded, team]);

  const displayPlayers = team?.roster ?? [];
  const playerCount = displayPlayers.length;
  const playerPositions = [...new Set(displayPlayers.map((player) => player.position))].sort(
    (a, b) => POSITION_ORDER.indexOf(a) - POSITION_ORDER.indexOf(b),
  );

  return (
    <div className={`team-players-row ${isClosing ? 'is-closing' : ''}`}>
      <div className="team-players-container">
        <h4 className="players-title" />
        {loading && (
          <div className="players-loading">
            <p>{t('common.loading', 'Loading...')}</p>
          </div>
        )}
        {!loading && playerCount === 0 && (
          <div className="no-players">
            <p>{t('hockey.teams.noPlayersInTeam', 'This team has no players assigned yet.')}</p>
            <p className="help-text">
              {t('hockey.teams.addPlayersHelp', 'Use the edit button to manage team roster and add players.')}
            </p>
          </div>
        )}
        {!loading && playerCount > 0 && (
          <div className="admin-roster-section">
            {playerPositions.map((pos) => (
              <div key={pos} className="admin-roster-group">
                <div className="admin-roster-position-header">
                  {t(`hockey.positions.${pos}`, pos)}
                </div>
                <div className="admin-roster-table-header">
                  <span className="col-jersey">#</span>
                  <span className="col-name">{t('roster.name', 'Name')}</span>
                  <span className="col-stat">{t('hockey.roster.status', 'Status')}</span>
                </div>
                <div className="admin-roster-players">
                  {displayPlayers
                    .filter((player) => player.position === pos)
                    .map((player) => (
                      <div
                        key={player.id}
                        className={`admin-roster-player ${!player.isActive ? 'inactive' : ''}`}
                      >
                        <span className="col-jersey">{player.jerseyNumber ?? '?'}</span>
                        <span className="col-name">{playerNames.get(player.playerId) ?? player.playerId.slice(0, 8)}</span>
                        <span className="col-stat">{player.rosterStatus}</span>
                      </div>
                    ))}
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}

export default TeamPlayersRow;
