import type { CalendarEvent } from '../../../types/calendar';
import { isHockeyMatchFinished, isHockeyMatchLive, type HockeyMatchDto } from '../../../types/hockey/hockeyTypes';

function formatCalendarDate(isoDateTime: string): { date: string; time: string } {
  const parsed = new Date(isoDateTime);
  const date = parsed.toISOString().slice(0, 10);
  const hours = parsed.getHours().toString().padStart(2, '0');
  const minutes = parsed.getMinutes().toString().padStart(2, '0');
  return { date, time: `${hours}.${minutes}` };
}

export function mapHockeyMatchToCalendarEvent(
  match: HockeyMatchDto,
  teamNames: Map<string, string>,
  competitionName?: string,
): CalendarEvent {
  const { date, time } = formatCalendarDate(match.scheduledStartTime);
  const home = match.homeTeamId ? teamNames.get(match.homeTeamId) ?? 'TBD' : 'TBD';
  const away = match.awayTeamId ? teamNames.get(match.awayTeamId) ?? 'TBD' : 'TBD';
  let status: CalendarEvent['status'] = 'scheduled';
  if (isHockeyMatchLive(match.status)) {
    status = 'live';
  } else if (isHockeyMatchFinished(match.status)) {
    status = 'completed';
  } else if (match.status === 'Cancelled') {
    status = 'cancelled';
  }
  return {
    id: match.id,
    date,
    time,
    title: `${home} – ${away}`,
    subtitle: competitionName,
    link: `/hockey/match/${match.id}`,
    sport: 'icehockey',
    status,
    venue: match.venue ?? undefined,
    homeScore: match.homeScore,
    awayScore: match.awayScore,
  };
}
