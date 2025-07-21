/**
 * Formats a date/time string to return date and time in DD/MM HH:MM format
 * 
 * @param dateTime - ISO date string or Date object
 * @returns Array with [date, time] where date is "DD/MM" and time is "HH:MM"
 * 
 * @example
 * formatMatchDateTime("2025-09-07T21:30:00")
 * // Returns: ["07/09", "21:30"]
 */
export const formatMatchDateTime = (dateTime: string | Date): [string, string] => {
   const date = new Date(dateTime);
   
   // Format date as "DD/MM" (no year)
   const day = date.getDate().toString().padStart(2, '0');
   const month = (date.getMonth() + 1).toString().padStart(2, '0');
   const formattedDate = `${day}/${month}`;
   
   // Format time as "HH:MM"
   const hours = date.getHours().toString().padStart(2, '0');
   const minutes = date.getMinutes().toString().padStart(2, '0');
   const formattedTime = `${hours}:${minutes}`;
   
   return [formattedDate, formattedTime];
};

/**
 * Formats only the date part (DD/MM format without year)
 * 
 * @param dateTime - ISO date string or Date object
 * @returns Date string in "DD/MM" format
 * 
 * @example
 * formatMatchDate("2025-09-07T21:30:00")
 * // Returns: "07/09"
 */
export const formatMatchDate = (dateTime: string | Date): string => {
   const date = new Date(dateTime);
   const day = date.getDate().toString().padStart(2, '0');
   const month = (date.getMonth() + 1).toString().padStart(2, '0');
   return `${day}/${month}`;
};

/**
 * Formats only the time part (HH:MM format)
 * 
 * @param dateTime - ISO date string or Date object
 * @returns Time string in "HH:MM" format
 * 
 * @example
 * formatMatchTime("2025-09-07T21:30:00")
 * // Returns: "21:30"
 */
export const formatMatchTime = (dateTime: string | Date): string => {
   const date = new Date(dateTime);
   const hours = date.getHours().toString().padStart(2, '0');
   const minutes = date.getMinutes().toString().padStart(2, '0');
   return `${hours}:${minutes}`;
}; 