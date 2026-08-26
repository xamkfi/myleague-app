import { useTranslation } from 'react-i18next';
import ActionsDropdown from '../ActionsDropdown/ActionsDropdown';
import LoadingSpinner from '../LoadingSpinner/LoadingSpinner';
import TeamLink from '../SportLinks/TeamLink';
import type { SportKind } from '../../utils/sportRoutes';
import type { AdminAction, AdminMatchRow, AdminMatchTableLabels } from './adminTableTypes';
import '../../styles/AdminTable.scss';
import './AdminMatchTable.scss';

interface AdminMatchTableProps {
  sport: SportKind;
  matches: AdminMatchRow[];
  labels: AdminMatchTableLabels;
  loading: boolean;
  hideActions?: boolean;
  formatDateTime: (value: string) => string;
  getStatusBadge: (status: string) => { className: string; label: string };
  getActions: (match: AdminMatchRow) => AdminAction[];
  onRowClick: (match: AdminMatchRow) => void;
}

export default function AdminMatchTable({
  sport,
  matches,
  labels,
  loading,
  hideActions = false,
  formatDateTime,
  getStatusBadge,
  getActions,
  onRowClick,
}: AdminMatchTableProps) {
  const { t } = useTranslation();

  if (loading) {
    return (
      <div className="match-table__loading">
        <LoadingSpinner text={labels.loading} />
      </div>
    );
  }

  if (matches.length === 0) {
    return (
      <div className="match-table__empty">
        <i className="fas fa-calendar-times match-table__empty-icon"></i>
        <p>{labels.noMatchesFound}</p>
      </div>
    );
  }

  return (
    <div className="admin-table__wrapper">
      <table className="admin-table">
        <thead>
          <tr>
            <th>{labels.match}</th>
            <th>{labels.season}</th>
            <th>{labels.dateTime}</th>
            <th>{labels.venue}</th>
            <th>{labels.score}</th>
            <th>{labels.status}</th>
            {!hideActions && (
              <th className="admin-table__actions-col">{t('common.actions')}</th>
            )}
          </tr>
        </thead>
        <tbody>
          {matches.map((match) => {
            const badge = getStatusBadge(match.status);
            const hideScore = match.status === 'Scheduled' || match.status === 'Postponed';

            return (
              <tr
                key={match.id}
                className="admin-table__row--clickable"
                onClick={() => onRowClick(match)}
              >
                <td>
                  <div className="match-table__teams">
                    {match.homeTeamId && match.homeTeamName ? (
                      <TeamLink
                        sport={sport}
                        teamName={match.homeTeamName}
                        teamId={match.homeTeamId}
                        className="admin-table__name"
                      />
                    ) : (
                      <span className="admin-table__name">{match.homeTeamName || labels.tbd}</span>
                    )}
                    <span className="match-table__vs">vs</span>
                    {match.awayTeamId && match.awayTeamName ? (
                      <TeamLink
                        sport={sport}
                        teamName={match.awayTeamName}
                        teamId={match.awayTeamId}
                        className="admin-table__name"
                      />
                    ) : (
                      <span className="admin-table__name">{match.awayTeamName || labels.tbd}</span>
                    )}
                  </div>
                </td>
                <td>
                  <span className="admin-table__muted">{match.competitionName || '-'}</span>
                </td>
                <td className="admin-table__muted">{formatDateTime(match.scheduledDateTime)}</td>
                <td>
                  {match.venue ? (
                    <span className="admin-table__muted">{match.venue}</span>
                  ) : (
                    <span className="admin-table__muted match-table__tbd">{labels.tbd}</span>
                  )}
                </td>
                <td>
                  {hideScore ? (
                    <span className="admin-table__muted">-</span>
                  ) : (
                    <span className="admin-table__bold">
                      {match.homeScore} - {match.awayScore}
                    </span>
                  )}
                </td>
                <td>
                  <span className={badge.className}>{badge.label}</span>
                </td>
                {!hideActions && (
                  <td
                    className="admin-table__actions-col"
                    onClick={(event) => event.stopPropagation()}
                  >
                    <ActionsDropdown
                      actions={getActions(match)}
                      ariaLabel={labels.actionsMenu}
                    />
                  </td>
                )}
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
}
