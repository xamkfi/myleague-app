import type { ReactElement } from 'react';

export interface EventPeriodSelectLabels {
  field: string;
  period: (periodNumber: number) => string;
  overtime: string;
  shootout: string;
}

interface EventPeriodSelectProps {
  id: string;
  periodNumber: number;
  periods: readonly number[];
  overtimePeriodNumber: number;
  shootoutPeriodNumber: number;
  /** Sport-specific wording, so each desk keeps its own terms (e.g. "Erä" vs "Puoliaika"). */
  labels: EventPeriodSelectLabels;
  onChange: (periodNumber: number) => void;
}

/**
 * Period picker for event forms. Lets the recorder put an event in any played period, which
 * matters when events are entered or corrected after the match.
 */
function EventPeriodSelect({
  id,
  periodNumber,
  periods,
  overtimePeriodNumber,
  shootoutPeriodNumber,
  labels,
  onChange,
}: EventPeriodSelectProps): ReactElement {
  const options: number[] = periods.length > 0 ? [...periods] : [Math.max(1, periodNumber)];

  const labelFor = (period: number): string => {
    if (period === shootoutPeriodNumber) {
      return labels.shootout;
    }
    if (period === overtimePeriodNumber) {
      return labels.overtime;
    }
    return labels.period(period);
  };

  return (
    <div className="field">
      <label htmlFor={id}>{labels.field}</label>
      <select
        id={id}
        className="select-field"
        value={periodNumber}
        onChange={(event) => onChange(parseInt(event.target.value, 10) || periodNumber)}
      >
        {options.map((period) => (
          <option key={period} value={period}>
            {labelFor(period)}
          </option>
        ))}
      </select>
    </div>
  );
}

export default EventPeriodSelect;
