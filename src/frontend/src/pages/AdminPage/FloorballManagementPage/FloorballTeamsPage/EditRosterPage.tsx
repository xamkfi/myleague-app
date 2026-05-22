import { useState, useEffect, useCallback } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';
import { floorballTeamService } from '../../../../api/floorball/floorballTeamService';
import { 
  FloorballPosition,
  type FloorballTeam,
  type FloorballTeamPlayer,
  type UpdateFloorballTeamPlayerRequest
} from '../../../../types/floorball/floorballTypes';
import SearchField from '../../../../components/SearchField';
import Button from '../../../../components/Button/Button';
import AddIcon from '../../../../assets/basicIcons/add.svg';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';
import './EditRosterPage.scss';

const EditRosterPage = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { id: teamId } = useParams<{ id: string }>();
  
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [currentTeam, setCurrentTeam] = useState<FloorballTeam | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [dropdownOpen, setDropdownOpen] = useState<string | null>(null);
  const [updatingPlayer, setUpdatingPlayer] = useState<string | null>(null);

  const loadTeamData = useCallback(async () => {
    if (!teamId) return;
    
    try {
      setLoading(true);
      const team = await floorballTeamService.getById(teamId);
      setCurrentTeam(team);
      setError(null);
    } catch (err) {
      console.error('Error loading team data:', err);
      setError(err instanceof Error ? err.message : 'Failed to load team data');
    } finally {
      setLoading(false);
    }
  }, [teamId]);

  useEffect(() => {
    loadTeamData();
  }, [loadTeamData]);

  // Filter roster by search term
  const filteredRoster = currentTeam?.roster?.filter(player =>
    player.playerName.toLowerCase().includes(searchTerm.toLowerCase())
  ) || [];

  // Handle removing a player from the team
  const handleRemovePlayer = async (playerId: string) => {
    if (!teamId) return;
    
    const confirmRemove = window.confirm(t('floorball.teams.confirmRemovePlayer', 'Are you sure you want to remove this player from the team?'));
    if (!confirmRemove) return;

    try {
      setError(null);
      await floorballTeamService.removePlayerFromTeam(teamId, playerId);
      await loadTeamData();
    } catch (err) {
      console.error('Error removing player:', err);
      setError(err instanceof Error ? err.message : 'Failed to remove player');
    }
    setDropdownOpen(null);
  };

  // Handle toggling player active status
  const handleToggleActive = async (player: FloorballTeamPlayer) => {
    if (!teamId) return;

    try {
      setError(null);
      const updateData: UpdateFloorballTeamPlayerRequest = {
        position: player.position,
        jerseyNumber: player.jerseyNumber,
        isActive: !player.isActive
      };
      await floorballTeamService.updateTeamPlayer(teamId, player.playerId, updateData);
      await loadTeamData();
    } catch (err) {
      console.error('Error updating player status:', err);
      setError(err instanceof Error ? err.message : 'Failed to update player status');
    }
    setDropdownOpen(null);
  };

  // Handle updating player position
  const handleUpdatePosition = async (player: FloorballTeamPlayer, newPosition: FloorballPosition) => {
    if (!teamId || player.position === newPosition) return;

    try {
      setUpdatingPlayer(player.playerId);
      setError(null);
      const updateData: UpdateFloorballTeamPlayerRequest = {
        position: newPosition,
        jerseyNumber: player.jerseyNumber,
        isActive: player.isActive
      };
      await floorballTeamService.updateTeamPlayer(teamId, player.playerId, updateData);
      await loadTeamData();
    } catch (err) {
      console.error('Error updating player position:', err);
      setError(err instanceof Error ? err.message : 'Failed to update player position');
    } finally {
      setUpdatingPlayer(null);
    }
  };

  // Handle updating player jersey number
  const handleUpdateJerseyNumber = async (player: FloorballTeamPlayer, newJerseyNumber: number | undefined) => {
    if (!teamId || player.jerseyNumber === newJerseyNumber) return;

    try {
      setUpdatingPlayer(player.playerId);
      setError(null);
      const updateData: UpdateFloorballTeamPlayerRequest = {
        position: player.position,
        jerseyNumber: newJerseyNumber,
        isActive: player.isActive
      };
      await floorballTeamService.updateTeamPlayer(teamId, player.playerId, updateData);
      await loadTeamData();
    } catch (err) {
      console.error('Error updating jersey number:', err);
      setError(err instanceof Error ? err.message : 'Failed to update jersey number');
    } finally {
      setUpdatingPlayer(null);
    }
  };

  // Generate jersey number options (1-99 plus "None" option)
  const jerseyNumberOptions = [
    { value: '', label: '-' },
    ...Array.from({ length: 99 }, (_, i) => ({ 
      value: String(i + 1), 
      label: `#${i + 1}` 
    }))
  ];

  // Position options for dropdown
  const positionOptions = [
    { value: FloorballPosition.None, label: t('floorball.positions.none', 'None') },
    { value: FloorballPosition.Goalkeeper, label: t('floorball.positions.goalkeeper', 'Goalkeeper') },
    { value: FloorballPosition.Defender, label: t('floorball.positions.defender', 'Defender') },
    { value: FloorballPosition.Forward, label: t('floorball.positions.forward', 'Forward') },
  ];

  // Handle adding new player (navigate to add player page)
  const handleAddPlayer = () => {
    navigate(`/admin/floorball/teams/${teamId}/roster/add`);
  };

  // Handle dropdown toggle
  const toggleDropdown = (playerId: string) => {
    setDropdownOpen(dropdownOpen === playerId ? null : playerId);
  };

  // Close dropdown when clicking outside
  useEffect(() => {
    const handleClickOutside = () => {
      setDropdownOpen(null);
    };

    if (dropdownOpen) {
      document.addEventListener('click', handleClickOutside);
      return () => document.removeEventListener('click', handleClickOutside);
    }
  }, [dropdownOpen]);

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
      <PageTemplate title={t('floorball.teams.editRoster', 'Edit Roster')}>
        <ErrorPopup message={error || 'Team not found'} />
      </PageTemplate>
    );
  }

  const rosterCount = currentTeam.roster?.length || 0;

  return (
    <PageTemplate title={`${t('floorball.teams.editRoster', 'Edit Roster')} - ${currentTeam.name}`}>
      <div className="edit-roster-container">
        <h2 className="edit-roster-title">
          {t('floorball.teams.manageTeamRoster', 'MANAGE TEAM ROSTER')}
        </h2>
        
        <div className="team-info-header">
          <span className="team-name">{currentTeam.name}</span>
          <span className="roster-count">{rosterCount} {t('floorball.teams.players', 'players')}</span>
        </div>

        <div className="edit-roster-header">
          <div className="roster-actions">
            <SearchField
              value={searchTerm}
              onChange={setSearchTerm}
              placeholder={t('floorball.teams.searchPlayers', 'Search players...')}
              fullWidth
              rounded="pill"
            />
            <Button
              className="add-player-button"
              iconLeft={AddIcon}
              onClick={handleAddPlayer}
            >
              {t('floorball.teams.addPlayerToTeam', 'Add New Player to Team')}
            </Button>
          </div>
        </div>

        <ErrorPopup message={error} />

        <div className="roster-table-wrapper">
          <table className="roster-table">
            <thead>
              <tr>
                <th className="name-column">{t('floorball.players.name', 'NAME')}</th>
                <th className="jersey-column">{t('floorball.players.jersey', 'JERSEY')}</th>
                <th className="position-column">{t('floorball.players.position', 'POSITION')}</th>
                <th className="status-column">{t('floorball.players.status', 'STATUS')}</th>
                <th className="actions-column">{t('common.actions', 'ACTIONS')}</th>
              </tr>
            </thead>
            <tbody>
              {filteredRoster.length === 0 ? (
                <tr>
                  <td colSpan={5} className="no-players">
                    {searchTerm 
                      ? t('floorball.teams.noPlayersFound', 'No players found matching your search')
                      : t('floorball.teams.noPlayersInRoster', 'No players in this team roster')
                    }
                  </td>
                </tr>
              ) : (
                filteredRoster.map((player) => {
                  // A "substituted" jersey is one whose originally-requested number (typically
                  // set by the tournament import flow when the preferred number was taken)
                  // differs from the actually-assigned number. We highlight the row until the
                  // admin picks a different number — at which point the backend clears the
                  // requestedJerseyNumber and the highlight disappears on the next refresh.
                  const hasSubstitutedJersey: boolean =
                    typeof player.requestedJerseyNumber === 'number'
                    && player.requestedJerseyNumber !== player.jerseyNumber;
                  const jerseyTooltip: string | undefined = hasSubstitutedJersey
                    ? t(
                        'floorball.teams.requestedJerseyTooltip',
                        'Requested #{{requested}} during import but it was taken; assigned #{{assigned}} instead. Pick a different number to clear this notice.',
                        {
                          requested: player.requestedJerseyNumber,
                          assigned: player.jerseyNumber ?? '–',
                        }
                      )
                    : undefined;

                  return (
                  <tr
                    key={player.playerId}
                    className={hasSubstitutedJersey ? 'roster-row roster-row--substituted-jersey' : 'roster-row'}
                    title={jerseyTooltip}
                  >
                    <td className="name-column">
                      <span className="player-name">{player.playerName}</span>
                    </td>
                    <td className="jersey-column">
                      <select
                        className={`jersey-select${hasSubstitutedJersey ? ' jersey-select--substituted' : ''}`}
                        value={player.jerseyNumber !== undefined && player.jerseyNumber !== null
                          ? String(player.jerseyNumber)
                          : ''}
                        onChange={(e) => {
                          const value = e.target.value;
                          const jerseyNum = value === '' ? undefined : parseInt(value, 10);
                          handleUpdateJerseyNumber(player, jerseyNum);
                        }}
                        disabled={updatingPlayer === player.playerId}
                        title={jerseyTooltip}
                      >
                        {jerseyNumberOptions.map((option) => (
                          <option key={option.value} value={option.value}>
                            {option.label}
                          </option>
                        ))}
                      </select>
                      {hasSubstitutedJersey && (
                        <span className="jersey-substituted-badge" title={jerseyTooltip}>
                          {t(
                            'floorball.teams.requestedJerseyBadge',
                            'requested #{{requested}}',
                            { requested: player.requestedJerseyNumber }
                          )}
                        </span>
                      )}
                    </td>
                    <td className="position-column">
                      <select
                        className="position-select"
                        value={player.position || FloorballPosition.None}
                        onChange={(e) => {
                          handleUpdatePosition(player, e.target.value as FloorballPosition);
                        }}
                        disabled={updatingPlayer === player.playerId}
                      >
                        {positionOptions.map((option) => (
                          <option key={option.value} value={option.value}>
                            {option.label}
                          </option>
                        ))}
                      </select>
                    </td>
                    <td className="status-column">
                      <span className={`status-badge ${player.isActive ? 'active' : 'inactive'}`}>
                        {player.isActive ? '✓' : '✗'}
                      </span>
                    </td>
                    <td className="actions-column">
                      <div className="player-actions-dropdown">
                        <button
                          className="dropdown-trigger"
                          onClick={(e) => {
                            e.stopPropagation();
                            toggleDropdown(player.playerId);
                          }}
                        >
                          <span className="three-dots">⋮</span>
                        </button>
                        {dropdownOpen === player.playerId && (
                          <div className="dropdown-menu" onClick={(e) => e.stopPropagation()}>
                            <button
                              className="dropdown-item status-item"
                              onClick={() => handleToggleActive(player)}
                            >
                              {player.isActive 
                                ? t('floorball.players.setInactive', 'Set Inactive')
                                : t('floorball.players.setActive', 'Set Active')
                              }
                            </button>
                            <button
                              className="dropdown-item delete-item"
                              onClick={() => handleRemovePlayer(player.playerId)}
                            >
                              {t('floorball.teams.removeFromTeam', 'Remove from Team')}
                            </button>
                          </div>
                        )}
                      </div>
                    </td>
                  </tr>
                  );
                })
              )}
            </tbody>
          </table>
        </div>
      </div>
    </PageTemplate>
  );
};

export default EditRosterPage;
