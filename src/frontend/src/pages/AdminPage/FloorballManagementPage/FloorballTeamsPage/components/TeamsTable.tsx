import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import type { FloorballTeam, FloorballTeamRequest } from '../../../../../types/floorball/floorballTypes';
import TeamPlayersRow from './TeamPlayersRow';
import EditTeamModal from './EditTeamModal';
import React from 'react';

interface TeamsTableProps {
  teams: FloorballTeam[];
  onEdit: (teamData: FloorballTeamRequest, teamId: string) => Promise<void>;
  onDelete: (teamId: string, teamName: string) => void;
}

const TeamsTable = ({ teams, onEdit, onDelete }: TeamsTableProps) => {
  const { t } = useTranslation();
  const [expandedTeams, setExpandedTeams] = useState<Set<string>>(new Set());
  const [editModalOpen, setEditModalOpen] = useState(false);
  const [editingTeamId, setEditingTeamId] = useState<string | null>(null);

  // Format division for display
  const formatDivision = (division: string) => {
    return t(`floorball.divisions.${division.toLowerCase()}`, division);
  };

  // Toggle team expansion
  const toggleTeamExpansion = (teamId: string) => {
    const newExpandedTeams = new Set(expandedTeams);
    if (newExpandedTeams.has(teamId)) {
      newExpandedTeams.delete(teamId);
    } else {
      newExpandedTeams.add(teamId);
    }
    setExpandedTeams(newExpandedTeams);
  };

  // Handle edit team
  const handleEditTeam = (teamId: string) => {
    setEditingTeamId(teamId);
    setEditModalOpen(true);
  };

  // Handle edit modal submit
  const handleEditSubmit = async (teamData: FloorballTeamRequest) => {
    if (editingTeamId) {
      await onEdit(teamData, editingTeamId);
      setEditModalOpen(false);
      setEditingTeamId(null);
    }
  };

  // Handle edit modal close
  const handleEditClose = () => {
    setEditModalOpen(false);
    setEditingTeamId(null);
  };

  return (
    <>
      <div className="teams-table-container">
        <table className="teams-table">
          <thead>
            <tr>
              <th>{t('floorball.teams.table.name', 'Team Name')}</th>
              <th>{t('floorball.teams.table.club', 'Club')}</th>
              <th>{t('floorball.teams.table.division', 'Division')}</th>
              <th>{t('floorball.teams.table.homeArena', 'Home Arena')}</th>
              <th>{t('floorball.teams.table.activeMembers', 'Active Members')}</th>
              <th>{t('floorball.teams.table.actions', 'Actions')}</th>
            </tr>
          </thead>
          <tbody>
            {!teams || teams.length === 0 ? (
              <tr>
                <td colSpan={6} className="no-teams">
                  {t('floorball.teams.noTeams', 'No teams found')}
                </td>
              </tr>
            ) : (
              teams.map((team) => (
                <React.Fragment key={team.id}>
                  <tr 
                    className={`team-row ${expandedTeams.has(team.id) ? 'expanded' : ''}`}
                    onClick={() => toggleTeamExpansion(team.id)}
                  >
                    <td className="team-name">
                      <div className="team-info">
                        <div className="team-name-container">
                          <span className="expand-icon">
                            {expandedTeams.has(team.id) ? '▼' : '▶'}
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
                    </td>
                    <td>{team.club.name}</td>
                    <td>{formatDivision(team.division)}</td>
                    <td>{team.homeArena}</td>
                    <td>
                      <span className={`active-status ${team.hasActiveMembers ? 'has-members' : 'no-members'}`}>
                        {team.hasActiveMembers 
                          ? t('floorball.teams.hasMembers', 'Yes') 
                          : t('floorball.teams.noMembers', 'No')
                        }
                      </span>
                    </td>
                    <td className="actions" onClick={(e) => e.stopPropagation()}>
                      <button
                        className="edit-button"
                        onClick={() => handleEditTeam(team.id)}
                        title={t('common.edit', 'Edit')}
                      >
                        ✏️
                      </button>
                      <button
                        className="delete-button"
                        onClick={() => onDelete(team.id, team.name)}
                        title={t('common.delete', 'Delete')}
                      >
                        🗑️
                      </button>
                    </td>
                  </tr>
                  <TeamPlayersRow 
                    teamId={team.id}
                    isExpanded={expandedTeams.has(team.id)}
                    team={team}
                  />
                </React.Fragment>
              ))
            )}
          </tbody>
        </table>
      </div>

      <EditTeamModal
        isOpen={editModalOpen}
        onClose={handleEditClose}
        onSubmit={handleEditSubmit}
        teamId={editingTeamId}
      />
    </>
  );
};

export default TeamsTable; 