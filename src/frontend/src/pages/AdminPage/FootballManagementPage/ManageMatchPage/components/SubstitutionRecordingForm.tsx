import { useEffect, useMemo, useRef } from 'react';
import './SubstitutionRecordingForm.scss';
import type { FootballMatchDto, FootballTeam } from '../../../../../types/football/footballTypes';
import type { FootballPlayerDto } from '../../../../../api/football/footballPlayerService';
import type { SubstitutionForm } from './types';
import { formatPlayerOptionLabel, sortPlayersForSelect } from './eventFormHelpers';

interface SubstitutionRecordingFormProps {
  showSubstitutionForm: boolean;
  substitutionForm: SubstitutionForm;
  setSubstitutionForm: React.Dispatch<React.SetStateAction<SubstitutionForm>>;
  currentMatch: FootballMatchDto;
  homeTeam: FootballTeam | null;
  awayTeam: FootballTeam | null;
  loading: boolean;
  getOnFieldPlayersForTeam: (teamId: string) => FootballPlayerDto[];
  getBenchPlayersForTeam: (teamId: string) => FootballPlayerDto[];
  onRecordSubstitution: () => Promise<void>;
  onClose: () => void;
}

const DESCRIPTION_MAX_LENGTH: number = 280;

const clampInt = (raw: string, min: number, max: number): number => {
  const parsed: number = parseInt(raw, 10);
  if (Number.isNaN(parsed)) return min;
  return Math.max(min, Math.min(max, parsed));
};

const SubstitutionRecordingForm = ({
  showSubstitutionForm,
  substitutionForm,
  setSubstitutionForm,
  currentMatch,
  homeTeam,
  awayTeam,
  loading,
  getOnFieldPlayersForTeam,
  getBenchPlayersForTeam,
  onRecordSubstitution,
  onClose,
}: SubstitutionRecordingFormProps) => {
  const firstFieldRef = useRef<HTMLSelectElement | null>(null);

  const onFieldPlayers: FootballPlayerDto[] = useMemo(
    () =>
      substitutionForm.teamId
        ? sortPlayersForSelect(getOnFieldPlayersForTeam(substitutionForm.teamId))
        : [],
    [substitutionForm.teamId, getOnFieldPlayersForTeam],
  );

  const benchPlayers: FootballPlayerDto[] = useMemo(
    () =>
      substitutionForm.teamId
        ? sortPlayersForSelect(getBenchPlayersForTeam(substitutionForm.teamId))
        : [],
    [substitutionForm.teamId, getBenchPlayersForTeam],
  );

  const canSubmit: boolean =
    !!substitutionForm.playerOffId && !!substitutionForm.playerOnId && !loading;

  const selectedTeamName: string | undefined =
    substitutionForm.teamId === currentMatch.homeTeamId ? homeTeam?.name : awayTeam?.name;

  useEffect(() => {
    if (showSubstitutionForm) {
      firstFieldRef.current?.focus();
    }
  }, [showSubstitutionForm]);

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
        void onRecordSubstitution();
      }
    }
  };

  if (!showSubstitutionForm) return null;

  return (
    <div className="sub-record-modal-overlay" onClick={onClose} role="presentation">
      <div
        className="sub-record-modal"
        onClick={(e) => e.stopPropagation()}
        onKeyDown={handleKeyDown}
        role="dialog"
        aria-modal="true"
        aria-labelledby="sub-record-modal-title"
      >
        <div className="sub-record-modal__header">
          <h3 id="sub-record-modal-title">Record substitution for {selectedTeamName ?? 'team'}</h3>
          <button
            className="sub-record-modal__close"
            onClick={onClose}
            disabled={loading}
            type="button"
            aria-label="Close"
          >
            ×
          </button>
        </div>

        <div className="sub-record-modal__body">
          <div className="event-form sub-form">
            <div className="form-grid">
              <div className="field">
                <label htmlFor="player-off">Player off (on field)</label>
                <select
                  id="player-off"
                  ref={firstFieldRef}
                  className={`select-field${substitutionForm.playerOffId ? '' : ' is-placeholder'}`}
                  value={substitutionForm.playerOffId}
                  onChange={(e) =>
                    setSubstitutionForm((prev) => ({ ...prev, playerOffId: e.target.value }))
                  }
                >
                  <option value="">Select player going off</option>
                  {onFieldPlayers.map((player) => (
                    <option key={player.id} value={player.id}>
                      {formatPlayerOptionLabel(player)}
                    </option>
                  ))}
                </select>
              </div>

              <div className="field">
                <label htmlFor="player-on">Player on (bench)</label>
                <select
                  id="player-on"
                  className={`select-field${substitutionForm.playerOnId ? '' : ' is-placeholder'}`}
                  value={substitutionForm.playerOnId}
                  onChange={(e) =>
                    setSubstitutionForm((prev) => ({ ...prev, playerOnId: e.target.value }))
                  }
                >
                  <option value="">Select player coming on</option>
                  {benchPlayers.map((player) => (
                    <option key={player.id} value={player.id}>
                      {formatPlayerOptionLabel(player)}
                    </option>
                  ))}
                </select>
              </div>

              <div className="field field--time">
                <label htmlFor="sub-time-minutes">Time</label>
                <div className="time-input-group">
                  <input
                    id="sub-time-minutes"
                    type="number"
                    className="time-input time-input-minutes"
                    value={substitutionForm.timeMinutes}
                    onChange={(e) =>
                      setSubstitutionForm((prev) => ({
                        ...prev,
                        timeMinutes: clampInt(e.target.value, 0, 99),
                      }))
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
                    id="sub-time-seconds"
                    type="number"
                    className="time-input time-input-seconds"
                    value={substitutionForm.timeSeconds}
                    onChange={(e) =>
                      setSubstitutionForm((prev) => ({
                        ...prev,
                        timeSeconds: clampInt(e.target.value, 0, 59),
                      }))
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
              <label htmlFor="sub-description">
                Description <span className="field-hint">(optional)</span>
              </label>
              <textarea
                id="sub-description"
                value={substitutionForm.description}
                onChange={(e) =>
                  setSubstitutionForm((prev) => ({ ...prev, description: e.target.value }))
                }
                placeholder="Optional note"
                className="description-input"
                maxLength={DESCRIPTION_MAX_LENGTH}
                rows={3}
              />
              <div className="description-counter" aria-live="polite">
                {substitutionForm.description.length}/{DESCRIPTION_MAX_LENGTH}
              </div>
            </div>

            <div className="form-actions">
              <button onClick={onClose} className="cancel-btn" type="button" disabled={loading}>
                Cancel
              </button>
              <button
                onClick={onRecordSubstitution}
                disabled={!canSubmit}
                className="submit-btn"
                type="button"
              >
                {loading ? 'Recording…' : 'Record Substitution'}
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default SubstitutionRecordingForm;
