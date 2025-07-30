import React from 'react';
import type { FloorballMatchDto } from '../../../../../../types/floorball/floorballTypes';

interface LiveMatchQuickActionsProps {
  loading: boolean;
  currentMatch: FloorballMatchDto;
  onShowGoalForm: () => void;
  onShowPenaltyForm: () => void;
}

const LiveMatchQuickActions: React.FC<LiveMatchQuickActionsProps> = ({
  loading,
  currentMatch,
  onShowGoalForm,
  onShowPenaltyForm
}) => {
  return (
    <div className="quick-actions">
      <button 
        onClick={onShowGoalForm} 
        className="action-btn goal-btn"
        disabled={loading || currentMatch.status !== 'InProgress'}
      >
        ⚽ Record Goal
      </button>
      <button 
        onClick={onShowPenaltyForm} 
        className="action-btn penalty-btn"
        disabled={loading || currentMatch.status !== 'InProgress'}
      >
        🟨 Record Penalty
      </button>
    </div>
  );
};

export default LiveMatchQuickActions; 