import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import type { FootballTeam } from '../../../../../types/football/footballTypes';
import type { DivisionType } from '../../../../../types/common/divisionType';
import { divisionService } from '../../../../../api/common/divisionService';
import { SportsCategory } from '../../../../../types/common/sports';
import AdminTeamsTable from '../../../../../components/admin/AdminTeamsTable';
import TeamPlayersRow from './TeamPlayersRow';

interface TeamsTableProps {
  teams: FootballTeam[];
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
}: TeamsTableProps) => {
  const { t } = useTranslation();
  const [divisions, setDivisions] = useState<DivisionType[]>([]);
  const teamById = new Map(teams.map((team) => [team.id, team]));

  useEffect(() => {
    const fetchDivisions = async (): Promise<void> => {
      const response = await divisionService.getAll();
      setDivisions(response.data.filter((division) => division.sportType === SportsCategory.Football));
    };
    void fetchDivisions();
  }, []);

  return (
    <AdminTeamsTable
      sport="football"
      teams={teams.map((team) => ({
        id: team.id,
        name: team.name,
        teamCategory: team.teamCategory,
        clubName: team.club.name,
        divisionName: divisions.find((division) => division.id === team.divisionId)?.name ?? '',
        homeArena: team.homeArena,
        hasActiveMembers: team.hasActiveMembers,
        primaryJerseyColor: team.primaryJerseyColor,
        secondaryJerseyColor: team.secondaryJerseyColor,
      }))}
      labels={{
        noTeams: t('football.teams.noTeams', 'No teams found'),
        selectAll: t('football.teams.selectAll', 'Select all teams'),
        teamName: t('football.teams.table.name', 'Team Name'),
        club: t('football.teams.table.club', 'Club'),
        division: t('football.teams.table.division', 'Division'),
        homeArena: t('football.teams.table.homeArena', 'Home Arena'),
        activeMembers: t('football.teams.table.activeMembers', 'Active Members'),
        actions: t('football.teams.table.actions', 'Actions'),
        primary: t('football.teams.primary', 'Primary'),
        secondary: t('football.teams.secondary', 'Secondary'),
        hasMembers: t('football.teams.hasMembers', 'Yes'),
        noMembers: t('football.teams.noMembers', 'No'),
        editTeamInfo: t('football.teams.editTeamInfo', 'Edit Team Information'),
        editRoster: t('football.teams.editRoster', 'Edit Roster'),
        delete: t('common.delete'),
        actionsMenu: t('football.teams.actions.menu', 'Team actions menu'),
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
