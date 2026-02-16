import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import type { FloorballTeam } from '../../../../../types/floorball/floorballTypes';
import type { DivisionType } from '../../../../../types/common/divisionType';
import { divisionService } from '../../../../../api/common/divisionService';
import TeamPlayersRow from './TeamPlayersRow';
import Pagination from '../../../../../components/Pagination';
import ActionsDropdown from '../../../../../components/ActionsDropdown/ActionsDropdown';
import BulkActionsBar from '../../../../../components/BulkActionsBar/BulkActionsBar';
import '../../../../../styles/AdminTable.scss';
import './TeamsTable.scss';

interface TeamsTableProps {
  teams: FloorballTeam[];
  loading: boolean;
  onEdit: (teamId: string) => void;
  onEditRoster: (teamId: string) => void;
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

const TeamsTable = ({
  teams,
  loading,
  onEdit,
  onEditRoster,
  onDelete,
  selectedIds,
  onToggleSelect,
  onSelectAll,
  onClearSelection,
  onBulkDelete,
  pagination,
  onPageChange,
  onPageSizeChange,
}: TeamsTableProps) => {
  const { t } = useTranslation();
  const [expandedTeams, setExpandedTeams] = useState<Set<string>>(new Set());
  const [closingTeams, setClosingTeams] = useState<Set<string>>(new Set());
  const [divisions, setDivisions] = useState<DivisionType[]>([]);

  const animationDuration = 350;

  // Toggle team expansion
  const toggleTeamExpansion = (teamId: string) => {
    if (closingTeams.has(teamId)) return;

    const isCurrentlyExpanded = expandedTeams.has(teamId);

    if (isCurrentlyExpanded) {
      setClosingTeams(prev => new Set(prev).add(teamId));
      setTimeout(() => {
        setExpandedTeams(prev => {
          const updated = new Set(prev);
          updated.delete(teamId);
          return updated;
        });
        setClosingTeams(prev => {
          const updated = new Set(prev);
          updated.delete(teamId);
          return updated;
        });
      }, animationDuration);
    } else {
      setExpandedTeams(prev => new Set(prev).add(teamId));
    }
  };

  useEffect(() => {
    const fetchDivisions = async () => {
      const tempDivisions = await divisionService.getAll();
      setDivisions(tempDivisions.data);
    };
    fetchDivisions();
  }, []);

  const totalColumns = 7; // checkbox + 5 data columns + actions

  return (
    <>
      {/* Bulk Actions Bar */}
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
          <div className="admin-table__empty">
            {t('common.loading', 'Loading...')}
          </div>
        ) : !teams || teams.length === 0 ? (
          <div className="admin-table__empty">
            {t('floorball.teams.noTeams', 'No teams found')}
          </div>
        ) : (
          <table className="admin-table">
            <thead>
              <tr>
                <th className="admin-table__checkbox-col">
                  <input
                    type="checkbox"
                    checked={teams.length > 0 && teams.every(team => selectedIds.has(team.id))}
                    onChange={(e) => {
                      if (e.target.checked) {
                        onSelectAll();
                      } else {
                        onClearSelection();
                      }
                    }}
                    title={t('floorball.teams.selectAll', 'Select all teams')}
                  />
                </th>
                <th>{t('floorball.teams.table.name', 'Team Name')}</th>
                <th>{t('floorball.teams.table.club', 'Club')}</th>
                <th>{t('floorball.teams.table.division', 'Division')}</th>
                <th>{t('floorball.teams.table.homeArena', 'Home Arena')}</th>
                <th>{t('floorball.teams.table.activeMembers', 'Active Members')}</th>
                <th className="admin-table__actions-col">{t('floorball.teams.table.actions', 'Actions')}</th>
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
                        onClick={(e) => e.stopPropagation()}
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
                          </div>
                          <div className="jersey-colors">
                            <span
                              className="color-indicator primary"
                              style={{ backgroundColor: team.primaryJerseyColor }}
                              title={`${t('floorball.teams.primary', 'Primary')}: ${team.primaryJerseyColor}`}
                            ></span>
                            {team.secondaryJerseyColor && (
                              <span
                                className="color-indicator secondary"
                                style={{ backgroundColor: team.secondaryJerseyColor }}
                                title={`${t('floorball.teams.secondary', 'Secondary')}: ${team.secondaryJerseyColor}`}
                              ></span>
                            )}
                          </div>
                        </div>
                      </div>
                    </td>
                    <td>{team.club.name}</td>
                    <td>{divisions.find(d => d.id == team.divisionId)?.name}</td>
                    <td>{team.homeArena}</td>
                    <td>
                      <span className={`admin-badge ${team.hasActiveMembers ? 'admin-badge--active' : 'admin-badge--inactive'}`}>
                        {team.hasActiveMembers
                          ? t('floorball.teams.hasMembers', 'Yes')
                          : t('floorball.teams.noMembers', 'No')
                        }
                      </span>
                    </td>
                    <td className="admin-table__actions-col" onClick={(e) => e.stopPropagation()}>
                      <ActionsDropdown
                        actions={[
                          {
                            label: t('floorball.teams.editTeamInfo', 'Edit Team Information'),
                            onClick: () => onEdit(team.id),
                          },
                          {
                            label: t('floorball.teams.editRoster', 'Edit Roster'),
                            onClick: () => onEditRoster(team.id),
                          },
                          {
                            label: t('common.delete', 'Delete'),
                            onClick: () => onDelete(team.id, team.name),
                            variant: 'danger',
                          },
                        ]}
                        ariaLabel={t('floorball.teams.actions.menu', 'Team actions menu')}
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
              onPageChange={(p) => onPageChange ? onPageChange(p) : undefined}
              onPageSizeChange={(s) => onPageSizeChange ? onPageSizeChange(s) : undefined}
              className="no-margin"
            />
          </div>
        )}
      </div>
    </>
  );
};

export default TeamsTable;
