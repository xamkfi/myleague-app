import { useTranslation } from 'react-i18next';
import type { HockeySeasonDto } from '../../../../../types/hockey/hockeyTypes';
import AdminSeasonsTable from '../../../../../components/admin/AdminSeasonsTable';
import { useHockeyInProgressMatches } from '../../../../../hooks/useHockeyInProgressMatches';
import { formatHockeyDate } from '../../../../../utils/hockeyLookups';

interface SeasonsTableProps {
  seasons: HockeySeasonDto[];
  onEdit: (season: HockeySeasonDto) => void;
  onActivateToggle: (season: HockeySeasonDto) => void;
  onComplete: (season: HockeySeasonDto) => void;
  operationLoading?: string | null;
}

export function SeasonsTable({
  seasons,
  onEdit,
  onActivateToggle,
  onComplete,
  operationLoading,
}: SeasonsTableProps) {
  const { t } = useTranslation();
  const { countByCompetitionId } = useHockeyInProgressMatches();
  const byId = new Map(seasons.map((season) => [season.id, season]));

  return (
    <AdminSeasonsTable
      sport="hockey"
      seasons={seasons.map((season) => ({
        id: season.id,
        name: season.name,
        teamCategory: season.teamCategory,
        startDate: season.startDate,
        endDate: season.endDate,
        teamCount: season.teams?.length || 0,
        isActive: season.isActive,
        isCompleted: season.isCompleted,
        divisions: (season.divisions ?? []).map((division) => ({
          id: division.id,
          name: division.name,
        })),
      }))}
      labels={{
        name: t('hockey.seasons.fields.name', 'Name'),
        division: t('hockey.seasons.fields.division', 'Division'),
        startDate: t('hockey.seasons.fields.startDate', 'Starts'),
        endDate: t('hockey.seasons.fields.endDate', 'Ends'),
        teams: t('hockey.seasons.fields.teams', 'Teams'),
        status: t('hockey.seasons.fields.status', 'Status'),
        completed: t('hockey.seasons.statusCompleted', 'Completed'),
        active: t('hockey.seasons.statusActive', 'Active'),
        inactive: t('hockey.seasons.statusInactive', 'Inactive'),
        deactivate: t('hockey.seasons.deactivate', 'Deactivate'),
        activate: t('hockey.seasons.activate', 'Activate'),
        complete: t('hockey.seasons.complete', 'Complete Season'),
        noDivisions: t('hockey.seasons.noDivisions', 'No divisions'),
        teamsCount: t('hockey.seasons.teamsCountLabel', 'teams'),
        matchesInProgress: (count) => t(
          'hockey.seasons.matchesInProgress',
          '{{count}} match(es) in progress',
          { count },
        ),
        openEdit: t('hockey.seasons.actions.openEdit', 'Open and edit season'),
        actionsMenu: t('hockey.seasons.actions.menu', 'Season actions menu'),
      }}
      liveCounts={countByCompetitionId}
      formatDate={formatHockeyDate}
      onEdit={(seasonId) => {
        const season = byId.get(seasonId);
        if (season) onEdit(season);
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
}
