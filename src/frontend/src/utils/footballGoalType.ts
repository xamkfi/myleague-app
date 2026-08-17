import { FootballGoalType } from '../types/football/footballTypes';

export interface FootballGoalTypeInfo {
  value: FootballGoalType;
  name: string;
  abbreviation: string;
  label: string;
}

export const FOOTBALL_GOAL_TYPE_OPTIONS: readonly FootballGoalTypeInfo[] = [
  { value: FootballGoalType.Regular, name: 'Regular', abbreviation: '', label: 'Regular' },
  { value: FootballGoalType.PenaltyKick, name: 'PenaltyKick', abbreviation: 'PK', label: 'Penalty kick' },
  { value: FootballGoalType.OwnGoal, name: 'OwnGoal', abbreviation: 'OG', label: 'Own goal' },
  { value: FootballGoalType.ExtraTime, name: 'ExtraTime', abbreviation: 'ET', label: 'Extra time' },
  { value: FootballGoalType.PenaltyShootout, name: 'PenaltyShootout', abbreviation: 'PSO', label: 'Penalty shootout' },
] as const;

const INFO_BY_VALUE: ReadonlyMap<FootballGoalType, FootballGoalTypeInfo> = new Map(
  FOOTBALL_GOAL_TYPE_OPTIONS.map((option) => [option.value, option] as const),
);

const INFO_BY_NAME: ReadonlyMap<string, FootballGoalTypeInfo> = new Map(
  FOOTBALL_GOAL_TYPE_OPTIONS.map((option) => [option.name.toLowerCase(), option] as const),
);

export function getFootballGoalTypeInfo(
  goalType: FootballGoalType | number | string | null | undefined,
): FootballGoalTypeInfo | undefined {
  if (goalType === null || goalType === undefined) return undefined;

  if (typeof goalType === 'number') {
    return INFO_BY_VALUE.get(goalType as FootballGoalType);
  }

  if (typeof goalType === 'string') {
    const trimmed = goalType.trim();
    if (trimmed.length === 0) return undefined;

    const asNumber: number = Number(trimmed);
    if (!Number.isNaN(asNumber)) {
      const byNumber = INFO_BY_VALUE.get(asNumber as FootballGoalType);
      if (byNumber) return byNumber;
    }

    return INFO_BY_NAME.get(trimmed.toLowerCase());
  }

  return undefined;
}
