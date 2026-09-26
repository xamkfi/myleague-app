import { useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import AdminTeamsBrowser from '../../../../components/admin/teams/AdminTeamsBrowser';
import type {
  AdminSeasonChoice,
  AdminTeamListPage,
  AdminTeamListQuery,
  NamedOption,
} from '../../../../components/admin/teams/adminTeamListTypes';
import { hockeyTeamService } from '../../../../api/hockey/hockeyTeamService';
import { hockeySeasonService } from '../../../../api/hockey/hockeySeasonService';
import { clubService } from '../../../../api/common/clubService';
import { divisionService } from '../../../../api/common/divisionService';
import { SportsCategory } from '../../../../types/common/sports';
import type { HockeyTeamCategory, HockeyTeamDto } from '../../../../types/hockey/hockeyTypes';
import TeamsTable from './components/TeamsTable';
import './HockeyTeamsPage.scss';

function HockeyTeamsPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();

  const loadSeasons = useCallback(async (): Promise<AdminSeasonChoice[]> => {
    const seasons = await hockeySeasonService.getAll(undefined, true);
    return seasons.map((season) => {
      const teamIdByCompetitionTeam = new Map((season.teams ?? []).map((team) => [team.id, team.teamId]));
      const divisionNameByTeamId = new Map<string, string>();
      const divisions = (season.divisions ?? [])
        .filter((division) => division.isActive)
        .map((division) => {
          division.teams.forEach((membership) => {
            if (!membership.isActive) {
              return;
            }
            const teamId = teamIdByCompetitionTeam.get(membership.competitionTeamId);
            if (teamId) {
              divisionNameByTeamId.set(teamId, division.name);
            }
          });
          return { id: division.divisionId, name: division.name };
        });
      return {
        id: season.id,
        name: season.name,
        isActive: season.isActive,
        startDate: season.startDate,
        divisions,
        divisionNameByTeamId,
      };
    });
  }, []);

  const loadClubs = useCallback(async (): Promise<NamedOption[]> => {
    const clubs = await clubService.getAll();
    return clubs.map((club) => ({ id: club.id, name: club.name }));
  }, []);

  const loadHomeDivisions = useCallback(async (): Promise<NamedOption[]> => {
    const divisions = await divisionService.getAll();
    return divisions.data
      .filter((division) => division.sportType === SportsCategory.Icehockey)
      .map((division) => ({ id: division.id, name: division.name }));
  }, []);

  const loadTeams = useCallback(async (query: AdminTeamListQuery): Promise<AdminTeamListPage<HockeyTeamDto>> => {
    const response = await hockeyTeamService.getPaged({
      page: query.page,
      pageSize: query.pageSize,
      searchTerm: query.searchTerm,
      clubId: query.clubId,
      competitionId: query.competitionId,
      competitionDivisionId: query.competitionDivisionId,
      divisionId: query.homeDivisionId,
      teamCategories: query.categories.length > 0 ? query.categories as HockeyTeamCategory[] : undefined,
    });
    return {
      items: response.data ?? [],
      totalCount: response.pagination?.totalCount ?? 0,
      totalPages: response.pagination?.totalPages ?? 1,
    };
  }, []);

  const onRemoveFromSeason = useCallback(async (seasonId: string, teamId: string): Promise<void> => {
    await hockeySeasonService.removeTeam(seasonId, teamId);
  }, []);

  const onDeleteTeam = useCallback(async (teamId: string): Promise<void> => {
    await hockeyTeamService.setActive(teamId, false);
  }, []);

  return (
    <AdminTeamsBrowser
      title={t('hockey.teams.title', 'Manage Teams')}
      sportPath="/admin/hockey"
      loadSeasons={loadSeasons}
      loadClubs={loadClubs}
      loadHomeDivisions={loadHomeDivisions}
      loadTeams={loadTeams}
      onRemoveFromSeason={onRemoveFromSeason}
      onDeleteTeam={onDeleteTeam}
      confirmDelete={(name) => t('hockey.teams.confirmDeactivate', 'Deactivate team "{{name}}"?', { name })}
      confirmBulkDelete={(count) => t('hockey.teams.confirmBulkDeactivate', 'Deactivate {{count}} teams?', { count })}
      deleteFailed={t('hockey.teams.errors.deactivateFailed', 'Failed to deactivate team')}
      bulkDeleteFailed={t('hockey.teams.errors.bulkDeactivateFailed', 'Failed to deactivate teams')}
      renderTable={(slot) => (
        <TeamsTable
          teams={slot.teams}
          clubNames={slot.clubNameById}
          loading={slot.loading}
          viewMode={slot.viewMode}
          divisionNameByTeamId={slot.divisionNameByTeamId}
          bulkActionLabel={slot.bulkActionLabel}
          selectedIds={slot.selectedIds}
          onToggleSelect={slot.onToggleSelect}
          onSelectAll={slot.onSelectAll}
          onClearSelection={slot.onClearSelection}
          onBulkDelete={slot.onBulkDelete}
          onEdit={slot.onEdit}
          onEditRoster={slot.onEditRoster}
          onEditLines={(teamId) => navigate(`/admin/hockey/teams/${teamId}/lines`)}
          onDelete={slot.onDelete}
          pagination={slot.pagination}
          onPageChange={slot.onPageChange}
          onPageSizeChange={slot.onPageSizeChange}
        />
      )}
    />
  );
}

export default HockeyTeamsPage;
