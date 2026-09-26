import { useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import AdminTeamsBrowser from '../../../../components/admin/teams/AdminTeamsBrowser';
import type {
  AdminSeasonChoice,
  AdminTeamListPage,
  AdminTeamListQuery,
  NamedOption,
} from '../../../../components/admin/teams/adminTeamListTypes';
import { floorballTeamService } from '../../../../api/floorball/floorballTeamService';
import { floorballSeasonService } from '../../../../api/floorball/floorballSeasonService';
import { clubService } from '../../../../api/common/clubService';
import { divisionService } from '../../../../api/common/divisionService';
import { SportsCategory } from '../../../../types/common/sports';
import type { FloorballTeam, TeamCategory } from '../../../../types/floorball/floorballTypes';
import TeamsTable from './components/TeamsTable';
import { mapDeletionError } from '../../../../utils/mapDeletionError';
import './FloorballTeamsPage.scss';

function FloorballTeamsPage() {
  const { t } = useTranslation();

  const loadSeasons = useCallback(async (): Promise<AdminSeasonChoice[]> => {
    const [seasons, divisions] = await Promise.all([
      floorballSeasonService.getAll(true),
      divisionService.getAll(),
    ]);
    const divisionNames = new Map(
      divisions.data
        .filter((division) => division.sportType === SportsCategory.Floorball)
        .map((division) => [division.id, division.name]),
    );
    return (seasons.data ?? []).map((season) => {
      const divisionNameByTeamId = new Map<string, string>();
      const seasonDivisions = (season.seasonDivisions ?? []).map((entry) => {
        const name = divisionNames.get(entry.divisionId) ?? entry.divisionId;
        entry.teamIds.forEach((teamId) => divisionNameByTeamId.set(teamId, name));
        return { id: entry.divisionId, name };
      });
      return {
        id: season.id,
        name: season.name,
        isActive: season.isActive,
        startDate: season.startDate,
        divisions: seasonDivisions,
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
      .filter((division) => division.sportType === SportsCategory.Floorball)
      .map((division) => ({ id: division.id, name: division.name }));
  }, []);

  const loadTeams = useCallback(async (query: AdminTeamListQuery): Promise<AdminTeamListPage<FloorballTeam>> => {
    const response = await floorballTeamService.getAll({
      page: query.page,
      pageSize: query.pageSize,
      searchTerm: query.searchTerm,
      clubId: query.clubId,
      division: query.homeDivisionId,
      competitionId: query.competitionId,
      competitionDivisionId: query.competitionDivisionId,
      teamCategories: query.categories.length > 0 ? query.categories as TeamCategory[] : undefined,
    });
    return {
      items: response.data ?? [],
      totalCount: response.pagination?.totalCount ?? 0,
      totalPages: response.pagination?.totalPages ?? 1,
    };
  }, []);

  const onRemoveFromSeason = useCallback(async (seasonId: string, teamId: string): Promise<void> => {
    await floorballSeasonService.removeTeamFromSeason(seasonId, teamId);
  }, []);

  const onDeleteTeam = useCallback(async (teamId: string): Promise<void> => {
    try {
      await floorballTeamService.delete(teamId);
    } catch (err) {
      throw new Error(mapDeletionError(err, t) ?? t('floorball.teams.errors.deleteFailed', 'Failed to delete team'));
    }
  }, [t]);

  return (
    <AdminTeamsBrowser
      title={t('floorball.teams.title', 'Manage Teams')}
      sportPath="/admin/floorball"
      loadSeasons={loadSeasons}
      loadClubs={loadClubs}
      loadHomeDivisions={loadHomeDivisions}
      loadTeams={loadTeams}
      onRemoveFromSeason={onRemoveFromSeason}
      onDeleteTeam={onDeleteTeam}
      confirmDelete={(name) => t('floorball.teams.confirmDelete', { name })}
      confirmBulkDelete={(count) => t('floorball.teams.confirmBulkDelete', {
        count,
        defaultValue: 'Are you sure you want to delete {{count}} teams?',
      })}
      deleteFailed={t('floorball.teams.errors.deleteFailed', 'Failed to delete team')}
      bulkDeleteFailed={t('floorball.teams.errors.bulkDeleteFailed', 'Failed to delete teams')}
      renderTable={(slot) => (
        <TeamsTable
          teams={slot.teams}
          loading={slot.loading}
          viewMode={slot.viewMode}
          seasonId={slot.seasonId}
          divisionNameByTeamId={slot.divisionNameByTeamId}
          bulkActionLabel={slot.bulkActionLabel}
          selectedIds={slot.selectedIds}
          onToggleSelect={slot.onToggleSelect}
          onSelectAll={slot.onSelectAll}
          onClearSelection={slot.onClearSelection}
          onBulkDelete={slot.onBulkDelete}
          onEdit={slot.onEdit}
          onEditRoster={slot.onEditRoster}
          onDelete={slot.onDelete}
          pagination={slot.pagination}
          onPageChange={slot.onPageChange}
          onPageSizeChange={slot.onPageSizeChange}
        />
      )}
    />
  );
}

export default FloorballTeamsPage;
