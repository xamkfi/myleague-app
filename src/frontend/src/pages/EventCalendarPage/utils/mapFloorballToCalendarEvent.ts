import type { CalendarEvent } from '../../../types/calendar';
import type { FloorballMatchDto } from '../../../types/floorball/floorballTypes';
import { FloorballMatchStatus } from '../../../types/floorball/floorballTypes';

function formatCalendarDate(isoDateTime: string): { date: string; time: string } {
  const d = new Date(isoDateTime);
  const date = d.toISOString().slice(0, 10);
  const hours = d.getHours().toString().padStart(2, '0');
  const minutes = d.getMinutes().toString().padStart(2, '0');
  const time = `${hours}.${minutes}`;
  return { date, time };
}

function matchStatusToCalendarStatus(status: FloorballMatchStatus): CalendarEvent['status'] {
  switch (status) {
    case FloorballMatchStatus.InProgress:
      return 'live';
    case FloorballMatchStatus.Completed:
      return 'completed';
    case FloorballMatchStatus.Cancelled:
      return 'cancelled';
    default:
      return 'scheduled';
  }
}

export function mapFloorballMatchToCalendarEvent(match: FloorballMatchDto): CalendarEvent {
  const { date, time } = formatCalendarDate(match.scheduledDateTime);
  return {
    id: match.id,
    date,
    time,
    title: `${match.homeTeamName} – ${match.awayTeamName}`,
    subtitle: match.competitionName,
    link: `/match/${match.id}`,
    sport: 'floorball',
    status: matchStatusToCalendarStatus(match.status),
    venue: match.venue,
    homeScore: match.homeScore,
    awayScore: match.awayScore,
  };
}
