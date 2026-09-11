/* eslint-disable react-refresh/only-export-components */
import { SportsCategory } from '../../../../types/common/sports';

export const NewsCategory = {
  None: 'None',
  General: 'General',
  MatchReports: 'MatchReports',
  LeagueNews: 'LeagueNews',
  PlayerUpdates: 'PlayerUpdates',
  TeamNews: 'TeamNews',
  Announcements: 'Announcements',
  Events: 'Events',
  Transfers: 'Transfers',
  Injuries: 'Injuries',
  Awards: 'Awards',
} as const;

export type NewsCategoryValue = (typeof NewsCategory)[keyof typeof NewsCategory];

export const NEWS_CATEGORY_OPTIONS: Exclude<NewsCategoryValue, 'None'>[] = Object.values(NewsCategory).filter(
  (value): value is Exclude<NewsCategoryValue, 'None'> => value !== NewsCategory.None
);

export { SportsCategory };

export const NEWS_SPORT_CATEGORY_OPTIONS: SportsCategory[] = Object.values(SportsCategory).filter(
  (value): value is SportsCategory => value !== SportsCategory.None
);
