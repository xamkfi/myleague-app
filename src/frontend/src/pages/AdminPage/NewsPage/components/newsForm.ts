import { SportsCategory } from '../../../../types/common/sports';
import type { SportKind } from '../../../../utils/sportRoutes';

/** Editable article fields in the news editor. The content HTML is kept separately. */
export interface NewsFormData {
  title: string;
  mainPicture: string;
  summary: string;
  author: string;
  category: string;
  sportCategory: string;
  teamCategory: string;
  tags: string[];
}

export type NewsFormErrors = Partial<Record<keyof NewsFormData, string>>;

export const TITLE_MIN_LENGTH = 5;
export const TITLE_MAX_LENGTH = 200;
export const SUMMARY_MAX_LENGTH = 200;
export const AUTHOR_MAX_LENGTH = 50;
export const MAX_IMAGE_BYTES: number = 5 * 1024 * 1024;

export const emptyNewsForm = (): NewsFormData => ({
  title: '',
  mainPicture: '',
  summary: '',
  author: '',
  category: '',
  sportCategory: '',
  teamCategory: '',
  tags: [],
});

const SPORT_CATEGORY_BY_KIND: Record<SportKind, SportsCategory> = {
  floorball: SportsCategory.Floorball,
  football: SportsCategory.Football,
  hockey: SportsCategory.Icehockey,
};

/** The sport of an article's `sportCategory`, or undefined for none. */
export function sportKindForCategory(sportCategory: string): SportKind | undefined {
  const entry = (Object.entries(SPORT_CATEGORY_BY_KIND) as [SportKind, SportsCategory][]).find(
    ([, category]) => category === sportCategory,
  );
  return entry?.[0];
}

/** The `sportCategory` value stored for a sport. */
export function sportCategoryForKind(kind: SportKind): SportsCategory {
  return SPORT_CATEGORY_BY_KIND[kind];
}
