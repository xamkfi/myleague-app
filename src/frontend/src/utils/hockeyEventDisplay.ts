import type { HockeyMatchEventDto } from '../types/hockey/hockeyTypes';

export function isHockeyGoalEvent(event: HockeyMatchEventDto): boolean {
  return event.eventType.toLowerCase() === 'goal';
}

export function isHockeyPenaltyEvent(event: HockeyMatchEventDto): boolean {
  return event.eventType.toLowerCase() === 'penalty';
}

/** Finnish scoresheet tag for a goal strength: YV for power play, AV for short-handed. */
export function hockeyGoalStrengthAbbreviation(goalStrength: string | null | undefined): string {
  if (!goalStrength) return '';
  if (goalStrength.startsWith('PowerPlay')) return 'YV';
  if (goalStrength.startsWith('ShortHanded')) return 'AV';
  return '';
}
