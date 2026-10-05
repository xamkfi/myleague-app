import type { ReactElement } from 'react';
import { useTranslation } from 'react-i18next';

interface EventTimeInputProps {
  idPrefix: string;
  label: string;
  minutes: number;
  seconds: number;
  onChange: (minutes: number, seconds: number) => void;
  maxMinutes?: number;
}

/** Treats empty or NaN input as `min` so partially deleted values never surface NaN. */
function clampInt(raw: string, min: number, max: number): number {
  const parsed: number = parseInt(raw, 10);
  if (Number.isNaN(parsed)) return min;
  return Math.max(min, Math.min(max, parsed));
}

function EventTimeInput({
  idPrefix,
  label,
  minutes,
  seconds,
  onChange,
  maxMinutes = 99,
}: EventTimeInputProps): ReactElement {
  const { t } = useTranslation();

  return (
    <div className="field field--time">
      <label htmlFor={`${idPrefix}-minutes`}>{label}</label>
      <div className="time-input-group">
        <input
          id={`${idPrefix}-minutes`}
          type="number"
          className="time-input time-input-minutes"
          value={minutes}
          onChange={(e) => onChange(clampInt(e.target.value, 0, maxMinutes), seconds)}
          min={0}
          max={maxMinutes}
          placeholder="MM"
          aria-label={t('floorball.matches.manage.timeMinutesAria', 'Minutes')}
        />
        <span className="time-separator" aria-hidden="true">
          :
        </span>
        <input
          id={`${idPrefix}-seconds`}
          type="number"
          className="time-input time-input-seconds"
          value={seconds}
          onChange={(e) => onChange(minutes, clampInt(e.target.value, 0, 59))}
          min={0}
          max={59}
          placeholder="SS"
          aria-label={t('floorball.matches.manage.timeSecondsAria', 'Seconds')}
        />
      </div>
    </div>
  );
}

export default EventTimeInput;
