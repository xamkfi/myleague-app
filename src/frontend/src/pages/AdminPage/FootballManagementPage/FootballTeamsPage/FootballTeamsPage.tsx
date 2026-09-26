import { useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import AdminTeamsBrowser from '../../../../components/admin/teams/AdminTeamsBrowser';
import type {
  AdminSeasonChoice,
  AdminTeamListPage,
  AdminTeamListQuery,
  NamedOption,
} from '../../../../components/admin/teams/adminTeamListTypes';
import { footballTeamService } from '../../../../api/football/footballTeamService';
import { footballSeasonService } from '../../../../api/football/footballSeasonService';
import { clubService } from '../../../../api/common/clubService';
import { divisionService } from '../../../../api/common/divisionService';
import { SportsCategory } from '../../../../types/common/sports';
import type { FootballTeam, TeamCategory } from '../../../../types/football/footballTypes';
import TeamsTable from './components/TeamsTable';
import { mapDeletionError } from '../../../../utils/mapDeletionError';
import './FootballTeamsPage.scss';

function FootballTeamsPage() {
  const { t } = useTranslation();

  const loadSeasons = useCallback(async (): Promise<AdminSeasonChoice[]> => {
    const [seasons, divisions] = await Promise.all([
      footballSeasonService.getAll(true),
      divisionService.getAll(),
    ]);
    const divisionNames = new Map(
      divisions.data
        .filter((division) => division.sportType === SportsCategory.Football)
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
      .filter((division) => division.sportType === SportsCategory.Football)
      .map((division) => ({ id: division.id, name: division.name }));
  }, []);

  const loadTeams = useCallback(async (query: AdminTeamListQuery): Promise<AdminTeamListPage<FootballTeam>> => {
    const response = await footballTeamService.getAll({
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
    await footballSeasonService.removeTeamFromSeason(seasonId, teamId);
  }, []);

  const onDeleteTeam = useCallback(async (teamId: string): Promise<void> => {
    try {
      await footballTeamService.delete(teamId);
    } catch (err) {
      throw new Error(mapDeletionError(err, t) ?? t('football.teams.errors.deleteFailed', 'Failed to delete team'));
    }
  }, [t]);

  return (
    <AdminTeamsBrowser
      title={t('football.teams.title', 'Manage Teams')}
      sportPath="/admin/football"
      loadSeasons={loadSeasons}
      loadClubs={loadClubs}
      loadHomeDivisions={loadHomeDivisions}
      loadTeams={loadTeams}
      onRemoveFromSeason={onRemoveFromSeason}
      onDeleteTeam={onDeleteTeam}
      confirmDelete={(name) => t('football.teams.confirmDelete', { name, defaultValue: 'Delete team {{name}}?' })}
      confirmBulkDelete={(count) => t('football.teams.confirmBulkDelete', {
        count,
        defaultValue: 'Are you sure you want to delete {{count}} teams?',
      })}
      deleteFailed={t('football.teams.errors.deleteFailed', 'Failed to delete team')}
      bulkDeleteFailed={t('football.teams.errors.bulkDeleteFailed', 'Failed to delete teams')}
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

export default FootballTeamsPage;
