import { useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import { useDivisions } from '../../../../../hooks/useDivisions';
import { useInProgressFootballMatches } from '../../../../../hooks/useInProgressFootballMatches';
import type { FootballSeasonDto } from '../../../../../api/football/footballSeasonService';
import AdminSeasonsTable from '../../../../../components/admin/AdminSeasonsTable';
import { SportsCategory } from '../../../../../types/common/sports';

interface SeasonsTableProps {
  seasons: FootballSeasonDto[];
  onEdit: (season: FootballSeasonDto) => void;
  onDelete: (season: FootballSeasonDto) => void;
  onActivateToggle: (season: FootballSeasonDto) => void;
  onComplete: (season: FootballSeasonDto) => void;
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
  const footballDivisions = useMemo(
    () => divisions.filter((division) => division.sportType === SportsCategory.Football),
    [divisions],
  );
  const { countByCompetitionId } = useInProgressFootballMatches();
  const byId = new Map(seasons.map((season) => [season.id, season]));

  return (
    <AdminSeasonsTable
      sport="football"
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
          name: footballDivisions.find((division) => division.id === seasonDivision.divisionId)?.name
            || seasonDivision.divisionId,
        })),
      }))}
      labels={{
        name: t('football.seasons.fields.name', 'Name'),
        division: t('football.seasons.fields.division', 'Division'),
        startDate: t('football.seasons.fields.startDate', 'Starts'),
        endDate: t('football.seasons.fields.endDate', 'Ends'),
        teams: t('football.seasons.fields.teams', 'Teams'),
        status: t('football.seasons.fields.status', 'Status'),
        completed: t('football.seasons.status.completed', 'Completed'),
        active: t('football.seasons.status.active', 'Active'),
        inactive: t('football.seasons.status.inactive', 'Inactive'),
        deactivate: t('football.seasons.deactivate', 'Deactivate'),
        activate: t('football.seasons.activate', 'Activate'),
        complete: t('football.seasons.complete', 'Complete Season'),
        noDivisions: t('football.seasons.noDivisions', 'No divisions'),
        teamsCount: t('football.seasons.teamsCount', 'teams'),
        matchesInProgress: (count) => t(
          'football.seasons.matchesInProgress',
          '{{count}} match(es) in progress',
          { count },
        ),
        openEdit: t('football.seasons.actions.openEdit', 'Open and edit season'),
        actionsMenu: t('football.seasons.actions.menu', 'Season actions menu'),
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
