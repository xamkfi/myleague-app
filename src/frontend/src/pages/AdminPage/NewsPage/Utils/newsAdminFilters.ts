export type NewsFilters = {
  category: string;
  sportCategory: string;
  searchTerm: string;
  includeArchived: boolean;
  teamCategories: string[];
};

export const EMPTY_NEWS_FILTERS: NewsFilters = {
  category: '',
  sportCategory: '',
  searchTerm: '',
  includeArchived: true,
  teamCategories: [],
};

/** True when any filter differs from the defaults, so a "clear filters" action is useful. */
export function hasActiveNewsFilters(filters: NewsFilters): boolean {
  return (
    filters.category !== EMPTY_NEWS_FILTERS.category ||
    filters.sportCategory !== EMPTY_NEWS_FILTERS.sportCategory ||
    filters.searchTerm.trim() !== '' ||
    filters.includeArchived !== EMPTY_NEWS_FILTERS.includeArchived ||
    filters.teamCategories.length > 0
  );
}
