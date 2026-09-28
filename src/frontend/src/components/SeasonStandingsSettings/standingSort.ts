export const STANDING_SORT_OPTIONS = [
  'Points',
  'GoalDifference',
  'GoalsFor',
  'HeadToHeadPoints',
  'HeadToHeadGoalDifference',
  'HeadToHeadGoalsFor',
  'PenaltyMinutes',
  'Draw',
] as const;

export const DEFAULT_STANDING_SORT = [
  'Points',
  'GoalDifference',
  'GoalsFor',
  'HeadToHeadPoints',
  'HeadToHeadGoalDifference',
] as const;

const KNOWN_STANDING_SORT = new Set<string>([...STANDING_SORT_OPTIONS, 'GoalsAgainst']);

export function normalizeStandingCriteria(value: readonly string[] | null | undefined): string[] {
  if (!value || value.length === 0) {
    return [...DEFAULT_STANDING_SORT];
  }

  const ordered: string[] = [];
  const seen = new Set<string>();
  for (const item of value) {
    if (!KNOWN_STANDING_SORT.has(item) || seen.has(item)) {
      continue;
    }
    seen.add(item);
    ordered.push(item);
  }

  return ordered.length > 0 ? ordered : [...DEFAULT_STANDING_SORT];
}
