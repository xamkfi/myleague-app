import { useCallback, useEffect, useMemo, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';
import { hockeyPlayerService } from '../../../../api/hockey/hockeyPlayerService';
import { hockeyTeamService } from '../../../../api/hockey/hockeyTeamService';
import type { HockeyPosition, HockeyTeamDto } from '../../../../types/hockey/hockeyTypes';
import SearchField from '../../../../components/SearchField';
import Button from '../../../../components/Button/Button';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';
import { loadPersonNameMap } from '../../../../utils/hockeyLookups';
import './AddPlayerToRosterPage.scss';

interface AvailableHockeyPlayerRow {
  playerId: string;
  name: string;
  position: HockeyPosition;
  currentTeamName: string;
}

const PAGE_SIZE = 10;

function AddHockeyPlayerToRosterPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { id: teamId } = useParams<{ id: string }>();

  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [currentTeam, setCurrentTeam] = useState<HockeyTeamDto | null>(null);
  const [availablePlayers, setAvailablePlayers] = useState<AvailableHockeyPlayerRow[]>([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedPlayers, setSelectedPlayers] = useState<Set<string>>(new Set());
  const [currentPage, setCurrentPage] = useState(1);

  const loadData = useCallback(async (): Promise<void> => {
    if (!teamId) {
      return;
    }
    try {
      setLoading(true);
      const [team, allTeams] = await Promise.all([
        hockeyTeamService.getById(teamId),
        hockeyTeamService.getAll(),
      ]);
      setCurrentTeam(team);

      const currentPlayerIds = new Set(team.roster.map((row) => row.playerId));
      const unique = new Map<string, { teamName: string; position: HockeyPosition }>();
      for (const otherTeam of allTeams) {
        for (const row of otherTeam.roster) {
          if (currentPlayerIds.has(row.playerId) || unique.has(row.playerId)) {
            continue;
          }
          unique.set(row.playerId, { teamName: otherTeam.name, position: row.position });
        }
      }

      const playerIds = [...unique.keys()];
      const profiles = await Promise.all(
        playerIds.map(async (playerId) => {
          try {
            return await hockeyPlayerService.getById(playerId);
          } catch {
            return null;
          }
        }),
      );
      const valid = profiles.filter((player) => player !== null);
      const people = await loadPersonNameMap(valid.map((player) => player.personId));
      const rows: AvailableHockeyPlayerRow[] = [];
      for (const playerId of playerIds) {
        const meta = unique.get(playerId);
        const profile = valid.find((player) => player.id === playerId);
        if (!meta) {
          continue;
        }
        rows.push({
          playerId,
          name: profile ? people.get(profile.personId) ?? playerId.slice(0, 8) : playerId.slice(0, 8),
          position: profile?.primaryPosition ?? meta.position,
          currentTeamName: meta.teamName,
        });
      }
      rows.sort((a, b) => a.name.localeCompare(b.name, undefined, { sensitivity: 'base' }));
      setAvailablePlayers(rows);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : t('hockey.teams.errors.loadFailed', 'Failed to load team data'));
    } finally {
      setLoading(false);
    }
  }, [teamId, t]);

  useEffect(() => {
    void loadData();
  }, [loadData]);

  useEffect(() => {
    setCurrentPage(1);
  }, [searchTerm]);

  const filteredPlayers = useMemo(() => {
    const query = searchTerm.trim().toLowerCase();
    if (query.length < 2) {
      return availablePlayers;
    }
    return availablePlayers.filter((player) =>
      `${player.name} ${player.currentTeamName} ${player.position}`.toLowerCase().includes(query),
    );
  }, [availablePlayers, searchTerm]);

  const totalPages = Math.max(1, Math.ceil(filteredPlayers.length / PAGE_SIZE));
  const page = Math.min(currentPage, totalPages);
  const displayedPlayers = filteredPlayers.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE);

  const togglePlayerSelection = (playerId: string): void => {
    setSelectedPlayers((prev) => {
      const next = new Set(prev);
      if (next.has(playerId)) {
        next.delete(playerId);
      } else {
        next.add(playerId);
      }
      return next;
    });
  };

  const selectAllOnPage = (): void => {
    setSelectedPlayers((prev) => {
      const next = new Set(prev);
      displayedPlayers.forEach((player) => next.add(player.playerId));
      return next;
    });
  };

  const clearSelection = (): void => {
    setSelectedPlayers(new Set());
  };

  const isAllOnPageSelected =
    displayedPlayers.length > 0 && displayedPlayers.every((player) => selectedPlayers.has(player.playerId));

  const handleAddSelectedPlayers = async (): Promise<void> => {
    if (!teamId || selectedPlayers.size === 0) {
      return;
    }
    try {
      setSaving(true);
      setError(null);
      for (const playerId of selectedPlayers) {
        const row = availablePlayers.find((player) => player.playerId === playerId);
        try {
          await hockeyTeamService.addPlayer(teamId, playerId, row?.position ?? 'Center');
        } catch (err) {
          console.error(`Failed to add player ${playerId}:`, err);
        }
      }
      navigate(`/admin/hockey/teams/${teamId}/roster`);
    } catch (err) {
      setError(err instanceof Error ? err.message : t('hockey.teams.errors.addPlayersFailed', 'Failed to add players to team'));
      setSaving(false);
    }
  };

  if (loading) {
    return (
      <PageTemplate title={t('common.loading', 'Loading...')}>
        <div className="add-player-roster-loading">
          <p>{t('common.loading', 'Loading...')}</p>
        </div>
      </PageTemplate>
    );
  }

  if (!teamId || !currentTeam) {
    return (
      <PageTemplate title={t('hockey.teams.addPlayer', 'Add Player')}>
        <ErrorPopup message={error || t('hockey.teams.errors.teamNotFound', 'Team not found')} />
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title={`${t('hockey.teams.addPlayerToTeam', 'Add Player to Team')} - ${currentTeam.name}`}>
      <div className="add-player-roster-container">
        <h2 className="add-player-roster-title">
          {t('hockey.teams.addPlayerToTeam', 'ADD PLAYER TO TEAM')}
        </h2>

        <div className="team-info-header">
          <span className="team-name">{currentTeam.name}</span>
        </div>

        <ErrorPopup message={error} />

        <div className="add-player-roster-header">
          <div className="search-section">
            <SearchField
              value={searchTerm}
              onChange={setSearchTerm}
              placeholder={t('hockey.teams.searchAvailablePlayers', 'Search available players...')}
              fullWidth
              rounded="pill"
            />
          </div>
        </div>

        <div className="selection-controls">
          <div className="selection-info">
            <span className="selected-count">
              {t('hockey.teams.selectedPlayers', '{{count}} selected', { count: selectedPlayers.size })}
            </span>
            {selectedPlayers.size > 0 && (
              <button type="button" className="clear-selection-btn" onClick={clearSelection}>
                {t('common.clearSelection', 'Clear Selection')}
              </button>
            )}
          </div>
          <div className="selection-actions">
            <Button
              variant="primary"
              onClick={() => void handleAddSelectedPlayers()}
              disabled={selectedPlayers.size === 0 || saving}
            >
              {saving
                ? t('common.saving', 'Saving...')
                : t('hockey.teams.addSelectedToTeam', 'Add Selected to Team ({{count}})', { count: selectedPlayers.size })}
            </Button>
          </div>
        </div>

        <div className="players-table-wrapper">
          <table className="players-table">
            <thead>
              <tr>
                <th className="select-column">
                  <input
                    type="checkbox"
                    checked={isAllOnPageSelected}
                    onChange={(event) => {
                      if (event.target.checked) {
                        selectAllOnPage();
                      } else {
                        setSelectedPlayers((prev) => {
                          const next = new Set(prev);
                          displayedPlayers.forEach((player) => next.delete(player.playerId));
                          return next;
                        });
                      }
                    }}
                    title={t('hockey.teams.selectAllOnPage', 'Select all on this page')}
                  />
                </th>
                <th className="name-column">{t('hockey.players.name', 'NAME')}</th>
                <th className="position-column">{t('hockey.players.position', 'POSITION')}</th>
                <th className="team-column">{t('hockey.players.team', 'CURRENT TEAM')}</th>
              </tr>
            </thead>
            <tbody>
              {displayedPlayers.length === 0 ? (
                <tr>
                  <td colSpan={4} className="no-players">
                    {searchTerm
                      ? t('hockey.teams.noPlayersFoundSearch', 'No players found matching your search')
                      : t('hockey.teams.noAvailablePlayers', 'No available players found')}
                  </td>
                </tr>
              ) : (
                displayedPlayers.map((player) => (
                  <tr
                    key={player.playerId}
                    className={`clickable-row${selectedPlayers.has(player.playerId) ? ' selected' : ''}`}
                    onClick={() => togglePlayerSelection(player.playerId)}
                  >
                    <td className="select-column">
                      <input
                        type="checkbox"
                        checked={selectedPlayers.has(player.playerId)}
                        onChange={() => togglePlayerSelection(player.playerId)}
                        onClick={(event) => event.stopPropagation()}
                      />
                    </td>
                    <td className="name-column">
                      <span className="player-name">{player.name}</span>
                    </td>
                    <td className="position-column">
                      <span className="position">{t(`hockey.positions.${player.position}`, player.position)}</span>
                    </td>
                    <td className="team-column">
                      <span className="team-name">{player.currentTeamName}</span>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        {totalPages > 1 && (
          <div className="pagination">
            <Button variant="secondary" size="sm" onClick={() => setCurrentPage(page - 1)} disabled={page === 1}>
              {t('common.previous', 'Previous')}
            </Button>
            <span className="page-info">
              {t('common.pageOf', 'Page {{current}} of {{total}}', { current: page, total: totalPages })}
            </span>
            <Button variant="secondary" size="sm" onClick={() => setCurrentPage(page + 1)} disabled={page === totalPages}>
              {t('common.next', 'Next')}
            </Button>
          </div>
        )}

        <div className="page-footer">
          <Button variant="secondary" onClick={() => navigate(`/admin/hockey/players/create`)}>
            {t('hockey.players.create', 'Create new player')}
          </Button>
        </div>
      </div>
    </PageTemplate>
  );
}

export default AddHockeyPlayerToRosterPage;
