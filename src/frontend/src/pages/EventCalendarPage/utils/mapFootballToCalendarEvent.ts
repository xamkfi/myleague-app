import type { CalendarEvent } from '../../../types/calendar';
import type { FootballMatchDto } from '../../../types/football/footballTypes';
import { FootballMatchStatus } from '../../../types/football/footballTypes';

function formatCalendarDate(isoDateTime: string): { date: string; time: string } {
  const parsed = new Date(isoDateTime);
  const date = parsed.toISOString().slice(0, 10);
  const hours = parsed.getHours().toString().padStart(2, '0');
  const minutes = parsed.getMinutes().toString().padStart(2, '0');
  return { date, time: `${hours}.${minutes}` };
}

function matchStatusToCalendarStatus(status: FootballMatchStatus): CalendarEvent['status'] {
  switch (status) {
    case FootballMatchStatus.InProgress:
      return 'live';
    case FootballMatchStatus.Completed:
      return 'completed';
    case FootballMatchStatus.Cancelled:
      return 'cancelled';
    default:
      return 'scheduled';
  }
}

export function mapFootballMatchToCalendarEvent(match: FootballMatchDto): CalendarEvent {
  const { date, time } = formatCalendarDate(match.scheduledDateTime);
  const home = match.homeTeamName ?? 'TBD';
  const away = match.awayTeamName ?? 'TBD';
  return {
    id: match.id,
    date,
    time,
    title: `${home} – ${away}`,
    subtitle: match.competitionName,
    link: `/football/match/${match.id}`,
    sport: 'football',
    status: matchStatusToCalendarStatus(match.status),
    venue: match.venue,
    homeScore: match.homeScore,
    awayScore: match.awayScore,
  };
}
