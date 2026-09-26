import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import type { HockeyTeamDto } from '../../../../../types/hockey/hockeyTypes';
import type { DivisionType } from '../../../../../types/common/divisionType';
import { divisionService } from '../../../../../api/common/divisionService';
import AdminTeamsTable from '../../../../../components/admin/AdminTeamsTable';
import TeamPlayersRow from './TeamPlayersRow';

interface TeamsTableProps {
  teams: HockeyTeamDto[];
  clubNames: Map<string, string>;
  loading: boolean;
  onEdit: (teamId: string) => void;
  onEditRoster: (teamId: string) => void;
  onEditLines: (teamId: string) => void;
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
  divisionNameByTeamId?: Map<string, string>;
  bulkActionLabel?: string;
}

function TeamsTable({
  teams,
  clubNames,
  loading,
  onEdit,
  onEditRoster,
  onEditLines,
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
  divisionNameByTeamId,
  bulkActionLabel,
}: TeamsTableProps) {
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
      sport="hockey"
      teams={teams.map((team) => ({
        id: team.id,
        name: team.name,
        teamCategory: team.teamCategory,
        clubName: clubNames.get(team.clubId) ?? '—',
        divisionName: viewMode === 'season'
          ? (divisionNameByTeamId?.get(team.id) ?? '—')
          : (divisions.find((division) => division.id === team.divisionId)?.name ?? '—'),
        homeArena: team.homeArena,
        hasActiveMembers: team.hasActiveRoster ?? team.roster.some((row) => row.isActive),
        primaryJerseyColor: team.primaryJerseyColor,
        secondaryJerseyColor: team.secondaryJerseyColor,
        rosterCount: viewMode === 'season' ? (team.competitionRosterCount ?? 0) : undefined,
      }))}
      labels={{
        noTeams: t('hockey.teams.noTeams', 'No teams found'),
        selectAll: t('hockey.teams.selectAll', 'Select all teams'),
        teamName: t('hockey.teams.table.name', 'Team Name'),
        club: t('hockey.teams.table.club', 'Club'),
        division: viewMode === 'season'
          ? t('admin.teams.division')
          : t('admin.teams.homeDivision'),
        homeArena: t('hockey.teams.table.homeArena', 'Home Arena'),
        activeMembers: viewMode === 'season'
          ? t('admin.teams.roster')
          : t('hockey.teams.table.activeMembers', 'Active Members'),
        actions: t('hockey.teams.table.actions', 'Actions'),
        primary: t('hockey.teams.primary', 'Primary'),
        secondary: t('hockey.teams.secondary', 'Secondary'),
        hasMembers: t('hockey.teams.hasMembers', 'Yes'),
        noMembers: t('hockey.teams.noMembers', 'No'),
        editTeamInfo: t('admin.teams.editTeam'),
        editRoster: t('admin.teams.editRoster'),
        delete: t('common.deactivate', 'Deactivate'),
        removeFromSeason: t('admin.teams.removeFromSeason'),
        noRoster: t('admin.teams.noRoster'),
        actionsMenu: t('hockey.teams.actions.menu', 'Team actions menu'),
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
      extraActions={(team) => ([
        {
          label: t('hockey.teams.lines', 'Lines'),
          onClick: () => onEditLines(team.id),
        },
      ])}
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
}

export default TeamsTable;
