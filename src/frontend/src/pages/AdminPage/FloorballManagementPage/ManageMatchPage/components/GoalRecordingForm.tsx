import { useEffect, useMemo, useRef } from 'react';
import { FloorballGoalType, type FloorballMatchDto, type FloorballTeam } from '../../../../../types/floorball/floorballTypes';
import './GoalRecordingForm.scss';
import type { FloorballPlayerDto } from '../../../../../api/floorball/floorballPlayerService';
import type { GoalForm, LocalClock } from './types';
import { FLOORBALL_GOAL_TYPE_OPTIONS } from '../../../../../utils/floorballGoalType';
import { formatPlayerOptionLabel, sortPlayersForSelect } from './eventFormHelpers';

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

/**
 * Clamps a numeric input value to a valid range, treating empty/NaN input as 0. Used by the
 * time fields so manual typing of "00" or partially-deleted values doesn't surface NaN to
 * the goal form state.
 */
const clampInt = (raw: string, min: number, max: number): number => {
  const parsed: number = parseInt(raw, 10);
  if (Number.isNaN(parsed)) return min;
  return Math.max(min, Math.min(max, parsed));
};

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
  onClose,
}: GoalRecordingFormProps) => {
  const firstFieldRef = useRef<HTMLSelectElement | null>(null);

  const sortedPlayers: FloorballPlayerDto[] = useMemo(
    () => (goalForm.teamId ? sortPlayersForSelect(getPlayersForTeam(goalForm.teamId)) : []),
    [goalForm.teamId, getPlayersForTeam],
  );

  const selectedPlayer: FloorballPlayerDto | undefined = sortedPlayers.find((p) => p.id === goalForm.playerId);
  const missingJersey: boolean = !!(goalForm.playerId && selectedPlayer?.jerseyNumber === undefined);
  const canSubmit: boolean = !!goalForm.playerId && !missingJersey && !loading;

  const selectedTeamName: string | undefined =
    goalForm.teamId === currentMatch.homeTeamId ? homeTeam?.name : awayTeam?.name;

  const goalTypeValue: string =
    goalForm.goalType === null || goalForm.goalType === undefined ? '' : String(goalForm.goalType);

  // Autofocus the first interactive field whenever the modal opens. Improves keyboard /
  // accessibility flow and matches the user expectation that they can start typing/selecting
  // immediately after clicking the trigger button.
  useEffect(() => {
    if (showGoalForm) {
      firstFieldRef.current?.focus();
    }
  }, [showGoalForm]);

  // Close on Escape; submit on Enter as long as required fields are valid. Submitting on
  // Enter is scoped to the modal (via the `onKeyDown` on the dialog) so it doesn't fight
  // with global keybinds defined in the parent.
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
        void onRecordGoal();
      }
    }
  };

  if (!showGoalForm) return null;

  return (
    <div className="goal-record-modal-overlay" onClick={onClose} role="presentation">
      <div
        className="goal-record-modal"
        onClick={(e) => e.stopPropagation()}
        onKeyDown={handleKeyDown}
        role="dialog"
        aria-modal="true"
        aria-labelledby="goal-record-modal-title"
      >
        <div className="goal-record-modal__header">
          <h3 id="goal-record-modal-title">Record goal for {selectedTeamName ?? 'team'}</h3>
          <button
            className="goal-record-modal__close"
            onClick={onClose}
            disabled={loading}
            type="button"
            aria-label="Close"
          >
            ×
          </button>
        </div>

        <div className="goal-record-modal__body">
          <div className="event-form goal-form">
            <div className="form-grid">
              <div className="field">
                <label htmlFor="scoring-player">Scoring player</label>
                <select
                  id="scoring-player"
                  ref={firstFieldRef}
                  className={`select-field${goalForm.playerId ? '' : ' is-placeholder'}`}
                  value={goalForm.playerId}
                  onChange={(e) => setGoalForm((prev) => ({ ...prev, playerId: e.target.value }))}
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
                <label htmlFor="assisting-player">
                  Assisting player <span className="field-hint">(optional)</span>
                </label>
                <select
                  id="assisting-player"
                  className={`select-field${goalForm.assisterId ? '' : ' is-placeholder'}`}
                  value={goalForm.assisterId}
                  onChange={(e) => setGoalForm((prev) => ({ ...prev, assisterId: e.target.value }))}
                >
                  <option value="">No assist</option>
                  {sortedPlayers
                    .filter((player) => player.id !== goalForm.playerId)
                    .map((player) => (
                      <option key={player.id} value={player.id}>
                        {formatPlayerOptionLabel(player)}
                      </option>
                    ))}
                </select>
              </div>

              <div className="field">
                <label htmlFor="goal-type">Goal type</label>
                <select
                  id="goal-type"
                  className={`select-field${goalTypeValue === '' ? ' is-placeholder' : ''}`}
                  value={goalTypeValue}
                  onChange={(e) => {
                    const next: string = e.target.value;
                    setGoalForm((prev) => ({
                      ...prev,
                      goalType: next === '' ? null : (Number(next) as FloorballGoalType),
                    }));
                  }}
                >
                  <option value="">Regular goal</option>
                  {FLOORBALL_GOAL_TYPE_OPTIONS.filter((o) => o.value !== FloorballGoalType.Regular).map((option) => (
                    <option key={option.value} value={option.value}>
                      {option.label}
                    </option>
                  ))}
                </select>
              </div>

              <div className="field field--time">
                <label htmlFor="goal-time-minutes">Time</label>
                <div className="time-input-group">
                  <input
                    id="goal-time-minutes"
                    type="number"
                    className="time-input time-input-minutes"
                    value={goalForm.timeMinutes}
                    onChange={(e) =>
                      setGoalForm((prev) => ({ ...prev, timeMinutes: clampInt(e.target.value, 0, 99) }))
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
                    id="goal-time-seconds"
                    type="number"
                    className="time-input time-input-seconds"
                    value={goalForm.timeSeconds}
                    onChange={(e) =>
                      setGoalForm((prev) => ({ ...prev, timeSeconds: clampInt(e.target.value, 0, 59) }))
                    }
                    min={0}
                    max={59}
                    placeholder="SS"
                    aria-label="Seconds"
                  />
                </div>
              </div>
            </div>

            {missingJersey && (
              <div className="field-error" role="alert">
                Selected player has no jersey number. Assign a jersey before recording the goal.
              </div>
            )}

            <div className="form-actions">
              <button onClick={onClose} className="cancel-btn" type="button" disabled={loading}>
                Cancel
              </button>
              <button
                onClick={onRecordGoal}
                disabled={!canSubmit}
                className="submit-btn"
                type="button"
              >
                {loading ? 'Recording…' : missingJersey ? 'Missing jersey' : 'Record Goal'}
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default GoalRecordingForm;
