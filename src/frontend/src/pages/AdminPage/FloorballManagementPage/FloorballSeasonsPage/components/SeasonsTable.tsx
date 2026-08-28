import { useTranslation } from 'react-i18next';
import { useDivisions } from '../../../../../hooks/useDivisions';
import { useInProgressMatches } from '../../../../../hooks/useInProgressMatches';
import type { FloorballSeasonDto } from '../../../../../api/floorball/floorballSeasonService';
import AdminSeasonsTable from '../../../../../components/admin/AdminSeasonsTable';

interface SeasonsTableProps {
  seasons: FloorballSeasonDto[];
  onEdit: (season: FloorballSeasonDto) => void;
  onDelete: (season: FloorballSeasonDto) => void;
  onActivateToggle: (season: FloorballSeasonDto) => void;
  onComplete: (season: FloorballSeasonDto) => void;
  operationLoading?: string | null;
}

export const SeasonsTable = ({
  seasons,
  onEdit,
  onDelete,
  onActivateToggle,
  onComplete,
  operationLoading,
}: SeasonsTableProps) => {
  const { t } = useTranslation();
  const { divisions } = useDivisions();
  const { countByCompetitionId } = useInProgressMatches();

  const byId = new Map(seasons.map((season) => [season.id, season]));

  return (
    <AdminSeasonsTable
      sport="floorball"
      seasons={seasons.map((season) => ({
        id: season.id,
        name: season.name,
        teamCategory: season.teamCategory,
        startDate: season.startDate,
        endDate: season.endDate,
        teamCount: season.teams?.length || 0,
        isActive: season.isActive,
        isCompleted: season.isCompleted,
        divisions: (season.seasonDivisions ?? []).map((seasonDivision) => ({
          id: seasonDivision.divisionId,
          name: divisions.find((division) => division.id === seasonDivision.divisionId)?.name
            || seasonDivision.divisionId,
        })),
      }))}
      labels={{
        name: t('floorball.seasons.fields.name', 'Name'),
        division: t('floorball.seasons.fields.division', 'Division'),
        startDate: t('floorball.seasons.fields.startDate', 'Starts'),
        endDate: t('floorball.seasons.fields.endDate', 'Ends'),
        teams: t('floorball.seasons.fields.teams', 'Teams'),
        status: t('floorball.seasons.fields.status', 'Status'),
        completed: t('floorball.seasons.status.completed', 'Completed'),
        active: t('floorball.seasons.status.active', 'Active'),
        inactive: t('floorball.seasons.status.inactive', 'Inactive'),
        deactivate: t('floorball.seasons.deactivate', 'Deactivate'),
        activate: t('floorball.seasons.activate', 'Activate'),
        complete: t('floorball.seasons.complete', 'Complete Season'),
        noDivisions: t('floorball.seasons.noDivisions', 'No divisions'),
        teamsCount: t('floorball.seasons.teamsCount', 'teams'),
        matchesInProgress: (count) => t(
          'floorball.seasons.matchesInProgress',
          '{{count}} match(es) in progress',
          { count },
        ),
        openEdit: t('floorball.seasons.actions.openEdit', 'Open and edit season'),
        actionsMenu: t('floorball.seasons.actions.menu', 'Season actions menu'),
      }}
      liveCounts={countByCompetitionId}
      onEdit={(seasonId) => {
        const season = byId.get(seasonId);
        if (season) onEdit(season);
      }}
      onDelete={(seasonId) => {
        const season = byId.get(seasonId);
        if (season) onDelete(season);
      }}
      onActivateToggle={(seasonId) => {
        const season = byId.get(seasonId);
        if (season) onActivateToggle(season);
      }}
      onComplete={(seasonId) => {
        const season = byId.get(seasonId);
        if (season) onComplete(season);
      }}
      operationLoading={operationLoading}
    />
  );
};
