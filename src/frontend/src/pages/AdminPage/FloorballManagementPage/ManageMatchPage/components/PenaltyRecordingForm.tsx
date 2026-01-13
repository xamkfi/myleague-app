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
  loading: boolean;
  getPlayersForTeam: (teamId: string) => FloorballPlayerDto[];
  onRecordPenalty: () => Promise<void>;
  onClose: () => void;
}

const PenaltyRecordingForm = ({
  showPenaltyForm,
  penaltyForm,
  setPenaltyForm,
  currentMatch,
  homeTeam,
  awayTeam,
  loading,
  getPlayersForTeam,
  onRecordPenalty,
  onClose
}: PenaltyRecordingFormProps) => {
  if (!showPenaltyForm) return null;

  const selectedTeamName = penaltyForm.teamId === currentMatch.homeTeamId
    ? homeTeam?.name
    : awayTeam?.name;
  const sortedPlayers = [...getPlayersForTeam(penaltyForm.teamId)].sort((a, b) => {
    const aNumber = a.jerseyNumber ?? Number.POSITIVE_INFINITY;
    const bNumber = b.jerseyNumber ?? Number.POSITIVE_INFINITY;
    if (aNumber !== bNumber) {
      return aNumber - bNumber;
    }
    const aName = `${a.person.firstName} ${a.person.lastName}`.toLowerCase();
    const bName = `${b.person.firstName} ${b.person.lastName}`.toLowerCase();
    return aName.localeCompare(bName);
  });
  const selectedPlayer = sortedPlayers.find(p => p.id === penaltyForm.playerId);
  const missingJersey = !!(penaltyForm.playerId && !selectedPlayer?.jerseyNumber);

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
              <div className="time-input-group">
                <input
                  id="penalty-time-minutes"
                  type="number"
                  className="time-input time-input-minutes"
                  value={penaltyForm.timeMinutes}
                  onChange={(e) => {
                    const mins = Math.max(0, Math.min(99, parseInt(e.target.value) || 0));
                    setPenaltyForm(prev => ({ ...prev, timeMinutes: mins }));
                  }}
                  min="0"
                  max="99"
                  placeholder="MM"
                />
                <span className="time-separator">:</span>
                <input
                  id="penalty-time-seconds"
                  type="number"
                  className="time-input time-input-seconds"
                  value={penaltyForm.timeSeconds}
                  onChange={(e) => {
                    const secs = Math.max(0, Math.min(59, parseInt(e.target.value) || 0));
                    setPenaltyForm(prev => ({ ...prev, timeSeconds: secs }));
                  }}
                  min="0"
                  max="59"
                  placeholder="SS"
                />
              </div>
            </div>

            <textarea 
              value={penaltyForm.description}
              onChange={(e) => setPenaltyForm(prev => ({ ...prev, description: e.target.value }))}
              placeholder="Description (optional)"
              className="description-input"
            />

            <div className="form-actions">
              {missingJersey && (
                <div className="field-error" role="alert">
                  Selected player has no jersey number.
                </div>
              )}
              <button onClick={onClose} className="cancel-btn">Cancel</button>
              <button onClick={onRecordPenalty} disabled={loading || missingJersey} className="submit-btn">
                {loading ? 'Recording...' : missingJersey ? 'Missing jersey' : 'Record Penalty'}
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default PenaltyRecordingForm; 