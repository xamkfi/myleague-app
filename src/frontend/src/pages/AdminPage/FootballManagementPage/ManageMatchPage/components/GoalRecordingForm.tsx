import { useEffect, useMemo, useRef } from 'react';
import { useTranslation } from 'react-i18next';
import FootballEventPeriodSelect from './FootballEventPeriodSelect';
import { FootballGoalType, type FootballMatchDto, type FootballTeam } from '../../../../../types/football/footballTypes';
import './GoalRecordingForm.scss';
import type { FootballPlayerDto } from '../../../../../api/football/footballPlayerService';
import type { GoalForm } from './types';
import { FOOTBALL_GOAL_TYPE_OPTIONS } from '../../../../../utils/footballGoalType';
import { formatPlayerOptionLabel, sortPlayersForSelect } from './eventFormHelpers';
import { isExtraTimePeriod, resolveMatchRules } from '../utils/lineupValidation';

interface GoalRecordingFormProps {
  showGoalForm: boolean;
  goalForm: GoalForm;
  setGoalForm: React.Dispatch<React.SetStateAction<GoalForm>>;
  currentMatch: FootballMatchDto;
  homeTeam: FootballTeam | null;
  awayTeam: FootballTeam | null;
  loading: boolean;
  /** Everyone in the match lineup. */
  getLineupPlayersForTeam: (teamId: string) => FootballPlayerDto[];
  onRecordGoal: () => Promise<void>;
  onClose: () => void;
  /** Played periods the event can be put in. */
  periods: readonly number[];
  shootoutPeriodNumber: number;
  onTimeChange: (timeMinutes: number, timeSeconds: number) => void;
  onPeriodChange: (periodNumber: number) => void;
}

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
  getLineupPlayersForTeam,
  onRecordGoal,
  onClose,
  periods,
  shootoutPeriodNumber,
  onTimeChange,
  onPeriodChange,
}: GoalRecordingFormProps) => {
  const { t } = useTranslation();
  const firstFieldRef = useRef<HTMLSelectElement | null>(null);
  const isOwnGoal: boolean = goalForm.goalType === FootballGoalType.OwnGoal;

  const scoringTeamId: string = goalForm.teamId;
  const opponentTeamId: string =
    scoringTeamId === currentMatch.homeTeamId
      ? (currentMatch.awayTeamId ?? '')
      : (currentMatch.homeTeamId ?? '');

  const scorerPlayers: FootballPlayerDto[] = useMemo(() => {
    const sourceTeamId = isOwnGoal ? opponentTeamId : scoringTeamId;
    return sourceTeamId ? sortPlayersForSelect(getLineupPlayersForTeam(sourceTeamId)) : [];
  }, [isOwnGoal, opponentTeamId, scoringTeamId, getLineupPlayersForTeam]);

  const assisterPlayers: FootballPlayerDto[] = useMemo(() => {
    if (isOwnGoal || !scoringTeamId) return [];
    return sortPlayersForSelect(getLineupPlayersForTeam(scoringTeamId));
  }, [isOwnGoal, scoringTeamId, getLineupPlayersForTeam]);

  const selectedPlayer: FootballPlayerDto | undefined = scorerPlayers.find((p) => p.id === goalForm.playerId);
  const missingJersey: boolean = !!(goalForm.playerId && selectedPlayer?.jerseyNumber === undefined);
  const canSubmit: boolean = !!goalForm.playerId && !missingJersey && !loading;

  const selectedTeamName: string | undefined =
    goalForm.teamId === currentMatch.homeTeamId ? homeTeam?.name : awayTeam?.name;

  const goalTypeValue: string =
    goalForm.goalType === null || goalForm.goalType === undefined
      ? String(FootballGoalType.Regular)
      : String(goalForm.goalType);

  // Extra-time and shootout goals belong to their own periods. Standings points follow the period
  // that was started, not the goal type, so the types are only offered there.
  const isGoalTypeAvailable = (goalType: FootballGoalType): boolean => {
    if (goalType === FootballGoalType.ExtraTime) {
      return isExtraTimePeriod(goalForm.periodNumber, resolveMatchRules(currentMatch.matchRules));
    }
    if (goalType === FootballGoalType.PenaltyShootout) return goalForm.periodNumber === shootoutPeriodNumber;
    return true;
  };

  useEffect(() => {
    if (showGoalForm) {
      firstFieldRef.current?.focus();
    }
  }, [showGoalForm]);

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
          <h3 id="goal-record-modal-title">{t('football.matches.manage.goalForm.title', { team: selectedTeamName ?? '' })}</h3>
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
                <label htmlFor="scoring-player">
                  {isOwnGoal
                    ? t('football.matches.manage.goalForm.ownGoalScorer')
                    : t('football.matches.manage.goalForm.scoringPlayer')}
                </label>
                <select
                  id="scoring-player"
                  ref={firstFieldRef}
                  className={`select-field${goalForm.playerId ? '' : ' is-placeholder'}`}
                  value={goalForm.playerId}
                  onChange={(e) => setGoalForm((prev) => ({ ...prev, playerId: e.target.value }))}
                >
                  <option value="">{t('football.matches.manage.goalForm.selectPlayer')}</option>
                  {scorerPlayers.map((player) => (
                    <option key={player.id} value={player.id}>
                      {formatPlayerOptionLabel(player)}
                    </option>
                  ))}
                </select>
              </div>

              <div className="field">
                <label htmlFor="assisting-player">
                  {t('football.matches.manage.goalForm.assistingPlayer')}{' '}
                  <span className="field-hint">{t('football.matches.manage.goalForm.optional')}</span>
                </label>
                <select
                  id="assisting-player"
                  className={`select-field${goalForm.assisterId ? '' : ' is-placeholder'}`}
                  value={isOwnGoal ? '' : goalForm.assisterId}
                  disabled={isOwnGoal}
                  onChange={(e) => setGoalForm((prev) => ({ ...prev, assisterId: e.target.value }))}
                >
                  <option value="">{t('football.matches.manage.goalForm.noAssist')}</option>
                  {assisterPlayers
                    .filter((player) => player.id !== goalForm.playerId)
                    .map((player) => (
                      <option key={player.id} value={player.id}>
                        {formatPlayerOptionLabel(player)}
                      </option>
                    ))}
                </select>
              </div>

              <div className="field">
                <label htmlFor="goal-type">{t('football.matches.manage.goalForm.goalType')}</label>
                <select
                  id="goal-type"
                  className="select-field"
                  value={goalTypeValue}
                  onChange={(e) => {
                    const next: FootballGoalType = Number(e.target.value) as FootballGoalType;
                    setGoalForm((prev) => ({
                      ...prev,
                      goalType: next,
                      playerId: '',
                      assisterId: next === FootballGoalType.OwnGoal ? '' : prev.assisterId,
                    }));
                  }}
                >
                  {FOOTBALL_GOAL_TYPE_OPTIONS.filter((option) => isGoalTypeAvailable(option.value)).map((option) => (
                    <option key={option.value} value={option.value}>
                      {t(`football.matches.manage.goalTypes.${option.name}`, option.label)}
                    </option>
                  ))}
                </select>
              </div>

              <FootballEventPeriodSelect
                id="goal-period"
                periodNumber={goalForm.periodNumber}
                periods={periods}
                shootoutPeriodNumber={shootoutPeriodNumber}
                rules={currentMatch.matchRules}
                onChange={onPeriodChange}
              />

              <div className="field field--time">
                <label htmlFor="goal-time-minutes">{t('football.matches.manage.goalForm.time')}</label>
                <div className="time-input-group">
                  <input
                    id="goal-time-minutes"
                    type="number"
                    className="time-input time-input-minutes"
                    value={goalForm.timeMinutes}
                    onChange={(e) => onTimeChange(clampInt(e.target.value, 0, 99), goalForm.timeSeconds)}
                    min={0}
                    max={99}
                    placeholder="MM"
                    aria-label={t('floorball.matches.manage.timeMinutesAria')}
                  />
                  <span className="time-separator" aria-hidden="true">
                    :
                  </span>
                  <input
                    id="goal-time-seconds"
                    type="number"
                    className="time-input time-input-seconds"
                    value={goalForm.timeSeconds}
                    onChange={(e) => onTimeChange(goalForm.timeMinutes, clampInt(e.target.value, 0, 59))}
                    min={0}
                    max={59}
                    placeholder="SS"
                    aria-label={t('floorball.matches.manage.timeSecondsAria')}
                  />
                </div>
              </div>
            </div>

            {missingJersey && (
              <div className="field-error" role="alert">
                {t('football.matches.manage.goalForm.missingJersey')}
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
                  ? t('football.matches.manage.goalForm.recording')
                  : missingJersey
                    ? t('football.matches.manage.goalForm.missingJerseyShort')
                    : t('football.matches.manage.goalForm.submit')}
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default GoalRecordingForm;
