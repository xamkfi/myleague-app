import type { FloorballMatchDto } from '../../../types/floorball/floorballTypes';

export function formatDate(dateString: string) {
  const date = new Date(dateString);
  const formattedDate = date.toLocaleDateString('fi-FI', {
    day: 'numeric',
    month: 'numeric',
    year: 'numeric'
  });
  const formattedTime = date.toLocaleTimeString('fi-FI', {
    hour: '2-digit',
    minute: '2-digit'
  });
  return `${formattedDate} ${formattedTime}`;
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