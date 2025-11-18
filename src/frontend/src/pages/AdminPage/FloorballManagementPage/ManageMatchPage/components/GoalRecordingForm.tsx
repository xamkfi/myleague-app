import { useState } from 'react';
import type { FloorballMatchDto, FloorballTeam } from '../../../../../types/floorball/floorballTypes';
import './GoalRecordingForm.scss';
import type { FloorballPlayerDto } from '../../../../../api/floorball/floorballPlayerService';
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

const GoalRecordingForm = ({
  showGoalForm,
  goalForm,
  setGoalForm,
  currentMatch,
  homeTeam,
  awayTeam,
  currentTimerElapsedTime,
  loading,
  getPlayersForTeam,
  onRecordGoal,
  onClose,
  formatTime
}: GoalRecordingFormProps) => {
  const [goalType, setGoalType] = useState<string>('');
  if (!showGoalForm) return null;

  const selectedTeamName = goalForm.teamId === currentMatch.homeTeamId 
    ? homeTeam?.name 
    : awayTeam?.name;

  return (
    <div className="goal-record-modal-overlay" onClick={onClose}>
      <div className="goal-record-modal" onClick={(e) => e.stopPropagation()}>
        <div className="goal-record-modal__header">
          <h3>Record goal for {selectedTeamName}</h3>
          <button className="goal-record-modal__close" onClick={onClose} disabled={loading}>×</button>
        </div>
        <div className="goal-record-modal__body">
          <div className="event-form goal-form">
            <div className="form-row">
              <label htmlFor="scoring-player">Scoring player</label>
              <select 
                id="scoring-player"
                className={`select-field${goalForm.playerId ? '' : ' is-placeholder'}`}
                value={goalForm.playerId} 
                onChange={(e) => setGoalForm(prev => ({ ...prev, playerId: e.target.value }))}
              >
                <option value="">Player name</option>
                {getPlayersForTeam(goalForm.teamId).map(player => (
                  <option key={player.id} value={player.id}>
                    {player.person.firstName} {player.person.lastName}
                  </option>
                ))}
              </select>
              
              <label htmlFor="assisting-player">Assisting player (optional)</label>
              <select 
                id="assisting-player"
                className={`select-field${goalForm.assisterId ? '' : ' is-placeholder'}`}
                value={goalForm.assisterId} 
                onChange={(e) => setGoalForm(prev => ({ ...prev, assisterId: e.target.value }))}
              >
                <option value="">Player name</option>
                {getPlayersForTeam(goalForm.teamId)
                  .filter(player => player.id !== goalForm.playerId)
                  .map(player => (
                    <option key={player.id} value={player.id}>
                      {player.person.firstName} {player.person.lastName}
                    </option>
                  ))}
              </select>

              <label htmlFor="goal-type">Goal type</label>
              <select 
                id="goal-type"
                className={`select-field${goalType ? '' : ' is-placeholder'}`}
                value={goalType}
                onChange={(e) => setGoalType(e.target.value)}
              >
                <option value="" disabled>Goal type</option>
                <option value="not-implemented">Not implemented yet</option>
              </select>
            </div>
            
            <div className="form-row">
              <label htmlFor="goal-time">Time</label>
              <input
                id="goal-time"
                type="text"
                className="time-input"
                defaultValue={formatTime(Math.floor(currentTimerElapsedTime / 60), currentTimerElapsedTime % 60)}
                placeholder="00:00:00"
              />
            </div>
            
            <div className="form-actions">
              <button onClick={onRecordGoal} disabled={loading} className="submit-btn">
                {loading ? 'Recording...' : 'Record Goal'}
              </button>
              <button onClick={onClose} className="cancel-btn">Cancel</button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default GoalRecordingForm; 