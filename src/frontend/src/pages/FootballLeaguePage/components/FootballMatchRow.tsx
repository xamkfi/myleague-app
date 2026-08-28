import MatchRow, { type MatchRowProps } from '../../../components/MatchRow';
import { FloorballMatchStatus } from '../../../types/floorball/floorballTypes';
import { FootballMatchStatus } from '../../../types/football/footballTypes';
import { getMatchPath } from '../../../utils/sportRoutes';

function footballPeriodLabel(period: number): string {
  return period <= 2 ? `H${period}` : `ET${period - 2}`;
}

function toRowStatus(status?: FootballMatchStatus | FloorballMatchStatus): FloorballMatchStatus {
  if (status === FootballMatchStatus.InProgress || status === FloorballMatchStatus.InProgress) {
    return FloorballMatchStatus.InProgress;
  }
  if (status === FootballMatchStatus.Completed || status === FloorballMatchStatus.Completed) {
    return FloorballMatchStatus.Completed;
  }
  return FloorballMatchStatus.Scheduled;
}

export default function FootballMatchRow(props: Omit<MatchRowProps, 'status'> & { status?: FootballMatchStatus | FloorballMatchStatus }) {
  const { status, periodCount, periodLabel, href, ...rest } = props;
  return (
    <MatchRow
      {...rest}
      href={href ?? getMatchPath('football', props.id)}
      periodCount={periodCount ?? 2}
      periodLabel={periodLabel ?? footballPeriodLabel}
      status={toRowStatus(status)}
    />
  );
}
