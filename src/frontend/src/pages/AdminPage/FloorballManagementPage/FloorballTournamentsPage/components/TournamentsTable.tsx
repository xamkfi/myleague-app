import { useTranslation } from 'react-i18next';
import type { FloorballTournamentDto } from '../../../../../types/floorball/tournamentTypes';
import LiveDot from '../../../../../components/LiveDot/LiveDot';
import { useInProgressMatches } from '../../../../../hooks/useInProgressMatches';
import '../../../../../styles/AdminTable.scss';

interface TournamentsTableProps {
  tournaments: FloorballTournamentDto[];
  /** Navigates to the tournament edit page; triggered by clicking anywhere on the row. */
  onEdit: (tournament: FloorballTournamentDto) => void;
}

const formatDate = (dateString: string): string => {
  try {
    return new Date(dateString).toLocaleDateString();
  } catch {
    return dateString;
  }
};

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
    <table className="admin-table">
      <thead>
        <tr>
          <th>{t('floorball.tournaments.fields.name', 'Name')}</th>
          <th>{t('floorball.tournaments.fields.groups', 'Groups')}</th>
          <th>{t('floorball.tournaments.fields.startDate', 'Starts')}</th>
          <th>{t('floorball.tournaments.fields.endDate', 'Ends')}</th>
          <th>{t('floorball.tournaments.fields.teams', 'Teams')}</th>
          <th>{t('floorball.tournaments.fields.matches', 'Matches')}</th>
          <th>{t('floorball.tournaments.fields.status', 'Status')}</th>
        </tr>
      </thead>
      <tbody>
        {tournaments.map((tournament) => {
          const liveCount: number = countByCompetitionId.get(tournament.id) ?? 0;
          return (
            <tr
              key={tournament.id}
              className="admin-table__row--clickable"
              onClick={() => onEdit(tournament)}
              role="button"
              tabIndex={0}
              onKeyDown={(e) => {
                if (e.key === 'Enter' || e.key === ' ') {
                  e.preventDefault();
                  onEdit(tournament);
                }
              }}
              title={t('floorball.tournaments.actions.openEdit', 'Open and edit tournament')}
            >
              <td className="admin-table__name">
                <span className="admin-table__name-inner">
                  {liveCount > 0 && (
                    <LiveDot
                      tone="light"
                      count={liveCount}
                      ariaLabel={t('floorball.tournaments.matchesInProgress', '{{count}} match(es) in progress', { count: liveCount })}
                    />
                  )}
                  <span>{tournament.name}</span>
                </span>
              </td>
              <td>
                <div style={{ display: 'flex', flexWrap: 'wrap', gap: '0.25rem' }}>
                  {tournament.groups && tournament.groups.length > 0 ? (
                    tournament.groups.map((group) => (
                      <span key={group.id} className="admin-tag admin-tag--blue">
                        {group.name}
                      </span>
                    ))
                  ) : (
                    <span className="admin-table__muted">
                      {t('floorball.tournaments.noGroups', 'No groups')}
                    </span>
                  )}
                </div>
              </td>
              <td>{formatDate(tournament.startDate)}</td>
              <td>{formatDate(tournament.endDate)}</td>
              <td>
                <span className="admin-table__muted">
                  {t('floorball.tournaments.teamsCount', '{{count}} teams', { count: tournament.teamCount })}
                </span>
              </td>
              <td>
                <span className="admin-table__muted">
                  {t('floorball.tournaments.matchesCount', '{{count}} matches', { count: tournament.matchCount })}
                </span>
              </td>
              <td>
                <span className={`admin-badge ${getStatusBadgeClass(tournament.tournamentStatus)}`}>
                  {getStatusLabel(tournament.tournamentStatus)}
                </span>
              </td>
            </tr>
          );
        })}
      </tbody>
    </table>
  );
};
