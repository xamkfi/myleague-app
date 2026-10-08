import type { ReactElement } from 'react';
import { useTranslation } from 'react-i18next';
import SharedEventPeriodSelect from '../../../../../components/match/EventPeriodSelect';

interface EventPeriodSelectProps {
  id: string;
  periodNumber: number;
  periods: readonly number[];
  overtimePeriodNumber: number;
  shootoutPeriodNumber: number;
  onChange: (periodNumber: number) => void;
}

/** Hockey wording for the shared event period picker. */
function EventPeriodSelect(props: EventPeriodSelectProps): ReactElement {
  const { t } = useTranslation();

  return (
    <SharedEventPeriodSelect
      {...props}
      labels={{
        field: t('hockey.matches.eventPeriod', 'Period'),
        period: (number: number) => t('hockey.matches.periodN', 'Period {{number}}', { number }),
        overtime: t('hockey.matches.overtime', 'Overtime'),
        shootout: t('hockey.matches.shootout', 'Shootout'),
      }}
    />
  );
}

export default EventPeriodSelect;
