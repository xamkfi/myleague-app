import type { ReactElement } from 'react';
import { useTranslation } from 'react-i18next';

interface EventPeriodSelectProps {
  id: string;
  periodNumber: number;
  periods: readonly number[];
  overtimePeriodNumber: number;
  shootoutPeriodNumber: number;
  onChange: (periodNumber: number) => void;
}

function EventPeriodSelect({
  id,
  periodNumber,
  periods,
  overtimePeriodNumber,
  shootoutPeriodNumber,
  onChange,
}: EventPeriodSelectProps): ReactElement {
  const { t } = useTranslation();
  const options: number[] = periods.length > 0 ? [...periods] : [Math.max(1, periodNumber)];

  const labelFor = (period: number): string => {
    if (period === shootoutPeriodNumber) {
      return t('hockey.matches.shootout', 'Shootout');
    }
    if (period === overtimePeriodNumber) {
      return t('hockey.matches.overtime', 'Overtime');
    }
    return t('hockey.matches.periodN', 'Period {{number}}', { number: period });
  };

  return (
    <div className="field">
      <label htmlFor={id}>{t('hockey.matches.eventPeriod', 'Period')}</label>
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
