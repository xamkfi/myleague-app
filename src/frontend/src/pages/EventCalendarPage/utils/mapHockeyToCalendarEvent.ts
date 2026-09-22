import type { CalendarEvent } from '../../../types/calendar';
import { isHockeyMatchFinished, isHockeyMatchLive, type HockeyMatchListDto } from '../../../types/hockey/hockeyTypes';

function formatCalendarDate(isoDateTime: string): { date: string; time: string } {
  const parsed = new Date(isoDateTime);
  const date = parsed.toISOString().slice(0, 10);
  const hours = parsed.getHours().toString().padStart(2, '0');
  const minutes = parsed.getMinutes().toString().padStart(2, '0');
  return { date, time: `${hours}.${minutes}` };
}

export function mapHockeyMatchToCalendarEvent(match: HockeyMatchListDto): CalendarEvent {
  const { date, time } = formatCalendarDate(match.scheduledStartTime);
  const home = match.homeTeamName ?? 'TBD';
  const away = match.awayTeamName ?? 'TBD';
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
    subtitle: match.competitionName ?? undefined,
    link: `/hockey/match/${match.id}`,
    sport: 'icehockey',
    status,
    venue: match.venue ?? undefined,
    homeScore: match.homeScore,
    awayScore: match.awayScore,
  };
}
