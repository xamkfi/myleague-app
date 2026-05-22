/**
 * Formats a date string to a localized format in UTC (D.M.YYYY)
 * @param dateString Date string to format (can be null)
 * @returns Formatted date string or '-' if null
 */
export function formatDate(dateString: string | null): string {
  if (!dateString) {
    return '-';
  }
  try {
    const date = new Date(dateString);
    const day = date.getUTCDate();
    const month = date.getUTCMonth() + 1; // getUTCMonth is 0-indexed
    const year = date.getUTCFullYear();
    return `${day}.${month}.${year}`;
  } catch (error) {
    console.error('Error formatting date:', error);
    return dateString;
  }
}

/**
 * Formats a date/time string to return date and time in DD/MM HH:MM format.
 * Uses the browser's local timezone (API returns UTC ISO strings).
 *
 * @param dateTime - ISO date string or Date object
 * @returns Array with [date, time] where date is "DD/MM" and time is "HH:MM"
 */
export const formatMatchDateTime = (dateTime: string | Date): [string, string] => {
  const date = new Date(dateTime);

  const day = date.getDate().toString().padStart(2, '0');
  const month = (date.getMonth() + 1).toString().padStart(2, '0');
  const formattedDate = `${day}/${month}`;

  const hours = date.getHours().toString().padStart(2, '0');
  const minutes = date.getMinutes().toString().padStart(2, '0');
  const formattedTime = `${hours}:${minutes}`;

  return [formattedDate, formattedTime];
};

/**
 * Formats only the date part (DD/MM format without year).
 * Uses the browser's local timezone.
 *
 * @param dateTime - ISO date string or Date object
 * @returns Date string in "DD/MM" format
 */
export const formatMatchDate = (dateTime: string | Date): string => {
  const date = new Date(dateTime);
  const day = date.getDate().toString().padStart(2, '0');
  const month = (date.getMonth() + 1).toString().padStart(2, '0');
  return `${day}/${month}`;
};

/**
 * Formats only the time part (HH:MM format).
 * Uses the browser's local timezone.
 *
 * @param dateTime - ISO date string or Date object
 * @returns Time string in "HH:MM" format
 */
export const formatMatchTime = (dateTime: string | Date): string => {
  const date = new Date(dateTime);
  const hours = date.getHours().toString().padStart(2, '0');
  const minutes = date.getMinutes().toString().padStart(2, '0');
  return `${hours}:${minutes}`;
};

/**
 * Truncates a text to a specified length
 * @param text Text to truncate
 * @param maxLength Maximum length before truncating
 * @returns Truncated text with ellipsis if needed
 */
export function truncateText(text: string, maxLength: number): string {
  if (text.length <= maxLength) return text;
  return text.slice(0, maxLength) + '...';
}

/**
 * Generates a unique ID
 * @returns A unique string ID
 */
export function generateId(): string {
  return Math.random().toString(36).substring(2) + Date.now().toString(36);
} 