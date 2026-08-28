import { useTranslation } from 'react-i18next';
import type { FootballTournamentDto } from '../../../../../types/football/tournamentTypes';
import AdminTournamentsTable from '../../../../../components/admin/AdminTournamentsTable';
import { useInProgressFootballMatches } from '../../../../../hooks/useInProgressFootballMatches';

interface TournamentsTableProps {
  tournaments: FootballTournamentDto[];
  onEdit: (tournament: FootballTournamentDto) => void;
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
  const { countByCompetitionId } = useInProgressFootballMatches();
  const byId = new Map(tournaments.map((tournament) => [tournament.id, tournament]));

  const getStatusLabel = (status: string): string => {
    switch (status) {
      case 'Draft': return t('football.tournaments.status.draft', 'Draft');
      case 'GroupStage': return t('football.tournaments.status.groupStage', 'Group Stage');
      case 'PlayoffStage': return t('football.tournaments.status.playoffStage', 'Playoff Stage');
      case 'Completed': return t('football.tournaments.status.completed', 'Completed');
      case 'Cancelled': return t('football.tournaments.status.cancelled', 'Cancelled');
      default: return status;
    }
  };

  return (
    <AdminTournamentsTable
      sport="football"
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
        name: t('football.tournaments.fields.name', 'Name'),
        groups: t('football.tournaments.fields.groups', 'Groups'),
        startDate: t('football.tournaments.fields.startDate', 'Starts'),
        endDate: t('football.tournaments.fields.endDate', 'Ends'),
        teams: t('football.tournaments.fields.teams', 'Teams'),
        matches: t('football.tournaments.fields.matches', 'Matches'),
        status: t('football.tournaments.fields.status', 'Status'),
        noGroups: t('football.tournaments.noGroups', 'No groups'),
        teamsCount: (count) => t('football.tournaments.teamsCount', '{{count}} teams', { count }),
        matchesCount: (count) => t('football.tournaments.matchesCount', '{{count}} matches', { count }),
        matchesInProgress: (count) => t(
          'football.tournaments.matchesInProgress',
          '{{count}} match(es) in progress',
          { count },
        ),
        openEdit: t('football.tournaments.actions.openEdit', 'Open and edit tournament'),
        actionsMenu: t('football.tournaments.actions.menu', 'Tournament actions menu'),
      }}
      liveCounts={countByCompetitionId}
      onEdit={(tournamentId) => {
        const tournament = byId.get(tournamentId);
        if (tournament) onEdit(tournament);
      }}
    />
  );
};
