import { FloorballGoalType, type FloorballMatchDto, type FloorballTeam } from '../../../../../types/floorball/floorballTypes';
import './GoalRecordingForm.scss';
import type { FloorballPlayerDto } from '../../../../../api/floorball/floorballPlayerService';
import type { GoalForm, LocalClock } from './types';
import { FLOORBALL_GOAL_TYPE_OPTIONS } from '../../../../../utils/floorballGoalType';

interface GoalRecordingFormProps {
  showGoalForm: boolean;
  goalForm: GoalForm;
  setGoalForm: React.Dispatch<React.SetStateAction<GoalForm>>;
  currentMatch: FloorballMatchDto;
  homeTeam: FloorballTeam | null;
  awayTeam: FloorballTeam | null;
  clock: LocalClock;
  loading: boolean;
  getPlayersForTeam: (teamId: string) => FloorballPlayerDto[];
  onRecordGoal: () => Promise<void>;
  onClose: () => void;
}

const GoalRecordingForm = ({
  showGoalForm,
  goalForm,
  setGoalForm,
  currentMatch,
  homeTeam,
  awayTeam,
  loading,
  getPlayersForTeam,
  onRecordGoal,
  onClose
}: GoalRecordingFormProps) => {
  if (!showGoalForm) return null;
  const goalTypeValue: string = goalForm.goalType === null || goalForm.goalType === undefined
    ? ''
    : String(goalForm.goalType);

  const selectedTeamName = goalForm.teamId === currentMatch.homeTeamId
    ? homeTeam?.name
    : awayTeam?.name;
  const sortedPlayers = [...getPlayersForTeam(goalForm.teamId)].sort((a, b) => {
    const aNumber = a.jerseyNumber ?? Number.POSITIVE_INFINITY;
    const bNumber = b.jerseyNumber ?? Number.POSITIVE_INFINITY;
    if (aNumber !== bNumber) {
      return aNumber - bNumber;
    }
    const aName = `${a.person.firstName} ${a.person.lastName}`.toLowerCase();
    const bName = `${b.person.firstName} ${b.person.lastName}`.toLowerCase();
    return aName.localeCompare(bName);
  });
  const selectedPlayer = sortedPlayers.find(p => p.id === goalForm.playerId);
  const missingJersey = !!(goalForm.playerId && !selectedPlayer?.jerseyNumber);

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
                <option value="">Player</option>
                {sortedPlayers.map(player => {
                  const label = `${player.jerseyNumber ?? '??'} - ${player.person.firstName} ${player.person.lastName}`;
                  return (
                    <option key={player.id} value={player.id}>
                      {label}
                    </option>
                  );
                })}
              </select>
              
              <label htmlFor="assisting-player">Assisting player (optional)</label>
              <select 
                id="assisting-player"
                className={`select-field${goalForm.assisterId ? '' : ' is-placeholder'}`}
                value={goalForm.assisterId} 
                onChange={(e) => setGoalForm(prev => ({ ...prev, assisterId: e.target.value }))}
              >
                <option value="">Player</option>
                {sortedPlayers
                  .filter(player => player.id !== goalForm.playerId)
                  .map(player => {
                    const label = `${player.jerseyNumber ?? '??'} - ${player.person.firstName} ${player.person.lastName}`;
                    return (
                      <option key={player.id} value={player.id}>
                        {label}
                      </option>
                    );
                  })}
              </select>

              <label htmlFor="goal-type">Goal type</label>
              <select 
                id="goal-type"
                className={`select-field${goalTypeValue === '' ? ' is-placeholder' : ''}`}
                value={goalTypeValue}
                onChange={(e) => {
                  const next: string = e.target.value;
                  setGoalForm(prev => ({
                    ...prev,
                    goalType: next === '' ? null : (Number(next) as FloorballGoalType)
                  }));
                }}
              >
                <option value="">Goal type</option>
                {FLOORBALL_GOAL_TYPE_OPTIONS.map(option => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </select>
            </div>
            
            <div className="form-row">
              <label htmlFor="goal-time">Time</label>
              <div className="time-input-group">
                <input
                  id="goal-time-minutes"
                  type="number"
                  className="time-input time-input-minutes"
                  value={goalForm.timeMinutes}
                  onChange={(e) => {
                    const mins = Math.max(0, Math.min(99, parseInt(e.target.value) || 0));
                    setGoalForm(prev => ({ ...prev, timeMinutes: mins }));
                  }}
                  min="0"
                  max="99"
                  placeholder="MM"
                />
                <span className="time-separator">:</span>
                <input
                  id="goal-time-seconds"
                  type="number"
                  className="time-input time-input-seconds"
                  value={goalForm.timeSeconds}
                  onChange={(e) => {
                    const secs = Math.max(0, Math.min(59, parseInt(e.target.value) || 0));
                    setGoalForm(prev => ({ ...prev, timeSeconds: secs }));
                  }}
                  min="0"
                  max="59"
                  placeholder="SS"
                />
              </div>
            </div>
            
            <div className="form-actions">
              {missingJersey && (
                <div className="field-error" role="alert">
                  Selected player has no jersey number.
                </div>
              )}
              <button onClick={onClose} className="cancel-btn">Cancel</button>
              <button onClick={onRecordGoal} disabled={loading || missingJersey} className="submit-btn">
                {loading ? 'Recording...' : missingJersey ? 'Missing jersey' : 'Record Goal'}
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default GoalRecordingForm; 