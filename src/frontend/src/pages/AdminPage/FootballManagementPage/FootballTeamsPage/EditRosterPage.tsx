import { useState, useEffect, useCallback, useMemo } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';
import { footballTeamService } from '../../../../api/football/footballTeamService';
import { 
  FootballPosition,
  type FootballTeam,
  type FootballTeamPlayer,
  type UpdateFootballTeamPlayerRequest
} from '../../../../types/football/footballTypes';
import SearchField from '../../../../components/SearchField';
import Button from '../../../../components/Button/Button';
import AddIcon from '../../../../assets/basicIcons/add.svg';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';
import JerseyNumberSelect, { collectJerseyNumbers } from '../../../../components/JerseyNumberSelect';
import './EditRosterPage.scss';

const EditRosterPage = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { id: teamId } = useParams<{ id: string }>();
  
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [currentTeam, setCurrentTeam] = useState<FootballTeam | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [dropdownOpen, setDropdownOpen] = useState<string | null>(null);
  const [updatingPlayer, setUpdatingPlayer] = useState<string | null>(null);

  const loadTeamData = useCallback(async () => {
    if (!teamId) return;
    
    try {
      setLoading(true);
      const team = await footballTeamService.getById(teamId);
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
    
    const confirmRemove = window.confirm(t('football.teams.confirmRemovePlayer', 'Are you sure you want to remove this player from the team?'));
    if (!confirmRemove) return;

    try {
      setError(null);
      await footballTeamService.removePlayerFromTeam(teamId, playerId);
      await loadTeamData();
    } catch (err) {
      console.error('Error removing player:', err);
      setError(err instanceof Error ? err.message : 'Failed to remove player');
    }
    setDropdownOpen(null);
  };

  // Handle toggling player active status
  const handleToggleActive = async (player: FootballTeamPlayer) => {
    if (!teamId) return;

    try {
      setError(null);
      const updateData: UpdateFootballTeamPlayerRequest = {
        position: player.position,
        jerseyNumber: player.jerseyNumber,
        isActive: !player.isActive
      };
      await footballTeamService.updateTeamPlayer(teamId, player.playerId, updateData);
      await loadTeamData();
    } catch (err) {
      console.error('Error updating player status:', err);
      setError(err instanceof Error ? err.message : 'Failed to update player status');
    }
    setDropdownOpen(null);
  };

  // Handle updating player position
  const handleUpdatePosition = async (player: FootballTeamPlayer, newPosition: FootballPosition) => {
    if (!teamId || player.position === newPosition) return;

    try {
      setUpdatingPlayer(player.playerId);
      setError(null);
      const updateData: UpdateFootballTeamPlayerRequest = {
        position: newPosition,
        jerseyNumber: player.jerseyNumber,
        isActive: player.isActive
      };
      await footballTeamService.updateTeamPlayer(teamId, player.playerId, updateData);
      await loadTeamData();
    } catch (err) {
      console.error('Error updating player position:', err);
      setError(err instanceof Error ? err.message : 'Failed to update player position');
    } finally {
      setUpdatingPlayer(null);
    }
  };

  // Handle updating player jersey number
  const handleUpdateJerseyNumber = async (player: FootballTeamPlayer, newJerseyNumber: number | undefined) => {
    if (!teamId || player.jerseyNumber === newJerseyNumber) return;

    try {
      setUpdatingPlayer(player.playerId);
      setError(null);
      const updateData: UpdateFootballTeamPlayerRequest = {
        position: player.position,
        jerseyNumber: newJerseyNumber,
        isActive: player.isActive
      };
      await footballTeamService.updateTeamPlayer(teamId, player.playerId, updateData);
      await loadTeamData();
    } catch (err) {
      console.error('Error updating jersey number:', err);
      setError(err instanceof Error ? err.message : 'Failed to update jersey number');
    } finally {
      setUpdatingPlayer(null);
    }
  };

  const takenJerseyNumbers = useMemo(
    () => collectJerseyNumbers(currentTeam?.roster ?? []),
    [currentTeam],
  );

  // Position options for dropdown
  const positionOptions = [
    { value: FootballPosition.None, label: t('football.positions.none', 'None') },
    { value: FootballPosition.Goalkeeper, label: t('football.positions.goalkeeper', 'Goalkeeper') },
    { value: FootballPosition.Defender, label: t('football.positions.defender', 'Defender') },
    { value: FootballPosition.Midfielder, label: t('football.positions.midfielder', 'Midfielder') },
    { value: FootballPosition.Forward, label: t('football.positions.forward', 'Forward') },
  ];

  // Handle adding new player (navigate to add player page)
  const handleAddPlayer = () => {
    navigate(`/admin/football/teams/${teamId}/roster/add`);
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
      <PageTemplate title={t('football.teams.editRoster', 'Edit Roster')}>
        <ErrorPopup message={error || 'Team not found'} />
      </PageTemplate>
    );
  }

  const rosterCount = currentTeam.roster?.length || 0;

  return (
    <PageTemplate title={`${t('football.teams.editRoster', 'Edit Roster')} - ${currentTeam.name}`}>
      <div className="edit-roster-container">
        <h2 className="edit-roster-title">
          {t('football.teams.manageTeamRoster', 'MANAGE TEAM ROSTER')}
        </h2>
        
        <div className="team-info-header">
          <span className="team-name">{currentTeam.name}</span>
          <span className="roster-count">{rosterCount} {t('football.teams.players', 'players')}</span>
        </div>

        <div className="edit-roster-header">
          <div className="roster-actions">
            <SearchField
              value={searchTerm}
              onChange={setSearchTerm}
              placeholder={t('football.teams.searchPlayers', 'Search players...')}
              fullWidth
              rounded="pill"
            />
            <Button
              className="add-player-button"
              iconLeft={AddIcon}
              onClick={handleAddPlayer}
            >
              {t('football.teams.addPlayerToTeam', 'Add New Player to Team')}
            </Button>
          </div>
        </div>

        <ErrorPopup message={error} />

        <div className="roster-table-wrapper">
          <table className="roster-table">
            <thead>
              <tr>
                <th className="name-column">{t('football.players.name', 'NAME')}</th>
                <th className="jersey-column">{t('football.players.jersey', 'JERSEY')}</th>
                <th className="position-column">{t('football.players.position', 'POSITION')}</th>
                <th className="status-column">{t('football.players.status', 'STATUS')}</th>
                <th className="actions-column">{t('common.actions', 'ACTIONS')}</th>
              </tr>
            </thead>
            <tbody>
              {filteredRoster.length === 0 ? (
                <tr>
                  <td colSpan={5} className="no-players">
                    {searchTerm 
                      ? t('football.teams.noPlayersFound', 'No players found matching your search')
                      : t('football.teams.noPlayersInRoster', 'No players in this team roster')
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
                        'football.teams.requestedJerseyTooltip',
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
                      <JerseyNumberSelect
                        className={`jersey-select${hasSubstitutedJersey ? ' jersey-select--substituted' : ''}`}
                        value={player.jerseyNumber}
                        takenNumbers={takenJerseyNumbers}
                        prefixHash
                        disabled={updatingPlayer === player.playerId}
                        title={jerseyTooltip}
                        onChange={(next) => {
                          void handleUpdateJerseyNumber(player, next ?? undefined);
                        }}
                      />
                      {hasSubstitutedJersey && (
                        <span className="jersey-substituted-badge" title={jerseyTooltip}>
                          {t(
                            'football.teams.requestedJerseyBadge',
                            'requested #{{requested}}',
                            { requested: player.requestedJerseyNumber }
                          )}
                        </span>
                      )}
                    </td>
                    <td className="position-column">
                      <select
                        className="position-select"
                        value={player.position || FootballPosition.None}
                        onChange={(e) => {
                          handleUpdatePosition(player, e.target.value as FootballPosition);
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
                                ? t('football.players.setInactive', 'Set Inactive')
                                : t('football.players.setActive', 'Set Active')
                              }
                            </button>
                            <button
                              className="dropdown-item delete-item"
                              onClick={() => handleRemovePlayer(player.playerId)}
                            >
                              {t('football.teams.removeFromTeam', 'Remove from Team')}
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
