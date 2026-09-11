import { useCallback, useEffect, useMemo, useState, type ChangeEvent, type ReactElement } from 'react';
import { useTranslation } from 'react-i18next';
import { floorballTournamentService } from '../../../../../../api/floorball/floorballTournamentService';
import type {
  FloorballPlayoffRoundKey,
  FloorballTournamentDto,
  PlayoffScheduleSlotDto,
  PlayoffScheduleSlotRequest,
} from '../../../../../../types/floorball/tournamentTypes';
import './PlayoffScheduleManager.scss';

interface PlayoffScheduleManagerProps {
  tournament: FloorballTournamentDto;
  onUpdated: (tournament: FloorballTournamentDto) => void;
}

interface EditableSlot {
  /**
   * Stable identifier for React keys. Slots loaded from the server reuse `${round}-${order}`,
   * newly added rows get a `temp-${n}` id so re-ordering on save is safe.
   */
  key: string;
  round: FloorballPlayoffRoundKey;
  order: number;
  scheduledDateTime: string;
  venue: string;
}

const ROUND_OPTIONS: ReadonlyArray<FloorballPlayoffRoundKey> = [
  'QuarterFinal',
  'SemiFinal',
  'ThirdPlaceMatch',
  'Final',
];

const ROUND_SORT_ORDER: Record<FloorballPlayoffRoundKey, number> = {
  QuarterFinal: 1,
  SemiFinal: 2,
  ThirdPlaceMatch: 3,
  Final: 4,
};

/**
 * Converts a UTC ISO string (e.g. "2026-05-21T15:00:00Z") into the `<input type="datetime-local">`
 * value shape ("2026-05-21T18:00") in the user's local timezone. The input element stores the
 * value without a timezone, so we strip seconds + Z and re-emit local-time digits.
 */
function utcIsoToLocalInputValue(iso: string): string {
  if (!iso) return '';
  const d: Date = new Date(iso);
  if (Number.isNaN(d.getTime())) return '';
  const pad = (n: number): string => n.toString().padStart(2, '0');
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
}

/**
 * Inverse of {@link utcIsoToLocalInputValue}: turns the local datetime-local value into a UTC
 * ISO string suitable for the API. Returns an empty string when the input is blank/invalid so
 * the caller can surface a validation error instead of sending `Invalid Date`.
 */
function localInputValueToUtcIso(local: string): string {
  if (!local) return '';
  const d: Date = new Date(local);
  if (Number.isNaN(d.getTime())) return '';
  return d.toISOString();
}

function slotToEditable(slot: PlayoffScheduleSlotDto): EditableSlot {
  return {
    key: `${slot.round}-${slot.order}`,
    round: slot.round,
    order: slot.order,
    scheduledDateTime: utcIsoToLocalInputValue(slot.scheduledDateTime),
    venue: slot.venue ?? '',
  };
}

/**
 * Computes the next available `order` index for the given round, so when an admin adds another
 * "QuarterFinal" row we suggest the next position (QF1, QF2, QF3 ...) instead of duplicating 0.
 */
function nextOrderForRound(slots: EditableSlot[], round: FloorballPlayoffRoundKey): number {
  const used: Set<number> = new Set(slots.filter((s) => s.round === round).map((s) => s.order));
  let candidate: number = 0;
  while (used.has(candidate)) candidate += 1;
  return candidate;
}

/**
 * Admin-side editor for the tournament's pre-defined playoff schedule. Lets the operator plan
 * the kickoff time (and optional venue) for each bracket slot — quarterfinals, semifinals, the
 * optional third-place match, and the final — before any teams are known.
 *
 * Visibility rules:
 *  - Only meaningful while the tournament is still in `Draft` or `GroupStage` because the
 *    backend's `SetPlayoffSchedule` rejects edits once the bracket has been generated. When the
 *    status is already past `GroupStage`, this component renders a read-only summary.
 *  - The slot list also drives the public placeholder rows in `PlannedPlayoffSchedule`, so any
 *    edits made here show up on the public tournament schedule immediately after saving.
 */
export default function PlayoffScheduleManager({ tournament, onUpdated }: PlayoffScheduleManagerProps): ReactElement {
  const { t } = useTranslation();

  const isLocked: boolean = useMemo(() => {
    return tournament.tournamentStatus === 'PlayoffStage' || tournament.tournamentStatus === 'Completed';
  }, [tournament.tournamentStatus]);

  const hasPlayoffStage: boolean = tournament.tournamentRules?.hasPlayoffStage === true;

  const [slots, setSlots] = useState<EditableSlot[]>(() => tournament.playoffSchedule.map(slotToEditable));
  const [tempIdSeed, setTempIdSeed] = useState<number>(0);
  const [saving, setSaving] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  // Reset local edit state whenever the parent reloads the tournament; this matters after a
  // successful save so the "dirty" indicator clears, and after lifecycle transitions that may
  // mutate the schedule out-of-band.
  useEffect(() => {
    setSlots(tournament.playoffSchedule.map(slotToEditable));
    setTempIdSeed(0);
    setError(null);
    setSuccessMessage(null);
  }, [tournament.playoffSchedule]);

  const sortedSlots: EditableSlot[] = useMemo(() => {
    // Display order: chronological if a date is set, otherwise by (round, order). Slots that
    // share a kickoff time still need (round, order) as the tiebreaker so QF1 stays above QF2.
    return [...slots].sort((a, b) => {
      const aTime: number = a.scheduledDateTime ? new Date(a.scheduledDateTime).getTime() : Number.POSITIVE_INFINITY;
      const bTime: number = b.scheduledDateTime ? new Date(b.scheduledDateTime).getTime() : Number.POSITIVE_INFINITY;
      if (aTime !== bTime) return aTime - bTime;
      const aRound: number = ROUND_SORT_ORDER[a.round] ?? 99;
      const bRound: number = ROUND_SORT_ORDER[b.round] ?? 99;
      if (aRound !== bRound) return aRound - bRound;
      return a.order - b.order;
    });
  }, [slots]);

  const teamsAdvancingPerGroup: number = tournament.tournamentRules?.teamsAdvancingPerGroup ?? 0;
  const groupCount: number = tournament.groups?.length ?? 0;
  const expectedPlayoffTeams: number = teamsAdvancingPerGroup * groupCount;
  const hasThirdPlaceMatch: boolean = tournament.tournamentRules?.hasThirdPlaceMatch === true;

  const updateSlot = useCallback((key: string, patch: Partial<EditableSlot>): void => {
    setSlots((prev) => prev.map((s) => (s.key === key ? { ...s, ...patch } : s)));
  }, []);

  const removeSlot = useCallback((key: string): void => {
    setSlots((prev) => prev.filter((s) => s.key !== key));
  }, []);

  const addSlot = useCallback((round: FloorballPlayoffRoundKey): void => {
    setSlots((prev) => {
      const order: number = nextOrderForRound(prev, round);
      const nextSeed: number = tempIdSeed + 1;
      setTempIdSeed(nextSeed);
      return [
        ...prev,
        {
          key: `temp-${nextSeed}`,
          round,
          order,
          scheduledDateTime: '',
          venue: '',
        },
      ];
    });
  }, [tempIdSeed]);

  /**
   * Generates a complete bracket scaffold based on the configured tournament rules — number
   * of groups × teams advancing → 2/4/8 bracket size. Re-running it on an existing schedule
   * is a no-op for slots that already exist (matched by `round` + `order`), so admins can fill
   * in some times manually and then "Fill in missing slots" to add the rest.
   */
  const handleGenerateLayout = useCallback((): void => {
    if (expectedPlayoffTeams !== 2 && expectedPlayoffTeams !== 4 && expectedPlayoffTeams !== 8) {
      setError(t(
        'floorball.tournaments.playoffScheduleManager.errors.unsupportedSize',
        'Cannot suggest a layout: TeamsAdvancingPerGroup × group count must equal 2, 4 or 8 (current: {{count}}).',
        { count: expectedPlayoffTeams }
      ));
      return;
    }
    setError(null);

    const target: Array<{ round: FloorballPlayoffRoundKey; order: number }> = [];
    if (expectedPlayoffTeams === 8) {
      for (let i = 0; i < 4; i += 1) target.push({ round: 'QuarterFinal', order: i });
    }
    if (expectedPlayoffTeams >= 4) {
      for (let i = 0; i < 2; i += 1) target.push({ round: 'SemiFinal', order: i });
    }
    if (hasThirdPlaceMatch && expectedPlayoffTeams >= 4) {
      target.push({ round: 'ThirdPlaceMatch', order: 0 });
    }
    target.push({ round: 'Final', order: 0 });

    setSlots((prev) => {
      const existing: Map<string, EditableSlot> = new Map(prev.map((s) => [`${s.round}-${s.order}`, s]));
      let seed: number = tempIdSeed;
      const merged: EditableSlot[] = [];
      for (const targetSlot of target) {
        const k: string = `${targetSlot.round}-${targetSlot.order}`;
        const hit: EditableSlot | undefined = existing.get(k);
        if (hit) {
          merged.push(hit);
          existing.delete(k);
        } else {
          seed += 1;
          merged.push({
            key: `temp-${seed}`,
            round: targetSlot.round,
            order: targetSlot.order,
            scheduledDateTime: '',
            venue: '',
          });
        }
      }
      // Keep any "extra" slots the admin added manually (e.g. a fifth QF) so we don't drop edits.
      for (const leftover of existing.values()) {
        merged.push(leftover);
      }
      setTempIdSeed(seed);
      return merged;
    });
  }, [expectedPlayoffTeams, hasThirdPlaceMatch, tempIdSeed, t]);

  const handleSave = useCallback(async (): Promise<void> => {
    setError(null);
    setSuccessMessage(null);

    // Reject the request client-side so the admin sees the error inline instead of round-tripping
    // a ValidationFailed from the backend.
    const conflicts: string[] = [];
    const seen: Set<string> = new Set();
    for (const s of slots) {
      const k: string = `${s.round}-${s.order}`;
      if (seen.has(k)) {
        conflicts.push(k);
      } else {
        seen.add(k);
      }
      if (!s.scheduledDateTime) {
        setError(t(
          'floorball.tournaments.playoffScheduleManager.errors.missingTime',
          'Every playoff slot must have a kickoff time.'
        ));
        return;
      }
    }
    if (conflicts.length > 0) {
      setError(t(
        'floorball.tournaments.playoffScheduleManager.errors.duplicateSlot',
        'Duplicate playoff slots detected: {{slots}}',
        { slots: conflicts.join(', ') }
      ));
      return;
    }

    const payload: PlayoffScheduleSlotRequest[] = slots.map((s) => ({
      round: s.round,
      order: s.order,
      scheduledDateTime: localInputValueToUtcIso(s.scheduledDateTime),
      venue: s.venue.trim() ? s.venue.trim() : undefined,
    }));

    try {
      setSaving(true);
      const response = await floorballTournamentService.updatePlayoffSchedule(tournament.id, payload);
      if (response.data) {
        onUpdated(response.data);
        setSuccessMessage(t(
          'floorball.tournaments.playoffScheduleManager.savedSuccess',
          'Playoff schedule saved.'
        ));
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to save playoff schedule');
    } finally {
      setSaving(false);
    }
  }, [slots, tournament.id, onUpdated, t]);

  if (!hasPlayoffStage) {
    return (
      <div className="psm psm--disabled">
        <div className="psm__header">
          <h4 className="psm__title">
            <i className="fas fa-trophy" aria-hidden="true" />{' '}
            {t('floorball.tournaments.playoffScheduleManager.title', 'Planned playoff schedule')}
          </h4>
        </div>
        <p className="psm__hint">
          {t(
            'floorball.tournaments.playoffScheduleManager.disabledHint',
            'This tournament does not have a playoff stage enabled. Enable it on the Tournament Details tab to plan playoff matches.'
          )}
        </p>
      </div>
    );
  }

  return (
    <div className="psm">
      <div className="psm__header">
        <h4 className="psm__title">
          <i className="fas fa-trophy" aria-hidden="true" />{' '}
          {t('floorball.tournaments.playoffScheduleManager.title', 'Planned playoff schedule')}
        </h4>
        {!isLocked && (
          <div className="psm__actions">
            <button
              type="button"
              className="psm__btn psm__btn--ghost"
              onClick={handleGenerateLayout}
              disabled={saving}
              title={t(
                'floorball.tournaments.playoffScheduleManager.fillSuggestedTooltip',
                'Add the missing bracket slots based on the configured tournament rules.'
              )}
            >
              <i className="fas fa-magic" aria-hidden="true" />{' '}
              {t('floorball.tournaments.playoffScheduleManager.fillSuggested', 'Fill in missing slots')}
            </button>
            <button
              type="button"
              className="psm__btn psm__btn--primary"
              onClick={handleSave}
              disabled={saving}
            >
              {saving ? (
                <>
                  <i className="fas fa-spinner fa-spin" aria-hidden="true" />{' '}
                  {t('common.saving', 'Saving...')}
                </>
              ) : (
                <>
                  <i className="fas fa-save" aria-hidden="true" />{' '}
                  {t('common.save', 'Save')}
                </>
              )}
            </button>
          </div>
        )}
      </div>

      <p className="psm__hint">
        {isLocked
          ? t(
              'floorball.tournaments.playoffScheduleManager.lockedHint',
              'The playoff bracket has been generated, so the planned schedule is no longer editable. Edit individual matches in the matches list instead.'
            )
          : t(
              'floorball.tournaments.playoffScheduleManager.hint',
              'Set the kickoff time and optional venue for every playoff round. Teams are filled in automatically when the playoff stage is started; the slots appear as "TBD vs TBD" placeholders in the schedule meanwhile.'
            )}
      </p>

      {error && <div className="psm__error" role="alert">{error}</div>}
      {successMessage && <div className="psm__success" role="status">{successMessage}</div>}

      {sortedSlots.length === 0 ? (
        <div className="psm__empty">
          <p>
            {t(
              'floorball.tournaments.playoffScheduleManager.empty',
              'No playoff slots planned yet.'
            )}
          </p>
        </div>
      ) : (
        <div className="psm__table-wrap">
          <table className="psm__table">
            <thead>
              <tr>
                <th>{t('floorball.tournaments.playoffScheduleManager.fields.round', 'Round')}</th>
                <th>{t('floorball.tournaments.playoffScheduleManager.fields.order', 'Order')}</th>
                <th>{t('floorball.tournaments.playoffScheduleManager.fields.scheduledDateTime', 'Date & time')}</th>
                <th>{t('floorball.tournaments.playoffScheduleManager.fields.venue', 'Venue (optional)')}</th>
                {!isLocked && (
                  <th className="psm__table-actions-col">
                    {t('common.actions', 'Toiminnot')}
                  </th>
                )}
              </tr>
            </thead>
            <tbody>
              {sortedSlots.map((slot) => (
                <tr key={slot.key}>
                  <td>
                    <select
                      value={slot.round}
                      disabled={isLocked || saving}
                      onChange={(e: ChangeEvent<HTMLSelectElement>): void => {
                        const round = e.target.value as FloorballPlayoffRoundKey;
                        updateSlot(slot.key, { round });
                      }}
                    >
                      {ROUND_OPTIONS.map((r) => (
                        <option key={r} value={r}>
                          {t(`floorball.tournaments.playoffScheduleManager.rounds.${r}`, r)}
                        </option>
                      ))}
                    </select>
                  </td>
                  <td>
                    <input
                      type="number"
                      min={0}
                      step={1}
                      value={slot.order}
                      disabled={isLocked || saving}
                      onChange={(e: ChangeEvent<HTMLInputElement>): void => {
                        const next: number = parseInt(e.target.value, 10);
                        updateSlot(slot.key, { order: Number.isFinite(next) && next >= 0 ? next : 0 });
                      }}
                    />
                  </td>
                  <td>
                    <input
                      type="datetime-local"
                      value={slot.scheduledDateTime}
                      disabled={isLocked || saving}
                      onChange={(e: ChangeEvent<HTMLInputElement>): void => {
                        updateSlot(slot.key, { scheduledDateTime: e.target.value });
                      }}
                    />
                  </td>
                  <td>
                    <input
                      type="text"
                      value={slot.venue}
                      placeholder={t(
                        'floorball.tournaments.playoffScheduleManager.placeholders.venue',
                        'e.g. Court 1'
                      )}
                      disabled={isLocked || saving}
                      onChange={(e: ChangeEvent<HTMLInputElement>): void => {
                        updateSlot(slot.key, { venue: e.target.value });
                      }}
                    />
                  </td>
                  {!isLocked && (
                    <td className="psm__table-actions-col">
                      <button
                        type="button"
                        className="psm__btn psm__btn--danger"
                        onClick={(): void => removeSlot(slot.key)}
                        disabled={saving}
                        aria-label={t(
                          'floorball.tournaments.playoffScheduleManager.removeRowAriaLabel',
                          'Poista tämä pudotuspelipaikka'
                        )}
                        title={t(
                          'floorball.tournaments.playoffScheduleManager.removeRowTooltip',
                          'Poista tämä pudotuspelipaikka aikataulusta'
                        )}
                      >
                        <i className="fas fa-trash" aria-hidden="true" />
                        <span>{t('common.remove', 'Poista')}</span>
                      </button>
                    </td>
                  )}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {!isLocked && (
        <div className="psm__add-row">
          {ROUND_OPTIONS.map((round) => (
            <button
              key={round}
              type="button"
              className="psm__btn psm__btn--ghost"
              onClick={(): void => addSlot(round)}
              disabled={saving}
            >
              <i className="fas fa-plus" aria-hidden="true" />{' '}
              {t(`floorball.tournaments.playoffScheduleManager.addRound.${round}`, `Add ${round}`)}
            </button>
          ))}
        </div>
      )}
    </div>
  );
}
