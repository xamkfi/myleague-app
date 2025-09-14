import { useState, useEffect, useRef } from 'react';
import { useTranslation } from 'react-i18next';
import type { FloorballTeam } from '../../../../../types/floorball/floorballTypes';
import TeamPlayersRow from './TeamPlayersRow';
import React from 'react';
import type { DivisionType } from '../../../../../types/common/divisionType';
import { divisionService } from '../../../../../api/common/divisionService';
import Pagination from '../../../../../components/Pagination';
import './TeamsTable.scss';

interface TeamsTableProps {
  teams: FloorballTeam[];
  loading: boolean;
  onEdit: (teamId: string) => void;
  onDelete: (teamId: string, teamName: string) => void;
  pagination?: {
    currentPage: number;
    totalPages: number;
    totalCount: number;
    pageSize: number;
  };
  onPageChange?: (page: number) => void;
  onPageSizeChange?: (pageSize: number) => void;
}

const TeamsTable = ({ teams, loading, onEdit, onDelete, pagination, onPageChange, onPageSizeChange }: TeamsTableProps) => {
  const { t } = useTranslation();
  const [expandedTeams, setExpandedTeams] = useState<Set<string>>(new Set());
  const [closingTeams, setClosingTeams] = useState<Set<string>>(new Set());
  const [divisions, setDivisions] = useState<DivisionType[]>([])
  const [dropdownOpen, setDropdownOpen] = useState<string | null>(null);
  const [activeDropdown, setActiveDropdown] = useState<string | null>(null);
  const [closingDropdown, setClosingDropdown] = useState<string | null>(null);
  const closeTimer = useRef<number | null>(null);

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

  // Handle edit team
  const handleEditTeam = (teamId: string) => {
    onEdit(teamId);
  };

  const cancelCloseTimer = () => {
    if (closeTimer.current) {
      clearTimeout(closeTimer.current);
      closeTimer.current = null;
    }
  };

  const startCloseTimer = () => {
    cancelCloseTimer();
    closeTimer.current = setTimeout(() => {
      if (dropdownOpen) {
        setClosingDropdown(dropdownOpen);
        setDropdownOpen(null);
        setTimeout(() => setClosingDropdown(null), 300); // Animation duration
      }
    }, 500);
  };

  // Handle dropdown toggle
  const toggleDropdown = (teamId: string) => {
    cancelCloseTimer();
    if (dropdownOpen === teamId) {
      startCloseTimer();
    } else {
      setDropdownOpen(teamId);
    }
  };

  // Close dropdown when clicking outside
  useEffect(() => {
    return () => {
      cancelCloseTimer();
    };
  }, []);

  useEffect(() => {
    if (dropdownOpen) {
      const timer = setTimeout(() => {
        setActiveDropdown(dropdownOpen);
      }, 10);
      return () => clearTimeout(timer);
    } else {
      setActiveDropdown(null);
    }
  }, [dropdownOpen]);
  
  useEffect(() => {
    const fetchDivisions = async () => {
      const tempDivisions = await divisionService.getAll();
      setDivisions(tempDivisions.data);
    };
    fetchDivisions();
  }, []);

  return (
    <>
      <div className={`teams-table-container ${dropdownOpen || closingDropdown ? 'dropdown-active' : ''}`}>
        <div className={`teams-grid ${dropdownOpen || closingDropdown ? 'dropdown-active' : ''}`}>
          {/* Header Row */}
          <div className="teams-header">
            {t('floorball.teams.table.name', 'Team Name')}
          </div>
          <div className="teams-header">
            {t('floorball.teams.table.club', 'Club')}
          </div>
          <div className="teams-header">
            {t('floorball.teams.table.division', 'Division')}
          </div>
          <div className="teams-header">
            {t('floorball.teams.table.homeArena', 'Home Arena')}
          </div>
          <div className="teams-header">
            {t('floorball.teams.table.activeMembers', 'Active Members')}
          </div>
          <div className="teams-header">
            {t('floorball.teams.table.actions', 'Actions')}
          </div>

          {/* Teams Data */}
          {loading ? (
            <div className="teams-loading">
              {t('common.loading', 'Loading...')}
            </div>
          ) : !teams || teams.length === 0 ? (
            <div className="no-teams">
              {t('floorball.teams.noTeams', 'No teams found')}
            </div>
          ) : (
            teams.map((team) => (
              <React.Fragment key={team.id}>
                <div 
                  className={`team-row ${expandedTeams.has(team.id) ? 'expanded' : ''} ${dropdownOpen === team.id ? 'dropdown-open' : ''}`}
                  onClick={() => toggleTeamExpansion(team.id)}
                >
                  <div className="team-cell team-name-cell" data-label="Team Name">
                    <div className="team-info">
                      <div className="team-name-container">
                        <span className="expand-icon">
                          {expandedTeams.has(team.id) && !closingTeams.has(team.id) ? '▼' : '▶'}
                        </span>
                        <span className="name">{team.name}</span>
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
                  <div className="team-cell club-cell" data-label="Club">
                    {team.club.name}
                  </div>
                  <div className="team-cell division-cell" data-label="Division">
                    {divisions.find(d => d.id == team.divisionId)?.name}
                  </div>
                  <div className="team-cell home-arena-cell" data-label="Home Arena">
                    {team.homeArena}
                  </div>
                  <div className="team-cell active-members-cell" data-label="Active Members">
                    <span className={`active-status ${team.hasActiveMembers ? 'has-members' : 'no-members'}`}>
                      {team.hasActiveMembers 
                        ? t('floorball.teams.hasMembers', 'Yes') 
                        : t('floorball.teams.noMembers', 'No')
                      }
                    </span>
                  </div>
                  <div className="team-cell actions-cell" data-label="Actions" onClick={(e) => e.stopPropagation()}>
                    <div 
                      className="actions-dropdown"
                      onMouseLeave={startCloseTimer}
                      onMouseEnter={cancelCloseTimer}
                    >
                      <button
                        className="actions-button"
                        onClick={(e) => {
                          e.stopPropagation();
                          toggleDropdown(team.id);
                        }}
                        title={t('common.actions', 'Actions')}
                      >
                        ⋮
                      </button>
                      {(dropdownOpen === team.id || closingDropdown === team.id) && (
                        <div className={`dropdown-menu ${activeDropdown === team.id ? 'open' : ''}`}>
                          <button
                            className="dropdown-item edit-item"
                            onClick={(e) => {
                              e.stopPropagation();
                              handleEditTeam(team.id);
                              setDropdownOpen(null);
                            }}
                          >
                            ✏️ {t('common.edit', 'Edit')}
                          </button>
                          <button
                            className="dropdown-item delete-item"
                            onClick={(e) => {
                              e.stopPropagation();
                              onDelete(team.id, team.name);
                              setDropdownOpen(null);
                            }}
                          >
                            🗑️ {t('common.delete', 'Delete')}
                          </button>
                        </div>
                      )}
                    </div>
                  </div>
                </div>
                {expandedTeams.has(team.id) && (
                  <TeamPlayersRow 
                    teamId={team.id}
                    isExpanded={!closingTeams.has(team.id)}
                    isClosing={closingTeams.has(team.id)}
                    team={team}
                  />
                )}
              </React.Fragment>
            ))
          )}
        </div>

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