// Centralized list of supported sports for selection UIs
// Extend this list when adding new sports across the app
export const SPORTS = [
  'Floorball',
  'Icehockey',
  'Football',
] as const;

export type SportType = typeof SPORTS[number];


