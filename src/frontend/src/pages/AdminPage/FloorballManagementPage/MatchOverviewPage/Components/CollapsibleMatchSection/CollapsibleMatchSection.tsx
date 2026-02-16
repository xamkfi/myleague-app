import { useTranslation } from 'react-i18next';
import type { FloorballMatchDto, FloorballMatchStatus } from '../../../../../../types/floorball/floorballTypes';
import { formatDateTime } from '../../../ManageMatchPage/utils/matchFormatters';
import ActionsDropdown from '../../../../../../components/ActionsDropdown/ActionsDropdown';
import '../../../../../../styles/AdminTable.scss';
import './CollapsibleMatchSection.scss';

interface CollapsibleMatchSectionProps {
  title: string;
  matches: FloorballMatchDto[];
  isCollapsed: boolean;
  onToggleCollapse: () => void;
  onLiveMatch: (match: FloorballMatchDto) => void;
  onEditMatch: (match: FloorballMatchDto) => void;
  onCancelMatch?: (match: FloorballMatchDto) => void;
  onReactivateMatch?: (match: FloorballMatchDto) => void;
  sectionType?: 'ongoing' | 'scheduled' | 'completed' | 'cancelled';
}

const CollapsibleMatchSection = ({
  title,
  matches,
  isCollapsed,
  onToggleCollapse,
  onLiveMatch,
  onEditMatch,
  onCancelMatch,
  onReactivateMatch,
  sectionType
}: CollapsibleMatchSectionProps) => {
  const { t } = useTranslation();

  if (matches.length === 0) {
    return null;
  }

  const getMatchStatusBadge = (status: FloorballMatchStatus) => {
    const map: Record<string, { className: string; label: string }> = {
      Scheduled: { className: 'admin-badge admin-badge--info', label: t('floorball.matches.status.scheduled', 'Scheduled') },
      InProgress: { className: 'admin-badge admin-badge--active', label: t('floorball.matches.status.inProgress', 'In Progress') },
      Completed: { className: 'admin-badge admin-badge--completed', label: t('floorball.matches.status.completed', 'Completed') },
      Cancelled: { className: 'admin-badge admin-badge--danger', label: t('floorball.matches.status.cancelled', 'Cancelled') },
      Postponed: { className: 'admin-badge admin-badge--warning', label: t('floorball.matches.status.postponed', 'Postponed') },
    };
    return map[status] ?? { className: 'admin-badge', label: status };
  };

  const getActions = (match: FloorballMatchDto) => {
    const actions: { label: string; onClick: () => void; variant?: 'default' | 'danger' | 'status'; disabled: boolean }[] = [];

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

    if (match.status === 'Cancelled' && onReactivateMatch) {
      actions.push({
        label: t('floorball.matches.actions.reactivate', 'Reactivate Match'),
        onClick: () => onReactivateMatch(match),
        disabled: false,
      });
    }

    if (match.status !== 'Cancelled' && match.status !== 'Completed' && onCancelMatch) {
      actions.push({
        label: t('floorball.matches.actions.cancel', 'Cancel Match'),
        onClick: () => onCancelMatch(match),
        variant: 'danger',
        disabled: false,
      });
    }

    return actions;
  };

  return (
    <div className={`collapsible-match-section ${sectionType ? `${sectionType}-section` : ''}`}>
      <div
        className="section-header"
        onClick={onToggleCollapse}
      >
        <div className="section-title">
          <span className="collapse-icon">
            <i className={`fas fa-chevron-${isCollapsed ? 'right' : 'down'}`}></i>
          </span>
          {title}
        </div>
      </div>

      {!isCollapsed && (
        <div className="section-content">
          <div className="admin-table__wrapper cms-table-wrapper">
            <table className="admin-table cms-table">
              <thead>
                <tr>
                  <th>{t('floorball.matches.columns.match', 'Match')}</th>
                  <th>{t('floorball.matches.columns.season', 'Season')}</th>
                  <th>{t('floorball.matches.columns.dateTime', 'Date & Time')}</th>
                  <th>{t('floorball.matches.columns.venue', 'Venue')}</th>
                  <th>{t('floorball.matches.columns.score', 'Score')}</th>
                  <th>{t('floorball.matches.columns.status', 'Status')}</th>
                  <th className="admin-table__actions-col">{t('common.actions', 'Actions')}</th>
                </tr>
              </thead>
              <tbody>
                {matches.map((match: FloorballMatchDto) => (
                  <tr
                    key={match.id}
                    className="admin-table__row--clickable"
                    onClick={() => match.status === 'InProgress' ? onLiveMatch(match) : onEditMatch(match)}
                  >
                    <td>
                      <div className="cms-match-teams">
                        <span className="admin-table__name">{match.homeTeamName}</span>
                        <span className="cms-vs">vs</span>
                        <span className="admin-table__name">{match.awayTeamName}</span>
                      </div>
                    </td>
                    <td>
                      <span className="admin-table__muted">{match.seasonName || '-'}</span>
                    </td>
                    <td className="admin-table__muted">
                      {formatDateTime(match.scheduledDateTime)}
                    </td>
                    <td>
                      {match.venue ? (
                        <span className="admin-table__muted">{match.venue}</span>
                      ) : (
                        <span className="admin-table__muted cms-tbd">
                          {t('floorball.matches.tbd', 'TBD')}
                        </span>
                      )}
                    </td>
                    <td className="cms-score-col">
                      {match.status === 'Scheduled' ? (
                        <span className="admin-table__muted">-</span>
                      ) : (
                        <span className="admin-table__bold">{match.homeScore} - {match.awayScore}</span>
                      )}
                    </td>
                    <td>
                      {(() => {
                        const badge = getMatchStatusBadge(match.status);
                        return <span className={badge.className}>{badge.label}</span>;
                      })()}
                    </td>
                    <td className="admin-table__actions-col" onClick={(e) => e.stopPropagation()}>
                      <ActionsDropdown
                        actions={getActions(match)}
                        ariaLabel={t('floorball.matches.actions.menu', 'Match actions menu')}
                      />
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
};

export default CollapsibleMatchSection;
