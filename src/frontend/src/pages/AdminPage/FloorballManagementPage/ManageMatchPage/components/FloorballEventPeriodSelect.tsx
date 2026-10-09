import type { ReactElement } from 'react';
import { useTranslation } from 'react-i18next';
import EventPeriodSelect from '../../../../../components/match/EventPeriodSelect';

interface FloorballEventPeriodSelectProps {
  id: string;
  periodNumber: number;
  periods: readonly number[];
  overtimePeriodNumber: number;
  shootoutPeriodNumber: number;
  onChange: (periodNumber: number) => void;
}

/** Floorball wording for the shared event period picker. */
function FloorballEventPeriodSelect(props: FloorballEventPeriodSelectProps): ReactElement {
  const { t } = useTranslation();

  return (
    <EventPeriodSelect
      {...props}
      labels={{
        field: t('floorball.matches.manage.eventPeriod.label', 'Period'),
        period: (number: number) => t('floorball.matches.manage.eventPeriod.period', 'Period {{number}}', { number }),
        overtime: t('floorball.matches.manage.eventPeriod.overtime', 'Overtime'),
        shootout: t('floorball.matches.manage.eventPeriod.shootout', 'Shootout'),
      }}
    />
  );
}

export default FloorballEventPeriodSelect;
