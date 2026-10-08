import type { ReactElement } from 'react';
import { useTranslation } from 'react-i18next';
import EventPeriodSelect from '../../../../../components/match/EventPeriodSelect';
import type { FootballMatchRules } from '../../../../../types/football/footballTypes';
import { extraTimeStartPeriod, isExtraTimePeriod, resolveMatchRules } from '../utils/lineupValidation';

interface FootballEventPeriodSelectProps {
  id: string;
  periodNumber: number;
  periods: readonly number[];
  shootoutPeriodNumber: number;
  rules: FootballMatchRules | null | undefined;
  onChange: (periodNumber: number) => void;
}

/** Football wording for the shared event period picker (halves, extra time, shootout). */
function FootballEventPeriodSelect({ rules, ...props }: FootballEventPeriodSelectProps): ReactElement {
  const { t } = useTranslation();
  const resolvedRules: FootballMatchRules = resolveMatchRules(rules);

  const periodLabel = (period: number): string => {
    if (isExtraTimePeriod(period, resolvedRules)) {
      return t('football.matches.manage.eventPeriod.extraTime', 'Extra time {{number}}', {
        number: period - extraTimeStartPeriod(resolvedRules) + 1,
      });
    }
    return t('football.matches.manage.eventPeriod.half', 'Half {{number}}', { number: period });
  };

  return (
    <EventPeriodSelect
      {...props}
      overtimePeriodNumber={-1}
      labels={{
        field: t('football.matches.manage.eventPeriod.label', 'Period'),
        period: periodLabel,
        overtime: '',
        shootout: t('football.matches.manage.eventPeriod.shootout', 'Penalty shootout'),
      }}
    />
  );
}

export default FootballEventPeriodSelect;
