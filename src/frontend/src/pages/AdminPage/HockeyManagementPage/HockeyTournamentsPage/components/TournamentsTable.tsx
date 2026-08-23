import { useTranslation } from 'react-i18next';
import type { HockeyTournamentDto } from '../../../../../types/hockey/hockeyTypes';
import LiveDot from '../../../../../components/LiveDot/LiveDot';
import { useHockeyInProgressMatches } from '../../../../../hooks/useHockeyInProgressMatches';
import { formatHockeyDate } from '../../../../../utils/hockeyLookups';
import '../../../../../styles/AdminTable.scss';

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
    <table className="admin-table">
      <thead>
        <tr>
          <th>{t('hockey.tournaments.fields.name', 'Name')}</th>
          <th>{t('hockey.tournaments.fields.groups', 'Groups')}</th>
          <th>{t('hockey.tournaments.fields.startDate', 'Starts')}</th>
          <th>{t('hockey.tournaments.fields.endDate', 'Ends')}</th>
          <th>{t('hockey.tournaments.fields.teams', 'Teams')}</th>
          <th>{t('hockey.tournaments.fields.status', 'Status')}</th>
        </tr>
      </thead>
      <tbody>
        {tournaments.map((tournament) => {
          const liveCount = countByCompetitionId.get(tournament.id) ?? 0;
          const statusValue = tournament.currentStage || tournament.status;
          return (
            <tr
              key={tournament.id}
              className="admin-table__row--clickable"
              onClick={() => onEdit(tournament)}
              role="button"
              tabIndex={0}
              onKeyDown={(event) => {
                if (event.key === 'Enter' || event.key === ' ') {
                  event.preventDefault();
                  onEdit(tournament);
                }
              }}
              title={t('hockey.tournaments.actions.openEdit', 'Open and edit tournament')}
            >
              <td className="admin-table__name">
                <span className="admin-table__name-inner">
                  {liveCount > 0 && (
                    <LiveDot
                      tone="light"
                      count={liveCount}
                      ariaLabel={t('hockey.tournaments.matchesInProgress', '{{count}} match(es) in progress', { count: liveCount })}
                    />
                  )}
                  <span>{tournament.name}</span>
                </span>
              </td>
              <td>
                <div style={{ display: 'flex', flexWrap: 'wrap', gap: '0.25rem' }}>
                  {(tournament.groups ?? []).length > 0 ? (
                    tournament.groups.map((group) => (
                      <span key={group.id} className="admin-tag admin-tag--blue">{group.name}</span>
                    ))
                  ) : (
                    <span className="admin-table__muted">{t('hockey.tournaments.noGroups', 'No groups')}</span>
                  )}
                </div>
              </td>
              <td>{formatHockeyDate(tournament.startDate)}</td>
              <td>{formatHockeyDate(tournament.endDate)}</td>
              <td>
                <span className="admin-table__muted">
                  {t('hockey.tournaments.teamsCount', '{{count}} teams', { count: tournament.teams.length })}
                </span>
              </td>
              <td>
                <span className={`admin-badge ${getStatusBadgeClass(statusValue)}`}>
                  {getStatusLabel(tournament)}
                </span>
              </td>
            </tr>
          );
        })}
      </tbody>
    </table>
  );
}
