import type { HockeyMatchEventDto } from '../types/hockey/hockeyTypes';

export function isHockeyGoalEvent(event: HockeyMatchEventDto): boolean {
  return event.eventType.toLowerCase() === 'goal';
}

export function isHockeyPenaltyEvent(event: HockeyMatchEventDto): boolean {
  return event.eventType.toLowerCase() === 'penalty';
}

export function isHockeyMissedPenaltyShotEvent(event: HockeyMatchEventDto): boolean {
  return event.eventType.toLowerCase() === 'missedpenaltyshot';
}

/** Finnish scoresheet tag for a goal strength: YV for power play, AV for short-handed. */
export function hockeyGoalStrengthAbbreviation(goalStrength: string | null | undefined): string {
  if (!goalStrength) return '';
  if (goalStrength.startsWith('PowerPlay')) return 'YV';
  if (goalStrength.startsWith('ShortHanded')) return 'AV';
  return '';
}

export interface HockeyGoalTag {
  abbreviation: string;
  /** i18n key of the full name. */
  labelKey: string;
}

/** Scoresheet tags of a goal: strength, penalty shot, empty net and delayed penalty. */
export function hockeyGoalTags(event: HockeyMatchEventDto): HockeyGoalTag[] {
  const tags: HockeyGoalTag[] = [];
  const strength: string = hockeyGoalStrengthAbbreviation(event.goalStrength);
  if (strength) tags.push({ abbreviation: strength, labelKey: `hockey.matches.goalStrengths.${event.goalStrength}` });
  if (event.wasPenaltyShotGoal) tags.push({ abbreviation: 'RL', labelKey: 'hockey.matches.goalFlags.penaltyShot' });
  if (event.wasEmptyNet) tags.push({ abbreviation: 'TM', labelKey: 'hockey.matches.goalFlags.emptyNet' });
  if (event.wasDelayedPenalty) tags.push({ abbreviation: 'SR', labelKey: 'matchPage.events.delayedPenalty' });
  return tags;
}
