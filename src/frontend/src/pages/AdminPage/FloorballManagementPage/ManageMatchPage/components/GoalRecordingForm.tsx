import { useEffect, useMemo, useRef } from 'react';
import { useTranslation } from 'react-i18next';
import { FloorballGoalType, type FloorballMatchDto, type FloorballTeam } from '../../../../../types/floorball/floorballTypes';
import './GoalRecordingForm.scss';
import type { FloorballPlayerDto } from '../../../../../api/floorball/floorballPlayerService';
import type { GoalForm } from './types';
import { FLOORBALL_GOAL_TYPE_OPTIONS } from '../../../../../utils/floorballGoalType';
import { formatPlayerOptionLabel, sortPlayersForSelect } from './eventFormHelpers';
import EventTimeInput from '../../../../../components/match/EventTimeInput';
import FloorballEventPeriodSelect from './FloorballEventPeriodSelect';

interface GoalRecordingFormProps {
  showGoalForm: boolean;
  goalForm: GoalForm;
  setGoalForm: React.Dispatch<React.SetStateAction<GoalForm>>;
  currentMatch: FloorballMatchDto;
  homeTeam: FloorballTeam | null;
  awayTeam: FloorballTeam | null;
  loading: boolean;
  getPlayersForTeam: (teamId: string) => FloorballPlayerDto[];
  onRecordGoal: () => Promise<void>;
  onClose: () => void;
  /** Played periods the event can be put in. */
  periods: readonly number[];
  overtimePeriodNumber: number;
  shootoutPeriodNumber: number;
  onTimeChange: (timeMinutes: number, timeSeconds: number) => void;
  onPeriodChange: (periodNumber: number) => void;
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
  onClose,
  periods,
  overtimePeriodNumber,
  shootoutPeriodNumber,
  onTimeChange,
  onPeriodChange,
}: GoalRecordingFormProps) => {
  const { t } = useTranslation();
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
          <h3 id="goal-record-modal-title">
            {t('floorball.matches.manage.goalForm.title', { team: selectedTeamName ?? t('common.team') })}
          </h3>
          <button
            className="goal-record-modal__close"
            onClick={onClose}
            disabled={loading}
            type="button"
            aria-label={t('common.close')}
          >
            ×
          </button>
        </div>

        <div className="goal-record-modal__body">
          <div className="event-form goal-form">
            <div className="form-grid">
              <div className="field">
                <label htmlFor="scoring-player">{t('floorball.matches.manage.goalForm.scoringPlayer')}</label>
                <select
                  id="scoring-player"
                  ref={firstFieldRef}
                  className={`select-field${goalForm.playerId ? '' : ' is-placeholder'}`}
                  value={goalForm.playerId}
                  onChange={(e) => setGoalForm((prev) => ({ ...prev, playerId: e.target.value }))}
                >
                  <option value="">{t('floorball.matches.manage.goalForm.selectPlayer')}</option>
                  {sortedPlayers.map((player) => (
                    <option key={player.id} value={player.id}>
                      {formatPlayerOptionLabel(player)}
                    </option>
                  ))}
                </select>
              </div>

              <div className="field">
                <label htmlFor="assisting-player">
                  {t('floorball.matches.manage.goalForm.assistingPlayer')}{' '}
                  <span className="field-hint">{t('floorball.matches.manage.goalForm.optional')}</span>
                </label>
                <select
                  id="assisting-player"
                  className={`select-field${goalForm.assisterId ? '' : ' is-placeholder'}`}
                  value={goalForm.assisterId}
                  onChange={(e) => setGoalForm((prev) => ({ ...prev, assisterId: e.target.value }))}
                >
                  <option value="">{t('floorball.matches.manage.goalForm.noAssist')}</option>
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
                <label htmlFor="goal-type">{t('floorball.matches.manage.goalForm.goalType')}</label>
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
                  <option value="">{t('floorball.matches.manage.goalTypes.Regular')}</option>
                  {FLOORBALL_GOAL_TYPE_OPTIONS.filter((o) => o.value !== FloorballGoalType.Regular).map((option) => (
                    <option key={option.value} value={option.value}>
                      {t(`floorball.matches.manage.goalTypes.${option.name}`, option.label)}
                    </option>
                  ))}
                </select>
              </div>

              <FloorballEventPeriodSelect
                id="goal-period"
                periodNumber={goalForm.periodNumber}
                periods={periods}
                overtimePeriodNumber={overtimePeriodNumber}
                shootoutPeriodNumber={shootoutPeriodNumber}
                onChange={onPeriodChange}
              />

              <EventTimeInput
                idPrefix="goal-time"
                label={t('floorball.matches.manage.goalForm.time')}
                minutes={goalForm.timeMinutes}
                seconds={goalForm.timeSeconds}
                onChange={onTimeChange}
              />
            </div>

            {missingJersey && (
              <div className="field-error" role="alert">
                {t('floorball.matches.manage.goalForm.missingJersey')}
              </div>
            )}

            <div className="form-actions">
              <button onClick={onClose} className="cancel-btn" type="button" disabled={loading}>
                {t('common.cancel')}
              </button>
              <button
                onClick={onRecordGoal}
                disabled={!canSubmit}
                className="submit-btn"
                type="button"
              >
                {loading
                  ? t('floorball.matches.manage.goalForm.recording')
                  : missingJersey
                    ? t('floorball.matches.manage.goalForm.missingJerseyShort')
                    : t('floorball.matches.manage.goalForm.submit')}
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default GoalRecordingForm;
