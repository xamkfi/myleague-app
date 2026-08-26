import { useTranslation } from 'react-i18next';
import type { HockeyMatchDto } from '../../../../../../types/hockey/hockeyTypes';
import { isHockeyMatchFinished, isHockeyMatchLive } from '../../../../../../types/hockey/hockeyTypes';
import { formatHockeyDateTime } from '../../../../../../utils/hockeyLookups';
import ActionsDropdown from '../../../../../../components/ActionsDropdown/ActionsDropdown';
import LoadingSpinner from '../../../../../../components/LoadingSpinner/LoadingSpinner';
import '../../../../../../styles/AdminTable.scss';
import './MatchTable.scss';

interface MatchTableProps {
  matches: HockeyMatchDto[];
  teamNames: Map<string, string>;
  competitionNames: Map<string, string>;
  loading: boolean;
  onLiveMatch: (match: HockeyMatchDto) => void;
  onEditMatch: (match: HockeyMatchDto) => void;
  onOpenMatch: (match: HockeyMatchDto) => void;
  onStartMatch: (match: HockeyMatchDto) => void;
  onCancelMatch: (match: HockeyMatchDto) => void;
  onReactivateMatch: (match: HockeyMatchDto) => void;
  hideActions?: boolean;
}

function MatchTable({
  matches,
  teamNames,
  competitionNames,
  loading,
  onLiveMatch,
  onEditMatch,
  onOpenMatch,
  onStartMatch,
  onCancelMatch,
  onReactivateMatch,
  hideActions = false,
}: MatchTableProps) {
  const { t } = useTranslation();

  const getMatchStatusBadge = (status: string): { className: string; label: string } => {
    if (isHockeyMatchLive(status)) {
      return {
        className: 'admin-badge admin-badge--active',
        label: status === 'InProgress'
          ? t('hockey.matches.status.inProgress', 'In Progress')
          : t(`hockey.matches.status.${status.charAt(0).toLowerCase()}${status.slice(1)}`, status),
      };
    }
    const map: Record<string, { className: string; label: string }> = {
      Scheduled: {
        className: 'admin-badge admin-badge--info',
        label: t('hockey.matches.status.scheduled', 'Scheduled'),
      },
      Finished: {
        className: 'admin-badge admin-badge--completed',
        label: t('hockey.matches.status.completed', 'Completed'),
      },
      Forfeit: {
        className: 'admin-badge admin-badge--completed',
        label: t('hockey.matches.status.forfeit', 'Forfeit'),
      },
      Cancelled: {
        className: 'admin-badge admin-badge--danger',
        label: t('hockey.matches.status.cancelled', 'Cancelled'),
      },
      Postponed: {
        className: 'admin-badge admin-badge--warning',
        label: t('hockey.matches.status.postponed', 'Postponed'),
      },
    };
    return map[status] ?? { className: 'admin-badge', label: status };
  };

  const getActions = (match: HockeyMatchDto) => {
    const actions: {
      label: string;
      onClick: () => void;
      variant?: 'default' | 'danger' | 'status';
      disabled: boolean;
    }[] = [];

    actions.push({
      label: t('hockey.matches.actions.open', 'Open Match'),
      onClick: () => onOpenMatch(match),
      disabled: false,
    });

    if (isHockeyMatchLive(match.status)) {
      actions.push({
        label: t('hockey.matches.actions.live', 'Live View'),
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
        label: t('hockey.matches.actions.manage', 'Manage'),
        onClick: () => onEditMatch(match),
        disabled: false,
      });
    }

    if (match.status === 'Scheduled' || match.status === 'Postponed') {
      actions.push({
        label: t('hockey.matches.actions.start', 'Start Match'),
        onClick: () => onStartMatch(match),
        variant: 'status',
        disabled: false,
      });
    }

    if (match.status === 'Cancelled') {
      actions.push({
        label: t('hockey.matches.actions.reactivate', 'Reactivate Match'),
        onClick: () => onReactivateMatch(match),
        disabled: false,
      });
    }

    if (match.status !== 'Cancelled' && !isHockeyMatchFinished(match.status) && !isHockeyMatchLive(match.status)) {
      actions.push({
        label: t('hockey.matches.actions.cancel', 'Cancel Match'),
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
        <LoadingSpinner text={t('hockey.matches.loading', 'Loading matches...')} />
      </div>
    );
  }

  if (matches.length === 0) {
    return (
      <div className="match-table__empty">
        <i className="fas fa-calendar-times match-table__empty-icon"></i>
        <p>{t('hockey.matches.noMatchesFound', 'No matches found')}</p>
      </div>
    );
  }

  return (
    <div className="admin-table__wrapper">
      <table className="admin-table">
        <thead>
          <tr>
            <th>{t('hockey.matches.columns.match', 'Match')}</th>
            <th>{t('hockey.matches.columns.season', 'Season')}</th>
            <th>{t('hockey.matches.columns.dateTime', 'Date & Time')}</th>
            <th>{t('hockey.matches.columns.venue', 'Venue')}</th>
            <th>{t('hockey.matches.columns.score', 'Score')}</th>
            <th>{t('hockey.matches.columns.status', 'Status')}</th>
            {!hideActions && (
              <th className="admin-table__actions-col">{t('common.actions', 'Actions')}</th>
            )}
          </tr>
        </thead>
        <tbody>
          {matches.map((match) => {
            const badge = getMatchStatusBadge(String(match.status));
            return (
              <tr
                key={match.id}
                className="admin-table__row--clickable"
                onClick={() => onLiveMatch(match)}
              >
                <td>
                  <div className="match-table__teams">
                    <span className="admin-table__name">
                      {match.homeTeamId ? teamNames.get(match.homeTeamId) ?? 'TBD' : 'TBD'}
                    </span>
                    <span className="match-table__vs">vs</span>
                    <span className="admin-table__name">
                      {match.awayTeamId ? teamNames.get(match.awayTeamId) ?? 'TBD' : 'TBD'}
                    </span>
                  </div>
                </td>
                <td>
                  <span className="admin-table__muted">
                    {match.competitionId ? competitionNames.get(match.competitionId) ?? '-' : '-'}
                  </span>
                </td>
                <td className="admin-table__muted">{formatHockeyDateTime(match.scheduledStartTime)}</td>
                <td>
                  {match.venue ? (
                    <span className="admin-table__muted">{match.venue}</span>
                  ) : (
                    <span className="admin-table__muted match-table__tbd">{t('hockey.matches.tbd', 'TBD')}</span>
                  )}
                </td>
                <td>
                  {match.status === 'Scheduled' || match.status === 'Postponed' ? (
                    <span className="admin-table__muted">-</span>
                  ) : (
                    <span className="admin-table__bold">{match.homeScore} - {match.awayScore}</span>
                  )}
                </td>
                <td>
                  <span className={badge.className}>{badge.label}</span>
                </td>
                {!hideActions && (
                  <td className="admin-table__actions-col" onClick={(event) => event.stopPropagation()}>
                    <ActionsDropdown
                      actions={getActions(match)}
                      ariaLabel={t('hockey.matches.actions.menu', 'Match actions menu')}
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

export default MatchTable;
