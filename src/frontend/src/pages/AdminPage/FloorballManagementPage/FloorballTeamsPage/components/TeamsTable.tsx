import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import type { FloorballTeam } from '../../../../../types/floorball/floorballTypes';
import type { DivisionType } from '../../../../../types/common/divisionType';
import { divisionService } from '../../../../../api/common/divisionService';
import AdminTeamsTable from '../../../../../components/admin/AdminTeamsTable';
import TeamPlayersRow from './TeamPlayersRow';

interface TeamsTableProps {
  teams: FloorballTeam[];
  loading: boolean;
  onEdit: (teamId: string) => void;
  onEditRoster: (teamId: string) => void;
  onDelete: (teamId: string, teamName: string) => void;
  selectedIds: Set<string>;
  onToggleSelect: (id: string) => void;
  onSelectAll: () => void;
  onClearSelection: () => void;
  onBulkDelete: () => void;
  pagination?: {
    currentPage: number;
    totalPages: number;
    totalCount: number;
    pageSize: number;
  };
  onPageChange?: (page: number) => void;
  onPageSizeChange?: (pageSize: number) => void;
  viewMode?: 'season' | 'catalog';
  seasonId?: string | null;
  divisionNameByTeamId?: Map<string, string>;
  bulkActionLabel?: string;
}

const TeamsTable = ({
  teams,
  loading,
  onEdit,
  onEditRoster,
  onDelete,
  selectedIds,
  onToggleSelect,
  onSelectAll,
  onClearSelection,
  onBulkDelete,
  pagination,
  onPageChange,
  onPageSizeChange,
  viewMode = 'catalog',
  seasonId = null,
  divisionNameByTeamId,
  bulkActionLabel,
}: TeamsTableProps) => {
  const { t } = useTranslation();
  const [divisions, setDivisions] = useState<DivisionType[]>([]);
  const teamById = new Map(teams.map((team) => [team.id, team]));

  useEffect(() => {
    const fetchDivisions = async (): Promise<void> => {
      const response = await divisionService.getAll();
      setDivisions(response.data);
    };
    void fetchDivisions();
  }, []);

  return (
    <AdminTeamsTable
      sport="floorball"
      teams={teams.map((team) => ({
        id: team.id,
        name: team.name,
        teamCategory: team.teamCategory,
        clubName: team.club.name,
        divisionName: viewMode === 'season'
          ? (divisionNameByTeamId?.get(team.id) ?? '—')
          : (divisions.find((division) => division.id === team.divisionId)?.name ?? ''),
        homeArena: team.homeArena,
        hasActiveMembers: team.hasActiveMembers,
        primaryJerseyColor: team.primaryJerseyColor,
        secondaryJerseyColor: team.secondaryJerseyColor,
        rosterCount: viewMode === 'season'
          ? team.roster.filter((player) => player.competitionId === seasonId).length
          : undefined,
      }))}
      labels={{
        noTeams: t('floorball.teams.noTeams', 'No teams found'),
        selectAll: t('floorball.teams.selectAll', 'Select all teams'),
        teamName: t('floorball.teams.table.name', 'Team Name'),
        club: t('floorball.teams.table.club', 'Club'),
        division: viewMode === 'season'
          ? t('admin.teams.division')
          : t('admin.teams.homeDivision'),
        homeArena: t('floorball.teams.table.homeArena', 'Home Arena'),
        activeMembers: viewMode === 'season'
          ? t('admin.teams.roster')
          : t('floorball.teams.table.activeMembers', 'Active Members'),
        actions: t('floorball.teams.table.actions', 'Actions'),
        primary: t('floorball.teams.primary', 'Primary'),
        secondary: t('floorball.teams.secondary', 'Secondary'),
        hasMembers: t('floorball.teams.hasMembers', 'Yes'),
        noMembers: t('floorball.teams.noMembers', 'No'),
        editTeamInfo: t('admin.teams.editTeam'),
        editRoster: t('admin.teams.editRoster'),
        delete: t('common.delete'),
        removeFromSeason: t('admin.teams.removeFromSeason'),
        noRoster: t('admin.teams.noRoster'),
        actionsMenu: t('floorball.teams.actions.menu', 'Team actions menu'),
      }}
      loading={loading}
      selectedIds={selectedIds}
      onToggleSelect={onToggleSelect}
      onSelectAll={onSelectAll}
      onClearSelection={onClearSelection}
      onBulkDelete={onBulkDelete}
      onEdit={onEdit}
      onEditRoster={onEditRoster}
      onDelete={onDelete}
      pagination={pagination}
      onPageChange={onPageChange}
      onPageSizeChange={onPageSizeChange}
      viewMode={viewMode}
      bulkActionLabel={bulkActionLabel}
      renderExpandedRow={(row, isExpanded, isClosing) => (
        <TeamPlayersRow
          teamId={row.id}
          isExpanded={isExpanded}
          isClosing={isClosing}
          team={teamById.get(row.id)}
        />
      )}
    />
  );
};

export default TeamsTable;
