import { useCallback, useEffect, useMemo, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';
import SearchField from '../../../../components/SearchField';
import Button from '../../../../components/Button/Button';
import AddIcon from '../../../../assets/basicIcons/add.svg';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';
import { hockeyTeamService } from '../../../../api/hockey/hockeyTeamService';
import { hockeyPlayerService } from '../../../../api/hockey/hockeyPlayerService';
import {
  HOCKEY_CAPTAIN_ROLES,
  HOCKEY_POSITIONS,
  type HockeyCaptainRole,
  type HockeyPosition,
  type HockeyTeamDto,
  type HockeyTeamPlayerDto,
} from '../../../../types/hockey/hockeyTypes';
import { loadPersonNameMap } from '../../../../utils/hockeyLookups';
import JerseyNumberSelect, { collectJerseyNumbers } from '../../../../components/JerseyNumberSelect';
import './EditRosterPage.scss';

function EditHockeyRosterPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { id: teamId } = useParams<{ id: string }>();
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [currentTeam, setCurrentTeam] = useState<HockeyTeamDto | null>(null);
  const [names, setNames] = useState<Map<string, string>>(new Map());
  const [searchTerm, setSearchTerm] = useState('');
  const [dropdownOpen, setDropdownOpen] = useState<string | null>(null);
  const [updatingPlayer, setUpdatingPlayer] = useState<string | null>(null);

  const loadTeamData = useCallback(async (): Promise<void> => {
    if (!teamId) {
      return;
    }
    try {
      setLoading(true);
      const team = await hockeyTeamService.getById(teamId);
      setCurrentTeam(team);
      const nameEntries = await Promise.all(
        team.roster.map(async (row) => {
          try {
            const player = await hockeyPlayerService.getById(row.playerId);
            const people = await loadPersonNameMap([player.personId]);
            return [row.playerId, people.get(player.personId) ?? row.playerId.slice(0, 8)] as const;
          } catch {
            return [row.playerId, row.playerId.slice(0, 8)] as const;
          }
        }),
      );
      setNames(new Map(nameEntries));
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load team data');
    } finally {
      setLoading(false);
    }
  }, [teamId]);

  useEffect(() => {
    void loadTeamData();
  }, [loadTeamData]);

  const filteredRoster = currentTeam?.roster.filter((player) =>
    (names.get(player.playerId) ?? '').toLowerCase().includes(searchTerm.toLowerCase()),
  ) ?? [];

  const handleUpdate = async (
    player: HockeyTeamPlayerDto,
    patch: Partial<Pick<HockeyTeamPlayerDto, 'position' | 'jerseyNumber' | 'rosterStatus' | 'captainRole'>>,
  ): Promise<void> => {
    if (!teamId) {
      return;
    }
    try {
      setUpdatingPlayer(player.playerId);
      setError(null);
      await hockeyTeamService.updatePlayer(teamId, player.playerId, {
        position: (patch.position ?? player.position) as HockeyPosition,
        jerseyNumber: patch.jerseyNumber === undefined ? player.jerseyNumber : patch.jerseyNumber,
        rosterStatus: patch.rosterStatus ?? player.rosterStatus,
        captainRole: patch.captainRole ?? player.captainRole,
      });
      await loadTeamData();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to update player');
    } finally {
      setUpdatingPlayer(null);
    }
  };

  const handleRemovePlayer = async (playerId: string): Promise<void> => {
    if (!teamId) {
      return;
    }
    if (!window.confirm(t('hockey.teams.confirmRemovePlayer', 'Are you sure you want to remove this player from the team?'))) {
      return;
    }
    try {
      setError(null);
      await hockeyTeamService.removePlayer(teamId, playerId);
      await loadTeamData();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to remove player');
    }
    setDropdownOpen(null);
  };

  const handleToggleActive = async (player: HockeyTeamPlayerDto): Promise<void> => {
    const nextStatus = player.rosterStatus === 'Active' ? 'Inactive' : 'Active';
    await handleUpdate(player, { rosterStatus: nextStatus });
    setDropdownOpen(null);
  };

  useEffect(() => {
    const handleClickOutside = (): void => setDropdownOpen(null);
    if (dropdownOpen) {
      document.addEventListener('click', handleClickOutside);
      return () => document.removeEventListener('click', handleClickOutside);
    }
  }, [dropdownOpen]);

  const takenJerseyNumbers = useMemo(
    () => collectJerseyNumbers(currentTeam?.roster ?? []),
    [currentTeam],
  );

  if (loading) {
    return (
      <PageTemplate title={t('common.loading', 'Loading...')}>
        <div className="edit-roster-loading">
          <p>{t('common.loading', 'Loading...')}</p>
        </div>
      </PageTemplate>
    );
  }

  if (!teamId || !currentTeam) {
    return (
      <PageTemplate title={t('hockey.teams.editRoster', 'Edit Roster')}>
        <ErrorPopup message={error || 'Team not found'} />
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title={`${t('hockey.teams.editRoster', 'Edit Roster')} - ${currentTeam.name}`}>
      <div className="edit-roster-container">
        <h2 className="edit-roster-title">
          {t('hockey.teams.manageTeamRoster', 'MANAGE TEAM ROSTER')}
        </h2>
        <div className="team-info-header">
          <span className="team-name">{currentTeam.name}</span>
          <span className="roster-count">{currentTeam.roster.length} {t('hockey.teams.players', 'players')}</span>
        </div>
        <div className="edit-roster-header">
          <div className="roster-actions">
            <SearchField
              value={searchTerm}
              onChange={setSearchTerm}
              placeholder={t('hockey.teams.searchPlayers', 'Search players...')}
              fullWidth
              rounded="pill"
            />
            <Button
              className="add-player-button"
              iconLeft={AddIcon}
              onClick={() => navigate(`/admin/hockey/teams/${teamId}/roster/add`)}
            >
              {t('hockey.teams.addPlayerToTeam', 'Add New Player to Team')}
            </Button>
            <Button onClick={() => navigate(`/admin/hockey/teams/${teamId}/lines`)}>
              {t('hockey.teams.lines', 'Lines')}
            </Button>
          </div>
        </div>
        <ErrorPopup message={error} />
        <div className="roster-table-wrapper">
          <table className="roster-table">
            <thead>
              <tr>
                <th className="name-column">{t('hockey.players.name', 'NAME')}</th>
                <th className="jersey-column">{t('hockey.players.jersey', 'JERSEY')}</th>
                <th className="position-column">{t('hockey.players.position', 'POSITION')}</th>
                <th className="status-column">{t('hockey.players.status', 'STATUS')}</th>
                <th className="actions-column">{t('common.actions', 'ACTIONS')}</th>
              </tr>
            </thead>
            <tbody>
              {filteredRoster.length === 0 ? (
                <tr>
                  <td colSpan={5} className="no-players">
                    {searchTerm
                      ? t('hockey.teams.noPlayersFound', 'No players found matching your search')
                      : t('hockey.teams.noPlayersInRoster', 'No players in this team roster')}
                  </td>
                </tr>
              ) : (
                filteredRoster.map((player) => (
                  <tr key={player.id} className="roster-row">
                    <td className="name-column">
                      <span className="player-name">{names.get(player.playerId) ?? player.playerId.slice(0, 8)}</span>
                    </td>
                    <td className="jersey-column">
                      <JerseyNumberSelect
                        className="jersey-select"
                        value={player.jerseyNumber}
                        takenNumbers={takenJerseyNumbers}
                        prefixHash
                        disabled={updatingPlayer === player.playerId}
                        onChange={(next) => {
                          void handleUpdate(player, { jerseyNumber: next });
                        }}
                      />
                    </td>
                    <td className="position-column">
                      <select
                        className="position-select"
                        value={player.position}
                        onChange={(event) => void handleUpdate(player, { position: event.target.value as HockeyPosition })}
                        disabled={updatingPlayer === player.playerId}
                      >
                        {HOCKEY_POSITIONS.map((position) => (
                          <option key={position} value={position}>{t(`hockey.positions.${position}`, position)}</option>
                        ))}
                      </select>
                    </td>
                    <td className="status-column">
                      <span className={`status-badge ${player.rosterStatus === 'Active' ? 'active' : 'inactive'}`}>
                        {player.rosterStatus === 'Active' ? '✓' : '✗'}
                      </span>
                    </td>
                    <td className="actions-column">
                      <div className="player-actions-dropdown">
                        <button
                          type="button"
                          className="dropdown-trigger"
                          onClick={(event) => {
                            event.stopPropagation();
                            setDropdownOpen(dropdownOpen === player.playerId ? null : player.playerId);
                          }}
                        >
                          <span className="three-dots">⋮</span>
                        </button>
                        {dropdownOpen === player.playerId && (
                          <div className="dropdown-menu" onClick={(event) => event.stopPropagation()}>
                            <button type="button" className="dropdown-item status-item" onClick={() => void handleToggleActive(player)}>
                              {player.rosterStatus === 'Active'
                                ? t('hockey.players.setInactive', 'Set Inactive')
                                : t('hockey.players.setActive', 'Set Active')}
                            </button>
                            <div className="dropdown-item">
                              <select
                                value={player.captainRole}
                                onChange={(event) => void handleUpdate(player, { captainRole: event.target.value as HockeyCaptainRole })}
                              >
                                {HOCKEY_CAPTAIN_ROLES.map((role) => (
                                  <option key={role} value={role}>{role}</option>
                                ))}
                              </select>
                            </div>
                            <button type="button" className="dropdown-item delete-item" onClick={() => void handleRemovePlayer(player.playerId)}>
                              {t('hockey.teams.removeFromTeam', 'Remove from Team')}
                            </button>
                          </div>
                        )}
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>
    </PageTemplate>
  );
}

export default EditHockeyRosterPage;
