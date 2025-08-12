import type { FloorballMatchStatus } from '../../../../../types/floorball/floorballTypes';
import type { FloorballSeasonDto } from '../../../../../api/floorball/floorballSeasonService';

export const formatDateTime = (dateTime: string): string => {
  // Create a date object from the ISO string
  // The backend stores it as UTC, so we need to convert to local time for display
  const date = new Date(dateTime);
  
  // Format the date in local timezone
  const day = date.getDate().toString().padStart(2, '0');
  const month = (date.getMonth() + 1).toString().padStart(2, '0');
  const year = date.getFullYear();
  const hours = date.getHours().toString().padStart(2, '0');
  const minutes = date.getMinutes().toString().padStart(2, '0');
  
  return `${day}.${month}.${year}, ${hours}:${minutes}`;
};

export const getStatusBadge = (status: FloorballMatchStatus): string => {
  const statusClasses = {
    'Scheduled': 'status-scheduled',
    'InProgress': 'status-progress',
    'Completed': 'status-completed',
    'Cancelled': 'status-cancelled',
    'Postponed': 'status-postponed'
  };
  
  return `status-badge ${statusClasses[status] || 'status-completed'}`;
};

export const formatSeasonDisplayName = (season: FloorballSeasonDto): string => {
  return `${season.name} (${season.startDate.split('-')[0]}-${season.endDate.split('-')[0]})`;
}; 