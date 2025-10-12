import type { FloorballMatchDto } from '../../../types/floorball/floorballTypes';

export function formatDate(dateString: string) {
  const date = new Date(dateString);
  const weekday = date.toLocaleDateString('en-US', { weekday: 'long' });
  const day = date.toLocaleDateString('en-US', { day: 'numeric' });
  const month = date.toLocaleDateString('en-US', { month: 'short' });
  const time = date.toLocaleTimeString('en-US', {
    hour: '2-digit',
    minute: '2-digit',
    hour12: false
  });
  
  return {
    weekday,
    date: `${day} ${month}`,
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

export function getTeamName(teamId: string, match: FloorballMatchDto): string {
  if (teamId === match.homeTeamId) return match.homeTeamName;
  if (teamId === match.awayTeamId) return match.awayTeamName;
  return 'Unknown Team';
}

export function getPeriodName(period: number): string {
  if (period <= 3) return `${period}${period === 1 ? 'ST' : period === 2 ? 'ND' : 'RD'} PERIOD`;
  return `${period === 4 ? 'OVERTIME' : `PERIOD ${period}`}`;
} 