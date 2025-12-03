import './PenaltyRecordingForm.scss';
import type { FloorballMatchDto, FloorballTeam } from '../../../../../types/floorball/floorballTypes';
import type { FloorballPlayerDto } from '../../../../../api/floorball/floorballPlayerService';
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
  currentTimerElapsedTime,
  loading,
  getPlayersForTeam,
  onRecordPenalty,
  onClose,
  formatTime
}: PenaltyRecordingFormProps) => {
  if (!showPenaltyForm) return null;

  const selectedTeamName = penaltyForm.teamId === currentMatch.homeTeamId 
    ? homeTeam?.name 
    : awayTeam?.name;

  return (
    <div className="penalty-record-modal-overlay" onClick={onClose}>
      <div className="penalty-record-modal" onClick={(e) => e.stopPropagation()}>
        <div className="penalty-record-modal__header">
          <h3>Record penalty for {selectedTeamName}</h3>
          <button className="penalty-record-modal__close" onClick={onClose} disabled={loading}>×</button>
        </div>
        <div className="penalty-record-modal__body">
          <div className="event-form penalty-form">
            <div className="form-row">
              <label htmlFor="penalty-player">Receiving player</label>
              <select
                id="penalty-player"
                className={`select-field${penaltyForm.playerId ? '' : ' is-placeholder'}`}
                value={penaltyForm.playerId}
                onChange={(e) => setPenaltyForm(prev => ({ ...prev, playerId: e.target.value }))}
              >
                <option value="">Player name</option>
                {getPlayersForTeam(penaltyForm.teamId).map(player => (
                  <option key={player.id} value={player.id}>
                    {player.person.firstName} {player.person.lastName}
                  </option>
                ))}
              </select>

              <label htmlFor="penalty-type">Penalty type</label>
              <select
                id="penalty-type"
                className={`select-field${penaltyForm.penaltyType ? '' : ' is-placeholder'}`}
                value={penaltyForm.penaltyType}
                onChange={(e) => setPenaltyForm(prev => ({ ...prev, penaltyType: e.target.value }))}
              >
                <option value="">Penalty type</option>
                <option value="Minor">Minor</option>
                <option value="Major">Major</option>
              </select>

              <label htmlFor="penalty-duration">Duration</label>
              <select
                id="penalty-duration"
                className={`select-field${penaltyForm.minutes ? '' : ' is-placeholder'}`}
                value={penaltyForm.minutes || ''}
                onChange={(e) => setPenaltyForm(prev => ({ ...prev, minutes: parseInt(e.target.value) }))}
              >
                <option value="" disabled>2 minutes</option>
                <option value={2}>2 minutes</option>
                <option value={5}>5 minutes</option>
                <option value={10}>10 minutes</option>
                <option value={20}>20 minutes</option>
              </select>
            </div>

            <div className="form-row">
              <label htmlFor="penalty-time">Time</label>
              <input
                id="penalty-time"
                type="text"
                className="time-input"
                defaultValue={formatTime(Math.floor(currentTimerElapsedTime / 60), currentTimerElapsedTime % 60)}
                placeholder="(current time)"
              />
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
        </div>
      </div>
    </div>
  );
};

export default PenaltyRecordingForm; 