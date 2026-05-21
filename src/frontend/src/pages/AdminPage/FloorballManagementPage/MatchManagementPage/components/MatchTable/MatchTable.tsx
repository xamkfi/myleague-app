import { useTranslation } from 'react-i18next';
import type { FloorballMatchDto, FloorballMatchStatus } from '../../../../../../types/floorball/floorballTypes';
import { formatDateTime } from '../../../ManageMatchPage/utils/matchFormatters';
import ActionsDropdown from '../../../../../../components/ActionsDropdown/ActionsDropdown';
import LoadingSpinner from '../../../../../../components/LoadingSpinner/LoadingSpinner';
import '../../../../../../styles/AdminTable.scss';
import './MatchTable.scss';

interface MatchTableProps {
  matches: FloorballMatchDto[];
  loading: boolean;
  onLiveMatch: (match: FloorballMatchDto) => void;
  onEditMatch: (match: FloorballMatchDto) => void;
  onOpenMatch: (match: FloorballMatchDto) => void;
  onStartMatch: (match: FloorballMatchDto) => void;
  onCancelMatch: (match: FloorballMatchDto) => void;
  onReactivateMatch: (match: FloorballMatchDto) => void;
  /**
   * When true, hides the Actions column entirely. Used by callers (e.g. the tournament
   * edit page) where the only meaningful action is "open the match", which the whole
   * row already triggers via {@link onLiveMatch}. Defaults to false so the global match
   * management page keeps its existing dropdown menu.
   */
  hideActions?: boolean;
}

const MatchTable = ({
  matches,
  loading,
  onLiveMatch,
  onEditMatch,
  onOpenMatch,
  onStartMatch,
  onCancelMatch,
  onReactivateMatch,
  hideActions = false,
}: MatchTableProps) => {
  const { t } = useTranslation();

  const getMatchStatusBadge = (status: FloorballMatchStatus) => {
    const map: Record<string, { className: string; label: string }> = {
      Scheduled: {
        className: 'admin-badge admin-badge--info',
        label: t('floorball.matches.status.scheduled', 'Scheduled'),
      },
      InProgress: {
        className: 'admin-badge admin-badge--active',
        label: t('floorball.matches.status.inProgress', 'In Progress'),
      },
      Completed: {
        className: 'admin-badge admin-badge--completed',
        label: t('floorball.matches.status.completed', 'Completed'),
      },
      Cancelled: {
        className: 'admin-badge admin-badge--danger',
        label: t('floorball.matches.status.cancelled', 'Cancelled'),
      },
      Postponed: {
        className: 'admin-badge admin-badge--warning',
        label: t('floorball.matches.status.postponed', 'Postponed'),
      },
    };

    return map[status] ?? { className: 'admin-badge', label: status };
  };

  const getActions = (match: FloorballMatchDto) => {
    const actions: {
      label: string;
      onClick: () => void;
      variant?: 'default' | 'danger' | 'status';
      disabled: boolean;
    }[] = [];

    actions.push({
      label: t('floorball.matches.actions.open', 'Open Match'),
      onClick: () => onOpenMatch(match),
      disabled: false,
    });

    if (match.status === 'InProgress') {
      actions.push({
        label: t('floorball.matches.actions.live', 'Live View'),
        onClick: () => onLiveMatch(match),
        disabled: false,
      });

      actions.push({
        label: t('common.edit', 'Edit'),
        onClick: () => onEditMatch(match),
        disabled: false,
      });
    } else {
      actions.push({
        label: t('floorball.matches.actions.manage', 'Manage'),
        onClick: () => onEditMatch(match),
        disabled: false,
      });
    }

    if ((match.status === 'Scheduled' || match.status === 'Postponed') && onStartMatch) {
      actions.push({
        label: t('floorball.matches.actions.start', 'Start Match'),
        onClick: () => onStartMatch(match),
        variant: 'status',
        disabled: false,
      });
    }

    if (match.status === 'Cancelled') {
      actions.push({
        label: t('floorball.matches.actions.reactivate', 'Reactivate Match'),
        onClick: () => onReactivateMatch(match),
        disabled: false,
      });
    }

    if (
      match.status !== 'Cancelled' &&
      match.status !== 'Completed' &&
      match.status !== 'InProgress'
    ) {
      actions.push({
        label: t('floorball.matches.actions.cancel', 'Cancel Match'),
        onClick: () => onCancelMatch(match),
        variant: 'danger',
        disabled: false,
      });
    }

    return actions;
  };

  if (loading) {
    return (
      <div className="match-table__loading">
        <LoadingSpinner text={t('floorball.matches.loading', 'Loading matches...')} />
      </div>
    );
  }

  if (matches.length === 0) {
    return (
      <div className="match-table__empty">
        <i className="fas fa-calendar-times match-table__empty-icon"></i>
        <p>{t('floorball.matches.noMatchesFound', 'No matches found')}</p>
      </div>
    );
  }

  return (
    <div className="admin-table__wrapper">
      <table className="admin-table">
        <thead>
          <tr>
            <th>{t('floorball.matches.columns.match', 'Match')}</th>
            <th>{t('floorball.matches.columns.season', 'Season')}</th>
            <th>{t('floorball.matches.columns.dateTime', 'Date & Time')}</th>
            <th>{t('floorball.matches.columns.venue', 'Venue')}</th>
            <th>{t('floorball.matches.columns.score', 'Score')}</th>
            <th>{t('floorball.matches.columns.status', 'Status')}</th>
            {!hideActions && (
              <th className="admin-table__actions-col">{t('common.actions', 'Actions')}</th>
            )}
          </tr>
        </thead>

        <tbody>
          {matches.map((match: FloorballMatchDto) => (
            <tr
              key={match.id}
              className="admin-table__row--clickable"
              onClick={() => onLiveMatch(match)}
            >
              <td>
                <div className="match-table__teams">
                  <span className="admin-table__name">{match.homeTeamName}</span>
                  <span className="match-table__vs">vs</span>
                  <span className="admin-table__name">{match.awayTeamName}</span>
                </div>
              </td>

              <td>
                <span className="admin-table__muted">{match.competitionName || '-'}</span>
              </td>

              <td className="admin-table__muted">
                {formatDateTime(match.scheduledDateTime)}
              </td>

              <td>
                {match.venue ? (
                  <span className="admin-table__muted">{match.venue}</span>
                ) : (
                  <span className="admin-table__muted match-table__tbd">
                    {t('floorball.matches.tbd', 'TBD')}
                  </span>
                )}
              </td>

              <td>
                {match.status === 'Scheduled' || match.status === 'Postponed' ? (
                  <span className="admin-table__muted">-</span>
                ) : (
                  <span className="admin-table__bold">
                    {match.homeScore} - {match.awayScore}
                  </span>
                )}
              </td>

              <td>
                {(() => {
                  const badge = getMatchStatusBadge(match.status);
                  return <span className={badge.className}>{badge.label}</span>;
                })()}
              </td>

              {!hideActions && (
                <td
                  className="admin-table__actions-col"
                  onClick={(e) => e.stopPropagation()}
                >
                  <ActionsDropdown
                    actions={getActions(match)}
                    ariaLabel={t('floorball.matches.actions.menu', 'Match actions menu')}
                  />
                </td>
              )}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default MatchTable;