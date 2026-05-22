import { FloorballGoalType } from '../types/floorball/floorballTypes';

/**
 * Display metadata for a {@link FloorballGoalType}.
 *
 * - `abbreviation` is the short, sport-standard tag rendered in compact lists
 *   (e.g. the match-events list). It uses the Finnish abbreviations that are
 *   the de-facto standard in Finnish floorball statistics. `Regular` has no
 *   abbreviation because it is the implicit default.
 * - `label` is the long human-readable name used in dropdowns and tooltips.
 */
export interface FloorballGoalTypeInfo {
  value: FloorballGoalType;
  /** Backend enum name (e.g. "PenaltyShot") as serialized by JsonStringEnumConverter. */
  name: string;
  abbreviation: string;
  label: string;
}

/**
 * Ordered list of goal types shown in the goal recording form. Order matches
 * the rough frequency / importance for a typical floorball match.
 */
export const FLOORBALL_GOAL_TYPE_OPTIONS: readonly FloorballGoalTypeInfo[] = [
  { value: FloorballGoalType.Regular, name: 'Regular', abbreviation: '', label: 'Regular (TM)' },
  { value: FloorballGoalType.PowerPlay, name: 'PowerPlay', abbreviation: 'YV', label: 'Power play (YV)' },
  { value: FloorballGoalType.ShortHanded, name: 'ShortHanded', abbreviation: 'AV', label: 'Short-handed (AV)' },
  { value: FloorballGoalType.EmptyNet, name: 'EmptyNet', abbreviation: 'TM', label: 'Empty net (TM)' },
  { value: FloorballGoalType.PenaltyShot, name: 'PenaltyShot', abbreviation: 'RL', label: 'Penalty shot (RL)' },
  { value: FloorballGoalType.OwnGoal, name: 'OwnGoal', abbreviation: 'OM', label: 'Own goal (OM)' },
  { value: FloorballGoalType.Overtime, name: 'Overtime', abbreviation: 'JA', label: 'Overtime (JA)' },
  { value: FloorballGoalType.Shootout, name: 'Shootout', abbreviation: 'VL', label: 'Shootout (VL)' },
] as const;

const INFO_BY_VALUE: ReadonlyMap<FloorballGoalType, FloorballGoalTypeInfo> = new Map(
  FLOORBALL_GOAL_TYPE_OPTIONS.map((option) => [option.value, option] as const),
);

const INFO_BY_NAME: ReadonlyMap<string, FloorballGoalTypeInfo> = new Map(
  FLOORBALL_GOAL_TYPE_OPTIONS.map((option) => [option.name.toLowerCase(), option] as const),
);

/**
 * Looks up the display metadata for a goal type, accepting either the numeric
 * enum value (e.g. `4`) or the backend's serialized string name (e.g.
 * `"PenaltyShot"`). The backend's `JsonStringEnumConverter` emits the name
 * form, while command payloads still use the numeric form, so we normalize
 * both at the boundary.
 *
 * Returns `undefined` when the input is missing or not a known enum member.
 */
export function getFloorballGoalTypeInfo(
  goalType: FloorballGoalType | number | string | null | undefined,
): FloorballGoalTypeInfo | undefined {
  if (goalType === null || goalType === undefined) return undefined;

  if (typeof goalType === 'number') {
    return INFO_BY_VALUE.get(goalType as FloorballGoalType);
  }

  if (typeof goalType === 'string') {
    const trimmed = goalType.trim();
    if (trimmed.length === 0) return undefined;

    // Numeric strings (e.g. "4") — coerce to number then look up by value.
    const asNumber: number = Number(trimmed);
    if (!Number.isNaN(asNumber)) {
      const byNumber = INFO_BY_VALUE.get(asNumber as FloorballGoalType);
      if (byNumber) return byNumber;
    }

    return INFO_BY_NAME.get(trimmed.toLowerCase());
  }

  return undefined;
}

/**
 * Returns the short abbreviation for a goal type, or an empty string when no
 * meaningful abbreviation should be rendered (unknown value or `Regular`).
 */
export function getFloorballGoalTypeAbbreviation(
  goalType: FloorballGoalType | number | string | null | undefined,
): string {
  return getFloorballGoalTypeInfo(goalType)?.abbreviation ?? '';
}

/**
 * Returns the long human-readable label for a goal type, falling back to an
 * empty string for unknown values.
 */
export function getFloorballGoalTypeLabel(
  goalType: FloorballGoalType | number | string | null | undefined,
): string {
  return getFloorballGoalTypeInfo(goalType)?.label ?? '';
}
