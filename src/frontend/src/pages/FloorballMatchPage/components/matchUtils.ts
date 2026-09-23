import type { FloorballMatchDto } from '../../../types/floorball/floorballTypes';

export function formatDate(dateString: string) {
  const date = new Date(dateString);
  const weekday = date.toLocaleDateString('fi-FI', { weekday: 'long' });
  const day = date.getDate();
  const month = date.getMonth() + 1;
  const hours = date.getHours().toString().padStart(2, '0');
  const minutes = date.getMinutes().toString().padStart(2, '0');
  const time = `klo: ${hours}:${minutes}`;
  
  return {
    weekday,
    date: `${day}.${month}`,
    time
  };
}

export function getTeamInitials(name: string): string {
  return name
    .split(' ')
    .map(word => word.charAt(0))
    .join('')
    .substring(0, 3)
    .toUpperCase();
}

export function formatTime(timeInSeconds: number): string {
  const minutes = Math.floor(timeInSeconds / 60);
  const seconds = timeInSeconds % 60;
  return `${minutes}:${seconds.toString().padStart(2, '0')}`;
}

/**
 * Resolves a team's display name by ID. Falls back to "TBD" when the slot is unassigned on the
 * match (placeholder fixtures), and to "Unknown Team" when the ID matches neither participant.
 */
export function getTeamName(teamId: string, match: FloorballMatchDto): string {
  if (teamId === match.homeTeamId) return match.homeTeamName ?? 'TBD';
  if (teamId === match.awayTeamId) return match.awayTeamName ?? 'TBD';
  return 'Unknown Team';
}

export function getPeriodName(period: number): string {
  if (period <= 3) return `${period}${period === 1 ? 'ST' : period === 2 ? 'ND' : 'RD'} PERIOD`;
  return `${period === 4 ? 'OVERTIME' : `PERIOD ${period}`}`;
} 