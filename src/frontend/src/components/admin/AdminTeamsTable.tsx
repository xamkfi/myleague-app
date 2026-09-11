import { Fragment, type ReactNode, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import Pagination from '../Pagination';
import ActionsDropdown from '../ActionsDropdown/ActionsDropdown';
import BulkActionsBar from '../BulkActionsBar/BulkActionsBar';
import TeamCategoryBadge from '../TeamCategoryBadge/TeamCategoryBadge';
import TeamLink from '../SportLinks/TeamLink';
import { createTeamSlug } from '../../utils/slugUtils';
import { getTeamPath, type SportKind } from '../../utils/sportRoutes';
import type { AdminAction, AdminTablePagination, AdminTeamRow, AdminTeamTableLabels } from './adminTableTypes';
import '../../styles/AdminTable.scss';
import './AdminTeamsTable.scss';

interface AdminTeamsTableProps {
  sport: SportKind;
  teams: AdminTeamRow[];
  labels: AdminTeamTableLabels;
  loading: boolean;
  selectedIds: Set<string>;
  onToggleSelect: (id: string) => void;
  onSelectAll: () => void;
  onClearSelection: () => void;
  onBulkDelete: () => void;
  onEdit: (teamId: string) => void;
  onEditRoster: (teamId: string) => void;
  onDelete: (teamId: string, teamName: string) => void;
  extraActions?: (team: AdminTeamRow) => AdminAction[];
  renderExpandedRow: (team: AdminTeamRow, isExpanded: boolean, isClosing: boolean) => ReactNode;
  pagination?: AdminTablePagination;
  onPageChange?: (page: number) => void;
  onPageSizeChange?: (pageSize: number) => void;
}

const ANIMATION_DURATION_MS = 350;
const TOTAL_COLUMNS = 7;

export default function AdminTeamsTable({
  sport,
  teams,
  labels,
  loading,
  selectedIds,
  onToggleSelect,
  onSelectAll,
  onClearSelection,
  onBulkDelete,
  onEdit,
  onEditRoster,
  onDelete,
  extraActions,
  renderExpandedRow,
  pagination,
  onPageChange,
  onPageSizeChange,
}: AdminTeamsTableProps) {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [expandedTeams, setExpandedTeams] = useState<Set<string>>(new Set());
  const [closingTeams, setClosingTeams] = useState<Set<string>>(new Set());

  const toggleTeamExpansion = (teamId: string): void => {
    if (closingTeams.has(teamId)) {
      return;
    }

    if (expandedTeams.has(teamId)) {
      setClosingTeams((prev) => new Set(prev).add(teamId));
      window.setTimeout(() => {
        setExpandedTeams((prev) => {
          const updated = new Set(prev);
          updated.delete(teamId);
          return updated;
        });
        setClosingTeams((prev) => {
          const updated = new Set(prev);
          updated.delete(teamId);
          return updated;
        });
      }, ANIMATION_DURATION_MS);
      return;
    }

    setExpandedTeams((prev) => new Set(prev).add(teamId));
  };

  const slugById = new Map(
    teams.map((team) => [team.id, createTeamSlug({ id: team.id, name: team.name }, teams)]),
  );

  return (
    <>
      <BulkActionsBar
        selectedCount={selectedIds.size}
        totalCount={teams.length}
        onSelectAll={onSelectAll}
        onClearSelection={onClearSelection}
        actions={[
          {
            label: t('common.bulk.delete', 'Delete ({{count}})', { count: selectedIds.size }),
            onClick: onBulkDelete,
            variant: 'danger',
          },
        ]}
      />

      <div className="admin-table__wrapper">
        {loading ? (
          <div className="admin-table__empty">{t('common.loading')}</div>
        ) : teams.length === 0 ? (
          <div className="admin-table__empty">{labels.noTeams}</div>
        ) : (
          <table className="admin-table">
            <thead>
              <tr>
                <th className="admin-table__checkbox-col">
                  <input
                    type="checkbox"
                    checked={teams.length > 0 && teams.every((team) => selectedIds.has(team.id))}
                    onChange={(event) => {
                      if (event.target.checked) {
                        onSelectAll();
                      } else {
                        onClearSelection();
                      }
                    }}
                    title={labels.selectAll}
                  />
                </th>
                <th>{labels.teamName}</th>
                <th>{labels.club}</th>
                <th>{labels.division}</th>
                <th>{labels.homeArena}</th>
                <th>{labels.activeMembers}</th>
                <th className="admin-table__actions-col">{labels.actions}</th>
              </tr>
            </thead>
            <tbody>
              {teams.map((team) => {
                const publicPath = getTeamPath(sport, slugById.get(team.id) ?? createTeamSlug(team));
                return (
                  <Fragment key={team.id}>
                    <tr
                      className={`admin-table__row--clickable${selectedIds.has(team.id) ? ' admin-table__row--selected' : ''}${expandedTeams.has(team.id) ? ' admin-table__row--expanded' : ''}`}
                      onClick={() => toggleTeamExpansion(team.id)}
                    >
                      <td className="admin-table__checkbox-col">
                        <input
                          type="checkbox"
                          checked={selectedIds.has(team.id)}
                          onChange={() => onToggleSelect(team.id)}
                          onClick={(event) => event.stopPropagation()}
                        />
                      </td>
                      <td>
                        <div className="team-name-cell">
                          <div className="team-info">
                            <div className="team-name-container">
                              <span className="expand-icon">
                                {expandedTeams.has(team.id) && !closingTeams.has(team.id) ? '▼' : '▶'}
                              </span>
                              <TeamLink
                                sport={sport}
                                teamName={team.name}
                                teamId={team.id}
                                teams={teams}
                                className="admin-table__name"
                              />
                              <TeamCategoryBadge category={team.teamCategory} />
                            </div>
                            <div className="jersey-colors">
                              <span
                                className="color-indicator primary"
                                style={{ backgroundColor: team.primaryJerseyColor }}
                                title={`${labels.primary}: ${team.primaryJerseyColor}`}
                              />
                              {team.secondaryJerseyColor && (
                                <span
                                  className="color-indicator secondary"
                                  style={{ backgroundColor: team.secondaryJerseyColor }}
                                  title={`${labels.secondary}: ${team.secondaryJerseyColor}`}
                                />
                              )}
                            </div>
                          </div>
                        </div>
                      </td>
                      <td>{team.clubName}</td>
                      <td>{team.divisionName}</td>
                      <td>{team.homeArena}</td>
                      <td>
                        <span className={`admin-badge ${team.hasActiveMembers ? 'admin-badge--active' : 'admin-badge--inactive'}`}>
                          {team.hasActiveMembers ? labels.hasMembers : labels.noMembers}
                        </span>
                      </td>
                      <td className="admin-table__actions-col" onClick={(event) => event.stopPropagation()}>
                        <ActionsDropdown
                          actions={[
                            {
                              label: labels.editTeamInfo,
                              onClick: () => onEdit(team.id),
                            },
                            {
                              label: labels.editRoster,
                              onClick: () => onEditRoster(team.id),
                            },
                            {
                              label: t('common.viewPublic'),
                              onClick: () => navigate(publicPath),
                            },
                            ...(extraActions ? extraActions(team) : []),
                            {
                              label: labels.delete,
                              onClick: () => onDelete(team.id, team.name),
                              variant: 'danger',
                            },
                          ]}
                          ariaLabel={labels.actionsMenu}
                        />
                      </td>
                    </tr>
                    {expandedTeams.has(team.id) && (
                      <tr className="teams-table__expanded-row">
                        <td colSpan={TOTAL_COLUMNS} className="teams-table__expanded-cell">
                          {renderExpandedRow(team, !closingTeams.has(team.id), closingTeams.has(team.id))}
                        </td>
                      </tr>
                    )}
                  </Fragment>
                );
              })}
            </tbody>
          </table>
        )}

        {pagination && (
          <div className="teams-pagination sticky-bottom">
            <Pagination
              currentPage={pagination.currentPage}
              totalPages={pagination.totalPages}
              totalCount={pagination.totalCount}
              pageSize={pagination.pageSize}
              onPageChange={(page) => onPageChange?.(page)}
              onPageSizeChange={(size) => onPageSizeChange?.(size)}
              className="no-margin"
            />
          </div>
        )}
      </div>
    </>
  );
}
