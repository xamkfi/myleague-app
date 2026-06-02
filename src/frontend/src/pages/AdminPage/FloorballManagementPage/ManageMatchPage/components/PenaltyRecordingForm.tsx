import { useEffect, useMemo, useRef } from 'react';
import './PenaltyRecordingForm.scss';
import type { FloorballMatchDto, FloorballTeam } from '../../../../../types/floorball/floorballTypes';
import type { FloorballPlayerDto } from '../../../../../api/floorball/floorballPlayerService';
import type { PenaltyForm, LocalClock } from './types';
import { formatPlayerOptionLabel, sortPlayersForSelect } from './eventFormHelpers';

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

const PENALTY_DURATION_OPTIONS: ReadonlyArray<{ value: number; label: string }> = [
  { value: 2, label: '2 minutes (minor)' },
  { value: 5, label: '5 minutes (major)' },
  { value: 10, label: '10 minutes (misconduct)' },
  { value: 20, label: '20 minutes (game misconduct)' },
];

const PENALTY_TYPE_OPTIONS: ReadonlyArray<{ value: string; label: string }> = [
  { value: 'Minor', label: 'Minor' },
  { value: 'Major', label: 'Major' },
];

const DESCRIPTION_MAX_LENGTH: number = 280;

/**
 * Clamps a numeric input value to a valid range, mirroring the helper in `GoalRecordingForm`.
 * Kept inline (instead of being shared) because the two forms otherwise own their own input
 * semantics and we want to keep their files self-contained for readability.
 */
const clampInt = (raw: string, min: number, max: number): number => {
  const parsed: number = parseInt(raw, 10);
  if (Number.isNaN(parsed)) return min;
  return Math.max(min, Math.min(max, parsed));
};

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
  onClose,
}: PenaltyRecordingFormProps) => {
  const firstFieldRef = useRef<HTMLSelectElement | null>(null);

  const sortedPlayers: FloorballPlayerDto[] = useMemo(
    () => (penaltyForm.teamId ? sortPlayersForSelect(getPlayersForTeam(penaltyForm.teamId)) : []),
    [penaltyForm.teamId, getPlayersForTeam],
  );

  const selectedPlayer: FloorballPlayerDto | undefined = sortedPlayers.find((p) => p.id === penaltyForm.playerId);
  const missingJersey: boolean = !!(penaltyForm.playerId && selectedPlayer?.jerseyNumber === undefined);
  const canSubmit: boolean =
    !!penaltyForm.playerId && !!penaltyForm.penaltyType && penaltyForm.minutes > 0 && !missingJersey && !loading;

  const selectedTeamName: string | undefined =
    penaltyForm.teamId === currentMatch.homeTeamId ? homeTeam?.name : awayTeam?.name;

  useEffect(() => {
    if (showPenaltyForm) {
      firstFieldRef.current?.focus();
    }
  }, [showPenaltyForm]);

  const handleKeyDown = (e: React.KeyboardEvent<HTMLDivElement>): void => {
    if (e.key === 'Escape') {
      e.preventDefault();
      e.stopPropagation();
      onClose();
      return;
    }
    if (e.key === 'Enter') {
      const target = e.target as HTMLElement;
      if (target?.tagName === 'TEXTAREA') return;
      if (canSubmit) {
        e.preventDefault();
        e.stopPropagation();
        void onRecordPenalty();
      }
    }
  };

  if (!showPenaltyForm) return null;

  return (
    <div className="penalty-record-modal-overlay" onClick={onClose} role="presentation">
      <div
        className="penalty-record-modal"
        onClick={(e) => e.stopPropagation()}
        onKeyDown={handleKeyDown}
        role="dialog"
        aria-modal="true"
        aria-labelledby="penalty-record-modal-title"
      >
        <div className="penalty-record-modal__header">
          <h3 id="penalty-record-modal-title">Record penalty for {selectedTeamName ?? 'team'}</h3>
          <button
            className="penalty-record-modal__close"
            onClick={onClose}
            disabled={loading}
            type="button"
            aria-label="Close"
          >
            ×
          </button>
        </div>

        <div className="penalty-record-modal__body">
          <div className="event-form penalty-form">
            <div className="form-grid">
              <div className="field">
                <label htmlFor="penalty-player">Receiving player</label>
                <select
                  id="penalty-player"
                  ref={firstFieldRef}
                  className={`select-field${penaltyForm.playerId ? '' : ' is-placeholder'}`}
                  value={penaltyForm.playerId}
                  onChange={(e) => setPenaltyForm((prev) => ({ ...prev, playerId: e.target.value }))}
                >
                  <option value="">Select player</option>
                  {sortedPlayers.map((player) => (
                    <option key={player.id} value={player.id}>
                      {formatPlayerOptionLabel(player)}
                    </option>
                  ))}
                </select>
              </div>

              <div className="field">
                <label htmlFor="penalty-type">Penalty severity</label>
                <select
                  id="penalty-type"
                  className={`select-field${penaltyForm.penaltyType ? '' : ' is-placeholder'}`}
                  value={penaltyForm.penaltyType}
                  onChange={(e) => setPenaltyForm((prev) => ({ ...prev, penaltyType: e.target.value }))}
                >
                  <option value="">Select severity</option>
                  {PENALTY_TYPE_OPTIONS.map((option) => (
                    <option key={option.value} value={option.value}>
                      {option.label}
                    </option>
                  ))}
                </select>
              </div>

              <div className="field">
                <label htmlFor="penalty-duration">Duration</label>
                <select
                  id="penalty-duration"
                  className={`select-field${penaltyForm.minutes ? '' : ' is-placeholder'}`}
                  value={penaltyForm.minutes || ''}
                  onChange={(e) => setPenaltyForm((prev) => ({ ...prev, minutes: parseInt(e.target.value, 10) || 0 }))}
                >
                  <option value="">Select duration</option>
                  {PENALTY_DURATION_OPTIONS.map((option) => (
                    <option key={option.value} value={option.value}>
                      {option.label}
                    </option>
                  ))}
                </select>
              </div>

              <div className="field field--time">
                <label htmlFor="penalty-time-minutes">Time</label>
                <div className="time-input-group">
                  <input
                    id="penalty-time-minutes"
                    type="number"
                    className="time-input time-input-minutes"
                    value={penaltyForm.timeMinutes}
                    onChange={(e) =>
                      setPenaltyForm((prev) => ({ ...prev, timeMinutes: clampInt(e.target.value, 0, 99) }))
                    }
                    min={0}
                    max={99}
                    placeholder="MM"
                    aria-label="Minutes"
                  />
                  <span className="time-separator" aria-hidden="true">
                    :
                  </span>
                  <input
                    id="penalty-time-seconds"
                    type="number"
                    className="time-input time-input-seconds"
                    value={penaltyForm.timeSeconds}
                    onChange={(e) =>
                      setPenaltyForm((prev) => ({ ...prev, timeSeconds: clampInt(e.target.value, 0, 59) }))
                    }
                    min={0}
                    max={59}
                    placeholder="SS"
                    aria-label="Seconds"
                  />
                </div>
              </div>
            </div>

            <div className="field field--description">
              <label htmlFor="penalty-description">
                Description <span className="field-hint">(optional)</span>
              </label>
              <textarea
                id="penalty-description"
                value={penaltyForm.description}
                onChange={(e) => setPenaltyForm((prev) => ({ ...prev, description: e.target.value }))}
                placeholder="E.g. hooking, slashing, unsportsmanlike conduct…"
                className="description-input"
                maxLength={DESCRIPTION_MAX_LENGTH}
                rows={3}
              />
              <div className="description-counter" aria-live="polite">
                {penaltyForm.description.length}/{DESCRIPTION_MAX_LENGTH}
              </div>
            </div>

            {missingJersey && (
              <div className="field-error" role="alert">
                Selected player has no jersey number. Assign a jersey before recording the penalty.
              </div>
            )}

            <div className="form-actions">
              <button onClick={onClose} className="cancel-btn" type="button" disabled={loading}>
                Cancel
              </button>
              <button
                onClick={onRecordPenalty}
                disabled={!canSubmit}
                className="submit-btn"
                type="button"
              >
                {loading ? 'Recording…' : missingJersey ? 'Missing jersey' : 'Record Penalty'}
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default PenaltyRecordingForm;
