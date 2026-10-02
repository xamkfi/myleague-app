import { useCallback, useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';
import SearchField from '../../../../components/SearchField';
import Button from '../../../../components/Button/Button';
import AddIcon from '../../../../assets/basicIcons/add.svg';
import BulkActionsBar from '../../../../components/BulkActionsBar/BulkActionsBar';
import Pagination from '../../../../components/Pagination';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';
import { hockeyTeamService } from '../../../../api/hockey/hockeyTeamService';
import { hockeyPlayerService } from '../../../../api/hockey/hockeyPlayerService';
import type { HockeyPosition, HockeyTeamDto } from '../../../../types/hockey/hockeyTypes';
import type { ActivePlayerLicence } from '../../../../types/activePlayerLicence';
import PlayersTable, { type HockeyPlayerListRow } from './components/PlayersTable';
import PlayerLicenceFilter from '../../../../components/admin/PlayerLicenceFilter';
import type { LicenceFilterValue } from '../../../../components/admin/licenceFilterUtils';
import AssignToTeamModal from './components/AssignToTeamModal';
import ConfirmDeleteModal from './components/ConfirmDeleteModal';
import '../../../../styles/AdminTable.scss';
import './HockeyPlayersPage.scss';

function HockeyPlayersPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [rows, setRows] = useState<HockeyPlayerListRow[]>([]);
  const [teamOptions, setTeamOptions] = useState<Array<{ id: string; name: string }>>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [teamFilter, setTeamFilter] = useState('');
  const [licenceFilter, setLicenceFilter] = useState<LicenceFilterValue>('all');
  const [selectedPlayers, setSelectedPlayers] = useState<Set<string>>(new Set());
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(25);
  const [assignPlayer, setAssignPlayer] = useState<HockeyPlayerListRow | null>(null);
  const [isAssigning, setIsAssigning] = useState(false);
  const [deleteTarget, setDeleteTarget] = useState<HockeyPlayerListRow | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);

  const load = useCallback(async (): Promise<void> => {
    try {
      setLoading(true);
      const [teams, profiles] = await Promise.all([
        hockeyTeamService.getAll(),
        hockeyPlayerService.getAllPages(),
      ]);
      const byPlayer = new Map<string, HockeyPlayerListRow>();
      for (const profile of profiles) {
        const firstName = profile.firstName ?? profile.id.slice(0, 8);
        const lastName = profile.lastName ?? '';
        byPlayer.set(profile.id, {
          playerId: profile.id,
          teamId: '',
          teamIds: [],
          firstName,
          lastName,
          name: `${firstName} ${lastName}`.trim(),
          position: profile.primaryPosition,
          isActive: profile.isActive,
          licences: [],
        });
      }
      for (const team of teams) {
        for (const row of team.roster) {
          const existing = byPlayer.get(row.playerId);
          if (!existing) {
            continue;
          }
          const licence = openHockeyLicence(team, row);
          if (existing.teamIds.length === 0) {
            existing.teamId = team.id;
            existing.position = row.position;
            existing.isActive = row.rosterStatus === 'Active';
          }
          if (!existing.teamIds.includes(team.id)) {
            existing.teamIds.push(team.id);
          }
          if (licence && !existing.licences.some((item) => item.teamId === licence.teamId && item.competitionId === licence.competitionId)) {
            existing.licences.push(licence);
          }
          if (row.rosterStatus === 'Active') {
            existing.isActive = true;
          }
        }
      }
      const list = [...byPlayer.values()].sort((left, right) =>
        `${left.lastName} ${left.firstName}`.localeCompare(`${right.lastName} ${right.firstName}`, undefined, {
          sensitivity: 'base',
        }),
      );
      setRows(list);
      setTeamOptions(
        [...teams]
          .map((team) => ({ id: team.id, name: team.name }))
          .sort((left, right) => left.name.localeCompare(right.name, undefined, { sensitivity: 'base' })),
      );
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : t('hockey.players.errors.loadFailed', 'Failed to load players'));
    } finally {
      setLoading(false);
    }
  }, [t]);

  useEffect(() => {
    void load();
  }, [load]);

  const filtered = useMemo(() => {
    const needle = searchTerm.trim().toLowerCase();
    return rows.filter((row) => {
      if (teamFilter && !row.teamIds.includes(teamFilter)) {
        return false;
      }
      if (licenceFilter === 'active' && row.licences.length === 0) {
        return false;
      }
      if (licenceFilter === 'inactive' && row.licences.length > 0) {
        return false;
      }
      if (!needle) {
        return true;
      }
      return `${row.firstName} ${row.lastName}`.toLowerCase().includes(needle);
    });
  }, [rows, searchTerm, teamFilter, licenceFilter]);

  useEffect(() => {
    setCurrentPage(1);
  }, [searchTerm, teamFilter, licenceFilter]);

  const totalPages = Math.max(1, Math.ceil(filtered.length / pageSize));
  const paged = filtered.slice((currentPage - 1) * pageSize, currentPage * pageSize);

  const handleStatusChange = async (player: HockeyPlayerListRow, isActive: boolean): Promise<void> => {
    if (!player.teamId) {
      return;
    }
    const team = (await hockeyTeamService.getById(player.teamId));
    const row = team.roster.find((item) => item.playerId === player.playerId);
    if (!row) {
      return;
    }
    await hockeyTeamService.updatePlayer(player.teamId, player.playerId, {
      position: row.position as HockeyPosition,
      jerseyNumber: row.jerseyNumber,
      rosterStatus: isActive ? 'Active' : 'Inactive',
      captainRole: row.captainRole,
      competitionId: row.competitionId,
    });
    await load();
  };

  const handleAssign = async (teamId: string, position: HockeyPosition, jerseyNumber?: number): Promise<void> => {
    if (!assignPlayer) {
      return;
    }
    setIsAssigning(true);
    try {
      await hockeyTeamService.addPlayer(teamId, assignPlayer.playerId, position, jerseyNumber);
      setAssignPlayer(null);
      await load();
    } finally {
      setIsAssigning(false);
    }
  };

  const handleDelete = async (): Promise<void> => {
    if (!deleteTarget) {
      return;
    }
    setIsDeleting(true);
    try {
      await hockeyTeamService.removePlayer(deleteTarget.teamId, deleteTarget.playerId);
      setDeleteTarget(null);
      await load();
    } finally {
      setIsDeleting(false);
    }
  };

  return (
    <PageTemplate title={t('hockey.players.title', 'Manage Players')}>
      <div className="floorball-players-container">
        <h2 className="floorball-players-title">{t('hockey.players.title', 'MANAGE PLAYERS')}</h2>
        <div className="floorball-players-header">
          <div className="players-actions">
            <SearchField
              value={searchTerm}
              onChange={setSearchTerm}
              placeholder={t('hockey.players.searchByName', 'Search by name...')}
              fullWidth
              rounded="pill"
            />
            <PlayerLicenceFilter
              id="hockey-players-licence-filter"
              value={licenceFilter}
              onChange={setLicenceFilter}
            />
            <Button iconLeft={AddIcon} onClick={() => navigate('/admin/hockey/players/create')}>
              {t('hockey.players.create', 'Create player')}
            </Button>
          </div>
        </div>
        <div className="players-team-filter">
          <label htmlFor="hockey-players-team-filter">
            {t('hockey.players.filterByTeam', 'Team')}
          </label>
          <select
            id="hockey-players-team-filter"
            value={teamFilter}
            onChange={(event) => setTeamFilter(event.target.value)}
          >
            <option value="">{t('hockey.players.allTeams', 'All teams')}</option>
            {teamOptions.map((team) => (
              <option key={team.id} value={team.id}>
                {team.name}
              </option>
            ))}
          </select>
        </div>
        <ErrorPopup message={error} />
        <BulkActionsBar
          selectedCount={selectedPlayers.size}
          totalCount={paged.length}
          onSelectAll={() => setSelectedPlayers(new Set(paged.map((row) => row.playerId)))}
          onClearSelection={() => setSelectedPlayers(new Set())}
          actions={[
            {
              label: t('hockey.players.actions.deactivate', 'Deactivate Player'),
              onClick: () => {
                void Promise.all(
                  paged
                    .filter((row) => selectedPlayers.has(row.playerId))
                    .map((row) => handleStatusChange(row, false)),
                );
              },
              variant: 'danger',
            },
          ]}
        />
        {loading ? (
          <div className="floorball-players-loading">
            <p>{t('common.loading', 'Loading...')}</p>
          </div>
        ) : (
          <PlayersTable
            players={paged}
            onDelete={(playerId, teamId) => {
              const row = rows.find((item) => item.playerId === playerId && item.teamId === teamId) ?? null;
              setDeleteTarget(row);
            }}
            onStatusChange={(player, isActive) => void handleStatusChange(player, isActive)}
            onAssignToTeam={setAssignPlayer}
            selectedPlayers={selectedPlayers}
            onToggleSelection={(playerId) => {
              setSelectedPlayers((prev) => {
                const next = new Set(prev);
                if (next.has(playerId)) {
                  next.delete(playerId);
                } else {
                  next.add(playerId);
                }
                return next;
              });
            }}
            onSelectAll={() => setSelectedPlayers(new Set(paged.map((row) => row.playerId)))}
            onClearSelection={() => setSelectedPlayers(new Set())}
          />
        )}
        <Pagination
          currentPage={currentPage}
          totalPages={totalPages}
          totalCount={filtered.length}
          pageSize={pageSize}
          onPageChange={setCurrentPage}
          onPageSizeChange={(next) => {
            setPageSize(next);
            setCurrentPage(1);
          }}
        />
      </div>
      <AssignToTeamModal
        isOpen={assignPlayer !== null}
        player={assignPlayer}
        onConfirm={handleAssign}
        onCancel={() => setAssignPlayer(null)}
        isAssigning={isAssigning}
      />
      <ConfirmDeleteModal
        isOpen={deleteTarget !== null}
        name={deleteTarget?.name ?? null}
        onConfirm={() => void handleDelete()}
        onCancel={() => setDeleteTarget(null)}
        isDeleting={isDeleting}
      />
    </PageTemplate>
  );
}

export default HockeyPlayersPage;

function openHockeyLicence(
  team: HockeyTeamDto,
  row: HockeyTeamDto['roster'][number],
): ActivePlayerLicence | null {
  if (row.rosterStatus !== 'Active' || row.competitionId === null) {
    return null;
  }
  return {
    teamId: team.id,
    teamName: team.name,
    competitionId: row.competitionId,
    competitionName: null,
  };
}
