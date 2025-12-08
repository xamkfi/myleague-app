export enum SportsCategory {
  None = 'None',
  Floorball = 'Floorball',
  Icehockey = 'Icehockey',
  Football = 'Football',
}

export const ACTIVE_SPORTS: SportsCategory[] = [
  SportsCategory.Floorball,
  SportsCategory.Icehockey,
  SportsCategory.Football,
];

export type SportType = (typeof ACTIVE_SPORTS)[number];

export const SPORT_LABELS: Record<SportsCategory, string> = {
  [SportsCategory.None]: 'None',
  [SportsCategory.Floorball]: 'Floorball',
  [SportsCategory.Icehockey]: 'Icehockey',
  [SportsCategory.Football]: 'Football',
};

export const isSportType = (value: string): value is SportsCategory => {
  return Object.values(SportsCategory).includes(value as SportsCategory);
};

