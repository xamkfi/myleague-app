import { HOCKEY_SHOT_RESULTS, type HockeyMatchEventDto } from '../types/hockey/hockeyTypes';

type Translate = (key: string, fallback: string) => string;

const SHOT_RESULT_SET: ReadonlySet<string> = new Set(HOCKEY_SHOT_RESULTS);

const PUBLIC_EVENT_TYPES = new Set(['goal', 'penalty', 'shot', 'stoppage']);

export function isPublicHockeyEvent(event: HockeyMatchEventDto): boolean {
  const type = event.eventType.toLowerCase();
  if (!PUBLIC_EVENT_TYPES.has(type) && !type.includes('goal') && !type.includes('penalty') && !type.includes('shot') && !type.includes('stoppage')) {
    return false;
  }
  if (type.includes('faceoff') || type.includes('period')) {
    return false;
  }
  if (type.includes('stoppage') && event.description !== 'Offside') {
    return false;
  }
  return true;
}

export function hockeyPublicEventLabel(
  event: HockeyMatchEventDto,
  t: Translate,
): { label: string; badge: string; typeClass: string } {
  const type = event.eventType.toLowerCase();
  if (type.includes('goal')) {
    return { label: t('hockey.matches.eventGoal', 'Goal'), badge: 'G', typeClass: 'goal' };
  }
  if (type.includes('penalty')) {
    return { label: t('hockey.matches.eventPenalty', 'Penalty'), badge: 'P', typeClass: 'penalty' };
  }
  if (type.includes('shot')) {
    if (event.description === 'Saved') {
      return { label: t('hockey.matches.eventSave', 'Save'), badge: 'S', typeClass: '' };
    }
    return { label: t('hockey.matches.eventShot', 'Shot'), badge: 'S', typeClass: '' };
  }
  if (type.includes('stoppage') && event.description === 'Offside') {
    return { label: t('hockey.matches.eventOffside', 'Offside'), badge: 'O', typeClass: '' };
  }
  return { label: event.eventType, badge: '•', typeClass: '' };
}

export function hockeyPublicEventDetail(event: HockeyMatchEventDto, t: Translate): string {
  const type = event.eventType.toLowerCase();
  const description = event.description;
  if (!description) {
    return '';
  }
  if (type.includes('shot')) {
    if (description === 'Saved' || !SHOT_RESULT_SET.has(description)) {
      return '';
    }
    return t(`hockey.matches.shotResults.${description}`, description);
  }
  return '';
}
