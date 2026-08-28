export function formatMatchHeaderDate(dateString: string): {
  weekday: string;
  date: string;
  time: string;
} {
  const date = new Date(dateString);
  const weekday = date.toLocaleDateString('fi-FI', { weekday: 'long' });
  const day = date.getDate();
  const month = date.getMonth() + 1;
  const hours = date.getHours().toString().padStart(2, '0');
  const minutes = date.getMinutes().toString().padStart(2, '0');

  return {
    weekday,
    date: `${day}.${month}`,
    time: `klo: ${hours}:${minutes}`,
  };
}

export function getTeamInitials(name: string): string {
  return name
    .split(' ')
    .map((word) => word.charAt(0))
    .join('')
    .substring(0, 3)
    .toUpperCase();
}
