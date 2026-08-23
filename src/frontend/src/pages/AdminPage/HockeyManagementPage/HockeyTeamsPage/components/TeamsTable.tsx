import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import type { HockeyTeamDto } from '../../../../../types/hockey/hockeyTypes';
import type { DivisionType } from '../../../../../types/common/divisionType';
import { divisionService } from '../../../../../api/common/divisionService';
import TeamPlayersRow from './TeamPlayersRow';
import Pagination from '../../../../../components/Pagination';
import ActionsDropdown from '../../../../../components/ActionsDropdown/ActionsDropdown';
import BulkActionsBar from '../../../../../components/BulkActionsBar/BulkActionsBar';
import TeamCategoryBadge from '../../../../../components/TeamCategoryBadge/TeamCategoryBadge';
import '../../../../../styles/AdminTable.scss';
import './TeamsTable.scss';

interface TeamsTableProps {
  teams: HockeyTeamDto[];
  clubNames: Map<string, string>;
  loading: boolean;
  onEdit: (teamId: string) => void;
  onEditRoster: (teamId: string) => void;
  onEditLines: (teamId: string) => void;
  onDelete: (teamId: string, teamName: string) => void;
  selectedIds: Set<string>;
  onToggleSelect: (id: string) => void;
  onSelectAll: () => void;
  onClearSelection: () => void;
  onBulkDelete: () => void;
  pagination?: {
    currentPage: number;
    totalPages: number;
    totalCount: number;
    pageSize: number;
  };
  onPageChange?: (page: number) => void;
  onPageSizeChange?: (pageSize: number) => void;
}

function TeamsTable({
  teams,
  clubNames,
  loading,
  onEdit,
  onEditRoster,
  onEditLines,
  onDelete,
  selectedIds,
  onToggleSelect,
  onSelectAll,
  onClearSelection,
  onBulkDelete,
  pagination,
  onPageChange,
  onPageSizeChange,
}: TeamsTableProps) {
  const { t } = useTranslation();
  const [expandedTeams, setExpandedTeams] = useState<Set<string>>(new Set());
  const [closingTeams, setClosingTeams] = useState<Set<string>>(new Set());
  const [divisions, setDivisions] = useState<DivisionType[]>([]);
  const animationDuration = 350;

  const toggleTeamExpansion = (teamId: string): void => {
    if (closingTeams.has(teamId)) {
      return;
    }
    const isCurrentlyExpanded = expandedTeams.has(teamId);
    if (isCurrentlyExpanded) {
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
      }, animationDuration);
    } else {
      setExpandedTeams((prev) => new Set(prev).add(teamId));
    }
  };

  useEffect(() => {
    const fetchDivisions = async (): Promise<void> => {
      const response = await divisionService.getAll();
      setDivisions(response.data);
    };
    void fetchDivisions();
  }, []);

  const totalColumns = 7;

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
          <div className="admin-table__empty">{t('common.loading', 'Loading...')}</div>
        ) : !teams || teams.length === 0 ? (
          <div className="admin-table__empty">{t('hockey.teams.noTeams', 'No teams found')}</div>
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
                    title={t('hockey.teams.selectAll', 'Select all teams')}
                  />
                </th>
                <th>{t('hockey.teams.table.name', 'Team Name')}</th>
                <th>{t('hockey.teams.table.club', 'Club')}</th>
                <th>{t('hockey.teams.table.division', 'Division')}</th>
                <th>{t('hockey.teams.table.homeArena', 'Home Arena')}</th>
                <th>{t('hockey.teams.table.activeMembers', 'Active Members')}</th>
                <th className="admin-table__actions-col">{t('hockey.teams.table.actions', 'Actions')}</th>
              </tr>
            </thead>
            <tbody>
              {teams.map((team) => (
                <>
                  <tr
                    key={team.id}
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
                            <span className="admin-table__name">{team.name}</span>
                            <TeamCategoryBadge category={team.teamCategory} />
                          </div>
                          <div className="jersey-colors">
                            <span
                              className="color-indicator primary"
                              style={{ backgroundColor: team.primaryJerseyColor }}
                              title={`${t('hockey.teams.primary', 'Primary')}: ${team.primaryJerseyColor}`}
                            />
                            {team.secondaryJerseyColor && (
                              <span
                                className="color-indicator secondary"
                                style={{ backgroundColor: team.secondaryJerseyColor }}
                                title={`${t('hockey.teams.secondary', 'Secondary')}: ${team.secondaryJerseyColor}`}
                              />
                            )}
                          </div>
                        </div>
                      </div>
                    </td>
                    <td>{clubNames.get(team.clubId) ?? '—'}</td>
                    <td>{divisions.find((division) => division.id === team.divisionId)?.name ?? '—'}</td>
                    <td>{team.homeArena}</td>
                    <td>
                      <span className={`admin-badge ${team.roster.some((row) => row.isActive) ? 'admin-badge--active' : 'admin-badge--inactive'}`}>
                        {team.roster.some((row) => row.isActive)
                          ? t('hockey.teams.hasMembers', 'Yes')
                          : t('hockey.teams.noMembers', 'No')}
                      </span>
                    </td>
                    <td className="admin-table__actions-col" onClick={(event) => event.stopPropagation()}>
                      <ActionsDropdown
                        actions={[
                          {
                            label: t('hockey.teams.editTeamInfo', 'Edit Team Information'),
                            onClick: () => onEdit(team.id),
                          },
                          {
                            label: t('hockey.teams.editRoster', 'Edit Roster'),
                            onClick: () => onEditRoster(team.id),
                          },
                          {
                            label: t('hockey.teams.lines', 'Lines'),
                            onClick: () => onEditLines(team.id),
                          },
                          {
                            label: t('common.deactivate', 'Deactivate'),
                            onClick: () => onDelete(team.id, team.name),
                            variant: 'danger',
                          },
                        ]}
                        ariaLabel={t('hockey.teams.actions.menu', 'Team actions menu')}
                      />
                    </td>
                  </tr>
                  {expandedTeams.has(team.id) && (
                    <tr key={`${team.id}-expanded`} className="teams-table__expanded-row">
                      <td colSpan={totalColumns} className="teams-table__expanded-cell">
                        <TeamPlayersRow
                          teamId={team.id}
                          isExpanded={!closingTeams.has(team.id)}
                          isClosing={closingTeams.has(team.id)}
                          team={team}
                        />
                      </td>
                    </tr>
                  )}
                </>
              ))}
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

export default TeamsTable;
