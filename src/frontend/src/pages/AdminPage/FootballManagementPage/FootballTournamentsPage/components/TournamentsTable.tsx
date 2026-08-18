import { useTranslation } from 'react-i18next';
import type { FootballTournamentDto } from '../../../../../types/football/tournamentTypes';
import LiveDot from '../../../../../components/LiveDot/LiveDot';
import TeamCategoryBadge from '../../../../../components/TeamCategoryBadge/TeamCategoryBadge';
// TODO: parent agent will add useInProgressFootballMatches
import { useInProgressFootballMatches } from '../../../../../hooks/useInProgressFootballMatches';
import '../../../../../styles/AdminTable.scss';

interface TournamentsTableProps {
  tournaments: FootballTournamentDto[];
  /** Navigates to the tournament edit page; triggered by clicking anywhere on the row. */
  onEdit: (tournament: FootballTournamentDto) => void;
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
  const { countByCompetitionId } = useInProgressFootballMatches();

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
    <table className="admin-table">
      <thead>
        <tr>
          <th>{t('football.tournaments.fields.name', 'Name')}</th>
          <th>{t('football.tournaments.fields.groups', 'Groups')}</th>
          <th>{t('football.tournaments.fields.startDate', 'Starts')}</th>
          <th>{t('football.tournaments.fields.endDate', 'Ends')}</th>
          <th>{t('football.tournaments.fields.teams', 'Teams')}</th>
          <th>{t('football.tournaments.fields.matches', 'Matches')}</th>
          <th>{t('football.tournaments.fields.status', 'Status')}</th>
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
              title={t('football.tournaments.actions.openEdit', 'Open and edit tournament')}
            >
              <td className="admin-table__name">
                <span className="admin-table__name-inner">
                  {liveCount > 0 && (
                    <LiveDot
                      tone="light"
                      count={liveCount}
                      ariaLabel={t('football.tournaments.matchesInProgress', '{{count}} match(es) in progress', { count: liveCount })}
                    />
                  )}
                  <span>{tournament.name}</span>
                  <TeamCategoryBadge category={tournament.teamCategory} />
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
                      {t('football.tournaments.noGroups', 'No groups')}
                    </span>
                  )}
                </div>
              </td>
              <td>{formatDate(tournament.startDate)}</td>
              <td>{formatDate(tournament.endDate)}</td>
              <td>
                <span className="admin-table__muted">
                  {t('football.tournaments.teamsCount', '{{count}} teams', { count: tournament.teamCount })}
                </span>
              </td>
              <td>
                <span className="admin-table__muted">
                  {t('football.tournaments.matchesCount', '{{count}} matches', { count: tournament.matchCount })}
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
