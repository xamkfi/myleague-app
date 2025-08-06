import React from 'react';
import type { FloorballMatchDto, FloorballTeam } from '../../../../../../../types/floorball/floorballTypes';
import type { FloorballPlayerDto } from '../../../../../../../api/floorball/floorballPlayerService';
import type { PenaltyForm, LocalClock } from './types';

interface PenaltyRecordingFormProps {
  showPenaltyForm: boolean;
  penaltyForm: PenaltyForm;
  setPenaltyForm: React.Dispatch<React.SetStateAction<PenaltyForm>>;
  currentMatch: FloorballMatchDto;
  homeTeam: FloorballTeam | null;
  awayTeam: FloorballTeam | null;
  clock: LocalClock;
  currentTimerElapsedTime: number;
  loading: boolean;
  getPlayersForTeam: (teamId: string) => FloorballPlayerDto[];
  onRecordPenalty: () => Promise<void>;
  onClose: () => void;
  formatTime: (minutes: number, seconds: number) => string;
}

const PenaltyRecordingForm = ({
  showPenaltyForm,
  penaltyForm,
  setPenaltyForm,
  currentMatch,
  homeTeam,
  awayTeam,
  clock,
  currentTimerElapsedTime,
  loading,
  getPlayersForTeam,
  onRecordPenalty,
  onClose,
  formatTime
}: PenaltyRecordingFormProps) => {
  if (!showPenaltyForm) return null;

  return (
    <div className="event-form penalty-form">
      <h3>Record Penalty</h3>
      <div className="form-row">
        <select 
          value={penaltyForm.teamId} 
          onChange={(e) => setPenaltyForm(prev => ({ ...prev, teamId: e.target.value, playerId: '' }))}
        >
          <option value="">Select Team</option>
          <option value={currentMatch.homeTeamId}>{homeTeam?.name || 'Home'}</option>
          <option value={currentMatch.awayTeamId}>{awayTeam?.name || 'Away'}</option>
        </select>
        
        {penaltyForm.teamId && (
          <select 
            value={penaltyForm.playerId} 
            onChange={(e) => setPenaltyForm(prev => ({ ...prev, playerId: e.target.value }))}
          >
            <option value="">Select Player (Optional)</option>
            {getPlayersForTeam(penaltyForm.teamId).map(player => (
              <option key={player.id} value={player.id}>
                {player.person.firstName} {player.person.lastName}
              </option>
            ))}
          </select>
        )}
        
        <select 
          value={penaltyForm.penaltyType} 
          onChange={(e) => setPenaltyForm(prev => ({ ...prev, penaltyType: e.target.value }))}
        >
          <option value="">Select Penalty Type</option>
          <option value="Minor">Minor</option>
          <option value="Major">Major</option>
        </select>
        
        <select 
          value={penaltyForm.minutes} 
          onChange={(e) => setPenaltyForm(prev => ({ ...prev, minutes: parseInt(e.target.value) }))}
        >
          <option value={2}>2 minutes</option>
          <option value={5}>5 minutes</option>
          <option value={10}>10 minutes</option>
          <option value={20}>20 minutes</option>
        </select>
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
      
      <textarea 
        value={penaltyForm.description}
        onChange={(e) => setPenaltyForm(prev => ({ ...prev, description: e.target.value }))}
        placeholder="Description (optional)"
        className="description-input"
      />
      
      <div className="form-actions">
        <button onClick={onRecordPenalty} disabled={loading} className="submit-btn">
          {loading ? 'Recording...' : 'Record Penalty'}
        </button>
        <button onClick={onClose} className="cancel-btn">Cancel</button>
      </div>
    </div>
  );
};

export default PenaltyRecordingForm; 