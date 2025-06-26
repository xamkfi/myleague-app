import React from 'react';
import type { FloorballMatchDto } from '../../../../../../types/floorball/floorballTypes';
import './MatchStatsCards.scss';

interface MatchStatsCardsProps {
  allMatches: FloorballMatchDto[];
  filteredMatches: FloorballMatchDto[];
  selectedSeasonId: string;
  onCreateNew?: () => void;
}

const MatchStatsCards: React.FC<MatchStatsCardsProps> = ({
  allMatches,
  filteredMatches,
  selectedSeasonId,
  onCreateNew
}) => {
  const getMatchCountByStatus = (status: string) => {
    return allMatches.filter(m => m.status === status).length;
  };

  return (
    <div className="stats-grid">
      <div className="stat-card">
        <div className="stat-number">{filteredMatches.length}</div>
        <div className="stat-label">{selectedSeasonId ? 'Season Matches' : 'Total Matches'}</div>
      </div>
      <div className="stat-card">
        <div className="stat-number">{getMatchCountByStatus('Completed')}</div>
        <div className="stat-label">Completed</div>
        <div className="stat-indicator completed"></div>
      </div>
      <div className="stat-card">
        <div className="stat-number">{getMatchCountByStatus('Scheduled')}</div>
        <div className="stat-label">Scheduled</div>
        <div className="stat-indicator scheduled"></div>
      </div>
      <div className="stat-card">
        <div className="stat-number">{getMatchCountByStatus('InProgress')}</div>
        <div className="stat-label">In Progress</div>
        <div className="stat-indicator progress"></div>
      </div>
      <div className="stat-card stat-card--create" onClick={onCreateNew}>
        <div className="stat-number stat-number--plus">+</div>
        <div className="stat-label">Create New</div>
      </div>
    </div>
  );
};

export default MatchStatsCards; 