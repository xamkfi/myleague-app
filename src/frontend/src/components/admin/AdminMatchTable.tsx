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

  const teamName = (teamId: string | null | undefined, name: string | undefined) => {
    if (teamId && name) {
      return (
        <TeamLink sport={sport} teamName={name} teamId={teamId} className="match-card__team-link" />
      );
    }
    return <span className={name ? undefined : 'match-table__tbd'}>{name || labels.tbd}</span>;
  };

  return (
    <ul className="match-cards">
      {matches.map((match) => {
        const badge = getStatusBadge(match.status);
        const hideScore = match.status === 'Scheduled' || match.status === 'Postponed';

        return (
          <li key={match.id}>
            <article
              className="match-card"
              onClick={() => onRowClick(match)}
            >
              <div className="match-card__teams">
                {teamName(match.homeTeamId, match.homeTeamName)}
                <span className="match-table__vs">vs</span>
                {teamName(match.awayTeamId, match.awayTeamName)}
              </div>
              <div className="match-card__field match-card__field--season">
                <span className="match-card__label">{labels.season}</span>
                <span className="match-card__value">{match.competitionName || '-'}</span>
              </div>
              <div className="match-card__field match-card__field--date">
                <span className="match-card__label">{labels.dateTime}</span>
                <span className="match-card__value">{formatDateTime(match.scheduledDateTime)}</span>
              </div>
              <div className="match-card__field match-card__field--venue">
                <span className="match-card__label">{labels.venue}</span>
                <span className={`match-card__value${match.venue ? '' : ' match-table__tbd'}`}>
                  {match.venue || labels.tbd}
                </span>
              </div>
              <div className="match-card__field match-card__field--score">
                <span className="match-card__label">{labels.score}</span>
                <span className={`match-card__value${hideScore ? '' : ' match-card__value--score'}`}>
                  {hideScore ? '-' : `${match.homeScore} - ${match.awayScore}`}
                </span>
              </div>
              <div className="match-card__status">
                <span className={badge.className}>{badge.label}</span>
              </div>
              {!hideActions && (
                <div
                  className="match-card__actions"
                  onClick={(event) => event.stopPropagation()}
                  onKeyDown={(event) => event.stopPropagation()}
                >
                  <ActionsDropdown
                    actions={getActions(match)}
                    ariaLabel={labels.actionsMenu}
                  />
                </div>
              )}
            </article>
          </li>
        );
      })}
    </ul>
  );
}
