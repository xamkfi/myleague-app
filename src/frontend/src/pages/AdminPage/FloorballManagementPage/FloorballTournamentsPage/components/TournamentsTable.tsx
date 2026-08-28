import { useTranslation } from 'react-i18next';
import type { FloorballTournamentDto } from '../../../../../types/floorball/tournamentTypes';
import AdminTournamentsTable from '../../../../../components/admin/AdminTournamentsTable';
import { useInProgressMatches } from '../../../../../hooks/useInProgressMatches';

interface TournamentsTableProps {
  tournaments: FloorballTournamentDto[];
  onEdit: (tournament: FloorballTournamentDto) => void;
}

const getStatusBadgeClass = (status: string): string => {
  switch (status) {
    case 'Draft': return 'admin-badge--inactive';
    case 'GroupStage':
    case 'PlayoffStage': return 'admin-badge--active';
    case 'Completed': return 'admin-badge--completed';
    case 'Cancelled': return 'admin-badge--danger';
    default: return 'admin-badge--inactive';
  }
};

export const TournamentsTable = ({
  tournaments,
  onEdit,
}: TournamentsTableProps) => {
  const { t } = useTranslation();
  const { countByCompetitionId } = useInProgressMatches();
  const byId = new Map(tournaments.map((tournament) => [tournament.id, tournament]));

  const getStatusLabel = (status: string): string => {
    switch (status) {
      case 'Draft': return t('floorball.tournaments.status.draft', 'Draft');
      case 'GroupStage': return t('floorball.tournaments.status.groupStage', 'Group Stage');
      case 'PlayoffStage': return t('floorball.tournaments.status.playoffStage', 'Playoff Stage');
      case 'Completed': return t('floorball.tournaments.status.completed', 'Completed');
      case 'Cancelled': return t('floorball.tournaments.status.cancelled', 'Cancelled');
      default: return status;
    }
  };

  return (
    <AdminTournamentsTable
      sport="floorball"
      tournaments={tournaments.map((tournament) => ({
        id: tournament.id,
        name: tournament.name,
        teamCategory: tournament.teamCategory,
        startDate: tournament.startDate,
        endDate: tournament.endDate,
        teamCount: tournament.teamCount,
        matchCount: tournament.matchCount,
        status: tournament.tournamentStatus,
        statusLabel: getStatusLabel(tournament.tournamentStatus),
        statusClassName: getStatusBadgeClass(tournament.tournamentStatus),
        groups: tournament.groups ?? [],
      }))}
      labels={{
        name: t('floorball.tournaments.fields.name', 'Name'),
        groups: t('floorball.tournaments.fields.groups', 'Groups'),
        startDate: t('floorball.tournaments.fields.startDate', 'Starts'),
        endDate: t('floorball.tournaments.fields.endDate', 'Ends'),
        teams: t('floorball.tournaments.fields.teams', 'Teams'),
        matches: t('floorball.tournaments.fields.matches', 'Matches'),
        status: t('floorball.tournaments.fields.status', 'Status'),
        noGroups: t('floorball.tournaments.noGroups', 'No groups'),
        teamsCount: (count) => t('floorball.tournaments.teamsCount', '{{count}} teams', { count }),
        matchesCount: (count) => t('floorball.tournaments.matchesCount', '{{count}} matches', { count }),
        matchesInProgress: (count) => t(
          'floorball.tournaments.matchesInProgress',
          '{{count}} match(es) in progress',
          { count },
        ),
        openEdit: t('floorball.tournaments.actions.openEdit', 'Open and edit tournament'),
        actionsMenu: t('floorball.tournaments.actions.menu', 'Tournament actions menu'),
      }}
      liveCounts={countByCompetitionId}
      onEdit={(tournamentId) => {
        const tournament = byId.get(tournamentId);
        if (tournament) onEdit(tournament);
      }}
    />
  );
};
