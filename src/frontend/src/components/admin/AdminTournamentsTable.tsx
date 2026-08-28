import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import ActionsDropdown from '../ActionsDropdown/ActionsDropdown';
import LiveDot from '../LiveDot/LiveDot';
import TeamCategoryBadge from '../TeamCategoryBadge/TeamCategoryBadge';
import { getTournamentPath, type SportKind } from '../../utils/sportRoutes';
import AdminNameLink from './AdminNameLink';
import type { AdminTournamentRow, AdminTournamentTableLabels } from './adminTableTypes';
import '../../styles/AdminTable.scss';

interface AdminTournamentsTableProps {
  sport: SportKind;
  tournaments: AdminTournamentRow[];
  labels: AdminTournamentTableLabels;
  liveCounts: Map<string, number>;
  onEdit: (tournamentId: string) => void;
  showMatchCount?: boolean;
  formatDate?: (value: string) => string;
}

function defaultFormatDate(dateString: string): string {
  try {
    return new Date(dateString).toLocaleDateString();
  } catch {
    return dateString;
  }
}

export default function AdminTournamentsTable({
  sport,
  tournaments,
  labels,
  liveCounts,
  onEdit,
  showMatchCount = true,
  formatDate = defaultFormatDate,
}: AdminTournamentsTableProps) {
  const { t } = useTranslation();
  const navigate = useNavigate();

  return (
    <table className="admin-table">
      <thead>
        <tr>
          <th>{labels.name}</th>
          <th>{labels.groups}</th>
          <th>{labels.startDate}</th>
          <th>{labels.endDate}</th>
          <th>{labels.teams}</th>
          {showMatchCount && <th>{labels.matches}</th>}
          <th>{labels.status}</th>
          <th className="admin-table__actions-col">{t('common.actions')}</th>
        </tr>
      </thead>
      <tbody>
        {tournaments.map((tournament) => {
          const liveCount = liveCounts.get(tournament.id) ?? 0;
          const publicPath = getTournamentPath(sport, tournament.id);

          return (
            <tr
              key={tournament.id}
              className="admin-table__row--clickable"
              onClick={() => onEdit(tournament.id)}
              role="button"
              tabIndex={0}
              title={labels.openEdit}
              onKeyDown={(event) => {
                if (event.key === 'Enter' || event.key === ' ') {
                  event.preventDefault();
                  onEdit(tournament.id);
                }
              }}
            >
              <td className="admin-table__name">
                <span className="admin-table__name-inner">
                  {liveCount > 0 && (
                    <LiveDot
                      tone="light"
                      count={liveCount}
                      ariaLabel={labels.matchesInProgress(liveCount)}
                    />
                  )}
                  <AdminNameLink to={publicPath}>{tournament.name}</AdminNameLink>
                  <TeamCategoryBadge category={tournament.teamCategory} />
                </span>
              </td>
              <td>
                <div style={{ display: 'flex', flexWrap: 'wrap', gap: '0.25rem' }}>
                  {tournament.groups.length > 0 ? (
                    tournament.groups.map((group) => (
                      <span key={group.id} className="admin-tag admin-tag--blue">
                        {group.name}
                      </span>
                    ))
                  ) : (
                    <span className="admin-table__muted">{labels.noGroups}</span>
                  )}
                </div>
              </td>
              <td>{formatDate(tournament.startDate)}</td>
              <td>{formatDate(tournament.endDate)}</td>
              <td>
                <span className="admin-table__muted">
                  {labels.teamsCount(tournament.teamCount)}
                </span>
              </td>
              {showMatchCount && (
                <td>
                  <span className="admin-table__muted">
                    {labels.matchesCount(tournament.matchCount ?? 0)}
                  </span>
                </td>
              )}
              <td>
                <span className={`admin-badge ${tournament.statusClassName}`}>
                  {tournament.statusLabel}
                </span>
              </td>
              <td
                className="admin-table__actions-col"
                onClick={(event) => event.stopPropagation()}
                onKeyDown={(event) => event.stopPropagation()}
              >
                <ActionsDropdown
                  actions={[
                    {
                      label: t('common.viewPublic'),
                      onClick: () => navigate(publicPath),
                    },
                  ]}
                  ariaLabel={labels.actionsMenu}
                />
              </td>
            </tr>
          );
        })}
      </tbody>
    </table>
  );
}
