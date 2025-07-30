import React from 'react';
import type { FloorballMatchDto, FloorballTeam } from '../../../../../../../types/floorball/floorballTypes';
import type { FloorballPlayerDto } from '../../../../../../../api/floorball/floorballPlayerService';
import type { GoalForm, LocalClock } from './types';

interface GoalRecordingFormProps {
  showGoalForm: boolean;
  goalForm: GoalForm;
  setGoalForm: React.Dispatch<React.SetStateAction<GoalForm>>;
  currentMatch: FloorballMatchDto;
  homeTeam: FloorballTeam | null;
  awayTeam: FloorballTeam | null;
  clock: LocalClock;
  currentTimerElapsedTime: number;
  loading: boolean;
  getPlayersForTeam: (teamId: string) => FloorballPlayerDto[];
  onRecordGoal: () => Promise<void>;
  onClose: () => void;
  formatTime: (minutes: number, seconds: number) => string;
}

const GoalRecordingForm: React.FC<GoalRecordingFormProps> = ({
  showGoalForm,
  goalForm,
  setGoalForm,
  currentMatch,
  homeTeam,
  awayTeam,
  clock,
  currentTimerElapsedTime,
  loading,
  getPlayersForTeam,
  onRecordGoal,
  onClose,
  formatTime
}) => {
  if (!showGoalForm) return null;

  return (
    <div className="event-form goal-form">
      <h3>Record Goal</h3>
      <div className="form-row">
        <select 
          value={goalForm.teamId} 
          onChange={(e) => setGoalForm(prev => ({ ...prev, teamId: e.target.value, playerId: '' }))}
        >
          <option value="">Select Team</option>
          <option value={currentMatch.homeTeamId}>{homeTeam?.name || 'Home'}</option>
          <option value={currentMatch.awayTeamId}>{awayTeam?.name || 'Away'}</option>
        </select>
        
        {goalForm.teamId && (
          <select 
            value={goalForm.playerId} 
            onChange={(e) => setGoalForm(prev => ({ ...prev, playerId: e.target.value }))}
          >
            <option value="">Select Player</option>
            {getPlayersForTeam(goalForm.teamId).map(player => (
              <option key={player.id} value={player.id}>
                {player.person.firstName} {player.person.lastName}
              </option>
            ))}
          </select>
        )}
        
        {goalForm.teamId && (
          <select 
            value={goalForm.assisterId} 
            onChange={(e) => setGoalForm(prev => ({ ...prev, assisterId: e.target.value }))}
          >
            <option value="">Select Assist (Optional)</option>
            {getPlayersForTeam(goalForm.teamId)
              .filter(player => player.id !== goalForm.playerId)
              .map(player => (
                <option key={player.id} value={player.id}>
                  {player.person.firstName} {player.person.lastName}
                </option>
              ))}
          </select>
        )}
      </div>
      
      <div className="form-row compact-time-row">
        <div className="time-info">
          <div className="time-display">
            <label>Current Time:</label>
            <span className="current-time">
              Period {clock.period} - {formatTime(Math.floor(currentTimerElapsedTime / 60), currentTimerElapsedTime % 60)}
            </span>
          </div>
        </div>
      </div>
      
      <div className="form-actions">
        <button onClick={onRecordGoal} disabled={loading} className="submit-btn">
          {loading ? 'Recording...' : 'Record Goal'}
        </button>
        <button onClick={onClose} className="cancel-btn">Cancel</button>
      </div>
    </div>
  );
};

export default GoalRecordingForm; 