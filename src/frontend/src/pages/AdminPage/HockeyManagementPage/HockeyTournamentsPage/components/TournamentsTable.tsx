import { useTranslation } from 'react-i18next';
import type { HockeyTournamentDto } from '../../../../../types/hockey/hockeyTypes';
import AdminTournamentsTable from '../../../../../components/admin/AdminTournamentsTable';
import { useHockeyInProgressMatches } from '../../../../../hooks/useHockeyInProgressMatches';
import { formatHockeyDate } from '../../../../../utils/hockeyLookups';

interface TournamentsTableProps {
  tournaments: HockeyTournamentDto[];
  onEdit: (tournament: HockeyTournamentDto) => void;
}

const getStatusBadgeClass = (status: string): string => {
  switch (status) {
    case 'Active':
    case 'GroupStage':
    case 'PlayoffStage':
    case 'RegistrationOpen':
      return 'admin-badge--active';
    case 'Completed':
      return 'admin-badge--completed';
    case 'Cancelled':
      return 'admin-badge--danger';
    default:
      return 'admin-badge--inactive';
  }
};

export function TournamentsTable({ tournaments, onEdit }: TournamentsTableProps) {
  const { t } = useTranslation();
  const { countByCompetitionId } = useHockeyInProgressMatches();
  const byId = new Map(tournaments.map((tournament) => [tournament.id, tournament]));

  const getStatusLabel = (tournament: HockeyTournamentDto): string => {
    const value = tournament.currentStage && tournament.currentStage !== tournament.status
      ? tournament.currentStage
      : tournament.status;
    switch (value) {
      case 'Draft':
        return t('hockey.tournaments.statusDraft', 'Draft');
      case 'Published':
        return t('hockey.tournaments.statusPublished', 'Published');
      case 'RegistrationOpen':
        return t('hockey.tournaments.statusRegistrationOpen', 'Registration open');
      case 'Active':
        return t('hockey.tournaments.statusActive', 'Active');
      case 'GroupStage':
        return t('hockey.tournaments.statusGroupStage', 'Group Stage');
      case 'PlayoffStage':
        return t('hockey.tournaments.statusPlayoffStage', 'Playoff Stage');
      case 'Completed':
        return t('hockey.tournaments.statusCompleted', 'Completed');
      case 'Cancelled':
        return t('hockey.tournaments.statusCancelled', 'Cancelled');
      default:
        return value;
    }
  };

  return (
    <AdminTournamentsTable
      sport="hockey"
      showMatchCount={false}
      formatDate={formatHockeyDate}
      tournaments={tournaments.map((tournament) => {
        const statusValue = tournament.currentStage || tournament.status;
        return {
          id: tournament.id,
          name: tournament.name,
          teamCategory: tournament.teamCategory,
          startDate: tournament.startDate,
          endDate: tournament.endDate,
          teamCount: tournament.teams.length,
          status: statusValue,
          statusLabel: getStatusLabel(tournament),
          statusClassName: getStatusBadgeClass(statusValue),
          groups: tournament.groups ?? [],
        };
      })}
      labels={{
        name: t('hockey.tournaments.fields.name', 'Name'),
        groups: t('hockey.tournaments.fields.groups', 'Groups'),
        startDate: t('hockey.tournaments.fields.startDate', 'Starts'),
        endDate: t('hockey.tournaments.fields.endDate', 'Ends'),
        teams: t('hockey.tournaments.fields.teams', 'Teams'),
        matches: t('hockey.tournaments.fields.matches', 'Matches'),
        status: t('hockey.tournaments.fields.status', 'Status'),
        noGroups: t('hockey.tournaments.noGroups', 'No groups'),
        teamsCount: (count) => t('hockey.tournaments.teamsCount', '{{count}} teams', { count }),
        matchesCount: (count) => t('hockey.tournaments.matchesCount', '{{count}} matches', { count }),
        matchesInProgress: (count) => t(
          'hockey.tournaments.matchesInProgress',
          '{{count}} match(es) in progress',
          { count },
        ),
        openEdit: t('hockey.tournaments.actions.openEdit', 'Open and edit tournament'),
        actionsMenu: t('hockey.tournaments.actions.menu', 'Tournament actions menu'),
      }}
      liveCounts={countByCompetitionId}
      onEdit={(tournamentId) => {
        const tournament = byId.get(tournamentId);
        if (tournament) onEdit(tournament);
      }}
    />
  );
}
